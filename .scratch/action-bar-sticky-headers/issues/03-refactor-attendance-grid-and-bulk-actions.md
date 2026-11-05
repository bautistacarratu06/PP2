# 03: Refactorización de Asistencia con Acciones en Bloque y Columna de Estado

**What to build:** Migración de la pantalla de Asistencia (`AttendancePage` / `AttendanceViewModel`): eliminación de los switches interactivos fila por fila, incorporación de una columna "Estado" de solo lectura con badge visual, e integración del Action Bar flotante para marcar asistencia masiva ("Marcar como Presente", "Marcar como Ausente") previa confirmación obligatoria en modal.

**Blocked by:** 01: Sticky Header y Selección Múltiple en Grillas, 02: Floating Action Bar y Modal de Confirmación Reutilizable

**Status:** completed

- [x] Se eliminan los `SegmentedControl` por fila en `AttendancePage`.
- [x] Cada fila muestra el estado de asistencia actual mediante una columna de solo lectura ("Presente", "Ausente", o "Sin registro").
- [x] La cabecera de la grilla de asistencia permanece fija al hacer scroll.
- [x] Las filas de asistencia y la cabecera disponen de checkboxes para selección individual y total.
- [x] Al seleccionar 1 o más estudiantes, emerge el Action Bar con las acciones "Presente" y "Ausente".
- [x] Al presionar una acción en el Action Bar, se abre el modal de confirmación informando la acción y la cantidad de estudiantes afectados.
- [x] Confirmar la acción en el modal ejecuta la mutación en la base de datos a través de `AttendanceService.SaveAsync` en lote para todos los estudiantes seleccionados, actualiza la UI y limpia la selección.
- [x] Si se cancela el modal, no se modifican los registros de asistencia.

