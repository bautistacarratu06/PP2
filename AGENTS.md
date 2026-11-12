# AGENTS.md

Map for coding agents in this repository. Human docs start at [`README.md`](./README.md). Architecture source of truth: [`GestionEscolar/ARCHITECTURE.md`](./GestionEscolar/ARCHITECTURE.md) (Spanish — read it before structural changes). Further detail: [`docs/index.md`](./docs/index.md).

Do not commit, stage, push, or rewrite git history unless the user explicitly asks. Do not reformat or refactor files outside your task.

## Overview

GestionEscolar is a .NET 10 MAUI desktop app (Windows target) for tracking course sections (*comisiones*): the student roster, attendance, assignment submissions, a class/assignment calendar, and risk tracking and interventions. Data lives in SQL Server Express (`.\SQLEXPRESS`, database `course_tracking`).

The git root is this folder. The solution is `GestionEscolar/GestionEscolar.slnx`.

## Project tree

```
PP2/
├─ docs/                         index plus build, conventions, and harness notes
├─ scripts/                      run-tests, run-database, run-desktop, hooks/pre-push.cmd
├─ .husky/                       Husky.Net hooks + task-runner.json
└─ GestionEscolar/
   ├─ GestionEscolar.slnx · ARCHITECTURE.md
   ├─ GestionEscolar.Core/       domain: Models/, Enums/, Interfaces/, Services/ (net10.0)
   ├─ GestionEscolar.Data/       EF Core SQL Server: Persistence/, Repositories/, Migrations/
   ├─ UI/                        MAUI Shell: Views/, ViewModels/, Controls/, MauiProgram.cs
   ├─ GestionEscolar.Tests/      xUnit over Core with in-memory Fakes/
   ├─ GestionEscolar.Data.Tests/ xUnit over EF repositories (SQL Server Express)
   └─ GestionEscolar.Architecture.Tests/  layer and convention checks (ArchUnitNET)
```

## Golden rules

These are hard invariants. Tests in `GestionEscolar.Architecture.Tests` enforce the ones marked **[tested]**; the others are documented and reviewed.

1. **[tested] Dependencies point to Core.** `UI → Data → Core`, `Tests → Core`, `Data.Tests → Data → Core`, `Architecture.Tests → Core and Data`. Core references no other solution project, no MAUI, no SQL, and no Entity Framework.
2. **[tested] Data project-references Core only.** SQL, EF, and connection code stay in `GestionEscolar.Data`.
3. **[tested] No SQL in ViewModels, Views, or Core.** ViewModels call Core services. Services depend on Core repository interfaces, which Data implements.
4. **[tested] ViewModels do not inject repositories** — only Core services and UI helpers such as `SectionSelection`.
5. **[tested] LINQ or parameterized SQL only.** Do not concatenate SQL, and do not use `ExecuteSqlRaw` or `FromSqlRaw`.
6. **Enums, not magic strings.** Map database status values (`presente`/`ausente`, `entregó`/`no entregó`) only in `DbStatusValues` (`ToDbValue` / `Parse*`).
7. **UI text is in Spanish** (for example "Comisión", "Registrar asistencia"). Code identifiers are in English; comments are mostly in Spanish. Follow the surrounding file.
8. **`.cs` files are UTF-8 with BOM.** Keep each file's existing line endings. Do not mass-reformat unrelated files. **[tested]** for new files; a documented allow-list covers existing files that still lack a BOM.
9. **Register types in `UI/MauiProgram.cs`:** repositories as singletons; services, ViewModels, and pages as transient. Exception: `SectionSelection` is a UI singleton for the commission shared by every screen. Shell resolves pages through DI.
10. **Schema changes** are new EF migrations under `GestionEscolar.Data/Migrations`, applied with `dotnet tool run dotnet-ef database update`. Never edit an applied EF migration or the historical `0001`–`0010` SQL scripts in `GestionEscolar.Data/Database/Migrations`.
11. **Tests.** New Core logic gets tests with fakes. Repository behavior is covered in `GestionEscolar.Data.Tests`. Do not bypass git hooks with `--no-verify` unless the user asks.

## Where to look

| Topic | Location |
|---|---|
| Domain model and hooks (human) | [`README.md`](./README.md) |
| Layers, flows, conventions (Spanish) | [`GestionEscolar/ARCHITECTURE.md`](./GestionEscolar/ARCHITECTURE.md) |
| Doc index | [`docs/index.md`](./docs/index.md) |
| Build, test, database, hooks | [`docs/build-and-test.md`](./docs/build-and-test.md) |
| Language, encoding, UI look | [`docs/conventions.md`](./docs/conventions.md) |
| Architecture tests and skipped harness pieces | [`docs/harness.md`](./docs/harness.md) |
| AI usage notes | [`AI_USAGE.md`](./AI_USAGE.md) |
| Composition root / DI | `GestionEscolar/UI/MauiProgram.cs` |
| Shell / sidebar routes | `GestionEscolar/UI/AppShell.xaml` |
| Current screens | `StudentsPage` (Estudiantes), `CalendarPage` (Calendario), `SettingsViewPage` (Ajustes); `StudentDetailPage` is a registered route |
| Status value mapping | `GestionEscolar.Core/Enums/DbStatusValues.cs` |
| Connection string | `GestionEscolar/UI/appsettings.json` (`ConnectionStrings:CourseTracking`) |

## Commands

Run from the repo root:

```bat
dotnet build GestionEscolar\UI\UI.csproj -f net10.0-windows10.0.19041.0
scripts\run-tests.cmd
dotnet format GestionEscolar\GestionEscolar.slnx --verify-no-changes
```

Close `UI.exe` before building; it locks the output DLLs. Alternatively, build elsewhere with `-p:BaseOutputPath=%TEMP%\uibuild\`. Schema and hook details: [`docs/build-and-test.md`](./docs/build-and-test.md).
