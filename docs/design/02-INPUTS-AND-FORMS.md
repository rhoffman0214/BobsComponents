# Design 02: Inputs, forms, validation and translatable errors

Status: draft. Builds on `BobComponentBase`, the `BobError` system and the event layer (doc 01).

## 1. Goals
- Basic inputs that are correct by default (label, hint, error, ARIA, required, disabled, readonly, autocomplete).
- One value pipeline for every input: parse, validate, format, bind timing.
- Works with Blazor `EditForm`/`EditContext`/DataAnnotations, and also without them.
- Every failure (developer mistake or user mistake) is a structured, translatable message with a stable code.
- Cheap to render: native elements, minimal re-renders, no JS unless needed.

## 2. Class model

```
BobComponentBase                      (exists)
 └─ BobFieldBase<TValue>              value pipeline, EditContext bridge, shared parameters, ARIA ids
     ├─ BobTextField / BobTextArea / BobPasswordField / BobSearchField     (string)
     ├─ BobNumberField<T>             (int, long, decimal, double, nullable variants)
     ├─ BobCheckbox / BobSwitch       (bool, bool?)
     ├─ BobRadioGroup<T> + BobRadio<T>
     ├─ BobSelectField<T>             (migrate existing SelectField, keep its CSS contract)
     └─ later tiers: Date, Time, Slider, Combobox, MultiSelect, FileUpload ... (doc 03)

BobField                              chrome component (label, control slot, hint, error list). Public, reusable.
BobForm                               wraps EditForm; adds submit pipeline, error summary, dirty tracking
BobFormScope                          cascading defaults (density, label placement, validate-on, disabled)
IBobValueConverter<T>                 culture-aware parse/format, one per value type
IBobFieldValidator<T>                 optional sync/async rule objects (non-DataAnnotations)
```

`SelectField` already ships. It migrates onto `BobFieldBase<T>` in lane B with its CSS and parameter names unchanged
(no breaking change, only the plumbing moves).

## 3. Shared parameters (every field)

| Parameter | Type | Notes |
| --- | --- | --- |
| `Value`, `ValueChanged`, `ValueExpression` | `TValue`, `EventCallback<TValue>`, `Expression<Func<TValue>>` | Standard `@bind-Value` shape; `ValueExpression` is what links to `EditContext` |
| `Label`, `Hint`, `Placeholder` | `string?` | Label is required for a11y unless `AriaLabel` is set (parameter validation raises `BOB-PARAM-001`) |
| `Required`, `Disabled`, `ReadOnly` | `bool` | `Required` adds `aria-required` and a built-in rule |
| `BindOn` | `BobBindTiming` | `Change` (default), `Input`, `Debounced` (+ `DebounceMs`) |
| `ValidateOn` | `BobValidateTiming` | `Blur` (default), `Change`, `Input`, `Submit`; first error shows on blur, then live (the "reward early, punish late" pattern) |
| `Errors` | `IReadOnlyList<BobFieldError>?` | External (server-side) errors an app can push in; merged with computed ones |
| `Id`, `Name`, `AutoComplete` | `string?` | Name defaults from `ValueExpression` for forms/autofill |
| `OnBefore`, `OnAfter` | event delegates | From the event layer; events such as `bob.field.commit-on-enter` |
| `AdditionalAttributes` | captured | Applied to the native control, not the wrapper (documented, tested) |

## 4. The value pipeline

```
 DOM input  ──►  raw string (per BindOn)  ──►  IBobValueConverter<T>.TryParse(raw, culture)
                                                   │ fail → BobFieldError(BOB-INPUT-001, facts) ; Value unchanged
                                                   ▼ ok
                                              Value set ──► ValueChanged ──► EditContext.NotifyFieldChanged
                                                   ▼
                                       validators (DataAnnotations via EditContext + IBobFieldValidator<T>)
                                                   ▼
                                       merged error list ──► BobField renders + aria-describedby/aria-invalid
```

Points that matter:
- **Parse failure is data, not an exception.** `"12x"` in a number field yields an error entry with facts
  `{input:"12x", expected:"integer", culture:"en-US"}`. The field keeps showing the user's raw text so they can fix it.
- **Culture** comes from `BobOptions.Culture` (default: `CultureInfo.CurrentCulture`). Number fields render with
  `inputmode` and `type="text"` + converter, not `type="number"`, because native number inputs hide the raw text and
  differ per browser; this is a deliberate choice (documented), with `UseNativeNumber` as an opt-in.
- **Bind timing** is the main efficiency lever: `Change` costs one round trip per commit, `Input` one per keystroke,
  `Debounced` one per pause. Defaults follow doc 00 D6.4.
- **Re-rendering**: `ShouldRender` compares parameter values; internal raw text lives in a field and updates the DOM via
  `value` binding only when it differs. A test asserts <= N+1 renders for N keystrokes.
- **Nullables** are first class (`int?`, `DateTime?`); empty string maps to `null`, and `Required` rejects it.

## 5. Validation

Three sources, merged by field and by form, in this order of display:
1. **Parse errors** (converter).
2. **Built-in rules** from parameters: `Required`, `MinLength`/`MaxLength`, `Min`/`Max`, `Pattern`, `Step`.
3. **Model rules**: DataAnnotations via `EditContext` (works unchanged), plus `IBobFieldValidator<T>` (async allowed;
   cancellation on supersede; a `Validating` state exposed as `aria-busy`).
Plus **external errors** pushed through `Errors` or `BobFormContext.AddErrors(...)` (server validation, uniqueness checks).

Error entry shape (the translation contract):

```csharp
public sealed record BobFieldError(
    string Code,                              // "BOB-INPUT-001", "BOB-VALID-REQUIRED", ...
    IReadOnlyDictionary<string,string> Facts, // { field:"Weight", min:"20", max:"500", input:"12x" }
    BobFieldErrorKind Kind,                   // Parse | Rule | Model | External
    string? Message = null);                  // already-rendered text; null → provider renders from Code+Facts
```

## 6. Forms

`BobForm` wraps `EditForm` and adds:
- **Submit pipeline** as named events (doc 01): `bob.form.submit` (cancellable; default runs validation), `bob.form.invalid`,
  `bob.form.reset`. So "Enter submits" and "block submit while uploading" are overridable, not hidden.
- **Dirty tracking** and `bob.form.leave-guard` (optional, uses the same back-button machinery as `modalHistory`).
- **Error summary** (`BobFormSummary`): lists every error with a link that focuses the field, announced via a live region;
  first invalid field takes focus after a failed submit (configurable).
- **Busy state**: while `OnValidSubmit` runs, fields get `aria-busy`, the submit button is an `AsyncButton`, and double
  submit is blocked by default.
- **Scope defaults** via `BobFormScope`: density, label placement, `ValidateOn`, `BindOn`, disabled-all.
- **Server errors**: `OnValidSubmit` may return `BobFormResult` with field-keyed errors, mapped back onto fields.

Forms without `EditForm` are supported: `BobFormContext` can be created manually (`Validate()`, `Errors`, `IsDirty`) for
wizard steps and dialogs.

## 7. Errors that are sensible and translatable

Two classes of problem, handled differently on purpose:

| | Developer mistakes (wrong parameters, missing service) | User input problems (parse, rules) |
| --- | --- | --- |
| Surface | `BobError` -> `BobErrorCard`/boundary/dev kit | `BobFieldError` -> inline under the field |
| Audience | developer | end user |
| Throw? | yes, at the boundary, once, classified | never: data, not exceptions |
| Codes | `BOB-PARAM-*`, `BOB-DATA-*`, `BOB-EVT-*` | `BOB-INPUT-*`, `BOB-VALID-*` |

### 7.1 Text is produced last
```csharp
public interface IBobErrorTextProvider
{
    // Returns null when it has no text for this code/culture; the next provider (or English catalog) is tried.
    string? GetText(string code, IReadOnlyDictionary<string,string> facts, CultureInfo culture, BobTextAudience audience);
}
```
- Providers chain: **app override -> culture resource -> English catalog (`error-catalog.json`)**.
- Templates use named placeholders from `Facts` (`"{field} must be between {min} and {max}."`), the same style the catalog
  already uses, so translators see the whole sentence and can reorder words. No string concatenation in components.
- `BobTextAudience` is `Developer` or `User`: developer text keeps the cause/fix wording from the catalog; user text is
  short and has no jargon (`"Enter a whole number"`).
- Plural/gender: templates support an ICU-style `{count, plural, one {...} other {...}}` subset (small parser, in lane D).
- **Routing**: an app can subscribe to errors instead of showing them inline (`IBobFieldErrorSink`: toast, summary
  banner, log), and it receives code + facts + rendered text, so UI and translation are independent.
- The component never loses the original: `BobError.Exception` and the exact original message stay available behind the
  Details toggle, unchanged from the current behavior.

### 7.2 New codes (added to `error-catalog.json`; the catalog test enforces constants <-> catalog)
| Code | Kind | Meaning | Facts |
| --- | --- | --- | --- |
| `BOB-INPUT-001` | user | Value could not be parsed | `input`, `expected`, `culture` |
| `BOB-INPUT-002` | developer | Unsupported `TValue` for this field (no converter registered) | `component`, `type`, `supported` |
| `BOB-INPUT-003` | developer | `ValueExpression` missing for a field inside a form | `component`, `fix` |
| `BOB-INPUT-004` | developer | Label missing and no `AriaLabel` | `component` |
| `BOB-VALID-REQUIRED` | user | Required value missing | `field` |
| `BOB-VALID-RANGE` | user | Outside `Min`/`Max` | `field`, `min`, `max`, `actual` |
| `BOB-VALID-LENGTH` | user | Outside `MinLength`/`MaxLength` | `field`, `min`, `max`, `actual` |
| `BOB-VALID-PATTERN` | user | Pattern mismatch | `field`, `hint` |
| `BOB-VALID-MODEL` | user | DataAnnotations message passthrough | `field`, `message` |
| `BOB-FORM-001` | developer | Field used outside a form that requires context | `component` |
| `BOB-FORM-002` | developer | Async validator threw | `field`, `validator`, inner exception |

Codes are final once documented. The existing wording rule applies: titles/messages say what happened and what to do.

## 8. Accessibility (done criteria for every field)
- `<label for>` wired to the native control; `aria-describedby` lists hint and error ids; `aria-invalid` when errors exist.
- Errors in a `role="alert"`-free live region (polite) so typing does not shout; the form summary uses `role="alert"` on submit.
- Required indicated by text, not color alone; focus ring from the existing token; touch target >= 44px.
- Each bullet in `Content/accessibility/<slug>.md` names a bUnit test, as the repo's rule requires.

## 9. CSS contract (new sheets)
`field.css` (chrome) plus one sheet per input. Parts: `root`, `label`, `control`, `hint`, `error`, `prefix`, `suffix`,
`clear`. Variables read with fallback: `--bob-field-bg`, `--bob-field-border`, `--bob-field-border-invalid`,
`--bob-field-radius`, `--bob-field-height-{sm,md,lg}`. Density via `data-bob-density`. No `display: grid`.

## 10. First deliverables (lane B, in order)
1. `BobFieldBase<T>`, converters, `BobField` chrome, `BobFieldError`, text provider hookup (needs lane D contracts; stubbed until then).
2. `BobTextField`, `BobTextArea`, `BobCheckbox`, `BobSwitch`.
3. `BobNumberField<T>`, `BobRadioGroup<T>`, migrate `SelectField`.
4. `BobForm`, `BobFormSummary`, `BobFormScope`, leave guard.
Each with CSS, contract entry, `[Description]`, a11y doc, bUnit tests, docs steps (the existing "adding a component" checklist).

## 11. Alternatives considered
| Option | Why not |
| --- | --- |
| Inherit `InputBase<T>` | Narrow hooks, no bind timing, no part model, exceptions on parse failure paths |
| `type="number"` natively for numbers | Browser-dependent text/locale behavior, hides raw input, poor error reporting |
| Validation messages as plain strings | Cannot be translated, re-routed or tested by code |
| Throwing on user input errors | Wrong audience, wrong cost; those are expected states |
