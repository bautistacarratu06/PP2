@echo off
setlocal

cd /d "%~dp0.."

echo.
echo ========================================
echo Running GestionEscolar tests
echo ========================================
echo.

dotnet test

if errorlevel 1 (
echo.
echo [ERROR] Some tests failed or could not be executed.
echo.
pause
exit /b 1
)

echo.
echo [SUCCESS] All tests passed.
echo.
pause
exit /b 0
dotnet test Domain.Tests                                           
dotnet test Domain.Tests --filter "FullyQualifiedName~CSVValidator"