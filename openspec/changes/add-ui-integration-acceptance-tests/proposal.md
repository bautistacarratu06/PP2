# Proposal

## Why

The UI layer (`UI/` ViewModels) has no automated tests: `GestionEscolar.Tests` covers Core services with fakes, and `GestionEscolar.Data.Tests` covers repositories against SQL Server, but nothing exercises the ViewModels or the Core services end-to-end through the real repositories. The plan in `TEST_PLAN.md` defines two suites — integration (real EF Core repositories against SQL Server Express) and acceptance (ViewModels driven directly with repository fakes) — and ranks five features by implementation effort. This change builds that test infrastructure and the five feature suites.

## What Changes

- Create a new `GestionEscolar.UI.Tests` xUnit project for acceptance tests that drive the ViewModel classes directly with the existing repository fakes in `GestionEscolar.Tests/Fakes/`, no MAUI UI controls, no real DB, no network.
- Resolve the TFM friction between the multi-targeted `UI.csproj` and a test project (strategy recommended in design.md: multi-target the test project rather than extracting a shared library).
- Introduce two small seams in the ViewModels so they are testable outside a running MAUI app: `IMainThreadDispatcher` (replacing direct `MainThread.BeginInvokeOnMainThread` calls) and `INavigator` (replacing direct `Shell.Current.GoToAsync` calls), with production implementations registered in `MauiProgram`.
- Add integration tests to `GestionEscolar.Data.Tests` (reusing `SqlServerFixture`) and acceptance tests to `GestionEscolar.UI.Tests`, one pair per feature, in the plan's ranked order: settings/risk thresholds, calendar, student detail, students list, commission import.
- Add shared test data helpers: per-test seeding of 3–5 students across risk levels, one section, one class session + one assignment, and temp-file CSV fixtures for the import tests.
- Acceptance tests cover: settings load/save/status flow; calendar item creation and validation; student detail query-attribute loading, contact registration, and empty states; students list loading, filtering, selection, and bulk actions; commission import wizard steps, file acceptance, conflict decisions, and confirm.

## Capabilities

### New Capabilities

- `ui-test-coverage`: Automated integration and acceptance testing of the UI layer — ViewModels testable outside MAUI via dispatch/navigation seams, acceptance tests on fakes, and integration tests against a real database.

### Modified Capabilities

None. `openspec list --specs` currently returns only the `bulk-student-import` capability from another in-flight change; this change does not alter any specified behavior of existing capabilities — it adds test coverage and test-enabling seams.

## Impact

- New project `GestionEscolar.UI.Tests/` (xUnit, net10.0, references UI + Core + the fakes from `GestionEscolar.Tests`).
- `GestionEscolar.Data.Tests/` gains integration test classes; `SqlServerFixture`, `StudentSeeder`, and `sample.csv` are reused, not modified.
- `UI/ViewModels/` (`StudentsViewModel`, `StudentDetailViewModel`, `SectionScopedViewModel`): constructor-injected `IMainThreadDispatcher` and `INavigator` replace direct `MainThread`/`Shell.Current` calls; `UI/MauiProgram.cs` registers the production implementations. ViewModel public behavior and bindings are unchanged.
- `UI/UI.csproj` is referenced by the new test project; no TFM change to UI itself under the recommended strategy.
- `GestionEscolar.Tests/Fakes/` fakes are reused as-is; no new repository fakes expected.
- No production behavior changes; the risk is limited to the two seams, which are behavior-preserving indirections.
