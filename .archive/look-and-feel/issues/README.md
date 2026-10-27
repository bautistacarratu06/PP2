# Revamp visual — historias

Cada archivo es una tajada vertical: se puede abrir la app y comparar esa superficie con `Gestión Escolar · Mockup.html`. La apariencia la define el HTML. `ui-revamp.md` aporta medidas y textos; si se contradicen, gana el HTML.

Los controles reutilizables se agregan en el proyecto UI, junto a lo que ya existe (`Views`, `ViewModels`, `Converters`, `Resources/Styles`). No se crean carpetas Foundation, Layout, Navigation, Inputs, Display, Tables ni Gallery.

No se toca Core, Data ni el esquema. Los tests actuales tienen que seguir pasando.

## Qué no entra, porque la app no lo tiene

El mock y las guías quedaron alineados con esto:

- No existe el estado **Tarde**. Asistencia es Presente / Ausente; entregas es Entregó / No entregó. Sin selección sigue siendo «Sin registro».
- No hay alta de contacto. El detalle muestra las intervenciones que ya se leen.
- No hay drawer. El detalle es la página que ya se abre desde la grilla.
- No hay galería de componentes ni un árbol de carpetas nuevo.
- Los umbrales de Ajustes son globales. El selector de Comisión se muestra igual, pero no parte los umbrales por comisión.
- La referencia es un solo tema oscuro. No se diseña una paleta clara paralela, ni se conserva el azul marino ni el acento teal.

## Orden

| # | Historia | Bloqueada por |
|---|---|---|
| 01 | [Marco de la app](01-marco-de-la-app.md) | — |
| 02 | [Nómina](02-nomina.md) | 01 |
| 03 | [Seguimiento](03-seguimiento.md) | 02 |
| 04 | [Detalle del alumno](04-detalle-del-alumno.md) | 03 |
| 05 | [Entregas](05-entregas.md) | 02 |
| 06 | [Asistencia](06-asistencia.md) | 05 |
| 07 | [Calendario](07-calendario.md) | 01 |
| 08 | [Ajustes](08-ajustes.md) | 07 |

02 y 07 pueden hacerse en paralelo después del marco. 05 espera a 02 porque reutiliza la fila de estudiante, la tarjeta y la barra de herramientas.
