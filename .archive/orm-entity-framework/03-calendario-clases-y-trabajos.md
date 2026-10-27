# Calendario: listar y dar de alta clases y trabajos

## What to build

El calendario de una comisión lista clases y trabajos, y da de alta los dos, a través de Entity Framework. La lista combinada sigue ordenada por fecha. Una clase guarda solo el día. Un trabajo guarda el nombre recortado y el día de entrega, sin hora.

Una clase en una fecha que ya existe en esa comisión se rechaza y no se inserta. Un trabajo sin nombre, o con nombre de más de 120 caracteres, se rechaza y no se inserta. La unique de la base sigue siendo la red de seguridad de la fecha de clase.

Este corte no cambia el esquema y no toca asistencia ni entregas.

Al terminar se puede verificar: en el calendario se ve la lista, una clase nueva aparece, la misma fecha en la misma comisión no agrega otra fila, y un trabajo con nombre válido queda guardado.

## Acceptance criteria

- [ ] El calendario lista las clases y los trabajos de la comisión, ordenados por fecha.
- [ ] Dar de alta una clase en un día libre la guarda y la muestra.
- [ ] Dar de alta una clase en un día que ya tiene clase en esa comisión no inserta y explica el rechazo.
- [ ] Un trabajo con nombre válido (recortado, hasta 120 caracteres) se guarda con la fecha sin hora.
- [ ] Un trabajo sin nombre, o con más de 120 caracteres, no se inserta.
- [ ] Este corte no cambia el esquema.
- [ ] Los tests de Core siguen en verde.

## Blocked by

- [01 — Comisiones leídas con EF, y el esquema actual como snapshot ya aplicado](01-comisiones-leidas-con-ef.md)
