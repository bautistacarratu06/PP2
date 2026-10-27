# Reportar la importación y resolver cada conflicto

## What to build

El mismo asistente permite elegir una comisión que ya existe. La vista previa separa las filas nuevas de los conflictos y dice cuántas se van a insertar. Una fila es conflicto cuando se cumple alguno de estos criterios:

- su DNI ya está guardado, en esta comisión o en otra, porque el DNI identifica al alumno en toda la base;
- su legajo ya está en la comisión elegida;
- el archivo repite ese DNI o ese legajo en otra fila.

La vista previa nombra el criterio (DNI, legajo o los dos) y si el choque está en la base o en el archivo.

Cada conflicto se resuelve por su cuenta, omitir o fusionar, antes de confirmar. Al confirmar se aplican esas decisiones juntas.

Omitir no escribe esa fila. Los alumnos ya guardados quedan como estaban. Un DNI de otra comisión tampoco se agrega a esta.

Fusionar solo cuando el DNI y el legajo señalan al mismo alumno ya guardado. Fusionar copia a ese alumno el teléfono, el correo, el apellido y el nombre del CSV. El DNI y el legajo no cambian.

Si el DNI y el legajo no coinciden en el mismo alumno (falta una de las dos claves, o cada una apunta a un alumno distinto), esa fila solo se puede omitir. Si el duplicado existe solo dentro del archivo y todavía no está guardado, la primera fila puede insertarse y las copias siguientes solo se omiten: no hay un alumno guardado para fusionar.

Al terminar se puede verificar: sobre una comisión con alumnos, un CSV mezcla filas nuevas, un alumno ya cargado (mismo DNI y mismo legajo, otro teléfono), un DNI de otra comisión, un legajo ya usado que no coincide con ese DNI, y una fila repetida en el archivo. Al confirmar, la nómina suma las filas nuevas, el alumno fusionado muestra el teléfono nuevo, y el resto no se escribe.

## Acceptance criteria

- [ ] Elegir una comisión existente muestra cuántas filas se insertarían y lista los conflictos.
- [ ] Una fila es conflicto si su DNI ya está guardado, si su legajo ya está en esa comisión, o si el archivo repite ese DNI o ese legajo.
- [ ] Cada conflicto indica el criterio (DNI, legajo o ambos) y si el choque es contra la base o contra otra fila del archivo.
- [ ] Omitir y fusionar conviven en el mismo paso, y cada conflicto se decide antes de confirmar.
- [ ] Al omitir, esa fila no se escribe y los alumnos ya guardados no cambian. Un DNI que ya pertenece a otra comisión no se inserta en esta.
- [ ] Si el DNI y el legajo señalan al mismo alumno guardado, fusionar actualiza teléfono, correo, apellido y nombre, y deja el DNI y el legajo iguales.
- [ ] Si las dos claves no señalan al mismo alumno, la fila solo se puede omitir y no se escribe.
- [ ] Una repetición que solo está en el archivo deja una fila para insertar y el resto solo se puede omitir.
- [ ] Esos casos quedan cubiertos por tests con un repositorio falso.

## Blocked by

- [01 — Cargar una comisión nueva desde un CSV](01-cargar-comision-nueva-desde-csv.md)
