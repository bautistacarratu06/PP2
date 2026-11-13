# 03: Exportación del reporte a archivo de texto (.txt)

## Parent

[Epic H12: Vista de Reportes, Resumen de Riesgo por Comisión y Exportación](../../ticket-reportes-resumen-riesgo.md)

## What to build

Implementar la capacidad de descargar y exportar el reporte textual generado de la comisión a un archivo `.txt` utilizando las funciones nativas del sistema operativo mediante `Share.Default.RequestAsync` en .NET MAUI.

El usuario dispondrá de una acción visible (botón o ícono "Descargar reporte") en la vista `ReportsPage`. Al accionarlo:
1. Se valida que exista un reporte cargado con contenido exportable. Si la comisión no tiene datos o está vacía, la acción se deshabilita o muestra un mensaje orientativo al usuario.
2. Se formatea el documento de texto plano exportable con membrete institucional, nombre de la comisión, fecha y hora de emisión, resumen ejecutivo de riesgo ("X de Y estudiantes están en riesgo de abandonar" o "Todavía no hay datos...") y los párrafos descriptivos generados.
3. Se escribe el archivo temporal en disco local (ej. `FileSystem.CacheDirectory` con nombre descriptivo `Reporte_Comision_<Nombre>_<Fecha>.txt`).
4. Se invoca `Share.Default.RequestAsync(new ShareFileRequest { ... })` para abrir el diálogo nativo de Windows (o del SO) que permite guardar o compartir el archivo.
5. Se incluye manejo de excepciones y retroalimentación al usuario mediante mensajes de estado.

## Acceptance criteria

- [ ] Botón/ícono de exportación/descarga visible y accesible en `ReportsPage`.
- [ ] Comando de exportación en `ReportsViewModel` que produce el archivo `.txt` estructurado con la información del reporte.
- [ ] Invocación nativa a `Share.Default.RequestAsync` con el archivo generado.
- [ ] Deshabilitación o retroalimentación adecuada si no hay reporte disponible o la comisión no tiene datos.
- [ ] Manejo de errores con mensaje visible en caso de fallo durante la exportación.
- [ ] Tests unitarios que validen la generación del formato de texto para exportación y el comportamiento del comando en `ReportsViewModel`.

## Blocked by

- 02: Vista de Reportes con navegación, carga por defecto, selector y skeletons

