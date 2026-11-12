# Spec Delta

## Purpose

Make the UI feel fast: the sidebar animates without dropping frames, icons render cheaply, the students grid scales with roster size, back-navigation is instant when nothing changed, and opening a student detail costs one connection instead of four.

## ADDED Requirements

### Requirement: Smooth sidebar collapse animation

Collapsing or expanding the sidebar SHALL NOT trigger a full application re-layout per animation frame. Label text SHALL be hidden before the width transition starts and restored after it ends. Sidebar repainting SHALL be skipped when the active route and collapsed state have not changed.

#### Scenario: Collapse animation stays responsive

- **WHEN** the sidebar is collapsed and expanded on a screen with a large roster
- **THEN** the animation completes without visible stutter and the page content does not reflow per frame

#### Scenario: Repeated navigation does not repaint unchanged rows

- **WHEN** navigation occurs between routes while the sidebar's active item and collapsed state are unchanged
- **THEN** the sidebar performs no repaint work (no icon rebuilds, no color changes)

### Requirement: Icon rendering does not re-parse geometry

The icon control SHALL parse each glyph's geometry once per (name, size) and reuse the cached geometry on subsequent renders. Changing the icon color SHALL update only the existing paths' stroke, not rebuild the icon.

#### Scenario: Color change is cheap

- **WHEN** an icon's color changes (e.g. sidebar active-state change)
- **THEN** no geometry parsing occurs and no new path objects are allocated

#### Scenario: Same icon rendered repeatedly

- **WHEN** an icon with the same name and size is rendered again after a first render
- **THEN** the geometry comes from cache rather than being parsed again

### Requirement: Virtualized students grid

The students list SHALL use a virtualized collection control so that the number of realized row visuals stays bounded by the viewport, not the roster size. Row updates on load SHALL be applied as a single batch rather than one notification per row.

#### Scenario: Large roster stays responsive

- **WHEN** a section with 500 students is loaded and scrolled
- **THEN** only the visible rows (plus a small buffer) are realized, and scroll remains smooth

#### Scenario: Reload applies rows in one batch

- **WHEN** the grid reloads after a filter change or section change
- **THEN** the row collection is replaced in one update rather than cleared and re-added item by item

### Requirement: Back-navigation skips redundant reloads

Returning to the students screen SHALL NOT re-query the database or rebuild the grid when the data is still fresh. A reload occurs only when a mutation happened (contact saved, student created/deactivated, import completed) or the selected section changed.

#### Scenario: Return from detail without changes

- **WHEN** the user drills into a student and returns without mutating anything
- **THEN** the grid shows the same rows immediately with no database query and no rebuild

#### Scenario: Return after a mutation

- **WHEN** the user returns after a mutation (e.g. contact saved or student deactivated)
- **THEN** the grid reloads and shows the updated data

### Requirement: Single-connection student detail load

Loading a student's detail SHALL fetch header, attendance, submissions, and interventions through a single database context/connection. Previously displayed history content SHALL remain visible until the new student's data arrives.

#### Scenario: Detail open uses one connection

- **WHEN** a student detail is opened
- **THEN** the four data reads execute on one database connection instead of four separate context/connection setups

#### Scenario: Switching students does not flash empty content

- **WHEN** the detail page loads a student while a previous student's data is displayed
- **THEN** the old content stays visible until the new data replaces it in one swap

### Requirement: Load errors are surfaced

Fire-and-forget page loads SHALL NOT silently swallow exceptions; load failures SHALL be reported through the screen's existing error state.

#### Scenario: Load failure shows feedback

- **WHEN** the detail or students load throws (e.g. database unreachable)
- **THEN** the screen's error message state is set instead of the exception disappearing

### Requirement: Performance scales with roster size

At seeded scales of 20, 200, and 500 students, grid load time and back-navigation time SHALL stay roughly flat as roster size grows (no linear degradation), and the sidebar animation SHALL remain smooth at every scale.

#### Scenario: Scale check

- **WHEN** the app is exercised at 20, 200, and 500 seeded students
- **THEN** load and navigation timings do not grow linearly with roster size, and the sidebar animation is smooth at all three scales
