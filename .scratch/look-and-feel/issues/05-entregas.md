# Entregas

## What to build

Registrar entregas queda como en el mock: una tarjeta, la barra con el trabajo elegido y el avance, y una fila por estudiante con un control segmentado en lugar del desplegable.

En la barra, el selector de trabajo usa el mismo trato visual que un campo del mock (fondo de página, borde, radio 9) y la etiqueta «Trabajo». Las opciones siguen el texto que ya arma la pantalla, del estilo «TP1 - Fundamentos · 10/09/2026». Al lado, una barra de 5px y 140px de ancho con relleno de acento, y el texto «X de N registrados». El conteo es el de filas que ya tienen un estado guardado para ese trabajo.

Cada fila mide 60px: avatar e «Apellido, Nombre» a la izquierda, y a la derecha dos segmentos, **Entregó** (verde) y **No entregó** (rojo). No se agrega «Tarde»: ese estado no existe. Sin selección, los dos segmentos quedan en gris, que es «Sin registro», igual que hoy un valor vacío. Elegir uno lo pinta con el fondo suave y el texto del color pleno, y guarda como ya guarda el cambio de estado. Tocarlo de nuevo lo deselecciona y vuelve a «Sin registro» si el guardado actual lo permite; si quitar un estado no es un guardado válido, el segmento permanece en el último estado guardado y no se inventa un tercer valor. La lista sigue en orden alfabético por apellido.

La fila reutiliza la celda de identidad, la tarjeta y la barra de Nómina. El control segmentado y la barra de avance quedan reutilizables para Asistencia.

## Acceptance criteria

- [ ] La pantalla coincide con Entregas del mock: selector de trabajo en la barra, avance «X de N registrados» y filas de 60px.
- [ ] El control ofrece solo Entregó y No entregó. No existe un segmento Tarde.
- [ ] Elegir un segmento persiste el mismo estado que hoy persiste el desplegable, y el avance se actualiza.
- [ ] Una fila sin estado se ve con los dos segmentos en gris.
- [ ] El orden es alfabético por apellido.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [02 — Nómina](02-nomina.md)
