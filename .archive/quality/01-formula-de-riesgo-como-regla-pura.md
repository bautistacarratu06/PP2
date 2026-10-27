# Fórmula de riesgo como regla pura

## What to build

La regla de riesgo pasa a ser un cálculo puro: asistencias, entregas, totales y umbrales entran juntos y devuelven puntaje y nivel. No queda un objeto al que primero se le cargan los totales y después se le pide el cálculo. Seguimiento sigue evaluando cada alumno con esa regla y con los mismos umbrales de ahora (50 y 25). La fórmula sigue siendo 60 % inasistencia y 40 % entregas faltantes, con redondeo alejándose de cero. Los porcentajes de asistencia y de entregas que ya muestra la grilla quedan definidos al lado de esos datos, con total cero igual a cero. Los números visibles no cambian: el caso mixto sigue en 32, el borde 50 sigue en Alto y el borde 25 sigue en Medio.

Cubre el punto del diagnóstico en que el nivel según umbrales vive en la regla, no en un servicio con totales mutables. Hoy no tienen test `StudentTrackingData.AttendancePercent` y `StudentTrackingData.DeliveryPercent`. `ScoringRiskServices.Calculate` y `GetRiskLevel` sí están cubiertos; el rechazo de totales negativos no.

Tests a renombrar a `Given_When_Then` (ninguno sigue ese patrón hoy):

- `ScoringRiskServicesTests.Calculate_MixedCase_Returns32`
- `ScoringRiskServicesTests.GetRiskLevel_DefaultThresholds`
- `ScoringRiskServicesTests.Calculate_MoreThanTotal_Throws`
- `RiskSummaryTests.Formula_MatchesSqlEquivalent`

Al terminar se puede verificar: un alumno con 16 de 20 clases y 5 de 10 entregas sigue en riesgo 32, nivel Medio, y la grilla sigue mostrando sus porcentajes.

## Acceptance criteria

- [ ] Dado el caso mixto, al calcular, el puntaje es 32.
- [ ] Dados los umbrales 50 y 25, al clasificar 50, 49, 25 y 24, los niveles son Alto, Medio, Medio y Bajo.
- [ ] Dado un hecho mayor que el total, o un total negativo, al calcular, la operación se rechaza.
- [ ] Dados cero clases y cero trabajos, al calcular, el riesgo es 0.
- [ ] Dado un redondeo .5, al calcular, el resultado se aleja de cero (el caso 18 que hoy documenta la fórmula).
- [ ] Dados asistidos y totales, al leer los porcentajes, coinciden con lo que la grilla ya muestra, y total cero da 0.
- [ ] Los tests de esta área se llaman `Given…_When…_Then…`.
- [ ] Seguimiento, para los mismos datos, muestra el mismo puntaje y el mismo nivel que antes.

## Blocked by

- None - can start immediately
