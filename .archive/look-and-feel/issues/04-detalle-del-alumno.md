# Detalle del alumno

## What to build

El detalle ocupa la página que ya se abre desde Nómina y Seguimiento, con la disposición del mock.

Arriba del encabezado hay un botón de texto «Seguimiento», con chevron, sin fondo ni borde. Vuelve a Seguimiento. El ítem de la sidebar sigue siendo Seguimiento. El subtítulo del marco es «Nombre Apellido · legajo».

El cuerpo es una grilla: columna izquierda fija de 300px y columna derecha flexible, separadas 16px. Bajo ~860px pasa a una sola columna.

La izquierda es una tarjeta con el puntaje grande (52) en el color del nivel y, al lado, la cápsula del nivel sin puntaje. Debajo, dos tarjetas internas (radio 12): «Asistencia — X de N · P %» y «Entregas — X de N · P %», cada una con su barra. Más abajo, los bloques TELÉFONO y CORREO: etiqueta en mayúsculas terciaria y valor en color de acento. El correo largo corta sin desbordar la tarjeta. No hay ícono de persona ni el párrafo de justificación; ese desglose es el que muestran las dos tarjetas internas.

La derecha apila tres tarjetas: historial de asistencia, historial de entregas e intervenciones. Cada fila de historial tiene el texto a la izquierda y la cápsula de estado a la derecha (Presente, Ausente, Entregó, No entregó o Sin registro), con separador y sin separador en la última. Intervenciones lista lo que ya se carga. Si no hay ninguna, dice «Sin contactos registrados.». No se agrega un botón de alta ni un flujo nuevo: la app no tiene un comando para crear una intervención, y los resultados posibles ya están fijos en los datos.

Cambiar la comisión desde esta página vuelve a Seguimiento de la comisión nueva, porque el alumno abierto es de la anterior.

Se reutilizan la cápsula de riesgo, la barra y la tarjeta. La cápsula de estado de una fila de historial y el bloque etiqueta/valor quedan como controles reutilizables.

## Acceptance criteria

- [ ] El detalle coincide con la grilla del mock: puntaje, cápsula, dos métricas, teléfono, correo y tres tarjetas a la derecha.
- [ ] «Seguimiento» vuelve a la grilla y la sidebar mantiene Seguimiento activo.
- [ ] Asistencia y entregas muestran el conteo y el porcentaje que ya calcula el detalle, con la barra del mock.
- [ ] Las cápsulas de historial usan solo los estados que la app guarda, más «Sin registro».
- [ ] Sin intervenciones se lee «Sin contactos registrados.». No aparece un alta de contacto nueva.
- [ ] No se muestra el párrafo de justificación ni un ícono de persona.
- [ ] En una ventana angosta la grilla pasa a una columna sin scroll horizontal de la página.
- [ ] Cambiar la comisión vuelve a Seguimiento.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [03 — Seguimiento](03-seguimiento.md)
