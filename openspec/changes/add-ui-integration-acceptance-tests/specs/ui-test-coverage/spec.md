# Spec Delta

## Purpose

Make the UI layer verifiable by automated tests: ViewModels run outside a MAUI app through dispatch and navigation seams, acceptance tests drive them against repository fakes, and integration tests exercise Core services end-to-end through real repositories on SQL Server.

## ADDED Requirements

### Requirement: ViewModels are testable outside the MAUI runtime

ViewModels SHALL NOT call `MainThread` or `Shell.Current` directly. UI-thread dispatch and page navigation SHALL go through injected seams (`IMainThreadDispatcher`, `INavigator`) whose production implementations are registered in the composition root. ViewModel behavior visible to the user SHALL NOT change.

#### Scenario: Students list loads without a MAUI runtime

- **WHEN** `StudentsViewModel` is constructed and loaded in a plain xUnit test with fakes
- **THEN** rows, KPI counts, and status/error state populate without any MAUI platform dependency

#### Scenario: Detail back navigation goes through the seam

- **WHEN** `StudentDetailViewModel`'s back command executes in a test
- **THEN** the injected navigator is called and no `Shell.Current` is touched

### Requirement: Acceptance suite on fakes

An acceptance test suite SHALL exercise each screen ViewModel directly, using the existing repository fakes and real Core services. Acceptance tests SHALL NOT touch MAUI UI controls, a real database, or the network. Each feature's acceptance tests cover its user-visible flows and error/empty states.

#### Scenario: Settings thresholds round-trip

- **WHEN** the settings ViewModel loads, edits, and saves thresholds with valid and then invalid input
- **THEN** values persist through the fake repository and the status/error messages match the app's current messages

#### Scenario: Calendar add and validation

- **WHEN** a section is selected and a class or assignment item is added, including a duplicate assignment name or past date
- **THEN** the item list refreshes on success and the validation error surfaces in `ErrorMessage` without an exception

#### Scenario: Student detail contact registration

- **WHEN** a student is opened via query attributes and a contact is registered, valid and then invalid
- **THEN** the interventions history grows on success and `HasContactError` is set on invalid input, with empty states shown when histories are empty

#### Scenario: Students list filters, selection, and bulk actions

- **WHEN** rows are loaded and the user applies text/risk/contact filters, selects rows, toggles select-all, and runs a bulk attendance or submission mark
- **THEN** `FilteredStudents` narrows per filter, `SelectedCount`/`SelectedCountText` and `IsAllSelected` track selection, and the bulk action applies to exactly the selected rows

#### Scenario: Commission import wizard

- **WHEN** the import wizard advances through its steps with a temp CSV file, resolves conflicts with merge or omit decisions, and confirms
- **THEN** the preview matches the file's conflicts, the confirm applies the decisions, and the wizard closes

### Requirement: Integration suite on real repositories

An integration test suite SHALL exercise Core services end-to-end through the real EF Core repositories against a migrated SQL Server Express database, reusing the existing fixture pattern (drop → migrate → clean per test) and cleaning up after itself.

#### Scenario: Settings persist across re-read

- **WHEN** thresholds are saved through the service and re-read from the database
- **THEN** the stored values match the saved values

#### Scenario: Import flow against the database

- **WHEN** a CSV is parsed, previewed, and confirmed through the real repositories
- **THEN** the stored roster reflects the confirmed decisions and conflicts were detected as specified

### Requirement: Ranked feature coverage

Both suites SHALL cover, at minimum, the five features in the plan's ranked order: settings/risk thresholds, calendar, student detail, students list, and commission import — each with one integration and one acceptance test set.

#### Scenario: Feature coverage check

- **WHEN** the suites are complete
- **THEN** each of the five features has both an integration test set in `GestionEscolar.Data.Tests` and an acceptance test set in `GestionEscolar.UI.Tests`

### Requirement: Test data strategy

Integration tests SHALL seed realistic rosters via the existing seeder and fixture pattern and clean up per test. Acceptance tests SHALL seed fakes per test with 3–5 students across risk levels, one section, one class session and one assignment, and write CSV strings to temp files for import tests.

#### Scenario: Acceptance seed is self-contained

- **WHEN** an acceptance test runs
- **THEN** it creates its own students, section, session, and assignment on the fakes and leaves no shared mutable state for other tests

### Requirement: Suites run green in CI-style execution

Both suites SHALL pass via the existing test scripts or `dotnet test`, and coverage collection (coverlet) SHALL report the exercised ViewModels and services.

#### Scenario: Full run

- **WHEN** all test projects are run
- **THEN** every suite is green and the coverage report includes the five features' ViewModels and services
