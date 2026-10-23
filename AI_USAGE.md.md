# Uso de IA en el proyecto

## Skills recomendados para agentes

Los *skills* son carpetas con un `SKILL.md` que le enseñan al agente (Cursor, Claude Code, Codex, Copilot, etc.) un flujo de trabajo concreto. Recomendamos dos colecciones. Las instrucciones propias del repo están en [`AGENTS.md`](./AGENTS.md).

### pstack (Lauren Tan, "poteto")

- Repo oficial: <https://github.com/cursor/plugins/tree/main/pstack>
- Copia para usar fuera de Cursor: <https://github.com/backnotprop/pstack>
- Qué ofrece: skills y principios para trabajar con rigor junto a un agente. La idea es escribir menos código, pero de mejor calidad y verificado. `poteto-mode` elige el flujo según la tarea. Otros skills:
  - `how`: explica cómo funciona un subsistema siguiendo el código real.
  - `architect`: define la interfaz y la forma de los módulos antes de implementar.
  - `tdd`: test que falla primero y después el arreglo.
  - `blast-radius`: qué más puede romper un cambio.
  - `interrogate` / `reflect`: revisión.
  - `unslop`, `no-comments`: limpieza.
- Instalación:
  - En Cursor: `/add-plugin pstack`.
  - En Claude Code: `/plugin marketplace add cursor/plugins` y después `/plugin install pstack@cursor-plugins`.
  - En otros agentes: `npx skills add backnotprop/pstack`.
  - Primer uso: correr `/setup-pstack`. Después, `/poteto-mode` para cualquier tarea que pida rigor.

### Skills de Matt Pocock

- Repo: <https://github.com/mattpocock/skills> ("Skills for Real Engineers")
- Qué ofrece: skills chicos y combinables, pensados para no perder el control del proceso. Algunos:
  - `grill-me` / `grill-with-docs`: el agente te hace preguntas hasta que los dos tengan claro el cambio.
  - `tdd`, `diagnosing-bugs`, `code-review`, `improve-codebase-architecture`, `domain-modeling`.
  - `to-spec` / `implement-spec`, `to-tickets`, `pr`.
- Instalación:
  - En Claude Code: `claude plugin install mattpocock-skills@claude-plugins-official`.
  - En Cursor y otros agentes: `npx skills@latest add mattpocock/skills -a cursor`. Incluí `setup-matt-pocock-skills` y corré `/setup-matt-pocock-skills` una vez por repo.

### Cómo encajan en GestionEscolar

| Necesidad | Skill sugerido |
|---|---|
| Entender un flujo (p. ej. Seguimiento: ViewModel → Service → Repository → SQL) | `how` (pstack) |
| Diseñar una vista o servicio nuevo respetando las capas de `ARCHITECTURE.md` | `architect` (pstack), `grill-with-docs` / `domain-modeling` (Pocock) |
| Lógica de Core con tests en `GestionEscolar.Tests` (fakes) | `tdd` (cualquiera de los dos) |
| Bugs de datos o de UI | `diagnosing-bugs` (Pocock), `blast-radius` (pstack) |
| Revisar un cambio antes del push (además de los hooks de Husky) | `code-review` (Pocock), `interrogate` (pstack) |
| Refactors de estructura | `improve-codebase-architecture` (Pocock) |

Regla general: lo que genere la IA se revisa, se compila (`scripts\run-tests.cmd`, build de UI) y lo commitea una persona.
