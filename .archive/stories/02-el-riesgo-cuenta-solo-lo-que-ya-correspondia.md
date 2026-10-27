# El riesgo cuenta solo lo que ya correspondía

Cierra **H6** de la consigna. `RiskRule` ya combina inasistencias y entregas faltantes, pero el total que le llega cuenta todas las clases y todos los trabajos de la comisión. Una clase del mes que viene, sin marca de presente, sube el riesgo de toda la nómina. Quien se incorpora tarde carga también las clases anteriores a su fecha.

## What to build

El puntaje sigue siendo 60 % inasistencia y 40 % entregas faltantes, con los umbrales de Ajustes y el mismo redondeo. Lo que cambia es qué entra en cada total.

Cuentan las clases de la comisión cuya fecha es hoy o anterior, y es igual o posterior a la fecha de incorporación de ese estudiante. Cuentan los trabajos cuyo vencimiento cumple la misma condición. Una clase o un trabajo posterior a hoy no entra. Una clase o un trabajo anterior a la incorporación no entra.

Dentro de lo que sí entra, presente suma asistencia y entregó suma entrega. Sin marca sigue contando como no presente o no entregado: la ficha puede decir «Sin registro» y el riesgo no lo trata como presente.

Si a un estudiante no le queda ninguna clase en el tramo, esa señal aporta 0, igual que el caso de cero clases que ya cubre la regla. Lo mismo para los trabajos. El panel y la ficha muestran los mismos totales que usó el cálculo, en el texto que justifica el valor.

La fecha de incorporación es la que guardó el alta. Los estudiantes que ya están en la base usan la fecha, sin hora, con la que fueron cargados.

## Acceptance criteria

- [ ] Dado un estudiante incorporado el 1 de abril y una clase el 15 de marzo, esa clase no entra en su total ni en sus inasistencias.
- [ ] Dada una clase con fecha posterior a hoy y sin marca, esa clase no entra en el total de nadie.
- [ ] Dado un trabajo que vence después de hoy, ese trabajo no entra en el total ni como no entregado.
- [ ] Dada una clase de hoy o anterior, posterior a la incorporación, sin marca de presente, cuenta como inasistencia.
- [ ] Dado un estudiante al que no le queda ninguna clase ni ningún trabajo en el tramo, el riesgo de esas señales es 0.
- [ ] El caso que ya da 32 (16 de 20 y 5 de 10, todo ya ocurrido y posterior a la incorporación) sigue dando 32 y nivel según los umbrales guardados.
- [ ] El texto que justifica el valor en el panel usa los mismos totales que el cálculo.
- [ ] Estos casos quedan cubiertos por tests de la regla, con la fecha de incorporación entre los datos.

## Blocked by

- [01 — Cargar un estudiante en la comisión](01-cargar-un-estudiante-en-la-comision.md) — la verificación en la app necesita un alta que deje la fecha de incorporación. La regla y sus tests pueden escribirse con esa fecha ya presente en el dato del alumno.
