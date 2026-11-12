@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion
rem Genera un CSV de alumnos ficticios en .local\ para probar la importacion de comisiones.
rem Los DNI y legajos arrancan despues del maximo numerico que ya hay en la base (sqlcmd),
rem asi el archivo no choca con alumnos guardados.
rem Variables opcionales: SQL_SERVER (por defecto .\SQLEXPRESS) y SQL_DATABASE (course_tracking).

if "%SQL_SERVER%"=="" set "SQL_SERVER=.\SQLEXPRESS"
if "%SQL_DATABASE%"=="" set "SQL_DATABASE=course_tracking"

set "COUNT=%~1"
if "%COUNT%"=="" set /p "COUNT=Cantidad de alumnos (1 a 1000000): "

set "NONDIGIT="
for /f "delims=0123456789" %%a in ("%COUNT%") do set "NONDIGIT=%%a"
if "%COUNT%"=="" goto invalid
if defined NONDIGIT goto invalid
if %COUNT% LSS 1 goto invalid
if %COUNT% GTR 1000000 goto invalid

where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo No se encontro sqlcmd en el PATH.
    exit /b 1
)

set "MAXDNI="
set "MAXLEG="
for /f "tokens=1,2 delims=," %%a in ('sqlcmd -S "%SQL_SERVER%" -d "%SQL_DATABASE%" -E -h -1 -W -s "," -Q "SET NOCOUNT ON; SELECT ISNULL((SELECT MAX(CAST(dni AS bigint)) FROM dbo.student WHERE dni NOT LIKE '%%[^0-9]%%'), 29999999), ISNULL((SELECT MAX(CAST(student_number AS bigint)) FROM dbo.student WHERE student_number NOT LIKE '%%[^0-9]%%'), 13000)"') do (
    set "MAXDNI=%%a"
    set "MAXLEG=%%b"
)
if not defined MAXDNI (
    echo No se pudo leer el maximo DNI de %SQL_DATABASE% en %SQL_SERVER%.
    exit /b 1
)

set /a FIRSTDNI=%MAXDNI%+1
set /a FIRSTLEG=%MAXLEG%+1

set "OUTDIR=%~dp0..\.local"
if not exist "%OUTDIR%" mkdir "%OUTDIR%"
set "OUTFILE=%OUTDIR%\alumnos_%COUNT%.csv"

echo Maximo DNI en la base: %MAXDNI%  ^|  primer DNI nuevo: %FIRSTDNI%  ^|  primer legajo nuevo: %FIRSTLEG%
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0generate-students-csv.ps1" -Count %COUNT% -FirstDni %FIRSTDNI% -FirstLegajo %FIRSTLEG% -OutFile "%OUTFILE%"
if errorlevel 1 (
    echo Fallo la generacion del CSV.
    exit /b 1
)
echo.
echo Archivo creado: %OUTFILE%
exit /b 0

:invalid
echo Ingresa un numero entero entre 1 y 1000000.
exit /b 1
