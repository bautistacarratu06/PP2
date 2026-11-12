# Agent harness

This repository was made agent-ready with the [`setup-codebase-harness`](https://github.com/AI-Builder-Club/skills/tree/main/skills/setup-codebase-harness) skill, adapted to .NET 10 / MAUI. The goal is a short map (`AGENTS.md`), a `docs/` system of record, and mechanical checks for the golden rules.

## What is enforced

`GestionEscolar.Architecture.Tests` (xUnit + [ArchUnitNET](https://github.com/TNG/ArchUnitNET)) is registered in `GestionEscolar.slnx`. `scripts\run-tests.cmd` already runs every `*.Tests.csproj`, so these checks also run in the Husky pre-push hook.

| Rule | How it is checked |
|---|---|
| Core references no Data, UI, MAUI, EF, or SQL | ArchUnitNET type rules + assembly references + `.csproj` package/project references |
| Data project-references Core only, and does not reference UI/MAUI | ArchUnitNET + `.csproj` project/package references |
| No SQL/EF in Core or UI source | Source scan of production `.cs` files |
| ViewModels and pages do not inject `I*Repository` (or DbContext / `SqlConnection`) | Constructor scan under `UI/ViewModels` and `UI/Views` |
| No `ExecuteSqlRaw` / `FromSqlRaw` in Data (outside historical migrations) | Source scan |
| New `.cs` files are UTF-8 with BOM | Encoding check, with an allow-list of existing files that still lack a BOM |

Failure messages state the fix (for example: "Core isn't allowed to reference Data — keep domain logic in Core and put persistence in GestionEscolar.Data.").

Analyzer rules `CA1502` (cyclomatic complexity) and `CA1506` (class coupling) are enabled as **warnings** in the root [`.editorconfig`](../.editorconfig). They do not fail the build.

## Allow-list (do not "fix" these as a drive-by change)

A number of existing `.cs` files are UTF-8 without BOM. `EncodingTests` lists them explicitly so the suite stays green. Add new source files with a BOM; do not add new paths to the allow-list.

There is currently no ViewModel that injects a repository (the old `TrackingViewModel` / `IRiskSummaryRepository` case is gone). If a violation like that reappears, do not silently weaken the test: either fix the constructor or, if production code must stay as-is, add a documented skip/allow-list entry with a reason.

## Documented, not mechanically enforced

These remain golden rules in `AGENTS.md` but are not fully linted, because a reliable check would be noisy or would require production refactors:

- Enums over magic strings / `DbStatusValues` as the only status mapper
- UI text in Spanish
- DI lifetimes in `MauiProgram.cs` (repositories singleton; services, ViewModels, and pages transient; `SectionSelection` singleton)
- "Do not edit applied EF migrations or historical `0001`–`0010` scripts"
- Visual style tokens (colors, typography, `PageShell`)

`GestionEscolar.Tests` already checks Core DI lifetimes for use cases (`CoreRegistrationTests`). `GestionEscolar.Data.Tests` already asserts that the Core assembly does not reference Entity Framework (`CourseModelTests`).

## Skipped on purpose

The upstream skill also describes capabilities this repo does not need right now, or that would require external services or launching the desktop app:

- **`codebase-memory-mcp`** — queryable code graph. Skipped: it requires an external MCP server.
- **`dev-local-setup` / `scripts/dev-local.sh`** — the repo already has `scripts\run-desktop.cmd`, `scripts\run-database.cmd`, and `scripts\run-tests.cmd`. A new Unix-style stack script was not added.
- **`e2e-setup` / Playwright** and **`verifier-setup` (`/verify`)** — skipped: a trustworthy e2e gate would need to drive the MAUI desktop app, and agents must not launch `UI.exe` unless the user asks.
- **`crabbox-setup`** — skipped: cloud/parallel boxes are out of scope for this project.

Commit hygiene already exists: Husky.Net pre-commit (`dotnet format --verify-no-changes` on staged `.cs` files) and pre-push (UI build + all tests).
