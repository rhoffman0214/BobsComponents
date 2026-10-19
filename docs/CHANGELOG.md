# Changelog

Names are public API: CSS classes, `--bob-*` custom properties, `data-bob-part` values, component parameters and error codes.
Every release lists **Breaking** changes first. A test (`ApiSurfaceTests`) compares the code against `docs/api-surface.txt`
and fails when a documented name disappears without an entry in the Breaking section of the newest release.

## 0.2.0 - 2026-10-18

### Added
- Dev kit: a Storage tab (localStorage and sessionStorage key names and sizes, never values, with a Remove button; hide it with `DevKit.ShowStoragePanel = false`) and a Copy report button that puts a plain-text snapshot of every panel on the clipboard.
- Second-wave component `ElapsedClock`: a self-ticking `mm:ss` / `h:mm:ss` readout (`role="timer"`) that re-renders only itself and can be paused.
- Second-wave component `RestPicker`: rest length chips (`aria-pressed`) plus a free number box in one `fieldset`, bound with `@bind-Value`.
- Second-wave component `SegmentPicker`: preset chips, custom areas and a keyboard-reorderable list bound as newline-separated text (no pointer drag).
- Second-wave component DraftSaver: hot-saves unsaved form state (string payload) through IBobDraftStore, restores it once on load and announces the restore.
- Second-wave component ProgressChart: dependency-free SVG line chart placed by date, with text axes and a hidden data table; no JS.
- ActivityHeatmap: streak stats plus a multi-week activity heatmap; presentational, takes timestamps as a parameter.
- NotificationBell: bell button with unread badge and a notification panel, fed by IBobNotificationSource.

## 0.1.0 - 2026-10-18

### Added
- `BobsComponents` Razor class library: `bob-` classes, `--bob-*` custom properties and `data-bob-part` on every styleable part,
  plain stylesheets in `@layer bob`, no framework requirement.
- Error core: every exception becomes one `BobError` with an origin, a code from `error-catalog.json` and the exact message.
  `BobErrorBoundary` and `BobErrorCard` show it; stack traces sit behind a Details toggle.
- Dev kit: `BobDevKit` with errors, network, interop and DI panels.
- First-wave components: `LoadingSpinner`, `AsyncButton`, `FullScreenLoader`, `ModalSheet`, `ConfirmDialog`, `ToastHost` with `IBobToastService`.
- Docs site: component pages generated from the code (parameters from `[Parameter]` and XML docs, CSS reference from the
  stylesheets, DOM map from the rendered demo, accessibility notes with the tests that prove them), a part inspector,
  one search over components, parts, classes, variables and effects, copy-ready CSS snippets, the error catalog and this changelog.
