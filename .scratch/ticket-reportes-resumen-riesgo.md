# Epic H12: Vista de Reportes, Resumen de Riesgo por Comisión y Exportación

- **Tipo:** Epic / Historia de usuario
- **Estado:** Listo para implementar (issues en `.scratch/reportes-resumen-riesgo/issues/`)
- **Prioridad:** Alta
- **Componentes afectados:** `GestionEscolar.Core`, `UI`, `GestionEscolar.Tests`

---

## Objetivo

Permitir a los tutores visualizar un reporte textual dinámico sobre la situación de riesgo de una comisión (evaluando asistencia y entregas registradas), cargar automáticamente una comisión por defecto, visualizar estados de carga con skeletons, garantizar consistencia estricta con el panel de Seguimiento mediante un servicio centralizado, gestionar comisiones sin historial ("Todavía no hay datos para calcular el riesgo") y posibilitar la descarga/exportación del reporte en formato `.txt`.

## Issues

| # | Slice | Blocked by |
|---|---|---|
| 01 | Servicio centralizado de riesgo y generador del reporte textual | — |
| 02 | Vista de Reportes con navegación, carga por defecto, selector y skeletons | 01 |
| 03 | Exportación del reporte a archivo de texto (.txt) | 02 |

## Criterios de Aceptación Cubiertos

1. **Nueva sección en navegación y carga por defecto:** Opción "Reportes" en barra lateral; al ingresar carga automáticamente la comisión por defecto sin acción del usuario; reporte textual dinámico.
2. **Skeletons y selector de comisión:** Selector en cabecera para alternar comisión; skeletons (`SkeletonBlock`) durante la carga y recarga; ocultamiento de datos hasta tener el resultado procesado.
3. **CA1 (El número):** Identificación explícita de alumnos en riesgo sobre el total (ej. "7 de 30 estudiantes están en riesgo de abandonar").
4. **CA2 (Coincidencia con panel de Seguimiento):** Mismo servicio o método centralizado para el cálculo de riesgo sobre alumnos activos, evitando cálculos divergentes.
5. **CA3 (Comisión sin datos):** Si hay estudiantes cargados pero cero asistencias y cero entregas, mostrar exactamente "Todavía no hay datos para calcular el riesgo" (nunca "0 en riesgo").
6. **Exportación:** Botón para descargar el reporte en `.txt` utilizando APIs nativas (`Share.Default.RequestAsync`).

## Supuestos y Decisiones Técnicas

1. La evaluación de riesgo se calcula exclusivamente sobre alumnos activos (`IsActive`), en correspondencia con el resumen mostrado en `StudentsViewModel` (`ActiveRiskSummary`).
2. Se considera "sin datos" cuando todos los alumnos de la comisión registran 0 clases y 0 trabajos evaluados en la ventana temporal (o la comisión no posee asistencias ni entregas registradas).
3. La exportación utiliza formato `.txt` mediante `Share.Default.RequestAsync` con un archivo temporal generado en caché local, al no requerirse dependencias externas de PDF.

