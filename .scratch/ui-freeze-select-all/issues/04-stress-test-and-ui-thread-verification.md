# 04: Pruebas de estrés y verificación de fluidez del UI Thread

**What to build:** Verificación integral de rendimiento y estabilidad en las grillas de Asistencia y Entregas bajo condiciones de carga (nóminas de 30 o más alumnos), asegurando que la selección múltiple individual y masiva se mantenga a 60 fps sin parpadeos ni bloqueos de hilo principal.

**Blocked by:** 02: Implementar Batch Suppression Flag en AttendanceViewModel, 03: Implementar Batch Suppression Flag en SubmissionsViewModel

**Status:** ready-for-agent

- [ ] La selección masiva con más de 30 estudiantes en Asistencia responde en menos de 100 ms sin bloqueo del hilo principal.
- [ ] La selección masiva con más de 30 estudiantes en Entregas responde en menos de 100 ms sin bloqueo del hilo principal.
- [ ] Deseleccionar todos desde el Action Bar es instantáneo.
- [ ] Seleccionar todas las filas una a una actualiza progresivamente el contador y marca automáticamente el checkbox maestro al seleccionar la última fila.
- [ ] La suite de pruebas de la solución (`scripts/run-tests.cmd`) se mantiene 100% en verde.

