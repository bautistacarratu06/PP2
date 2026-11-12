# Conventions

These conventions come from the existing project docs and code. Follow the surrounding file when something is unclear.

## Language and encoding

- UI text is in Spanish (for example "Comisión", "Registrar asistencia").
- Code identifiers are in English; comments are mostly in Spanish.
- `.cs` files are UTF-8 with BOM. Keep each file's existing line endings.
- Some existing `.cs` files still lack a BOM. Do not mass-convert them as a drive-by change. New `.cs` files must include the BOM. The allow-list and the check live in `GestionEscolar.Architecture.Tests` (see [`harness.md`](./harness.md)).

## Domain / data

- Use enums, not magic strings. Database status values (`presente`/`ausente`, `entregó`/`no entregó`) are mapped only in `DbStatusValues` (`ToDbValue` / `Parse*`).
- Keep formatting and display (dates, "Apellido, Nombre" labels) in UI or Data projections, not in Core models.
- Data access goes through Entity Framework in `GestionEscolar.Data`:
  - async all the way (`async`/`await` with `CancellationToken`);
  - one `CourseTrackingDbContext` per operation (`IDbContextFactory`), closed when the operation ends;
  - LINQ only. Do not concatenate SQL.
- ViewModels use `[ObservableProperty]` partial properties and `[RelayCommand]`. Section-scoped screens derive from `SectionScopedViewModel`.
- The chosen commission is the `UI.Services.SectionSelection` singleton. Screens subscribe to it; they do not hardcode a section id.

## UI look

For every new screen or control:

- One dark theme. Colors come from `UI/Resources/Styles/Colors.xaml` (`Bg`, `Side`, `Surface`, `Raised`, `Line`, `TextPrimary`, `TextSecondary`, `TextTertiary`, `Accent`, `Danger`, `Warning`, `Success`). Do not hardcode hex in new pages, and do not bring back navy or teal.
- Type: Inter (`Inter`, `InterMedium`, `InterSemibold`, `InterBold`) and JetBrains Mono (`JetBrainsMono`) for student numbers, phones, and dates.
- Reuse `UI/Controls` (`PageShell`, `SidebarView`, `SectionPicker`, `SearchBox`, `Avatar`, `Icon`, `Meter`, `RiskPill`, `StatusPill`, `SegmentedControl`, `KpiCard`, `FilterChips`, `HoverRow`) and the named styles in `UI/Resources/Styles/Styles.xaml` (`Card`, `FieldBorder`, `PrimaryButton`, `GhostButton`, `LinkButton`, `PageTitle`, `PageSubtitle`, `ColumnHeader`, `Mono`). New chrome goes through `PageShell`, not a private title.
- Cards are a `Border` with a 14px radius and a 1px stroke. No `Frame`, no drop shadow.
- List names are "Apellido, Nombre" via `UI/Formatting/PersonName`. The detail subtitle is "Nombre Apellido · legajo".
- Visual reference: `.scratch/look-and-feel/` (`Gestión Escolar · Mockup.html`, `ui-revamp.md`, `component-guide.md`, and `issues/`).
