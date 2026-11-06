# 03: Filtros de cabecera, orden y atajos de riesgo

## Parent

[Vista unificada de Estudiantes](../../ticket-estudiantes-vista-unificada.md)

## What to build

Sobre la página Estudiantes: cabeceras filtrables (título, indicador de orden, icono de filtro → popover), combinación AND de filtros, orden por defecto apellido+nombre ascendente, botón **Limpiar filtros** cuando hay alguno activo, e indicador visual en cabeceras con filtro activo.

Filtros por columna según spec: Riesgo (multi Alto/Medio/Bajo); Asistencia y Entregas (rango % min/max); Contacto (multi estados); Estado (Activos por defecto / Inactivos / Todos); texto contiene en Estudiante, Legajo y Correo. Conservar las tarjetas de riesgo arriba de la tabla como atajo que aplica/quita el filtro de Riesgo. El contador “N estudiantes” y el buscador siguen reflejando el resultado filtrado. La lógica de filtro/orden puede vivir en el ViewModel y/o en el caso de uso de consulta; tests Core para filtros y orden.

## Acceptance criteria

- [ ] Cada cabecera relevante abre un popover de filtro y muestra indicador cuando el filtro está activo.
- [ ] Los filtros se combinan con AND; el orden por defecto es apellido y nombre ascendente; se puede reordenar desde cabeceras.
- [ ] Estado por defecto: solo Activos.
- [ ] Existe **Limpiar filtros** solo (o claramente habilitado) cuando hay filtros activos, y al usarlo vuelve al default coherente.
- [ ] Clic en tarjeta Alto/Medio/Bajo aplica o quita ese valor en el filtro de Riesgo.
- [ ] Contador y buscador operan sobre el resultado filtrado.
- [ ] Tests xUnit cubren filtros y orden del caso de uso / reglas de filtrado.
- [ ] Build sin warnings nuevos.

## Blocked by

- [02: Página Estudiantes con consulta unificada y tabla](./02-pagina-estudiantes-consulta-y-tabla.md)
