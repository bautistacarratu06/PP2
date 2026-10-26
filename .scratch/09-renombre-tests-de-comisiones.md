# Renombre de tests de comisiones

## What to build

Esos cuatro tests pasan a `Given_When_Then` y siguen afirmando lo mismo: la lista sale del repositorio; si existe la comisión 1, es la de por defecto; si no, la primera; si no hay ninguna, ninguna. Se agrega el caso de que el texto de la comisión es su nombre, porque el selector lo usa así. La regla de la comisión por defecto no se mueve de lugar.

El esquema de nombres de `SectionService`, después de que el test de seguimiento haya salido de ese archivo (corte 02). `GetAllAsync` y `PickDefault` ya están cubiertos. `Section.ToString`, que devuelve el nombre, no tiene test.

Tests a renombrar:

- `SectionServiceTests.GetAllAsync_ReturnsRepositorySections`
- `SectionServiceTests.PickDefault_PrefersSectionOne`
- `SectionServiceTests.PickDefault_WithoutSectionOne_ReturnsFirst`
- `SectionServiceTests.PickDefault_Empty_ReturnsNull`

Al terminar se puede verificar: los tests pasan y el selector de Seguimiento, Asistencia, Entregas y Calendario sigue abriendo en la comisión 1 cuando existe.

## Acceptance criteria

- [ ] Dado un repositorio con comisiones, al listar, se devuelven esas comisiones.
- [ ] Dada una lista que incluye la comisión 1, al elegir la de por defecto, el id es 1.
- [ ] Dada una lista sin la comisión 1, al elegir, queda la primera.
- [ ] Dada una lista vacía, al elegir, el resultado es vacío.
- [ ] Dada una comisión con nombre, al pedir su texto, el texto es el nombre.
- [ ] Los nombres son `Given…_When…_Then…`.

## Blocked by

- [02 — Seguimiento calcula grilla y tarjetas en un solo caso de uso](02-seguimiento-grilla-y-tarjetas.md)
