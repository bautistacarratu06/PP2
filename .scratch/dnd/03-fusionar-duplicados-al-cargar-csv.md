# Fusionar los duplicados en el mismo paso

## What to build

En el paso de conflictos, además de omitir, se puede fusionar cuando el DNI y el legajo señalan al mismo alumno ya guardado. Fusionar copia a ese alumno el teléfono, el correo, el apellido y el nombre del CSV. El DNI y el legajo no cambian.

Si el DNI y el legajo no coinciden en el mismo alumno (falta una de las dos claves, o cada una apunta a un alumno distinto), esa fila solo se puede omitir. Si el duplicado existe solo dentro del archivo y todavía no está guardado, la primera fila puede insertarse y las copias siguientes solo se omiten: no hay un alumno guardado para fusionar.

Cada conflicto se resuelve por su cuenta, omitir o fusionar, antes de confirmar. Al confirmar se aplican esas decisiones juntas.

Al terminar se puede verificar: un CSV con un alumno ya cargado (mismo DNI y mismo legajo, otro teléfono) se fusiona y la nómina muestra el teléfono nuevo. Una fila cuyo DNI y cuyo legajo apuntan a alumnos distintos solo se puede omitir.

## Acceptance criteria

- [ ] Si el DNI y el legajo señalan al mismo alumno guardado, fusionar actualiza teléfono, correo, apellido y nombre, y deja el DNI y el legajo iguales.
- [ ] Si las dos claves no señalan al mismo alumno, la fila solo se puede omitir y no se escribe.
- [ ] Una repetición que solo está en el archivo deja una fila para insertar y el resto solo se puede omitir.
- [ ] Omitir y fusionar conviven en el mismo paso, y cada conflicto se decide antes de confirmar.
- [ ] Esos casos quedan cubiertos por tests con un repositorio falso.

## Blocked by

- [02 — Agregar alumnos a una comisión existente, omitiendo duplicados](02-append-comision-existente-omitiendo-duplicados.md)
