# 04: Selección, barra Acciones y modal command palette

## Parent

[Vista unificada de Estudiantes](../../ticket-estudiantes-vista-unificada.md)

## What to build

En Estudiantes: selección múltiple (checkbox por fila + cabecera “todos los visibles”), barra inferior flotante con contador, **Deseleccionar** y único botón primario **Acciones** (solo con ≥1 seleccionado). La selección persiste al filtrar/ordenar y se limpia al cambiar de comisión.

Modal tipo command palette en dos pasos:

1. Búsqueda con foco automático (“Buscar acción…”) y lista filtrable: Gestionar asistencias, Gestionar entregas, Desactivar; teclado (↑↓, Enter, Esc); muestra cuántos hay seleccionados.
2. Formulario de la acción: asistencias (clase + Presente/Ausente), entregas (trabajo + Entregó/No entregó), desactivar (texto de confirmación); **Volver** y **Cancelar** en todos.

Al confirmar: una sola operación por lote (reutilizar comandos existentes cuando existan; desactivar del slice 01), cerrar modal, refrescar tabla (% y riesgo), toast/aviso de éxito. Si falla: transacción sin cambios parciales, modal abierto con error. No reintroducir las action bars viejas de Presente/Ausente ni Entregó/No entregó en esta página.

## Acceptance criteria

- [ ] Checkbox de fila y de cabecera (solo visibles); barra flotante aparece con selección ≥1 y ofrece Deseleccionar + Acciones.
- [ ] La selección sobrevive filtro/orden y se limpia al cambiar comisión.
- [ ] El modal cumple los dos pasos, búsqueda en vivo y navegación por teclado básica.
- [ ] Gestionar asistencias / entregas / desactivar aplican al lote seleccionado en una operación.
- [ ] Éxito cierra el modal, refresca datos (% y riesgo) y muestra aviso; fallo no muta y deja el modal con error.
- [ ] Tests xUnit de los comandos por lote usados desde esta UI (asistencia, entregas, desactivar) si aún no están cubiertos.
- [ ] Build sin warnings nuevos.

## Blocked by

- [02: Página Estudiantes con consulta unificada y tabla](./02-pagina-estudiantes-consulta-y-tabla.md)
- [01: Baja lógica de estudiantes](./01-baja-logica-estudiantes.md) (para la acción Desactivar)
