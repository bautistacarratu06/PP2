# Nómina de una comisión

## What to build

Nómina de una comisión pasa a leerse con Entity Framework. Para la comisión elegida se ven los mismos alumnos, ordenados por apellido, con apellido y nombre, legajo, teléfono y correo.

Este repositorio solo lee. Este corte no da de alta alumnos, no cambia el esquema y no toca las otras pantallas.

Al terminar se puede verificar: abrir Nómina sobre una comisión con alumnos muestra esas filas. Una comisión sin alumnos muestra la lista vacía.

## Acceptance criteria

- [ ] Nómina de la comisión elegida muestra los mismos alumnos que antes, ordenados por apellido.
- [ ] Cada fila muestra apellido y nombre, legajo, teléfono y correo.
- [ ] Una comisión sin alumnos deja la lista vacía, sin error.
- [ ] Este corte no inserta alumnos y no cambia el esquema.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md)
