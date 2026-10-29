# Mission: Dominio y persistencia de GestionEscolar

## Why
Necesitás leer y tocar el código de GestionEscolar (Core, Data, EF) sin trabarte: entidades vs filas, relaciones, asistencia/`corrected_at`, migraciones y `RiskRule`, para poder contribuir y debuggear con criterio.

## Success looks like
- Distinguir modelos de dominio (Core) de entidades EF (Data) y decir quién traduce
- Explicar 1:N comisión→alumno y el puente `attendance_record` / `submission`
- Describir el ciclo corto de `IDbContextFactory` + `SaveChangesAsync` y cuándo se setea `corrected_at`
- Saber qué va en migraciones EF vs scripts históricos, y qué hace `DbStatusValues` / `RiskRule`

## Constraints
- Español rioplatense en la UI de las lecciones
- Lecciones cortas, con quiz interactivo (retrieval) encima del material ya visto
- Workspace en `.scratch/` para no mezclar con el código de producto

## Out of scope
- UI MAUI / XAML en profundidad
- Diseño visual del producto
- Reescribir el schema o el dominio
