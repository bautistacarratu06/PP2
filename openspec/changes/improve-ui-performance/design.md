# Design

## Context

See `proposal.md` for motivation and `specs/ui-performance/spec.md` for the behavior contract. Evidence with file/line references is in `PERFORMANCE_REPORT.md` (P1–P6, ranked action plan).

Current state that shapes the approach:

- `SidebarView.SetCollapsed` animates `shell.FlyoutWidth` (64↔232) via `Animation.Commit` — a non-compositor property, so every ~16 ms tick re-measures the flyout *and* the page content. `Paint()` runs before the animation (labels re-wrap mid-resize) and again on every `Shell.Navigated`.
- `Icon.Apply()` clears its children and re-parses each figure with `new PathGeometryConverter().ConvertFromInvariantString(...)` per rebuild; `GlyphColor` is a `BindableProperty` whose change triggers a full rebuild; `SidebarView.Paint()` sets `GlyphColor` on all icons on every navigation/toggle.
- `StudentsPage.xaml` binds `FilteredStudents` to `BindableLayout` on a `VerticalStackLayout` (no virtualization); `StudentsViewModel.LoadAsync` does `Clear()` + N×`Add`, firing N layout invalidations; rows have fixed `HeightRequest="64"`.
- `StudentsPage.OnAppearing` unconditionally executes `LoadCommand`; `StudentDetailViewModel.ApplyQueryAttributes` fire-and-forgets `LoadAsync`.
- `StudentDetailsService.GetAsync` opens 4 `DbContext`s (1 sequential + 3 via `Task.WhenAll`); each context is a fresh SQL Server connection setup. `StudentDetailPage`/VM are transient (route-registered), so first drill-in also pays JIT + XAML inflation.
- The `remediate-code-quality` change (in progress) splits `StudentDetailViewModel` and touches `StudentsViewModel`; the `add-ui-integration-acceptance-tests` change adds acceptance tests for the same ViewModels. This change must compose with both: behavior-preserving edits, no constructor signature changes beyond what those changes already introduce.

## Goals / Non-Goals

**Goals:**

- Sidebar animation without per-frame full-app re-layout; navigation/toggle no longer triggers redundant icon rebuilds.
- Students grid that scales with roster size (virtualization) and reloads in one batch.
- Instant back-navigation when data is fresh; reload only on real staleness.
- Detail open on one connection, without content flash.
- Each fix independently verifiable with the report's measurement guidance (Stopwatch logging, 20/200/500-student scales).

**Non-Goals:**

- Rewriting the data layer beyond the single-context detail method (queries are already projected and `AsNoTracking`).
- Converting sync code to async or restructuring ViewModels (the report confirms async is correct).
- App-wide theming/control redesign; only the named controls change.
- Pagination of the students grid (owned by `support-bulk-student-import`); virtualization is independent of it.
- New automated perf-test infrastructure; verification is manual/measured per the report.

## Decisions

### 1. Sidebar: hide-then-animate now, inner-content animation as the structural fix

Step 1 (same-day): in `SetCollapsed`, set `BrandText`/`CollapseLabel`/row labels `IsVisible=false` before the animation and restore at completion; add an early-exit to `Paint()` when `active`/`_collapsed` are unchanged. Step 2 (structural): keep the flyout at a fixed width (`FlyoutBehavior="Locked"`) and animate the sidebar's inner root `Grid` width (or `TranslationX` slide + clip), so each frame's measure/arrange is bounded to the small sidebar subtree instead of Shell + page.

*Alternative:* snap width + fade labels only (compositor-friendly, cheapest). Kept as fallback if the inner-content animation still stutters on Windows; it satisfies the spec's "no full re-layout per frame" either way.

### 2. Icon: static geometry cache + stroke-only color path

A `static ConcurrentDictionary<(string Name, double Size), Geometry>` populated on first render; `Apply()` reuses cached geometry and only rebuilds `Path` objects when name/size change. `OnChanged` for `GlyphColor` iterates existing paths setting `Stroke`. One static `PathGeometryConverter` instance. Geometry is immutable so the cache is safe to share.

*Alternative:* replace `Icon` with pre-built `FontImageSource` glyphs. Rejected: changes the visual rendering path app-wide for a problem a cache fully solves.

### 3. Students grid: virtualized `CollectionView` with fixed row height

Replace the `BindableLayout`/`VerticalStackLayout` with a `CollectionView` (`ItemsLayout` = vertical linear grid, `ItemSizingStrategy="MeasureFirstItem"`, fixed 64 px row template). The spreadsheet look (9 columns, checkbox, pills) moves into the item template. `StudentsViewModel.LoadAsync` builds the full row list and swaps it in one assignment (new `ObservableCollection` instance or list replacement) instead of `Clear()` + N×`Add`.

*Alternative:* keep `BindableLayout` and only batch updates. Rejected as the primary fix — it removes the N-event thrash but still realizes every row; it remains the fallback if `CollectionView` cannot reproduce the hover-row behavior, in which case the spec's batch-update requirement still holds.

### 4. Dirty-flag reload on `OnAppearing`

`StudentsViewModel` gains a `_dirty` flag: set by every mutation path (add student, bulk attendance/submission, deactivate, import confirm, contact save from detail) and by `SectionSelection.Changed`; `OnAppearing` executes `LoadCommand` only when dirty, then clears it. First appearance always loads (flag starts dirty).

*Alternative:* compare a data timestamp/hash against the DB. Rejected: more machinery than the problem needs; mutations are the only staleness sources in this app.

### 5. Single-context detail fetch

Add `GetStudentDetailAsync(studentId, sectionId)` to `IStudentDetailsRepository`/`EfStudentDetailsRepository`: one `CreateDbContextAsync`, then the header + 3 history queries on that context (kept as 3 queries rather than one mega-projection for readability; they share the connection so the handshake cost disappears). `StudentDetailsService.GetAsync` calls it and keeps its `Task.WhenAll` shape only if the repository exposes per-read methods — otherwise it delegates entirely. The 4-context pattern is removed.

*Alternative:* pre-warming the page/VM after grid load. Kept as an optional follow-up task (it touches `MauiProgram` and route lifetime); the single-context fix is the deterministic win.

### 6. Collection swap on detail load + error surfacing

`StudentDetailViewModel.LoadAsync` awaits the repository result before clearing/replacing the three history collections (one swap, no flash). The fire-and-forget `_ = LoadAsync(...)` calls get a try/catch that routes exceptions into the existing `ErrorMessage`/`HasError` state — matching the `add-ui-integration-acceptance-tests` change's expectation that error state is observable.

### 7. `SizeChanged` handling on detail page

Only react when the window crosses the 720 px threshold (track the previous state; no-op when the boolean branch is unchanged), removing repeated definition mutations during window resize.

## Risks / Trade-offs

- [`CollectionView` template may not perfectly reproduce `HoverRow` pointer-hover styling] → Port the hover behavior into the item template's `PointerGestureRecognizer`; if a behavior gap remains, fall back to batched `BindableLayout` updates (spec still satisfied on the batch requirement; virtualization requirement would then need revisiting — flag it early).
- [Dirty flag can miss an unforeseen mutation path] → Set the flag in the shared mutation helpers rather than each command; acceptance tests from the test-coverage change assert reload-after-mutation behavior.
- [Geometry cache grows unbounded] → Bounded in practice (icon set is small and fixed); key includes size so entries are few; no eviction needed at app scale.
- [Single-context fetch changes `IStudentDetailsRepository` consumers] → Only `StudentDetailsService` consumes it; the old per-read methods stay for compatibility with existing tests.
- [Changes overlap in-flight refactors of the same ViewModels] → Land after (or rebase onto) `remediate-code-quality`'s splits; all edits here are behavior-preserving so rebase conflicts are mechanical.

## Migration Plan

1. Land items 1–3 of the report's action plan (dirty flag, sidebar hide+early-exit, icon cache) — low-risk, same-day, independently verifiable.
2. Land the single-context detail method and collection swap.
3. Land the sidebar inner-content animation and the `CollectionView` migration last (largest, most visual).
4. Verify at 20/200/500 seeded students per the report's "How to verify".
5. Rollback: each item is an independent commit; no schema or contract changes to revert.

## Open Questions

None. The report's fix menu (e.g. inner-content animation vs snap+fade; `CollectionView` vs batched BindableLayout) is resolved inside the design with fallbacks, and none of the choices change the specs or task breakdown.
