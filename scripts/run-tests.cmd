@echo off
setlocal EnableDelayedExpansion

cd /d "%~dp0..\GestionEscolar"

echo.
echo ========================================
echo Running GestionEscolar tests
echo ========================================
echo.

set FAILED=0
set FOUND=0

rem Corre todos los proyectos de test (*.Tests.csproj) que haya en la solucion.
for /r %%P in (*.Tests.csproj) do (
    echo %%P | findstr /i "\\bin\\ \\obj\\" >nul
    if errorlevel 1 (
        set FOUND=1
        echo ---- %%~nP
        dotnet test "%%P" %*
        if errorlevel 1 set FAILED=1
        echo.
    )
)

if "%FOUND%"=="0" (
    echo [ERROR] No test projects ^(*.Tests.csproj^) were found.
    pause
    exit /b 1
)

if "%FAILED%"=="1" (
    echo [ERROR] Some tests failed or could not be executed.
    echo.
    pause
    exit /b 1
)

echo [SUCCESS] All tests passed.
echo.
pause
exit /b 0