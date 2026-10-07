@echo off
cd /d "%~dp0..\GestionEscolar"
taskkill /F /IM UI.exe >nul 2>&1
dotnet run --project UI -f net10.0-windows10.0.19041.0
if errorlevel 1 pause
