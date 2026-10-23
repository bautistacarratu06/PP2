@echo off
rem Hook pre-push (Husky.Net, .husky\task-runner.json): compila la UI y corre los tests.
rem Saltear en una emergencia: git push --no-verify
setlocal
cd /d "%~dp0..\.."

tasklist /FI "IMAGENAME eq UI.exe" 2>nul | findstr /i "UI.exe" >nul
if not errorlevel 1 (
    echo.
    echo [pre-push] UI.exe esta abierto y bloquea las DLL del build.
    echo [pre-push] Cerra la app y volve a hacer push ^(o usa git push --no-verify^).
    exit /b 1
)

echo [pre-push] 1/2 dotnet build UI (net10.0-windows10.0.19041.0)
dotnet build GestionEscolar\UI\UI.csproj -f net10.0-windows10.0.19041.0 -v q -nologo
if errorlevel 1 (
    echo [pre-push] El build fallo. Push cancelado.
    exit /b 1
)

echo [pre-push] 2/2 tests
call scripts\run-tests.cmd < nul
if errorlevel 1 (
    echo [pre-push] Fallaron los tests. Push cancelado.
    exit /b 1
)
exit /b 0