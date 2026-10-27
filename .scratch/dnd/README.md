# Carga de una comisión desde un CSV

Tareas locales del asistente de carga. El archivo se elige con un botón. Soltar el CSV queda para el final.

Un alumno entra en conflicto si su DNI ya está guardado o si su legajo ya está en la comisión. El DNI identifica al alumno en toda la base. El legajo se repite solo dentro de la comisión.

- [01 — Cargar una comisión nueva desde un CSV](01-cargar-comision-nueva-desde-csv.md) — sin bloqueo
- [02 — Agregar alumnos a una comisión existente, omitiendo duplicados](02-append-comision-existente-omitiendo-duplicados.md) — bloqueada por 01
- [03 — Fusionar los duplicados en el mismo paso](03-fusionar-duplicados-al-cargar-csv.md) — bloqueada por 02
- [04 — Soltar el CSV sobre el asistente](04-soltar-csv-sobre-el-asistente.md) — bloqueada por 01
