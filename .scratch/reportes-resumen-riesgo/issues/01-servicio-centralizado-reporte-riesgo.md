# 01: Servicio centralizado de riesgo y generador del reporte textual

## Parent

[Epic H12: Vista de Reportes, Resumen de Riesgo por Comisión y Exportación](../../ticket-reportes-resumen-riesgo.md)

## What to build

Construir en `GestionEscolar.Core` el servicio centralizado de cálculo y reporte de riesgo para comisiones (`IReportService` / `ReportService`, junto con el modelo `SectionRiskReport`).

Este servicio debe constituir la **única fuente de verdad** para el resumen de riesgo de una comisión, garantizando consistencia absoluta con el panel de Seguimiento (`StudentsViewModel`) (CA2). Evalúa a los alumnos activos (`IsActive`) aplicando la regla de riesgo (`RiskRule`) sobre los datos devueltos por `ITrackingRepository`.

Además, implementa la regla de negocio para comisiones sin historial (CA3): si una comisión tiene alumnos pero registra 0 clases y 0 entregas (o no hay asistencias ni entregas registradas en el período), el reporte debe marcar explícitamente que no cuenta con datos suficientes y emitir textualmente: *"Todavía no hay datos para calcular el riesgo"*, evitando clasificar a los estudiantes como "0 en riesgo".

Cuando existan datos, genera dinámicamente los párrafos de texto que describen la situación general de la comisión, destacando el conteo de alumnos en riesgo alto sobre el total (ej. *"7 de 30 estudiantes están en riesgo de abandonar"*, CA1), junto al promedio de asistencia y entregas.

Unificar `StudentsViewModel` (o la consulta base en `StudentsQueryService`) para utilizar este mismo cálculo centralizado de riesgo sobre alumnos activos.

## Acceptance criteria

- [ ] Contrato `IReportService` y clase `ReportService` creados en `GestionEscolar.Core.Services`.
- [ ] Modelo de resultado `SectionRiskReport` con información de la comisión, resumen de riesgo (alto, medio, bajo, total), bandera `HasData` y colección de párrafos textuales generados dinámicamente.
- [ ] Si la comisión tiene estudiantes pero cero asistencias y entregas registradas, el reporte contiene el texto exacto: `"Todavía no hay datos para calcular el riesgo"` y no reporta "0 en riesgo" (CA3).
- [ ] Si hay datos, el texto incluye la relación exacta de alumnos en riesgo alto sobre el total (ej. `"X de Y estudiantes están en riesgo de abandonar"`) (CA1).
- [ ] `StudentsViewModel` y `ReportService` consumen la misma lógica o método centralizado para el resumen de riesgo de alumnos activos (CA2).
- [ ] Servicio registrado en el contenedor de dependencias (`CoreServiceCollectionExtensions.cs` / `MauiProgram.cs`).
- [ ] Tests unitarios en `GestionEscolar.Tests` con fakes cubriendo: comisión con datos normales, comisión vacía, comisión con estudiantes pero sin historial de asistencia/entregas, y consistencia de conteos.

## Blocked by

None - can start immediately

