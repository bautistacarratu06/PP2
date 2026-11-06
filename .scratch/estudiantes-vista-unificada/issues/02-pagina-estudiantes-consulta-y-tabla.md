# 02: Página Estudiantes con consulta unificada y tabla

## Parent

[Vista unificada de Estudiantes](../../ticket-estudiantes-vista-unificada.md)

## What to build

Entregar el camino de lectura end-to-end: una consulta unificada por comisión (sin N+1) que devuelva por estudiante datos de contacto, activo, estado de contacto, % asistencia, % entregas y riesgo vía `RiskRule` existente; y una página nueva **Estudiantes** con tabla de las 10 columnas del spec, shell común (título/subtítulo/Comisión), contenedor ensanchado (máx. ~1500–1600 px), buscador por nombre/legajo, contador “N estudiantes”, y en el encabezado las acciones **Cargar estudiante** y **Cargar comisión** (las de Nómina).

Registrar la página en DI/Shell y agregar entrada **Estudiantes** en el menú lateral (aún puede coexistir con las cuatro páginas viejas). Estilo: tema oscuro, tokens y controles existentes (`PageShell`, `Avatar`, `RiskPill`, `Meter`, etc.). Formato de nombres “Apellido, Nombre”; legajo/teléfono en mono.

## Acceptance criteria

- [ ] Una sola consulta/agregación por comisión alimenta la tabla (verificable con ~30 alumnos; sin N+1).
- [ ] La página Estudiantes muestra las 10 columnas en el orden del spec, con anchos explícitos alineados cabecera/filas.
- [ ] En ~1920 px de ancho no se truncan valores clave y los % de asistencia/entregas no se parten en dos líneas.
- [ ] Scroll vertical propio del cuerpo con cabecera fija; mínimo de tabla con scroll horizontal; Selección y Estudiante fijas al scrollear horizontal; prioridad de ocultar Correo luego Teléfono en ventanas chicas.
- [ ] Buscador por nombre/legajo y contador reflejan el conjunto visible.
- [ ] “Cargar estudiante” y “Cargar comisión” están en el encabezado de Estudiantes.
- [ ] El dropdown de Comisión recarga la tabla.
- [ ] Entrada de menú Estudiantes navega a la página nueva.
- [ ] Tests xUnit del caso de uso de consulta (al menos proyección, riesgo y exclusión de inactivos en agregados).
- [ ] Build sin warnings nuevos.

## Blocked by

- [01: Baja lógica de estudiantes](./01-baja-logica-estudiantes.md)
