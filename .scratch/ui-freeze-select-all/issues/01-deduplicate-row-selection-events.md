# 01: Eliminar doble suscripción de eventos en StatusRow y Watch

**What to build:** Unificación del flujo de eventos de selección de fila para que cada cambio de `IsSelected` en `StatusRow` viaje por un único canal de notificación hacia el ViewModel, eliminando la doble invocación de `NotifySelectionChanged()` provocada por la combinación del callback del constructor y el manejador `PropertyChanged` en `Watch()`.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] Al cambiar `StatusRow.IsSelected`, la notificación al ViewModel (`NotifySelectionChanged()`) se ejecuta exactamente 1 vez por fila.
- [ ] Se elimina la suscripción redundante para `IsSelected` dentro del método `Watch()`.
- [ ] La selección manual individual de cada fila sigue actualizando correctamente el contador y el Action Bar.

