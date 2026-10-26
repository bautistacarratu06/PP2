# Connection string fuera de la constante de arranque

## What to build

La cadena deja de ser una constante en el arranque y se lee de la configuración de la aplicación. El valor de desarrollo sigue siendo el servidor local, la base `course_tracking` y la conexión de confianza actuales. `DatabaseOptions` sigue siendo el único objeto que la transporta hacia la infraestructura. Si la cadena viene vacía, la app falla al construir la conexión, igual que ahora.

Hoy la cadena está fija en el arranque de la app y entra por `DatabaseOptions`. El comentario del propio arranque ya indica configurarla por entorno. No va a `dbo.setting`: esa tabla guarda enteros de 0 a 100.

Al terminar se puede verificar: la app abre y Seguimiento, Nómina o Asistencia cargan datos de `course_tracking` en el SQL Server local.

## Acceptance criteria

- [ ] Con la configuración de desarrollo, la app conecta a la misma base que hoy.
- [ ] La cadena no queda escrita como constante en el registro de dependencias.
- [ ] Una cadena vacía sigue impidiendo abrir la conexión.

## Blocked by

- None - can start immediately
