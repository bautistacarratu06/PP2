# 04: Refactorización de Entregas con Acciones en Bloque y Columna de Estado

**What to build:** Migración de la pantalla de Entregas (`SubmissionsPage` / `SubmissionsViewModel`): reemplazo de los selectores interactivos fila por fila por una columna "Estado" de solo lectura, incorporación de selección múltiple y Action Bar flotante para operaciones en bloque ("Marcar como Entregó", "Marcar como No entregó") con diálogo modal de confirmación.

**Blocked by:** 01: Sticky Header y Selección Múltiple en Grillas, 02: Floating Action Bar y Modal de Confirmación Reutilizable

**Status:** completed

- [x] Se eliminan los selectores interactivos por fila en `SubmissionsPage`.
- [x] Cada fila muestra el estado de entrega actual mediante una columna de solo lectura ("Entregó", "No entregó" o "Sin registro").
- [x] La cabecera de la grilla de entregas permanece fija al hacer scroll.
- [x] Las filas de entregas y la cabecera disponen de checkboxes para selección individual y total.
- [x] Al seleccionar 1 o más estudiantes, emerge el Action Bar con las acciones "Entregó" y "No entregó".
- [x] Al pulsar una acción en el Action Bar, se despliega el modal de confirmación con la cantidad exacta de estudiantes a modificar.
- [x] Al confirmar en el modal, se ejecuta la actualización en la base de datos a través de `SubmissionService.SaveAsync` en lote, se refrescan las filas en la UI y se deselecciona.
- [x] Si se cancela el modal, no se modifican los registros de entregas.

