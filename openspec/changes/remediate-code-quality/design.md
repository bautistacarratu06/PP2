# Design

## Context

See `proposal.md` for why this change exists. The notes below are the current structure the approach has to fit.

`StudentsPage` sets `BindingContext` to `StudentsViewModel` and sets the import wizard's `BindingContext` to `viewModel.Import`. Every other students-page binding is flat on the screen ViewModel (`NameFilter`, `SaveStudentCommand`, `IsAllSelected`, `IsAttendanceAction`, …). `StudentDetailPage` binds flat on `StudentDetailViewModel` and already subscribes to `SectionSelection.Changed` in `OnAppearing` and unsubscribes in `OnDisappearing`. `StudentsViewModel` and `SectionScopedViewModel` subscribe in the constructor and never unsubscribe. `CalendarViewModel` is the only `SectionScopedViewModel` subclass.

Core use cases are registered as concrete transients in `AddGestionEscolarCore`. `CoreRegistrationTests` asserts those concrete service types and a singleton `RiskConfig`. Tests construct the concrete classes (`new StudentsQueryService(...)`, `service.Read(...)`, `CommissionImportService.IsCsvFile`). `RiskSettingsService.Apply` writes `HighThreshold` and `MediumThreshold` on the shared `RiskConfig` instance that `StudentsQueryService` already holds.

`DbStatusValues` maps each enum to the CHECK-constraint string with a `ToDbValue` switch and a `Parse*` switch that calls back into `ToDbValue`. UI labels ("Presente", "Entregó", risk colors) are separate switches in `StudentsViewModel`, `StudentDetailViewModel`, `StatusRow`, `SegmentedControl`, `RiskPill`, `KpiCard`, and `RiskBadge`.

## Goals / Non-Goals

**Goals:**

- Cut the screen ViewModels and `CommissionImportService` along the responsibilities named in the proposal, without changing what the screens do.
- Give each edited ViewModel a constructor dependency that is an interface, while the existing test constructors and `CoreRegistrationTests` keep passing.
- One subscription lifetime for `SectionSelection.Changed`: subscribe on appear, unsubscribe on disappear.
- One table per status enum for DB strings, one label map for attendance/submission UI text, one risk-color map.
- Methods named in the proposal drop under ~30 lines by extracting the steps they already contain.

**Non-Goals:**

- Splitting repository interfaces or editing `GestionEscolar.Data` implementations.
- Replacing the mutable `RiskConfig` singleton, or adding locks around it.
- A logging abstraction, a navigation framework, or a clock inside Data (`EfTrackingRepository` stays as it is).
- New tests. Existing tests must still pass without assertion edits.

## Decisions

### 1. Nested ViewModels, page BindingContext unchanged

`StudentsViewModel` keeps roster loading, search text, risk summary counts, empty/error presentation, and navigation to a student. It exposes four children:

- `Filters` — column text, percent bounds, risk set, contact, active status, sort, which filter popup is open, and `StudentsQueryFilter` building.
- `Selection` — selected ids, all-selected, selected-count text.
- `AddStudent` — the seven fields, add error, show/save/cancel.
- `Actions` — palette, step, highlight, session/assignment pickers, attendance, submission, and deactivate confirms.

`StudentsActionKind` and `ActionPaletteItem` move next to `Actions`. `Import` stays the nested `CommissionImportViewModel` the wizard already binds.

`StudentsPage.xaml` gains a prefix (`Filters.`, `Selection.`, `AddStudent.`, `Actions.`) only where the property moves. The wizard code-behind assignment stays. `StudentDetailViewModel` keeps header, metrics, and the three history collections, and exposes `Contact` for the register-contact form. Query parsing becomes `StudentDetailQuery.Read`, called from `ApplyQueryAttributes`. History row text becomes a formatter used by the load method, not a third ViewModel.

Alternative: partial classes. That only splits the file. Alternative: retarget each region with `BindingContext` in code-behind, as the wizard does. The filter row and the action palette are inline in the page grid, so a prefix is easier to see than a second context.

### 2. Share busy/error state without inheriting the section screen

`SectionScopedViewModel` owns a section list, selects a section, and reloads on that selection. The students screen and the detail screen do not. Pull `IsBusy`, `ErrorMessage`, `StatusMessage`, `HasError`, `HasStatus`, and the pending-work loop in `RunAsync` into a base both of those ViewModels and `SectionScopedViewModel` use. `StudentsViewModel.LoadAsync` and `StudentDetailViewModel.LoadAsync` call that loop instead of their own nested try/finally. Detail's "section changed → go back to the roster" stays on `StudentDetailPage`.

Alternative: make both ViewModels extend `SectionScopedViewModel`. That would load a section collection and reload-on-select into screens whose section behavior is different.

### 3. Appear / disappear owns the section subscription

Remove the constructor subscriptions in `StudentsViewModel` and `SectionScopedViewModel`. Add `Subscribe` / `Unsubscribe` on the shared base (or on the section helper the base uses). `StudentsPage` and `CalendarPage` call them from `OnAppearing` / `OnDisappearing`, matching `StudentDetailPage`. A page that reappears subscribes once.

Alternative: `IDisposable` from the constructor subscription. A transient page that disappears and appears again would stay unsubscribed after the first dispose. Weak events would be a new mechanism; the page already has the appear/disappear pattern.

### 4. Interfaces beside the concrete registrations

For each use-case class registered in `AddGestionEscolarCore`, add an interface with the public instance methods that class already has (`IStudentsQueryService`, `IStudentService`, `IAttendanceService`, `ISubmissionService`, `ICommissionImportService`, `IStudentDetailsService`, `ICalendarService`, `IRiskSettingsService`, `ISectionService`, `ITrackingService`). The class keeps its constructor and its public methods, so `new StudentsQueryService(...)` in tests still compiles.

Registration stays `AddTransient<StudentsQueryService>()` and adds `AddTransient<IStudentsQueryService>(sp => sp.GetRequiredService<StudentsQueryService>())`. ViewModels take the interface. `SettingsViewModel` still takes `RiskConfig` for the values it displays; that object is the shared thresholds, not a use case.

`CommissionImportService.Read` and `IsCsvFile` stay public and forward to `RosterCsvReader`. `PreviewAsync` forwards conflict building to `ImportConflictDetector`. `CreateAsync` / `ApplyAsync` stay on the service and call those types. `CommissionImportServiceTests` and `CourseTrackingDatabaseTests` keep calling `Read` and `IsCsvFile`.

Alternative: register only the interface. `CoreRegistrationTests` looks up `ServiceType == typeof(StudentsQueryService)` and would fail. Alternative: edit that test. The proposal keeps assertion edits out of scope.

### 5. File, navigation, and today as UI seams

`CommissionImportViewModel` (the file step) depends on a small `ICsvFileSource` in the UI project: pick a path, read its text. The production implementation calls `FilePicker.Default` and `File.ReadAllTextAsync`. Parsing stays `ICommissionImportService.Read`.

`StudentDetailViewModel` depends on `IAppNavigation` with `Task GoToStudentsAsync()`. The implementation calls `Shell.Current.GoToAsync("//StudentsPage")`. The route string lives in that one implementation.

ViewModels this change already edits take `IClock` with `DateTime Today { get; }`. Production returns `DateTime.Today`. Those ViewModels pass `clock.Today` into service methods that already accept `today`. No clock is added to Core services or to Data.

Alternative: `TimeProvider`. It returns a `DateTimeOffset` and is a heavier fit for call sites that already pass `DateTime`. Alternative: keep calling `DateTime.Today` at each site. That leaves the DIP note the report made on the types we are already opening.

### 6. One map per concept, not a strategy per value

`DbStatusValues` builds `ToDbValue` and `Parse*` from one list of `(enum value, db string)` pairs per enum. Unknown strings still throw the same `ArgumentOutOfRangeException` messages. Null and empty still parse as null.

UI attendance and submission labels ("Presente", "Ausente", "Entregó", "No entregó", and the success/danger kind) live in one UI type used by the students actions, student detail, `StatusRow`, and `SegmentedControl`. Risk colors live in one `RiskPalette` used by `RiskBadge`, `RiskPill`, and `KpiCard`.

Bulk actions become one list of definitions (kind, title, how to confirm). `IsAttendanceAction` and its siblings become "the selected definition is this kind". Risk filter checkboxes stay bools for the XAML, and they write one `SetRisk(RiskLevel, bool)` instead of three copied handlers. `ToggleRisk` updates that set. This change does not add a fourth action or a fourth risk level.

Alternative: a class per bulk action and a class per risk level. Three actions do not need a hierarchy.

### 7. Risk settings and student rules

`RiskSettingsService.SaveAsync` calls a pure parser that returns either the two ints or the existing failure message (`NonNumericMessage`, `InvalidThresholdMessage`). `Apply` remains the only writer of `RiskConfig`. Load and save still call `Apply` after a successful read or write.

`StudentEntryRules` keeps `Normalize` and the length constants. `Label` moves to a name formatter; `StudentService` calls that formatter when it builds the duplicate legajo/DNI message. `SectionNameError` moves to a section-name rule type; `CommissionImportService` and the import ViewModel call it there.

### 8. Long methods stay in the type that owns the step

After the splits, each method named in the proposal is shortened by private methods on that type: one step per method (read rows, build the filter, parse a CSV field, apply one filter predicate, paint the Windows title bar, write the boot log). `StudentsQueryService.ApplyFilters` becomes a sequence of named predicates over `StudentsQueryFilter`. `ApplySort` can keep its switch; the nine fields are one sort key, and the length is the switch itself. No new project, package, or source generator.

`MauiProgram.CreateMauiApp` calls private methods for fonts, the Windows window setup (title bar, maximize, boot log), and the existing `LoadAppSettings`. DI registration stays in `CreateMauiApp` so the composition root is still obvious.

## Risks / Trade-offs

- [A renamed binding path fails only at runtime] → Move a property only when the XAML prefix is updated in the same task. Keep the property name. After each screen split, run the students page, the detail page, calendar, and settings and exercise filter, add, bulk action, import, contact, and back navigation.
- [Appear/disappear double-subscribe] → Delete the constructor handler in the same task that adds the page hooks. Subscribe only from `OnAppearing`.
- [Interface factory resolves a second transient] → The factory calls `GetRequiredService` of the concrete transient. One resolution of the interface is one instance, same as today.
- [Forwarding `Read` / `IsCsvFile` hides the new reader] → The service methods are one-line forwards. New CSV work goes to `RosterCsvReader`. Accepted so Data.Tests and Core tests do not change.
- [`RiskConfig` can still tear between the two threshold assignments] → Pre-existing. This change does not add synchronization. Both assignments stay in `Apply`.
- [Extracting `RunAsync` changes which error string a failed load shows] → Keep the message each screen already uses (`ReportError` takes the message). Do not switch the students screen to the section screen's "No se pudieron cargar los datos."

## Migration Plan

No schema, configuration, or package change. The app composes the same services plus the new UI seams (`ICsvFileSource`, `IAppNavigation`, `IClock`) in `MauiProgram`.

Apply in the task order: Core seams and extractions first (tests keep passing), then ViewModel splits one screen at a time, then XAML prefixes for that screen, then the subscription move. Each task leaves `dotnet test` for `GestionEscolar.Tests` green. Rollback is reverting the change; there is no data to migrate.
