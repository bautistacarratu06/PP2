@echo off
cd /d "%~dp0..\GestionEscolar"
taskkill /F /IM UI.exe >nul 2>&1
dotnet run --project UI -f net10.0-windows10.0.19041.0
set EXITCODE=%ERRORLEVEL%
if not "%EXITCODE%"=="0" (
    echo.
    echo La app termino con el codigo %EXITCODE%.
    pause
)
