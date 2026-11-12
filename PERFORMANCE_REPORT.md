# Performance Report — GestionEscolar UI

**Date:** 2026-11-11
**Symptoms reported:**
1. Sidebar collapse/expand animation doesn't feel like 60 fps.
2. Drilling into a student's detail page doesn't feel instant (noticeable delay before content appears).

**Verdict up front:** the queries are mostly fine (they're projected, `AsNoTracking`, and the detail service already parallelizes 3 of its 4 reads). The perceived slowness comes from **UI-layer costs**: layout-thrashing animations, geometry re-parsing on the UI thread, no list virtualization, and full page/grid rebuilds on navigation.

### What it is NOT: an async vs sync problem

The async code in the hot paths is written correctly:

- `StudentDetailsService.GetAsync` uses `Task.WhenAll` to parallelize 3 of its 4 reads; all EF queries are awaited with `ConfigureAwait(false)`. Nothing blocks the UI thread on DB calls.
- `StudentsViewModel.LoadAsync` awaits properly. The symptom is *waiting*, not a *frozen UI* — the time goes into real work (connection setups, page construction, layout), not into a blocked thread.

The exceptions worth noting:

- `Icon.Apply()` does synchronous geometry-string parsing and `Path` allocation **on the UI thread** (P2) — real UI-thread work competing with animations, fixable with caching.
- `async void` / fire-and-forget loads (`StudentDetailViewModel.ApplyQueryAttributes`, `StudentsPage.OnAppearing`) are not perf bugs, but they swallow load exceptions (P6).

So the fix levers are *reducing work per frame*, *reducing round trips*, and *avoiding redundant rebuilds* — not converting sync to async.

---

## Findings (ranked by user-perceived impact)

### 🔴 P1 — Sidebar animation animates `Shell.FlyoutWidth` → full app re-layout every frame

**Evidence:** `UI/Controls/SidebarView.xaml.cs:130-148`

```csharp
var target = collapsed ? 64 : 232;
var animation = new Animation(value => shell.FlyoutWidth = value, from, target);
animation.Commit(this, "FlyoutWidth", 16, 200, Easing.CubicOut);
```

**Why it's not 60 fps:** `FlyoutWidth` is not a compositor property. Every tick (~every 16 ms) forces Shell to re-measure and re-arrange **the flyout content AND the page content** (the whole window reflows because the locked flyout resizes the page area). That's a full layout pass per frame — on a page as heavy as StudentsPage (see P3), a layout pass costs far more than 16 ms, so frames are dropped and the animation stutters. Additionally:

- `Paint()` runs **before** the animation and toggles `label.IsVisible` / `HorizontalOptions` immediately, so labels re-wrap during the resize instead of being hidden first.
- `shell.Navigated` also triggers `Paint()` (line 31), which sets `icon.GlyphColor` on every row — and each `GlyphColor` change rebuilds the whole icon (see P2), adding work right when a navigation animation is running.

**Fixes (pick in order of effort):**
1. **Cheap (minutes):** hide text first, then animate. In `SetCollapsed`, set `BrandText/CollapseLabel/labels IsVisible=false` *before* starting the animation, and only restore at the end. Removes text re-wrap from every frame.
2. **Better (hours):** stop animating `FlyoutWidth`. Keep the flyout at a fixed width and animate the *inner content* of `SidebarView` (e.g. a `TranslationX` slide + clip, or animate the width of the sidebar's root `Grid` inside a fixed-width flyout with `FlyoutBehavior="Locked"`). Inner-layout animations still cost a measure/arrange, but only of the small sidebar subtree, not the whole Shell + page.
3. **Alternative:** drop the width animation entirely — snap to 64/232 and animate only opacity of the labels (compositor-friendly, always smooth).

---

### 🔴 P2 — `Icon` control re-parses SVG path geometry on every property change

**Evidence:** `UI/Controls/Icon.cs:44-77`

- Every `Apply()` call does `_glyphs.Children.Clear()` then, **per figure**, runs `new PathGeometryConverter().ConvertFromInvariantString(figure)` — XAML-geometry string parsing on the UI thread — and allocates fresh `Path` objects.
- `GlyphColor` is a `BindableProperty` with `propertyChanged: OnChanged` → **changing the color rebuilds everything**. `SidebarView.Paint()` sets `GlyphColor` on all 3 icons on *every* navigation and every collapse toggle.
- A converter is also allocated per figure per rebuild (`new PathGeometryConverter()` inside the loop).

**Impact:** small per call, but it lands exactly on the hot paths (navigation + sidebar toggle), adding allocations and UI-thread work during animations → contributes to the dropped frames in P1.

**Fixes:**
1. Cache parsed `Geometry` per `(name, size)` in a static `Dictionary` — geometry is immutable; only `Stroke` needs to change with color.
2. Split `Name/Size` (rebuild) from `GlyphColor` (just update `Stroke` on existing paths). Color changes become O(icons), not O(paths × parse).
3. Reuse a single static `PathGeometryConverter` instance.

---

### 🔴 P3 — Students grid uses `BindableLayout` on a `VerticalStackLayout` → no virtualization

**Evidence:** `UI/Views/StudentsPage.xaml:317`

```xml
<VerticalStackLayout BindableLayout.ItemsSource="{Binding FilteredStudents}">
```

`BindableLayout` realizes **every row immediately** — there is no virtualization. Each row is a `HoverRow` Grid with 9 columns, CheckBox, icons, pills, etc. With a real roster (hundreds of students), this means:

- Hundreds of complex visual trees created on every load.
- `LoadAsync` does `FilteredStudents.Clear()` then `Add(row)` in a loop (`StudentsViewModel.cs:287-299`) → N separate `CollectionChanged` events, each triggering layout invalidation, all on the UI thread.
- Every section change, filter change, and **every return from the detail page** repeats this full rebuild.

**Impact:** this is the main reason the app feels heavy overall, and it amplifies P1 (sidebar animation reflows this giant tree every frame) and P4 (returning from detail re-realizes everything).

**Fixes:**
1. Replace with a virtualized `CollectionView` (grid-style `ItemsLayout`) — the single highest-impact change for large rosters.
2. If keeping BindableLayout for the spreadsheet look: batch updates (build a new `List<StudentRow>` and swap the collection reference, or use an `ObservableCollection` replacement pattern) instead of Clear + N×Add.
3. Row height is already fixed (`HeightRequest="64"`) — a `CollectionView` with `ItemSizingStrategy="MeasureFirstItem"` will virtualize cheaply.

---

### 🟠 P4 — Returning from Student Detail reloads the entire students grid

**Evidence:**
- `UI/Views/StudentsPage.xaml.cs:19-24` — `OnAppearing` → `await _viewModel.LoadCommand.ExecuteAsync(null)` unconditionally.
- `StudentsViewModel.LoadAsync` re-queries, clears, and re-adds every row (see P3).

**Impact:** every "back" from detail pays: 1 query + full grid rebuild + full re-layout. This is a big part of why navigation feels sluggish in both directions.

**Fix:** only reload when data is stale (section changed, or a mutation happened — contact saved, student created/deactivated, import done). E.g. a `_dirty` flag set by mutations/`SectionSelection.Changed`, and `OnAppearing` skips the reload when clean. Instant back-navigation, correct data.

---

### 🟠 P5 — Student detail page: heavy transient page + 4 DB contexts per open

**Evidence:**
- `MauiProgram.cs:93,101` — `StudentDetailPage` and its VM are **transient**; route registered in `AppShell.xaml.cs:10`. Every drill-in constructs the page, inflates `StudentDetailPage.xaml` (a large XAML with 3 history lists, meters, contact modal), and creates a new VM.
- `StudentDetailViewModel.ApplyQueryAttributes` (line 163) fires `_ = LoadAsync(...)` — good: row data paints first. But then:
- `StudentDetailsService.GetAsync` (`Core/Services/StudentDetailsService.cs:18-35`) opens **4 separate DbContexts** (1 sequential + 3 parallel). Each `CreateDbContextAsync` = new connection setup against SQL Server (auth/handshake cost on first use per context).

**Impact:** the "some sec/millis that doesn't feel instant" = page construction + XAML inflation + 4 connection setups + query time, all before `IsBusy` even shows meaningful content beyond the header.

**Fixes:**
1. **Single context / single round trip:** add a repository method `GetStudentDetailAsync(studentId, sectionId)` that fetches header + attendance + submissions + interventions with **one** context (3 queries on one connection, or one query with `Include`-style projections). Removes 3 connection setups.
2. **Pre-warm the page:** since the route is transient, the first drill-in also pays JIT + handler creation. Optionally pre-create the page/VM on the background thread after the students grid loads, or make the detail page a singleton that re-binds per student (careful with state reset).
3. `LoadAsync` clears the 3 collections *then* awaits — if a previous student was shown, the user sees content flash away. Consider swapping collections only after the new data arrives.

---

### 🟡 P6 — Minor contributors

| Item | Evidence | Note |
|---|---|---|
| `Paint()` on every `Shell.Navigated` | `SidebarView.xaml.cs:31` | Recolors all rows + rebuilds icons (P2) even when the active route didn't change. Early-exit if `active` and `_collapsed` are unchanged. |
| `SizeChanged → Arrange()` on detail page | `StudentDetailPage.xaml.cs:15` | Mutates `ColumnDefinitions`/`RowDefinitions` on every size change (e.g. during window resize) → repeated re-layout. Debounce or only react to crossing the 720 px threshold. |
| `async void` / fire-and-forget loads | `StudentDetailViewModel.cs:163`, `StudentsViewModel.cs:37` | Not a perf bug per se, but exceptions during load are swallowed; combined with `IsBusy` gating, a slow first load shows skeleton + no feedback. |
| Skeleton pulse animation | `SkeletonGrid.cs:93-98` | 1200 ms opacity loop is fine, but it runs on a `VerticalStackLayout` with many children; keep skeleton rows ≤ 5 (it already defaults to 5). |
| `HoverRow` sets `BackgroundColor` per pointer event | `HoverRow.cs` | Fine, but with P3's non-virtualized grid there are N such gesture recognizers; virtualization fixes this too. |

---

## Recommended action plan

| # | Action | Addresses | Effort | Expected win |
|---|---|---|---|---|
| 1 | Skip grid reload on `OnAppearing` when data is fresh (dirty flag) | P4 | ~1 h | Back-navigation feels instant |
| 2 | Sidebar: hide labels before animating; early-exit `Paint()` | P1.1, P6 | ~1 h | Fewer dropped frames during collapse |
| 3 | `Icon`: cache parsed geometry; color change only updates `Stroke` | P2 | ~2 h | Cheaper navigation + toggle |
| 4 | Detail: single-context repository method (1 connection, 3 queries) | P5 | ~2–4 h | Detail load drops by connection overhead + simpler pipeline |
| 5 | Sidebar: animate inner content instead of `FlyoutWidth` (or snap + fade) | P1.2 | ~0.5–1 d | Smooth, truly 60 fps sidebar |
| 6 | Replace `BindableLayout` grid with virtualized `CollectionView` (or batch row updates) | P3 | ~1–2 d | Scales with roster size; fixes scroll + animation reflow |

Items 1–3 are low-risk, same-day wins. Item 6 is the structural fix if rosters grow beyond ~50 students.

## How to verify

- Measure with MAUI's **GraphicsView/Profiler** or simply `Stopwatch` around `LoadAsync` and `ApplyQueryAttributes` → log to Debug.
- On Windows, enable **UI Response Time** traces (Visual Studio Diagnostic Tools → UI Responsiveness) to confirm layout passes per frame during the sidebar animation (before/after item 5).
- Test with the seeder at 3 scales: 20, 200, 500 students (`Seeding/StudentSeeder.cs` + `sample.csv`) — P3/P4 symptoms should scale linearly before the fixes and stay flat after.
