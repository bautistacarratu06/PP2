# Cargar un estudiante en la comisión

Cierra **H1** de la consigna. Hoy la nómina solo lee. `IStudentRepository` no tiene alta, y un DNI o un legajo repetidos no se pueden rechazar porque nadie los escribe desde la app.

## What to build

En Nómina, con una comisión elegida, hay un alta de un estudiante. El docente carga apellido, nombre, legajo, DNI y fecha de incorporación. Teléfono y correo son opcionales. La fecha de incorporación sale en hoy y se puede cambiar a un día anterior. Al confirmar, ese estudiante aparece en la nómina de esa comisión, ordenado por apellido, y el total sube en uno. La otra comisión no lo muestra.

La fecha de incorporación queda guardada. La historia del riesgo la usa como día desde el cual cuentan las clases y los trabajos.

Un apellido, un nombre, un legajo o un DNI vacíos se explican y no se guarda nada. Una fecha posterior a hoy se explica y no se guarda nada. Si el legajo ya está en esa comisión, el alta se rechaza y el mensaje nombra al estudiante que ya lo tiene. Si el DNI ya está en la base, en esta comisión o en otra, el alta se rechaza y el mensaje dice a quién corresponde. La nómina no pierde ni gana filas en esos rechazos.

Una comisión sin estudiantes lo dice y ofrece esta misma alta, en lugar de una tabla vacía sin acción.

Este corte no importa un archivo, no edita y no da de baja. Nadie desaparece de la nómina.

## Acceptance criteria

- [ ] En Nómina, con una comisión elegida, se puede cargar apellido, nombre, legajo, DNI y fecha de incorporación, con teléfono y correo opcionales.
- [ ] La fecha de incorporación propone hoy, acepta un día anterior y rechaza un día posterior.
- [ ] Dada una comisión sin ese legajo ni ese DNI, al confirmar, la nómina de esa comisión muestra al estudiante, el total aumenta en uno y la otra comisión no lo muestra.
- [ ] La fecha de incorporación confirmada queda guardada y se puede leer después.
- [ ] Dado un apellido, nombre, legajo o DNI vacío, el alta se rechaza, el mensaje lo explica y no se escribe nada.
- [ ] Dado el legajo 12345 ya cargado en la comisión, al intentar otro estudiante con el 12345, el alta se rechaza, el mensaje nombra a quién corresponde y el total no cambia.
- [ ] Dado un DNI que ya existe, en esta comisión o en otra, el alta se rechaza, el mensaje dice a quién corresponde y el total no cambia.
- [ ] Una comisión sin estudiantes informa que todavía no tiene estudiantes y ofrece el alta.
- [ ] El alta y los rechazos quedan cubiertos por tests con un repositorio falso.
- [ ] Este corte no importa un CSV, no edita y no da de baja.

## Blocked by

- None - can start immediately
