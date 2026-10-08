```bat
@echo off
setlocal

REM SQL Server connection
set "SQL_SERVER=localhost"

REM Resolve paths relative to this script
set "PROJECT_ROOT=%~dp0.."
set "MIGRATIONS_DIR=%PROJECT_ROOT%\DATABASE\migrations"

echo ========================================
echo       GESTION ESCOLAR - MIGRATIONS
echo ========================================
echo.

REM Check that sqlcmd is installed
where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo ERROR: sqlcmd was not found.
    echo Install SQL Server command-line tools.
    exit /b 1
)

REM Check migrations directory
if not exist "%MIGRATIONS_DIR%\" (
    echo ERROR: Migrations directory not found:
    echo %MIGRATIONS_DIR%
    exit /b 1
)

REM Execute SQL files in alphabetical order
set "FOUND=0"

for /f "delims=" %%F in ('dir /b /a-d "%MIGRATIONS_DIR%\*.sql" 2^>nul ^| sort') do (
    set "FOUND=1"
    call :RunMigration "%MIGRATIONS_DIR%\%%F"
    if errorlevel 1 exit /b 1
)

if "%FOUND%"=="0" (
    echo No SQL migration files found.
    exit /b 0
)

echo.
echo All migrations completed successfully.
exit /b 0

:RunMigration
echo Running: %~nx1

sqlcmd -S "%SQL_SERVER%" -E -b -r 1 -i "%~1"

if errorlevel 1 (
    echo.
    echo ERROR: Migration failed: %~nx1
    exit /b 1
)

echo OK: %~nx1
echo.
exit /b 0
```
