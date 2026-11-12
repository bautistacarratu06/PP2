# Tasks

## 1. Back-navigation reload skip

- [ ] 1.1 Add a dirty flag to `StudentsViewModel` set by every mutation path (add student, bulk attendance/submission, deactivate, import confirm, contact save) and by `SectionSelection.Changed`; `OnAppearing` executes `LoadCommand` only when dirty and clears it after loading; first appearance always loads. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 1.2 Verify manually: drill into a student and return — no reload occurs (no busy flash, no query); mutate from detail (save contact) and return — the grid reloads with updated data. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj` still passes.

## 2. Sidebar animation and repaint

- [ ] 2.1 In `SidebarView.SetCollapsed`, hide `BrandText`/`CollapseLabel`/row labels before starting the width animation and restore them at completion; add an early-exit to `Paint()` when the active route and collapsed state are unchanged. Verify the build succeeds.
- [ ] 2.2 Stop animating `Shell.FlyoutWidth`: keep the flyout fixed-width (`FlyoutBehavior="Locked"`) and animate the sidebar's inner root grid width or `TranslationX` with clip; if smoothness is still insufficient on Windows, fall back to snap width + label fade. Verify manually that collapse/expand on the students screen (200+ seeded students) shows no per-frame page reflow and no visible stutter.

## 3. Icon geometry cache

- [ ] 3.1 In `Icon.cs`, add a static `(Name, Size) → Geometry` cache, reuse a single static `PathGeometryConverter`, and make `GlyphColor` changes update only the existing paths' `Stroke` without rebuilding children. Verify `dotnet build UI/UI.csproj -f net10.0-maccatalyst` succeeds.
- [ ] 3.2 Verify manually: navigate between routes and toggle the sidebar repeatedly — icons render correctly (correct glyph, correct active/inactive colors) with no geometry re-parsing (confirm via breakpoint/log on the parse path firing once per name+size).

## 4. Single-connection detail load

- [ ] 4.1 Add `GetStudentDetailAsync(studentId, sectionId)` to `IStudentDetailsRepository` and implement it in `EfStudentDetailsRepository` with one `CreateDbContextAsync` fetching header + attendance + submissions + interventions; keep the existing per-read methods. Verify `dotnet build GestionEscolar.Data/GestionEscolar.Data.csproj` succeeds and `dotnet test GestionEscolar.Data.Tests/GestionEscolar.Data.Tests.csproj` passes.
- [ ] 4.2 Switch `StudentDetailsService.GetAsync` to the single-context method; keep the returned `StudentDetail` shape identical. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj --filter StudentDetailsServiceTests` passes.
- [ ] 4.3 In `StudentDetailViewModel.LoadAsync`, await the result before replacing the three history collections (single swap, no content flash), and wrap the fire-and-forget load in `ApplyQueryAttributes` so exceptions set the existing error state. Verify the build succeeds and a forced failure (e.g. bad connection) shows the error state instead of a silent failure.

## 5. Students grid virtualization

- [ ] 5.1 Replace the `BindableLayout` on `VerticalStackLayout` in `StudentsPage.xaml` with a virtualized `CollectionView` (fixed 64 px row template, `ItemSizingStrategy="MeasureFirstItem"`), moving the 9-column `HoverRow` layout into the item template with its pointer-hover behavior. Verify the build succeeds and the grid's visual layout (columns, checkbox, pills, selection) is unchanged.
- [ ] 5.2 Change `StudentsViewModel.LoadAsync` to build the full row list and swap the collection in one update instead of `Clear()` + N×`Add`. Verify `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj` passes and the grid updates in one refresh on filter/section change.
- [ ] 5.3 Verify selection and bulk actions still work through the new item template: select rows, toggle `IsAllSelected`, run a bulk attendance mark — exactly the selected rows are affected. Verify the acceptance tests from `add-ui-integration-acceptance-tests` (students list) pass if landed, otherwise verify manually.

## 6. Minor contributors

- [ ] 6.1 In `StudentDetailPage.xaml.cs`, react to `SizeChanged` only when crossing the 720 px threshold (track previous state; no-op otherwise). Verify the build succeeds and resizing the window does not repeatedly mutate column/row definitions.
- [ ] 6.2 Wrap the remaining fire-and-forget loads (`StudentsPage.OnAppearing` path) so exceptions set the screen's error state instead of being swallowed. Verify the build succeeds and a forced failure shows the error message.

## 7. Integration and scale verification

- [ ] 7.1 Run `dotnet test GestionEscolar.Tests/GestionEscolar.Tests.csproj`, `dotnet test GestionEscolar.Data.Tests/GestionEscolar.Data.Tests.csproj`, and `dotnet build UI/UI.csproj -f net10.0-maccatalyst`; verify all succeed.
- [ ] 7.2 Seed and exercise the app at 20, 200, and 500 students (`Seeding/StudentSeeder.cs` + `sample.csv`): measure `LoadAsync` and detail-open time with Stopwatch logging; verify timings stay roughly flat across scales, back-navigation without mutations issues no query, and the sidebar animation is smooth at all three scales.
