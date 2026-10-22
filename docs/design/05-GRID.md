# Design 05: The data grid (`BobDataTable`, `BobDataGrid<TItem>`)

Status: draft. Bob's decision: everything possible goes into V1 (doc 00 s6). The grid is the largest single component, so it is
built in ten slices that each ship and are tested on their own. A stalled slice never blocks a release.

Depends on: `BobCollection<T>` and `BobVirtualizer` (lane C), the event layer (doc 01), fields (doc 02, from S6).
Lane: F, wave 2 (doc 04).

## 1. Shape

Two components, one core:

| Component | Purpose |
| --- | --- |
| `BobDataTable<TItem>` | Static table: columns, templates, client-side sort. Slice S1, the stepping stone and the right choice for small lists |
| `BobDataGrid<TItem>` | The full grid. Same column model, plus every feature below. Built on the table's column model, not a rewrite |

```razor
<BobDataGrid TItem="Order" Source="@_source" Key="@(o => o.Id)" Selection="BobSelectionMode.Multiple">
    <BobGridColumn Field="@(o => o.Customer)" Title="Customer" Sortable Filterable />
    <BobGridColumn Field="@(o => o.Total)" Title="Total" Format="C2" Align="End" Aggregate="BobAggregate.Sum" />
    <BobGridColumn Title="">
        <CellTemplate Context="o"><BobButton OnClick="() => Open(o)">Open</BobButton></CellTemplate>
    </BobGridColumn>
</BobDataGrid>
```

Columns are declared as child components (the Blazor idiom) or as a `Columns` list for dynamic cases. Field accessors are
expressions so the grid can derive sort/filter keys and the typed filter editor without reflection on hot paths.

## 2. Data source abstraction (S2)

One interface serves client and server data, so switching is a one-line change:

```csharp
public interface IBobGridSource<TItem>
{
    ValueTask<BobGridPage<TItem>> LoadAsync(BobGridQuery query, CancellationToken ct);
}
// BobGridQuery: Skip, Take, Sort[] (field, direction), Filter (typed tree), Group[], Search
// BobGridPage<TItem>: Items, TotalCount, Aggregates?
```

- `BobGridSource.FromList(IReadOnlyList<T>)`: in-memory, sorts/filters/pages locally.
- `BobGridSource.FromQueryable(IQueryable<T>)`: translates the query to LINQ (EF-friendly), no reflection on the render path.
- `BobGridSource.FromDelegate(...)`: server paging for any API.
- Requests are cancelled when superseded; the latest query wins. A failed load becomes a `BOB-GRID-001` in the grid's error
  slot, with a retry action, never an exception in the render tree.

## 3. Slices

| Slice | Scope | Exit tests |
| --- | --- | --- |
| S1 | `BobDataTable`, `BobGridColumn`, header/cell/footer templates, empty and loading states, ARIA `table` semantics, density | column model, template render, a11y notes, render count |
| S2 | `IBobGridSource<T>`, the three sources, sort (multi-column), paging (`BobPager` reuse), query cancellation | source contract tests, sort stability, paging edge cases |
| S3 | Typed per-column filters (text/number/date/bool/enum), filter row and menu, global search | filter tree to LINQ, culture-aware parse via doc 02 converters |
| S4 | Selection (single/multi/checkbox column, range with Shift), keyboard navigation (full ARIA `grid` pattern), row activate | key map tests, `bob.grid.*` behavior overrides |
| S5 | Virtualization (rows, then columns) on `BobVirtualizer`; fixed and measured row heights; sticky header and frozen columns | render-count and DOM-size tests (rows in DOM <= viewport + overscan) |
| S6 | Inline edit (cell, row, form modes) with the field components; validation per cell; commit/cancel behaviors | edit lifecycle, validation errors shown with doc 02 text provider |
| S7 | Grouping (multi-level, collapse/expand), aggregates (sum/avg/min/max/count/custom), group and footer templates | group tree, aggregate correctness, virtualized groups |
| S8 | Column resize, reorder (drag), hide/show chooser, pinning; pointer input via the event layer | pointer-event tests, persisted layout round trip |
| S9 | Export: CSV first (respecting filters and visible columns, streaming), then Excel-compatible XLSX if demand; print stylesheet | export content tests, large-set streaming |
| S10 | State persistence (sort, filter, page, columns, groups) to a pluggable store (`localStorage` default), named views | save/restore round trip, version-mismatch fallback |

Each slice also lands: its CSS contract fragment, its a11y doc bullets (each naming a bUnit test), its docs demo, and its
changelog line. Slices S1 to S3 do not need fields and can start as soon as lane C has merged.

## 4. Events and behaviors (doc 01)

Every default the grid performs from user input is a named, cancellable behavior:

| Name | Default |
| --- | --- |
| `bob.grid.row-activate` | double-click / Enter on a row raises `OnRowActivate` |
| `bob.grid.select-row` | click / Space selects; Shift range, Ctrl toggle |
| `bob.grid.sort-toggle` | header click cycles asc / desc / none |
| `bob.grid.cell-edit-start` | Enter / F2 / double-click starts editing |
| `bob.grid.cell-edit-commit` | Enter or blur commits; Escape cancels |
| `bob.grid.navigate` | arrow, Home/End, PageUp/PageDown, Ctrl+Home/End move the active cell |
| `bob.grid.column-resize` | pointer drag on the header edge |
| `bob.grid.column-reorder` | pointer drag on a header |
| `bob.grid.copy` | Ctrl+C copies the selection as TSV |

Apps cancel, replace or disable these by name exactly as for modals. The grid raises one delegated listener on its root, never
one per cell.

## 5. Errors (`BOB-GRID-*`, added to `error-catalog.json`)

| Code | Origin | When | Facts |
| --- | --- | --- | --- |
| `BOB-GRID-001` | FailedNetworkCall | The data source threw or timed out | `source`, `query`, inner exception |
| `BOB-GRID-002` | WrongParameters | Column has no `Field`, `Template` or `Title`; duplicate column id | `column`, `reason` |
| `BOB-GRID-003` | WrongParameters | `Key` missing while selection, edit or virtualization needs stable keys | `feature` |
| `BOB-GRID-004` | DataShapeMismatch | A sort/filter expression cannot be translated by the source | `field`, `operator`, `source` |
| `BOB-GRID-005` | DataShapeMismatch | Persisted state does not match the current columns (falls back to defaults) | `version`, `missingColumns` |
| `BOB-GRID-006` | YourCallback | An edit commit or row handler threw | `row`, `column`, `phase` |

All carry structured facts so they translate (doc 02 s7). The grid shows errors in an inline banner slot, not by throwing.

## 6. Accessibility

- Table semantics (`role=table`) when read-only and not interactive; the ARIA `grid` pattern (`role=grid`, roving
  `tabindex`, `aria-rowindex`/`aria-colindex`, `aria-sort`, `aria-selected`, `aria-busy`) when navigation or edit is on.
- With virtualization, `aria-rowcount` and `aria-rowindex` carry the real positions, since most rows are not in the DOM.
- Live region announces sort, filter result count, group expand/collapse and load errors.
- Every bullet in `Content/accessibility/grid.md` names its bUnit test or is marked `gap:`.

## 7. CSS (flexbox only)

The library rejects `display: grid` in CSS, so rows are flex containers and column widths come from a shared per-column
`flex: 0 0 var(--bob-grid-col-w, ...)` set as an inline custom property per column; header and body rows use the same value so
they align. Frozen columns use `position: sticky`.

- Block: `bob-grid`; parts via `data-bob-part`: `root`, `toolbar`, `header`, `header-cell`, `filter-row`, `body`, `row`, `cell`,
  `group-row`, `footer`, `pager`, `empty`, `loading`, `error`, `resize-handle`.
- Variables (read with fallbacks, documented in CSS-CONTRACT.md): `--bob-grid-row-h`, `--bob-grid-header-bg`,
  `--bob-grid-row-bg`, `--bob-grid-row-bg-alt`, `--bob-grid-row-bg-selected`, `--bob-grid-border`, `--bob-grid-cell-pad`.

## 8. Efficiency

- Rows are rendered through `BobVirtualizer` from S5; before that, paging bounds the DOM.
- Row and cell components override `ShouldRender`: they re-render only when their item reference, selection flag, edit flag or
  column layout changed. A render-count test scrolls and edits and asserts untouched rows do not re-render.
- Sorting/filtering/grouping run in the source, not in the render path; client sources use a stable key index to avoid
  reallocating rows.
- Column resize writes a CSS custom property on the root (no re-render per pixel).
- Export streams; it never materializes the whole set when the source can page.

## 9. Migration tables

Each docs page gets a table mapping Telerik `TelerikGrid`, MudBlazor `MudDataGrid` and Blazorise `DataGrid` parameters and events
to `BobDataGrid` (for example `Data` / `Items` / `Data` -> `Source`, `OnRead` / `ServerData` / `ReadData` -> an
`IBobGridSource<T>`, `SelectionMode` -> `Selection`). Written by lane E from the real parameter list, once each slice lands.

## 10. Non-goals for V1

- PivotGrid and TreeList (built on the grid core later if there is demand; doc 03 Tier 5).
- Master/detail with nested grids beyond one level (detail row template is in; nested grid state persistence is not).
- Real-time collaborative editing and server-side cell locking.
- Blazor Server tuning (doc 00 D8): the data source model already keeps the grid cheap there, but it is not tested or documented now.

## 11. Release mapping

- 0.4: S1 to S3 (table, sources, sort, page, typed filters)
- 0.5: S4 to S6 (selection, keyboard, virtualization, inline edit)
- 0.6: S7 to S10 (grouping, aggregates, column management, export, persistence)

If a slice slips it moves to the next release on its own; nothing else waits for it.
