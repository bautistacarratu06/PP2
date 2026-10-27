# Entregas: listar y guardar

## What to build

La pantalla de entregas lista y guarda con Entity Framework. Al elegir un trabajo se ven todos los alumnos de esa comisión: los que tienen registro y los que no. Sin registro, el estado queda vacío.

Guardar un estado crea el registro o corrige el que ya existe para ese trabajo y ese alumno. Solo se guarda un alumno de la comisión de ese trabajo. El valor que llega a la base sigue siendo `entregó` o `no entregó`.

Este corte no cambia el esquema y no agrega una fecha de corrección: la tabla de entregas no la tiene.

Al terminar se puede verificar: un trabajo muestra un renglón por alumno, marcar a uno sin registro lo crea, y volver a marcarlo actualiza la misma fila.

## Acceptance criteria

- [ ] Para un trabajo, la lista trae un renglón por alumno de la comisión, con o sin entrega cargada.
- [ ] Guardar `entregó` o `no entregó` sobre un alumno sin registro inserta una fila.
- [ ] Volver a guardar sobre el mismo alumno y el mismo trabajo actualiza esa fila.
- [ ] No queda más de una fila por trabajo y alumno.
- [ ] Guardar un alumno que no es de la comisión de ese trabajo no inserta.
- [ ] Este corte no cambia el esquema.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md)
