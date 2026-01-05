# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

BobsBlazingComponents is a Blazor WebAssembly component library project. The goals are to:
- Create reusable, themeable components that are open to user customization while providing base themes and functionality
- Provide "amazing" examples of real-world usage, especially combining popular packages
- Demonstrate practical integration patterns that developers can learn from and adapt

## Solution Structure

The solution uses a two-project architecture:

- **BobsComponent.Library**: Razor component library (SDK: Microsoft.NET.Sdk.Razor) that houses reusable Blazor components
- **BobsComponent.Client**: Blazor WebAssembly client application for testing/showcasing the component library

Both projects reference BobsComponent.Library - the Client uses both a PackageReference and a ProjectReference to the Library.

## Application Sections

The client application is organized into three distinct sections:

1. **Documentation Area**: Comprehensive documentation for components, APIs, and usage patterns
2. **Sandbox**: Interactive playground for experimenting with components in real-time
3. **Learning Section**: Structured progression-based learning path for users to master the component library

### User Sessions

The application includes user session management:
- Email-based validation (no password authentication)
- No PII (Personally Identifiable Information) stored
- No security-sensitive data - session tracking is purely for progression data
- Focus on safe, data-only user experience tracking

## Build Configuration

The solution uses a centralized build configuration via `src/Directory.Build.props`:
- Target framework: net10.0 (configurable via `$(NetVersion)`)
- AOT compilation is enabled (`RunAOTCompilation: true`)

## .NET Version

This project targets **.NET 10.0** (LTS release, supported until November 2028) with **C# 14** language features.

### Required Workloads

For Blazor WebAssembly development, install the wasm-tools workload:
```bash
dotnet workload install wasm-tools
```

### C# 14 Features in Use

This codebase demonstrates modern C# 14 patterns:
- **Collection expressions**: `[]` syntax for cleaner collection initialization
- **Field-backed properties**: Using `field` keyword in custom accessors (planned for future components)

When contributing, prefer these modern patterns over legacy syntax.

## Common Commands

### Running the Application
```bash
cd src/BobsComponent.Client
dotnet watch run
```

The application will be available at:
- HTTPS: https://localhost:7017
- HTTP: http://localhost:5096

### Building
```bash
cd src
dotnet build BobsComponents.sln
```

### Build Configurations
```bash
# Debug build (default)
dotnet build

# Release build with AOT
dotnet build -c Release
```

### Running Tests

**IMPORTANT:** Always run tests BEFORE starting `dotnet watch` to catch issues that would cause hot reload errors.

```bash
# Run all tests
cd src
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests for specific project
cd src/BobsComponent.Library.Tests
dotnet test

# Run tests in watch mode (auto-run on file changes)
dotnet watch test
```

**Test Structure:**
- `BobsComponent.Library.Tests` - xUnit + bUnit tests for component library
- Tests are organized by component in `Components/` folder
- Example: `Components/AsyncButtonTests.cs` tests the AsyncButton component

**Writing Component Tests:**
```csharp
using Bunit;
using BobsComponent.Library.Components;
using Xunit;

public class MyComponentTests : TestContext
{
    [Fact]
    public void MyComponent_Renders_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<MyComponent>();

        // Assert
        Assert.NotNull(cut.Find("button"));
    }
}
```

**Test-Driven Development Workflow:**
1. Write unit tests for new component functionality
2. Run tests to verify they fail (`dotnet test`)
3. Implement the component feature
4. Run tests again to verify they pass
5. ONLY THEN start `dotnet watch run` for manual testing

This prevents hot reload errors by catching issues before they reach the runtime.

### Hot Reload Limitations

**CRITICAL:** Some code changes cannot be hot-reloaded and require a manual restart to prevent Mono runtime errors.

**Changes that REQUIRE manual restart (Ctrl+C then `dotnet watch run`):**

1. **Component References (`@ref`)**
   - Adding/removing `@ref` attributes on components
   - Changing component reference types
   - **Why:** Hot reload can't update the component reference infrastructure

2. **Component Parameters**
   - Adding/removing `[Parameter]` attributes
   - Changing parameter types
   - **Why:** Parameter binding is compiled, not runtime

3. **Public API Changes**
   - Adding new public methods to components (sometimes works, sometimes fails)
   - Changing method signatures
   - **Why:** Component metadata changes require recompilation

4. **Render Fragment Changes**
   - Significant changes to `RenderFragment` or `ChildContent`
   - **Why:** Render tree compilation issues

**Safe for Hot Reload:**
- CSS changes (always safe)
- Markup changes in `.razor` files (usually safe)
- Private method implementations (safe)
- Logic inside existing methods (safe)

**When you see Mono assertion errors:**
```
[MONO] * Assertion at hot_reload.c:835, condition '<disabled>' not met
```

**Action:**
1. Stop the app (Ctrl+C)
2. Restart with `dotnet watch run`
3. Document the change type that caused it
4. Consider adding unit tests to catch issues before hot reload

**Prevention Strategy:**
- Run unit tests BEFORE starting `dotnet watch`
- Make incremental changes and test frequently
- For large refactors, stop hot reload and do a full rebuild

## Development Environment

The project includes a VS Code dev container configuration (.devcontainer):
- Base image: .NET 10.0 with Node.js LTS
- Opens workspace as a remote container in Docker
- Includes C# extension (ms-dotnettools.csharp)

To use the dev container, open the workspace in VS Code and select "Reopen in Container".

## Architecture Notes

### Component Library Structure

The Library project (BobsComponent.Library) currently has minimal components:
- Uses `_Imports.razor` for shared namespace imports (@using Microsoft.AspNetCore.Components.Web)
- Contains a `wwwroot` folder with:
  - `background.png`: Background image asset
  - `exampleJsInterop.js`: JavaScript interop example

### Client Application Structure

The Client project follows standard Blazor WebAssembly patterns:
- `Program.cs`: Entry point with service registration and HttpClient configuration
- `App.razor`: Router configuration with MainLayout as default layout
- Pages:
  - `Index.razor`: Home page that renders the MicroActions component
  - `MicroActions.razor`: Example component demonstrating component lifecycle (OnInitialized)
- Static assets in `wwwroot`:
  - CSS: Bootstrap, Open Iconic icons, custom app styles, jc-reset.css
  - Favicon and PWA icons

### Theming Approach

The project emphasizes user-toggleable theming. Current CSS structure includes:
- `app.css`: Main application styles
- `jc-reset.css`: CSS reset
- Bootstrap integration
- Open Iconic icon library

When adding new components, consider how they will support theming and customization.

### Color System Architecture

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

**RULES:**
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

### Development Policy: Never Duplicate Work

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

**Example:**
- ❌ DON'T: Create `--async-primary` when `--color-primary` exists
- ✅ DO: Ask "I need button colors - should I use existing variables?"

## Import Structure

The Client project's `_Imports.razor` establishes global namespace access:
- Standard Blazor namespaces (Components.Forms, Components.Routing, etc.)
- BobsComponent.Client and BobsComponent.Client.Shared
- **BobsComponent.Library** (making library components available throughout the client)

New components in the Library will be automatically available in Client pages without additional imports.

## Component Development Philosophy

When creating new components for this library:
- **Real-world examples**: Components should showcase practical, production-ready usage patterns
- **Package integration**: Demonstrate how to combine popular NuGet packages effectively (e.g., FluentValidation, AutoMapper, popular charting libraries, state management solutions)
- **Educational value**: Code should serve as a learning resource, showing best practices and integration patterns
- **Practical over trivial**: Focus on components that solve real problems developers face, not just basic examples

## Security Best Practices

This project follows security-first development principles. All code must adhere to these security guidelines.

### XSS (Cross-Site Scripting) Protection

**JavaScript Interop:**
- ✅ NEVER use `eval()` for DOM manipulation or JavaScript execution
- ✅ ALWAYS create dedicated JavaScript functions for specific tasks
- ✅ Sanitize all input before passing to JavaScript

**Example - SECURE JavaScript Interop:**
```javascript
// wwwroot/theme.js
window.BobsComponents = window.BobsComponents || {};
window.BobsComponents.setTheme = function(theme) {
    // Sanitize input
    const sanitized = theme.replace(/[^a-zA-Z0-9\-_]/g, '');
    document.documentElement.setAttribute('data-theme', sanitized);
};
```

```csharp
// ThemeService.cs
await _jsRuntime.InvokeVoidAsync("BobsComponents.setTheme", theme);
```

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

**Example - SECURE Error Handling:**
```csharp
catch (Exception ex)
{
    // Log server-side (production)
    _logger.LogError(ex, "Failed to process request");

    // Return generic message to client
    return AsyncOperationResult.CreateError("An error occurred. Please try again.");
}
```

**Example - DEMO Service (acceptable):**
```csharp
/// <remarks>
/// WARNING: This service exposes exception details for demo/testing purposes.
/// In production services, NEVER expose exception.Message to clients.
/// </remarks>
public class MockApiService
{
    catch (Exception ex)
    {
        // OK for demos - clearly documented as test/demo code
        return AsyncOperationResult.CreateError($"API Error: {ex.Message}");
    }
}
```

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

**Code Review Checklist:**
```bash
# Scan for potential secrets before committing
grep -r "password\|api[_-]?key\|secret\|token" --include="*.cs" --include="*.json"
```

### Security Audit Checklist

Before releasing new features, verify:

1. **XSS Protection:**
   - [ ] No `eval()` usage in JavaScript
   - [ ] No `MarkupString` with unsanitized input
   - [ ] All JavaScript interop uses dedicated functions

2. **Information Disclosure:**
   - [ ] No exception details exposed to clients (production code)
   - [ ] Error messages are generic
   - [ ] No sensitive data in logs sent to client

3. **Input Validation:**
   - [ ] All user inputs validated/sanitized
   - [ ] CSS values sanitized if building inline styles
   - [ ] Enum values used for predefined options

4. **Secrets:**
   - [ ] No hardcoded credentials
   - [ ] No API keys in code or config files
   - [ ] Sensitive config in environment variables/user secrets

5. **Dependencies:**
   - [ ] NuGet packages up to date
   - [ ] No known vulnerabilities in dependencies
   - [ ] Run `dotnet list package --vulnerable`

## Refactoring Policy

**IMPORTANT:** Do NOT rename existing files, functions, classes, or components without explicit user approval.

### Rules for Renaming

1. **Never rename existing code elements** unless:
   - The user explicitly requests the rename
   - You ask first and receive approval
   - The element is brand new (created in the current session)

2. **Always ask before refactoring** that involves:
   - Renaming files (e.g., `Component.razor` → `BetterComponent.razor`)
   - Renaming functions/methods (e.g., `GetData()` → `FetchData()`)
   - Renaming classes/interfaces (e.g., `DataService` → `ApiService`)
   - Renaming properties or parameters
   - Moving code between files

3. **Why this matters:**
   - Breaking changes affect other developers
   - Existing code may depend on current names
   - Git history becomes harder to track
   - Refactoring should be intentional, not incidental

4. **What you CAN do without asking:**
   - Create new files, functions, or classes
   - Fix bugs in existing code (keeping the same names)
   - Add new functionality to existing files
   - Update implementation details without changing signatures

**Example:**
- ❌ DON'T: Rename `MicroActions.razor` to `UserActions.razor` while fixing a bug
- ✅ DO: Ask "I noticed MicroActions.razor might be better named UserActions.razor. Would you like me to rename it?"
- ✅ DO: Create a new component called `UserActions.razor` if that's what the task requires

## Git Workflow

### Syncing with Master Branch

**ALWAYS** sync with master before creating or pushing to the remote repository. All conflicts must be resolved locally before any push occurs.

**Required Steps:**

1. **Pull latest master:**
   ```bash
   git pull origin master
   ```

2. **Switch to your branch:**
   ```bash
   git checkout branchNameHere
   ```

3. **Merge master into your branch:**
   ```bash
   git merge master
   ```

4. **Resolve conflicts (if any):**
   - May require manual input to resolve conflicting files
   - Open conflicts in your preferred editor
   - Look for conflict markers (`<<<<<<<`, `=======`, `>>>>>>>`)
   - Choose the correct code and remove conflict markers
   - Stage resolved files with `git add <file>`
   - Complete the merge with `git commit`

5. **Only after conflicts are resolved**, push to remote

### Pre-Push Testing

**CRITICAL:** Before pushing to remote as the FINAL action, ALWAYS verify your changes:

1. **Run the application locally for testing:**
   ```bash
   cd src/BobsComponent.Client
   dotnet watch run
   ```
   - Verify application starts without errors
   - Test affected functionality manually
   - Check for console errors or warnings

2. **Run tests for the application:**
   ```bash
   dotnet test
   ```
   - Ensure all tests pass
   - Fix any failing tests before pushing

3. **Manual verification:**
   - For now, ask the user to verify changes manually
   - Automation of this verification will be implemented in the future
   - Document what was tested in commit messages or PR description

**Remember:** Never push broken code to the remote repository. Local testing is mandatory before every push.

### CI/CD Pipeline Changes

**CRITICAL:** When modifying GitHub Actions workflows or CI/CD configuration, ALWAYS verify changes locally before pushing:

1. **Validate YAML syntax:**
   ```bash
   # Install yamllint if not already installed
   pip install yamllint

   # Validate workflow syntax
   yamllint .github/workflows/deploy.yml
   ```

2. **Test build commands locally:**
   - Extract the exact commands from the workflow file
   - Run them in the same sequence locally
   - Example from current workflow:
   ```bash
   dotnet workload install wasm-tools
   dotnet restore src/BobsComponents.sln
   dotnet build src/BobsComponents.sln --configuration Release --no-restore -m
   dotnet test src/BobsComponents.sln --configuration Release --verbosity normal
   dotnet publish src/BobsComponent.Client/BobsComponent.Client.csproj --configuration Release --output publish
   ```

3. **Verify command flags are valid:**
   - Check `dotnet --help` for valid flags before using them
   - Test unfamiliar flags locally first (e.g., `--parallel`, `-maxcpucount` vs `-m`)
   - Common mistakes to avoid:
     - Using invalid flags like `--parallel` on `dotnet restore`
     - Using MSBuild syntax incorrectly (`-maxcpucount` should be `-m`)
     - Mixing `--no-build` with different project contexts

4. **Test conditional logic:**
   - If using branch-based conditionals (e.g., AOT on/off), test both paths
   - Verify environment variables and GitHub expressions are correct
   - Test shell script syntax if using multiline `run:` blocks

5. **Incremental testing approach:**
   - Make ONE change at a time to workflows
   - Push and verify it works before adding more changes
   - If multiple changes are needed, create a feature branch to test them together first
   - Never batch multiple untested workflow changes into a single commit

**Why this matters:** CI/CD failures waste GitHub Actions minutes, delay deployments, and can block the entire team. A single untested workflow change can cause 5-10 failed builds before being fixed.

### Commit Guidelines

**IMPORTANT:** Always fix git commit warnings before proceeding with commits or pull requests.

When working with git:
- Review `git status` output carefully before committing
- Address any warnings about uncommitted changes
- Stage only files related to the current work/feature
- Ensure commit messages are clear and descriptive
- Keep commits focused on a single logical change
