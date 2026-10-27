# Calendario

## What to build

El calendario queda en dos tarjetas, como en el mock, sin el rótulo en mayúsculas ni el hueco vacío bajo la lista.

La tarjeta de alta es una sola fila: selector de tipo (Clase / Trabajo), campo de nombre con placeholder «La fecha identifica la clase», selector de fecha y botón primario compacto «Agregar». El campo de nombre ocupa el espacio libre. Se respetan las reglas de alta que ya tiene la pantalla: una clase se identifica por la fecha, y un trabajo exige el nombre que el servicio ya valida. Si el servicio rechaza el nombre vacío, se muestra el error actual; no se inventa un nombre para saltar esa validación.

La tarjeta de lista, 16px debajo, crece con las filas. Cada fila mide 54px, ordenada por fecha ascendente: nombre en semibold a la izquierda, cápsula de tipo y fecha mono alineada a la derecha en un ancho fijo. «Clase» es texto secundario sobre fondo elevado. «Trabajo» es texto de acento sobre el acento al 14 %. No hay separador bajo la última fila.

La cápsula de tipo queda reutilizable para los pesos de la fórmula en Ajustes. El campo y el botón primario usan los estilos del marco.

## Acceptance criteria

- [ ] Alta y lista se ven como en el mock: una fila de alta y filas de 54px con nombre, cápsula y fecha.
- [ ] Agregar una clase o un trabajo sigue las reglas de validación actuales y la lista queda ordenada por fecha ascendente.
- [ ] La tarjeta de la lista no reserva un alto vacío cuando hay pocos ítems.
- [ ] «Clase» y «Trabajo» usan los dos tratos de cápsula del mock.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [01 — Marco de la app](01-marco-de-la-app.md)
