# Design

## Context

See `proposal.md` for motivation and `specs/bulk-student-import/spec.md` for the behavior contract. Detailed findings with line references are in `bulk-data-report.md` at the repo root.

Current state that shapes the approach:

- `CommissionImportService.BuildPreview` compares each CSV row against the full student list with `FirstOrDefault` scans; `PreviewAsync` loads every student of every section via `EfCommissionImportRepository.GetStudentsAsync`, and `CreateAsync` triggers that load twice.
- `EfCommissionImportRepository.ApplyAsync` and `CreateSectionAsync` add every entity to one `DbContext` and flush with a single `SaveChangesAsync` (one implicit transaction). `DeactivateAsync` in `EfStudentRepository` already shows the explicit-transaction pattern to reuse.
- `ICommissionImportRepository` is the only seam between the import service and persistence; `DatabaseInstance` already exposes raw `SqlConnection`s, and the project targets SQL Server (`UseSqlServer`, `Microsoft.Data.SqlClient`).
- `StudentsQueryFilter` has no paging fields; `StudentsQueryService.QueryAsync` loads, filters, sorts, and summarizes everything in memory. The risk summary must keep covering the whole active roster even when a page is requested.
- `CsvParser.ParseLine` is a pure line-to-fields function; both importers (`CommissionImportService.Read`, `StudentCsvImporter.Import`) drive it with `ReadLine`, so quoted multi-line fields cannot work today.
- The `remediate-code-quality` change (in progress) splits `CommissionImportService` into `RosterCsvReader` and `ImportConflictDetector`. This change must not conflict with that split; the dictionary lookup belongs in the conflict detector wherever it ends up living.

## Goals / Non-Goals

**Goals:**

- Make a 1,000-row import complete reliably: no timeout, no memory spike, no all-or-nothing failure.
- Keep the confirmed import atomic (all-or-nothing) while reporting per-row errors for rows that could not be imported.
- Keep small imports on the existing EF path so behavior and messages stay identical below the threshold.
- Add optional paging to the students query without changing any existing call site's results.
- Keep the service layer storage-agnostic: bulk mechanics stay behind the repository interface.

**Non-Goals:**

- Background/async import jobs, progress reporting UI, or import cancellation.
- Changing the conflict rules (what counts as a conflict) or the merge/omit decision flow.
- Replacing EF Core for normal operations; the bulk path is only for large confirmed imports.
- Virtualized/infinite-scroll UI; paging at the service level is enough for this change.
- Touching `EfTrackingRepository`'s correlated-subquery shape (noted in the report as future work).

## Decisions

### 1. Dictionary-based lookup inside the conflict detector

`BuildPreview` builds two dictionaries once per preview — DNI → student, and (section, student number) → student — then does O(1) lookups per row. In-file duplicates keep the existing `HashSet` approach. This lives wherever conflict detection lives after the `remediate-code-quality` split (`ImportConflictDetector`), so the two changes compose.

*Alternative considered:* pushing conflict detection into SQL now. Rejected for the small-import path because it changes the preview flow and the in-file duplicate handling; it arrives with the bulk path instead (decision 3).

### 2. Chunked saves inside one explicit transaction

`ApplyAsync` and `CreateSectionAsync` save in batches of ~200 rows, wrapped in `BeginTransactionAsync`/`Commit`/`Rollback` (the pattern `DeactivateAsync` already uses). Batching bounds command size and parameter count; the transaction preserves all-or-nothing semantics required by the spec.

*Alternative:* `SqlBulkCopy` for everything. Rejected as the default because it changes the small-import path that tests and users rely on; the bulk path is additive (decision 3).

### 3. Staging-table bulk path above a threshold

Add `BulkApplyAsync` to `ICommissionImportRepository`. Implementation: `SqlBulkCopy` rows into a `dbo.student_import_staging` table (new migration), then a set-based script that (a) selects conflicts back per staging row for the report, (b) inserts the non-conflicting rows into `dbo.student`. The threshold is a constant (e.g. 200 rows) chosen so the EF path still handles typical commissions. `DatabaseInstance` supplies the `SqlConnection`; the EF path stays for small imports.

*Alternative:* Table-valued parameters with a stored procedure. Functionally equivalent at this scale; TVP adds a user-defined type and proc maintenance for no gain here. `SqlBulkCopy` + staging is the canonical SQL Server pattern and reuses the raw-connection seam that already exists.

### 4. Report-returning apply variant

Add `ApplyWithReportAsync` (returns `ImportResult` with inserted/updated counts and `RowError(LineNumber, Message)` list) rather than changing `ApplyAsync`'s signature. Batch-level `DbUpdateException` handling maps the failing batch back to row indexes; rows in failed batches are recorded as errors and the transaction is retried excluding them, so valid rows still commit. The wizard's confirm step consumes the report and shows per-line errors.

*Alternative:* changing `ApplyAsync` to return the report. That breaks `GestionEscolar.Tests` call sites for no benefit; the old method stays as a thin forward.

### 5. Paging as optional fields on the existing filter

`StudentsQueryFilter` gains `int? Page` (1-based) and `int? PageSize`. `StudentsQueryService` applies `Skip/Take` after ordering, and computes the risk summary from the full evaluated set before paging, so summary semantics are unchanged. No paging → identical behavior to today, so existing tests pass untouched.

*Alternative:* pushing filters/sort/paging into SQL now. Deferred: it requires translating the whole filter model into the EF query or extending `vw_student_risk`; valuable but a larger change than this spec requires.

### 6. Stateful multi-line CSV reader

Wrap `CsvParser` with a record-assembly loop: accumulate physical lines while quote state is open, then hand the joined record to `ParseLine`. Both importers get this via a shared `ReadRecords(TextReader)` helper. `ParseLine` itself keeps its signature and results for single-line records.

*Alternative:* replacing the parser with a library (CsvHelper). Rejected: the project deliberately owns its parser and the fix is small.

## Risks / Trade-offs

- [Batch retry after a failed batch can mask the original error] → Map the failing row via the batch's index range and surface its line number and constraint message in the report; integration test covers the mixed valid/invalid scenario.
- [Staging table adds a schema object to manage] → One migration, dropped/recreated is safe; staging rows are deleted at the end of each bulk operation.
- [Two persistence paths can drift] → The EF path stays authoritative for results and messages; a test asserts bulk and EF paths produce identical rosters for the same input.
- [Paging in memory still loads the section] → Acceptable now (spec only requires paged results); pushing paging to SQL is noted as future work in the report.
- [Concurrent imports share the staging table] → Staging gains a per-import guid column; the merge only touches rows matching the import's id, and rows are removed on completion.

## Migration Plan

1. Add the staging-table migration (additive; no changes to existing tables).
2. Ship dictionary lookup + chunked saves first — behavior-preserving, independently verifiable with existing tests.
3. Add the report-returning apply and wire the wizard to display it.
4. Add the bulk path behind the threshold and the paging fields.
5. Rollback: each step is independently revertible; the staging table can be dropped without affecting the app.

## Open Questions

None. The threshold value (200) and batch size are tunable constants that do not change the specs or task breakdown.
