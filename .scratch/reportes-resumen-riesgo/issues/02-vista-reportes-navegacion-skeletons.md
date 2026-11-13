# 02: Vista de Reportes con navegación, carga por defecto, selector y skeletons

## Parent

[Epic H12: Vista de Reportes, Resumen de Riesgo por Comisión y Exportación](../../ticket-reportes-resumen-riesgo.md)

## What to build

Crear la pantalla de visualización del reporte textual en la aplicación MAUI (`ReportsPage` y `ReportsViewModel`), conectarla a la navegación lateral (`SidebarView`) y al enrutamiento general (`AppShell`).

La pantalla debe apoyarse en la infraestructura existente del proyecto (`PageShell` y `SectionScopedViewModel` / `SectionSelection`):
1. Al ingresar a la pantalla, se debe resolver y cargar de manera automática la comisión por defecto sin requerir intervención del usuario.
2. La cabecera incluye el selector de comisiones (`SectionPicker` provisto por `PageShell`). Al cambiar de comisión, el reporte se regenera de forma reactiva.
3. Durante la carga inicial y los cambios de comisión (`IsBusy = true`), se muestran skeletons visuales (`SkeletonBlock`) ocultando los datos hasta que la información de la base de datos se encuentre procesada.
4. Al completarse la carga, se presentan los párrafos de texto dinámicos generados por el servicio, mostrando claramente el resumen de riesgo o la advertencia *"Todavía no hay datos para calcular el riesgo"* si la comisión no tiene historial.

## Acceptance criteria

- [x] Opción "Reportes" agregada al menú de navegación lateral (`SidebarView`), con icono y ruta `ReportsPage`.
- [x] Ruta registrada en `AppShell.xaml` y tipo registrado en `MauiProgram.cs` como transient.
- [x] `ReportsViewModel` extiende `SectionScopedViewModel` (o utiliza `SectionSelection`) y consume `IReportService`.
- [x] Al entrar a la vista, se carga automáticamente la comisión por defecto y su reporte sin interacción previa del usuario.
- [x] El cambio de comisión desde el selector de cabecera regenera el reporte de la nueva comisión.
- [x] Durante los tiempos de espera (carga inicial y cambio de comisión), se visualizan componentes `SkeletonBlock` que reemplazan el contenido textual hasta finalizar el procesamiento.
- [x] El reporte textual renderiza los párrafos generados dinámicamente según los datos de la comisión.
- [x] Ciclo de vida conectado (`OnAppearing` / `OnDisappearing` con suscripciones de cambio de comisión).
- [x] Tests unitarios en `GestionEscolar.Tests` (`ReportServiceTests`) para el contenido del reporte; flujos de carga / cambio de comisión / `IsBusy` vía `SectionScopedViewModel` (no hay proyecto `UI.Tests`).

## Blocked by

- 01: Servicio centralizado de riesgo y generador del reporte textual

