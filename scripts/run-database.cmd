@echo off
setlocal
chcp 65001 >nul

set "SERVER=.\SQLEXPRESS"
set "DB=course_tracking"
set "DIR=%~dp0..\GestionEscolar\GestionEscolar.Data\Database\Migrations"

if not exist "%DIR%" (
    echo.
    echo ERROR: No se encuentra la carpeta de migraciones: "%DIR%"
    goto :error
)

for %%F in ("%DIR%\*.up.sql") do (
    echo Ejecutando %%~nxF ...
    sqlcmd -S "%SERVER%" -E -C -b -f 65001 -i "%%F"
    if errorlevel 1 (
        echo.
        echo ERROR en %%~nxF. Se detiene la ejecucion.
        goto :error
    )
)

echo.
echo Todas las migraciones se aplicaron correctamente.
echo.
echo ========================================
echo  Estructura de Tablas en [%DB%]
echo ========================================
echo.
echo %DB%

sqlcmd -S "%SERVER%" -d "%DB%" -E -C -h -1 -W -Q "SET NOCOUNT ON; SELECT '+-- ' + TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_SCHEMA, TABLE_NAME;"

echo.
goto :fin

:error
echo.
echo [Proceso interrumpido con errores]

:fin
endlocal
pause