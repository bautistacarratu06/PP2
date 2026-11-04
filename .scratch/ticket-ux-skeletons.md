# [UX] Implementar skeletons en todas las grillas de la aplicación

- **Tipo:** Historia de Usuario / Mejora de UX
- **Estado:** Completado
- **Prioridad:** Alta
- **Componentes afectados:** `GestionEscolar/UI` (Controls, Views, ViewModels)

---

## 1. Historia de Usuario

**Como** usuario de la aplicación,  
**quiero** visualizar skeletons o placeholders en todas las grillas mientras se cargan los datos,  
**para** mejorar la percepción de velocidad y evitar que las pantallas se vean vacías o incompletas durante la carga.

---

## 2. Descripción

Implementar un sistema de skeleton loading reutilizable para las diferentes grillas de la aplicación desarrollada con **.NET MAUI**, de manera que cada grilla muestre placeholders con una estructura visual similar a sus datos reales mientras espera la respuesta de la fuente de datos.

La solución deberá ser consistente visualmente en toda la aplicación y permitir su reutilización en las distintas pantallas que contengan grillas.

---

## 3. Criterios de Aceptación

- [x] **Cobertura total:** Todas las grillas de la aplicación deberán mostrar un skeleton mientras sus datos se encuentran en proceso de carga.
  - Nómina (`StudentListPage`)
  - Seguimiento (`TrackingPage`)
  - Registrar Asistencia (`AttendancePage`)
  - Registrar Entregas (`SubmissionsPage`)
  - Calendario (`CalendarPage`)
  - Detalle de alumno (`StudentDetailPage` - historiales de asistencia, entregas e intervenciones)
- [x] **Estructura y dimensiones fidedignas:** El skeleton deberá mantener una estructura y dimensiones similares a las filas y columnas que posteriormente mostrará la grilla (anchos de columna, avatares circulares, píldoras, barras de progreso y alturas de fila).
- [x] **Transición automática a cargado:** El skeleton deberá desaparecer automáticamente cuando finalice correctamente la carga de datos y mostrar los datos reales.
- [x] **Manejo de errores:** Si ocurre un error durante la carga, el skeleton deberá dejar de mostrarse y deberá visualizarse el estado correspondiente de error.
- [x] **Sin estados parciales:** Mientras se muestra el skeleton, no deberán visualizarse datos incompletos o parcialmente cargados ni etiquetas de "vacío" prematuras.
- [x] **Reutilización y modularidad:** La implementación deberá ser reutilizable para evitar desarrollar un skeleton diferente para cada grilla desde cero.
- [x] **Consistencia visual:** El comportamiento y estilo visual (tema oscuro, paleta `Surface`/`Raised`/`Line`, esquinas redondeadas y animación sutil de pulso) deberán ser consistentes en las distintas pantallas de la aplicación.
- [x] **Compatibilidad de arquitectura:** La solución deberá ser 100% compatible con la arquitectura .NET MAUI y el patrón MVVM del proyecto.

---

## 4. Diseño Técnico y Arquitectura

### 4.1 Estados de carga (`GridLoadingState`)
Definir un enum formal `GridLoadingState` con tres estados:
1. `Loading`: Se está consultando la fuente de datos. Se muestran los skeletons y se oculta el contenido real y los errores.
2. `Loaded`: Carga completada con éxito. Se ocultan los skeletons y se muestran los datos (o el estado vacío correspondiente si no hay filas).
3. `Error`: Falló la carga. Se ocultan los skeletons y se muestra el mensaje de error con estilo `Danger`.

### 4.2 Componentes reutilizables (`UI/Controls`)
1. **`SkeletonBlock`**:
   - Elemento visual atómico derivado de `Border`.
   - Propiedades: `IsCircle` (para avatares y badges circulares), `Radius` (radio de curvatura), `WidthRequest`, `HeightRequest`.
   - Color base acorde al tema oscuro (`Raised`/`LineStrong`).
2. **`SkeletonGrid`**:
   - Contenedor repetidor derivado de `VerticalStackLayout`.
   - Propiedades: `RowCount` (número de filas skeleton a renderizar, por defecto 5), `RowHeight`, `RowTemplate` (DataTemplate de la fila skeleton), `SeparatorColor` (color del divisor entre filas).
   - Animación de respiración / pulso de opacidad sincronizada (de 0.40 a 0.95) en un ciclo suave infinito mientras esté visible, deteniéndose limpiamente al desmontar.
3. **`GridLoadingView`**:
   - Contenedor de estado con propiedad de contenido (`[ContentProperty(nameof(Content))]`).
   - Propiedades vinculables: `IsLoading`, `HasError`, `ErrorMessage`, `State`, `SkeletonView`, `Content`, `ErrorView`.
   - Gestiona la visibilidad mutua y garantiza que nunca se muestren skeletons y datos incompletos a la vez.

### 4.3 Grillas a intervenir
| Pantalla | Columnas / Estructura del Skeleton | Altura de Fila |
|---|---|---|
| **Nómina** (`StudentListPage`) | Avatar (32x32 circular) + Apellido y Nombre (140px), Legajo (70px), Teléfono (90px), Correo (160px). ColumnDefinitions: `2.2*, 130, 150, 2*`. | 64px |
| **Seguimiento** (`TrackingPage`) | Avatar (32x32) + Nombre (130px) / Legajo (60px), RiskPill (64x22), Meter Asistencia (100x8), Meter Entregas (100x8), Contacto (70px). ColumnDefinitions: `2.2*, 110, 1.3*, 1.3*, 110`. | 64px |
| **Asistencia** (`AttendancePage`) | Avatar (32x32) + Nombre (140px), SegmentedControl skeleton (164x32). ColumnDefinitions: `*, Auto`. | 60px |
| **Entregas** (`SubmissionsPage`) | Avatar (32x32) + Nombre (140px), SegmentedControl skeleton (164x32). ColumnDefinitions: `*, Auto`. | 60px |
| **Calendario** (`CalendarPage`) | Título/Nombre (180px), StatusPill (64x22), Fecha (80px). ColumnDefinitions: `*, Auto, 110`. | 54px |
| **Detalle Alumno** (`StudentDetailPage`) | Historial Asistencia, Historial Entregas, Intervenciones (filas con texto y pill). | 44px |

---

## 5. Plan de Tareas / Checklist de Implementación

- [x] **Paso 1: Crear componentes reutilizables de Skeleton y Loading State**
  - [x] Crear `UI/Controls/GridLoadingState.cs`
  - [x] Crear `UI/Controls/SkeletonBlock.cs`
  - [x] Crear `UI/Controls/SkeletonGrid.cs`
  - [x] Crear `UI/Controls/GridLoadingView.cs`
- [x] **Paso 2: Ajustar ViewModels para estado inicial de carga consistente**
  - [x] Reseteo de `HasNoClasses` en cambio de comisión en `AttendanceViewModel`.
  - [x] Reseteo de `HasNoAssignments` en cambio de comisión en `SubmissionsViewModel`.
  - [x] Reseteo de `HasNoItems` en cambio de comisión en `CalendarViewModel`.
- [x] **Paso 3: Integrar `GridLoadingView` y skeletons en las vistas**
  - [x] `StudentListPage.xaml`
  - [x] `TrackingPage.xaml`
  - [x] `AttendancePage.xaml`
  - [x] `SubmissionsPage.xaml`
  - [x] `CalendarPage.xaml`
  - [x] `StudentDetailPage.xaml`
- [x] **Paso 4: Pruebas y Verificación**
  - [x] Compilación limpia con `dotnet build GestionEscolar\UI\UI.csproj -f net10.0-windows10.0.19041.0` (0 errores)
  - [x] Ejecución de la suite completa de pruebas: `GestionEscolar.Tests` (118 superadas) y `GestionEscolar.Data.Tests` (17 superadas)
  - [x] Verificación de formato (`dotnet format GestionEscolar\GestionEscolar.slnx --verify-no-changes`)
