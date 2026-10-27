# Historias oficiales del trabajo integrador

Fuente: [consigna de la cátedra](https://github.com/emigallo-edu/ifts-pp2/blob/main/trabajo-integrador/consigna.md) (`emigallo-edu/ifts-pp2`, `trabajo-integrador/consigna.md`). Son once historias en cinco specs. La pregunta del sistema es *¿a quién llamo esta semana?*

Este mapa contrasta cada historia con el código de hoy. Las historias de más abajo cierran lo que está a medias o no existe.

| Historia | Estado | Qué hay en el código | Hueco |
|---|---|---|---|
| **H1** · Cargar y ver la nómina de una comisión | Parcial | Nómina por comisión, ordenada por apellido, con legajo, teléfono y correo. La comisión vacía dice que no hay estudiantes. | No se puede dar de alta un estudiante. No hay rechazo de legajo o DNI repetido. |
| **H2** · Calendario de clases y de trabajos | Completa | `CalendarService` da de alta una clase por fecha y un trabajo con nombre y vencimiento, por comisión. Rechaza la fecha de clase repetida y el nombre vacío o demasiado largo. | — |
| **H3** · Registrar la asistencia de una clase | Completa | Asistencia elige comisión y fecha del calendario y guarda Presente o Ausente. | — |
| **H4** · Corregir la asistencia de una fecha ya registrada | Completa | Volver a guardar la misma clase y el mismo alumno actualiza la fila. `corrected_at` se escribe solo cuando el estado cambia. Hay una sola asistencia por alumno y clase. | — |
| **H5** · Registrar la entrega o no entrega de un trabajo | Completa | Entregas elige el trabajo y guarda Entregó o No entregó. Una segunda vez actualiza la misma fila. | — |
| **H6** · Calcular el riesgo con asistencia y entregas | Parcial | `RiskRule`: 60 % inasistencia, 40 % entregas faltantes, umbrales de `RiskConfig`. Cero clases y cero trabajos dan riesgo 0. Hay tests de ese borde. | El total de clases es todas las de la comisión, hayan ocurrido o no. Quien no tiene marca de presente suma como inasistencia, también en una clase futura. No hay fecha de incorporación: quien entra tarde carga las clases anteriores. |
| **H7** · Configurar los umbrales sin tocar el código | Completa | Ajustes lee y guarda Alto y Medio en `dbo.setting` (`RiskSettingsService`). Enteros de 0 a 100, y Medio menor que Alto. El panel usa esos valores en la carga siguiente. | — |
| **H8** · Ver la comisión ordenada por riesgo, con el detalle que lo justifica | Completa | Seguimiento ordena de mayor a menor `RiskScore`, muestra el nivel, el puntaje y el texto de clases y trabajos que lo justifican. | — |
| **H9** · Ficha del estudiante con su historial | Completa | El detalle abre desde la grilla y lista asistencia, entregas e intervenciones. Una clase o un trabajo sin marca figuran como «Sin registro». | El botón «Registrar contacto» no guarda nada: es H10. |
| **H10** · Registrar un contacto y su resultado | Faltante | Existen `Intervention`, la tabla y la lectura. El botón de la ficha muestra «Todavía no está implementado». | No hay alta de contacto. |
| **H11** · El panel muestra a quién ya se contactó y cuándo | Completa en lectura | La columna usa el contacto más reciente. Sin intervenciones dice «Sin contactar»; con fecha dice hoy, ayer, hace días o la fecha. | No se puede producir un contacto nuevo desde la app. Lo cierra la historia de H10. |

## Historias para los huecos

- [01 — Cargar un estudiante en la comisión](01-cargar-un-estudiante-en-la-comision.md) — cierra H1. Sin bloqueo.
- [02 — El riesgo cuenta solo lo que ya correspondía](02-el-riesgo-cuenta-solo-lo-que-ya-correspondia.md) — cierra H6. Bloqueada por 01.
- [03 — Registrar un contacto](03-registrar-un-contacto.md) — cierra H10 y hace que H11 muestre contactos cargados en la app. Sin bloqueo.

## Qué queda afuera de estas tres

La consigna deja la importación de archivo fuera de H1. Eso ya está en [`.scratch/dnd`](../dnd/README.md). Estas historias no cargan un CSV, no editan ni borran alumnos, clases, trabajos ni contactos, y no cambian el peso 60/40 ni los umbrales de Ajustes.
