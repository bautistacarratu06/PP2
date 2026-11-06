# 05: Retirar páginas antiguas y unificar menú

## Parent

[Vista unificada de Estudiantes](../../ticket-estudiantes-vista-unificada.md)

## What to build

Con Estudiantes ya usable (tabla, filtros y Acciones): eliminar las páginas y ViewModels de Nómina, Seguimiento, Entregas y Asistencia; dejar una sola entrada **Estudiantes** en el menú lateral junto a Calendario y Ajustes; limpiar rutas Shell, registros DI y navegación residual (p. ej. “volver a Seguimiento” desde detalle). Calendario y Ajustes no se tocan. Verificar build, tests y que no queden warnings nuevos por tipos huérfanos.

## Acceptance criteria

- [ ] Ya no existen (o no se registran/navegan) las cuatro vistas antiguas ni sus ViewModels dedicados.
- [ ] El menú lateral muestra Estudiantes, Calendario y Ajustes (sin Nómina/Seguimiento/Entregas/Asistencia).
- [ ] La navegación de detalle u otras pantallas no apunta a rutas eliminadas.
- [ ] “Cargar estudiante” y “Cargar comisión” siguen disponibles desde Estudiantes.
- [ ] `dotnet build` de UI y `scripts\run-tests.cmd` pasan; sin warnings nuevos atribuibles a este cambio.
- [ ] Criterio global del epic: una sola página para ver y gestionar estudiantes.

## Blocked by

- [03: Filtros de cabecera, orden y atajos de riesgo](./03-filtros-orden-y-atajos-riesgo.md)
- [04: Selección, barra Acciones y modal command palette](./04-seleccion-acciones-y-command-palette.md)
