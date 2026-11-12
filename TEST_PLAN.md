# Test Plan — GestionEscolar UI (Integration + Acceptance)

**Date:** 2026-11-11
**Scope:** Two test suites for the MAUI UI layer:

1. **Integration tests** — real EF Core repositories (`GestionEscolar.Data`) against a real SQL Server Express database, exercising Core services end-to-end (same pattern as `GestionEscolar.Data.Tests/SqlServerFixture.cs`).
2. **Acceptance tests** — exercise the **ViewModel classes directly** (as the UI would), with the **DB mocked** via the existing repository fakes in `GestionEscolar.Tests/Fakes/`.

---

## 1. Current state (what we can reuse)

| Asset | Location | Reuse |
|---|---|---|
| Repository fakes (all 10 repos) | `GestionEscolar.Tests/Fakes/` | ✅ Direct reuse for acceptance tests |
| Core services (SUT collaborators) | `GestionEscolar.Core/Services/` | ✅ Real services + fake repos = acceptance layer |
| SQL Server fixture (`IAsyncLifetime`, migrate + drop) | `GestionEscolar.Data.Tests/SqlServerFixture.cs` | ✅ Template for integration tests |
| xUnit + coverlet test projects | `GestionEscolar.Tests`, `GestionEscolar.Data.Tests` | ✅ Same stack (net10.0, xunit 2.9) |

### ⚠️ Platform blockers in ViewModels (affects acceptance effort)

| ViewModel | Blocker | Workaround |
|---|---|---|
| `StudentsViewModel` | `MainThread.BeginInvokeOnMainThread` in ctor; `Shell.Current.GoToAsync` (open detail) | Extract navigation behind an interface; guard `MainThread` or dispatch via `Func<Action, Task>` seam |
| `StudentDetailViewModel` | `Shell.Current.GoToAsync` (Back command) | Same navigation seam |
| `SectionScopedViewModel` (base of `CalendarViewModel`, `SettingsViewModel`-adjacent) | `MainThread.BeginInvokeOnMainThread` on section change | Small guard/seam; only fires on `SectionSelection.Changed` |
| `SettingsViewModel`, `CommissionImportViewModel` | None (pure MVVM) | None needed |

### ⚠️ TFM friction

`UI.csproj` multi-targets `net10.0-android/-ios/-maccatalyst/-windows`. A plain `net10.0` test project **cannot reference it directly**. Options (pick one):

- **A (recommended):** multi-target the new test project: `net10.0` + `net10.0-windows10.0.19041.0` (run on Windows) — or compile the ViewModels into a shared/net10.0 library.
- **B:** extract `ViewModels/` + `Services/` + `Formatting/` into `GestionEscolar.UI.Core` (net10.0) referenced by both UI and tests. Cleaner long-term, more upfront effort.

---

## 2. Candidates ranked by effort (easy first)

Effort = implementation + blockers + fixtures, per feature, for **both tests** (1 integration + 1 acceptance).

### 🥇 1. Settings (risk thresholds) — **~0.5 day**

| Test | What it does | Effort |
|---|---|---|
| **Integration** | `EfRiskSettingsRepository` reads/writes `dbo.setting`; `RiskSettingsService.LoadAsync/SaveAsync` round-trip; thresholds persist after re-read | Low — single table, fixture already exists |
| **Acceptance** | `SettingsViewModel` directly: `LoadCommand` populates `HighThreshold`/`MediumThreshold` from (fake) repo; `SaveThresholdsAsync` persists and sets `StatusMessage`/`StatusIsError`; invalid input shows error | **Lowest** — zero platform dependencies, only 2 ctor deps (`RiskSettingsService`, `RiskConfig`), `FakeRiskSettingsRepository` already exists |

**Why first:** no `MainThread`, no `Shell`, no files, no dates. Pure load → edit → save → status flow.

### 🥈 2. Calendar (classes & assignments) — **~1 day**

| Test | What it does | Effort |
|---|---|---|
| **Integration** | `EfCalendarRepository`: add `ClassSession` (date-identified) and `Assignment` (named), `GetItemsAsync` returns combined ordered list, validation rules (duplicate assignment name, past date) | Low-medium — a few rules, but fixture + seeder pattern exists |
| **Acceptance** | `CalendarViewModel` directly: select section → `Items` loads; switch `SelectedType` (Clase/Trabajo) → placeholder & name reset; `AddAsync` creates item and refreshes list; empty state `HasNoItems` | Low-medium — needs `SectionSelection` + `FakeSectionRepository` + `FakeCalendarRepository`; one `MainThread` guard in `SectionScopedViewModel` |

**Why second:** real CRUD flow with validation errors surfaced in `ErrorMessage`, but still a single small VM with one service dependency.

### 🥉 3. Student detail (seguimiento) — **~1.5 days**

| Test | What it does | Effort |
|---|---|---|
| **Integration** | `EfStudentDetailsRepository`: fetch student header + attendance/submission/intervention histories; register a contact (intervention) and see it appear; risk badge data consistency | Medium — 3 history sources + contact write |
| **Acceptance** | `StudentDetailViewModel` directly: `ApplyQueryAttributes` (Student/SectionId keys) loads name, phone, email, risk badge colors, `Attended/ClassTotal`; open contact form → save → `Interventions` grows, `HasContactError` on invalid; `HasNo*` empty states | Medium — needs `FakeStudentDetailsRepository`; **`Shell.Current.GoToAsync` in `BackAsync` must be neutralized** (navigation seam or null-guard) |

**Why third:** rich scenario (query-attribute navigation, 3 collections, contact modal) but a single service dependency.

### 4. Students list (grid, filters, bulk actions) — **~2 days**

| Test | What it does | Effort |
|---|---|---|
| **Integration** | `EfTrackingRepository` + `EfStudentRepository`: seed students with attendance/submissions → `StudentsQueryService` returns `StudentsQueryResult` with risk summary; filters (search, risk, contact, attendance/delivery ranges, active status) and sorting | Medium-high — the widest query surface in the app |
| **Acceptance** | `StudentsViewModel` directly: load rows + KPI counts; text/risk/contact filters narrow `FilteredStudents`; select rows → `SelectedCount`/`SelectedCountText`; bulk attendance mark & submission mark; `IsAllSelected` toggle | High — 5 services + `CommissionImportViewModel` child VM in ctor; `MainThread` in ctor; `Shell.GoToAsync` on row open; selection re-entrancy (`_isUpdatingSelection`) |

**Why fourth:** most dependencies and most UI-mirroring logic; needs the navigation + dispatch seams first.

### 5. Commission import (CSV wizard) — **~2.5 days**

| Test | What it does | Effort |
|---|---|---|
| **Integration** | `EfCommissionImportRepository` + `CommissionImportService`: parse CSV → preview → insert into existing section / create new; conflict detection (DNI/legajo duplicates) → merge vs omit decisions; blocked rows | High — multi-step transactional flow, most DB rules |
| **Acceptance** | `CommissionImportViewModel` directly: 3-step wizard (`IsFileStep` → `IsDestinationStep` → `IsPreviewStep`); `AcceptFileAsync(path)` with a temp CSV; `ConflictChoice.Merge` toggles → `Decision`; confirm inserts and closes | High — file system dependency (needs temp CSV fixtures), step machine, plus it is a **child VM of `StudentsViewModel`** (test standalone first) |

**Why last:** file I/O + 3-step wizard + conflict resolution = most moving parts.

---

## 3. Suggested execution order

1. **Setup (shared):** decide TFM strategy (option A or B above); create `GestionEscolar.UI.Tests` (acceptance, xUnit, references UI + Core + fakes) and add integration tests to `GestionEscolar.Data.Tests`. Introduce the two small seams (`IMainThreadDispatcher`, `INavigator`) if going for features 3–5.
2. **Settings** → 3. **Calendar** → 4. **Student detail** → 5. **Students list** → 6. **Commission import** (each = 1 integration + 1 acceptance test, ranked above).

## 4. Test data strategy

- **Integration:** reuse `SqlServerFixture` (drop → migrate → `ClearRowsAsync` per test) + `StudentSeeder`/`sample.csv` for realistic rosters.
- **Acceptance:** in-memory fakes seeded per test with 3–5 students across risk levels (high/medium/low), one section, one class session + one assignment; CSV strings written to a temp file for the import test.

## 5. Definition of done

- [ ] Both suites green via existing `scripts/run-tests` (or `dotnet test`).
- [ ] Acceptance tests touch **no** MAUI UI controls, no real DB, no network.
- [ ] Integration tests run against a migrated SQL Server Express DB and clean up after themselves.
- [ ] Coverage report shows the exercised VMs/services (coverlet already configured).
