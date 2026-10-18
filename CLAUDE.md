# CLAUDE.md

This file provides AI assistant instructions for working with code in this repository.

**For human developers:** See [README.md](README.md) for getting started, commands, and architecture.
**For code examples:** See [docs/EXAMPLES.md](docs/EXAMPLES.md) for detailed code examples and patterns.

---

## Project Organization

There are two libraries. **New work goes in `BobsComponents`.** The older `BobsComponent.Library` is legacy: it is not
prefixed, uses scoped CSS and the `--color-*` variables, and stays until its components are migrated one by one.

| Project | Role |
| --- | --- |
| `src/BobsComponents` | The Razor class library (package id `BobsComponents`, namespace `BobsComponents`). All new reusable components, the error system, the dev kit. |
| `src/BobsComponents.Tests` | bUnit + xUnit tests for `BobsComponents`. Every component and error origin has tests here. |
| `src/BobsComponent.Client` | Demo and docs site (Blazor WebAssembly). New-library pages live in `Pages/Bob/`. Examples never go in the library. |
| `src/BobsComponent.Library` / `.Library.Tests` | Legacy library and its tests. Do not add to it; fix bugs only. |

The public contract (naming, variables, asset paths, error codes) is in [docs/CSS-CONTRACT.md](docs/CSS-CONTRACT.md).
A test (`ContractTests`, `DocsPageTests`) fails the build when code and contract drift apart, so update the doc in the same change.

### Layout of `src/BobsComponents`

```
Components/        UI components (.razor), namespace BobsComponents.Components
Errors/            BobError, classifier, catalog (error-catalog.json), boundary, hub
DevKit/            BobDevKit, network handler, JS runtime logger, DI registry
Infrastructure/    BobComponentBase, BobJs, BobHttp, AddBobComponents
Options/           BobOptions (every default is configurable here)
Services/          IBobToastService
wwwroot/           bob.css, css/tokens.css, css/base.css, css/components/*.css, js/*.js
                   served at _content/BobsComponents/
```

---

## CRITICAL: BobsComponents conventions (new library)

1. **Prefix everything with `bob-`**: every CSS class (`bob-modal`, `bob-modal__header`, `bob-button--primary`),
   every custom property (`--bob-modal-bg`), every part attribute (`data-bob-part="header"`). Root part is `root`.
2. **Plain stylesheets in `@layer bob`.** No scoped `.razor.css` in the library (a test enforces this), no Bootstrap or
   other framework requirement. One sheet per component in `wwwroot/css/components/`, imported by `wwwroot/bob.css`
   into the `bob.components` layer. Do not set `!important` (the one exception is the shared reduced-motion rule).
3. **Component variables are read with a fallback and never declared on the component:**
   `background: var(--bob-modal-bg, var(--bob-color-surface))`. Add each new variable to docs/CSS-CONTRACT.md with its default.
   Global tokens (`--bob-color-*`, `--bob-radius-*`, `--bob-z-*`) are in `css/tokens.css`. Component-specific variables
   are allowed and expected here (this replaces the legacy "no component-specific variables" rule).
4. **Flexbox for layout.** Grid only for true two-dimensional layouts (a test rejects `display: grid` in library CSS for now).
5. **Components inherit `BobComponentBase`.** Validate parameters in `ValidateParameters(BobParameterValidator)`, invoke app
   callbacks with `InvokeCallbackAsync`, get services with `RequireService<T>()`, send fire-and-forget failures to `ReportError`.
   Components take data through parameters or small interfaces, never through an app's own services.
6. **Static assets** are imported through `BobJs` using `./_content/BobsComponents/js/<file>`. Declare services a component
   needs with `[BobRequiresService(typeof(T))]` so the dev kit DI panel can check them.
7. **Accessibility is part of done.** Each component's docs page has an Accessibility section (seeded from BobFit's
   ACCESSIBILITY.md) and each behavior in it has a test.
8. **Adding a component** = component + `wwwroot/css/components/<name>.css` (+ `@import` line in bob.css) + contract doc entry
   + bUnit tests + demo/docs page in `Pages/Bob/` (use `ComponentDocPage`) + nav link in `NavMenu.razor`.
9. Names are public API. Renaming a class, variable, part, parameter or error code is a breaking change: note it in the changelog.

### Error model

Every exception is classified once into a `BobError` (`IBobErrorClassifier`). Six origins: component bug, wrong parameters,
data shape mismatch, missing asset or service, failed network call, your callback threw. **Rules look at exception types,
structured properties and the assembly of the first non-framework stack frame, never at message text.** The card
(`BobErrorCard`, used by `BobErrorBoundary`) always shows the exact message; stack traces and inner exceptions are behind a
Details toggle (`BobOptions.Errors.Details`). New error codes go in `Errors/error-catalog.json` (a test checks every code
constant is in the catalog) and, if raised on purpose, get a `Bob*Exception` factory so the origin is explicit.
Fire-and-forget failures never reach a boundary: report them via `ReportError`/`IBobErrorHub`.

### Dev kit

`<BobDevKit />` is off unless `BobOptions.DevKit.Enabled` is true or the environment is Development
(`AddBobComponents(o => o.EnvironmentName = ...)`). It never records HTTP headers. All defaults live in `BobOptions`.

### Build and test

```powershell
cd src
dotnet test BobsComponents.Tests      # new library: build + all tests
dotnet build BobsComponent.Client     # demo site
dotnet test                           # everything (legacy tests included)
```

`RunAOTCompilation` is only requested when publishing (needs the `wasm-tools` workload); build and test do not need it.
To look at the demo: `dotnet run --project BobsComponent.Client --urls http://localhost:5096`.

---

## Legacy library rules (`BobsComponent.Library` and the existing Client pages)

The sections below describe the legacy library and the Client app. They do not apply to `BobsComponents`
where they conflict with the section above (scoped CSS, `--color-*` variables, Bootstrap-era conventions).

## CRITICAL: Flexbox-First Layout Philosophy

**Flexbox is King** - This application uses flexbox as the primary layout system.

### Layout Rules

1. **ALWAYS use `display: flex`** for layouts
   - ✅ Use flexbox for all container layouts
   - ✅ Use `flex-direction: column` for vertical stacking
   - ✅ Use `flex-direction: row` for horizontal layouts
   - ❌ AVOID `display: grid` unless absolutely necessary for complex 2D layouts
   - ❌ AVOID `display: block`, `inline-block`, or `table` layouts

2. **Flexbox Properties to Master:**
   ```css
   /* Container properties */
   display: flex;
   flex-direction: row | column;
   justify-content: flex-start | center | space-between | space-around;
   align-items: stretch | flex-start | center | flex-end;
   flex-wrap: wrap | nowrap;
   gap: 16px; /* Modern spacing */

   /* Item properties */
   flex: 1; /* Grow to fill space */
   flex-shrink: 0; /* Prevent shrinking */
   align-self: flex-start | center | flex-end;
   ```

3. **Common Flexbox Patterns:**
   ```css
   /* Two-column layout with flex */
   .container {
       display: flex;
       gap: 32px;
   }
   .sidebar { flex: 0 0 300px; } /* Fixed 300px sidebar */
   .main { flex: 1; } /* Main content fills remaining space */

   /* Vertical stack with spacing */
   .stack {
       display: flex;
       flex-direction: column;
       gap: 16px;
   }

   /* Centered content */
   .centered {
       display: flex;
       justify-content: center;
       align-items: center;
   }

   /* Space-between header */
   .header {
       display: flex;
       justify-content: space-between;
       align-items: center;
   }
   ```

4. **When Grid is Acceptable:**
   - Only use `display: grid` for true 2D layouts where both rows AND columns need explicit control
   - Example: Photo galleries, card grids with equal heights
   - Even then, consider if `flex-wrap` could work instead

5. **Migration from Grid to Flex:**
   ```css
   /* ❌ AVOID: Grid for simple two-column */
   .container {
       display: grid;
       grid-template-columns: 400px 1fr;
       gap: 32px;
   }

   /* ✅ PREFER: Flexbox for two-column */
   .container {
       display: flex;
       gap: 32px;
   }
   .sidebar { flex: 0 0 400px; }
   .main { flex: 1; }
   ```

---

## CRITICAL: CSS Organization (legacy library and Client pages)

**Applies to `BobsComponent.Library` and `BobsComponent.Client` only.** `BobsComponents` uses layered plain CSS (see above).

### Scoped CSS is Mandatory

- **ALWAYS use scoped CSS files** (`.razor.css`) for page and component-specific styles
- Scoped CSS automatically applies to that component only and doesn't pollute global namespace
- File naming: `ComponentName.razor.css` or `PageName.razor.css`

### Global vs Scoped CSS

**Global CSS** (`wwwroot/css/*.css`):
- ✅ Theme variables (colors, spacing, z-index)
- ✅ CSS reset files
- ✅ Third-party library CSS (Bootstrap, etc.)
- ❌ NEVER put page-specific or component-specific styles here

**Scoped CSS** (`.razor.css` files):
- ✅ All page-specific styles
- ✅ All component-specific styles
- ✅ Showcase/demo page styles
- ✅ Layout-specific styles

### File Structure Example

```
✅ CORRECT:
src/BobsComponent.Client/Pages/
  ├── MicroActions.razor
  └── MicroActions.razor.css              ← Scoped CSS

src/BobsComponent.Client/wwwroot/css/
  ├── themes.css                          ← Global theme variables
  └── app.css                             ← Global app styles

❌ WRONG:
src/BobsComponent.Client/wwwroot/css/
  └── micro-actions-showcase.css          ← Should be scoped!
```

### index.html

- **NEVER manually add scoped CSS** to index.html
- Blazor automatically injects scoped CSS via `<component>.styles.css`
- Only add global CSS files (themes, resets) to index.html

---

## Development Environment

**IMPORTANT:** This is a Windows development environment.

- **Default to PowerShell or Windows CMD** for all commands
- Use PowerShell syntax for process management, file operations, and system commands
- Avoid using bash-specific syntax (e.g., `for` loops, `$()` substitution)
- For complex operations, prefer PowerShell cmdlets over bash commands

---

## Page Route Management

**CRITICAL:** Always verify no duplicate `@page` routes exist in the codebase.

### Rules

1. **Check for existing routes** - Search for `@page "/your-route"` in all `.razor` files before creating new pages
2. **Use unique routes** - Each page must have a unique route path
3. **Delete old files** - When renaming/moving pages, delete the old file completely

### Verification

```powershell
Get-ChildItem -Path "src" -Filter "*.razor" -Recurse | Select-String -Pattern '@page "' | Select-Object Path, Line
```

**Why:** Duplicate routes cause routing conflicts, build warnings, and runtime failures.

---

## Color System Architecture (legacy library)

**Semantic Color Variables** - The single source of truth for all colors:

```css
--color-primary / --color-primary-hover / --color-primary-text
--color-secondary / --color-secondary-hover / --color-secondary-text
--color-success / --color-success-hover / --color-success-text
--color-danger / --color-danger-hover / --color-danger-text
--color-warning / --color-warning-hover / --color-warning-text
--color-info / --color-info-hover / --color-info-text
```

**Z-Index Layer System:**

```css
--z-sticky: 100      /* Sticky headers */
--z-backdrop: 999    /* Modal/dropdown backdrops */
--z-dropdown: 1000   /* Dropdowns */
--z-popover: 1500    /* Popovers */
--z-modal: 2000      /* Modals */
--z-tooltip: 3000    /* Tooltips */
```

### RULES

1. ✅ ALWAYS use `--color-*` variables, NEVER hardcode colors
2. ✅ ALWAYS use `--z-*` variables, NEVER hardcode z-index
3. ❌ NEVER create component-specific color variables (e.g., `--btn-*`, `--async-*`)
4. ❌ Buttons don't need z-index (only dropdowns/modals/tooltips)

**Example:**

```css
/* ✅ CORRECT */
.my-button {
    background: var(--color-primary);
    color: var(--color-primary-text);
}

/* ❌ WRONG - Hardcoded */
.my-button {
    background: #0d6efd;
    color: white;
    z-index: 1000;
}
```

For more CSS examples, see [docs/EXAMPLES.md - CSS Organization](docs/EXAMPLES.md#css-organization-examples).

---

## Development Policy: Never Duplicate Work

**BEFORE creating new CSS variables, utilities, or patterns:**

1. **Search first:**
   - Check `themes.css` for existing color variables
   - Check existing CSS files for similar utilities
   - Grep codebase for similar patterns

2. **Stop and ask:**
   - If unsure whether something exists, ASK
   - If creating something that seems standard, ASK
   - Better to ask than create duplicates

3. **Why it matters:**
   - Duplicate systems = maintenance nightmare
   - White-label theming breaks if colors scattered
   - Wastes developer time finding "right" variable

---

## Hot Reload Limitations

**CRITICAL:** Some code changes cannot be hot-reloaded and require a manual restart (Ctrl+C then `dotnet watch run`).

### Changes Requiring Restart

| Change Type | Why |
|-------------|-----|
| Component References (`@ref`) | Hot reload can't update reference infrastructure |
| Component Parameters (`[Parameter]`) | Parameter binding is compiled, not runtime |
| Public API changes | Component metadata changes require recompilation |
| Render Fragment changes | Render tree compilation issues |

### Safe for Hot Reload

- CSS changes (always safe)
- Markup changes in `.razor` files (usually safe)
- Private method implementations (safe)
- Logic inside existing methods (safe)

### Prevention Strategy

- Run `dotnet test` BEFORE starting `dotnet watch`
- Make incremental changes and test frequently
- For large refactors, stop hot reload and do a full rebuild

For detailed scenarios and examples, see [docs/EXAMPLES.md - Hot Reload Scenarios](docs/EXAMPLES.md#hot-reload-scenarios).

---

## Security Best Practices

This project follows security-first development principles. All code must adhere to these security guidelines.

For detailed examples and code patterns, see [docs/EXAMPLES.md - Security Examples](docs/EXAMPLES.md#security-examples).

### XSS (Cross-Site Scripting) Protection

**JavaScript Interop:**
- ❌ NEVER use `eval()` for DOM manipulation or JavaScript execution
- ✅ ALWAYS create dedicated JavaScript functions for specific tasks
- ✅ Sanitize all input before passing to JavaScript

**Blazor Rendering:**
- ✅ Use `@variable` syntax - Blazor automatically HTML-encodes strings
- ❌ NEVER use `@((MarkupString)userInput)` with unsanitized user input
- ✅ RenderFragment is safe - framework handles sanitization

**CSS Injection Protection:**
- ✅ Sanitize CSS values when building inline styles from parameters
- ✅ Use regex to allow only safe characters (alphanumeric, %, -, ., units)
- See `SkeletonLoader.razor` for sanitization example

### Information Disclosure Prevention

**Error Handling:**
- ❌ NEVER expose `exception.Message` or stack traces to clients in production
- ✅ Log errors server-side with full details
- ✅ Return generic error messages to clients
- ✅ For demo/test services (like `MockApiService`), clearly document that they expose errors for educational purposes

### Input Validation

**Component Parameters:**
- ✅ Validate all user-controllable parameters
- ✅ Use enums for predefined options (e.g., `ButtonStyle`, `ThemeInfo`)
- ✅ Sanitize string inputs used in styles, attributes, or JavaScript
- ❌ NEVER trust user input, even from other components

**Theme Selection:**
- ✅ Only allow selection from predefined theme list
- ✅ JavaScript sanitizes theme values (alphanumeric, hyphens, underscores only)
- ❌ NEVER accept arbitrary theme names from user

### Secrets Management

**Configuration:**
- ❌ NEVER commit API keys, passwords, or credentials to the repository
- ✅ Use environment variables or Azure Key Vault for secrets
- ✅ Add `.env` files to `.gitignore`
- ✅ Use user secrets for local development: `dotnet user-secrets`

---

## Refactoring Policy

**IMPORTANT:** Do NOT rename existing files, functions, classes, or components without explicit user approval. (`BobsComponents` is pre-1.0 and its names are still flexible during the contract work, but once a name is documented in docs/CSS-CONTRACT.md it follows rule 9 above.)

### Rules for Renaming

1. **Never rename existing code elements** unless:
   - The user explicitly requests the rename
   - You ask first and receive approval
   - The element is brand new (created in the current session)

2. **Always ask before refactoring** that involves:
   - Renaming files, functions/methods, classes/interfaces
   - Renaming properties or parameters
   - Moving code between files

3. **What you CAN do without asking:**
   - Create new files, functions, or classes
   - Fix bugs in existing code (keeping the same names)
   - Add new functionality to existing files
   - Update implementation details without changing signatures

**Why:** Breaking changes affect other developers, existing code may depend on current names, and git history becomes harder to track.

---

## Git Workflow

For detailed git examples and commands, see [docs/EXAMPLES.md - Git Workflow Examples](docs/EXAMPLES.md#git-workflow-examples).

### Syncing with Master Branch

**ALWAYS** sync with master before creating or pushing to the remote repository. All conflicts must be resolved locally before any push occurs.

**Checklist:**
1. ✅ `git pull origin master`
2. ✅ `git checkout your-branch`
3. ✅ `git merge master`
4. ✅ Resolve conflicts (if any) - may require manual input
5. ✅ Only after conflicts are resolved, push to remote

### Pre-Push Testing

**CRITICAL:** Before pushing to remote as the FINAL action, ALWAYS verify your changes:

**Checklist:**
1. ✅ Run `dotnet test` - ensure all tests pass
2. ✅ Run `dotnet watch run` - verify application starts without errors
3. ✅ Test affected functionality manually
4. ✅ Check for console errors or warnings
5. ✅ Ask user to verify changes (for now, automation coming later)

**Remember:** Never push broken code to the remote repository. Local testing is mandatory before every push.

For common commands, see [README.md - Common Commands](README.md#common-commands).

### CI/CD Pipeline Changes

**CRITICAL:** When modifying GitHub Actions workflows or CI/CD configuration, ALWAYS verify changes locally before pushing.

**Checklist:**
1. ✅ Validate YAML syntax with `yamllint .github/workflows/deploy.yml`
2. ✅ Test build commands locally in the same sequence
3. ✅ Verify command flags are valid (`dotnet --help`)
4. ✅ Test conditional logic (branch-based, environment variables)
5. ✅ Make ONE change at a time - never batch untested workflow changes

**Common mistakes to avoid:**
- ❌ Invalid flags like `--parallel` on `dotnet restore`
- ❌ Using MSBuild syntax incorrectly (`-maxcpucount` should be `-m`)
- ❌ Mixing `--no-build` with different project contexts

**Why:** CI/CD failures waste GitHub Actions minutes, delay deployments, and can block the entire team. A single untested workflow change can cause 5-10 failed builds.

### Base Path and Routing Configuration

**CRITICAL:** Never change the application's base path or routing configuration after it has been set unless you refactor it EVERYWHERE in the application and build tests to prove it is working.

**Rules:**
1. ✅ Base href must match deployment path (`<base href="/BobsComponents/" />` for GitHub Pages at `/BobsComponents/`)
2. ✅ Base href changes MUST be tested in CI/CD before merging
3. ✅ ALL static asset references must respect the base path
4. ❌ NEVER change base path without comprehensive testing
5. ❌ NEVER mix absolute and relative paths inconsistently

**Why:** Base path changes break ALL resource loading (CSS, JS, _framework files). A single mistake causes complete deployment failure with 404 errors.

For CI/CD examples and build commands, see [docs/EXAMPLES.md - CI/CD Configuration](docs/EXAMPLES.md#cicd-configuration-examples).

### Commit Guidelines

**Checklist:**
- ✅ Review `git status` output carefully before committing
- ✅ Address any warnings about uncommitted changes
- ✅ Stage only files related to the current work/feature
- ✅ Ensure commit messages are clear and descriptive
- ✅ Keep commits focused on a single logical change

---

## Quick Reference Links

**For Commands:** [README.md - Common Commands](README.md#common-commands)
**For Examples:** [docs/EXAMPLES.md](docs/EXAMPLES.md)
**For Project Info:** [README.md - Project Overview](README.md#project-overview)
**For Security Examples:** [docs/EXAMPLES.md - Security Examples](docs/EXAMPLES.md#security-examples)
**For Git Examples:** [docs/EXAMPLES.md - Git Workflow Examples](docs/EXAMPLES.md#git-workflow-examples)
