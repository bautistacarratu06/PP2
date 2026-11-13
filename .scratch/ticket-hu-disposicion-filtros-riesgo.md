# [HU] Mejorar la disposición visual de los filtros de riesgo

- **ID:** HU-XX  
- **Módulo:** Seguimiento / Nómina (`StudentsPage`)  
- **Funcionalidad:** Filtrado por nivel de riesgo  
- **Prioridad:** Media  
- **Estado:** Completado (Verificado)  

---

### 1. Historia de usuario

**Como** usuario de la aplicación de Gestión Escolar,  
**quiero** que los filtros de riesgo se muestren de manera horizontal y ordenada, con cada nivel acompañado de su respectivo checkbox,  
**para** mejorar la legibilidad, facilitar su identificación y hacer más cómoda la interacción con los filtros.

---

### 2. Descripción del problema

Actualmente, los filtros de riesgo (Alto, Medio y Bajo) presentan una distribución visual poco eficiente, con una separación y disposición que dificultan su lectura e identificación.

Esto afecta la experiencia de usuario (UX), ya que los elementos no mantienen una organización visual clara y uniforme.

#### Causa técnica identificada:
En `StudentsPage.xaml` (líneas 213–220), los filtros de riesgo estaban dispuestos en un único `HorizontalStackLayout Spacing="12"` plano sin agrupación interna:
```xml
<HorizontalStackLayout Spacing="12">
    <CheckBox IsChecked="{Binding Filters.FilterRiskAlto}" />
    <Label Text="Alto" VerticalOptions="Center" />
    <CheckBox IsChecked="{Binding Filters.FilterRiskMedio}" />
    <Label Text="Medio" VerticalOptions="Center" />
    <CheckBox IsChecked="{Binding Filters.FilterRiskBajo}" />
    <Label Text="Bajo" VerticalOptions="Center" />
</HorizontalStackLayout>
```
Esto provocaba los siguientes problemas de UX:
1. **Orden invertido respecto a la convención requerida:** El checkbox precedía a la etiqueta (`[CHECK] Alto`), en lugar de disponerse `Alto [CHECK]`.
2. **Ambigüedad de espaciado:** Al haber 12px de separación uniforme entre cada uno de los 6 controles, la etiqueta `Alto` se encontraba a la misma distancia del checkbox de `Alto` que del checkbox de `Medio`, haciendo confuso identificar a qué checkbox correspondía cada nivel de riesgo.
3. **Falta de agrupación visual:** No existían contenedores individuales por nivel de riesgo que agruparan semántica y espacialmente cada etiqueta con su checkbox.

---

### 3. Comportamiento esperado

Se deberá modificar la disposición de los filtros para que se presenten horizontalmente, siguiendo el siguiente orden:

**ALTO [CHECK]　MEDIO [CHECK]　BAJO [CHECK]**

Se deberá contemplar:
- Cada nivel de riesgo deberá aparecer junto a su respectivo checkbox.
- Los tres filtros deberán estar alineados horizontalmente.
- Se deberá incorporar un leve espaciado entre cada grupo para diferenciarlos visualmente.
- Se deberá mantener una distribución uniforme y consistente.
- Se deberá conservar el funcionamiento actual de los filtros y la posibilidad de seleccionar uno o varios niveles simultáneamente.

---

### 4. Criterios de Aceptación

1. Los filtros deberán visualizarse en una única fila horizontal.
2. El orden deberá ser: **ALTO, MEDIO y BAJO**.
3. Cada etiqueta deberá ubicarse inmediatamente junto a su checkbox.
4. Deberá existir un pequeño espacio entre cada grupo (etiqueta + checkbox).
5. Los elementos deberán mantener una alineación vertical uniforme.
6. La selección de los checkboxes deberá conservar el comportamiento actual del filtrado.
7. La modificación no deberá afectar otros filtros ni componentes de la pantalla.

---

### 5. Plan de Implementación

- [x] **Paso 1: `StudentsFiltersViewModel.cs`**
  - Añadir comando `ToggleRiskFilterCommand` para permitir conmutar el estado de un nivel de riesgo tanto desde el checkbox como al hacer clic/tap sobre la etiqueta correspondiente, maximizando la comodidad de interacción sin alterar la selección múltiple.
- [x] **Paso 2: `StudentsPage.xaml`**
  - Reestructurar el panel de filtro de riesgo (`OpenFilterColumn == "Risk"`):
    - Reemplazar el layout plano por un contenedor horizontal principal con espaciado definido entre grupos (`Spacing="20"`).
    - Agrupar cada nivel en su propio `HorizontalStackLayout Spacing="6"` con alineación vertical centrada.
    - Ordenar cada grupo con la etiqueta inmediatamente a la izquierda del checkbox:
      1. `Alto` + `CheckBox (FilterRiskAlto)`
      2. `Medio` + `CheckBox (FilterRiskMedio)`
      3. `Bajo` + `CheckBox (FilterRiskBajo)`
    - Asociar `TapGestureRecognizer` a cada etiqueta enlazado a `ToggleRiskFilterCommand`.
- [x] **Paso 3: Verificación**
  - Compilación de la solución con `dotnet build`: 0 errores.
  - Ejecución de suite de pruebas: 142 pruebas en `GestionEscolar.Tests` + 25 pruebas en `GestionEscolar.Data.Tests` (167 aprobadas en total, 0 fallidas).
  - Verificación de codificación UTF-8 con BOM en archivos `.cs`.

