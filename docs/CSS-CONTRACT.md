# BobsComponents contract

This is the public surface a consuming app can rely on. Class names, custom properties, `data-bob-part`
values, static asset paths and error codes are API. Changes to them are breaking and go in the changelog.

A test (`ContractTests`) fails the build if CSS uses a name outside this contract, so keep it current.

## Package layout

| Piece | Where |
| --- | --- |
| Razor class library | `src/BobsComponents` (package id `BobsComponents`, namespace `BobsComponents`) |
| Tests (bUnit + xUnit) | `src/BobsComponents.Tests` |
| Docs engine (generators, page template, demos) | `src/BobsComponents.Docs` |
| Docs site host (routes only) | `src/BobsComponent.Client/Pages/Bob/` |
| Legacy library (pre-contract, not prefixed) | `src/BobsComponent.Library`, kept until its components are migrated |

Namespaces: `BobsComponents.Components` (UI), `BobsComponents.Errors`, `BobsComponents.DevKit`,
`BobsComponents.Options`, `BobsComponents.Services`, `BobsComponents.Infrastructure`.

## Using the package

```csharp
// Program.cs
builder.Services.AddBobComponents(o => o.EnvironmentName = builder.HostEnvironment.Environment);
```

```html
<!-- index.html (or App.razor <HeadContent>) -->
<link rel="stylesheet" href="_content/BobsComponents/bob.css" />
```

No Bootstrap or other CSS framework is required. Scoped CSS (`.razor.css`) is not used by the library.

## Static web asset paths

Everything under `src/BobsComponents/wwwroot` is served at `_content/BobsComponents/`.

| File | Used by |
| --- | --- |
| `_content/BobsComponents/bob.css` | the stylesheet to link; imports the files below into layer `bob` |
| `_content/BobsComponents/css/tokens.css` | global tokens |
| `_content/BobsComponents/css/base.css` | shared utilities |
| `_content/BobsComponents/css/components/*.css` | one sheet per component |
| `_content/BobsComponents/js/focusTrap.js` | ModalSheet |
| `_content/BobsComponents/js/modalHistory.js` | ModalSheet |

Scripts are imported with the document-relative path `./_content/BobsComponents/js/<file>` (see `BobJs`).

## CSS rules

1. Every class starts with `bob-`, named `bob-<block>` / `bob-<block>__<element>` / `bob-<block>--<modifier>`.
2. Every custom property starts with `--bob-`.
3. Every styleable part carries `data-bob-part="<name>"`. The root of a component is `data-bob-part="root"`.
4. All library CSS is inside `@layer bob` (sublayers `bob.tokens`, `bob.base`, `bob.components`). Your unlayered
   CSS wins without `!important`.
5. Layout uses flexbox. Grid only where a true two-dimensional layout is needed.
6. Component variables are read with a fallback (`var(--bob-modal-bg, var(--bob-color-surface))`) and never
   declared on the component, so setting one on `:root` or any ancestor always works.
7. Dark mode: follows `prefers-color-scheme`; force with `data-bob-theme="light"` or `"dark"` on `<html>` or any ancestor.
8. `prefers-reduced-motion: reduce` is honored once, in `base.css`.

## Global tokens (`css/tokens.css`)

| Property | Default (light) |
| --- | --- |
| `--bob-color-surface` | `#ffffff` |
| `--bob-color-surface-alt` | `#f3f4f6` |
| `--bob-color-border` | `#d1d5db` |
| `--bob-color-text` | `#111827` |
| `--bob-color-text-muted` | `#4b5563` |
| `--bob-color-ground` | `#ffffff` |
| `--bob-color-accent` | `#2563eb` |
| `--bob-color-accent-text` | `#ffffff` |
| `--bob-color-danger` | `#b91c1c` |
| `--bob-color-danger-text` | `#ffffff` |
| `--bob-color-success` | `#15803d` |
| `--bob-color-info` | `#1d4ed8` |
| `--bob-color-warning` | `#b45309` |
| `--bob-color-focus` | `#2563eb` |
| `--bob-font-family` | `inherit` |
| `--bob-font-display` | `inherit` |
| `--bob-font-mono` | `ui-monospace, SFMono-Regular, Menlo, Consolas, monospace` |
| `--bob-radius-sm` | `0.4rem` |
| `--bob-radius-md` | `0.6rem` |
| `--bob-radius-lg` | `1rem` |
| `--bob-shadow` | `0 0.5rem 1.5rem rgb(0 0 0 / 0.2)` |
| `--bob-z-toast` | `1200` |
| `--bob-z-modal` | `1300` |
| `--bob-z-devkit` | `2000` |
| `--bob-duration` | `150ms` |

### Mapping from BobFit variables

| BobFit | BobsComponents |
| --- | --- |
| `--surface` | `--bob-color-surface` |
| `--surface-border` | `--bob-color-border` |
| `--ink` | `--bob-color-text` |
| `--ink-muted` | `--bob-color-text-muted` |
| `--ground` | `--bob-color-ground` |
| `--accent` | `--bob-color-accent` |
| `--danger` | `--bob-color-danger` |
| `--accent-2` | `--bob-color-success` |
| `--font-display` | `--bob-font-display` |
| `--radius-md` / `--radius-lg` | `--bob-radius-md` / `--bob-radius-lg` |
| `--shadow` | `--bob-shadow` |

## Component variables

Each is optional; the default is what applies when it is not set.

### LoadingSpinner (`css/components/spinner.css`)

Classes: `bob-spinner`, `bob-spinner__icon`, `bob-spinner__label`, `bob-visually-hidden`.
Parts: `root`, `icon`, `label`.

| Property | Default |
| --- | --- |
| `--bob-spinner-color` | `currentColor` |
| `--bob-spinner-gap` | `0.5rem` |
| `--bob-spinner-speed` | `0.8s` |

### Button look (`css/components/button.css`)

Classes: `bob-async-button` (AsyncButton root), `bob-button`, `bob-button--default`, `bob-button--primary`, `bob-button--danger`.
AsyncButton parts: `root`.

| Property | Default |
| --- | --- |
| `--bob-button-bg` | `var(--bob-color-surface)` |
| `--bob-button-fg` | `var(--bob-color-text)` |
| `--bob-button-border` | `var(--bob-color-border)` |
| `--bob-button-radius` | `var(--bob-radius-md)` |
| `--bob-button-padding` | `0.5rem 1rem` |
| `--bob-button-min-height` | `2.75rem` |
| `--bob-button-gap` | `0.5rem` |

### FullScreenLoader (`css/components/fullscreen-loader.css`)

Classes: `bob-fullscreen-loader`, `bob-fullscreen-loader__text`. Parts: `root`, `text`.

| Property | Default |
| --- | --- |
| `--bob-fullscreen-loader-gap` | `1.25rem` |
| `--bob-fullscreen-loader-padding` | `2rem` |
| `--bob-fullscreen-loader-text-color` | `var(--bob-color-text)` |
| `--bob-fullscreen-loader-text-size` | `1.15rem` |

### ModalSheet (`css/components/modal.css`)

Classes: `bob-modal-backdrop`, `bob-modal`, `bob-modal__handle`, `bob-modal__header`, `bob-modal__title`,
`bob-modal__close`, `bob-modal__body`. Parts: `backdrop`, `sheet`, `handle`, `header`, `title`, `close`, `body`.

| Property | Default |
| --- | --- |
| `--bob-modal-z` | `var(--bob-z-modal)` |
| `--bob-modal-backdrop` | `rgb(0 0 0 / 0.5)` |
| `--bob-modal-bg` | `var(--bob-color-surface)` |
| `--bob-modal-border` | `var(--bob-color-border)` |
| `--bob-modal-radius` | `1.25rem 1.25rem 0 0` (phone), `var(--bob-radius-lg)` from 640px |
| `--bob-modal-max-width` | `26rem` |
| `--bob-modal-max-height` | `88vh` |
| `--bob-modal-padding` | `0.9rem 1rem 1.1rem` |
| `--bob-modal-shadow` | `var(--bob-shadow)` |

### ConfirmDialog (`css/components/confirm.css`)

Classes: `bob-confirm`, `bob-confirm__message`, `bob-confirm__actions`, `bob-confirm__cancel`, `bob-confirm__confirm`.
Parts: `root`, `message`, `actions`, `cancel`, `confirm`. It also uses the ModalSheet and button variables.

| Property | Default |
| --- | --- |
| `--bob-confirm-gap` | `1rem` |
| `--bob-confirm-message-color` | `var(--bob-color-text-muted)` |

### ToastHost (`css/components/toast.css`)

Classes: `bob-toast-host`, `bob-toast`, `bob-toast--info`, `bob-toast--success`, `bob-toast--error`,
`bob-toast__icon`, `bob-toast__text`. Parts: `host`, `toast`, `icon`, `text`.

| Property | Default |
| --- | --- |
| `--bob-toast-z` | `var(--bob-z-toast)` |
| `--bob-toast-bg` | `var(--bob-color-text)` |
| `--bob-toast-fg` | `var(--bob-color-ground)` |
| `--bob-toast-radius` | `var(--bob-radius-md)` |
| `--bob-toast-shadow` | `0 0.25rem 0.75rem rgb(0 0 0 / 0.25)` |
| `--bob-toast-offset` | `1.25rem` |
| `--bob-toast-icon-success` | `var(--bob-color-success)` |
| `--bob-toast-icon-error` | `var(--bob-color-danger)` |
| `--bob-toast-icon-info` | `var(--bob-color-info)` |

### BobErrorCard / BobErrorBoundary (`css/components/error-card.css`)

Classes: `bob-error-card`, `bob-error-card--origin-{component-bug|wrong-parameters|data-shape|missing|network|callback}`,
`bob-error-card__header`, `__badge`, `__code`, `__blame`, `__message`, `__facts`, `__fact`, `__toggle`, `__details`,
`__stack`, `__inner`, `__footer`, `__id`, `__recover`.
Parts: `root`, `header`, `badge`, `code`, `blame`, `message`, `facts`, `toggle`, `details`, `stack`, `inner`, `footer`, `id`, `recover`.
The root also carries `data-bob-origin` and `data-bob-code`.

| Property | Default |
| --- | --- |
| `--bob-error-card-bg` | `var(--bob-color-surface)` |
| `--bob-error-card-border` | `var(--bob-color-border)` |
| `--bob-error-card-radius` | `var(--bob-radius-md)` |
| `--bob-error-card-accent` | origin color (danger, warning, info or accent) |
| `--bob-error-card-font-mono` | `var(--bob-font-mono)` |
| `--bob-error-card-padding` | `0.9rem 1rem` |

### BobDevKit (`css/components/devkit.css`)

Classes: `bob-devkit`, `bob-devkit--{bottom-right|bottom-left|top-right|top-left}`, `bob-devkit__toggle`, `__count`,
`__panel`, `__tabs`, `__tab`, `__body`, `__toolbar`, `__empty`, `__clear`, `__entry`, `__entry--failed`, `__method`,
`__url`, `__time`, `__message`, `__table`, `__row--missing`.
Parts: `root`, `toggle`, `count`, `panel`, `tabs`, `tab`, `body`, `empty`, `entry`, `di-table`, `copy-report`, `storage-table`.

| Property | Default |
| --- | --- |
| `--bob-devkit-z` | `var(--bob-z-devkit)` |
| `--bob-devkit-bg` | `var(--bob-color-text)` |
| `--bob-devkit-fg` | `var(--bob-color-ground)` |
| `--bob-devkit-width` | `34rem` |
| `--bob-devkit-height` | `24rem` |

### ElapsedClock (`css/components/elapsed-clock.css`)

Classes: `bob-elapsed-clock`.
Parts: `root`.

| Property | Default |
| --- | --- |
| `--bob-elapsed-clock-color` | `inherit` |
| `--bob-elapsed-clock-font` | `var(--bob-font-mono)` |
| `--bob-elapsed-clock-size` | `inherit` |

### RestPicker (`css/components/rest-picker.css`)

Classes: `bob-rest-picker`, `bob-rest-picker__legend`, `bob-rest-picker__chips`, `bob-rest-picker__chip`, `bob-rest-picker__chip--on`, `bob-rest-picker__other`, `bob-rest-picker__hint`.
Parts: `root`, `legend`, `chips`, `chip`, `other`, `input`, `hint`.

| Property | Default |
| --- | --- |
| `--bob-rest-picker-gap` | `0.5rem` |
| `--bob-rest-picker-radius` | `999px` |
| `--bob-rest-picker-chip-bg` | `var(--bob-color-surface)` |
| `--bob-rest-picker-chip-fg` | `var(--bob-color-text)` |
| `--bob-rest-picker-chip-on-bg` | `var(--bob-color-accent)` |
| `--bob-rest-picker-chip-on-fg` | `var(--bob-color-accent-text)` |

### SegmentPicker (`css/components/segment-picker.css`)

Classes: `bob-segment-picker`, `bob-segment-picker__title`, `bob-segment-picker__presets`, `bob-segment-picker__chip`, `bob-segment-picker__chip--on`, `bob-segment-picker__add`, `bob-segment-picker__add-button`, `bob-segment-picker__list`, `bob-segment-picker__item`, `bob-segment-picker__item--lifted`, `bob-segment-picker__handle`, `bob-segment-picker__name`, `bob-segment-picker__remove`, `bob-segment-picker__hint`.
Parts: `root`, `title`, `presets`, `chip`, `add`, `input`, `add-button`, `list`, `item`, `handle`, `name`, `remove`, `hint`, `status`.

| Property | Default |
| --- | --- |
| `--bob-segment-picker-gap` | `0.5rem` |
| `--bob-segment-picker-chip-bg` | `var(--bob-color-surface)` |
| `--bob-segment-picker-chip-fg` | `var(--bob-color-text)` |
| `--bob-segment-picker-chip-on-bg` | `var(--bob-color-accent)` |
| `--bob-segment-picker-chip-on-fg` | `var(--bob-color-accent-text)` |
| `--bob-segment-picker-lifted-bg` | `var(--bob-color-surface)` |

### DraftSaver (`css/components/draft-saver.css`)

Classes: `bob-draft-saver`.
Parts: `status`.

DraftSaver has no custom properties; its only element is a visually hidden status line.

### ProgressChart (`css/components/progress-chart.css`)

Classes: `bob-progress-chart`, `bob-progress-chart__grid`, `bob-progress-chart__row`, `bob-progress-chart__yaxis`, `bob-progress-chart__svg`, `bob-progress-chart__line`, `bob-progress-chart__point`, `bob-progress-chart__xaxis`, `bob-progress-chart__empty`.
Parts: `root`, `empty`, `grid`, `row`, `yaxis`, `plot`, `line`, `point`, `xaxis`, `table`.

| Property | Default |
| --- | --- |
| `--bob-progress-chart-height` | `8rem` |
| `--bob-progress-chart-line` | `var(--bob-color-accent)` |
| `--bob-progress-chart-point` | `var(--bob-color-accent)` |
| `--bob-progress-chart-axis-color` | `var(--bob-color-text-muted)` |
| `--bob-progress-chart-axis-gutter` | `2.5rem` |

### ActivityHeatmap (`css/components/activity-heatmap.css`)

Classes: `bob-activity-heatmap`, `bob-activity-heatmap__stats`, `bob-activity-heatmap__stat`, `bob-activity-heatmap__value`, `bob-activity-heatmap__label`, `bob-activity-heatmap__loading`, `bob-activity-heatmap__scroll`, `bob-activity-heatmap__grid`, `bob-activity-heatmap__week`, `bob-activity-heatmap__cell`, `bob-activity-heatmap__cell--level-1`, `bob-activity-heatmap__cell--level-2`, `bob-activity-heatmap__cell--level-3`, `bob-activity-heatmap__cell--empty`.
Parts: `root`, `loading`, `stats`, `stat`, `scroll`, `grid`, `week`, `cell`.

| Property | Default |
| --- | --- |
| `--bob-activity-heatmap-cell-size` | `0.75rem` |
| `--bob-activity-heatmap-gap` | `3px` |
| `--bob-activity-heatmap-level-0` | `var(--bob-color-surface-alt)` |
| `--bob-activity-heatmap-level-1` | 35% accent mixed into surface-alt |
| `--bob-activity-heatmap-level-2` | 65% accent mixed into surface-alt |
| `--bob-activity-heatmap-level-3` | `var(--bob-color-accent)` |

### NotificationBell (`css/components/notification-bell.css`)

Classes: `bob-notification-bell`, `bob-notification-bell__button`, `bob-notification-bell__icon`, `bob-notification-bell__badge`, `bob-notification-bell__backdrop`, `bob-notification-bell__panel`, `bob-notification-bell__status`, `bob-notification-bell__list`, `bob-notification-bell__item`, `bob-notification-bell__link`, `bob-notification-bell__message`, `bob-notification-bell__time`.
Parts: `root`, `button`, `icon`, `badge`, `backdrop`, `panel`, `status`, `list`, `item`, `link`.

| Property | Default |
| --- | --- |
| `--bob-notification-bell-badge-bg` | `var(--bob-color-danger)` |
| `--bob-notification-bell-badge-text` | `var(--bob-color-accent-text)` |
| `--bob-notification-bell-panel-width` | `20rem` |
| `--bob-notification-bell-z` | `10` |

### ProgressBar (`css/components/progress.css`)

Classes: `bob-progress`, `bob-progress--success`, `bob-progress--warning`, `bob-progress--danger`, `bob-progress--indeterminate`, `bob-progress__fill`, `bob-progress__value`.
Parts: `root`, `fill`, `value`.

| Property | Default |
| --- | --- |
| `--bob-progress-height` | `1.25rem` |
| `--bob-progress-radius` | `999px` |
| `--bob-progress-track` | `var(--bob-color-surface-alt)` |
| `--bob-progress-fill` | `var(--bob-color-accent)` |
| `--bob-progress-value-fg` | `var(--bob-color-text)` |
| `--bob-progress-speed` | `1.4s` |

### SkeletonLoader (`css/components/skeleton.css`)

Classes: `bob-skeleton-group`, `bob-skeleton`, `bob-skeleton--circle`, `bob-skeleton--text`, `bob-skeleton--rectangle`.
Parts: `root`, `label`, `shape`.

| Property | Default |
| --- | --- |
| `--bob-skeleton-gap` | `0.5rem` |
| `--bob-skeleton-width` | `100%` |
| `--bob-skeleton-height` | `1.25rem` |
| `--bob-skeleton-radius` | `var(--bob-radius-sm)` |
| `--bob-skeleton-bg` | `var(--bob-color-surface-alt)` |
| `--bob-skeleton-highlight` | `var(--bob-color-border)` |
| `--bob-skeleton-speed` | `1.5s` |

### SelectField (`css/components/select.css`)

Classes: `bob-select`, `bob-select--disabled`, `bob-select__label`, `bob-select__control`.
Parts: `root`, `label`, `control`.

| Property | Default |
| --- | --- |
| `--bob-select-gap` | `0.35rem` |
| `--bob-select-label-fg` | `var(--bob-color-text)` |
| `--bob-select-min-height` | `2.75rem` |
| `--bob-select-border` | `var(--bob-color-border)` |
| `--bob-select-radius` | `var(--bob-radius-md)` |
| `--bob-select-bg` | `var(--bob-color-surface)` |
| `--bob-select-fg` | `var(--bob-color-text)` |

## Error model

Every exception is classified once into a `BobError` with one of six origins. Classification looks at exception
types, structured properties and the assembly of the first non-framework stack frame, never at message text.

| Origin | Badge | Codes | Raised when |
| --- | --- | --- | --- |
| ComponentBug | Component bug | `BOB-COMP-001` | exception from BobsComponents code that matches nothing else |
| WrongParameters | Wrong parameters | `BOB-PARAM-001/002/003` | `BobComponentBase.ValidateParameters` fails |
| DataShapeMismatch | Data shape mismatch | `BOB-DATA-001/002` | `BobDataShapeException`, `JsonException` (path kept) |
| MissingAssetOrService | Missing asset or service | `BOB-ASSET-001/002` | `JSException`, failed import (`BobJs`), `RequireService<T>()` |
| NetworkFailure | Network failure | `BOB-NET-001/002/003` | `BobHttpException`, `HttpRequestException`, timeouts |
| CallbackThrew | Your callback threw | `BOB-CB-001/002` | app code threw (callback wrapper, or first owned frame is not in BobsComponents) |

The catalog (`Errors/error-catalog.json`, embedded) documents each code: title, message shape, cause, fix.
The card always shows the exact message. Stack traces and inner exceptions are behind a Details toggle by default
(`BobOptions.Errors.Details`: `Toggle`, `AlwaysOpen`, `Hidden`).

Known limit: an exception inside a fire-and-forget task never reaches an error boundary. Components await their work
or call `ReportError(ex)`; the dev kit also listens for unobserved task exceptions.
