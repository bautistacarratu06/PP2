# Gestión Escolar PP2 · Especificación de rediseño visual

Documento para el agente que implementa el rediseño en el proyecto .NET MAUI (Windows). La **fuente de verdad visual** es el mockup HTML `gestion-escolar-mockup.html`. Abrilo en un navegador y compará cada pantalla con la implementación. Este documento traduce el mockup a valores concretos. Si algo de este texto contradice lo que se ve en el mockup, **gana el mockup**.

El mockup cubre las 7 vistas de la app: Nómina, Seguimiento, Entregas, Asistencia, Calendario, Ajustes y Detalle del alumno. No cambia reglas de negocio, capas ni acceso a datos. Es un rediseño solo de la capa UI.

---

## 1. Principios de diseño

1. **Oscuro neutro.** Negros y grises sin tinte azul. Cero azul marino (el fondo actual `#0F172A` y similares desaparece por completo).
2. **Un solo acento** (violeta). Los colores semánticos (rojo, ámbar, verde) se usan únicamente para riesgo y estados, y siempre en tamaño chico (puntos, pills). Nunca como fondo de una tarjeta grande.
3. **Jerarquía por peso y color de texto**, no por cajas ni bordes gruesos. Tres niveles de texto: primario, secundario, terciario.
4. **Densidad respirable.** Filas de 60–64px, padding generoso, bordes de 1px de bajo contraste.
5. **No repetir información.** Si un dato es igual en todas las filas, va en un detalle o en un encabezado, no en una columna.
6. **Consistencia estructural.** Todas las páginas comparten el mismo shell: título, subtítulo y selector de Comisión.
7. **Sin sombras pesadas ni gradientes decorativos.** La profundidad se logra con 3 niveles de superficie y bordes.

---

## 2. Design tokens

### 2.1 Colores

| Token | Valor | Uso |
|---|---|---|
| `Bg` | `#0A0A0A` | Fondo de ventana y del área de contenido, inputs |
| `Side` | `#0E0E0E` | Fondo de la sidebar |
| `Surface` | `#131313` | Tarjetas, tablas, tarjetas KPI |
| `Raised` | `#1B1B1B` | Hover de filas, ítem activo del menú, chips activos, pista de barras |
| `Border` | `#232323` | Bordes de 1px de tarjetas, separadores de fila |
| `Border2` | `#303030` | Bordes de controles (select de Comisión, botón fantasma, avatar) |
| `TextPrimary` | `#EDEDED` | Títulos, nombres, valores |
| `TextSecondary` | `#8E8E93` | Subtítulos, etiquetas, ítems de menú inactivos |
| `TextTertiary` | `#5C5C61` | Legajos, encabezados de columna, "Sin contactar", metadatos |
| `Accent` | `#9D8CFF` | Iconos activos, punto del selector de Comisión, foco de inputs, progreso, links |
| `AccentStrong` | `#7C6AF2` | Fondo de botones primarios (texto blanco) |
| `AccentSoft` | `#9D8CFF` al 14 % de opacidad | Fondo del tag "Trabajo" |
| `Danger` | `#F87171` | Riesgo alto, "Ausente", "No entregó" |
| `DangerSoft` | `#F87171` al 12 % | Fondo de pill de riesgo alto |
| `Warning` | `#FBBF24` | Riesgo medio |
| `WarningSoft` | `#FBBF24` al 12 % | Fondo de pill de riesgo medio |
| `Success` | `#34D399` | Riesgo bajo, "Presente", "Entregó" |
| `SuccessSoft` | `#34D399` al 12 % | Fondo de pill de riesgo bajo |
| `Scrim` | negro al 50 % | Fondos modales (si se usan) |

**Prohibido:** cualquier color con matiz azul en superficies (nada de `#0F172A`, `#111827`, `#1E293B`, etc.) y el cian/aqua anterior (`#2DD4BF`, `#14B8A6`) como acento.

Para el logo, el gradiente va de `#9D8CFF` a `#6D58E8` a 135°.

### 2.2 Tipografía

- Familia principal: **Inter** (incluir `Regular 400`, `Medium 500`, `SemiBold 600`, `Bold 700` como fuentes embebidas). Fallback: Segoe UI.
- Monoespaciada: **JetBrains Mono** (400), solo para legajos, teléfonos y fechas en listas.
- Activar números tabulares (`font-variant-numeric: tabular-nums`) en puntajes, porcentajes y conteos. En MAUI, si no está disponible, usar la fuente mono o alinear con ancho fijo.

| Rol | Tamaño | Peso | Color | Notas |
|---|---|---|---|---|
| Título de página (H1) | 26 | SemiBold/Bold | Primary | Tracking ligeramente negativo (−0,02 em) |
| Subtítulo de página | 13 | Regular | Secondary | Un renglón, debajo del H1, margen 4 |
| Título de tarjeta (H3) | 15 | SemiBold | Primary | |
| Nombre de estudiante | 14 | SemiBold | Primary | |
| Cuerpo | 13 | Regular | Primary | Texto base |
| Etiqueta de control | 13 | Medium | Secondary | |
| Encabezado de columna | 11 | SemiBold | Tertiary | MAYÚSCULAS, tracking +0,06 em |
| Legajo / teléfono / fecha | 11–12 | Regular mono | Tertiary o Secondary | |
| Número KPI | 34 | SemiBold/Bold | Primary | Tracking −0,03 em, "de N" al lado en 13 Tertiary |
| Puntaje grande (detalle) | 52 | SemiBold/Bold | Color del nivel de riesgo | Tracking −0,04 em |

### 2.3 Espaciado, radios y bordes

- Escala de espaciado: 4, 8, 12, 16, 20, 24, 32, 40.
- Radios: tarjeta y KPI **14**, tarjeta interna (desglose) **12**, controles (input, select, búsqueda) **9–10**, chips internos **6**, ítem de menú **8**, pill y tag **999** (cápsula), avatar y puntos **círculo**.
- Todos los bordes son de **1px**. Ningún borde de 2px ni de color vivo, salvo el estado activo de una tarjeta KPI (ver 4.4).
- Sin sombras. Excepción permitida: ninguna.

### 2.4 Movimiento

- Transiciones de 120–200 ms con ease-out en hover, ancho de la sidebar (200 ms) y progreso.
- Nada de animaciones llamativas, rebotes ni parpadeos.

---

## 3. Estructura de la app

### 3.1 Ventana y barra de título

- La barra de título usa el **mismo color `Bg`** que el contenido (hoy es un gris distinto y desentona). En Windows, configurarlo vía `AppWindow.TitleBar` en un handler (`BackgroundColor`, `InactiveBackgroundColor`, botones transparentes, foreground `TextSecondary`).
- Sin texto "Gestión Escolar PP2" en la barra: el nombre vive en el logo de la sidebar.

### 3.2 Sidebar (barra lateral)

Es lo que más gustó del mockup. Replicarlo con fidelidad.

**Expandida (232px de ancho):**
- Fondo `Side`, borde derecho de 1px `Border`, padding interno 20 arriba / 12 laterales.
- **Marca** arriba: cuadrado de 28×28, radio 8, gradiente del logo, letra "G" blanca en SemiBold 13. A la derecha, "Gestión Escolar" (14 SemiBold) y debajo "PP2 · IFTS" (11, Tertiary). Separación de 10 entre logo y texto. Margen inferior de 24 antes del menú.
- **Ítems del menú:** Nómina, Seguimiento, Entregas, Asistencia, Calendario, Ajustes. Cada uno con **icono de línea de 17px** (trazo 1,7, extremos redondeados, estilo Lucide) + etiqueta (13 Medium). Alto aproximado 38 (padding 9/10), gap 12 entre icono y texto, radio 8, separación vertical de 2 entre ítems.
  - Iconos: Nómina = users, Seguimiento = activity, Entregas = inbox, Asistencia = check-circle, Calendario = calendar, Ajustes = sliders.
  - Inactivo: texto `TextSecondary`. Hover: fondo `Surface`, texto `TextPrimary`.
  - **Activo:** fondo `Raised`, texto `TextPrimary`, icono en `Accent`, y una **barra indicadora de 3px** pegada al borde izquierdo de la sidebar (alto = alto del ítem menos 9 arriba y abajo, esquinas derechas redondeadas de 3).
  - El detalle del alumno mantiene activo "Seguimiento".
- **Pie:** botón "Colapsar" con icono chevron-izquierda (17px) alineado a los ítems, separado del menú por un borde superior de 1px `Border`, siempre anclado abajo. Color `TextTertiary`, hover `TextPrimary`.

**Colapsada (64px de ancho):**
- Solo iconos centrados, padding 16/8. Se oculta el texto de la marca, las etiquetas y la palabra "Colapsar". El chevron se **gira 180°**. La barra indicadora se mueve para seguir pegada al borde.
- Cada ítem muestra su nombre como tooltip.
- Transición de ancho de 200 ms.
- En ventanas angostas (menos de ~860px) arranca colapsada.

### 3.3 Shell de página (componente común, obligatorio en todas las vistas)

Crear **un único componente reutilizable** (`PageShell`, un `ContentView` o `ControlTemplate`) que usen **todas** las páginas. Contiene:

1. **Título (H1)** a la izquierda y, debajo, el **subtítulo** (una línea).
2. **Selector de Comisión** alineado a la derecha del encabezado, **en todas las páginas, sin excepción** (Nómina, Seguimiento, Entregas, Asistencia, Calendario, Ajustes, Detalle del alumno).
3. El contenido de la página debajo, con 28px de separación.

Reglas del shell:
- Contenedor de contenido con **ancho máximo 1160px**, centrado horizontalmente, padding **36 arriba, 40 laterales, 56 abajo** (24/18 laterales en ventanas angostas). Todas las páginas comparten exactamente los mismos márgenes (hoy arrancan en posiciones distintas).
- El encabezado usa disposición horizontal con alineación inferior; si no cabe, el selector pasa abajo.
- La **Comisión es una sola para toda la ventana**. Cambiarla en una página la cambia en todas y recarga los datos de la vista actual. No hay un selector distinto por pantalla. Las pantallas que ya recargan al cambiar de comisión siguen usando esa selección; no se inventa un tipo nuevo de comisión. Nómina deja de fijar la comisión 1. En Ajustes el selector se muestra, y los umbrales siguen siendo globales.
- Cada página aporta solo: título, subtítulo y su cuerpo. Los controles propios de la página (fecha de clase, trabajo, búsqueda, filtros) viven **dentro de la barra de herramientas de la tarjeta principal**, nunca en el encabezado.

**Selector de Comisión (componente destacado, "clave"):** botón/combo con fondo `Raised`, borde 1px `Border2`, radio 10, padding 9/14. Contiene, en orden: un **punto de 8px en `Accent`**, la etiqueta "Comisión" (13 Medium Secondary) y el valor seleccionado (13 Medium Primary) con chevron. Debe verse más prominente que los demás controles. No usar el ComboBox/Picker por defecto con label flotante.

Textos de cada página:

| Página | Título | Subtítulo |
|---|---|---|
| Nómina | Nómina | Estudiantes inscriptos en la comisión. |
| Seguimiento | Seguimiento | Riesgo de abandono por estudiante, según asistencia y entregas. |
| Entregas | Registrar entregas | Registrá el estado de entrega de cada estudiante. |
| Asistencia | Registrar asistencia | Marcá la asistencia de cada estudiante en la clase elegida. |
| Calendario | Calendario | Clases y trabajos de la comisión. |
| Ajustes | Ajustes | Reglas de cálculo y umbrales de riesgo. |
| Detalle | Detalle del alumno | Nombre Apellido · LEG-XXXX |

Se eliminan los "eyebrows" en mayúsculas teal (ENTREGAS, ASISTENCIA, CALENDARIO, CONFIGURACIÓN) y los títulos distintos ("Reglas y datos", "Clases y trabajos").

---

## 4. Componentes

### 4.1 Tarjeta (`Card`)
Fondo `Surface`, borde 1px `Border`, radio 14, contenido recortado al radio. Variante `pad` con padding 20 (24 en Ajustes). Implementar con `Border` + `StrokeShape="RoundRectangle 14"`. **No usar `Frame`.**

### 4.2 Barra de herramientas de tarjeta (`Toolbar`)
Primera fila dentro de una tarjeta: padding 14/16, borde inferior 1px `Border`, elementos en fila con gap 12 y ajuste a la siguiente línea si falta espacio. Contiene búsqueda, chips, selectores propios de la página y el conteo.

### 4.3 Búsqueda
Fondo `Bg`, borde 1px `Border`, radio 9, padding 7/11, icono de lupa de 15px en `TextTertiary` y campo de texto sin borde. En foco, el borde pasa a `Accent`. Ocupa el espacio libre (ancho mínimo 180). Placeholder: "Buscar por nombre o legajo".

### 4.4 Tarjetas KPI (Seguimiento)
Tres tarjetas en grilla de 3 columnas, gap 16. Fondo `Surface`, borde 1px `Border`, radio 14, padding 18/20. Estructura vertical: fila con **punto de 7px** del color del nivel + etiqueta "Riesgo alto/medio/bajo" (13 Medium Secondary); debajo el número grande (34) y "de N" (13 Tertiary). **No usar fondos teñidos ni bordes de color en reposo.**
- Hover: borde `Border2`.
- **Activa (filtro aplicado):** borde 1px del color del nivel y un degradado muy sutil de arriba hacia abajo (color del nivel al 12 % → transparente al 70 %).
- Son clicables y funcionan como filtro de la tabla; un segundo clic lo quita.

### 4.5 Chips segmentados (filtro de nivel)
Contenedor con fondo `Bg`, borde 1px `Border`, radio 9, padding 3, gap 4. Cada chip: padding 5/12, radio 6, 13 Medium Secondary. Activo: fondo `Raised`, texto Primary. Opciones: Todos · Alto · Medio · Bajo.

### 4.6 Pill de nivel de riesgo
Cápsula con fondo `*Soft` del nivel, punto de 6px del color pleno, texto del nivel (12 SemiBold, color pleno) y, a continuación, el **puntaje** (Primary al ~85 % de opacidad, tabular). Ejemplo: `● Alto 100`. Reemplaza las columnas "Nivel" y "Riesgo".

### 4.7 Medidor (barra de progreso con porcentaje)
Barra de **5px** de alto, radio completo, pista `Raised`, relleno `TextSecondary` (gris, no de color), ancho máximo 110; a la derecha el porcentaje (13, Secondary, tabular, ancho fijo 34). Se usa para Asistencia y Entregas en la tabla de Seguimiento.

### 4.8 Avatar
Círculo de 34px (56 en contextos grandes), fondo `Raised`, borde 1px `Border2`, iniciales (nombre + apellido) en 12 SemiBold Primary. Sin imágenes ni íconos de persona violetas.

### 4.9 Fila de estudiante (celda de identidad)
Avatar + bloque vertical: "Apellido, Nombre" (14 SemiBold) y debajo el legajo **sin el prefijo "Legajo"**, solo `LEG-1015` en mono 11 Tertiary.

### 4.10 Control segmentado de estado (Entregas y Asistencia)
Reemplaza el dropdown de cada fila. Contenedor igual al de los chips, con dos botones. No existe el estado "Tarde".
- Asistencia: **Presente** (verde) · **Ausente** (rojo).
- Entregas: **Entregó** (verde) · **No entregó** (rojo).
Sin selección, ambos en gris (`TextSecondary`), que equivale a "Sin registro". Al elegir uno, ese botón toma fondo `*Soft` y texto del color pleno. Volver a tocar el mismo botón lo deselecciona solo si el guardado actual admite vacío; si no, queda el último estado guardado.

### 4.11 Barra de progreso de registro
Barra de 5px de alto y 140 de ancho, pista `Raised`, relleno `Accent`, con el texto "X de N registrados" (13, Tertiary) al lado. Sirve de feedback en Entregas y Asistencia.

### 4.12 Botones
- **Primario:** fondo `AccentStrong`, texto blanco, 13 SemiBold, radio 10, padding 11/14 (versión compacta 8/16). Hover: aumentar brillo ~8 %.
- **Fantasma:** fondo transparente, borde 1px `Border2`, texto Primary, radio 8, padding 6/12; hover fondo `Raised`. Ej.: "Marcar todos presentes".
- **Botón de volver:** texto Secondary con chevron-izquierda de 16px, sin fondo ni borde.

### 4.13 Inputs y selects de formulario
Fondo `Bg`, borde 1px `Border`, radio 9, padding 8/11, texto Primary, foco con borde `Accent`. Etiquetas sobre el campo (13 Medium Secondary, gap 6). Inputs numéricos con límites 0–100.

### 4.14 Tag (tipo de evento / pesos)
Cápsula 12 SemiBold, padding 3/10. "Clase" = texto `TextSecondary` sobre `Raised`. "Trabajo" = texto `Accent` sobre `AccentSoft`. Los pesos de la fórmula usan texto Primary sobre `Raised`.

### 4.15 Fila de historial
Fila con texto a la izquierda y pill de estado a la derecha, padding vertical 12, separador inferior 1px `Border` (sin separador en la última).

---

## 5. Pantallas

### 5.1 Nómina
- Shell con título y subtítulo (ver 3.3).
- Una `Card` con toolbar (búsqueda + conteo "N estudiantes" a la derecha, Tertiary) y tabla.
- Columnas: **Apellido y nombre** (con avatar) · **Legajo** (mono) · **Teléfono** (mono, Secondary) · **Correo** (Tertiary).
- Encabezado de columna de 38px de alto con borde inferior. Filas de **64px**, separador 1px `Border`, hover `Raised`, cursor de mano.
- Clic en una fila → Detalle del alumno.
- **Bug a corregir:** hoy el contador dice "0 estudiantes" con la lista llena. Debe reflejar el conteo real filtrado.
- El `CollectionView` debe scrollear con la ventana (sin scroll interno dentro de la tarjeta).

### 5.2 Seguimiento
- Shell.
- Fila de 3 KPI (4.4).
- `Card` con toolbar (búsqueda, chips de nivel, conteo) y tabla.
- Columnas (5, no 8): **Estudiante** · **Riesgo** (pill con puntaje) · **Asistencia** (medidor) · **Entregas** (medidor) · **Contacto**.
- Se **elimina la columna "Detalle que justifica el valor"** y la columna "Nivel" separada: ese contenido se ve en el Detalle del alumno.
- "Contacto" muestra "Sin contactar" en `TextTertiary`, o la fecha relativa ("Ayer", "Hace 3 días") en Primary.
- Orden por riesgo descendente. Clic en fila → Detalle.

### 5.3 Entregas
- Shell (título "Registrar entregas").
- `Card` con toolbar: selector de **Trabajo** (mismo estilo que el select del toolbar, etiqueta "Trabajo"), barra de progreso de registro y texto "X de N registrados".
- Lista ordenada alfabéticamente: avatar + "Apellido, Nombre" a la izquierda, **control segmentado** (4.10) a la derecha. Filas de 60px, separador 1px.
- Opciones del selector de trabajo: "TP1 - Fundamentos · 10/09/2026", "TP2 - Base de Datos · 20/09/2026".

### 5.4 Asistencia
- Igual que Entregas, con selector de **Clase** (fechas) y el botón fantasma **"Marcar todos presentes"** alineado a la derecha del toolbar.

### 5.5 Calendario
- Shell.
- **Tarjeta de alta:** una fila con selector de tipo (Clase / Trabajo), campo de nombre con placeholder "La fecha identifica la clase" (ocupa el espacio libre), selector de fecha y botón primario compacto "Agregar". Se respetan las reglas de alta que ya tiene la pantalla: una clase se identifica por la fecha, y un trabajo exige el nombre que el servicio ya valida.
- **Tarjeta de lista** debajo (margen 16): filas de 54px ordenadas por fecha ascendente, con nombre en SemiBold a la izquierda, tag de tipo y fecha en mono a la derecha (ancho fijo 110, alineada a la derecha). Se elimina el gran espacio vacío: la tarjeta crece según el contenido.

### 5.6 Ajustes
- Shell.
- Una `Card` de **ancho máximo 640** con padding 24:
  - H3 "Umbrales de riesgo" + descripción (Secondary): "Alto si el riesgo es igual o mayor al primer valor; Medio desde el segundo."
  - Dos inputs numéricos en grilla de 2 columnas (Alto, Medio) con etiquetas encima.
  - Botón primario compacto "Guardar umbrales" y, al lado, un texto de confirmación ("Guardado") en Tertiary.
  - Separador de 1px y bloque "Fórmula": texto "Score = asistencia × 60 % + entregas × 40 %. Riesgo = 100 − score." y dos tags: "60 % asistencia", "40 % entregas".
- Al guardar, se usa el guardado de umbrales que ya existe. Los pesos de la fórmula se muestran, no se editan.

### 5.7 Detalle del alumno
- Shell con título "Detalle del alumno" y subtítulo "Nombre Apellido · LEG-XXXX". Sobre el shell, botón de volver "‹ Seguimiento" (sin la flecha en la barra de título).
- Grilla de dos columnas: **izquierda fija de 300px** y derecha flexible, gap 16, alineadas arriba. En ventanas angostas pasa a una columna.
- **Columna izquierda (`Card` con padding 20):**
  - Puntaje grande (52) en el color del nivel y a su derecha la pill del nivel (sin puntaje).
  - Dos tarjetas internas (radio 12, fondo `Surface`, borde 1px, padding 14/16, margen superior 12): "Asistencia — X de N · P %" y "Entregas — X de N · P %", cada una con barra de 6px debajo.
  - Bloques "TELÉFONO" y "CORREO": etiqueta en 11 MAYÚSCULAS Tertiary y valor en `Accent`, con corte de línea seguro para correos largos.
- **Columna derecha (tres `Card` apiladas, gap 16, padding 20):**
  - **Historial de asistencia:** filas "Clase · dd/MM/yyyy" + pill Presente/Ausente.
  - **Historial de entregas:** filas "TP… · vence dd/MM/yyyy" + pill Entregó/No entregó.
  - **Intervenciones:** H3 y, debajo, la lista que ya se carga, o el texto "Sin contactos registrados." en Tertiary. No hay alta de contacto: la app no tiene un comando para crear una intervención.

---

## 6. Estados e interacción

- **Hover de fila:** fondo `Raised`, transición de 120 ms. En MAUI: `VisualStateManager` con `PointerOver` sobre el `Grid` de la fila.
- **Foco de teclado:** borde `Accent` en controles de texto; mantener el foco visible en botones.
- **Selección/activo:** fondo `Raised` (menú, chips) o color semántico suave (controles segmentados).
- **Vacío:** texto centrado en `TextTertiary` con padding 48 ("Sin resultados para esa búsqueda.").
- **Cursor:** mano en filas clicables, tarjetas KPI, chips y botones.
- **Escape / atrás:** el botón de volver del detalle regresa a Seguimiento conservando filtros si es posible.
- Cambiar la **Comisión** vuelve a cargar la vista actual y conserva la página.

---

## 7. Responsive y ventana

- Ancho mínimo de ventana recomendado: 900px.
- Bajo ~860px: sidebar colapsada por defecto, KPI en una columna, detalle en una columna, tablas con scroll horizontal dentro de su tarjeta (ancho mínimo de la grilla de tabla ~760px).
- El contenido nunca debe provocar scroll horizontal de la página entera.

---

## 8. Datos y lógica reflejados en la UI

- Riesgo = `60 × (1 − asistencia) + 40 × (1 − entregas)`, redondeado, de 0 a 100.
- Nivel: **Alto** si riesgo ≥ umbral alto (50 por defecto), **Medio** si ≥ umbral medio (25), si no **Bajo**.
- Asistencia y entregas se muestran como `X de N` y porcentaje. El mockup de ejemplo usa 3 clases y 2 trabajos, igual que la base de pruebas.
- Los datos del mockup son ficticios y variados a propósito. En la app real vendrán de `Core`.

---

## 9. Guía de implementación en .NET MAUI

Los controles nuevos viven en el proyecto UI, junto a Views, ViewModels, Converters y Resources/Styles. No se crea un árbol de carpetas nuevo. `Colors.xaml` y `Styles.xaml` ya existen: ahí van los tokens y los estilos con nombre.

1. **Recursos:** reemplazar los tokens de `Colors.xaml` por los de 2.1 (colores y variantes `*Soft` con alfa) y sumar en `Styles.xaml` los estilos con nombre: `Card`, `CardPad`, `PageTitle`, `PageSubtitle`, `Caption`, `ColumnHeader`, `Mono`, `Pill`, `Tag`, `PrimaryButton`, `GhostButton`, `Field`, `SegmentButton`, `NavItem`.
2. **Fuentes:** registrar Inter (400/500/600/700) y JetBrains Mono en `MauiProgram` con `ConfigureFonts` y usarlas vía `FontFamily`.
3. **Shell de navegación:** sidebar propia (no la del `Shell` por defecto) con dos estados (expandido/colapsado) controlados por una propiedad `IsCollapsed`, animando `WidthRequest` de 232 a 64. Iconos como `PathGeometry`/SVG o una fuente de iconos; el indicador activo es un `BoxView` o `Border` de 3px. Las rutas y la resolución de páginas por DI no cambian.
4. **Encabezado común:** un solo contenedor de página con título, subtítulo y el selector de Comisión. Todas las páginas lo usan. Evitar que cada página defina su propio encabezado.
5. **Comisión seleccionada:** una sola para la ventana. Las pantallas que ya recargan al cambiar de comisión se enganchan a esa selección. No se crea un tipo de comisión distinto del que ya usa la app.
6. **Selector de Comisión:** control propio (`Border` + `Picker` o `Popup`), con el punto de acento. No usar el `Picker` por defecto con título flotante.
7. **Tablas:** `CollectionView` con `ItemTemplate` basado en `Grid` con `ColumnDefinitions` proporcionales (p. ej. `2.2*,110,1.3*,1.3*,110` en Seguimiento) y una fila de encabezado aparte con el mismo `Grid`. Los separadores son un `BoxView` de 1px o el borde inferior del `Border` de cada fila.
8. **Barras:** `ProgressBar` con `ProgressColor` y altura 5, o un `Grid` con dos `BoxView` si se necesita redondeo exacto.
9. **Barra de título (Windows):** handler que aplique `Bg` a la barra de título y botones transparentes.
10. **Convertidores:** `RiskLevelToColorConverter`, `RiskLevelToSoftColorConverter`, `PercentToProgressConverter`.
11. **Pruebas:** no tocar `Core` ni `Data`. Los tests xUnit existentes deben seguir pasando.
12. **Orden sugerido:** tokens y estilos → `PageShell` + sidebar → Nómina → Seguimiento → Detalle → Entregas/Asistencia → Calendario → Ajustes.

---

## 10. Qué NO hacer

- No usar azules ni el cian/aqua anterior en ninguna superficie o acento.
- No usar fondos teñidos grandes para riesgo; solo puntos y pills.
- No repetir el texto de detalle de riesgo en cada fila.
- No mezclar colores de barra de título y de contenido.
- No usar `Frame`, sombras, bordes de más de 1px ni dropdowns por fila para estados.
- No dejar una página sin selector de Comisión ni sin título y subtítulo.
- No cambiar nombres de campos, reglas ni acceso a datos.
- No agregar el estado "Tarde", ni un alta de intervenciones, ni una galería de componentes, ni un drawer de detalle.
- No crear carpetas Foundation, Layout, Navigation, Inputs, Display, Tables ni Gallery.

---

## 11. Checklist de aceptación (comparar con el mockup)

- [ ] Paleta completamente neutra (sin azules) y acento violeta en iconos activos, foco, progreso y botones.
- [ ] Sidebar con iconos, indicador activo de 3px, botón Colapsar y modo de 64px con tooltips.
- [ ] Las 7 vistas usan el mismo `PageShell` con título, subtítulo y selector de Comisión a la derecha.
- [ ] Mismos márgenes y ancho máximo (1160) en todas las vistas.
- [ ] Barra de título del mismo color que el fondo.
- [ ] Nómina: conteo correcto, legajo sin prefijo, filas de 64px.
- [ ] Seguimiento: KPI sobrios y clicables, 5 columnas, pill con puntaje, medidores, sin columna de detalle.
- [ ] Entregas y Asistencia: control segmentado, barra de progreso, selector propio en el toolbar, "Marcar todos presentes".
- [ ] Calendario: alta en una fila y lista ordenada sin espacio vacío.
- [ ] Ajustes: tarjeta de 640px, umbrales funcionales, fórmula con tags.
- [ ] Detalle: columna izquierda de 300px, tres tarjetas a la derecha, intervenciones de solo lectura.
- [ ] Estados hover, foco y vacío implementados.
- [ ] Capturas de cada pantalla comparadas lado a lado con el mockup.