# AGENTS.md

Instructions for coding agents working in this repository. Human docs: [`README.md`](./README.md) (domain model, hooks) and [`GestionEscolar/ARCHITECTURE.md`](./GestionEscolar/ARCHITECTURE.md) (layers, flows, conventions; read it before structural changes).

## Overview

GestionEscolar is a .NET 10 MAUI desktop app (Windows target) for tracking course sections (*comisiones*). It covers:

- the student roster;
- attendance;
- assignment submissions;
- a class/assignment calendar;
- risk tracking and interventions.

Data lives in SQL Server Express (`.\SQLEXPRESS`, database `course_tracking`).

## Layout

The git root is this folder. The solution is `GestionEscolar/GestionEscolar.slnx`.

| Path | Role |
|---|---|
| `GestionEscolar/GestionEscolar.Core` | Domain: models, enums, services, repository interfaces. `net10.0`, no MAUI, no SQL, no Entity Framework. |
| `GestionEscolar/GestionEscolar.Data` | SQL Server via Entity Framework (`Persistence/CourseTrackingDbContext`, `Repositories/Ef*Repository.cs`). |
| `GestionEscolar/GestionEscolar.Data/Migrations` | EF Core migrations. New schema changes go here. |
| `GestionEscolar/GestionEscolar.Data/Database/Migrations` | Historical `0001`–`0010` `*.up.sql` scripts. Do not edit them and do not add new ones. |
| `GestionEscolar/UI` | MAUI Shell app: `Views/` (XAML), `ViewModels/` (CommunityToolkit.Mvvm), `Controls/` (reusable controls), `Converters/`, `Resources/Styles/` (tokens and named styles), `MauiProgram.cs` (DI), `AppShell.xaml` (sidebar). |
| `GestionEscolar/GestionEscolar.Tests` | xUnit tests for Core, using in-memory fakes in `Fakes/`. |
| `GestionEscolar/GestionEscolar.Data.Tests` | xUnit tests for the EF repositories against SQL Server Express. |
| `scripts/` | `run-tests.cmd`, `run-database.cmd`, `run-desktop.cmd`, `hooks/pre-push.cmd`. |

## Architecture rules

- Dependencies point to Core: `UI → Data → Core`, `Tests → Core`, `Data.Tests → Data → Core`. Core references nothing else.
- No SQL in ViewModels or Views. ViewModels call Core services. Services depend on Core interfaces, which are implemented in Data.
- Register everything in `UI/MauiProgram.cs`:
  - repositories as singletons;
  - services, ViewModels and pages as transient. `SectionSelection` is the exception: a UI singleton for the commission shared by every screen.
  - Shell resolves pages through DI.
- Data access goes through Entity Framework in `GestionEscolar.Data`:
  - async all the way (`async`/`await` with `CancellationToken`);
  - one `CourseTrackingDbContext` per operation (`IDbContextFactory`), closed when the operation ends;
  - LINQ only. Do not concatenate SQL.
- Use enums, not magic strings. DB status values (`presente`/`ausente`, `entregó`/`no entregó`) are mapped only in `DbStatusValues` (`ToDbValue` / `Parse*`).
- ViewModels:
  - use `[ObservableProperty]` partial properties and `[RelayCommand]`;
  - section-scoped screens derive from `SectionScopedViewModel`.
- Keep formatting/display (dates, "Apellido, Nombre" labels) in UI or Data projections, not in Core models.
- Schema changes:
  - add an EF migration under `GestionEscolar.Data/Migrations` (`dotnet tool run dotnet-ef migrations add`);
  - apply it with `dotnet tool run dotnet-ef database update --project GestionEscolar\GestionEscolar.Data\GestionEscolar.Data.csproj`;
  - never edit an applied EF migration or the historical `0001`–`0010` SQL scripts.
- New Core logic gets tests with fakes. Repository behavior is covered in `GestionEscolar.Data.Tests`.

## Build and test

Run from the repo root:

```bat
dotnet build GestionEscolar\UI\UI.csproj -f net10.0-windows10.0.19041.0
scripts\run-tests.cmd
dotnet format GestionEscolar\GestionEscolar.slnx --verify-no-changes
```

- Close `UI.exe` before building; it locks the output DLLs. Alternatively, build elsewhere with `-p:BaseOutputPath=%TEMP%\uibuild\`.
- **New schema changes do not go through `scripts\run-database.cmd`.** That script only replays the historical `0001`–`0010` SQL scripts. It no longer drops anything: the `DROP DATABASE` / `DROP TABLE` statements in those scripts are commented out. On an empty server it creates and seeds `course_tracking`. On an existing database the table scripts are no-ops (`IF OBJECT_ID(...) IS NULL`), but the seed in `0010` fails on `uq_section_name` and the script stops with an error. Existing data is kept. Apply later schema changes with `dotnet ef database update`. To reset, drop the database manually first, and only if the user asks. Use `sqlcmd -S .\SQLEXPRESS -d course_tracking -E` for targeted checks.

## Git hooks (Husky.Net)

Hooks are installed automatically on restore/build (target in `GestionEscolar.Core.csproj`; skipped with `HUSKY=0`). They are configured in `.husky/task-runner.json`.

- **pre-commit:** `dotnet format --verify-no-changes` on staged `.cs` files.
- **pre-push:** `scripts\hooks\pre-push.cmd`, which runs the UI build plus the tests and refuses to run while `UI.exe` is open.

Do not bypass the hooks with `--no-verify` unless the user asks.

## Conventions

- UI text is in Spanish (e.g. "Comisión", "Registrar asistencia"). Code identifiers are in English; comments are mostly in Spanish. Follow the surrounding file.
- `.cs` files: UTF-8 with BOM. Keep each file's existing line endings.
- UI look, for every new screen or control:
  - One dark theme. Colors come from `UI/Resources/Styles/Colors.xaml` (`Bg`, `Side`, `Surface`, `Raised`, `Line`, `TextPrimary`, `TextSecondary`, `TextTertiary`, `Accent`, `Danger`, `Warning`, `Success`). Do not hardcode hex in new pages, and do not bring back navy or teal.
  - Type: Inter (`Inter`, `InterMedium`, `InterSemibold`, `InterBold`) and JetBrains Mono (`JetBrainsMono`) for student numbers, phones, and dates.
  - Reuse `UI/Controls` (`PageShell`, `SidebarView`, `SectionPicker`, `SearchBox`, `Avatar`, `Icon`, `Meter`, `RiskPill`, `StatusPill`, `SegmentedControl`, `KpiCard`, `FilterChips`, `HoverRow`) and the named styles in `UI/Resources/Styles/Styles.xaml` (`Card`, `FieldBorder`, `PrimaryButton`, `GhostButton`, `LinkButton`, `PageTitle`, `PageSubtitle`, `ColumnHeader`, `Mono`). New chrome goes through `PageShell`, not a private title.
  - Cards are a `Border` with a 14px radius and a 1px stroke. No `Frame`, no drop shadow.
  - List names are "Apellido, Nombre" via `UI/Formatting/PersonName`. The detail subtitle is "Nombre Apellido · legajo".
  - The chosen commission is the `UI.Services.SectionSelection` singleton. Screens subscribe to it; they do not hardcode a section id.
  - Visual reference: `.scratch/look-and-feel/` (`Gestión Escolar · Mockup.html`, `ui-revamp.md`, `component-guide.md`, and `issues/`).

## Git rules

- Do not commit, stage, push or rewrite history unless the user explicitly asks.
- Respect teammates' code. Do not reformat or refactor files outside your task.
- Run `git status` / `git pull` before starting, and leave unrelated untracked files alone.
