# Asistencia: listar y guardar

## What to build

La pantalla de asistencia lista y guarda con Entity Framework. Al elegir una clase se ven todos los alumnos de esa comisión: los que tienen registro y los que no. Sin registro, el estado queda vacío.

Guardar un estado crea el registro o corrige el que ya existe para esa clase y ese alumno. Corregir uno existente deja `corrected_at` con la fecha de la corrección. El primer guardado lo deja nulo. Solo se guarda un alumno de la comisión de esa clase. El valor que llega a la base sigue siendo `presente` o `ausente`.

Este corte no cambia el esquema.

Al terminar se puede verificar: una clase muestra un renglón por alumno, marcar a uno sin registro lo crea, y volver a marcarlo actualiza la misma fila y completa `corrected_at`.

## Acceptance criteria

- [ ] Para una clase, la lista trae un renglón por alumno de la comisión, con o sin asistencia cargada.
- [ ] Guardar `presente` o `ausente` sobre un alumno sin registro inserta una fila y deja `corrected_at` nulo.
- [ ] Volver a guardar sobre el mismo alumno y la misma clase actualiza esa fila y completa `corrected_at`.
- [ ] No queda más de una fila por clase y alumno.
- [ ] Guardar un alumno que no es de la comisión de esa clase no inserta.
- [ ] Este corte no cambia el esquema.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md)
