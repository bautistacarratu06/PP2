# Seguimiento de la comisión

## What to build

Seguimiento pasa a leer las filas de la comisión con Entity Framework. Cada fila trae los datos crudos del alumno: clases totales, clases presentes, trabajos totales, trabajos entregados y último contacto. El puntaje y el nivel de riesgo se siguen calculando en Core, y las tarjetas siguen saliendo de esa lista ya evaluada.

El conteo que hoy replica la fórmula en SQL, y que la pantalla no usa, también pasa a Entity Framework en este corte. Sus totales coinciden con evaluar las mismas filas con la regla de Core. La consulta de las filas no calcula el riesgo.

Este corte no cambia el esquema y no lee `vw_student_risk`.

Al terminar se puede verificar: la grilla y las tarjetas de una comisión coinciden con el cálculo de Core, y un alumno sin clases o sin trabajos queda en cero por ciento, no en error.

## Acceptance criteria

- [ ] La grilla de seguimiento de una comisión muestra los mismos alumnos, con asistencia, entregas y último contacto.
- [ ] Las filas traen totales crudos. El puntaje y el nivel los calcula Core, y las tarjetas salen de esa lista.
- [ ] Sin clases o sin trabajos, el porcentaje de esa parte es cero.
- [ ] El conteo por nivel que no usa la pantalla coincide con la regla de Core sobre las mismas filas, y también se lee con Entity Framework.
- [ ] Ninguna de las dos consultas usa `vw_student_risk`.
- [ ] Este corte no cambia el esquema.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md)
