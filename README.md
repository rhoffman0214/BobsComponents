# BobsComponents

A Blazor component library with a stable, documented contract: every CSS class, custom property and part attribute starts
with `bob-`, the CSS is plain and lives in `@layer bob`, and every failure becomes an error card that shows the exact cause.
The docs site is generated from the code, so parameters, CSS and the DOM map cannot drift from what ships.

- **Package:** `src/BobsComponents` (id and namespace `BobsComponents`). .NET 10, Blazor WebAssembly, no CSS framework needed.
- **Docs and demos:** run the site (below) and open `/bob`.
- **Contract:** [docs/CSS-CONTRACT.md](docs/CSS-CONTRACT.md) lists every public name. [docs/CHANGELOG.md](docs/CHANGELOG.md) lists breaking changes first.

---

## Use it

```csharp
// Program.cs
builder.Services.AddBobComponents(o => o.EnvironmentName = builder.HostEnvironment.Environment);
```

```html
<!-- index.html (or App.razor <HeadContent>) -->
<link rel="stylesheet" href="_content/BobsComponents/bob.css" />
```

```razor
@using BobsComponents.Components

<BobErrorBoundary>
    <ProgressBar Label="Upload" Value="42" />
    <SelectField Label="Size" Options="_sizes" @bind-Value="_size" Placeholder="Choose a size" />
</BobErrorBoundary>
<BobDevKit />   @* shown only in Development, or when BobOptions.DevKit.Enabled is true *@
```

Restyle by setting custom properties on any ancestor; components read them with a fallback and never declare them:

```css
.checkout { --bob-progress-fill: var(--bob-color-success); --bob-select-radius: 0; }
```

## Run the demo and docs

```powershell
cd src
dotnet run --project BobsComponent.Client --urls http://localhost:5096   # then open /bob
```

## Test

```powershell
cd src
dotnet test BobsComponents.Tests      # new library + docs engine
dotnet build BobsComponent.Client     # demo and docs site
dotnet test                           # everything, legacy tests included
```

Tests fail the build when code, docs and contract drift apart (`ContractTests`, `DocsDriftTests`, `ApiSurfaceTests`).

---

## What is in it

| Area | Components and tools |
| --- | --- |
| Loading and progress | `LoadingSpinner`, `FullScreenLoader`, `ProgressBar`, `SkeletonLoader`, `AsyncButton` |
| Dialogs and feedback | `ModalSheet`, `ConfirmDialog`, `Toasts` (`IBobToastService`), `NotificationBell` |
| Form inputs | `SelectField`, `RestPicker`, `SegmentPicker`, `DraftSaver` |
| Data display | `ProgressChart`, `ActivityHeatmap`, `ElapsedClock` |
| Errors | `BobErrorBoundary`, `BobErrorCard`, `IBobErrorHub`; six origins, one card, [error catalog](src/BobsComponents/Errors/error-catalog.json) |
| Dev kit | `<BobDevKit />`: errors, network, interop, DI and storage panels, plus Copy report |

Each component has a generated page with a live demo, parameter tables, errors it can raise, accessibility notes with the
tests that prove them, the CSS reference and a DOM map with a part inspector. There is one search (`/bob/search`).

## Docs map

| You want to | Read |
| --- | --- |
| Use a component | the page at `/bob/<slug>` (demo, API, errors, accessibility, then CSS) |
| Know what is safe to rely on | [docs/CSS-CONTRACT.md](docs/CSS-CONTRACT.md) |
| See what changed | [docs/CHANGELOG.md](docs/CHANGELOG.md) (`/bob/changelog`) |
| Understand an error code | `/bob/error-catalog` |
| Copy a pattern | [docs/EXAMPLES.md](docs/EXAMPLES.md) |
| Test a deployment | [docs/DEPLOYMENT-TESTING.md](docs/DEPLOYMENT-TESTING.md) |
| Work as an AI assistant | [CLAUDE.md](CLAUDE.md) |

---

## Contribute a component

1. Build the component: `@attribute [System.ComponentModel.Description("one sentence")]`, `/// <summary>` on every
   `[Parameter]`, a `data-bob-part` on each styleable element, inheriting `BobComponentBase`.
2. Add its sheet in `wwwroot/css/components/<name>.css` and one `@import` line in `wwwroot/bob.css`.
3. Add its classes, parts and variables (with defaults) to [docs/CSS-CONTRACT.md](docs/CSS-CONTRACT.md).
4. Add bUnit tests, and `src/BobsComponents.Docs/Demos/<Name>Demo.razor` (a real, compiled demo; the code shown is this file).
5. Add `src/BobsComponents.Docs/Content/accessibility/<slug>.md`: one bullet per behavior, each ending with
   `[test: ClassName.Method]` naming a real test, or starting with `gap:` for a known gap.
6. Add one entry to `DocRegistry.Components` (slug, title, type, stylesheet, demos).
7. Add an Added line to `docs/CHANGELOG.md` and refresh the snapshot:
   `$env:BOB_UPDATE_SURFACE=1; dotnet test BobsComponents.Tests --filter ApiSurface`.

The nav menu, search and error catalog pick the component up from the registry. Names are public API: renaming one is a
breaking change and goes under `### Breaking` in the newest changelog release.

### Git

Sync with master before any push, run `dotnet test`, check the demo in a browser, then push. See [CLAUDE.md](CLAUDE.md) for the full rules.

---

## Legacy library and demo app

`src/BobsComponent.Library` is the original unprefixed library (scoped CSS, `--color-*` variables, Bootstrap-era). It stays
until its components are migrated one by one; do not add to it, fix bugs only. `src/BobsComponent.Client` is the Blazor
WebAssembly app that hosts the legacy showcase pages and the `/bob` docs site.

```powershell
cd src/BobsComponent.Client
dotnet watch run     # http://localhost:5096, https://localhost:7017
```

- Requires the .NET 10 SDK; publishing with AOT needs `dotnet workload install wasm-tools`. Build and test do not.
- Legacy CSS rules (scoped `.razor.css`, `--color-*` and `--z-*` variables, flexbox first) are in [CLAUDE.md](CLAUDE.md).
- Optional dev container: open the workspace in VS Code and choose "Reopen in Container" (.NET 10, Node LTS).
- Some changes cannot hot reload (new `[Parameter]`s, `@ref`, render fragments); restart `dotnet watch run` after those.

**Tech stack:** .NET 10, C# 14, Blazor WebAssembly, bUnit + xUnit.
