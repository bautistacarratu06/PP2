# Entity Framework en la capa de datos

Tareas locales para que Data hable con SQL Server a través de Entity Framework. Las pantallas, los servicios y los tests con repositorios falsos siguen igual. Core no referencia el ORM.

El primer corte fotografía el esquema que ya está en `course_tracking` y deja esa migración como aplicada, para que un update no vuelva a crear las tablas. Los cortes siguientes solo cambian por dónde pasa cada consulta o guardado. El último corte es el que aplica DDL nuevo.

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md) — sin bloqueo
- [02 — Nómina de una comisión](02-nomina-de-una-comision.md) — bloqueada por 01
- [03 — Calendario: listar y dar de alta clases y trabajos](03-calendario-clases-y-trabajos.md) — bloqueada por 01
- [04 — Asistencia: listar y guardar](04-asistencia-listar-y-guardar.md) — bloqueada por 01
- [05 — Entregas: listar y guardar](05-entregas-listar-y-guardar.md) — bloqueada por 01
- [06 — Ficha del alumno](06-ficha-del-alumno.md) — bloqueada por 01
- [07 — Seguimiento de la comisión](07-seguimiento-de-la-comision.md) — bloqueada por 01
- [08 — El próximo cambio de esquema sale de un snapshot de EF](08-cambio-de-esquema-con-snapshot.md) — bloqueada por 02, 03, 04, 05, 06 y 07
