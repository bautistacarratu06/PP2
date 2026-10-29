# GestionEscolar — Resources

## Knowledge

- [Repo: `AGENTS.md`](../../AGENTS.md)
  Reglas de capas, EF, migraciones y convenciones del proyecto. Use for: Core vs Data, dónde van los cambios.
- [Repo: `GestionEscolar/ARCHITECTURE.md`](../../GestionEscolar/ARCHITECTURE.md)
  Capas, flujos y convenciones estructurales. Use for: dependencias UI → Data → Core.
- [Repo: `README.md`](../../README.md)
  Modelo de dominio y hooks. Use for: mapa de entidades y conceptos de negocio.
- [Código: `CourseTrackingDbContext.cs`](../../GestionEscolar/GestionEscolar.Data/Persistence/CourseTrackingDbContext.cs)
  Fuente primaria del mapeo EF (PK/AK, FKs, nombres de tabla). Use for: relaciones y claves.
- [Código: repos `Ef*Repository.cs`](../../GestionEscolar/GestionEscolar.Data/Repositories/)
  Un context por operación, `SaveAsync`, `GetMarksAsync`. Use for: ciclo de vida del DbContext y asistencia.
- [Código: `RiskRule` en Core](../../GestionEscolar/GestionEscolar.Core/)
  Cálculo de riesgo puro. Use for: pesos 60/40 y umbrales desde `setting`.
- [Docs EF Core: DbContext lifetime](https://learn.microsoft.com/ef/core/dbcontext-configuration/)
  Factory y scopes. Use for: por qué no hay un context singleton de UI.

## Wisdom (Communities)

- Equipo / chat del curso PP2 — pedir review de PRs y dudas de dominio.
- [Stack Overflow /tag entity-framework-core](https://stackoverflow.com/questions/tagged/entity-framework-core)
  Para trampas concretas de EF (principal key, tracking), no para el dominio del repo.
