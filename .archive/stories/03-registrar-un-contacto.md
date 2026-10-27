# Registrar un contacto

Cierra **H10** de la consigna. La ficha ya lista intervenciones y el panel ya muestra el contacto más reciente, pero el botón «Registrar contacto» solo avisa que no está implementado. Sin un alta, H11 no puede mostrar un contacto cargado desde la app.

## What to build

En la ficha del estudiante, «Registrar contacto» abre un formulario. El docente indica la fecha, el medio y el resultado, y puede agregar una nota. La fecha propone hoy y acepta un día anterior. Al confirmar, se guarda un contacto de ese estudiante.

La ficha lo muestra en el historial, con fecha, medio, resultado y la nota si la hay. El panel de esa comisión, en la fila de ese estudiante, deja de decir «Sin contactar» y muestra ese contacto. Un segundo contacto, con fecha posterior, pasa a ser el que muestra el panel, y los dos siguen en la ficha.

Un medio vacío, un resultado vacío o una fecha posterior a hoy se explican y no se guarda nada. Este corte no edita ni borra contactos.

## Acceptance criteria

- [ ] En la ficha, «Registrar contacto» abre el formulario con fecha, medio, resultado y nota opcional. La fecha propone hoy.
- [ ] Dado medio y resultado con texto y una fecha de hoy o anterior, al confirmar, la ficha lista ese contacto con fecha, medio, resultado y la nota si se escribió.
- [ ] Después de confirmar, el panel de la comisión muestra ese contacto en la fila del estudiante, en lugar de «Sin contactar».
- [ ] Dado un contacto anterior y uno nuevo con fecha posterior, el panel muestra el nuevo y la ficha lista los dos.
- [ ] Dado un medio vacío, un resultado vacío o una fecha posterior a hoy, el alta se rechaza, el mensaje lo explica y no se escribe nada.
- [ ] El alta y los rechazos quedan cubiertos por tests con un repositorio falso.
- [ ] Este corte no edita ni borra contactos.

## Blocked by

- None - can start immediately
