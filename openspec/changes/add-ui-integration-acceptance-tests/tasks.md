# Tasks

## 1. Test project setup

- [x] 1.1 Create `GestionEscolar.UI.Tests` (xUnit, coverlet) multi-targeting `net10.0` and `net10.0-windows10.0.19041.0`, referencing `UI.csproj`, `GestionEscolar.Core`, and the fakes from `GestionEscolar.Tests`. Verify `dotnet build GestionEscolar.UI.Tests/GestionEscolar.UI.Tests.csproj` succeeds.
- [x] 1.2 Add one smoke acceptance test: construct `SettingsViewModel` with real `RiskSettingsService` over `FakeRiskSettingsRepository`, run `LoadCommand`, and assert thresholds populate. Verify `dotnet test GestionEscolar.UI.Tests/GestionEscolar.UI.Tests.csproj` passes.

## 2. Testability seams

- [ ] 2.1 Add `IMainThreadDispatcher` (production impl wrapping `MainThread.BeginInvokeOnMainThread`, inline test impl) and replace direct `MainThread` calls in `StudentsViewModel` and `SectionScopedViewModel`; register the production impl in `MauiProgram`. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds and app behavior is unchanged.
- [ ] 2.2 Add `INavigator` (or reuse `IAppNavigation` if `remediate-code-quality` has landed) wrapping `Shell.Current.GoToAsync`; replace the direct calls in `StudentsViewModel` (open detail) and `StudentDetailViewModel` (back); register the production impl in `MauiProgram`. Verify the build succeeds and no ViewModel references `Shell.Current` directly.
- [ ] 2.3 Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj` still passes with the new constructor parameters (existing tests keep compiling).

## 3. Settings (risk thresholds)

- [ ] 3.1 Integration: `EfRiskSettingsRepository` + `RiskSettingsService` round-trip — save thresholds, re-read, assert persisted values. Verify the test in `GestionEscolar.Data.Tests` passes.
- [ ] 3.2 Acceptance: `SettingsViewModel` load → edit → save sets `StatusMessage`/`StatusIsError`; invalid input shows the error message. Verify the test passes.

## 4. Calendar

- [ ] 4.1 Integration: `EfCalendarRepository` — add `ClassSession` and `Assignment`, `GetItemsAsync` returns the combined ordered list, duplicate assignment name and past date are rejected. Verify the test passes.
- [ ] 4.2 Acceptance: `CalendarViewModel` — section selection loads `Items`; `SelectedType` switch resets placeholder/name; `AddAsync` refreshes; empty state `HasNoItems`; validation errors surface in `ErrorMessage`. Verify the test passes.

## 5. Student detail

- [ ] 5.1 Integration: `EfStudentDetailsRepository` — header + three histories load, a registered contact appears in interventions, risk badge data is consistent with the seeded attendance/submissions. Verify the test passes.
- [x] 5.2 Acceptance: `StudentDetailViewModel` — `ApplyQueryAttributes` populates header, risk colors, and counts; contact form save grows `Interventions`; invalid contact sets `HasContactError`; `HasNo*` empty states. Back navigation goes through the navigator seam. Verify the test passes.

## 6. Students list

- [ ] 6.1 Integration: seed students with attendance/submissions; `StudentsQueryService` over `EfTrackingRepository`/`EfStudentRepository` returns `StudentsQueryResult` with correct risk summary; exercise search, risk, contact, attendance/delivery range, and active-status filters plus sorting. Verify the test passes.
- [ ] 6.2 Acceptance: `StudentsViewModel` — load rows + KPI counts; filters narrow `FilteredStudents`; row selection updates `SelectedCount`/`SelectedCountText` and `IsAllSelected`; bulk attendance and submission marks apply to exactly the selected rows. Verify the test passes.

## 7. Commission import

- [ ] 7.1 Integration: `EfCommissionImportRepository` + `CommissionImportService` — parse → preview → insert into existing section and into a new section; DNI/legajo conflict detection with merge vs omit decisions; blocked rows reported. Verify the test passes.
- [ ] 7.2 Acceptance: `CommissionImportViewModel` standalone — 3-step wizard (`IsFileStep` → `IsDestinationStep` → `IsPreviewStep`), `AcceptFileAsync` with a temp CSV, `ConflictChoice.Merge` toggles produce decisions, confirm applies and closes. Verify the test passes.
- [ ] 7.3 Acceptance: the wizard through `StudentsViewModel.Import` — open from the students screen, complete an import, return to a refreshed roster. Verify the test passes.

## 8. Test data and fixtures

- [ ] 8.1 Add the shared acceptance seed helper (3–5 students across risk levels, one section, one session, one assignment) and a temp-CSV file helper with dispose cleanup. Verify import acceptance tests use it and leave no temp files behind.

## 9. Integration

- [ ] 9.1 Run `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj`, `dotnet test GestionEscolar.Data.Tests/GestionEscolar.Data.Tests.csproj`, and `dotnet test GestionEscolar.UI.Tests/GestionEscolar.UI.Tests.csproj`; verify all three are green.
- [ ] 9.2 Run coverage (coverlet) and verify the report includes the five features' ViewModels and services; confirm acceptance tests touched no MAUI controls, no real DB, and no network.
