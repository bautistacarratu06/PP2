# Marco de la app

## What to build

La ventana queda con el marco del mockup en las siete vistas, aunque el cuerpo de cada pantalla conserve su contenido hasta su propia historia.

La paleta, la tipografía y los estilos con nombre salen del HTML: fondo `#0A0A0A`, sidebar `#0E0E0E`, superficies `#131313` / `#1B1B1B`, bordes `#232323` y `#303030`, texto en tres niveles, acento violeta `#9D8CFF` / `#7C6AF2`, y los semánticos solo para riesgo y estados. Inter (400, 500, 600, 700) y JetBrains Mono para legajos, teléfonos y fechas. Un solo tema, el del mock. Si sigue existiendo el enlace de tema claro/oscuro, los dos apuntan a estos tokens. No queda azul marino ni el acento teal.

La barra de título de Windows usa el mismo fondo que el contenido, con los botones legibles y sin un título de texto: el nombre vive en la marca de la sidebar.

La sidebar reemplaza el flyout genérico, sin cambiar las rutas ni cómo Shell resuelve las páginas. Expandida mide 232px: marca con el cuadrado degradado y la «G», «Gestión Escolar» y «PP2 · IFTS», y los ítems Nómina, Seguimiento, Entregas, Asistencia, Calendario y Ajustes, cada uno con su icono de línea. El ítem activo tiene fondo elevado, icono en acento y una barra de 3px pegada al borde izquierdo. En el detalle del alumno sigue activo Seguimiento. El pie «Colapsar» la lleva a 64px (solo iconos, chevron girado, tooltip con el nombre) en unos 200ms. Por debajo de ~860px arranca colapsada.

Todas las páginas usan el mismo encabezado: título, subtítulo y el selector de Comisión a la derecha. El contenido queda a ancho máximo 1160, centrado, con el padding del mock (36 arriba, 40 a los lados, 56 abajo; en angosto, 24 arriba y 18 a los lados). Si el encabezado no entra, el selector pasa debajo. Cada página deja de dibujar su propio título, su rótulo en mayúsculas y su propio selector.

La comisión elegida es una sola para toda la ventana. Cambiarla actualiza el selector en todas las pantallas y recarga la vista visible. Nómina deja de cargar siempre la comisión 1 y usa esa selección. Ajustes muestra el selector por consistencia del marco; los umbrales siguen siendo los globales que ya se guardan.

En este corte se dejan estilos con nombre para la superficie de tarjeta, el campo, el botón primario y el botón fantasma, con los radios, bordes y colores del mock, para que las pantallas siguientes no hardcodeen hex. Los controles nuevos viven en el proyecto UI junto a Views, Converters y Resources/Styles. No se crea el árbol de carpetas de la guía de componentes.

Textos del encabezado:

| Vista | Título | Subtítulo |
|---|---|---|
| Nómina | Nómina | Estudiantes inscriptos en la comisión. |
| Seguimiento | Seguimiento | Riesgo de abandono por estudiante, según asistencia y entregas. |
| Entregas | Registrar entregas | Registrá el estado de entrega de cada estudiante. |
| Asistencia | Registrar asistencia | Marcá la asistencia de cada estudiante en la clase elegida. |
| Calendario | Calendario | Clases y trabajos de la comisión. |
| Ajustes | Ajustes | Reglas de cálculo y umbrales de riesgo. |
| Detalle | Detalle del alumno | Nombre Apellido · legajo, cuando el alumno ya cargó |

## Acceptance criteria

- [ ] Fondo, sidebar, texto y acento coinciden con el mock. No queda azul marino (`#0F172A` y vecinos) ni el acento teal.
- [ ] Las siete vistas muestran ese encabezado, con un solo selector de Comisión, los mismos márgenes y el mismo ancho máximo.
- [ ] Colapsar la sidebar anima el ancho, oculta las etiquetas y mantiene la barra del ítem activo. Bajo ~860px arranca colapsada, y cada ícono tiene tooltip.
- [ ] En el detalle del alumno el ítem activo es Seguimiento. La ruta del detalle y la resolución de páginas por DI no cambian.
- [ ] Cambiar la comisión en una pantalla la cambia en las demás y recarga los datos de la pantalla visible. Nómina usa esa comisión, no un id fijo.
- [ ] En Ajustes, cambiar la comisión no cambia los umbrales que se leen ni los que se guardan.
- [ ] La barra de título de Windows es del mismo color que el fondo de la ventana.
- [ ] Los tests de Core y de Data siguen pasando.

## Blocked by

None - can start immediately
