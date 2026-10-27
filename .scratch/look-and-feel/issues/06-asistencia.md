# Asistencia

## What to build

Registrar asistencia usa la misma tarjeta que Entregas. Cambia el selector de la barra, los rótulos del control y suma una acción.

El selector se etiqueta «Clase» y lista las fechas que ya ofrece la pantalla. La barra de avance y el texto «X de N registrados» cuentan las filas con estado para esa clase. A la derecha de la barra, el botón fantasma «Marcar todos presentes» marca Presente a cada estudiante de la lista, por el mismo guardado que ya usa una fila, y actualiza el avance.

El control segmentado de cada fila ofrece solo **Presente** (verde) y **Ausente** (rojo). No se agrega «Tarde». Sin selección, ambos quedan en gris («Sin registro»). Elegir guarda como hoy; volver a tocar el segmento activo lo deselecciona solo si ese guardado ya admite vacío, y si no, deja el último estado guardado.

Se reutilizan el control segmentado, la barra de avance, la celda de identidad, la tarjeta y la barra de herramientas. El botón fantasma usa el estilo con nombre del marco.

## Acceptance criteria

- [ ] La pantalla coincide con Asistencia del mock, con selector de clase, avance y el botón «Marcar todos presentes» alineado a la derecha de la barra.
- [ ] «Marcar todos presentes» deja cada fila en Presente y el avance en N de N, persistiendo por el guardado que ya existe.
- [ ] El control ofrece solo Presente y Ausente. No existe un segmento Tarde.
- [ ] Elegir un segmento persiste el mismo estado que hoy persiste el desplegable.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [05 — Entregas](05-entregas.md)
