# Build, test, and database

Run these from the repository root (`PP2/`).

## Build and test

```bat
dotnet build GestionEscolar\UI\UI.csproj -f net10.0-windows10.0.19041.0
scripts\run-tests.cmd
dotnet format GestionEscolar\GestionEscolar.slnx --verify-no-changes
```

- Close `UI.exe` before building; it locks the output DLLs. Alternatively, build elsewhere with `-p:BaseOutputPath=%TEMP%\uibuild\`.
- `scripts\run-tests.cmd` runs `dotnet test` on every `*.Tests.csproj` under `GestionEscolar/` (including `GestionEscolar.Tests`, `GestionEscolar.Data.Tests`, and `GestionEscolar.Architecture.Tests`). It forwards extra arguments to `dotnet test`.
- The Husky **pre-push** hook (`scripts\hooks\pre-push.cmd`) runs the UI build and then `scripts\run-tests.cmd`. If `UI.exe` is open, the hook stops and asks you to close the app.

## Database and schema

**New schema changes do not go through `scripts\run-database.cmd`.** That script only replays the historical `0001`–`0010` SQL scripts under `GestionEscolar.Data/Database/Migrations`. It no longer drops anything: the `DROP DATABASE` / `DROP TABLE` statements in those scripts are commented out.

- On an empty server it creates and seeds `course_tracking`.
- On an existing database the table scripts are no-ops (`IF OBJECT_ID(...) IS NULL`), but the seed in `0010` fails on `uq_section_name` and the script stops with an error. Existing data is kept.

Apply later schema changes with:

```bat
dotnet tool run dotnet-ef migrations add <Name> --project GestionEscolar\GestionEscolar.Data\GestionEscolar.Data.csproj
dotnet tool run dotnet-ef database update --project GestionEscolar\GestionEscolar.Data\GestionEscolar.Data.csproj
```

Never edit an applied EF migration or the historical `0001`–`0010` SQL scripts. To reset, drop the database manually first, and only if the user asks. Use `sqlcmd -S .\SQLEXPRESS -d course_tracking -E` for targeted checks.

`GestionEscolar.Data.Tests` uses throwaway databases (`course_tracking_ef_test` and `course_tracking_ef_legacy`) and does not touch `course_tracking`.

## Scripts (`scripts/`)

| Script | What it does |
|---|---|
| `run-tests.cmd` | Runs `dotnet test` on every `*.Tests.csproj` in the solution. |
| `run-database.cmd` | Replays historical `0001`–`0010` SQL scripts. Does not apply new EF DDL. |
| `run-desktop.cmd` | Closes `UI.exe` if it is open and runs the app (`dotnet run`, Windows target). Do not start this from an agent unless the user asks. |
| `hooks/pre-push.cmd` | Used by the pre-push hook: refuses to run while `UI.exe` is open, then builds the UI and runs the tests. |

## Git hooks (Husky.Net)

Hooks are installed automatically on restore/build (target in `GestionEscolar.Core.csproj`; skipped with `HUSKY=0` or when there is no `.git` directory). They are configured in `.husky/task-runner.json`.

- **pre-commit:** `dotnet format --verify-no-changes` on staged `.cs` files.
- **pre-push:** `scripts\hooks\pre-push.cmd`.

Do not bypass the hooks with `--no-verify` unless the user asks. More detail: [`README.md`](../README.md).
