# 02: Floating Action Bar y Modal de Confirmación Reutilizable

**What to build:** Controles reutilizables de UI en .NET MAUI:
1. `FloatingActionBar`: un panel flotante inferior estilo Linear con elevación y bordes redondeados que emerge con animación cuando hay al menos 1 elemento seleccionado y se oculta cuando no hay selección, mostrando el contador de seleccionados y un slot para acciones contextuales.
2. `ConfirmationModal`: un diálogo modal superpuesto con backdrop oscuro que previene la ejecución inmediata de una acción masiva, mostrando la leyenda descriptiva, el conteo dinámico de elementos afectados, botón "Aceptar" y botón "Cancelar".

**Blocked by:** None (can start immediately)

**Status:** completed

- [x] `FloatingActionBar` flota en la parte inferior central de la pantalla sin bloquear permanentemente la vista de datos.
- [x] `FloatingActionBar` aparece automáticamente cuando `SelectedCount > 0` y se oculta cuando `SelectedCount == 0`.
- [x] Incluye visualización del conteo (ej. "3 seleccionados") y botón de acción rápida para deseleccionar todos.
- [x] Admite contenido contextual inyectado (botones con comandos específicos de cada pantalla).
- [x] `ConfirmationModal` bloquea la interacción con la pantalla subyacente mientras está abierto.
- [x] `ConfirmationModal` presenta título, mensaje dinámico con cantidad de elementos afectados (ej. "Estás a punto de modificar 15 registros"), botón "Aceptar" y botón "Cancelar".
- [x] Pulsar "Cancelar" o cerrar el modal descarta la ejecución sin alterar las filas seleccionadas.
- [x] Los estilos respetan el tema oscuro de la aplicación (paleta `Surface`, `Raised`, `Line`, `TextPrimary`, `Accent`).

