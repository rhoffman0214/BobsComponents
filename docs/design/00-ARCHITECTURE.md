# Design 00: Architecture overview and decisions

Status: approved in direction by Bob (open questions answered, section 6). Nothing here is built yet. Read this first; 01 to 04 hold the detail.

| Doc | Subject |
| --- | --- |
| [01-EVENT-LAYER.md](01-EVENT-LAYER.md) | First-class events: DOM/JS events, interception, overriding, built-in behaviors as named events |
| [02-INPUTS-AND-FORMS.md](02-INPUTS-AND-FORMS.md) | Field foundation, basic inputs, forms and validation, and how errors surface |
| [03-COMPONENT-ROADMAP.md](03-COMPONENT-ROADMAP.md) | Prioritized catalogue of the popular components (Telerik, MudBlazor, Blazorise class) |
| [04-PARALLEL-WORK-PLAN.md](04-PARALLEL-WORK-PLAN.md) | Phase 0 spine, five worktree lanes (plus the grid lane in wave 2), file ownership, merge order |
| [05-GRID.md](05-GRID.md) | DataGrid V1 scope and staged slices |

## 1. What we are building and why

BobsComponents is a Blazor component library meant to cover the same ground as Telerik, MudBlazor and Blazorise, with
three differentiators that the existing code already points at:

1. **Everything is exposed back to the client.** Every part is a `data-bob-part`, every default is a `--bob-*` variable
   or a `BobOptions` value, and (new) every built-in behavior is a named, overridable event.
2. **Errors a person can act on.** One classifier turns any exception into a `BobError` with an origin, a stable code,
   structured facts and the exact message. This already exists (`Errors/`); the new work makes the errors
   *translatable* and extends the codes to events and inputs.
3. **Efficient by construction.** Native elements first, no JS where the browser already does the job, no per-keystroke
   round trips unless asked, one shared JS runtime instead of per-component listeners.

## 2. Layers

```
 App code (BobFit, Kanji, ...)
   |  parameters, @bind, EventCallbacks, DI interceptors, BobOptions, CSS variables
 ┌──────────────────────────────────────────────────────────────────────────────┐
 │ L4  Composite components   Grid, Scheduler, Combobox, DatePicker, TreeView    │
 │ L3  Basic components       Button, Fields (Text, Number, Check, ...), Tabs   │
 │ L2  Primitives             BobField chrome, Popover/Anchor, Portal, FocusScope│
 │                            Virtualizer, Collection<T> model                  │
 │ L1  Spine                  BobComponentBase, Event layer, Value layer (parse/ │
 │                            validate), Error system, Options, Dev kit hooks   │
 │ L0  Platform               Blazor WASM (Server later), one JS runtime module,   │
 │                            @layer bob CSS, tokens                            │
 └──────────────────────────────────────────────────────────────────────────────┘
```

Rule: a layer may use the layers below it only. Composites are built from the same public primitives an app could use,
which is what keeps "everything exposed" honest: there is no private door the library uses that the client cannot.

## 3. Key architectural decisions

Each decision names what was rejected, because the rejected options are what the other libraries chose.

### D1. One event spine, not per-component event plumbing
- **Chosen:** a single event layer (doc 01) that every component's DOM and behavior events pass through. A shared JS
  runtime module registers **one delegated listener per event type per root**, not one per element.
- **Why:** Blazor's `@onclick:preventDefault` is decided at render time and .NET runs async, so a .NET handler can never
  decide *synchronously* whether to cancel a DOM default. Libraries paper over this with ad-hoc JS per component, which
  is exactly where "can't override it" comes from. Doing it once, properly, gives every component the same customization.
- **Rejected:** per-component `IJSObjectReference` listeners (what `focusTrap.js` and `modalHistory.js` do today; they
  are migrated onto the spine in lane A). Rejected because it multiplies listeners, interop calls and override surfaces.

### D2. Decide in JS, react in .NET
- **Chosen:** *policy* (prevent, stop, key filters, debounce, throttle, passive, payload projection) is declared in .NET
  as data and executed synchronously in JS. Only events a policy marks as `ToDotNet` cross the interop boundary, with
  only the requested fields. Conditional synchronous logic is a named JS function the app registers.
- **Why:** this is the only design that is both correct (sync cancel) and cheap (no round trip for `mousemove` you do not
  care about). It also works on Blazor Server, where every interop call is a network hop.
- **Rejected:** "send everything to .NET and let it cancel" (wrong: too late) and "all logic in JS" (loses the typed
  .NET API the clients want).

### D3. Built-in behaviors are events with names
- **Chosen:** "Escape closes the modal", "Enter submits the form", "Tab traps focus" are not hidden code. Each is a named
  behavior (`bob.modal.close-on-escape`) raised as a cancellable event before it runs. Apps cancel, replace or disable
  it by name, at component, scope or app level.
- **Why:** this is what "first-class events with all the proper support and customization" means in practice, and it is
  the thing the big libraries do not offer. Names are public API (CLAUDE.md rule 9).

### D4. Fields share one value pipeline
- **Chosen:** every input derives from `BobFieldBase<TValue>`: `Value`/`ValueChanged`/`ValueExpression`, culture-aware
  parse via `IBobValueConverter<T>`, validation through `EditContext` (DataAnnotations work) *and* a lightweight
  built-in path, and a shared `BobField` chrome (label, hint, error, ARIA wiring).
- **Why:** label/hint/error/ARIA/`required`/`disabled` are the same for 20 inputs. Writing them once is how a11y stays
  "part of done" (rule 7) without 20 chances to get it wrong. Parse failures become structured field errors, never
  exceptions in the render path.
- **Rejected:** wrapping `InputBase<T>` directly (its parse/format hooks are too narrow, it has no bind-timing control and
  no part model). We still *integrate* with `EditContext` so existing forms work.

### D5. Structured errors, text rendered last
- **Chosen:** `BobError` keeps `Code` + `Facts` (already there) as the contract; the human message is produced by an
  `IBobErrorTextProvider` keyed by code and culture, falling back to the English catalog. The exact original exception
  message is always still available (CLAUDE.md error model).
- **Why:** "easier to translate and show" requires the *data* to be separable from the *sentence*. Today the catalog
  holds English templates; lane D adds culture files and the provider interface, so an app can translate, reword or
  route errors to its own UI (toast, banner, log) without parsing text.
- **Rule kept:** classification never reads message text.

### D6. Efficiency rules, enforced by tests where possible
1. Native element first (`<input>`, `<select>`, `<dialog>`, `popover`, `<details>`) before any custom widget. Fewer
   bytes, free a11y, free mobile UI.
2. No JS for a component unless the browser cannot do it. Components without JS must not import the runtime.
3. `ShouldRender` is overridden on every field and list item: re-render only when a parameter *value* changed or internal
   state changed. A test counts renders for N keystrokes.
4. Default bind timing is `OnChange` (blur/enter). `OnInput` and `Debounced(ms)` are opt-in; Server hosts can flip the
   default in `BobOptions.Inputs`.
5. Collections over ~50 items virtualize (`BobVirtualizer`); grids and trees are built on it from day one.
6. Trim/AOT-safe: no reflection on hot paths; reflection only in the docs engine and dev kit (not shipped to prod
   bundles). Verify with a trimmed publish in CI (currently unverified, tracked in the roadmap).
7. One JS runtime module (`bob.core.js`), lazily imported once per page, with small feature modules loaded on demand.

### D7. Compatibility stance with Telerik / MudBlazor / Blazorise (confirmed by Bob)
- **Chosen:** our own naming (consistent with the existing `bob-` contract), plus a **migration table per component**
  in its docs page (their parameter/event name -> ours). No shim packages in 1.0.
- **Why:** shims lock the API to someone else's mistakes and freeze naming we want to improve. A mapping table gives the
  migration value without the coupling.
- **Revisit:** if a specific app needs a bulk migration, a `BobsComponents.Compat.*` package can be added later without
  changing the core.

### D8. Hosting model
- **WASM first (decided by Bob).** Every design and default targets Blazor WebAssembly. Blazor Server is its own,
  separate target, built later; it is not a constraint on this release. We keep it cheap to add by not baking
  WASM-only assumptions into public contracts (the event spec model already separates policy from delivery), but we do
  not design, test or document Server behavior now.
- A per-app playground component (live editing of a component's parameters and CSS variables inside each app) is
  wanted, but is later work and not part of this design.
- Rendering stays static-SSR-safe where cheap (display-only components must not require interactivity).

## 4. What exists vs what is new

| Exists (keep, build on) | New (this design) |
| --- | --- |
| `BobComponentBase` (validate, callbacks, services) | `BobFieldBase<T>`, `BobField`, value converters |
| Error classifier, catalog, hub, boundary, card | Text provider + culture files; codes `BOB-EVT-*`, `BOB-INPUT-*`, `BOB-FORM-*` |
| `BobJs` loader with named exceptions | `bob.core.js` event runtime; `BobEvents` service; interceptor pipeline |
| Dev kit (errors, network, interop, DI, storage) | Events panel (live stream, which behavior was cancelled by whom) |
| Docs engine (generated pages, drift tests) | Event/behavior reference generated from the registry; migration tables |
| `focusTrap.js`, `modalHistory.js` per-component JS | Re-homed on the event spine as named behaviors |
| 20 components | Fields, overlay/anchor primitives, then the roadmap |

## 5. Non-goals (for now)
- Charts beyond the existing dependency-free SVG ones (a full chart suite is Tier 6).
- A rich-text editor and a scheduler are scoped in the roadmap but deliberately last; each is a product on its own.
- Theme designer UI (phase 8 "style playground" is separate and stays undesigned).

## 6. Decisions (Bob's answers)
1. **Naming:** own names, short and branded, e.g. `BobTextBox`; migration tables instead of shims (D7). Input names are in doc 02.
2. **Hosting:** WASM is the first target. Server is a separate target later (D8). Input bind-timing defaults follow WASM (D6.4).
3. **Grid:** everything possible goes into V1. Doc 03 Tier 5 and the new [05-GRID.md](05-GRID.md) stage it; the grid gets
   its own lane (doc 04).
4. **Translations:** capability only now (provider seam), translations later, never blocking (doc 02 s7.3).
