# 05: Estandarización de Cabeceras Fijas en Seguimiento y Nómina

**What to build:** Estandarización del comportamiento de cabecera fija (sticky header) en la vista de Seguimiento (`TrackingPage`) y en la vista de Nómina (`StudentListPage`), respetando la regla de no renderizar checkboxes si la pantalla no cuenta con operaciones masivas habilitadas, y garantizando alineación visual estricta entre cabecera y filas.

**Blocked by:** 01: Sticky Header y Selección Múltiple en Grillas

**Status:** completed

- [x] En la vista de Seguimiento (`TrackingPage`), la cabecera con las columnas ESTUDIANTE, RIESGO, ASISTENCIA, ENTREGAS y CONTACTO se mantiene fija mientras el usuario scrollea verticalmente la lista de alumnos.
- [x] En la vista de Nómina (`StudentListPage`), la cabecera con APELLIDO Y NOMBRE, LEGAJO, TELÉFONO y CORREO se mantiene fija durante el scroll.
- [x] Dado que Seguimiento y Nómina actualmente no disponen de acciones masivas, no se renderizan los checkboxes en cabecera ni en filas, pero las columnas quedan perfectamente alineadas.
- [x] La estructura es extensible para habilitar `CanPerformBulkActions = true` en el futuro sin reestructurar el layout.
- [x] El comportamiento visual se mantiene consistente con el tema oscuro y los skeletons de carga existentes (`GridLoadingView`).

