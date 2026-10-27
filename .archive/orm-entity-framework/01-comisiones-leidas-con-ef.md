# Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado

## What to build

El selector de comisión sigue listando las comisiones de `course_tracking`, leídas con Entity Framework. La connection string sigue siendo la de la aplicación (`ConnectionStrings:CourseTracking`).

El ORM vive en la capa de datos. Core no lo referencia: los modelos de dominio siguen siendo los de siempre y el repositorio los arma al leer. Cada lectura abre su propio contexto y lo cierra al terminar.

El snapshot inicial describe el esquema que ya existe, con este mapa, para que una migración posterior no intente «corregir» claves ni tipos:

- `section`: `id` identity y clave, `name` único de hasta 20 caracteres.
- `student`: clave primaria `dni`; `id` identity es único y es el destino de las claves foráneas de asistencia, entregas e intervenciones. Legajo único por comisión. `created_at` con default.
- `class_session`: fecha única por comisión.
- `assignment`: nombre de hasta 120 caracteres, fecha sin hora.
- `attendance_record`: único por clase y alumno; `status` solo `presente` o `ausente`; `corrected_at` nulo.
- `submission`: único por trabajo y alumno; `status` solo `entregó` o `no entregó`.
- `intervention`: canal y resultado limitados a los valores que ya acepta la tabla; la fecha tiene default.
- `setting`: clave de texto, valor entero entre 0 y 100. Los umbrales `high_threshold` (50) y `medium_threshold` (25) forman parte del modelo.

`vw_student_risk` queda fuera: ninguna pantalla la lee. Los scripts SQL ya aplicados no se editan. En la base que ya tiene estas tablas, aplicar migraciones no crea, no altera y no borra objetos, y no inserta otra fila de umbral.

Las demás pantallas siguen con el acceso SQL actual. Este corte no agrega columnas.

Al terminar se puede verificar: el selector muestra las mismas comisiones que antes, y aplicar las migraciones de EF sobre `course_tracking` deja las filas como estaban.

## Acceptance criteria

- [ ] El selector de comisión lista las mismas comisiones, leídas con Entity Framework.
- [ ] Core no referencia el ORM. Cada operación abre un contexto y lo cierra al terminar.
- [ ] El snapshot cubre las ocho tablas de arriba, con las claves, uniques, checks, defaults y la clave de `student` tal como están hoy.
- [ ] `vw_student_risk` no está en el modelo. Los scripts SQL ya aplicados no se modifican.
- [ ] Sobre la base existente, aplicar migraciones no crea, no altera y no borra tablas, y no duplica los umbrales.
- [ ] Nómina, calendario, asistencia, entregas, ficha, seguimiento y ajustes siguen con el acceso SQL actual.
- [ ] Los tests de Core siguen en verde.

## Blocked by

None - can start immediately
