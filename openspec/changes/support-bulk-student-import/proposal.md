# Proposal

## Why

Importing a commission from CSV works for small rosters but fails or stalls at around 1,000 rows: conflict detection compares every CSV row against every stored student (O(N×M)), the whole `student` table is loaded into memory twice per import, and all inserts go through one `SaveChangesAsync` in a single transaction, so one bad row aborts the entire import with no row-level feedback. The findings are in `bulk-data-report.md`. The students screen also has no pagination, so large sections are slow to display.

## What Changes

- Replace the linear `FirstOrDefault` scans in `CommissionImportService.BuildPreview` with dictionary lookups (by DNI and by section + student number), turning conflict detection from O(N×M) into O(N+M).
- Chunk the bulk insert in `EfCommissionImportRepository.ApplyAsync` and `CreateSectionAsync` into bounded batches (e.g. 200 rows) inside an explicit transaction, instead of one `SaveChangesAsync` with every entity tracked.
- Return a per-row import report (`ImportResult` with inserted/updated counts and a list of row errors with line numbers) instead of throwing on the first conflict, so valid rows are imported and failures are reported per line.
- Add a SQL Server bulk path for large imports: `SqlBulkCopy` into a staging table followed by a set-based insert/merge, exposed as a new method on `ICommissionImportRepository` (e.g. `BulkApplyAsync`); the EF path remains for small imports.
- Move conflict detection into SQL for the bulk path (set-based `EXISTS` checks against the staging table) so the preview no longer loads the whole student table.
- Add `Page` and `PageSize` to `StudentsQueryFilter` and apply them in `StudentsQueryService.QueryAsync` after ordering, so large sections display in pages.
- Support multi-line quoted fields in `CsvParser` (stateful line accumulation while inside quotes) so real-world CSVs with embedded line breaks parse correctly.
- Apply the same length/required-field validation from `CommissionImportService.Read` in `StudentCsvImporter.Import`.

## Capabilities

### New Capabilities

- `bulk-student-import`: Importing students in bulk from CSV — conflict detection that scales, chunked/bulk persistence with per-row error reporting, and paginated display of large rosters.

### Modified Capabilities

None. `openspec list --specs` returns no capabilities; this change creates the first one.

## Impact

- `GestionEscolar.Core/Services/CommissionImportService.cs` (`BuildPreview`, `PreviewAsync`, `ApplyAsync`), `GestionEscolar.Core/Services/CsvParser.cs`, `GestionEscolar.Core/Services/StudentCsvImporter.cs`, `GestionEscolar.Core/Models/StudentsQueryFilter.cs`, `GestionEscolar.Core/Services/StudentsQueryService.cs`.
- `GestionEscolar.Data/Repositories/EfCommissionImportRepository.cs` (chunked saves, bulk path), `GestionEscolar.Core/Interfaces/ICommissionImportRepository.cs` (new bulk method), `GestionEscolar.Data/DatabaseInstance.cs` (raw `SqlConnection` for `SqlBulkCopy`).
- New staging table migration in `GestionEscolar.Data/Database/Migrations/` and a `StudentTableType`/stored procedure if the TVP option is chosen.
- `GestionEscolar.Tests` (CommissionImportServiceTests, StudentsQueryServiceTests) and `GestionEscolar.Data.Tests` need new cases for chunked saves, per-row errors, paging, and multi-line CSV.
- No breaking changes to existing public method signatures; `ApplyAsync` gains a report-returning variant rather than changing shape.
