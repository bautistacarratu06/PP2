# Gestión Escolar PP2 · Guía de componentes reutilizables (.NET MAUI)

Este documento complementa a `ui-revamp.md`. Aquel define **cómo se ve** la app (tokens, medidas, pantallas). Este define **qué piezas de código hay que crear** para construir esas pantallas sin repetir XAML, con nombres, API pública y estados.

La referencia visual de cada componente es el mockup `Gestión Escolar · Mockup.html`. Si hay dudas de apariencia, ganan el mockup y `ui-revamp.md`. La ubicación de los archivos sigue el proyecto UI que ya existe. Las historias de `.scratch/look-and-feel/issues/` cortan el trabajo; este documento no manda un orden de carpetas.

---

## 1. Principios de arquitectura

1. **Los componentes viven solo en el proyecto de UI** y no conocen `Core` ni `Data`. Reciben primitivas (`string`, `int`, `double`, enums, colecciones) o pequeños *display models* de UI. Los ViewModels son quienes transforman entidades de `Core` en esos modelos.
2. **Un componente = una responsabilidad visual.** Si dos pantallas dibujan lo mismo, debe ser un componente. Si solo una pantalla lo usa y es trivial, puede quedar como `DataTemplate` local.
3. **Todo estilo sale de los tokens** (`Colors.xaml`, `Styles.xaml`). Ningún componente tiene colores, tamaños de fuente ni radios hardcodeados. Si falta un token, se agrega al diccionario.
4. **API declarativa con `BindableProperty`.** Cada propiedad pública configurable debe ser enlazable, con valor por defecto sensato.
5. **Compiled bindings siempre** (`x:DataType`), tanto en componentes como en pantallas.
6. **Estados explícitos.** Hover, foco, activo, deshabilitado y vacío se resuelven con `VisualStateManager`, no con código imperativo disperso.
7. **Sin lógica de negocio.** Fórmula de riesgo, umbrales y filtros viven en `Core`. La UI solo recibe el resultado (`RiskLevel`, `Score`) y lo muestra.
8. **Reutilizar antes que crear.** Cada componente nuevo debe componerse con los anteriores (por ejemplo `StudentRow` usa `Avatar`, `RiskPill` y `Meter`).

---

## 2. Dónde viven y convenciones

No hay un árbol de carpetas nuevo. El proyecto UI ya tiene `Views/`, `ViewModels/`, `Converters/` y `Resources/Styles/`. Los controles reutilizables se agregan ahí al lado, en el namespace `UI` (hoy las páginas son `UI.Views`). `Colors.xaml` y `Styles.xaml` ya existen: los tokens y los estilos con nombre van en esos diccionarios. Inter y JetBrains Mono van con las fuentes que ya registra la app.

Convenciones:
- Nombres de estilos nombrados en PascalCase, sin prefijos (`PageTitle`, `PrimaryButton`).
- Cada control reutilizable es un `ContentView` con su `.xaml` + `.xaml.cs` (o `TemplatedView` cuando necesite `ControlTemplate`).
- No se duplica `RiskLevel`: el nivel que se muestra es el enum de Core. Un color de estado de la UI (`Success`, `Warning`, `Danger`, `Neutral`) puede vivir junto a los convertidores. `Warning` es el ámbar del riesgo medio, no un estado de asistencia ni de entrega.

Patrón base de una propiedad enlazable:

```csharp
public static readonly BindableProperty TextProperty =
    BindableProperty.Create(nameof(Text), typeof(string), typeof(Tag), string.Empty);

public string Text
{
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
}
```

---

## 3. Catálogo resumido

| # | Componente | Reemplaza / resuelve | Usado en |
|---|---|---|---|
| 1 | `Icon` | Iconos de línea (menú, búsqueda, chevrons) | Sidebar, búsqueda, volver |
| 2 | `Card` | `Border` repetido con borde y radio | Todas |
| 3 | `PageShell` | Encabezado distinto en cada página | Todas |
| 4 | `CommissionPicker` | Combo "Elegí una comisión" duplicado | Todas (vía `PageShell`) |
| 5 | `SidebarNav` | Menú lateral sin iconos | `AppShell` |
| 6 | `CardToolbar` | Fila de filtros arriba de la tabla | Nómina, Seguimiento, Entregas, Asistencia |
| 7 | `SearchBox` | — | Nómina, Seguimiento |
| 8 | `FilterChips` | — | Seguimiento |
| 9 | `SegmentedControl` | Dropdown por fila | Entregas, Asistencia |
| 10 | `AppButton` | Botones teal sueltos | Calendario, Ajustes, Detalle, Asistencia |
| 11 | `FormField` | Etiqueta + input | Calendario, Ajustes |
| 12 | `Avatar` | Ícono de persona violeta | Tablas, Detalle |
| 13 | `StudentIdentityCell` | Nombre + legajo | Nómina, Seguimiento, Entregas, Asistencia |
| 14 | `RiskPill` | Columnas Nivel + Riesgo | Seguimiento, Detalle |
| 15 | `Meter` | Porcentajes como texto | Seguimiento |
| 16 | `Tag` | Tipo de evento, pesos | Calendario, Ajustes |
| 17 | `StatusPill` | "Sin registro" / estados | Detalle |
| 18 | `KpiCard` | Tarjetas teñidas de riesgo | Seguimiento |
| 19 | `ScoreBadge` | Puntaje grande con nivel | Detalle |
| 20 | `MetricCard` | Bloque Asistencia/Entregas con barra | Detalle |
| 21 | `KeyValueBlock` | Teléfono / Correo | Detalle |
| 22 | `HistoryRow` | Fila de historial | Detalle |
| 23 | `RegistrationProgress` | Feedback de registro | Entregas, Asistencia |
| 24 | `EmptyState` | Lista sin resultados | Todas las listas |
| 25 | `TableHeader` | Encabezado de columnas | Tablas |

---

## 4. Fundación

### 4.1 `Icon`
Muestra un icono de línea por nombre, con tamaño y color configurables.

| Propiedad | Tipo | Defecto | Notas |
|---|---|---|---|
| `Name` | `string` | — | `users`, `activity`, `inbox`, `check-circle`, `calendar`, `sliders`, `search`, `chevron-left`, `x` |
| `Size` | `double` | 17 | Ancho y alto |
| `Color` | `Color` | `TextSecondary` | Trazo |
| `StrokeWidth` | `double` | 1.7 | Trazo con extremos redondeados |

Implementación sugerida: `Microsoft.Maui.Controls.Shapes.Path` con `Stroke`, `StrokeLineCap="Round"`, `StrokeLineJoin="Round"`, `Fill="Transparent"`. Las geometrías se guardan en `Icons.xaml` como cadenas de path y se convierten con `PathGeometryConverter`. Un diccionario `IconCatalog` mapea nombre → geometría. Si el trazo de un icono complejo da problemas, la alternativa válida para esta app Windows-only es la fuente **Segoe Fluent Icons**: la API pública de `Icon` no cambia, solo su interior.

### 4.2 `Card`
Contenedor con superficie, borde y radio estándar. Evita repetir `Border` + `StrokeShape` en cada pantalla.

| Propiedad | Tipo | Defecto |
|---|---|---|
| `Content` | `View` | — |
| `Padding` | `Thickness` | 0 |
| `IsPadded` | `bool` | `false` (si es `true`, padding 20) |
| `CornerRadius` | `double` | 14 |
| `Background` | `Color` | `Surface` |

Interno: `Border` con `Stroke = Border`, `StrokeThickness = 1`, `StrokeShape = RoundRectangle` y recorte del contenido. **No usar `Frame`.**

---

## 5. Estructura y navegación

### 5.1 `PageShell` (el más importante)
Contenedor común de todas las páginas. Garantiza que cada vista tenga título, subtítulo y selector de Comisión, con los mismos márgenes y ancho máximo.

| Propiedad | Tipo | Notas |
|---|---|---|
| `Title` | `string` | H1 |
| `Subtitle` | `string` | Una línea, Secondary |
| `Content` | `View` | El cuerpo de la página (se inyecta vía `ContentPresenter`) |

Reglas de implementación:
- Es un `ContentView` con `ControlTemplate` definido en `Styles.xaml` (estilo implícito sobre `PageShell`).
- El template contiene: `ScrollView` → `Grid` centrado con `MaximumWidthRequest="1160"`, `Padding="40,36,40,56"` → fila de encabezado (título + subtítulo a la izquierda, `CommissionPicker` a la derecha, alineado abajo) → `ContentPresenter` con 28 de separación.
- **El selector de Comisión no es opcional ni configurable por página.** Está siempre presente.
- En ventanas angostas (menos de ~860px) el padding baja a `18,24` y el selector pasa debajo del título.

```xml
<ControlTemplate x:Key="PageShellTemplate">
  <ScrollView>
    <Grid HorizontalOptions="Center" MaximumWidthRequest="1160"
          Padding="40,36,40,56" RowDefinitions="Auto,*" RowSpacing="28">
      <Grid ColumnDefinitions="*,Auto" ColumnSpacing="24">
        <VerticalStackLayout Spacing="4">
          <Label Text="{TemplateBinding Title}" Style="{StaticResource PageTitle}" />
          <Label Text="{TemplateBinding Subtitle}" Style="{StaticResource PageSubtitle}" />
        </VerticalStackLayout>
        <ui:CommissionPicker Grid.Column="1" VerticalOptions="End" />
      </Grid>
      <ContentPresenter Grid.Row="1" />
    </Grid>
  </ScrollView>
</ControlTemplate>
```

Uso en una pantalla:

```xml
<ContentPage ...>
  <ui:PageShell Title="Nómina" Subtitle="Estudiantes inscriptos en la comisión.">
    <ui:Card> ... </ui:Card>
  </ui:PageShell>
</ContentPage>
```

Atención: un `CollectionView` dentro de un `ScrollView` pierde virtualización. Para nóminas de cientos de filas, darle al `CollectionView` una altura propia o paginar. Para comisiones de ~30 estudiantes es aceptable.

### 5.2 `CommissionPicker`
Selector destacado de comisión, conectado al contexto global.

El selector muestra la comisión elegida de la ventana y escribe sobre esa misma selección. El modelo es el `Section` que las pantallas ya usan. No se crea un `CommissionItem` ni un servicio con otro nombre de dominio.

Reglas:
- Hay una sola comisión seleccionada. Cambiarla recarga la vista visible. Nómina deja de cargar siempre la comisión 1. En Ajustes el selector se ve, y los umbrales siguen siendo globales.
- Aspecto: fondo `Raised`, borde `Border2`, radio 10, punto de 8px en `Accent`, etiqueta "Comisión" y valor con chevron. Es el control más prominente del encabezado.
- Implementación sugerida: `Border` clicable que abre un `Popup` (CommunityToolkit.Maui) o un `Picker` estilizado oculto debajo. No exponer el `Picker` por defecto con título flotante.

### 5.3 `SidebarNav`
Barra lateral colapsable con iconos.

| Propiedad | Tipo | Defecto |
|---|---|---|
| `Items` | `IList<NavItem>` | — |
| `ActiveRoute` | `string` | — |
| `IsCollapsed` | `bool` | `false` |
| `NavigateCommand` | `ICommand` | — |
| `ExpandedWidth` / `CollapsedWidth` | `double` | 232 / 64 |

`NavItem` (display model): `Route`, `Title`, `IconName`.

Comportamiento:
- Ítem activo: fondo `Raised`, texto `TextPrimary`, icono en `Accent` y barra de 3px a la izquierda.
- Hover: fondo `Surface`.
- Pie con botón "Colapsar" y chevron que gira 180° al colapsar. El ancho se anima 200 ms con `Animation`.
- En modo colapsado, ocultar etiquetas y marca, y mostrar `ToolTipProperties.Text` con el nombre.
- Debe seguir resaltando "Seguimiento" cuando se está en el Detalle del alumno (se le pasa `ActiveRoute` del padre, no la ruta literal).
- Se usa una sola vez, en `AppShell`/ventana principal, pero se extrae como componente para poder previsualizarlo y testearlo.

### 5.4 `CardToolbar`
Fila superior de una `Card` con controles (búsqueda, chips, selectores, conteo, acciones).

| Propiedad | Tipo | Notas |
|---|---|---|
| `Content` | `View` | Normalmente un `FlexLayout` o `HorizontalStackLayout` |

Padding 14/16, borde inferior de 1px, elementos con gap 12 y ajuste de línea. Se recomienda `FlexLayout Wrap="Wrap" AlignItems="Center"` como contenido para conseguir el wrap.

### 5.5 `EmptyState`
Texto centrado en `TextTertiary` con padding 48.

| Propiedad | Tipo | Defecto |
|---|---|---|
| `Message` | `string` | "Sin resultados para esa búsqueda." |
| `IsVisible` | `bool` | Se enlaza a "lista vacía" |

---

## 6. Entradas y acciones

### 6.1 `SearchBox`
| Propiedad | Tipo | Notas |
|---|---|---|
| `Text` | `string` | `TwoWay` |
| `Placeholder` | `string` | "Buscar por nombre o legajo" |
| `Debounce` | `TimeSpan` | 150 ms recomendado |

Fondo `Bg`, borde `Border`, radio 9, icono de lupa a la izquierda. El borde pasa a `Accent` cuando el campo tiene foco (estado visual de `Entry`/contenedor).

### 6.2 `FilterChips`
| Propiedad | Tipo | Notas |
|---|---|---|
| `Options` | `IList<ChipOption>` | `Key`, `Label` |
| `SelectedKey` | `string` | `TwoWay` |

Contenedor con fondo `Bg`, borde `Border`, radio 9, padding 3. Chip activo con fondo `Raised`. Para Seguimiento: Todos, Alto, Medio, Bajo. Debe sincronizarse con las `KpiCard`: ambos escriben el mismo `SelectedLevel` del ViewModel.

### 6.3 `SegmentedControl`
Control de dos opciones con color semántico por opción. Reemplaza los dropdowns de Entregas y Asistencia. No se agrega una tercera opción.

| Propiedad | Tipo | Notas |
|---|---|---|
| `Options` | `IList<SegmentOption>` | `Key`, `Label`, `Kind` (`StatusKind`) |
| `SelectedKey` | `string?` | `TwoWay`; `null` = "Sin registro" |
| `AllowDeselect` | `bool` | `true`: tocar la opción activa la quita |

Aspecto: contenedor como `FilterChips`; opción activa con fondo `*Soft` y texto del color pleno de su `Kind`; inactivas en `TextSecondary`. Para asistencia: Presente (Success) y Ausente (Danger). Para entregas: Entregó (Success) y No entregó (Danger). No existe "Tarde". Sin selección, ambos quedan en gris ("Sin registro"). Deseleccionar solo vuelve a vacío si el guardado actual lo admite.

### 6.4 `AppButton`
| Propiedad | Tipo | Defecto |
|---|---|---|
| `Text` | `string` | — |
| `Kind` | `ButtonKind` | `Primary` (`Primary`, `Ghost`, `Link`) |
| `IconName` | `string?` | — |
| `Command` / `CommandParameter` | — | — |
| `IsCompact` | `bool` | `false` |

- `Primary`: fondo `AccentStrong`, texto blanco. Compacto: padding 8/16; normal: 11/14.
- `Ghost`: transparente, borde `Border2`; hover `Raised`.
- `Link`: texto `TextSecondary` con chevron-izquierda (botón "volver").
- Estados: hover (brillo +8 %), presionado, deshabilitado (opacidad 0.5).

### 6.5 `FormField`
Etiqueta + contenido de campo + mensaje opcional.

| Propiedad | Tipo | Notas |
|---|---|---|
| `Label` | `string` | 13 Medium Secondary |
| `Content` | `View` | `Entry`, `DatePicker`, `Picker`, etc. con el estilo `Field` |
| `HelperText` | `string?` | Tertiary |
| `ErrorText` | `string?` | Danger; si no es nulo, el borde del campo pasa a `Danger` |

Los estilos implícitos de `Entry`, `DatePicker` y `Picker` (fondo `Bg`, borde, radio 9, foco en `Accent`) se definen en `Styles.xaml` para que `FormField` solo componga.

---

## 7. Visualización de datos

### 7.1 `Avatar`
| Propiedad | Tipo | Defecto |
|---|---|---|
| `FirstName`, `LastName` | `string` | Se muestran las iniciales |
| `Size` | `double` | 34 (56 en el Detalle) |

Círculo con fondo `Raised`, borde `Border2` e iniciales en SemiBold. El tamaño de la fuente es `Size × 0,35`. Sin imágenes.

### 7.2 `StudentIdentityCell`
Compone `Avatar` + "Apellido, Nombre" (14 SemiBold) + legajo en mono 11 `TextTertiary` (sin prefijo "Legajo").

| Propiedad | Tipo |
|---|---|
| `LastName`, `FirstName`, `FileNumber` | `string` |

### 7.3 `RiskPill`
| Propiedad | Tipo | Notas |
|---|---|---|
| `Level` | `RiskLevel` | Define el color |
| `Score` | `int?` | Si es `null`, se muestra solo el nivel (Detalle) |

Cápsula con fondo `*Soft`, punto de 6px, texto del nivel y puntaje tabular. Los colores se resuelven con `RiskLevelToColorConverter` y `RiskLevelToSoftColorConverter`, nunca con `if` en el ViewModel.

### 7.4 `StatusPill`
Igual que `RiskPill` pero con texto libre y color de estado: `Presente`/`Entregó` (Success), `Ausente`/`No entregó` (Danger), `Sin registro` (Neutral). No hay cápsula "Tarde". El ámbar queda para el riesgo medio.

| Propiedad | Tipo |
|---|---|
| `Text` | `string` |
| `Kind` | `StatusKind` |

`RiskPill` puede implementarse reutilizando `StatusPill` internamente.

### 7.5 `Meter`
| Propiedad | Tipo | Notas |
|---|---|---|
| `Value` | `double` | 0–1 |
| `ShowLabel` | `bool` | Muestra el porcentaje a la derecha (34 de ancho, tabular) |
| `Height` | `double` | 5 (6 en el Detalle) |
| `FillColor` | `Color` | `TextSecondary` por defecto |
| `MaxBarWidth` | `double` | 110 en tablas; sin límite en tarjetas |

Implementar con `Grid` y dos `Border` redondeados (pista y relleno) para controlar el radio exacto; el ancho del relleno se actualiza en `SizeChanged`.

### 7.6 `Tag`
Cápsula chica: `Text` y `Kind` (`Neutral`, `Accent`). "Clase" = Neutral, "Trabajo" = Accent. Los pesos de la fórmula usan Neutral con texto `TextPrimary`.

### 7.7 `KpiCard`
| Propiedad | Tipo | Notas |
|---|---|---|
| `Label` | `string` | "Riesgo alto" |
| `Value` | `int` | Número grande |
| `Total` | `int` | "de N" |
| `Level` | `RiskLevel` | Color del punto y del estado activo |
| `IsActive` | `bool` | Borde y degradado sutil del color del nivel |
| `Command` | `ICommand` | Alterna el filtro |

Es clicable. En reposo es neutra (sin fondo ni borde de color). Usa `Card` internamente.

### 7.8 `ScoreBadge`
Puntaje de 52 de tamaño en el color del nivel, con `RiskPill` (sin puntaje) a su lado. Props: `Score`, `Level`.

### 7.9 `MetricCard`
Bloque interno del detalle: título a la izquierda, "X de N · P %" a la derecha y un `Meter` de 6px debajo. Props: `Title`, `Done`, `Total`. Radio 12, fondo `Surface`, borde 1px, padding 14/16.

### 7.10 `KeyValueBlock`
Etiqueta en MAYÚSCULAS 11 `TextTertiary` y valor debajo. Props: `Label`, `Value`, `ValueKind` (`Text` o `Accent`), `Selectable`. El valor debe cortar líneas largas (correos) sin desbordar.

### 7.11 `HistoryRow`
Texto a la izquierda y `StatusPill` a la derecha, padding vertical 12 y separador inferior (oculto en la última fila). Props: `Text`, `StatusText`, `StatusKind`, `ShowDivider`. Se usa dentro de `BindableLayout` para los historiales.

### 7.12 `RegistrationProgress`
Barra de 5px (140 de ancho, relleno `Accent`) + texto "X de N registrados". Props: `Done`, `Total`. Usa `Meter` con `FillColor = Accent` y `MaxBarWidth = 140`.

### 7.13 `TableHeader`
Fila de encabezado de columnas: 38px de alto, texto 11 SemiBold MAYÚSCULAS `TextTertiary`, borde inferior. Props: `Columns` (`IList<string>`) y `ColumnDefinitions`.

**Alineación encabezado/filas:** la grilla de columnas se define **una sola vez** como `ColumnDefinitionCollection` con `x:Shared="False"` en un diccionario, y se aplica tanto al encabezado como a la fila de datos:

```xml
<ColumnDefinitionCollection x:Key="SeguimientoColumns" x:Shared="False">
  <ColumnDefinition Width="2.2*" /><ColumnDefinition Width="110" />
  <ColumnDefinition Width="1.3*" /><ColumnDefinition Width="1.3*" />
  <ColumnDefinition Width="110" />
</ColumnDefinitionCollection>
```

---

## 8. Cómo se componen las pantallas (ejemplo)

Fila de Seguimiento, solo con componentes:

```xml
<DataTemplate x:DataType="models:StudentRowItem">
  <Grid ColumnDefinitions="{StaticResource SeguimientoColumns}" ColumnSpacing="20" Padding="20,0" HeightRequest="64">
    <ui:StudentIdentityCell LastName="{Binding LastName}" FirstName="{Binding FirstName}" FileNumber="{Binding FileNumber}" />
    <ui:RiskPill Grid.Column="1" Level="{Binding RiskLevel}" Score="{Binding RiskScore}" />
    <ui:Meter Grid.Column="2" Value="{Binding AttendanceRatio}" ShowLabel="True" MaxBarWidth="110" />
    <ui:Meter Grid.Column="3" Value="{Binding SubmissionRatio}" ShowLabel="True" MaxBarWidth="110" />
    <Label Grid.Column="4" Text="{Binding LastContactText}" Style="{StaticResource Caption}" />
  </Grid>
</DataTemplate>
```

Pantalla → componentes:

| Pantalla | Componentes |
|---|---|
| Nómina | `PageShell`, `Card`, `CardToolbar`, `SearchBox`, `TableHeader`, `StudentIdentityCell`, `EmptyState` |
| Seguimiento | Los de Nómina + `KpiCard` ×3, `FilterChips`, `RiskPill`, `Meter` |
| Entregas | `PageShell`, `Card`, `CardToolbar`, `FormField`/selector de trabajo, `RegistrationProgress`, `StudentIdentityCell`, `SegmentedControl` |
| Asistencia | Igual que Entregas + `AppButton` (Ghost) "Marcar todos presentes" |
| Calendario | `PageShell`, `Card`, `FormField`, `AppButton`, `Tag` |
| Ajustes | `PageShell`, `Card` (padded), `FormField`, `AppButton`, `Tag` |
| Detalle | `PageShell`, `AppButton` (Link), `Card`, `ScoreBadge`, `MetricCard`, `KeyValueBlock`, `HistoryRow`, `Avatar`, `AppButton` |

---

## 9. Dependencias entre componentes

```
Base:        Icon, Card
Display:     StatusPill → RiskPill, Meter, Avatar, Tag
Compuestos:  StudentIdentityCell (Avatar) · KpiCard (Card) · MetricCard (Meter) · ScoreBadge (RiskPill)
             HistoryRow (StatusPill) · RegistrationProgress (Meter)
Entradas:    AppButton (Icon) · SearchBox (Icon) · FilterChips · SegmentedControl · FormField
Estructura:  CardToolbar · EmptyState · TableHeader
Raíz:        selector de Comisión → encabezado de página → todas las vistas
             sidebar (Icon) → ventana
```

Es orden de composición, no de carpetas.

---

## 10. Orden de construcción

El corte en historias está en `issues/`. No hay una galería de debug: cada historia se verifica abriendo la pantalla y comparándola con el mockup.

---

## 11. Reglas de calidad para cada componente

- [ ] Todas las propiedades configurables son `BindableProperty` con valor por defecto.
- [ ] Sin colores, tamaños de fuente ni radios hardcodeados: solo tokens.
- [ ] Compiled bindings (`x:DataType`) en su XAML.
- [ ] Estados hover, foco y deshabilitado definidos con `VisualStateManager`.
- [ ] No referencia a `Core`, `Data` ni a ningún ViewModel concreto.
- [ ] Se ve bien con textos largos (truncado con elipsis o ajuste de línea definido).
- [ ] Accesibilidad básica: `AutomationProperties.Name` en controles interactivos, orden de tabulación lógico y foco visible.
- [ ] Probado a 900px y a 1920px de ancho de ventana.

---

## 12. Errores a evitar

- Duplicar un selector de Comisión por pantalla en vez de usar el del `PageShell`.
- Definir estilos con colores azules o con el aqua anterior.
- Usar `Frame` o sombras.
- Resolver colores de riesgo con `if/else` en XAML code-behind en lugar de convertidores.
- Dejar que un componente de UI llame a repositorios o a `RiskRule`.
- Crear una columna o un componente específico para mostrar el texto de detalle del riesgo en cada fila: ese contenido pertenece al Detalle del alumno.
- Hacer el control segmentado con un `Picker` oculto: debe ser un control propio, con botones reales.
- Agregar "Tarde", un alta de intervenciones, un drawer de detalle o una página galería.
- Crear carpetas Foundation, Layout, Navigation, Inputs, Display, Tables o Gallery.
- Duplicar `RiskLevel` de Core con un enum de UI del mismo nombre.