# [HU] Incorporar botones de confirmación y cancelación en la importación de comisiones

- **ID:** HU-XX  
- **Módulo:** Nómina (`StudentsPage`)  
- **Funcionalidad:** Importación de comisiones mediante CSV  
- **Prioridad:** Alta  
- **Estado:** Completado (Verificado)  

---

### 1. Historia de usuario

**Como** usuario de la aplicación de Gestión Escolar,  
**quiero** disponer de botones para aceptar o cancelar el proceso de fusión u omisión de registros al importar un archivo CSV,  
**para** poder confirmar los cambios o salir del proceso sin quedar bloqueado en la pantalla.

---

### 2. Descripción del problema

Actualmente, al importar un archivo CSV que contiene registros de alumnos que ya existen en la base de datos, la aplicación muestra una pantalla de revisión donde se permite seleccionar individualmente si se desea fusionar u omitir cada registro.

Sin embargo, esta pantalla no cuenta con botones generales de **ACEPTAR** ni **CANCELAR**.

Esto genera los siguientes inconvenientes:
- El usuario no puede confirmar explícitamente las decisiones tomadas sobre los registros.
- No existe una opción para cancelar la operación y regresar a la pantalla anterior.
- La pantalla puede quedar aparentemente congelada, impidiendo que el usuario continúe utilizando la aplicación.
- El usuario queda atrapado en el proceso de importación sin una salida clara.

Causa raíz técnica detectada:
1. En `StudentsPage.xaml`, el contenedor modal del asistente (`Wizard`) tiene un `VerticalStackLayout` sin límite de altura máxima (`MaximumHeightRequest`) ni `ScrollView`. Al existir múltiples conflictos (registros coincidentes en DNI/legajo), la tarjeta se expande verticalmente fuera de la ventana visible, dejando los botones de acción fuera de la pantalla e inalcanzables.
2. Los botones anteriores utilizaban etiquetas como "Atrás" y "Confirmar" en lugar de "ACEPTAR" y "CANCELAR", y no ofrecían un comando de cancelación explícito para abortar el asistente descartando decisiones y cerrando el diálogo.
3. Al finalizar la importación, no se emitía una notificación visual explícita de confirmación hacia la vista principal.

---

### 3. Criterios de Aceptación

1. **Pantalla de revisión:** Al importar un CSV con registros existentes, se deberá mostrar la pantalla de revisión de fusión u omisión.
2. **Visualización de botones:** Se deberán visualizar los botones **ACEPTAR** y **CANCELAR** en la pantalla.
3. **Acción de aceptación:** Al presionar **ACEPTAR**, se deberán aplicar las decisiones seleccionadas para cada registro y finalizar el proceso de importación.
4. **Acción de cancelación:** Al presionar **CANCELAR**, se deberá abandonar el proceso y regresar a la pantalla anterior sin aplicar modificaciones en la base de datos.
5. **Desplazamiento vertical:** La pantalla deberá permitir desplazamiento vertical para visualizar todos los registros, sin bloquear la navegación.
6. **Accesibilidad y persistencia de botones:** Los botones deberán permanecer visibles o ser fácilmente accesibles (fijos en el pie del diálogo modal), incluso cuando existan muchos registros.
7. **Prevención de re-ejecución:** Durante la ejecución de la importación, se deberá evitar que el usuario pueda ejecutar repetidamente la misma operación (inhabilitando los botones mientras `IsBusy`).
8. **Información de resultado:** Una vez finalizada la operación, se deberá informar al usuario si la importación se realizó correctamente o si ocurrió algún error.

---

### 4. Plan de Implementación

- [x] **Paso 1: `CommissionImportViewModel`**
  - Incorporar propiedad `IsNotBusy` y actualizar notificaciones.
  - Exponer propiedades de conveniencia para la vista: `IsNotPreviewStep`, `HasBlockedText`, `HasConflicts`.
  - Actualizar `NextLabel` para devolver `"Aceptar"` en el paso de revisión (`Step == 2`).
  - Agregar `CancelCommand` (`Cancel()`) que invoca `ResetWizard()` y cierra el diálogo (`IsOpen = false`), descartando decisiones sin alterar la BD.
  - Agregar `AcceptCommand` (`AcceptAsync()`) que invoca `ConfirmAsync()`.
  - Proteger contra doble clic (`if (IsBusy) return;`).
  - Disparar evento de éxito `Imported` para que `StudentsViewModel` informe el resultado positivo en `StatusMessage`.
- [x] **Paso 2: `StudentsViewModel`**
  - Suscribirse a `Import.Imported` para reflejar el mensaje de éxito en `StatusMessage`.
- [x] **Paso 3: `StudentsPage.xaml`**
  - Reestructurar el modal `Wizard`:
    - Tarjeta con `MaximumHeightRequest="680"`, `Margin="24"`, `RowDefinitions="Auto,*,Auto"`.
    - Fila 0 (Encabezado fijo): Título "Cargar comisión" y botón de cierre.
    - Fila 1 (Contenido con scroll): `ScrollView` vertical para que los pasos 0, 1 y especialmente la lista de conflictos en el paso 2 se desplacen con fluidez.
    - Fila 2 (Pie de acciones fijo): Separador visual, mensaje de error en rojo y botones de acción.
  - En el paso 2 (revisión de fusión u omisión), disponer los botones **CANCELAR** (`Style="{StaticResource GhostButton}"`) y **ACEPTAR** (`Style="{StaticResource PrimaryButton}"`), deshabilitados cuando `IsBusy == true`.
  - En los botones "Omitir" y "Fusionar" de cada tarjeta de conflicto, añadir triggers para indicar visualmente cuál está seleccionado con `AccentStrong`.
- [x] **Paso 4: Verificación**
  - Compilación limpia de la solución con `dotnet build`: 0 errores, 8 advertencias (sin advertencias bloqueantes).
  - Ejecución de pruebas automatizadas con `scripts\run-tests.cmd`:
    - `GestionEscolar.Data.Tests`: 25 pruebas superadas (0 fallidas).
    - `GestionEscolar.Tests`: 142 pruebas superadas (0 fallidas).
    - Total: 167 pruebas superadas.
  - Verificación de codificación UTF-8 con BOM en archivos modificados.

