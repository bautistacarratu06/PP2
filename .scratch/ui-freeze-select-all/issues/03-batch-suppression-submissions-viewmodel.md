# 03: Implementar Batch Suppression Flag en SubmissionsViewModel

**What to build:** Incorporación de la bandera de supresión por lote (`_isUpdatingSelection`) en `SubmissionsViewModel` para evitar notificaciones cruzadas y bucles de reentrancia al marcar o desmarcar el checkbox maestro en la grilla de Entregas.

**Blocked by:** 01: Eliminar doble suscripción de eventos en StatusRow y Watch

**Status:** ready-for-agent

- [ ] `IsAllSelected.set` en `SubmissionsViewModel` ejecuta la mutación masiva bajo la guardia `_isUpdatingSelection`.
- [ ] `NotifySelectionChanged()` no emite eventos de cambio de propiedad si `_isUpdatingSelection == true`.
- [ ] `ClearSelectionCommand` opera protegido bajo la bandera de lote.
- [ ] Hacer clic en el checkbox maestro de Entregas marca y desmarca todas las filas de forma inmediata sin UI freeze.
- [ ] El Action Bar de Entregas aparece fluidamente con las opciones contextuales ("Marcar Entregó", "Marcar No entregó").

