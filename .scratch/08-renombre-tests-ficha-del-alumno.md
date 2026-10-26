# Renombre de tests de la ficha del alumno

## What to build

Se renombran esos siete tests sin cambiar lo que afirman. Sigue valiendo: la ficha de la comisión pedida trae asistencia, entregas e intervenciones de esa comisión; sin comisión usa la del alumno; un alumno sin registros devuelve listas vacías; un id desconocido devuelve vacío; `presente`/`ausente` y `entregó`/`no entregó` se traducen con el enum; la descripción de la intervención con nota incluye la nota. La pantalla de detalle no se modifica.

Solo el esquema de nombres. `StudentDetailsService.GetAsync`, `DbStatusValues.ParseAttendanceStatus`, `DbStatusValues.ParseSubmissionStatus` e `Intervention.Description` (con nota) ya tienen test. `ToDbValue` se ejecuta desde esos parseos; no hace falta un caso nuevo.

Tests a renombrar (`StudentDetailsServiceTests`):

- `GetAsync_ComposesHistoriesForRequestedSection`
- `GetAsync_WithoutSection_UsesStudentSection`
- `GetAsync_StudentWithoutRecords_ReturnsEmptyLists`
- `GetAsync_UnknownStudent_ReturnsNull`
- `ParseAttendanceStatus_MapsDbValues`
- `ParseSubmissionStatus_MapsDbValues`
- `Intervention_Description_IncludesNote`

Al terminar se puede verificar: la suite de la ficha pasa y el detalle de un alumno en la app se ve igual.

## Acceptance criteria

- [ ] Los siete tests conservan sus afirmaciones actuales.
- [ ] Cada nombre es `Given…_When…_Then…`.
- [ ] Siguen usando el repositorio falso.

## Blocked by

- None - can start immediately
