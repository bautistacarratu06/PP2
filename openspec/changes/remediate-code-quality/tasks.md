# Tasks

## 1. Core service interfaces

- [ ] 1.1 Add an interface for each use-case class registered in `AddGestionEscolarCore` (`ITrackingService`, `IStudentsQueryService`, `IRiskSettingsService`, `ISectionService`, `IStudentDetailsService`, `IStudentService`, `ICommissionImportService`, `ICalendarService`, `IAttendanceService`, `ISubmissionService`) with that class's existing public instance methods. Implement the interface on the concrete class without changing constructors or method signatures. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter CoreRegistrationTests` still passes.
- [ ] 1.2 Keep `AddTransient<Concrete>()` and add `AddTransient<IInterface>(sp => sp.GetRequiredService<Concrete>())` for each service from 1.1. Leave ViewModel constructors on the concrete types for now. Verify `CoreRegistrationTests` still passes and `dotnet build GestionEscolar.Core/GestionEscolar.Core.csproj` succeeds.

## 2. Import and student-rule splits

- [ ] 2.1 Move CSV detection, header normalization, and `Read` into `RosterCsvReader`. Leave `CommissionImportService.IsCsvFile` and `Read` as one-line forwards. Break `Read` into helpers so the reader method and each forwarder are at most 30 lines. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter CommissionImportServiceTests` passes and `dotnet build GestionEscolar.Data.Tests/GestionEscolar.Data.Tests.csproj` succeeds.
- [ ] 2.2 Move `BuildPreview` and conflict explanation into `ImportConflictDetector`. `PreviewAsync` forwards to it. Shorten `ApplyAsync` by extracting the create-section and apply-decisions steps it already has, each at most 30 lines. Verify `CommissionImportServiceTests` passes.
- [ ] 2.3 Move `StudentEntryRules.Label` to a name formatter and call it from `StudentService`. Move `SectionNameError` to a section-name rule type and call it from `CommissionImportService` and `CommissionImportViewModel`. Leave `Normalize` and the length constants on `StudentEntryRules`. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter "StudentEnrollmentTests|CommissionImportServiceTests"` passes.

## 3. Status maps, filters, and long Core methods

- [ ] 3.1 Rebuild `DbStatusValues` so each enum has one list of `(value, db string)` pairs shared by `ToDbValue` and `Parse*`. Keep null/empty parsing as null and the same `ArgumentOutOfRangeException` messages for unknown strings. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj` passes.
- [ ] 3.2 Turn `StudentsQueryService.ApplyFilters` into a sequence of named predicates over `StudentsQueryFilter`, each at most 30 lines. Leave `ApplySort` as the existing sort-key switch. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter StudentsQueryServiceTests` passes.
- [ ] 3.3 Shorten `CsvParser.ParseLine`, `StudentCsvImporter.Import`, and `StudentDetailsService.RegisterContactAsync` by extracting the steps already inside them, so each of those methods is at most 30 lines. Do not change their results. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj` passes.

## 4. Risk settings parser

- [ ] 4.1 Extract the high/medium text parse and range check from `RiskSettingsService.SaveAsync` into a pure function that returns the two ints or the existing `NonNumericMessage` / `InvalidThresholdMessage`. Keep `Apply` as the only writer of `RiskConfig`, still called from `LoadAsync` and from a successful save. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter RiskSettingsServiceTests` passes.

## 5. UI seams and shared labels

- [ ] 5.1 Add `IClock` (`Today`), `ICsvFileSource` (pick a path, read all text), and `IAppNavigation` (`GoToStudentsAsync` calling `Shell.Current.GoToAsync("//StudentsPage")`). Register the production implementations in `MauiProgram`. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 5.2 Add one UI attendance/submission label map and use it from `StudentsViewModel.TryParseAttendance` / `TryParseSubmission`, `StudentDetailViewModel` history text, `StatusRow.StatusKind`, and `SegmentedControl` options. Add `RiskPalette` and use it from `RiskBadge`, `RiskPill`, and `KpiCard`. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.

## 6. Busy state and section subscription

- [ ] 6.1 Move `IsBusy`, `ErrorMessage`, `StatusMessage`, `HasError`, `HasStatus`, and the pending-work loop out of `SectionScopedViewModel` into a base that `SectionScopedViewModel` uses. Keep each screen's existing error text (do not replace the students or detail message with "No se pudieron cargar los datos."). Shorten the loop so `RunAsync` is at most 30 lines. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds and `CalendarViewModel` still compiles as a `SectionScopedViewModel`.
- [ ] 6.2 Remove the `SectionSelection.Changed` subscriptions from the `StudentsViewModel` and `SectionScopedViewModel` constructors. Subscribe in `OnAppearing` and unsubscribe in `OnDisappearing` on `StudentsPage` and `CalendarPage`, same as `StudentDetailPage`. Verify a search shows `Changed +=` only inside `OnAppearing` and each of those pages has a matching `-=` in `OnDisappearing`, and `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.

## 7. Students screen

- [ ] 7.1 Move column filters, sort, risk toggles, and `BuildFilter` onto a `Filters` child of `StudentsViewModel`. Point the moved `StudentsPage.xaml` bindings at `Filters.` and keep the page `BindingContext` on `StudentsViewModel`. `SetRisk(RiskLevel, bool)` is the only writer of the risk filter set. Shorten `BuildFilter` to at most 30 lines. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 7.2 Move selected ids, all-selected, and selected-count text onto a `Selection` child. Update the matching bindings to `Selection.`. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 7.3 Move the add-student fields, error, and save/cancel onto an `AddStudent` child. Update those bindings to `AddStudent.`. Shorten save so the method is at most 30 lines. Pass `IClock.Today` into the student service. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 7.4 Move the action palette, pickers, and confirms onto an `Actions` child backed by one list of action definitions (kind, title, confirm). Move `StudentsActionKind` and `ActionPaletteItem` out of `StudentsViewModel.cs`. Update those bindings to `Actions.`. Shorten `StudentsViewModel.LoadAsync` to at most 30 lines using the busy-state loop from 6.1. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 7.5 Change `StudentsViewModel` and `AddStudent` / `Actions` constructors to the service interfaces from 1.1. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds and `CoreRegistrationTests` still passes.

## 8. Commission import wizard

- [ ] 8.1 Move file pick and disk read out of `CommissionImportViewModel` onto a file step that depends on `ICsvFileSource` and `ICommissionImportService`. The wizard keeps step changes, section choice, preview, and confirm, and calls that file step. Shorten `OpenAsync`, `AcceptFileAsync`, `NextAsync`, `LoadPreviewAsync`, and `ConfirmAsync` so each is at most 30 lines; `ConfirmAsync` delegates the new-section and existing-section flows. Pass `IClock.Today` into create/apply. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds and `CommissionImportServiceTests` still passes.

## 9. Student detail

- [ ] 9.1 Move the register-contact form onto a `Contact` child and point those `StudentDetailPage.xaml` bindings at `Contact.`. Shorten `SaveContactAsync` to at most 30 lines. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 9.2 Parse Shell query attributes with `StudentDetailQuery.Read` and format history rows with one formatter. Shorten `LoadAsync` and `ApplyQueryAttributes` to at most 30 lines, using the busy-state loop from 6.1 and keeping the current error text. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 9.3 Inject `IStudentDetailsService`, `IAppNavigation`, and `IClock` into the detail ViewModel. Back navigation calls `GoToStudentsAsync`. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds and `StudentDetailViewModel` has no `Shell.Current` reference.

## 10. Composition root and remaining screens

- [ ] 10.1 Split `MauiProgram.CreateMauiApp` into private methods for fonts, Windows title bar / maximize / boot log, and the existing settings load, so `CreateMauiApp` and the window-created handler are each at most 30 lines. Keep service registration in `CreateMauiApp`. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 10.2 Shorten `CalendarViewModel.AddAsync` to at most 30 lines and take `ICalendarService` plus `IClock`. Change `SettingsViewModel` to take `IRiskSettingsService` and still read `RiskConfig` for the displayed thresholds. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds and `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter "CalendarServiceTests|RiskSettingsServiceTests"` passes.

## 11. Integration

- [ ] 11.1 Run `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj`, `dotnet build GestionEscolar.Data.Tests/GestionEscolar.Data.Tests.csproj`, and `dotnet build UI/UI.csproj -f net10.0-maccatalyst`. Verify all three succeed.
- [ ] 11.2 Run the app and confirm the students roster still filters, sorts, adds a student, runs each bulk action, and imports a CSV; student detail still loads history, saves a contact, and returns to the roster; calendar still adds an item; settings still save thresholds. Verify each of those flows completes without a binding error or a change in the success and validation messages.

## Workflow follow-up

- Archive the change after the implementation is reviewed.
- Verify the archived specs and change status.
