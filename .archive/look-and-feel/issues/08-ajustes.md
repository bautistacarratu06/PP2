# Ajustes

## What to build

Ajustes queda en una sola tarjeta de ancho máximo 640, con padding 24, como en el mock. El selector de Comisión del marco sigue visible; los umbrales no dependen de la comisión.

Dentro de la tarjeta: título «Umbrales de riesgo» y el texto «Alto si el riesgo es igual o mayor al primer valor; Medio desde el segundo.». Dos campos numéricos en dos columnas, con la etiqueta arriba: Alto y Medio, límite 0–100. El botón primario compacto «Guardar umbrales» usa el guardado que ya existe. Al lado, en texto terciario, la confirmación cuando el guardado responde bien (el mensaje de éxito que ya devuelve ese guardado, o «Guardado» si ese mensaje es genérico). Un error de carga o de guardado sigue viéndose, en el color de peligro, sin tapar los campos.

Un separador y el bloque «Fórmula». El texto explica la regla que ya calcula el riesgo: score = asistencia × 60 % + entregas × 40 %, riesgo = 100 − score. Dos cápsulas neutras, «60 % asistencia» y «40 % entregas», reutilizan la cápsula del calendario. Los pesos no se editan.

Los campos usan el estilo del marco. No se hardcodean colores.

## Acceptance criteria

- [ ] La tarjeta coincide con Ajustes del mock: ancho 640, dos campos, botón, confirmación y bloque de fórmula con las dos cápsulas.
- [ ] Guardar persiste los umbrales por el mismo camino que hoy, y un valor fuera de 0–100 no se guarda.
- [ ] Cambiar la comisión en el marco no modifica los valores de los campos ni lo guardado.
- [ ] La fórmula se muestra como texto y cápsulas. No aparece un editor de pesos.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [07 — Calendario](07-calendario.md)
