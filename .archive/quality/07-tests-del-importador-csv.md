# Tests del importador CSV y renombre del parser

## What to build

Los tests del parser conservan los cinco casos que ya tienen (línea simple, coma entre comillas, comillas escapadas, campos vacíos, línea vacía) y pasan a `Given_When_Then`. El importador se prueba con un lector en memoria: toma el encabezado sin importar mayúsculas, saltea líneas en blanco, ignora columnas que no conoce y arma los datos de seguimiento que el mapa ya define. Un archivo vacío falla. Un número o una fecha inválidos fallan indicando el número de línea. No se inventa una columna de clases totales: si el archivo no la trae, ese total queda en cero, que es el comportamiento actual.

`StudentCsvImporter.Import` no tiene ningún test. El parser sí está cubierto, pero con otro esquema de nombres. Ninguna pantalla llama hoy al importador; este corte no lo conecta a la nómina.

Tests a renombrar (`CsvParserTests`):

- `ParseLine_SimpleLine_ReturnsFields`
- `ParseLine_CommaInsideQuotes_DoesNotSplitField`
- `ParseLine_EscapedQuotes_ReturnsLiteralQuotes`
- `ParseLine_EmptyFields_PreservesEmptyFields`
- `ParseLine_EmptyLine_ReturnsOneEmptyField`

Al terminar se puede verificar: la suite del parser y del importador pasa. La nómina de la app sigue saliendo de la base.

## Acceptance criteria

- [ ] Los cinco casos actuales del parser siguen verdes con nombre `Given…_When…_Then…`.
- [ ] Dado un CSV con el encabezado conocido, al importar, cada campo mapeado cae en el dato de seguimiento correspondiente.
- [ ] Dado un archivo vacío, al importar, la operación se rechaza.
- [ ] Dada una línea en blanco, al importar, esa línea no genera fila.
- [ ] Dado un entero o una fecha inválidos, al importar, el error indica el número de línea.
- [ ] Dada una columna desconocida, al importar, se ignora.
- [ ] Dado un archivo sin clases totales, al importar, ese total queda en cero.

## Blocked by

- None - can start immediately
