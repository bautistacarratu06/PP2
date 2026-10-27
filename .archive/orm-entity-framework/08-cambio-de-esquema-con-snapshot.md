# El próximo cambio de esquema sale de un snapshot de EF

## What to build

Con todas las pantallas ya leyendo y guardando por Entity Framework, el DDL nuevo sale de una migración generada desde el modelo. Los scripts SQL ya aplicados quedan como historia y no se editan. `run-database.cmd` deja de ser el camino para ese DDL.

El cambio de este corte es un índice de alumno por comisión, para la lectura de la nómina. No reescribe columnas, no borra filas y no cambia lo que muestran las pantallas.

Sobre `course_tracking` ya cargada, aplicar migraciones deja las comisiones y los alumnos intactos y solo agrega lo que todavía no estaba aplicado. En un servidor sin la base, el mismo update crea el esquema mapeado (tablas, claves, uniques, checks y defaults) y los dos umbrales. La nómina de prueba no se copia a ese camino: sigue viviendo en los scripts viejos.

Al terminar se puede verificar: la base existente conserva sus filas y gana el índice; una base creada de cero abre el selector de comisión; la nómina de una comisión vacía abre sin error.

## Acceptance criteria

- [ ] Hay una migración posterior al snapshot inicial. Su cambio de esquema es un índice de `student` por `section_id`.
- [ ] Esa migración se aplica con el actualizador de EF. `run-database.cmd` no es el camino de ese DDL.
- [ ] Los scripts SQL ya aplicados no se editan.
- [ ] En la base existente, comisiones y alumnos siguen con la misma cantidad de filas después del update.
- [ ] En un servidor sin la base, el update crea el esquema mapeado y los umbrales 50 y 25, sin la nómina de prueba.
- [ ] El selector de comisión abre contra esa base nueva. La nómina de una comisión sin alumnos queda vacía.
- [ ] Ningún repositorio de las pantallas sigue armando el SQL a mano.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [02 — Nómina de una comisión](02-nomina-de-una-comision.md)
- [03 — Calendario: listar y dar de alta clases y trabajos](03-calendario-clases-y-trabajos.md)
- [04 — Asistencia: listar y guardar](04-asistencia-listar-y-guardar.md)
- [05 — Entregas: listar y guardar](05-entregas-listar-y-guardar.md)
- [06 — Ficha del alumno](06-ficha-del-alumno.md)
- [07 — Seguimiento de la comisión](07-seguimiento-de-la-comision.md)
