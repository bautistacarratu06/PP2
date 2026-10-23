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
| `GestionEscolar/GestionEscolar.Core` | Domain: models, enums, services, repository interfaces. `net10.0`, no MAUI, no SQL. |
| `GestionEscolar/GestionEscolar.Data` | SQL Server implementations of the Core interfaces (`Repositories/Sql*Repository.cs`, `DatabaseInstance`). |
| `GestionEscolar/GestionEscolar.Data/Database/Migrations` | Numbered `NNNN_*.up.sql` scripts. |
| `GestionEscolar/UI` | MAUI Shell app: `Views/` (XAML), `ViewModels/` (CommunityToolkit.Mvvm), `Converters/`, `MauiProgram.cs` (DI), `AppShell.xaml` (flyout). |
| `GestionEscolar/GestionEscolar.Tests` | xUnit tests for Core, using in-memory fakes in `Fakes/`. |
| `scripts/` | `run-tests.cmd`, `run-database.cmd`, `run-desktop.cmd`, `hooks/pre-push.cmd`. |

## Architecture rules

- Dependencies point to Core: `UI → Data → Core`, `Tests → Core`. Core references nothing else.
- No SQL in ViewModels or Views. ViewModels call Core services. Services depend on Core interfaces, which are implemented in Data.
- Register everything in `UI/MauiProgram.cs`:
  - repositories as singletons;
  - services, ViewModels and pages as transient.
  - Shell resolves pages through DI.
- Data access:
  - async all the way (`async`/`await` with `CancellationToken`);
  - parameterized `SqlCommand` only, never string-concatenated SQL.
- Use enums, not magic strings. DB status values (`presente`/`ausente`, `entregó`/`no entregó`) are mapped only in `DbStatusValues` (`ToDbValue` / `Parse*`).
- ViewModels:
  - use `[ObservableProperty]` partial properties and `[RelayCommand]`;
  - section-scoped screens derive from `SectionScopedViewModel`.
- Keep formatting/display (dates, "Apellido, Nombre" labels) in UI or SQL projections, not in Core models.
- Schema changes:
  - add a new numbered migration;
  - apply it with `sqlcmd`;
  - never edit applied migrations.
- New Core logic gets tests with fakes.

## Build and test

Run from the repo root:

```bat
dotnet build GestionEscolar\UI\UI.csproj -f net10.0-windows10.0.19041.0
scripts\run-tests.cmd
dotnet format GestionEscolar\GestionEscolar.slnx --verify-no-changes
```

- Close `UI.exe` before building; it locks the output DLLs. Alternatively, build elsewhere with `-p:BaseOutputPath=%TEMP%\uibuild\`.
- **`scripts\run-database.cmd` drops and recreates the database.** Do not run it on a database with data you need. Use `sqlcmd -S .\SQLEXPRESS -d course_tracking -E` for targeted changes and checks.

## Git hooks (Husky.Net)

Hooks are installed automatically on restore/build (target in `GestionEscolar.Core.csproj`; skipped with `HUSKY=0`). They are configured in `.husky/task-runner.json`.

- **pre-commit:** `dotnet format --verify-no-changes` on staged `.cs` files.
- **pre-push:** `scripts\hooks\pre-push.cmd`, which runs the UI build plus the tests and refuses to run while `UI.exe` is open.

Do not bypass the hooks with `--no-verify` unless the user asks.

## Conventions

- UI text is in Spanish (e.g. "Comisión", "Registrar asistencia"). Code identifiers are in English; comments are mostly in Spanish. Follow the surrounding file.
- `.cs` files: UTF-8 with BOM. Keep each file's existing line endings.
- XAML styling:
  - `AppThemeBinding` for light/dark;
  - teal accent (`#0F766E` / `#5EEAD4`);
  - rounded `Border` cards.
- Student names display as "Apellido, Nombre".

## Git rules

- Do not commit, stage, push or rewrite history unless the user explicitly asks.
- Respect teammates' code. Do not reformat or refactor files outside your task.
- Run `git status` / `git pull` before starting, and leave unrelated untracked files alone.
