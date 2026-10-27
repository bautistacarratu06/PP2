# Soltar el CSV sobre el asistente

## What to build

El asistente acepta soltar un CSV encima, además del botón de elegir archivo. El archivo soltado entra al mismo recorrido: vista previa, alta de comisión o append, y las mismas reglas de duplicados que ya estén hechas. Soltar un archivo que no es CSV se explica y no avanza.

Este corte no cambia a quién se considera duplicado ni qué hace omitir o fusionar.

Al terminar se puede verificar: soltar el CSV de prueba deja el archivo elegido y el asistente sigue hasta la vista previa. Soltar otro tipo de archivo no importa nada.

## Acceptance criteria

- [ ] Soltar un CSV sobre el asistente lo deja elegido igual que el botón de archivo.
- [ ] Atrás, siguiente y la confirmación siguen el recorrido que ya existe.
- [ ] Un archivo que no es CSV se rechaza con un mensaje y no se importa.
- [ ] Las reglas de alta y de duplicados no cambian en este corte.

## Blocked by

- [01 — Cargar una comisión nueva desde un CSV](01-cargar-comision-nueva-desde-csv.md)
