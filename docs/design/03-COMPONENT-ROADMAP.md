# Design 03: Component roadmap

Status: draft. Ordered by what unblocks the most apps first, and by dependency. "Have" means shipped in 0.2.0.
The comparison is against the common set across Telerik UI for Blazor, MudBlazor and Blazorise.

Naming rule: own names (doc 00 D7), with a migration table in each docs page. Every component follows the existing
checklist (CSS contract, parts, `[Description]`, bUnit tests, accessibility doc, generated docs).

## Tier 0: Spine (blocks everything)
| Item | Notes |
| --- | --- |
| Event layer + behavior registry | Doc 01 |
| `BobFieldBase<T>`, `BobField`, converters, error text provider | Doc 02 |
| Overlay primitives: `BobPortal`, `BobAnchor` (positioning, flip/shift), `BobPopover` (native `popover` first), `BobFocusScope` | Used by every dropdown, menu, tooltip, date picker |
| `BobVirtualizer`, `BobCollection<T>` model (items, keys, selection, text/value accessors) | Used by select, combobox, list, tree, grid |

## Tier 1: Inputs and basics (first release goal: replace hand-rolled forms in BobFit)
| Component | Have | Notes |
| --- | --- | --- |
| Button, IconButton, ButtonGroup, ToggleButton | partial (`AsyncButton`) | Base `BobButton`; AsyncButton builds on it |
| Text, TextArea, Password, Search | - | Doc 02 |
| Number (int/decimal/double), Slider | - | Converter-based |
| Checkbox, Switch, Radio group | - | |
| Select (native) | yes (`SelectField`) | Migrates to field base |
| Form, FormSummary, validation | - | |
| Label, Badge, Chip, Avatar, Divider, Icon | - | Display-only, static-SSR safe |
| Alert, Progress, Skeleton, Spinner | yes | |

## Tier 2: Overlays and feedback
| Component | Have | Notes |
| --- | --- | --- |
| Modal / dialog, Confirm | yes | Move onto event layer and `<dialog>` |
| Toast / snackbar | yes | |
| Drawer / sheet | partial (`ModalSheet`) | Side variants |
| Tooltip, Popover | - | Built on `BobAnchor` |
| Menu / context menu, Dropdown button | - | Roving focus, type-ahead |
| Loader overlays | yes | |

## Tier 3: Selection and date/time
| Component | Notes |
| --- | --- |
| Combobox / Autocomplete | Filter, async load, virtualized list, custom item templates, free text |
| Dropdown list (custom), MultiSelect (chips) | |
| DatePicker, TimePicker, DateTimePicker, DateRangePicker, Calendar | Culture-aware, min/max/disabled dates, keyboard grid. One calendar core |
| ColorPicker | After Calendar |
| FileUpload / dropzone | Progress, validation, chunking hook, cancel |
| Rating, SegmentPicker, RestPicker | SegmentPicker and RestPicker have |

## Tier 4: Navigation and layout
| Component | Notes |
| --- | --- |
| Tabs, Accordion/Expander, Stepper/Wizard | Keyboard model and ARIA patterns are the work |
| Breadcrumb, Pagination, Nav menu, App bar, Sidebar | |
| Card, Panel, Splitter, Tile layout | Splitter needs pointer events via the event layer |
| TreeView (virtualized, lazy, checkable, drag) | After `BobCollection<T>` and virtualizer |
| List / ListView, Timeline | |

## Tier 5: Data (the grid, deliberately last of the big ones)
| Component | Notes |
| --- | --- |
| DataTable (simple) | Static table with sort. Ships earlier as the stepping stone |
| **DataGrid** | Server/client data source abstraction, sort, filter (typed per column), page, selection, inline edit, templates, virtualization, column resize/reorder/hide, grouping, aggregates, export (CSV first), state persistence, keyboard navigation |
| PivotGrid, TreeList | Only if demand; built on the grid core |
| Sortable / drag and drop | Pointer-event driven; needed by TreeView, grid, Kanban |
| Kanban board | After drag and drop |

## Tier 6: Heavy (each is a product; schedule on demand)
Charts (expand beyond heatmap/progress chart; likely wrap SVG primitives), Scheduler/Calendar-events, Gantt, Rich text
editor, Diagram, Map, PDF viewer, Spreadsheet, Barcode/QR, Signature. Each gets its own design doc when picked up.

## Cross-cutting tracks (run alongside the tiers)
| Track | Items |
| --- | --- |
| Theming | Tokens complete for fields, density, dark mode; phase 8 style playground (separate design) |
| Docs | Migration tables, event reference from the registry, live examples |
| Quality | Trimmed/AOT publish in CI; render-count tests; axe-style a11y smoke on the demo; JS unit tests |
| Packaging | Publish prerelease to the private feed per tier; changelog per tier; API surface snapshot |

## Release shape
- 0.3: Tier 0 + Tier 1 (the "basic inputs" milestone)
- 0.4: Tier 2 + core of Tier 3 (combobox, date pickers)
- 0.5: Tier 4
- 0.6: DataTable + DataGrid first cut (sort, filter, page, select, virtualization)
- 0.7+: grid depth, Tier 3 remainder, then Tier 6 by demand
- 1.0 when the grid and the docs/migration guide are solid and the trimmed build is verified

## Honest sizing note
Tier 0 and Tier 1 are weeks of focused work with the parallel plan in doc 04. The date pickers, combobox and grid
are each larger than all of Tier 1 combined; "all popular components" is a multi-month series of releases, not one.
