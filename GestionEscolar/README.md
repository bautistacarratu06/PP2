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

## Nota

Este documento representa únicamente una **primera aproximación al dominio**. Los conceptos, relaciones y responsabilidades aquí definidos son preliminares y **pueden cambiar** durante el desarrollo del análisis, la validación con los requerimientos y la implementación.
