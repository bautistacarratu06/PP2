# Code Quality Report — GestionEscolar

**Scope analyzed:** `UI/` and `GestionEscolar.Core/` (out of scope, as requested: `GestionEscolar.Data/` and `GestionEscolar.Tests/`, except where needed to explain the test-project split).
**Purpose:** locate SOLID violations, over-long methods, clean-code violations, explain the existence of `GestionEscolar.Data.Tests`, and scout candidates for acceptance-style ViewModel tests.

---

## 1. SOLID violations

### 1.1 SRP — Single Responsibility

**`UI/ViewModels/StudentsViewModel.cs` (799 lines) is a god class.** It carries ~55 `[ObservableProperty]` fields and 20 `On…Changed` hooks, with at least 9 distinct responsibilities:

| Responsibility | Lines (approx.) |
|---|---|
| Data loading + reload serialization (`_reloadPending`, `_boundSectionId`) | 247–320, 710–715 |
| Column filters (text, %, risk, contact, active) + filter building | 81–118, 717–767 |
| Sorting | 120–124, 358–370 |
| Row selection / bulk-select state | 18–19, 393–402, 647–681 |
| Add-student form (7 fields + error) | 138–163, 526–580 |
| Action palette / wizard (steps, search, keyboard highlight) | 165–178, 421–469, 626–645 |
| Bulk attendance & submission dialogs (pickers, defaults) | 471–505, 599–624 |
| Deactivation | 507–515 |
| Navigation + presentation strings (`SelectedCountText`, `ShowTable`, …) | 69–70, 189–208, 517–524 |

It also embeds a child ViewModel (`Import`, L41) and defines `StudentsActionKind` / `ActionPaletteItem` in the same file (L792–799). Natural split: `StudentFilterState`, `StudentSelectionState`, `AddStudentViewModel`, `BulkActionsViewModel`.

**`UI/ViewModels/CommissionImportViewModel.cs` (359 lines)** mixes wizard state, file picking, disk reads (`File.ReadAllTextAsync`, L143), CSV parsing, preview building, confirmation, section list management and validation (`StudentEntryRules.SectionNameError`, L202) in one class. `ConfirmAsync` (L256, ~50 lines) orchestrates three flows at once.

**`UI/ViewModels/StudentDetailViewModel.cs` (317 lines)** mixes header data + risk colors, three history loads with formatting (L244–257), the "register contact" form (L166–217), `IQueryAttributable` parsing (L134–164), navigation (L313) and formatting helpers (L270–293).

**`GestionEscolar.Core/Services/CommissionImportService.cs` (231 lines)** does five jobs: CSV file detection (`IsCsvFile`, L19), CSV parsing/validation (`Read`, L25–77), conflict detection (`BuildPreview`, L139–186), section-name validation (L90) and persistence orchestration (`CreateAsync`/`ApplyAsync`), plus text normalization (`NormalizeHeader`, L223–230) and user-facing text (`Explain`, L200–215). Natural split: `RosterCsvReader`, `ImportConflictDetector`, orchestrator.

**Other SRP offenders:** `StudentEntryRules` (validation + display formatting + section-name validation), `RiskSettingsService` (validation + UI-text parsing + mutating the shared `RiskConfig` singleton), `MauiProgram.CreateMauiApp` (~95 lines: DI + fonts + hardcoded Windows title-bar colors + temp-file boot logging + config loading), `SectionScopedViewModel` (section list + busy serialization + date formatting).

### 1.2 OCP — Open/Closed

- Adding a fourth bulk action requires editing `ConfirmAttendanceAsync`/`ConfirmSubmissionAsync`/`ConfirmDeactivateAsync` (StudentsViewModel L471–515), the `IsAttendanceAction`/`IsSubmissionsAction`/`IsDeactivateAction` flags (L203–205) and `RebuildActionCatalog` (L626–632). A command/strategy-per-action design would remove this.
- `ToggleRisk` (L323–351) hardcodes the three risk levels; the `FilterRiskAlto/Medio/Bajo` bools and `HighActive/MediumActive/LowActive` triplet are copy-pasted per level.
- `Enums/DbStatusValues.cs` has ~6 switch blocks that must all be edited together for each new enum value; each `Parse*` method re-lists every value.
- `StudentsQueryService.ApplyFilters` (L53–122, ~70 lines) is a growing `if`-chain, one per filter field; `ApplySort` (L128–139) is a 9-value switch.
- Status string ↔ enum ↔ UI-kind mappings are duplicated across `StudentsViewModel` (L769–789), `StudentDetailViewModel` (L272–291), `StatusRow` (L45–50), `SegmentedControl` (L46–48), `RiskPill`, `KpiCard`, `RiskBadge` — adding a status or risk level touches all of them.

### 1.3 LSP — Liskov

No hard violations (only one inheritance chain: `CalendarViewModel : SectionScopedViewModel`, which overrides correctly). However:

- `StudentsViewModel` and `StudentDetailViewModel` **do not** use `SectionScopedViewModel` and instead duplicate its `IsBusy`/`ErrorMessage`/`StatusMessage`/section-changed logic.
- `StudentsViewModel` L37 and `SectionScopedViewModel` L22 subscribe to `SectionSelection.Changed` in constructors and never unsubscribe — with transient ViewModels on a singleton `SectionSelection`, instances leak. Pages unsubscribe in `StudentDetailPage` L38–44, so the pattern is inconsistent.

### 1.4 ISP — Interface Segregation

- `ICalendarRepository` is shared by three services; `AttendanceService` and `SubmissionService` each depend on the whole interface but use only `GetClassSessionsAsync` / `GetAssignmentsAsync`. Splitting into `IClassSessionRepository` / `IAssignmentRepository` would fit better.
- `IStudentDetailsRepository` mixes three history reads with one write (`AddInterventionAsync`).
- `ICommissionImportRepository.GetStudentsAsync` returns **every** student in the DB, and `GetSectionsAsync` duplicates `ISectionRepository.GetSectionsAsync`.
- `IRiskSummaryRepository` has **no Core consumer** (its own comment admits it); `EfRiskSummaryRepository` re-implements `RiskRule` a second time. Dead or duplicate abstraction.
- On the UI side, `StudentsViewModel` exposes ~55 properties + 20 commands as one binding surface — a de-facto fat interface to XAML.

### 1.5 DIP — Dependency Inversion

**No UI ViewModel, service or page depends on a single interface.** Core defines 10 repository interfaces, but the UI consumes only concrete, non-virtual service classes (`StudentsQueryService`, `StudentService`, `AttendanceService`, `SubmissionService`, …). `grep virtual` in `Core/Services` returns nothing — nothing can be mocked.

| Class | Constructor dependencies (all concrete) |
|---|---|
| `StudentsViewModel` | `StudentsQueryService`, `StudentService`, `AttendanceService`, `SubmissionService`, `SectionSelection`, `CommissionImportViewModel` |
| `CommissionImportViewModel` | `CommissionImportService`, `SectionSelection` |
| `StudentDetailViewModel` | `StudentDetailsService` |
| `CalendarViewModel` | `SectionSelection`, `CalendarService` |
| `SettingsViewModel` | `RiskSettingsService`, `RiskConfig` |

Additional DIP problems:

- Ambient/static dependencies: `Shell.Current.GoToAsync`, `MainThread.BeginInvokeOnMainThread`, `FilePicker.Default`, `File.ReadAllTextAsync`, `DateTime.Today` (14 occurrences in UI), `Debug.WriteLine`. No `ILogger`, no `TimeProvider`, no navigation abstraction.
- Static calls inside Core services: `RiskRule.Evaluate`, `CsvParser.ParseLine`, `StudentEntryRules.*`, `CalendarEntryRules.*`, `DbStatusValues.ToDbValue` — none swappable in tests.
- `RiskConfig` is a **mutable singleton** mutated at runtime by `RiskSettingsService.Apply` — hidden shared state with no synchronization.
- `StudentsViewModel` takes another ViewModel (`CommissionImportViewModel`) as a constructor dependency, and the page re-wires it manually (`StudentsPage.xaml.cs` L14).
- `StudentDetailViewModel` is fed through Shell's weakly-typed query dictionary and hardcodes the route `"//StudentsPage"` (L313).

---

## 2. Long methods (> ~30 lines)

| Method | File | Span | ~Lines |
|---|---|---|---|
| `CreateMauiApp` | `UI/MauiProgram.cs` | 14–109 | **95** |
| `LoadAsync` | `UI/ViewModels/StudentsViewModel.cs` | 247–320 | **74** (nested do/while, try/catch in try/finally, row-building closure, 4 state resets) |
| `ApplyFilters` | `Core/Services/StudentsQueryService.cs` | 53–122 | **70** |
| `ConfirmAsync` | `UI/ViewModels/CommissionImportViewModel.cs` | 256–305 | 50 |
| `LoadAsync(int, int?)` | `UI/ViewModels/StudentDetailViewModel.cs` | 219–268 | 50 |
| `ParseLine` | `Core/Services/CsvParser.cs` | 7–65 | 59 (4 levels of nesting) |
| `NextAsync` | `UI/ViewModels/CommissionImportViewModel.cs` | 178–220 | 43 |
| `Read` | `Core/Services/CommissionImportService.cs` | 25–77 | 53 |
| `BuildPreview` | `Core/Services/CommissionImportService.cs` | 139–186 | 48 |
| `SaveStudentAsync` | `UI/ViewModels/StudentsViewModel.cs` | 544–580 | 37 |
| `SaveContactAsync` | `UI/ViewModels/StudentDetailViewModel.cs` | 181–217 | 37 |
| `Import` | `Core/Services/StudentCsvImporter.cs` | 23–61 | 39 |
| `ApplyAsync` | `Core/Services/CommissionImportService.cs` | 102–137 | 36 |
| `BuildFilter` | `UI/ViewModels/StudentsViewModel.cs` | 717–749 | 33 |
| `ApplyAsync` (lambda `OnWindowCreated`) | `UI/MauiProgram.cs` | 41–81 | 40 |
| `OpenAsync` | `UI/ViewModels/CommissionImportViewModel.cs` | 80–114 | 35 |
| `AcceptFileAsync` | `UI/ViewModels/CommissionImportViewModel.cs` | 129–162 | 34 |
| `LoadPreviewAsync` | `UI/ViewModels/CommissionImportViewModel.cs` | 222–254 | 33 |
| `RegisterContactAsync` | `Core/Services/StudentDetailsService.cs` | 37–70 | 34 |
| `AddAsync` | `UI/ViewModels/CalendarViewModel.cs` | 69–101 | 33 |
| `ApplyQueryAttributes` | `UI/ViewModels/StudentDetailViewModel.cs` | 134–164 | 31 |
| `RunAsync` | `UI/ViewModels/SectionScopedViewModel.cs` | 90–121 | 32 |
| `Import` | `Core/Services/StudentCsvImporter.cs` | 23–61 | 39 |

Borderline: `LoadActionPickersAsync` (StudentsViewModel L599–624, does two unrelated pickers), `BuildFilter` (L717–749), `ApplyQueryAttributes`, `WireFileDrop` (StudentsPage L37–67).
