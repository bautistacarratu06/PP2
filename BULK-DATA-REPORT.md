# Bulk Data Report — CSV Student Import & Large-Volume Handling

**Project:** GestionEscolar
**Date:** 2026-11-11
**Scope:** Why importing ~1000 students from CSV fails or degrades, why the app is not prepared for large datasets, and proposed solutions.

---

## 1. Executive summary

The student CSV import pipeline works for small files (a typical commission of 20–50 students) but breaks down around hundreds-to-thousands of rows for four compounding reasons:

1. **O(N×M) conflict detection** — every CSV row is compared against *every* student in the database with linear scans.
2. **Full-table loads** — the entire `student` table is materialized in memory (twice per import), and every UI query loads *all* students of a section with no pagination.
3. **One giant EF Core `SaveChangesAsync`** — 1000 tracked entities in a single `DbContext` and a single implicit transaction; one bad row rolls back everything and memory/latency spike.
4. **No bulk-load path** — despite targeting SQL Server, nothing uses `SqlBulkCopy`, table-valued parameters, or chunked batches.

There is also **no pagination anywhere in the codebase** (no `Skip`/`Take`/page-size concept exists in `GestionEscolar.Core`), so displaying 1000 students means materializing and rendering all of them at once.

---

## 2. Findings (with file references)

### 2.1 Conflict detection is quadratic — `CommissionImportService.BuildPreview`

`GestionEscolar.Core/Services/CommissionImportService.cs` (lines 139–186):

```csharp
foreach (var row in rows)
{
    var byDni = students.FirstOrDefault(student => student.Dni == row.Dni);
    var byNumber = ... students.FirstOrDefault(...);
```

- `students` is the **entire student table** loaded into memory (`PreviewAsync` → `_repository.GetStudentsAsync()`).
- For each of the N CSV rows, two linear scans over M students run → **O(N×M)**.
- With 1,000 CSV rows and 5,000 students already in the DB, that is ~10 million string comparisons per scan pass — noticeable; at 10,000+ students it becomes seconds-to-minutes of pure CPU on the UI thread's call path.

### 2.2 The whole student table is loaded into memory — `EfCommissionImportRepository.GetStudentsAsync`

`GestionEscolar.Data/Repositories/EfCommissionImportRepository.cs` (lines 25–58):

```csharp
var rows = await db.Students.AsNoTracking()...ToListAsync(...)
```

- No `WHERE`, no projection limit, no streaming: every student of **every** section is pulled into memory.
- Worse, it is called **twice** per import: `CreateAsync` → `PreviewAsync` (call 1), and `CreateAsync` → `PreviewAsync` again internally (call 2). `ApplyAsync` path loads it once more via `PreviewAsync` before applying.

### 2.3 Single monolithic save — `EfCommissionImportRepository.ApplyAsync` / `CreateSectionAsync`

`GestionEscolar.Data/Repositories/EfCommissionImportRepository.cs` (lines 60–133):

```csharp
foreach (var insert in inserts)
    db.Students.Add(new StudentEntity { ... });

await db.SaveChangesAsync(cancellationToken);
```

- All inserts are tracked by one `DbContext` and flushed in **one** `SaveChangesAsync`.
- EF Core generates one INSERT statement per row (grouped into batched commands, but still bounded by command size/parameter limits). At ~1000 rows this becomes a very long single command/transaction.
- **All-or-nothing:** a single duplicate DNI or constraint violation on row 987 aborts the whole import with a generic `DbUpdateException`, discarding 986 valid rows. There is no per-row error reporting.
- The update path has the same shape: `ids.Contains(...)` builds a large `IN (...)` clause, then mutates tracked entities one by one.

### 2.4 The per-student path is N+1 — `StudentService.AddAsync`

`GestionEscolar.Core/Services/StudentService.cs` (lines 33–48): each student enrollment issues **2 SELECTs + 1 INSERT** (find by student number, find by DNI, insert). Any code that loops over CSV rows calling `AddAsync` performs 3 × N round trips — 3,000 database round trips for 1,000 students. This is the "one student at a time" API; there is no batch equivalent in `IStudentRepository`.

### 2.5 No pagination anywhere

- `StudentsQueryService.QueryAsync` (`GestionEscolar.Core/Services/StudentsQueryService.cs`) loads **all** rows of a section (`GetRowsBySectionAsync`), evaluates risk, filters, sorts, and returns everything in `StudentsQueryResult`. There is no `Page`/`PageSize` in `StudentsQueryFilter`.
- `EfTrackingRepository.GetRowsBySectionAsync` (`GestionEscolar.Data/Repositories/EfTrackingRepository.cs`) builds one query with **four correlated subqueries per student** (attendance count, session count, assignment count, submission count, plus a `Max()` for last contact). With 1000 students this is a heavy query that materializes all rows at once, and the service then re-filters/re-sorts in memory.
- `GetRosterAsync` in `EfStudentRepository` likewise returns the full roster with no paging.

### 2.6 CSV parsing limitations — `CsvParser`

`GestionEscolar.Core/Services/CsvParser.cs`:

- `ParseLine` works line-by-line; **quoted fields containing line breaks** (multi-line addresses, notes) are not supported — the parser will treat continuation lines as separate records and misalign columns.
- `StudentCsvImporter.Import` (the other importer) accumulates all rows in a `List` with no streaming, no row-count cap, and no duplicate/conflict detection at all.

### 2.7 No bulk-write primitives

The data layer targets SQL Server (`UseSqlServer`, `Microsoft.Data.SqlClient`, `DatabaseInstance` even exposes raw `SqlConnection`s), but:

- No `SqlBulkCopy` usage anywhere.
- No table-valued parameters (TVPs).
- No `SqlBulkCopy`-friendly staging table.
- EF Core is used for everything, including bulk loads it was never designed for.

---

## 3. Why "1000 rows breaks it" — failure chain

| # | Stage | Behavior at 1,000 rows | Symptom |
|---|-------|------------------------|---------|
| 1 | CSV read | Whole file parsed into memory | OK (small) |
| 2 | Preview | Loads **entire** student table ×2, then O(N×M) scans | UI freeze / multi-second stall |
| 3 | Save | 1000 entities in one `DbContext`, one `SaveChangesAsync`, one transaction | Long command, possible timeout, large memory spike |
| 4 | Error handling | Any single conflict aborts everything | "It just breaks" — no partial import, no row-level errors |
| 5 | Display | Full section query with 4 correlated subqueries, no paging | Slow screens, janky UI with large rosters |

---

## 4. Proposed solutions

Ordered from quick wins to structural fixes. Items 4.1–4.3 alone will make a 1,000-row import work reliably; 4.4–4.6 make it scale further and improve the UI.

### 4.1 Fix the O(N×M) preview — index the loaded students (quick win, ~1 hour)

In `BuildPreview`, replace `FirstOrDefault` scans with dictionaries built once:

```csharp
var byDniMap = students
    .GroupBy(s => s.Dni)
    .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

var byNumberMap = sectionId is int section
    ? students
        .Where(s => s.SectionId == section)
        .GroupBy(s => s.StudentNumber)
        .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal)
    : new Dictionary<string, Student>();
```

This turns the preview from O(N×M) into O(N+M) — effectively instant even at 100k rows. **No interface changes needed.**

### 4.2 Chunk the bulk insert (quick win, ~2 hours)

In `EfCommissionImportRepository.ApplyAsync` and `CreateSectionAsync`, stop adding everything to one context. Save in batches:

```csharp
const int BatchSize = 200;
foreach (var batch in inserts.Chunk(BatchSize))
{
    foreach (var insert in batch)
        db.Students.Add(new StudentEntity { /* ... */ });
    await db.SaveChangesAsync(cancellationToken);
}
```

Wrap the whole loop in an explicit transaction (`BeginTransactionAsync` — the pattern already exists in `DeactivateAsync`) so it stays atomic, but each `SaveChangesAsync` now emits a bounded command. This alone removes the timeout/memory failure at 1000 rows.

### 4.3 Return a per-row result instead of throwing on first conflict

Change `ApplyAsync` (or add `ApplyWithReportAsync`) to return an `ImportResult`:

```csharp
public sealed record ImportResult(int Inserted, int Updated, IReadOnlyList<RowError> Errors);
public sealed record RowError(int LineNumber, string Message);
```

Catch `DbUpdateException` per batch, map the failing row back via its index, and continue. The UI can then show "988 imported, 12 failed: [line, reason]" instead of an all-or-nothing exception. This is the single biggest UX improvement for bulk imports.

### 4.4 Use SQL Server bulk copy for large imports (structural, ~1 day)

For inserts beyond a few hundred rows, bypass EF Core entirely. The project already has `DatabaseInstance` exposing raw `SqlConnection`s:

- **Option A — `SqlBulkCopy` into a staging table**, then a set-based `INSERT ... SELECT` / `MERGE` from staging into `dbo.student`. This is the canonical SQL Server bulk-upsert pattern and handles 100k+ rows in seconds.
- **Option B — Table-Valued Parameter**: create a `dbo.StudentTableType` and a stored procedure `dbo.usp_bulk_enroll_students` that does the insert/merge set-based. Simpler permissions model than bulk copy, works well up to tens of thousands of rows.

Add a method to `ICommissionImportRepository` (e.g. `BulkInsertAsync`) so the service layer stays storage-agnostic; the EF path remains for small imports (< 200 rows).

### 4.5 Push conflict detection into SQL (optional, pairs with 4.4)

Instead of loading all students to compare in C#, do the conflict check set-based:

```sql
SELECT csv.dni, csv.student_number
FROM   #staging csv
WHERE  EXISTS (SELECT 1 FROM dbo.student s WHERE s.dni = csv.dni)
    OR EXISTS (SELECT 1 FROM dbo.student s
               WHERE s.section_id = @sectionId AND s.student_number = csv.student_number);
```

One round trip, index-backed, scales to any size. The preview/conflict UI can then be fed from this query.

### 4.6 Add pagination to the students query

Add paging to the existing filter model — minimal, non-breaking change:

```csharp
// StudentsQueryFilter
public int? Page { get; init; }        // 1-based
public int? PageSize { get; init; }    // e.g. 50
```

In `StudentsQueryService.QueryAsync`, after ordering:

```csharp
if (filter.PageSize is int size)
{
    var page = Math.Max(1, filter.Page ?? 1);
    ordered = ordered.Skip((page - 1) * size).Take(size).ToList();
}
```

Longer term, push filtering/sorting/paging down to SQL (either translate the filters into the EF query in `EfTrackingRepository`, or extend `vw_student_risk` usage) so the app stops materializing whole sections. Also consider `AsSplitQuery()` for the tracking query to avoid the correlated-subquery cost, and let the DB do the sorting instead of `ApplySort` in memory.

### 4.7 Harden the CSV layer

- Support **multi-line quoted fields**: read with a stateful parser (accumulate lines while inside quotes) instead of one `ReadLine` per record.
- Optionally cap import size (e.g. warn above 10,000 rows) and report progress, since parsing is currently synchronous on the caller's thread.
- `StudentCsvImporter` should reuse the validation/limit logic from `CommissionImportService.Read` (length limits, required fields) — today it accepts anything.

---

## 5. Recommended implementation order

| Priority | Change | Effort | Impact |
|----------|--------|--------|--------|
| 1 | Dictionary-based conflict lookup (4.1) | ~1 h | Removes the CPU blow-up |
| 2 | Chunked saves in a transaction (4.2) | ~2 h | Fixes timeouts/memory at 1000+ rows |
| 3 | Per-row import report (4.3) | ~2–3 h | Partial imports + actionable errors |
| 4 | Paging in `StudentsQueryFilter` + service (4.6) | ~2–3 h | UI usable with large sections |
| 5 | `SqlBulkCopy`/TVP bulk path (4.4, 4.5) | ~1 day | Scales to tens of thousands of rows |
| 6 | Multi-line CSV support + importer validation (4.7) | ~2 h | Correctness for real-world files |

Priorities 1–3 make a 1,000-row import reliable with small, low-risk changes confined to `CommissionImportService` and `EfCommissionImportRepository`. Priorities 4–6 are the "prepared for a lot of data" work.
