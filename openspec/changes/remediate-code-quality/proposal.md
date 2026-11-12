# Proposal

## Why

`StudentsViewModel` (799 lines, about 55 observable properties), `CommissionImportViewModel`, `StudentDetailViewModel`, and `CommissionImportService` each mix unrelated jobs, and every screen ViewModel depends on a concrete Core service. A change to filters, bulk actions, import, or risk status edits the same types as loading, selection, and navigation, and those services cannot be substituted. The findings are in `CODE_QUALITY_REPORT.md` (SOLID and long methods). The report stops after that table.

## What Changes

Screen behavior stays the same: roster filters and sort, add student, bulk attendance, bulk submissions, deactivation, commission import, student detail, contact registration, calendar, and risk settings. This change only moves and splits code in `UI/` and `GestionEscolar.Core/`.

- Split `StudentsViewModel` into focused types for column filters, row selection, the add-student form, and bulk actions, and leave the screen ViewModel to load rows and host those pieces. Move `StudentsActionKind` and `ActionPaletteItem` out of that file.
- Split `CommissionImportViewModel` so wizard steps, file selection, and preview confirmation are not one class, and so the ViewModel does not read the file from disk.
- Split `StudentDetailViewModel` so header and risk presentation, history loading, the contact form, and Shell query parsing are separate. Replace the hardcoded `"//StudentsPage"` navigation call with the navigation seam the page already can supply.
- Split `CommissionImportService` into CSV reading, conflict detection, and persistence orchestration. Move header normalization and conflict explanation out of the orchestrator.
- Shorten every method the report lists past ~30 lines by extracting the steps already inside it: `CreateMauiApp` and its window-created handler, `StudentsViewModel.LoadAsync`, `SaveStudentAsync`, and `BuildFilter`, `StudentsQueryService.ApplyFilters`, `CommissionImportViewModel.ConfirmAsync`, `NextAsync`, `OpenAsync`, `AcceptFileAsync`, and `LoadPreviewAsync`, `StudentDetailViewModel.LoadAsync`, `SaveContactAsync`, and `ApplyQueryAttributes`, `CsvParser.ParseLine`, `CommissionImportService.Read`, `BuildPreview`, and `ApplyAsync`, `StudentCsvImporter.Import`, `StudentDetailsService.RegisterContactAsync`, `CalendarViewModel.AddAsync`, and `SectionScopedViewModel.RunAsync`.
- Replace the copy-pasted risk-level and status mappings in `StudentsViewModel`, `StudentDetailViewModel`, `StatusRow`, `SegmentedControl`, `RiskPill`, `KpiCard`, `RiskBadge`, and `DbStatusValues` with one mapping per concept.
- Make `StudentsViewModel` and `StudentDetailViewModel` share the section-scoped busy, error, and status behavior in `SectionScopedViewModel` instead of a second copy.
- Unsubscribe `SectionSelection.Changed` when a ViewModel leaves the screen. `StudentsViewModel` and `SectionScopedViewModel` subscribe in the constructor and never unsubscribe; `StudentDetailPage` already unsubscribes its own handler.
- Register the Core use-case services behind interfaces and inject those interfaces into ViewModels. Keep the current concrete classes as the implementations, with the same public methods `GestionEscolar.Tests` already calls.
- Separate validation, settings-text parsing, and mutation of the shared `RiskConfig` inside `RiskSettingsService`. Thresholds still load and save as they do now.
- Separate validation from display formatting in `StudentEntryRules`.

## Capabilities

### New Capabilities

None. No new user-facing behavior.

### Modified Capabilities

None. `openspec list --specs` returns no capabilities, and this change does not alter specified behavior. `.openspec.yaml` sets `skip_specs: true`.

## Impact

- `UI/ViewModels/` (`StudentsViewModel`, `CommissionImportViewModel`, `StudentDetailViewModel`, `CalendarViewModel`, `SettingsViewModel`, `SectionScopedViewModel`), the XAML bound to those properties (`StudentsPage.xaml` and `StudentDetailPage.xaml`), and `UI/MauiProgram.cs`.
- `GestionEscolar.Core/Services/` (`CommissionImportService`, `StudentsQueryService`, `CsvParser`, `StudentCsvImporter`, `StudentDetailsService`, `RiskSettingsService`, and the other use-case services that gain an interface), `Models/StudentEntryRules.cs`, `Models/RiskConfig.cs`, `Enums/DbStatusValues.cs`, and `CoreServiceCollectionExtensions.cs`.
- `GestionEscolar.Data/` and both test projects stay as they are, except a constructor or registration line that no longer compiles after a type moves. Repository interfaces are not split: `ICalendarRepository`, `IStudentDetailsRepository`, `ICommissionImportRepository`, and `IRiskSummaryRepository` are implemented in Data, which the report left out of scope.
- No new test project. The report does not contain the notes it promised on `GestionEscolar.Data.Tests` or acceptance-style ViewModel tests.

## Assumptions

- The page `BindingContext` stays the screen ViewModel. Child state may move to nested objects; XAML paths update to match. The controls and commands the pages use stay reachable on that path.
- `DateTime.Today` inside the ViewModels this change already edits is passed in as the current date, the same way `CommissionImportService.CreateAsync` and `ApplyAsync` already take `today`. `EfTrackingRepository` is not touched.
- `Debug.WriteLine` and a general logging abstraction are left as they are.
- Adding a fourth bulk action or a new risk level should not require a new parallel flag trio in `StudentsViewModel`, but this change does not add either.
