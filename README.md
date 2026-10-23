# Modelo de dominio

> **Estado:** dominio preliminar. Este modelo está sujeto a cambios a medida que avance el análisis y se definan con mayor precisión las reglas del dominio.

## Modelo

El dominio preliminar está compuesto por los siguientes conceptos principales:

- **Comisión**
- **Estudiante**
- **Criterio de riesgo**
- **Clase**
- **Asistencia**
- **Trabajo**
- **Entrega**
- **Contacto**

### Relaciones

- La **Comisión** está integrada por **Estudiantes**.
- La **Comisión** tiene **Clases** en el calendario.
- La **Comisión** tiene **Trabajos** en el calendario.
- El **Estudiante** acumula y recibe información relacionada con:
  - **Asistencias**
  - **Entregas**
  - **Contactos**
- El **Criterio de riesgo** determina el riesgo del **Estudiante**.
- La **Asistencia** se registra a partir de una **Clase**.
- La **Entrega** se registra a partir de un **Trabajo**.

## Diagrama

```text
                         ┌───────────────────────┐
                         │   CRITERIO DE RIESGO  │
                         └───────────┬───────────┘
                                     │
                              determina el riesgo de
                                     │
                                     ▼
┌──────────────┐    está integrada por    ┌──────────────┐
│   COMISIÓN   │─────────────────────────▶│  ESTUDIANTE  │
└──────┬───────┘                          └──────┬───────┘
       │                                        │
       │ tiene en el calendario                 │ acumula y recibe
       │                                        │
       ├──────▶ ┌──────────┐    registra    ┌──┴───────────┐
       │        │  CLASE   │───────────────▶│  ASISTENCIA  │
       │        └──────────┘                └───────────────┘
       │
       └──────▶ ┌──────────┐    registra    ┌──────────────┐
                │ TRABAJO  │───────────────▶│   ENTREGA    │
                └──────────┘                └──────────────┘

                                            ┌──────────────┐
                                            │   CONTACTO   │
                                            └──────────────┘
```

## Arquitectura preliminar

Por ahora seguimos la arquitectura que recomienda la cátedra: tres capas lógicas (presentación, dominio y datos) en un solo proceso, con el cálculo de riesgo aislado detrás de una interfaz. Igual que el modelado de dominio, es preliminar y **puede cambiar** cuando tengamos más claro el producto final.

```text
  ┌───────────────────────────────────────────────────────────┐
  │  PRESENTACIÓN (UI)                                        │
  │  pantallas de registro, panel, detalle del estudiante     │
  └───────────────────────────┬───────────────────────────────┘
                              │
                              ▼
  ┌───────────────────────────────────────────────────────────┐
  │  DOMINIO                                                  │
  │  reglas de asistencia, entregas e intervenciones          │
  │  cálculo de riesgo (detrás de una interfaz)               │
  └───────────────────────────┬───────────────────────────────┘
                              │
                              ▼
  ┌───────────────────────────────────────────────────────────┐
  │  DATOS                                                    │
  │  almacenamiento relacional SQL                            │
  └───────────────────────────────────────────────────────────┘
```

## Nota

Este documento representa únicamente una **primera aproximación al dominio**. Los conceptos, relaciones y responsabilidades aquí definidos son preliminares y **pueden cambiar** durante el desarrollo del análisis, la validación con los requerimientos y la implementación.
## Git hooks (Husky.Net)

Los hooks se instalan solos la primera vez que se restaura o compila la solución (target `Husky` en `GestionEscolar.Core.csproj`, que corre `dotnet tool restore` + `dotnet husky install`). También se pueden instalar a mano desde la raíz del repo: `dotnet tool restore` y luego `dotnet husky install`.

- **pre-commit**: `dotnet format --verify-no-changes`, solo sobre los `.cs` en stage. Si falla, corré `dotnet format GestionEscolar/GestionEscolar.slnx` y volvé a agregar los archivos.
- **pre-push**: `scripts\hooks\pre-push.cmd` compila `UI.csproj` (`net10.0-windows10.0.19041.0`) y corre `scripts\run-tests.cmd`. Si `UI.exe` está abierto, avisa y cancela el push (bloquea las DLL): cerrá la app y reintentá.
- Saltear en una emergencia: `git commit --no-verify` / `git push --no-verify`.
- En CI o para desactivar la instalación automática: variable de entorno `HUSKY=0`.
- Configuración: `.husky/task-runner.json`; versión de la herramienta: `dotnet-tools.json`.
