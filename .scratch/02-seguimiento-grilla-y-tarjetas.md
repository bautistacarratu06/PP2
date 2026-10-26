# Seguimiento calcula grilla y tarjetas en un solo caso de uso

## What to build

El caso de uso de seguimiento, en Core, devuelve un resultado con la lista de alumnos ya evaluados y el resumen Alto/Medio/Bajo contado sobre esa misma lista. El ViewModel de seguimiento pide ese resultado y las comisiones; deja de conocer un repositorio. Las tarjetas salen de ese resumen. La consulta que hoy clasifica el riesgo en SQL sale del camino de la pantalla. Un dato sucio (más asistencias que clases) se sigue recortando al total, y una comisión vacía sigue devolviendo lista vacía y resumen en cero. Cambiar de comisión recarga grilla y tarjetas juntas.

`TrackingViewModel` es el único ViewModel que recibe un repositorio (`IRiskSummaryRepository`) y pide el resumen en paralelo a `TrackingService.GetSectionRiskAsync`. Ese resumen SQL repite la fórmula (pesos 0.6 y 0.4, recorte al total, redondeo). La vista `vw_student_risk` no entra: no la usa el resumen, calcula otra cosa y la migración ya aplicada no se toca.

Tests a renombrar a `Given_When_Then`:

- `TrackingServiceTests.GetSectionRiskAsync_CalculatesRiskAndLevelPerStudent`
- `TrackingServiceTests.GetSectionRiskAsync_UsesConfiguredThresholds`
- `TrackingServiceTests.GetSectionRiskAsync_EmptySection_ReturnsEmpty`
- `TrackingServiceTests.Evaluate_InconsistentData_DoesNotThrow`
- `TrackingServiceTests.Evaluate_NoClassesNorAssignments_ReturnsZeroRisk`
- `RiskSummaryTests.FromResults_CountsMatchLevelsShownInTable`
- `RiskSummaryTests.Empty_HasZeroTotal`
- `SectionServiceTests.TrackingService_UsesRequestedSection` (está en el archivo de comisiones; pasa al área de seguimiento)

El test que compara la fórmula con una copia SQL (`Formula_MatchesSqlEquivalent`) se reemplaza por el conteo del propio caso de uso.

Al terminar se puede verificar: en Seguimiento, con el filtro en Todos, las tarjetas coinciden con la cantidad de filas de cada nivel. Al cambiar de comisión cambian las dos cosas a la vez. No queda una segunda fórmula para las tarjetas.

## Acceptance criteria

- [ ] Dada una comisión con alumnos Alto, Medio y Bajo, al pedir el seguimiento, el resumen cuenta esos niveles y el total es la cantidad de filas.
- [ ] Dados umbrales distintos de 50/25, al evaluar, el nivel usa esos umbrales.
- [ ] Dada una comisión vacía, al pedir el seguimiento, la lista va vacía y el resumen queda en cero.
- [ ] Dado un alumno con más asistencias que clases, al evaluar, no falla y el riesgo usa el total recortado.
- [ ] Dado el id de comisión pedido, al consultar, el repositorio de filas recibe ese id.
- [ ] El ViewModel de seguimiento ya no recibe un repositorio.
- [ ] Los tests de esta área se llaman `Given…_When…_Then…` y usan un repositorio falso.
- [ ] En la app, las tarjetas de la comisión elegida coinciden con la grilla sin filtrar.

## Blocked by

- [01 — Fórmula de riesgo como regla pura](01-formula-de-riesgo-como-regla-pura.md)
