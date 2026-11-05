# 01: Sticky Header y Selección Múltiple en Grillas

**What to build:** Estandarización de la estructura de grillas para que la cabecera permanezca fija (sticky) al hacer scroll vertical en las filas de datos, incorporando la capacidad de mostrar checkboxes en cada fila y un checkbox maestro "Seleccionar todos" en la cabecera, condicionados a una bandera de disponibilidad de acciones masivas (`CanPerformBulkActions`).

**Blocked by:** None (can start immediately)

**Status:** completed

- [x] La cabecera con los títulos de columna permanece fija y visible en la parte superior cuando se scrollea verticalmente la lista de filas.
- [x] La cabecera incluye un checkbox maestro "Seleccionar todos" alineado con los checkboxes de las filas.
- [x] Marcar el checkbox maestro selecciona todas las filas filtradas/visibles actuales; desmarcarlo deselecciona todas.
- [x] Si el usuario selecciona manualmente todas las filas una a una, el checkbox maestro se marca automáticamente; si desmarca una fila, el maestro se desmarca.
- [x] Si la vista define `CanPerformBulkActions = false`, no se renderizan los checkboxes (ni en las filas ni en la cabecera), pero la cabecera de títulos permanece visible y fija con la alineación exacta de columnas.
- [x] El scroll vertical de los datos es fluido y conserva el skeleton de carga (`GridLoadingView`) en su contenedor.

