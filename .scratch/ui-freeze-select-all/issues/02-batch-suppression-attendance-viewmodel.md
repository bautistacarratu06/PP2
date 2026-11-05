# 02: Implementar Batch Suppression Flag en AttendanceViewModel

**What to build:** Incorporación de una bandera de supresión por lote (`_isUpdatingSelection`) en `AttendanceViewModel` para bloquear notificaciones intermedias y romper el bucle de enlace reentrante al marcar o desmarcar el checkbox maestro en la grilla de Asistencia.

**Blocked by:** 01: Eliminar doble suscripción de eventos en StatusRow y Watch

**Status:** ready-for-agent

- [ ] `IsAllSelected.set` utiliza un bloque de guardia (`_isUpdatingSelection = true` con `finally { _isUpdatingSelection = false }`).
- [ ] `NotifySelectionChanged()` descarta ejecuciones intermedias cuando `_isUpdatingSelection == true`.
- [ ] `ClearSelectionCommand` se ejecuta protegido bajo `_isUpdatingSelection` con una única notificación al final.
- [ ] Hacer clic en el checkbox maestro de Asistencia marca todas las filas instantáneamente sin congelar la interfaz.
- [ ] El Action Bar emerge en un único ciclo de renderizado mostrando el contador exacto de estudiantes seleccionados.

