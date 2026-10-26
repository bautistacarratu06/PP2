# Tests de la nómina

## What to build

Un repositorio falso de alumnos devuelve la nómina de una comisión. El caso de uso pide esa comisión y devuelve los alumnos con id, nombre, legajo, teléfono y correo iguales a los de origen. Una comisión sin alumnos devuelve lista vacía. No se agrega formato de pantalla en el modelo.

`StudentService.GetRosterAsync` es el único método público de ese servicio y no tiene ningún test. La pantalla sigue cargando la comisión 1; este corte no le agrega selector ni cambia cuándo carga.

Al terminar se puede verificar: el test pasa con el falso, sin base. La nómina de la app sigue mostrando la comisión 1.

## Acceptance criteria

- [ ] Dada una comisión con alumnos, al pedir la nómina, el repositorio recibe ese id y cada alumno conserva id, nombre, legajo, teléfono y correo.
- [ ] Dada una comisión vacía, al pedir la nómina, la lista va vacía.
- [ ] El nombre del test es `Given…_When…_Then…`.

## Blocked by

- None - can start immediately
