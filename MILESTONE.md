
# CONTEXT.md

## 1. Objetivo y alcance del TP

Dominio: Seguimiento de cursada y alerta temprana. Un panel para que quien sigue una comisión sepa a qué estudiantes contactar esta semana, antes de que abandonen.

Pregunta central del sistema: ¿a quién llamo esta semana?

Usuario único: una sola persona que sigue dos comisiones de primer año (~60 estudiantes totales). No hay autenticación, ni múltiples usuarios, ni permisos.

Alcance: once historias de usuario en cinco specs, priorizadas como esqueleto mínimo H1 → H3 → H6 → H8 (nómina, asistencia, cálculo de riesgo, panel ordenado). Las cuatro se entregan en versión mínima primero y se engrosan en iteraciones posteriores.

Lo que no entra: importación de archivos, autenticación, gestión de notas/calificaciones, integración con sistemas del instituto, edición/baja en el sprint 1, validaciones de formato de campos.

## 2. Requisitos funcionales y reglas de negocio

### Historias de usuario (backlog de referencia)

| Spec | Historia | Descripción |
|---|---|---|
| 1 · Cursada y comisión | H1 | Cargar y ver la nómina de estudiantes de una comisión |
| | H2 | Definir el calendario de clases y de trabajos del cuatrimestre |
| 2 · Registro | H3 | Registrar la asistencia de una clase |
| | H4 | Corregir la asistencia de una fecha ya registrada |
| | H5 | Registrar la entrega o no entrega de un trabajo |
| 3 · Indicador de riesgo | H6 | Calcular el riesgo de cada estudiante a partir de asistencia y entregas |
| | H7 | Configurar los umbrales del cálculo sin tocar el código |
| 4 · Panel | H8 | Ver la comisión ordenada por riesgo, con el detalle que justifica cada valor |
| | H9 | Ver la ficha de un estudiante con su historial |
| 5 · Intervenciones | H10 | Registrar un contacto con un estudiante y su resultado |
| | H11 | Que el panel muestre a quién ya se contactó y cuándo |

### Reglas de negocio explícitas

La nómina no se depura: nadie desaparece de la lista, incluidos quienes abandonan.
El identificador (legajo) debe ser único por comisión. Un legajo duplicado se rechaza; es la única validación que entra en el mínimo porque corrompe el indicador.
La asistencia es un concepto, no un atributo de la clase ni del estudiante. Es un hecho con condición propia (presente/ausente) que vincula estudiante con clase y admite corrección (H4): corregir modifica el registro existente, no agrega otro.
A lo sumo una asistencia por estudiante y clase. A lo sumo una entrega por estudiante y trabajo.
El riesgo es un valor derivado, no se persiste. Se calcula a partir de asistencia y entregas contra el criterio vigente.
El criterio de riesgo (umbrales de inasistencia y de trabajos no entregados) es configurable por el cliente sin tocar código (H7). Mientras H7 no exista, el umbral vive escrito en el código.
Fecha de incorporación del estudiante es atributo obligatorio: sin ella, el porcentaje de inasistencias de quien entró tarde se calcula mal (infla el riesgo).
Estudiantes que se incorporan tarde pueden agregarse en cualquier momento, no sólo al inicio.
### Esqueleto mínimo (H1→H3→H6→H8) — versión flaca

| Historia | Entra en el mínimo | Queda para después |
|---|---|---|
| H1 · Nómina | Alta manual, ver nómina por comisión con total | Edición, baja, validaciones de forma, importación |
| H3 · Asistencia | Marcar presentes de una fecha y guardar | Corrección (H4), cierre de planilla, ausencias justificadas |
| H6 · Riesgo | Sólo inasistencias, umbral fijo en código | Incorporar entregas, ponderar señales, configurar umbral (H7) |
| H8 · Panel | Lista ordenada de mayor a menor riesgo con dato justificante | Ficha (H9), filtros, marcas de contacto (H11) |

### Preguntas de relevamiento abiertas (no resolver sin el cliente)

¿Un estudiante puede estar en ambas comisiones o cambiarse a mitad de cuatrimestre? Si sí, aparece el concepto Inscripción.
¿La asistencia es sólo presente/ausente o hay más condiciones (llegada tarde, ausencia justificada)?
¿Un trabajo entregado fuera de término cuenta como entregado?
¿Los umbrales son los mismos para ambas comisiones?
¿Qué cuenta como contacto? ¿Un mensaje sin respuesta es contacto?
¿Las clases se cargan de antemano o aparecen al tomar asistencia? (Diferencia entre faltó 4 de 10 y faltó 4 veces.)
## 3. Restricciones técnicas y criterios de implementación

### Arquitectura

Organización en capas con dependencias apuntando hacia el dominio: Presentación → Dominio ← Datos. El dominio no conoce a MAUI ni a la base de datos.
Patrón repositorio con interfaces definidas en el dominio (puertos). La persistencia es un adaptador. Cambiar de motor de base de datos no debe afectar al dominio.
Inyección de dependencias: ningún ViewModel instancia repositorios con new; los recibe por constructor.
Demo de referencia: proyecto .NET MAUI + SQLite (vía Entity Framework Core) + MSTest. Ver trabajo-integrador/DemoTPI/.
### Calidad de código (verificable)

1. Métodos chicos: máximo 20 líneas, complejidad ciclomática máxima 10.
2. Separación de capas: los .xaml.cs sólo tienen InitializeComponent, BindingContext y aviso de aparición al ViewModel. Las reglas de negocio (cálculo de riesgo, orden del panel) viven en el dominio, no en el ViewModel.
3. Dependencias apuntan al dominio: entidades y servicios sin using de MAUI ni de EF Core. Ningún ViewModel hace new de un repositorio.
4. Cada dato escrito una sola vez: estados, niveles de riesgo y umbrales se definen en un único lugar.
5. Las reglas están probadas: