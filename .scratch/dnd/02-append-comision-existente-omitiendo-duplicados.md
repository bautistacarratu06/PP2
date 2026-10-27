# Agregar alumnos a una comisión existente, omitiendo duplicados

## What to build

El mismo asistente permite elegir una comisión que ya existe. La vista previa separa las filas nuevas de los conflictos y dice cuántas se van a insertar. Una fila es conflicto cuando se cumple alguno de estos criterios:

- su DNI ya está guardado, en esta comisión o en otra, porque el DNI identifica al alumno en toda la base;
- su legajo ya está en la comisión elegida;
- el archivo repite ese DNI o ese legajo en otra fila.

La vista previa nombra el criterio (DNI, legajo o los dos) y si el choque está en la base o en el archivo.

Omitir inserta solo las filas nuevas. Las filas en conflicto no se escriben y los alumnos ya guardados quedan como estaban. Un DNI de otra comisión tampoco se agrega a esta.

Al terminar se puede verificar: sobre una comisión con alumnos, un CSV mezcla filas nuevas, un DNI ya guardado, un legajo ya usado en esa comisión y una fila repetida en el archivo. Al omitir, la nómina suma solo las filas nuevas.

## Acceptance criteria

- [ ] Elegir una comisión existente muestra cuántas filas se insertarían y lista los conflictos.
- [ ] Una fila es conflicto si su DNI ya está guardado, si su legajo ya está en esa comisión, o si el archivo repite ese DNI o ese legajo.
- [ ] Cada conflicto indica el criterio (DNI, legajo o ambos) y si el choque es contra la base o contra otra fila del archivo.
- [ ] Al omitir, se insertan solo las filas nuevas. Los alumnos ya guardados no cambian.
- [ ] Un DNI que ya pertenece a otra comisión no se inserta en esta.
- [ ] Esos casos quedan cubiertos por tests con un repositorio falso.
- [ ] Este corte no fusiona datos ni agrega el gesto de arrastrar.

## Blocked by

- [01 — Cargar una comisión nueva desde un CSV](01-cargar-comision-nueva-desde-csv.md)
