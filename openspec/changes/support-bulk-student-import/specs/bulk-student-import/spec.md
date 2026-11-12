# Spec Delta

## Purpose

Enable the system to import student rosters from CSV at large scale (1,000+ rows) reliably: conflict detection that does not degrade with size, persistence that completes without timeouts and reports per-row failures instead of aborting everything, and roster display that pages large sections.

## ADDED Requirements

### Requirement: Scalable conflict detection

The system SHALL detect DNI and student-number conflicts between the CSV file and stored students without performance degrading as either the file or the stored roster grows. Conflict detection SHALL report the same conflict kinds as before (DNI clash, student-number clash, in-file duplicates, database conflicts) with the same explanations.

#### Scenario: Import 1,000 rows against a populated database

- **WHEN** a CSV with 1,000 rows is previewed against a database that already contains students
- **THEN** the preview completes without a perceptible stall, and every conflict is identified with the same criterion and explanation as before

#### Scenario: Duplicate rows within the file

- **WHEN** the CSV contains two rows with the same DNI or the same student number
- **THEN** both rows are reported as conflicts with origin "file", and neither is silently inserted

### Requirement: Chunked bulk persistence

Bulk imports SHALL be saved in bounded batches within a single transaction, rather than tracking every row in one save. A failure during saving does not leave the database partially updated: either the whole confirmed import commits or none of it does.

#### Scenario: Import completes at 1,000 rows

- **WHEN** a confirmed import contains 1,000 rows to insert
- **THEN** the save completes without a command-size or timeout failure, and all 1,000 students are stored

#### Scenario: Failure mid-import rolls back

- **WHEN** a constraint violation occurs while saving a batch
- **THEN** no rows from that import are committed

### Requirement: Per-row import report

Confirming an import SHALL return a report of what happened per row instead of failing on the first problem. The report states how many rows were inserted, how many were merged/updated, and lists each rejected row with its line number and reason. Rows that can be imported are imported even when other rows fail.

#### Scenario: Mixed valid and invalid rows

- **WHEN** a confirmed import contains 988 valid rows and 12 rows with duplicate DNIs
- **THEN** the report shows 988 inserted and 12 errors, each error naming its CSV line number and the reason, and the 988 valid students exist in the database

#### Scenario: All rows valid

- **WHEN** a confirmed import has no conflicts
- **THEN** the report shows all rows inserted and an empty error list

### Requirement: Bulk path for large imports

Imports beyond a small threshold SHALL use a SQL Server bulk mechanism (staging plus set-based insert/merge) instead of row-by-row inserts. The bulk path detects conflicts set-based in the database rather than loading all students into application memory. Small imports continue to work through the existing path with identical results.

#### Scenario: Large import uses the bulk path

- **WHEN** a confirmed import exceeds the small-import threshold
- **THEN** the rows are persisted via a single bulk operation and the stored roster matches the file, with conflicts reported per line

#### Scenario: Small import unchanged

- **WHEN** a confirmed import is below the threshold
- **THEN** the import succeeds with the same results and messages as before this change

### Requirement: Paginated student display

The students query SHALL accept an optional page number and page size. When a page size is set, the result contains only that page of ordered, filtered rows, and the risk summary still covers all active students of the section regardless of the page requested.

#### Scenario: Requesting a page

- **WHEN** a section has 1,000 students and the query asks for page 2 with a page size of 50
- **THEN** the result contains students 51–100 in the established sort order, and the summary counts reflect the whole active roster

#### Scenario: No paging requested

- **WHEN** the query sets no page size
- **THEN** the result contains all matching rows, as before this change

### Requirement: Multi-line CSV fields

CSV parsing SHALL support quoted fields that contain line breaks. A record whose quoted field spans several physical lines is parsed as one row with the embedded breaks preserved in the field value.

#### Scenario: Quoted field with embedded line break

- **WHEN** a CSV row contains a quoted field with a line break inside it
- **THEN** the parser returns that field as a single value containing the break, and the following physical line is not treated as a new record

### Requirement: Consistent CSV validation across importers

Every CSV student importer SHALL enforce the same required fields and maximum field lengths, reporting the first invalid row with its line number.

#### Scenario: Oversized field in the secondary importer

- **WHEN** a CSV imported through the tracking importer has a DNI longer than the allowed maximum
- **THEN** the import is rejected with a message naming the line number, matching the roster importer's behavior
