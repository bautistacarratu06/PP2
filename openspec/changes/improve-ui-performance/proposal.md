# Proposal

## Why

Two user-reported symptoms — a sidebar collapse/expand animation that stutters and a student-detail drill-in that doesn't feel instant — trace to UI-layer costs, not the data layer: the sidebar animates `Shell.FlyoutWidth` (full app re-layout every frame), the `Icon` control re-parses SVG geometry on every property change, the students grid uses non-virtualized `BindableLayout` and rebuilds every row on every load, returning from detail unconditionally reloads the whole grid, and each detail open pays 4 separate DB context/connection setups. Findings and evidence are in `PERFORMANCE_REPORT.md` (P1–P6).

## What Changes

- **Sidebar animation (P1):** hide labels before animating and restore after; early-exit `Paint()` when the active route and collapsed state are unchanged; stop animating `Shell.FlyoutWidth` — animate the sidebar's inner content (slide/clip or inner-grid width inside a fixed flyout) or snap width and fade labels.
- **Icon control (P2):** cache parsed `Geometry` per (name, size) in a static dictionary; make `GlyphColor` changes update only the `Stroke` of existing paths instead of rebuilding; reuse a single static `PathGeometryConverter`.
- **Students grid (P3):** replace the `BindableLayout` on `VerticalStackLayout` with a virtualized `CollectionView` (fixed row height, `ItemSizingStrategy="MeasureFirstItem"`), or at minimum batch row updates (swap the collection reference) instead of `Clear()` + N×`Add()`.
- **Stale-data reload (P4):** `StudentsPage.OnAppearing` skips the reload when data is fresh; a dirty flag set by mutations (contact saved, student created/deactivated, import done) and by `SectionSelection.Changed` triggers reload only when needed.
- **Student detail load (P5):** add a single-context repository method fetching header + attendance + submissions + interventions on one connection; swap the three history collections only after new data arrives (no content flash); optionally pre-warm the detail page after the grid loads.
- **Minor contributors (P6):** debounce/limit the detail page's `SizeChanged` definition mutation to the 720 px threshold crossing; surface load exceptions instead of swallowing them in fire-and-forget loads.
- No async/sync conversions: the report confirms the async hot paths are correct.

## Capabilities

### New Capabilities

- `ui-performance`: Perceived UI performance — smooth sidebar animation, cheap icon rendering, virtualized students grid, instant back-navigation, and single-connection detail loading.

### Modified Capabilities

None. No existing capability's requirements change; this change alters performance characteristics and internal rendering/reload behavior, not user-visible feature behavior.

## Impact

- `UI/Controls/SidebarView.xaml.cs` (animation, `Paint()` early-exit), `UI/Controls/Icon.cs` (geometry cache, stroke-only color updates), `UI/Views/StudentsPage.xaml` (+ code-behind), `UI/Views/StudentsPage.xaml.cs` (conditional reload), `UI/Views/StudentDetailPage.xaml.cs` (size-change handling), `UI/ViewModels/StudentsViewModel.cs` (dirty flag, batched row updates), `UI/ViewModels/StudentDetailViewModel.cs` (collection swap after load, exception surfacing).
- `GestionEscolar.Data`: new `GetStudentDetailAsync` on the student-details repository (single context) replacing the 4-context pattern in `StudentDetailsService.GetAsync`; `IStudentDetailsRepository` gains the method.
- `UI/MauiProgram.cs` only if the optional page pre-warm is added.
- No database schema changes; no API contract changes; existing tests keep passing (ViewModel public behavior unchanged).
- Verification uses the seeder at 20/200/500 students and the measurement guidance in the report.
