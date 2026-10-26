# Cada capa registra sus propias dependencias

## What to build

Core expone el alta de sus casos de uso. Data expone el alta de la conexión y de los repositorios que ya existen. Si el puerto de umbrales ya está cuando se hace este corte, el alta de Data lo incluye; si no, este corte no lo agrega. La UI llama esas dos altas y sigue registrando ella los ViewModels y las páginas. Los repositorios quedan singleton. Los casos de uso, los ViewModels y las páginas quedan transitorios. El Shell sigue resolviendo las páginas por el contenedor. No se crean proyectos nuevos de dominio y aplicación.

El registro queda hoy entero en el arranque de la UI. Conviene hacerlo al final, cuando el resumen SQL ya salió y la cadena ya viene de configuración. Este corte registra lo que ya existe y no inventa el puerto de umbrales.

Al terminar se puede verificar: la app arranca y Seguimiento, Calendario y Asistencia cargan. Si el puerto de umbrales ya está registrado, Ajustes sigue leyendo los umbrales; este corte no lo construye.

## Acceptance criteria

- [ ] El arranque de la UI llama las altas de Core y de Data y registra aparte ViewModels y páginas.
- [ ] Repositorios singleton; casos de uso, ViewModels y páginas transitorios.
- [ ] La app arranca. Seguimiento muestra grilla y tarjetas, Calendario carga y Asistencia lista las clases de la comisión. Si el puerto de umbrales ya está registrado, Ajustes sigue leyendo los umbrales; este corte no lo construye.

## Blocked by

- [02 — Seguimiento calcula grilla y tarjetas en un solo caso de uso](02-seguimiento-grilla-y-tarjetas.md)
- [10 — Connection string fuera de la constante de arranque](10-connection-string-fuera-de-la-constante.md)
