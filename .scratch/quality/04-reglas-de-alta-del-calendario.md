# Reglas de alta del calendario en el dominio

## What to build

La fecha de clase y el nombre del trabajo se validan en un tipo de dominio que el caso de uso consulta antes de guardar. Repetir el día en la misma comisión sigue fallando con el mensaje que ya nombra esa fecha. La misma fecha en otra comisión se sigue permitiendo. El trabajo sin nombre, o con nombre vacío, se rechaza. El nombre se recorta, no pasa de 120 caracteres y la hora de la fecha no se guarda. La lista del calendario sigue ordenada por fecha, clase antes que trabajo. La pantalla de Calendario muestra ese mismo error y no agrega la fila. La restricción única de la base se mantiene como red de seguridad; este corte no la saca.

Hoy `CalendarService` rechaza la fecha repetida en la comisión, exige nombre, recorta espacios y corta en 120 caracteres.

Tests a renombrar (`CalendarServiceTests`):

- `GetItemsAsync_MergesAndSortsByDate_ClassBeforeAssignment`
- `AddClassAsync_DuplicateDateInSection_Throws`
- `AddClassAsync_SameDateOtherSection_IsAllowed`
- `AddAssignmentAsync_NameRequired`
- `AddAssignmentAsync_TooLongName_Throws`
- `AddAssignmentAsync_TrimsNameAndDropsTime`

Al terminar se puede verificar: en Calendario, una clase en una fecha ya usada muestra el aviso y no aparece duplicada. Un trabajo sin nombre no se agrega. Un nombre con espacios se ve recortado en la lista.

## Acceptance criteria

- [ ] Dada una clase el mismo día en la comisión, al agregar otra, se rechaza y no se inserta.
- [ ] Dada esa fecha en otra comisión, al agregar la clase, se inserta.
- [ ] Dado un nombre nulo, vacío o en blanco, al agregar el trabajo, se rechaza.
- [ ] Dado un nombre de más de 120 caracteres, al agregar el trabajo, se rechaza.
- [ ] Dado un nombre con espacios y una hora en la fecha, al agregar el trabajo, se guardan el nombre recortado y la fecha sin hora.
- [ ] Dadas clases y trabajos, al listar, se ordenan por fecha y, en la misma fecha, la clase va primero.
- [ ] Los tests se llaman `Given…_When…_Then…` y usan el repositorio falso.
- [ ] En la app, el alta inválida muestra el mensaje y la lista no incorpora esa fila.

## Blocked by

- None - can start immediately
