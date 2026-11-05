# [Bug/Performance] Resolver bloqueo de UI (UI Freeze) al usar "Seleccionar todos" en grillas

- **Tipo:** Corrección de Bug / Optimización de Rendimiento
- **Estado:** Listo para desarrollo (`ready-for-agent`)
- **Prioridad:** Bloqueante / Alta
- **Componentes afectados:** `GestionEscolar/UI` (`ViewModels/StatusRow.cs`, `ViewModels/AttendanceViewModel.cs`, `ViewModels/SubmissionsViewModel.cs`)

---

## 1. Descripción del Problema

Al hacer clic en el checkbox maestro de la cabecera ("Seleccionar todos") en las grillas de datos (Asistencia y Entregas), la aplicación se congela por completo (UI Thread freeze) y la pantalla queda bloqueada.

El diagnóstico técnico reveló que la causa raíz es una **recursión infinita por enlace bidireccional (`TwoWay` Binding Loop)** combinada con una **tormenta de eventos (`PropertyChanged` Storm)**:
1. `CheckBox.IsChecked` en MAUI es `TwoWay` por defecto.
2. Al iterar `row.IsSelected = value`, la primera fila mutada (`row[0]`) dispara `NotifySelectionChanged()`.
3. Esto notifica `OnPropertyChanged(nameof(IsAllSelected))` a mitad del bucle.
4. El getter `IsAllSelected => Rows.All(...)` evalúa a `false` porque las filas restantes aún no mutaron.
5. El control `CheckBox` recibe `false` y, por ser `TwoWay`, vuelve a invocar el setter `IsAllSelected = false` de forma reentrante.
6. Adicionalmente, existe una doble suscripción activa por fila (el callback del constructor de `StatusRow` y el evento `PropertyChanged` capturado en el método `Watch`).

---

## 2. Criterios de Aceptación

- [ ] **Resolución del Bloqueo (UI Freeze):** El usuario debe poder marcar y desmarcar todos los elementos desde el checkbox maestro de forma instantánea, sin que la aplicación se congele ni haya caída perceptible de frames.
- [ ] **Renderizado Óptimo del Action Bar:** Al pulsar "Seleccionar todos", el Action Bar debe emerger de forma fluida mostrando el contador total (ej. "30 estudiantes seleccionados") en un único ciclo de renderizado global, sin estados intermedios ni saltos secuenciales.
- [ ] **Eliminación de la Duplicidad de Eventos:** Cada mutación de `StatusRow.IsSelected` debe canalizarse por una sola vía de notificación al ViewModel.
- [ ] **Batch Suppression Flag:** Se debe incorporar una bandera de guardia (`_isUpdatingSelection`) que suspenda las notificaciones cruzadas durante las operaciones masivas de selección/deselección.
- [ ] **Integridad en Selección Individual:** Seleccionar filas una a una manualmente debe continuar funcionando reactivamente, actualizando el contador y marcando automáticamente el checkbox maestro cuando todas estén seleccionadas.

---

## 3. Plan de Desglose en Vertical Slices (Tracer Bullets)

| # | Ticket | Dependencias | Qué entrega |
|---|---|---|---|
| **01** | **Eliminar doble suscripción de eventos en `StatusRow` y `Watch`** | Ninguna | Unificación del canal de eventos de selección de fila; eliminación del listener redundante en `Watch`. |
| **02** | **Implementar Batch Suppression Flag en `AttendanceViewModel`** | 01 | Selección y deselección masiva instantánea y no reentrante en Asistencia con notificación global única. |
| **03** | **Implementar Batch Suppression Flag en `SubmissionsViewModel`** | 01 | Selección y deselección masiva instantánea y no reentrante en Entregas con notificación global única. |
| **04** | **Pruebas de estrés y verificación de fluidez del UI Thread** | 02, 03 | Validación con nóminas de más de 30 estudiantes sin caídas de frame y pruebas unitarias/regresión ejecutadas. |

