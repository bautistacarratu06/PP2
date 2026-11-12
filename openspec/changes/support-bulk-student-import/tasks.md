# Tasks

## 1. Scalable conflict detection

- [x] 1.1 In `CommissionImportService.BuildPreview` (or `ImportConflictDetector` if the `remediate-code-quality` split has landed), build DNI and (section, student number) dictionaries once and replace the `FirstOrDefault` scans with O(1) lookups; keep the existing `HashSet` in-file duplicate handling and identical conflict kinds/explanations. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter CommissionImportServiceTests` passes.
- [x] 1.2 Add a test that previews 1,000 rows against a populated student list and asserts the preview result (inserts + conflicts) matches the previous linear implementation's output for the same input. Verify the test passes.

## 2. Chunked bulk persistence

- [x] 2.1 In `EfCommissionImportRepository.ApplyAsync` and `CreateSectionAsync`, save inserts in batches of ~200 rows inside an explicit `BeginTransactionAsync`/commit/rollback, reusing the pattern from `EfStudentRepository.DeactivateAsync`; keep all-or-nothing semantics. Verify `dotnet build GestionEscolar.Data/GestionEscolar.Data.csproj` succeeds and `CommissionImportServiceTests` passes.
- [x] 2.2 Add a Data-layer test that inserts 1,000 rows through `ApplyAsync` and asserts all rows are stored, plus a test that a constraint violation mid-save commits nothing. Verify both tests pass.

## 3. Per-row import report

- [x] 3.1 Add `ImportResult` (inserted, updated, errors) and `RowError` (line number, message) models in `GestionEscolar.Core/Models/`, and an `ApplyWithReportAsync` method to `ICommissionImportRepository` that returns the report; keep `ApplyAsync` as a working forward. Verify `dotnet build GestionEscolar.Core/GestionEscolar.Core.csproj` and `dotnet build GestionEscolar.Data/GestionEscolar.Data.csproj` succeed.
- [x] 3.2 Implement `ApplyWithReportAsync` with batch-level error mapping: rows in a failed batch are recorded as `RowError`s with their CSV line numbers and the constraint reason, and the remaining rows still commit. Verify with a test asserting the 988-inserted/12-errors mixed scenario from the spec.
- [x] 3.3 Add tests asserting the report is correct when all rows are valid (all inserted, empty error list) and when every row conflicts (nothing inserted, every line reported). Verify the tests pass.

## 4. Staging-table bulk path

- [x] 4.1 Add migration `0011_create_student_import_staging` creating `dbo.student_import_staging` (with a per-import guid column) and drop the migration into `GestionEscolar.Data/Database/Migrations/`. Verify `dotnet ef database update` (or `dotnet build GestionEscolar.Data/GestionEscolar.Data.csproj`) succeeds.
- [x] 4.2 Add `BulkApplyAsync` to `ICommissionImportRepository` and implement it in `EfCommissionImportRepository` using `DatabaseInstance`'s `SqlConnection`: `SqlBulkCopy` into staging, set-based conflict selection, set-based insert of non-conflicting rows, staging rows for that import id removed on completion. Verify `dotnet build GestionEscolar.Data/GestionEscolar.Data.csproj` succeeds.
- [x] 4.3 Route confirmed imports above the threshold constant (200 rows) to `BulkApplyAsync` in `CommissionImportService`, keeping the EF path below it, and return the same `ImportResult` shape from both paths. Verify `CommissionImportServiceTests` passes.
- [x] 4.4 Add a test asserting the bulk path and the EF path produce identical stored rosters for the same CSV input, and a test that a large import through the bulk path reports conflicts per line. Verify both tests pass.

## 5. Paginated student display

- [x] 5.1 Add `int? Page` and `int? PageSize` to `StudentsQueryFilter` and apply `Skip/Take` after ordering in `StudentsQueryService.QueryAsync`, computing the risk summary from the full evaluated set before paging. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter StudentsQueryServiceTests` passes.
- [x] 5.2 Add tests: page 2 with size 50 over 1,000 students returns rows 51–100 in sort order with a whole-roster summary; no page size returns all rows as before. Verify the tests pass.

## 6. Multi-line CSV and importer validation

- [x] 6.1 Add a shared record-assembly helper that accumulates physical lines while a quoted field is open and feeds complete records to `CsvParser.ParseLine`; use it from both `CommissionImportService.Read` and `StudentCsvImporter.Import`. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter "CommissionImportServiceTests|StudentCsvImporterTests"` passes.
- [x] 6.2 Add a test with a quoted field containing a line break asserting the field value preserves the break and the next physical line is not a new record. Verify the test passes.
- [x] 6.3 Apply the same required-field and max-length validation from `CommissionImportService.Read` in `StudentCsvImporter.Import`, reporting the first invalid row with its line number. Verify with a test asserting an oversized DNI is rejected with the line number, matching the roster importer's message shape.

## 7. Integration

- [x] 7.1 Run `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj` and `dotnet build GestionEscolar.Data.Tests/GestionEscolar.Data.Tests.csproj`; verify both succeed.
- [ ] 7.2 Generate a 1,000-row CSV and run an import end-to-end against the local database; verify the import completes, the report shows the expected inserted count and per-line errors, and the students screen displays the section in pages without stalling.
