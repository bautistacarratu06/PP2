# Design

## Context

See `proposal.md` for motivation and `specs/ui-test-coverage/spec.md` for the behavior contract. The source plan is `TEST_PLAN.md` at the repo root; it ranks the five features and flags two blockers this design resolves.

Current state that shapes the approach:

- `GestionEscolar.Tests` already holds fakes for all 10 repositories (`Fakes/`) and tests Core services with them; `GestionEscolar.Data.Tests` has `SqlServerFixture` (IAsyncLifetime, drop → migrate → `ClearRowsAsync` per test), `StudentSeeder`, and `sample.csv`. Both projects are net10.0, xunit 2.9, coverlet.
- `StudentsViewModel` calls `MainThread.BeginInvokeOnMainThread` in its constructor and `Shell.Current.GoToAsync` to open a detail row; `StudentDetailViewModel` calls `Shell.Current.GoToAsync` in its back command; `SectionScopedViewModel` (base of `CalendarViewModel`) dispatches on section change. `SettingsViewModel` and `CommissionImportViewModel` are pure MVVM.
- `UI.csproj` multi-targets `net10.0-android/-ios/-maccatalyst/-windows`, so a plain net10.0 test project cannot reference it.
- The `remediate-code-quality` change (in progress) already introduces `IClock`, `ICsvFileSource`, and `IAppNavigation` seams and splits the ViewModels into child objects. This change must compose with it: where that change lands first, the seams here reuse its navigation abstraction instead of adding a competing one.

## Goals / Non-Goals

**Goals:**

- Two green suites: acceptance (ViewModels + fakes, no MAUI/DB/network) and integration (real repos on SQL Server Express).
- ViewModels constructible and loadable from plain xUnit tests with no platform runtime.
- Feature coverage in the plan's ranked order, cheapest first, so value lands early.
- Behavior-preserving seams: the app's runtime behavior and bindings do not change.

**Non-Goals:**

- XAML/UI-control automation (no Appium, no device tests).
- Rewriting ViewModels beyond the two seams; no state machine or messaging refactor.
- New repository fakes beyond what exists (the 10 fakes cover all features).
- Changing Core services or repositories to make them testable — they already are.
- E2E import of the `bulk-student-import` change's bulk path; that change owns its own tests.

## Decisions

### 1. TFM strategy: multi-target the test project (plan option A)

`GestionEscolar.UI.Tests` targets `net10.0` and `net10.0-windows10.0.19041.0`, referencing `UI.csproj` directly. Acceptance tests run on the `net10.0-windows` TFM (where the ViewModels compile); the project still builds everywhere CI builds Data/Core.

*Alternative (option B):* extract `ViewModels/` + `Services/` + `Formatting/` into a net10.0 `GestionEscolar.UI.Core` referenced by both UI and tests. Cleaner long-term but a structural move of every ViewModel file plus namespace updates, overlapping the in-flight `remediate-code-quality` split of the same files — the two changes would collide. Option A touches only the new csproj. If option B is ever wanted, it can follow later without invalidating the tests.

### 2. Two seams, injected with defaults

- `IMainThreadDispatcher` — `Post(Action)` / `InvokeAsync(Func<Task>)`. Production impl wraps `MainThread.BeginInvokeOnMainThread`; the test implementation invokes inline.
- `INavigator` — `GoToStudentsAsync()` / `GoToStudentDetailAsync(int sectionId, int studentId)` (or a single `GoToAsync(string)`; final shape follows whatever `remediate-code-quality` lands as `IAppNavigation` — if it has landed, reuse it and skip creating `INavigator`). Production impl wraps `Shell.Current.GoToAsync`; tests inject a recorder fake.

Constructors gain these as optional-with-default or DI-registered parameters so existing test constructors and XAML `BindingContext` creation keep working. `MauiProgram` registers the production implementations.

*Alternative:* null-guard `Shell.Current`/`MainThread` instead of seams. Rejected: silent no-op navigation in production is worse than an explicit seam, and the plan itself recommends the seams.

### 3. Acceptance tests: real services + fakes, one class per feature

Each acceptance test class constructs the real ViewModel with real Core services over the existing fakes, seeded per test (3–5 students across high/medium/low risk, one section, one session, one assignment). `CommissionImportViewModel` is tested standalone before being exercised through `StudentsViewModel.Import`, per the plan's note that it is a child VM. CSV fixtures are written to temp files per test and deleted on dispose.

### 4. Integration tests extend `GestionEscolar.Data.Tests`

New test classes reuse `SqlServerFixture` and the seeder; one class per feature mirroring the acceptance set. The commission-import integration tests exercise parse → preview → decisions → confirm against the real repositories, which also regression-protects the chunked/bulk persistence work from the `support-bulk-student-import` change.

### 5. Execution order follows the plan's ranking

Settings → Calendar → Student detail → Students list → Commission import. The seams (decision 1's `IMainThreadDispatcher`/`INavigator`) land in the setup group before feature 3 (student detail), the first feature that needs them; settings and calendar only need the dispatcher guard.

## Risks / Trade-offs

- [`net10.0-windows` TFM requires Windows to run acceptance tests] → Document that acceptance tests run on Windows (or via the maccatalyst TFM if added later); Core/Data suites remain runnable on macOS.
- [ViewModel constructor changes ripple into XAML or existing tests] → Keep new parameters optional-with-default or registered in DI only; existing `GestionEscolar.Tests` constructors keep compiling.
- [Selection re-entrancy (`_isUpdatingSelection`) can make bulk-action tests flaky] → Drive selection through the same public commands the UI uses, never private fields; assert on observable properties only.
- [Integration tests are slow and order-sensitive] → Per-test cleanup via the existing fixture pattern; no inter-test dependencies; keep one fixture per collection.
- [Overlap with `remediate-code-quality` on the same ViewModel files] → Land the seams after (or rebase onto) that change's splits; the design reuses its `IAppNavigation` if present rather than duplicating it.

## Migration Plan

1. Create `GestionEscolar.UI.Tests` with the TFM strategy and one smoke acceptance test (settings) to prove the reference chain works.
2. Land the two seams with production registrations; verify the app still builds and behaves identically.
3. Add feature suites in ranked order, each landing its integration + acceptance tests together.
4. Rollback: the test project is additive; reverting the seams restores the direct `MainThread`/`Shell` calls.

## Open Questions

None. The TFM fork was resolved with the user: the spec decides, and this design recommends option A.
