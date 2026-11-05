# [UX/UI] Implementar selección de filas, Action Bar flotante y unificar cabeceras fijas en grillas

- **Tipo:** Historia de Usuario / Mejora de UX/UI
- **Estado:** Pendiente / Listo para desarrollo (`ready-for-agent`)
- **Prioridad:** Alta
- **Componentes afectados:** `GestionEscolar/UI` (Controls, Views, ViewModels, Behaviors/Converters), `GestionEscolar/GestionEscolar.Core` (Servicios y modelos de marcado masivo)

---

## 1. Historia de Usuario

**Como** usuario del sistema,  
**quiero** poder seleccionar una o múltiples filas en las distintas grillas de la aplicación (Nómina, Asistencia, Entregas, Seguimiento, etc.) y visualizar una barra de acciones (Action Bar) dinámica,  
**para** poder ejecutar operaciones en bloque de forma rápida y segura. Además, se requiere estandarizar el comportamiento de las cabeceras de todas las grillas para que queden fijas al hacer scroll.

---

## 2. Descripción General y Contexto

Actualmente, las grillas de la aplicación presentan diferencias de usabilidad:
1. En **Asistencia** y **Entregas**, la edición se hace fila por fila mediante controles directos (`SegmentedControl`), lo que vuelve lenta y propensa a errores la carga masiva en cursos con decenas de estudiantes.
2. En la vista de **Seguimiento** y en la mayoría de las grillas, al scrollear hacia abajo, la cabecera de las columnas (`ColumnHeader`) desaparece junto con el contenido al estar todo dentro de un `ScrollView` global en `PageShell`, perdiendo el contexto de qué columna corresponde a cada dato.
3. No existe un mecanismo genérico y estandarizado para selección múltiple (checkbox maestro e individuales) ni un componente flotante de acciones masivas contextuales.

Esta iniciativa introduce:
- **Estandarización de cabeceras fijas ("sticky headers")**: la fila de nombres de columna permanece visible y anclada mientras el listado de filas scrollea fluidamente debajo.
- **Sistema de selección múltiple contextual**: checkbox maestro en la cabecera y checkboxes en cada fila, condicionados a si la vista ofrece operaciones masivas.
- **Action Bar flotante inferior (estilo Linear)**: panel con elevación que se sobrepone suavemente en la parte inferior de la ventana cuando hay 1 o más elementos seleccionados, exponiendo las acciones contextuales (ej. "Marcar Presentes", "Marcar Ausentes", "Marcar Entregó", etc.).
- **Modal de confirmación transaccional**: antes de aplicar cualquier acción en bloque, se previene la ejecución inmediata mostrando un diálogo de confirmación con el conteo dinámico de filas afectadas, descripción de la acción y botones de "Aceptar" / "Cancelar".

---

## 3. Criterios de Aceptación Detallados

### 3.1 Comportamiento General de Grillas (Checkbox y Cabecera Fija)
- [ ] **Checkbox por fila:** Cada fila de datos debe incluir un control de selección (`CheckBox` estilizado al tema oscuro).
- [ ] **Cabecera fija (Sticky Header):** Al hacer scroll vertical sobre los registros, la cabecera de columnas debe permanecer siempre fija y visible en la parte superior del contenedor de la grilla.
- [ ] **Checkbox maestro:** La cabecera fija debe incluir un checkbox maestro para "Seleccionar todos".
  - Si se marca el maestro, todas las filas visibles filtradas pasan a estado seleccionado.
  - Si se desmarca, se deseleccionan todas las filas.
  - Si se seleccionan manualmente todas las filas, el maestro se marca automáticamente.
  - Si se desmarca una fila individual estando todas seleccionadas, el maestro pasa a desmarcado (o estado indeterminado si la plataforma lo soporta).
- [ ] **Refactorización Seguimiento:** La vista de Seguimiento (`TrackingPage`) debe adoptar el mismo comportamiento de cabecera fija que el resto de las pantallas.
- [ ] **Condición de visibilidad de checkboxes:**
  - Si la vista actual **no** tiene acciones masivas disponibles para realizar (o la selección está deshabilitada por configuración/permiso), **no se deben renderizar los checkboxes** (ni en las filas ni en la cabecera).
  - La cabecera de columnas en sí misma debe mantenerse visible y fija, conservando la alineación exacta con las columnas de las filas.

### 3.2 Comportamiento del Action Bar Flotante
- [ ] **Aparición y ocultamiento reactivo:** El Action Bar debe emerger automáticamente con una transición fluida al seleccionar 1 o más filas, y desaparecer al deseleccionar todos los elementos.
- [ ] **Acciones contextuales:** El Action Bar debe presentar únicamente los botones de comando correspondientes a la pantalla actual.
- [ ] **Contador de seleccionados:** Debe reflejar en tiempo real la cantidad de elementos seleccionados (ej.: *"3 estudiantes seleccionados"*).
- [ ] **Botón de descarte rápido:** Debe incluir una opción o icono para "Deseleccionar todos" directamente desde el Action Bar.
- [ ] **Diseño y posicionamiento:** Debe flotar en la parte inferior central de la vista, con fondo elevado (`Surface`/`Raised`), borde sutil (`Line`), esquinas redondeadas (`12px` - `14px`) y sombra/elevación acorde a la estética del sistema (estilo Linear).

### 3.3 Refactorización de Vistas Específicas (Asistencia y Entregas)
- [ ] **Asistencia (`AttendancePage`):**
  - Eliminar los selectores/switches interactivos por fila (`SegmentedControl`).
  - Incorporar una columna **"Estado"** de solo lectura que muestre la condición actual del estudiante mediante un badge o etiqueta estilizada (`StatusPill` o similar: "Presente", "Ausente", o "Sin registro").
  - Las modificaciones de estado se ejecutan exclusivamente seleccionando filas y pulsando las acciones masivas del Action Bar: *"Marcar como Presente"*, *"Marcar como Ausente"*.
- [ ] **Entregas (`SubmissionsPage`):**
  - Eliminar los selectores interactivos por fila.
  - Incorporar una columna **"Estado"** de solo lectura que refleje si "Entregó", "No entregó" o "Sin registro".
  - Las operaciones se ejecutan seleccionando filas y pulsando las acciones masivas del Action Bar: *"Marcar como Entregó"*, *"Marcar como No entregó"*.

### 3.4 Modal de Confirmación de Acciones
- [ ] **Bloqueo preventivo:** Al pulsar cualquier acción masiva en el Action Bar, se debe suspender la mutación directa en la base de datos y abrir un diálogo/modal de confirmación modal.
- [ ] **Contenido obligatorio del modal:**
  - **Título/Leyenda clara:** Descripción explícita de la operación a ejecutar (ej.: *"Actualizar asistencia masiva"*).
  - **Mensaje con contador dinámico:** Ejemplo: *"Estás a punto de marcar como Presente a 12 estudiantes."*
  - **Botón Primario ("Aceptar"):** Ejecuta la operación en bloque en la capa de datos/servicios, actualiza la UI y cierra el modal.
  - **Botón Secundario ("Cancelar"):** Cierra el modal sin realizar ninguna modificación ni perder la selección previa.
- [ ] **Manejo de estado durante ejecución:** Mientras la operación en bloque se aplica en base de datos, se debe mostrar indicador de ocupado (`IsBusy`) y deshabilitar los botones para evitar dobles envíos.

---

## 4. Diseño Técnico y Arquitectura (.NET MAUI)

### 4.1 Estructura del Layout y Sticky Header
Para evitar que la cabecera scrollee junto con el resto de la página dentro del `PageShell`:
1. El contenedor dentro de la tarjeta (`Border Style="{StaticResource Card}"`) se organiza mediante una `Grid` de dos filas de contenido:
   - `RowDefinition Height="Auto"`: Contiene la cabecera fija (`Grid` con checkbox maestro y `Label Style="{StaticResource ColumnHeader}"`).
   - `RowDefinition Height="*"`: Contiene el área desplazable (`ScrollView` vertical interno o `CollectionView`) que aloja las filas de datos.
2. Esta estructura garantiza que la cabecera no se mueva sin importar cuántos registros se desplacen hacia abajo.
3. Se mantiene sincronizado el ancho de columnas mediante `ColumnDefinitions` compartidas o idénticas entre la cabecera y las filas.

### 4.2 Componente Flotante `FloatingActionBar` (`UI/Controls/FloatingActionBar.xaml`)
- Un control reutilizable embebido en la capa superior (`ZIndex` alto) de la vista o en una `Grid` raíz superpuesta.
- **Propiedades Bindables:**
  - `IsActive` (`bool`): Controla visibilidad y animación de entrada/salida.
  - `SelectedCount` (`int`): Cantidad de filas seleccionadas.
  - `SelectedCountText` (`string`): Texto formateado (ej. *"5 seleccionados"*).
  - `ClearSelectionCommand` (`ICommand`): Desmarca todas las filas.
  - `ActionsContent` (`View`): Permite inyectar los botones de acción contextuales de cada pantalla (DataTemplate o ContentPresenter).

### 4.3 Componente Modal `ConfirmationModal` (`UI/Controls/ConfirmationModal.xaml`)
- Diálogo modal con overlay oscuro semitransparente.
- **Propiedades Bindables:**
  - `IsOpen` (`bool`)
  - `Title` (`string`)
  - `Message` (`string`)
  - `ConfirmText` (`string`, por defecto "Aceptar")
  - `CancelText` (`string`, por defecto "Cancelar")
  - `ConfirmCommand` (`ICommand`)
  - `CancelCommand` (`ICommand`)
  - `IsBusy` (`bool`)

### 4.4 Modelado y Binding en ViewModels
1. **Fila seleccionable genérica / wrapper:**
   - Para las filas (`StatusRow`, `StudentRow`), incorporar la propiedad `[ObservableProperty] private bool _isSelected;`.
   - Alternativamente, un wrapper observable `SelectableItem<T>` con `Item` e `IsSelected`.
2. **Propiedades reactivas en ViewModels:**
   - `SelectedItemsCount` (calculado a partir de los elementos donde `IsSelected == true`).
   - `HasSelection => SelectedItemsCount > 0;`.
   - `IsAllSelected` (`bool?` o `bool` con lógica bidireccional para el checkbox maestro).
   - `CanPerformBulkActions` (`bool`, indica si la pantalla ofrece acciones masivas; si es `false`, oculta los checkboxes en vista y cabecera).
3. **Servicios de negocio (Core):**
   - Asegurar que `AttendanceService` y `SubmissionService` permitan guardar lotes eficientemente (guardado en bloque o secuencial optimizado sin bloqueos de UI).

---

## 5. Desglose en Vertical Slices (Tracer Bullets)

| # | Ticket | Dependencias | Qué entrega |
|---|---|---|---|
| **01** | **Fundación: Sticky Header y Selección Múltiple** | Ninguna | Grillas con cabecera fija desacoplada del scroll, soporte de checkbox maestro y checkbox por fila condicionado a `CanPerformBulkActions`. |
| **02** | **Componentes UI: Floating Action Bar y Modal de Confirmación** | Ninguna | Controles MAUI reutilizables `FloatingActionBar` (estilo Linear con animación de entrada) y `ConfirmationModal` con bloqueo y conteo dinámico. |
| **03** | **Refactorización de Asistencia con Acciones en Bloque** | 01, 02 | Asistencia migrada a columna Estado solo lectura, selección múltiple y Action Bar con confirmación para "Presente" y "Ausente". |
| **04** | **Refactorización de Entregas con Acciones en Bloque** | 01, 02 | Entregas migrada a columna Estado solo lectura, selección múltiple y Action Bar con confirmación para "Entregó" y "No entregó". |
| **05** | **Estandarización de Seguimiento y Nómina** | 01 | Cabeceras fijas unificadas en Seguimiento y Nómina, con checkboxes ocultos o activos según la disponibilidad de operaciones masivas. |

---

## 6. Checklist de Implementación y Verificación

- [x] **Paso 1: Crear Controles Reutilizables de UI**
  - [x] `UI/Controls/FloatingActionBar.xaml` (+ code-behind)
  - [x] `UI/Controls/ConfirmationModal.xaml` (+ code-behind)
- [x] **Paso 2: Soporte de Selección en Modelos de UI / ViewModels**
  - [x] Agregar `IsSelected` y callback de selección en `StatusRow` y `StudentRow`.
  - [x] Soporte de `SelectAllCommand`, `ClearSelectionCommand` y `HasSelection` en `AttendanceViewModel` y `SubmissionsViewModel`.
- [x] **Paso 3: Unificar Sticky Headers en Vistas**
  - [x] Adaptar `AttendancePage.xaml`
  - [x] Adaptar `SubmissionsPage.xaml`
  - [x] Adaptar `TrackingPage.xaml`
  - [x] Adaptar `StudentListPage.xaml`
- [x] **Paso 4: Migración de Estado y Acciones Masivas en Asistencia y Entregas**
  - [x] Reemplazar `SegmentedControl` por `StatusPill` / columna de texto de solo lectura.
  - [x] Conectar botones del Action Bar a comandos de apertura del modal de confirmación.
  - [x] Conectar confirmación con invocación en bloque a `AttendanceService.SaveAsync` / `SubmissionService.SaveAsync`.
- [x] **Paso 5: Pruebas y Control de Calidad**
  - [x] Integración de componentes y bindings reactivos en MVVM.
  - [x] Verificación de suite de tests en Core y Data (135 tests aprobados).

