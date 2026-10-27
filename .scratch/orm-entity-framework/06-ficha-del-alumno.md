# Ficha del alumno

## What to build

La ficha del alumno pasa a leerse con Entity Framework. Muestra el contacto y la comisión, el historial de asistencia de esa comisión (una fila por clase, con o sin registro, la más reciente primero), las entregas de esa comisión (una fila por trabajo, con o sin registro, por fecha de vencimiento) y las intervenciones del alumno, la más reciente primero.

Un alumno que no existe no trae ficha. Este repositorio solo lee. Este corte no cambia el esquema y no da de alta intervenciones.

Al terminar se puede verificar: abrir la ficha de un alumno de la comisión muestra contacto, clases, trabajos e intervenciones. Pedir un alumno inexistente no muestra datos y no falla la pantalla.

## Acceptance criteria

- [ ] La ficha muestra contacto y comisión del alumno.
- [ ] El historial de asistencia trae una fila por clase de la comisión, con o sin registro, de la más reciente a la más antigua.
- [ ] El historial de entregas trae una fila por trabajo de la comisión, con o sin registro, por fecha de vencimiento.
- [ ] Las intervenciones de ese alumno aparecen de la más reciente a la más antigua.
- [ ] Un alumno inexistente no devuelve ficha.
- [ ] Este corte no escribe intervenciones y no cambia el esquema.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md)
