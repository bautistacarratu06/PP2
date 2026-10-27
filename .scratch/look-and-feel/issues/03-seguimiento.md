# Seguimiento

## What to build

Seguimiento queda como en el mock: tres tarjetas de riesgo y, debajo, una tabla de cinco columnas. Reutiliza la tarjeta, la barra, la búsqueda, el encabezado y la celda de identidad de Nómina.

Las tres tarjetas (alto, medio, bajo) son neutras en reposo: punto de color, etiqueta «Riesgo alto/medio/bajo», número grande y «de N». No llevan fondo teñido. Son clicables y filtran la tabla; un segundo clic sobre la misma quita el filtro. La activa se distingue solo por el borde del color del nivel y un degradado muy suave, como en el HTML. El hover de una inactiva oscurece apenas el borde.

La barra de la tabla tiene la búsqueda, los chips Todos · Alto · Medio · Bajo y el conteo. Chips y tarjetas escriben el mismo filtro.

Columnas: **Estudiante** (avatar, «Apellido, Nombre» y el legajo mono debajo, sin prefijo), **Riesgo** (cápsula con punto, nombre del nivel y puntaje), **Asistencia** y **Entregas** (barra gris de 5px más el porcentaje) y **Contacto**. «Sin contactar» va en texto terciario; una fecha relativa va en texto primario. No hay columna de nivel separada ni columna con el texto que justifica el puntaje. El orden es por riesgo descendente. Clic en la fila abre el detalle.

La cápsula de riesgo, la barra con porcentaje, la tarjeta KPI y los chips quedan como controles reutilizables. Los colores de nivel salen de los tokens semánticos, no de una rama por pantalla.

## Acceptance criteria

- [ ] Las tres tarjetas muestran el conteo real de cada nivel sobre el total de la comisión, y filtrar desde una tarjeta o desde un chip deja la misma lista.
- [ ] Un segundo clic en la tarjeta activa vuelve a mostrar todos.
- [ ] La tabla tiene exactamente esas cinco columnas, con la cápsula «nivel + puntaje» y las dos barras de porcentaje.
- [ ] No se ve la justificación del puntaje ni una columna de nivel aparte.
- [ ] El orden es de mayor riesgo a menor. Clic en la fila abre el detalle.
- [ ] Una búsqueda sin coincidencias usa el mismo estado vacío que Nómina.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [02 — Nómina](02-nomina.md)
