# Nómina

## What to build

La nómina se ve como la del mock: una tarjeta con barra de herramientas y una tabla, adentro del marco de la historia 01.

La barra tiene la búsqueda («Buscar por nombre o legajo») ocupando el espacio libre y, a la derecha, el conteo «N estudiantes» en texto terciario. El conteo es el de la lista filtrada y se actualiza al escribir y al cambiar de comisión. Hoy el rótulo puede quedar en 0 con la lista llena; eso queda resuelto.

Columnas, de izquierda a derecha: **Apellido y nombre** (avatar con iniciales, luego «Apellido, Nombre»), **Legajo** (mono, sin el prefijo «Legajo»), **Teléfono** (mono) y **Correo** (texto terciario). En esta pantalla el legajo es una columna, no un segundo renglón bajo el nombre. Encabezados en mayúsculas, 11px, texto terciario, alto 38. Filas de 64px, separador de 1px, hover con fondo elevado y cursor de mano. Clic en la fila abre el detalle del alumno que ya existe.

Sin resultados, el texto centrado es «Sin resultados para esa búsqueda.». La lista scrollea con la página; la tarjeta crece con el contenido y no deja un scroll interno que recorte las filas.

La tarjeta, la barra, la búsqueda, el encabezado de tabla, la celda de identidad y el estado vacío quedan como controles reutilizables del proyecto UI, para Seguimiento, Entregas y Asistencia. No se copia el XAML en cada pantalla y no se arma el árbol de carpetas de la guía.

## Acceptance criteria

- [ ] Con datos, la nómina coincide con la tabla del mock: avatar, «Apellido, Nombre», legajo, teléfono y correo en esas columnas.
- [ ] El conteo muestra la cantidad filtrada y cambia al buscar y al cambiar de comisión. Con la lista llena no dice 0.
- [ ] Una búsqueda sin coincidencias muestra «Sin resultados para esa búsqueda.».
- [ ] La fila tiene hover y abre el detalle del alumno.
- [ ] Los controles de esta pantalla quedan reutilizables; la pantalla no hardcodea colores ni radios fuera de los tokens.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

- [01 — Marco de la app](01-marco-de-la-app.md)
