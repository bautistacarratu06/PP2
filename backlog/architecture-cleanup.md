Le faltan algunas cosas para ser Clean Architecture estricta. Lo principal es que el dominio y los casos de uso están mezclados en Core.

Separar dominio y aplicación. En Clean Architecture hay dos capas internas:

Domain: entidades y reglas puras, como Student, RiskLevel o la fórmula de ScoringRiskServices.
Application: casos de uso que orquestan, como GetSectionTrackingUseCase o RegisterAttendanceUseCase, y las interfaces de repositorio.
Hoy todo eso vive junto en Core, y los "casos de uso" son servicios que agrupan varias operaciones.

Que los ViewModels hablen siempre con casos de uso. TrackingViewModel todavía recibe IRiskSummaryRepository directo, sin pasar por un servicio, así que la UI conoce un repositorio.

Lógica de negocio solo en el dominio. La fórmula de riesgo está duplicada en C# y en el SQL del resumen. Eso es lógica de negocio dentro de infraestructura.

Entidades con comportamiento. Los modelos son casi todos records con datos y las reglas están en los servicios, lo que se llama un "modelo anémico". En Clean Architecture con DDD, reglas como "no repetir la fecha de una clase" o "el riesgo según los umbrales" suelen vivir en la entidad o en objetos de valor.

Configuración afuera. La connection string es una constante y los umbrales 50/25 están hardcodeados. Deberían venir de configuración o de dbo.setting, a través de una interfaz.

Composition root separado (opcional). El registro de dependencias está todo en MauiProgram. Lo común es que cada capa exponga su propio AddCore() / AddData(), y que UI solo los llame.

Lo que ya cumple: las dependencias apuntan hacia adentro, Core no conoce ni MAUI ni SQL, los repositorios están detrás de interfaces y se testea con fakes. Para el tamaño del proyecto es una versión simplificada razonable. Los puntos 2 y 3 son los que más conviene corregir.