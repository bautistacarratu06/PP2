# 01: Baja lógica de estudiantes

## Parent

[Vista unificada de Estudiantes](../../ticket-estudiantes-vista-unificada.md)

## What to build

Habilitar la baja lógica de estudiantes de una comisión: columna de activo en persistencia (migración EF, default activo), puerto y caso de uso para desactivar un lote de estudiantes en una sola operación transaccional, y tests xUnit que cubran el comando por lote (éxito atómico y que un fallo no deje cambios a medias).

Los inactivos deben poder excluirse de agregados de riesgo/porcentajes (no cuentan en tarjetas ni en %; ver supuestos del parent). Reutilizar servicios/repositorios existentes cuando ya existan caminos de escritura de alumno; no inventar ORM ni scripts históricos `0001`–`0010` — schema vía migración EF en `GestionEscolar.Data/Migrations`.

## Acceptance criteria

- [ ] Existe columna de activo en estudiantes (bit, default activo) aplicada con migración EF.
- [ ] Caso de uso / servicio Core desactiva una lista de IDs en una sola operación.
- [ ] La escritura en Data corre en transacción: fallo → sin cambios parciales.
- [ ] Los estudiantes inactivos quedan fuera de los agregados de riesgo y porcentajes usados por la consulta de comisión.
- [ ] Tests xUnit (Core con fakes; Data.Tests si toca el repositorio) cubren el comando por lote.
- [ ] Build sin warnings nuevos; formato del repo respetado.

## Blocked by

None - can start immediately
