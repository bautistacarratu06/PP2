# Asistencia y entregas: orden de trabajos y nombres de tests

## What to build

Se completan los tests del caso de uso de entregas con el mismo criterio que ya tiene asistencia. Los trabajos de una comisión salen ordenados por vencimiento y después por nombre. El trabajo elegido por defecto sigue siendo el último vencido hasta hoy; si todos son futuros, el primero; si no hay ninguno, ninguno. Guardar dos veces el mismo alumno en el mismo trabajo sigue corrigiendo el registro, no duplicándolo. La pantalla de Entregas no cambia de flujo.

Cobertura del área, sin cambio de comportamiento. Asistencia ya prueba guardado, orden de clases y la clase por defecto. Entregas prueba guardado y el trabajo vencido hasta hoy. Sin test hoy: `SubmissionService.GetAssignmentsAsync` (orden por vencimiento y, en la misma fecha, por nombre). En `PickDefault` de entregas no están el caso “todos futuros → el primero” ni la lista vacía; el de asistencia sí los tiene.

Tests a renombrar (`AttendanceAndSubmissionServiceTests`):

- `Attendance_SaveThenRead_UpsertsPerStudent`
- `Attendance_GetClassSessions_OrderedByDate`
- `Attendance_PickDefault_LatestUpToToday_ElseFirst`
- `Submissions_SaveThenRead_UpsertsPerStudent`
- `Submissions_PickDefault_LatestDueUpToToday`

Al terminar se puede verificar: los tests nuevos pasan y, en Entregas, el picker sigue mostrando los trabajos por fecha de vencimiento, con el último ya vencido seleccionado.

## Acceptance criteria

- [ ] Dados trabajos en distinto orden, al listarlos, quedan por vencimiento y luego por nombre.
- [ ] Dado un trabajo de hoy y otros futuros, al elegir el de por defecto, queda el de hoy.
- [ ] Dados solo trabajos futuros, al elegir el de por defecto, queda el primero por fecha.
- [ ] Dada una lista vacía, al elegir el de por defecto, el resultado es vacío.
- [ ] Dado un segundo guardado del mismo alumno y trabajo, al leer, hay un solo registro con el estado nuevo.
- [ ] Los cinco tests existentes, más los nuevos, se llaman `Given…_When…_Then…`.

## Blocked by

- None - can start immediately
