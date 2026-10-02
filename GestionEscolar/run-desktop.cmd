@echo off
cd /d "%~dp0"
dotnet run --project UI -f net10.0-windows10.0.19041.0
if errorlevel 1 pause
