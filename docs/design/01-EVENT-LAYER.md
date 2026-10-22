# Design 01: The event layer

Status: draft. Goal: JS/DOM events and the library's own built-in behaviors are first-class, typed, interceptable,
overridable and observable, without paying for it when nobody listens.

## 1. The problem Blazor leaves us with

1. A .NET handler runs asynchronously, so by the time it decides, the browser has already applied the default action.
   `@onclick:preventDefault` is fixed at render time and cannot depend on the event.
2. High-frequency events (`input`, `pointermove`, `scroll`, `resize`) are expensive to forward; on Blazor Server each is
   a network message.
3. Component libraries hide their keyboard and focus behavior in private JS, so an app cannot say "Escape should not
   close this modal while the form is dirty".
4. Handler failures surface as framework noise unrelated to the component that hosted them.

## 2. Model: Spec -> Pipeline -> Handlers

```
 DOM event ──► [ JS runtime, synchronous ] ──► [ .NET pipeline, async ] ──► app handlers / component default
                 delegated listener              Before -> Behavior -> After
                 policy: filter, prevent,        interceptors can cancel / replace
                 stop, throttle, project         the behavior, then After observers run
                 sync JS interceptors (opt.)
```

### 2.1 `BobEventSpec` (data, declared in .NET, executed in JS)

A spec says *what to do in the browser* for one event on one part. It is plain data so it can be sent once, at
registration, as JSON.

| Field | Meaning |
| --- | --- |
| `Event` | DOM event name (`keydown`, `input`, `pointerdown`, ...) |
| `Part` | `data-bob-part` target inside the component root (default `root`) |
| `Keys` | optional key filter, e.g. `["Escape"]`, `["Enter","Ctrl+Enter"]` (normalized, layout-safe via `code` and `key`) |
| `PreventDefault`, `StopPropagation` | `Always` / `Never` / `WhenHandled` (decided by sync JS interceptor result) |
| `Capture`, `Passive` | listener options; touch/wheel default to passive |
| `Throttle`, `Debounce` | ms; `Debounce` also supports `Leading` |
| `Payload` | projection: which event fields cross to .NET (`key`, `code`, `ctrlKey`, `value`, `clientX`, ...). Default is the minimum for that event. |
| `Delivery` | `JsOnly` (never crosses), `ToDotNet` (async to the pipeline), or `Both` |
| `Behavior` | name of the built-in behavior this spec triggers, if any (see 2.3) |

### 2.2 Typed .NET events

```csharp
public sealed class BobEvent<TArgs>
{
    public required string Name { get; }          // "bob.modal.close-on-escape" or "dom.keydown"
    public required object Source { get; }        // the component instance
    public required TArgs Args { get; }
    public bool IsCancelled { get; }              // read
    public void Cancel(string? reason = null);    // stops the default behavior, reason shown in dev kit
    public void Replace(Func<ValueTask> behavior);// run this instead of the default
    public IReadOnlyDictionary<string,string> Facts { get; } // for errors and dev kit
}
```

Phases, always in this order: **Before** (interceptors may cancel/replace) -> **Behavior** (the component default,
skipped if cancelled or replaced) -> **After** (observers; cannot cancel; run even when cancelled, with
`IsCancelled = true`, so logging/analytics see everything).

### 2.3 Built-in behaviors are named events

Every default behavior a component performs because of user input is registered in a single `BobBehaviorRegistry` with a
stable dotted name, its args type and its default spec. Examples:

| Name | Default | Typical override |
| --- | --- | --- |
| `bob.modal.close-on-escape` | Escape closes `ModalSheet` | cancel while the form is dirty |
| `bob.modal.close-on-backdrop` | backdrop click closes | disable for critical dialogs |
| `bob.modal.trap-focus` | Tab cycles inside | replace with a custom focus order |
| `bob.field.commit-on-enter` | Enter commits a field | Ctrl+Enter instead (textarea) |
| `bob.select.type-ahead` | printable key jumps to option | disable, or change the buffer timeout |
| `bob.grid.row-activate` | double-click / Enter opens row | route to app navigation |
| `bob.popover.dismiss-outside` | outside pointerdown closes | keep open while a child dialog shows |

The registry is the single source for: the docs page ("Events and behaviors" section, generated like the rest of the
docs), the dev kit events panel, and the did-you-mean errors for mistyped names.

## 3. How an app uses it (three levels, simplest first)

**Level 1: a parameter on the component (most common).**
```razor
<ModalSheet Title="Edit" OnBefore="@(e => e.Name == "bob.modal.close-on-escape" && _dirty ? e.Cancel("unsaved changes") : default)">
```
Every component exposes `OnBefore` and `OnAfter` of type `EventCallback<BobEvent>`-like delegates (`Func<BobEvent, ValueTask>`)
for *all* its events, plus the existing typed callbacks (`ValueChanged`, `OnClose`) for the semantic ones.

**Level 2: scoped overrides for a subtree.**
```razor
<BobEventScope Disable="@("bob.modal.close-on-backdrop")" Intercept="@_interceptor">
    ...every ModalSheet in here ignores backdrop close...
</BobEventScope>
```
`BobEventScope` is a cascading value. Components resolve their effective policy: **component parameter > nearest scope
> outer scope > app-wide `BobOptions.Events` > built-in default**. The resolved chain is visible in the dev kit.

**Level 3: app-wide DI interceptors and sync JS.**
```csharp
services.AddBobComponents(o => o.Events.Disable("bob.select.type-ahead"));
services.AddSingleton<IBobEventInterceptor, AuditInterceptor>();   // Before/After for everything
```
```js
// Synchronous, runs in the browser before any .NET hop. Return true to preventDefault.
BobEvents.intercept("bob.field.commit-on-enter", (ev, ctx) => ctx.part === "control" && ev.isComposing);
```
Sync JS interceptors are the only way to *conditionally* prevent a browser default; they are registered by name and are
optional. Everything else is async .NET.

**Raw DOM access without a component:** `<BobEventTarget Spec="..." OnEvent="...">` wraps any markup and gives it the
same spec/pipeline, so apps get the same guarantees for their own elements (replaces ad-hoc `@onkeydown` plus JS interop).

## 4. Overriding rules (the customization contract)

1. **Cancel** stops the default but later interceptors and After observers still see the event.
2. **Replace** swaps the default for app code; at most one replace wins (closest scope), the rest are logged as shadowed.
3. **Disable** (by name) is cancel-before-it-starts and costs nothing at runtime: the spec is never sent to JS.
4. **Modify spec** (`o.Events.Configure("bob.field.commit-on-enter", s => s with { Keys = ["Ctrl+Enter"] })`) changes
   when it fires, without code in the pipeline.
5. Order inside a phase: component parameter, then nearest scope outward, then DI, in registration order.
6. IME composition, auto-repeat and text-entry targets are handled by the runtime for key specs (a `keydown` spec never
   fires for `isComposing`, and Space/Enter on a text control is not stolen unless the spec names that part).

## 5. Efficiency design

- **One delegated listener per event type per root** (the runtime keeps a refcount; the listener is removed when the last
  spec is gone). No listener at all for `Delivery.JsOnly` specs that need no JS callback.
- **Registration is batched**: a component sends its specs in one interop call on first render, and a diff on parameter
  change. Components with no events registered never import the runtime.
- **Projection**: the default payload per event is minimal (`key`, `code`, modifier flags for keys; `value` for input;
  coordinates only on request). No `event.target` serialization.
- **Coalescing**: `input`/`pointermove`/`scroll`/`resize` use `requestAnimationFrame` or the spec's throttle; the last
  value wins; the pipeline is never entered with a backlog (latest-wins queue per spec, configurable to `Queue`).
- **Pipeline cost when nobody listens**: if a behavior has no interceptors and no overrides, the pipeline short-circuits
  to the default with no allocations (`BobEvent` is pooled for the no-interceptor case).
- **Server safety**: the effective default for `input` is `OnChange`; `Debounce` defaults are higher when
  `BobOptions.Hosting == Server`.

## 6. Errors (new codes, all in `error-catalog.json`)

All carry structured facts so they translate (see doc 02 section 7 and architecture D5).

| Code | Origin | When | Facts |
| --- | --- | --- | --- |
| `BOB-EVT-001` | YourCallback | An interceptor or handler threw | `event`, `phase`, `handler`, inner exception |
| `BOB-EVT-002` | MissingAssetOrService | A named sync JS interceptor is not registered | `name`, `registeredNames` |
| `BOB-EVT-003` | WrongParameters | `Disable`/`Configure`/`Intercept` names an unknown behavior | `name`, `didYouMean` |
| `BOB-EVT-004` | WrongParameters | Spec invalid (unknown payload field, conflicting prevent/passive, negative debounce) | `spec`, `field`, `expected` |
| `BOB-EVT-005` | ComponentBug | Pipeline invariant broken (double Replace commit, event used after completion) | `event`, `state` |
| `BOB-EVT-006` | MissingAssetOrService | Runtime module failed to load | `script` (reuses `BobJs` path) |

Fire-and-forget handler failures go to `IBobErrorHub` (never into the render tree); handlers awaited in the pipeline are
wrapped in `BobCallbackException` with phase info, consistent with the current `InvokeCallbackAsync`.

## 7. Observability

Dev kit **Events** panel (new): live stream of `name, component, phase, who cancelled/replaced, duration, payload`,
filterable by component and name; a "why did this not fire" view that shows the resolved override chain for a behavior.
`Copy report` includes it. Honors the existing rule: never record values from password-type fields.

## 8. Migration of what exists

| Today | After |
| --- | --- |
| `focusTrap.js` per modal | behavior `bob.modal.trap-focus` on the runtime; `focusTrap.js` kept as the behavior's implementation module |
| `modalHistory.js` back-button close | behavior `bob.modal.back-closes`; same module, spec-driven |
| Escape / backdrop in `ModalSheet`, `ConfirmDialog` | named behaviors above, same default behavior |
| Toast dismiss timers | stay .NET timers, expose `bob.toast.auto-dismiss` as cancellable |

No visible behavior change for existing users; the only API change is additive (`OnBefore`/`OnAfter`, scopes).

## 9. Test strategy
- Pure C# pipeline tests: ordering, cancel, replace, shadowing, disabled specs, exception mapping.
- Spec-serialization snapshot tests (the JSON sent to JS is a contract).
- JS runtime tests (node + jsdom, in `package.json` which already exists): delegation, key normalization, IME guard,
  throttle/debounce, projection, sync interceptor cancel.
- bUnit tests for each migrated component proving the default behavior is unchanged.
- A drift test: every `Behavior` named in code appears in the generated docs and the catalog, and the reverse.
- A perf test: a text field with no listeners does zero runtime imports and N keystrokes cause <= N+1 renders.

## 10. Alternatives considered
| Option | Why not |
| --- | --- |
| Forward all events to .NET and cancel there | Cancel arrives too late for browser defaults; floods Server circuits |
| Rx/observable streams as the API | Powerful, but a heavy concept to impose on every Blazor dev; the phase pipeline covers the use cases |
| Expose raw `IJSRuntime` hooks only | That is the status quo in other libraries; no typed args, no names, no docs, no dev kit |
| Per-component custom event classes with no shared base | Cannot be generically intercepted, logged or documented |
