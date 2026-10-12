# Consultas por vista

Este documento traduce las operaciones que hoy ejecuta el prototipo con Dexie/IndexedDB al esquema T-SQL de `db/migrations/`. Los nombres de columnas de la base están en inglés y los literales de dominio conservan los valores del prototipo.

## Vista: Panel de seguimiento (`/`)

**Decisión que habilita:** priorizar a quién contactar primero, ordenando por riesgo descendente y permitiendo filtrar por nivel o falta de contacto.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Comisiones | lectura | `components/cursada-app.tsx`, `useData` | `db.comisiones.toArray()` | `SELECT id, name FROM dbo.section ORDER BY name;` |
| Umbrales | lectura | `components/cursada-app.tsx`, `useData` | `db.configuracion.get('umbrales')` | `SELECT setting_key, setting_value FROM dbo.setting WHERE setting_key IN (N'high_threshold', N'medium_threshold');` |
| Estudiantes de la comisión | lectura | `useStudents` | `db.estudiantes.where('comisionId').equals(@section_id).sortBy('apellido')` | `SELECT id, section_id, student_number, last_name, first_name, phone, email FROM dbo.student WHERE section_id = @section_id ORDER BY last_name, first_name, id;` |
| Clases y trabajos | lectura | `Panel` | `db.clases.where('comisionId').equals(@section_id).toArray()` y `db.trabajos.where('comisionId').equals(@section_id).toArray()` | `SELECT id, section_id, session_date FROM dbo.class_session WHERE section_id = @section_id; SELECT id, section_id, name, due_date FROM dbo.assignment WHERE section_id = @section_id;` |
| Asistencias, entregas e intervenciones | lectura | `Panel` | `db.asistencias.toArray()`, `db.entregas.toArray()`, `db.intervenciones.toArray()` | `SELECT ar.* FROM dbo.attendance_record ar JOIN dbo.class_session cs ON cs.id = ar.class_session_id WHERE cs.section_id = @section_id; SELECT su.* FROM dbo.submission su JOIN dbo.assignment a ON a.id = su.assignment_id WHERE a.section_id = @section_id; SELECT i.* FROM dbo.intervention i JOIN dbo.student st ON st.id = i.student_id WHERE st.section_id = @section_id;` |
| Riesgo por estudiante | lectura | `Panel`, `calcularRiesgo` | Filtra arrays por `estudianteId` y calcula en `lib/riesgo.ts` | `SELECT st.id, st.last_name, st.first_name, r.* FROM dbo.student st JOIN dbo.vw_student_risk r ON r.student_id = st.id WHERE st.section_id = @section_id ORDER BY COALESCE(r.risk, -1) DESC, st.last_name, st.first_name;` Aplicar nivel: `CASE WHEN risk >= @high_threshold THEN N'Alto' WHEN risk >= @medium_threshold THEN N'Medio' WHEN risk IS NULL THEN N'Sin datos' ELSE N'Bajo' END`. |

**Tablas que toca:** `section` (L), `student` (L), `class_session` (L), `assignment` (L), `attendance_record` (L), `submission` (L), `intervention` (L), `setting` (L), `vw_student_risk` (L).

La vista `vw_student_risk` implementa la misma fórmula: asistencia 60%, entregas 40%, renormalizando al 100% cuando falta una señal; riesgo = `100 - score`, redondeado.

## Vista: Nómina (`/nomina`)

**Decisión que habilita:** importar estudiantes válidos sin duplicar el legajo dentro de una comisión y consultar la nómina ordenada por apellido.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Nómina | lectura | `Nomina`, `useStudents` | `db.estudiantes.where('comisionId').equals(@section_id).sortBy('apellido')` | `SELECT id, student_number, last_name, first_name, phone, email FROM dbo.student WHERE section_id = @section_id ORDER BY last_name, first_name, id;` |
| Buscar/crear comisión e importar filas | escritura | `Nomina.confirm` | `where('nombre').equals`; si no existe `db.comisiones.add`; luego `where('[comisionId+legajo]').equals`; si no existe `db.estudiantes.add` | Bloque transaccional: `BEGIN TRAN; INSERT INTO dbo.section(name) SELECT @section_name WHERE NOT EXISTS (SELECT 1 FROM dbo.section WHERE name = @section_name); INSERT INTO dbo.student(section_id, student_number, last_name, first_name, phone, email) SELECT s.id, @student_number, @last_name, @first_name, @phone, @email FROM dbo.section s WHERE s.name = @section_name AND NOT EXISTS (SELECT 1 FROM dbo.student x WHERE x.section_id = s.id AND x.student_number = @student_number); COMMIT;` |
| Plantilla CSV | lectura/descarga | `Nomina.download` | No consulta Dexie; genera texto CSV en memoria | No corresponde a una consulta SQL. |

La importación debe procesar cada fila válida dentro de una única transacción global (`BEGIN TRAN`/`COMMIT`). Para rechazos por legajo repetido se recomienda `NOT EXISTS` (la fila se omite de forma idempotente); si se requiere informar el rechazo, registrar las filas omitidas antes de confirmar. En concurrencia, mantener la restricción `uq_student_section_student_number` y capturar el error 2601/2627 con `TRY/CATCH` y `ROLLBACK`.

**Tablas que toca:** `section` (L/E), `student` (L/E).

## Vista: Calendario (`/calendario`)

**Decisión que habilita:** crear una clase por fecha o un trabajo con nombre y vencimiento para habilitar luego la toma de asistencia y entregas.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Clases | lectura | `Calendar` | `db.clases.where('comisionId').equals(@section_id).toArray()` | `SELECT id, section_id, session_date FROM dbo.class_session WHERE section_id = @section_id ORDER BY session_date;` |
| Trabajos | lectura | `Calendar` | `db.trabajos.where('comisionId').equals(@section_id).toArray()` | `SELECT id, section_id, name, due_date FROM dbo.assignment WHERE section_id = @section_id ORDER BY due_date, id;` |
| Agregar clase | escritura | `Calendar.add` | Comprueba `where('[comisionId+fecha]').equals([@section_id,@date]).first()` y luego `db.clases.add` | `INSERT INTO dbo.class_session(section_id, session_date) SELECT @section_id, @session_date WHERE NOT EXISTS (SELECT 1 FROM dbo.class_session WHERE section_id = @section_id AND session_date = @session_date);` |
| Agregar trabajo | escritura | `Calendar.add` | `db.trabajos.add({comisionId,nombre,fechaLimite})` | `INSERT INTO dbo.assignment(section_id, name, due_date) VALUES (@section_id, @name, @due_date);` |

**Tablas que toca:** `class_session` (L/E), `assignment` (L/E).

## Vista: Asistencia (`/asistencia`)

**Decisión que habilita:** marcar presente/ausente para todos los estudiantes de una clase y conservar correcciones posteriores.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Estudiantes | lectura | `Attendance`, `useStudents` | `db.estudiantes.where('comisionId').equals(@section_id).sortBy('apellido')` | `SELECT id, last_name, first_name FROM dbo.student WHERE section_id = @section_id ORDER BY last_name, first_name, id;` |
| Clases | lectura | `Attendance` | `db.clases.where('comisionId').equals(@section_id).toArray()` | `SELECT id, session_date, corrected_at = NULL FROM dbo.class_session WHERE section_id = @section_id ORDER BY session_date;` |
| Registros de una clase | lectura | `Attendance` | `db.asistencias.where('claseId').equals(@class_session_id).toArray()` | `SELECT id, class_session_id, student_id, status, corrected_at FROM dbo.attendance_record WHERE class_session_id = @class_session_id;` |
| Guardar toda la asistencia | escritura | `saveAttendance` en `lib/db.ts` | Por cada estudiante: busca `where('[claseId+estudianteId]').equals`; `update` si existe o `add` si no | Un solo bloque: `BEGIN TRAN; MERGE dbo.attendance_record AS target USING @attendance_rows AS src ON target.class_session_id = src.class_session_id AND target.student_id = src.student_id WHEN MATCHED THEN UPDATE SET status = src.status, corrected_at = SYSDATETIME() WHEN NOT MATCHED THEN INSERT (class_session_id, student_id, status) VALUES (src.class_session_id, src.student_id, src.status); COMMIT;` Validar que cada `student_id` pertenezca a la sección de la clase antes del `MERGE`. |

**Tablas que toca:** `student` (L), `class_session` (L), `attendance_record` (L/E).

## Vista: Entregas (`/entregas`)

**Decisión que habilita:** marcar entregó/no entregó para todos los estudiantes de un trabajo.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Estudiantes | lectura | `Deliveries`, `useStudents` | `db.estudiantes.where('comisionId').equals(@section_id).sortBy('apellido')` | `SELECT id, last_name, first_name FROM dbo.student WHERE section_id = @section_id ORDER BY last_name, first_name, id;` |
| Trabajos | lectura | `Deliveries` | `db.trabajos.where('comisionId').equals(@section_id).toArray()` | `SELECT id, section_id, name, due_date FROM dbo.assignment WHERE section_id = @section_id ORDER BY due_date, id;` |
| Registros de un trabajo | lectura | `Deliveries` | `db.entregas.where('trabajoId').equals(@assignment_id).toArray()` | `SELECT id, assignment_id, student_id, status FROM dbo.submission WHERE assignment_id = @assignment_id;` |
| Guardar todas las entregas | escritura | `saveDeliveries` en `lib/db.ts` | Por cada estudiante: busca `where('[trabajoId+estudianteId]').equals`; `update` si existe o `add` si no | Un solo bloque: `BEGIN TRAN; MERGE dbo.submission AS target USING @submission_rows AS src ON target.assignment_id = src.assignment_id AND target.student_id = src.student_id WHEN MATCHED THEN UPDATE SET status = src.status WHEN NOT MATCHED THEN INSERT (assignment_id, student_id, status) VALUES (src.assignment_id, src.student_id, src.status); COMMIT;` Validar que cada estudiante pertenezca a la sección del trabajo. |

**Tablas que toca:** `student` (L), `assignment` (L), `submission` (L/E).

## Vista: Configuración (`/configuracion`)

**Decisión que habilita:** cambiar los umbrales de clasificación, cargar datos de demostración o borrar el conjunto local.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Leer umbrales | lectura | `Config` | `db.configuracion.get('umbrales')` | `SELECT setting_key, setting_value FROM dbo.setting WHERE setting_key IN (N'high_threshold', N'medium_threshold');` |
| Guardar umbrales | escritura | `Config.save` | `db.configuracion.put({clave:'umbrales',alto,medio})` | `BEGIN TRAN; UPDATE dbo.setting SET setting_value = @high_threshold WHERE setting_key = N'high_threshold'; UPDATE dbo.setting SET setting_value = @medium_threshold WHERE setting_key = N'medium_threshold'; COMMIT;` |
| Cargar demo | escritura | `Config`, `seedDemo` y `addDemoRecords` | Inserta comisiones, estudiantes, clases, trabajos, asistencias y entregas; usa `count`, `add` y `put` | No es una sola consulta en el prototipo. Equivalente: transacción que inserta `section`, `student`, `class_session`, `assignment`, `attendance_record` y `submission`, respetando las FK y usando `NOT EXISTS`/restricciones únicas. |
| Borrar datos | escritura | `Config`, `clearAll` | `clear()` de todas las tablas locales | `BEGIN TRAN; DELETE FROM dbo.intervention; DELETE FROM dbo.submission; DELETE FROM dbo.attendance_record; DELETE FROM dbo.assignment; DELETE FROM dbo.class_session; DELETE FROM dbo.student; DELETE FROM dbo.section; COMMIT;` No borrar `setting` salvo que se quiera restaurar sus valores semilla. |

**Tablas que toca:** `setting` (L/E), `section` (L/E), `student` (L/E), `class_session` (E), `assignment` (E), `attendance_record` (E), `submission` (E), `intervention` (posiblemente L/E según la demo).

## Vista: Ficha del estudiante (`/estudiantes/[id]`)

**Decisión que habilita:** consultar el estado integral de un estudiante y decidir si registrar un nuevo contacto.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Datos del estudiante | lectura | `StudentPage` | `db.estudiantes.get(@student_id)` | `SELECT id, section_id, student_number, last_name, first_name, phone, email, created_at FROM dbo.student WHERE id = @student_id;` |
| Comisión | lectura | `StudentPage` | `db.comisiones.get(s.comisionId)` | `SELECT sec.id, sec.name FROM dbo.section sec JOIN dbo.student st ON st.section_id = sec.id WHERE st.id = @student_id;` |
| Clases y trabajos de la comisión | lectura | `StudentPage` | `where('comisionId').equals(s.comisionId).toArray()` en `clases` y `trabajos` | `SELECT cs.* FROM dbo.class_session cs JOIN dbo.student st ON st.section_id = cs.section_id WHERE st.id = @student_id ORDER BY cs.session_date; SELECT a.* FROM dbo.assignment a JOIN dbo.student st ON st.section_id = a.section_id WHERE st.id = @student_id ORDER BY a.due_date;` |
| Asistencia y entregas | lectura | `StudentPage` | `db.asistencias.where('estudianteId').equals(@student_id).toArray()` y `db.entregas.where('estudianteId').equals(@student_id).toArray()` | `SELECT ar.* FROM dbo.attendance_record ar WHERE ar.student_id = @student_id; SELECT su.* FROM dbo.submission su WHERE su.student_id = @student_id;` |
| Intervenciones | lectura | `StudentPage` | `db.intervenciones.where('estudianteId').equals(@student_id).reverse().sortBy('fecha')` | `SELECT i.* FROM dbo.intervention i WHERE i.student_id = @student_id ORDER BY i.intervention_date DESC, i.id DESC;` |
| Riesgo | lectura | `StudentPage`, `calcularRiesgo` | Filtra las dos colecciones y ejecuta la fórmula pura | `SELECT r.*, CASE WHEN r.risk >= @high_threshold THEN N'Alto' WHEN r.risk >= @medium_threshold THEN N'Medio' WHEN r.risk IS NULL THEN N'Sin datos' ELSE N'Bajo' END AS risk_level FROM dbo.vw_student_risk r WHERE r.student_id = @student_id;` |

**Tablas que toca:** `student` (L), `section` (L), `class_session` (L), `assignment` (L), `attendance_record` (L), `submission` (L), `intervention` (L), `setting` (L), `vw_student_risk` (L).

## Diálogo: Registrar contacto (dentro de la ficha)

**Decisión que habilita:** dejar trazabilidad de una intervención para que el panel pueda ocultar estudiantes ya contactados.

**Operaciones:**

| Operación | Tipo (lectura/escritura) | Dónde está hoy (archivo y función) | Query Dexie actual | Query T-SQL equivalente |
|---|---|---|---|---|
| Guardar contacto | escritura | `ContactDialog.save` | `db.intervenciones.add({fecha, canal, resultado, nota, estudianteId})` | `INSERT INTO dbo.intervention(student_id, intervention_date, channel, outcome, note) VALUES (@student_id, @intervention_date, @channel, @outcome, @note);` |
| Último contacto mostrado en panel | lectura | `Panel` | `db.intervenciones.toArray()` y filtro por estudiante; ordena por `fecha` descendente | `SELECT TOP (1) i.* FROM dbo.intervention i WHERE i.student_id = @student_id ORDER BY i.intervention_date DESC, i.id DESC;` |

**Tablas que toca:** `intervention` (L/E).

## Matriz vista × tabla

| Vista | section | student | class_session | assignment | attendance_record | submission | intervention | setting | vw_student_risk |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Panel de seguimiento | L | L | L | L | L | L | L | L | L |
| Nómina | L/E | L/E | — | — | — | — | — | — | — |
| Calendario | L | — | L/E | L/E | — | — | — | — | — |
| Asistencia | — | L | L | — | L/E | — | — | — | — |
| Entregas | — | L | — | L | — | L/E | — | — | — |
| Configuración | L/E | L/E | E | E | E | E | — | L/E | — |
| Ficha del estudiante | L | L | L | L | L | L | L | L | L |
| Diálogo de registrar contacto | — | L | — | — | — | — | L/E | — | — |

## Consultas repetidas para centralizar

1. `section` por comisión: selector global y listados de comisiones (`useData`, `Nomina`). Convertir en `SectionRepository.list()`.
2. Estudiantes por comisión ordenados por apellido: panel, nómina, asistencia y entregas. Convertir en `StudentRepository.listBySection(@section_id)`.
3. Clases por comisión: panel, calendario y asistencia. Convertir en `ClassSessionRepository.listBySection(@section_id)`.
4. Trabajos por comisión: panel, calendario y entregas. Convertir en `AssignmentRepository.listBySection(@section_id)`.
5. Registros de asistencia por estudiante o clase: panel, ficha y asistencia. Centralizar en consultas parametrizadas con índices sobre `(class_session_id, student_id)` y `student_id`.
6. Entregas por estudiante o trabajo: panel, ficha y entregas. Centralizar con índices sobre `(assignment_id, student_id)` y `student_id`.
7. Riesgo: panel y ficha deben leer `vw_student_risk` en lugar de recalcular en el cliente; los umbrales se aplican en una consulta o función común.
8. Última intervención: panel y ficha. Usar `TOP (1) ... ORDER BY intervention_date DESC, id DESC` en un procedimiento reutilizable.
9. Umbrales: `setting` se lee en el selector global, configuración y resolución de riesgo. Convertir en `SettingRepository.getRiskThresholds()`.

## Faltantes y diferencias detectadas

- El prototipo no guarda `corrected_at`/`corregidaEn` en `class_session` al registrar asistencia; la query propuesta deja el dato preparado en `attendance_record.corrected_at`.
- `StudentPage` carga clases y trabajos, pero la ficha no usa todos esos datos para una consulta independiente visible; se documentan porque sí ejecuta las lecturas.
- La carga de demo no está modelada en una única transacción Dexie; al migrarla a SQL conviene hacerla atómica.
- La base SQL usa `section`, `student` y nombres ingleses, mientras Dexie usa `comisiones`, `estudiantes` y nombres en español. La capa de acceso debe mapear ambos contratos.
- `FALTA EN EL PROTOTIPO`: no existe una consulta específica de “último contacto” en la ficha; el panel sí deriva el último contacto desde todas las intervenciones. La query `TOP (1)` anterior es la propuesta para completar esa historia.
- El diálogo usa el resultado visual `Contactado – se comprometió a volver`, mientras el constraint SQL define `Contactado - se comprometió a volver` con guion ASCII. La capa de persistencia debe normalizar ese valor antes del `INSERT`.
- Para todos los `MERGE` o `INSERT` desde una API, validar pertenencia a la misma comisión y usar parámetros estructurados (`@attendance_rows`, `@submission_rows`) para evitar mezclar estudiantes de otra sección.

## Fórmula de riesgo

La vista `dbo.vw_student_risk` produce proporciones de asistencia y entregas. Si existen ambas señales: `score = asistencia × 0.6 + entregas × 0.4`; si solo existe una, esa señal pesa 100%; luego `risk = ROUND(100 - score, 0)`. El nivel depende de `@high_threshold` y `@medium_threshold`.

Los literales de dominio usados en SQL deben escribirse como `N'presente'`, `N'ausente'`, `N'entregó'` y `N'no entregó'` cuando se expresen filtros explícitos.

## Nota sobre borrado

El prototipo ejecuta `clear()` local sobre todas las colecciones, incluida configuración. En SQL, el orden de borrado debe respetar las claves foráneas; si se desea conservar los umbrales semilla, no se debe borrar `dbo.setting`.

## Nota sobre importación CSV

La plantilla usa las columnas `legajo,apellido,nombre,comision,telefono,correo`. La importación debe validar las cuatro primeras, recortar espacios, resolver/crear la comisión y omitir duplicados por `(section_id, student_number)` dentro de una única transacción. No debe interpolar valores del CSV: todos los valores deben viajar como parámetros.

## Nota sobre tipo de SQL

Las consultas están escritas para SQL Server/T-SQL (`TOP`, `SYSDATETIME`, `DATETIME2`, `MERGE`, variables `@...`).

La documentación no modifica la aplicación ni las migraciones existentes.

## Fin

Este documento cubre las seis rutas principales, la ficha del estudiante y el diálogo de registrar contacto, incluyendo lecturas, escrituras, ordenamientos, fórmula de riesgo, transacciones y diferencias entre Dexie y SQL.
