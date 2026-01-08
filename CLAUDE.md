# CLAUDE.md

AI assistant instructions for working with this Blazor component library.

**For human developers:** See [README.md](README.md)
**For code examples:** See [docs/EXAMPLES.md](docs/EXAMPLES.md)

---

## Project Organization

- **Library** (`BobsComponent.Library`) - Reusable components and documentation
- **Client** (`BobsComponent.Client`) - Examples, showcases, demonstrations
- Windows development environment - use PowerShell/CMD for all commands

---

## CRITICAL: Flexbox-First Layout Philosophy

**Flexbox is King** - Primary layout system.

**Rules:**
- ✅ ALWAYS use `display: flex` for layouts (row/column)
- ❌ AVOID `display: grid`, `block`, `inline-block`, `table` unless necessary

**Essential Properties:**
- Container: `display: flex; flex-direction: row|column; justify-content; align-items; gap: 16px;`
- Items: `flex: 1;` (grow), `flex-shrink: 0;` (no shrink), `align-self;`

**Patterns:** Two-column: `flex: 0 0 300px;` (sidebar) + `flex: 1;` (main) | Stack: `flex-direction: column; gap: 16px;` | Center: `justify-content: center; align-items: center;`

---

## Bob Standards: No Horizontal Scrolling

**CRITICAL:** Pages must NEVER scroll horizontally by default.

**Prevention:**
- Global: `html, body { overflow-x: hidden; max-width: 100vw; }`
- Containers: `display: flex; flex-direction: column; width: 100%; max-width: 100vw; box-sizing: border-box;`

**Fixed/Sticky Elements:**
- ✅ `max-width: calc(100vw - 40px)` for fixed elements
- ❌ NEVER use fixed widths exceeding viewport

**Content:** `min-width: 0` on flex children, `overflow-wrap: break-word` for text, collapse to hamburger on mobile, stack vertically on small screens

---

## CSS Organization for Component Library

**Scoped CSS is Mandatory** - Use `.razor.css` files for all component/page-specific styles.

**Global CSS** (`wwwroot/css/*.css`):
- ✅ Theme variables, CSS resets, third-party libraries
- ❌ NEVER put component-specific styles here

**Scoped CSS** (`.razor.css` files):
- ✅ All page/component-specific styles
- Blazor auto-injects as `<component>.styles.css`

---

## Color System Architecture

**Semantic Variables** - Single source of truth:
```css
--color-primary / --color-primary-hover / --color-primary-text
--color-secondary / --color-secondary-hover / --color-secondary-text
--color-success / --color-danger / --color-warning / --color-info

--z-sticky: 100     /* Sticky headers */
--z-dropdown: 1000  /* Dropdowns */
--z-modal: 2000     /* Modals */
--z-tooltip: 3000   /* Tooltips */
```

**Rules:**
1. ✅ ALWAYS use `--color-*` variables, NEVER hardcode colors
2. ✅ ALWAYS use `--z-*` variables, NEVER hardcode z-index
3. ❌ NEVER create component-specific color variables
4. ✅ Search `themes.css` before creating new variables

See [docs/EXAMPLES.md](docs/EXAMPLES.md#css-organization-examples) for examples.

---

## Test-First Development & PR Guidelines

**Test-First (Hard Rule):** NEVER change existing functions without tests FIRST
- Workflow: Test current behavior (pass) → Test new behavior (fail) → Update function → All tests pass → Commit
- Applies to: Public methods, business logic, utilities, complex event handlers
- Skip for: Simple getters/setters, CSS, brand new functions

**PR Size:** Target ~250 lines. Justify if >250 lines. Avoid 1000+ lines unless auto-generated.

**File Deletion:** 3-6 month deprecation period. Mark: `// DEPRECATED (2026-01): Use NewFile.razor`. Requires approval. Can delete temp files/build artifacts.

---

## Hot Reload Limitations

**Changes Requiring Restart:**
- Component Parameters (`[Parameter]`)
- Component References (`@ref`)
- Public API changes
- Render Fragment changes

**Safe for Hot Reload:**
- CSS changes (always safe)
- Markup changes in `.razor` files (usually safe)
- Private method implementations

**Strategy:** Run `dotnet test` before `dotnet watch`, make incremental changes

See [docs/EXAMPLES.md](docs/EXAMPLES.md#hot-reload-scenarios) for scenarios.

---

## Security Best Practices

**XSS:** ❌ NEVER `eval()` or `@((MarkupString)userInput)` ✅ Use `@variable` (auto HTML-encoded), sanitize CSS, dedicated JS functions
**Input:** ✅ Enums for options, sanitize strings ❌ NEVER trust user input
**Secrets:** ❌ NEVER commit keys/passwords ✅ Environment variables, Azure Key Vault, `dotnet user-secrets`

See [docs/EXAMPLES.md](docs/EXAMPLES.md#security-examples).

---

## Refactoring Policy

**NEVER rename existing files, functions, or classes without explicit user approval.**

**Ask First For:**
- Renaming files, functions, classes, properties, parameters
- Moving code between files

**Can Do Without Asking:**
- Create new files/functions/classes
- Fix bugs (keeping same names)
- Add new functionality
- Update implementation details

---

## Git Workflow

**Pre-Push (CRITICAL):** ✅ `dotnet test` pass ✅ `dotnet watch run` works ✅ Manual test ✅ No console errors ✅ User verify

**CI/CD:** Validate YAML, test build commands locally, ONE change at a time

**Base Path:** NEVER change `<base href>` without comprehensive testing. Must match deployment (`/BobsComponents/`). Breaking change = deployment failure.

**Commits:** Review `git status`, stage related files only, single logical change

See [docs/EXAMPLES.md](docs/EXAMPLES.md#git-workflow-examples).

---

## C# Blazor Best Practices

**Popular Patterns to Follow:**
1. **Scoped CSS** - `.razor.css` for component isolation
2. **Parameter Binding** - `[Parameter]`, `[CascadingParameter]`
3. **EventCallback** - Component event pattern
4. **RenderFragment** - Custom content slots
5. **Dependency Injection** - `[Inject]`
6. **Code-Behind** - `.razor` + `.razor.cs` separation
7. **Lifecycle Methods** - `OnInitialized`, `OnAfterRender`
8. **CSS Isolation** - `::deep` for child components
9. **Data Binding** - `@bind`, `@bind:event`
10. **Component References** - `@ref`

---

## Quick Reference Links

**Commands:** [README.md - Common Commands](README.md#common-commands)
**Examples:** [docs/EXAMPLES.md](docs/EXAMPLES.md)
**Security:** [docs/EXAMPLES.md - Security](docs/EXAMPLES.md#security-examples)
**Git:** [docs/EXAMPLES.md - Git Workflow](docs/EXAMPLES.md#git-workflow-examples)
