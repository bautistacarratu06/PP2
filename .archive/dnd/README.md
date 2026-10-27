# Carga de una comisión desde un CSV

Dos cortes. El primero carga una comisión nueva: el archivo entra con un botón o soltándolo. El segundo reporta la importación sobre una comisión que ya existe y deja resolver cada conflicto, omitiendo o fusionando.

Un alumno entra en conflicto si su DNI ya está guardado o si su legajo ya está en la comisión. El DNI identifica al alumno en toda la base. El legajo se repite solo dentro de la comisión.

- [01 — Cargar una comisión nueva desde un CSV](01-cargar-comision-nueva-desde-csv.md) — sin bloqueo
- [02 — Reportar la importación y resolver cada conflicto](02-reportar-importacion-y-resolver-conflictos.md) — bloqueada por 01
