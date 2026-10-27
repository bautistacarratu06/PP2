# Cargar una comisión nueva desde un CSV

## What to build

En Nómina hay un botón «Cargar comisión». Abre un asistente en un modal, con atrás y siguiente. El docente elige el archivo con el botón de seleccionar archivo o soltando un CSV encima del asistente. Escribe el nombre de una comisión que no existe y ve cuántos alumnos se van a insertar. Al confirmar, se crea la comisión y su nómina, y Nómina pasa a mostrar esa comisión.

El archivo trae DNI, apellido, nombre, legajo, teléfono y correo. Este corte incluye un CSV de prueba con varias filas, sin DNI ni legajo repetidos, para recorrer el asistente tanto con el botón como soltando el archivo.

Un nombre vacío, ya usado o de más de 20 caracteres se explica en el asistente y no se guarda nada. Soltar un archivo que no es CSV se explica y no avanza. Un archivo con un DNI o un legajo repetido, o con un DNI que ya está guardado, no se confirma en este corte: lo resuelve el corte de conflictos. Este corte no fusiona filas ni elige una comisión que ya existe.

## Acceptance criteria

- [ ] En Nómina, «Cargar comisión» abre un asistente con atrás y siguiente.
- [ ] El archivo se elige con un botón o soltando un CSV sobre el asistente, y los dos caminos dejan el mismo archivo elegido.
- [ ] Hay un CSV de prueba con DNI, apellido, nombre, legajo, teléfono y correo, sin DNI ni legajo repetidos.
- [ ] Dado un nombre de comisión que no existe y ese archivo, al confirmar, se crean la comisión y los alumnos, y la vista previa dice cuántos se van a insertar.
- [ ] Después de confirmar, Nómina muestra esa comisión con esos alumnos.
- [ ] Dado un nombre vacío, repetido o de más de 20 caracteres, el asistente lo explica y no escribe nada.
- [ ] Un archivo que no es CSV se rechaza con un mensaje y no se importa.
- [ ] El alta (comisión nueva, N alumnos, nombre rechazado) queda cubierta por tests con un repositorio falso.
- [ ] Este corte no resuelve duplicados, no fusiona y no agrega alumnos a una comisión que ya existe.

## Blocked by

- None - can start immediately
