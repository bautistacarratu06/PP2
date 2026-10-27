# Cargar una comisión nueva desde un CSV

## What to build

En Nómina hay un botón «Cargar comisión». Abre un asistente en un modal, con atrás y siguiente. El docente elige un archivo con el botón de seleccionar archivo, escribe el nombre de una comisión que no existe y ve cuántos alumnos se van a insertar. Al confirmar, se crea la comisión y su nómina, y Nómina pasa a mostrar esa comisión.

El archivo trae DNI, apellido, nombre, legajo, teléfono y correo. Este corte incluye un CSV de prueba con varias filas, sin DNI ni legajo repetidos, para recorrer el asistente.

Un nombre vacío, ya usado o de más de 20 caracteres se explica en el asistente y no se guarda nada. Un archivo con un DNI o un legajo repetido, o con un DNI que ya está guardado, no se confirma en este corte: lo resuelve el corte de duplicados. Este corte no arrastra archivos ni fusiona filas.

Al terminar se puede verificar: con el CSV de prueba se crea una comisión nueva y Nómina lista esos alumnos, con apellido y nombre, legajo, teléfono y correo. Elegir el nombre de una comisión que ya existe no inserta filas.

## Acceptance criteria

- [ ] En Nómina, «Cargar comisión» abre un asistente con atrás y siguiente, y el archivo se elige con un botón.
- [ ] Hay un CSV de prueba con DNI, apellido, nombre, legajo, teléfono y correo, sin DNI ni legajo repetidos.
- [ ] Dado un nombre de comisión que no existe y ese archivo, al confirmar, se crean la comisión y los alumnos, y la vista previa dice cuántos se van a insertar.
- [ ] Después de confirmar, Nómina muestra esa comisión con esos alumnos.
- [ ] Dado un nombre vacío, repetido o de más de 20 caracteres, el asistente lo explica y no escribe nada.
- [ ] El alta (comisión nueva, N alumnos, nombre rechazado) queda cubierta por tests con un repositorio falso.
- [ ] Este corte no resuelve duplicados, no fusiona y no acepta soltar el archivo.

## Blocked by

- None - can start immediately
