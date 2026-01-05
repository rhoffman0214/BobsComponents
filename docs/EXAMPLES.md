# Code Examples & Patterns

This document provides detailed code examples referenced by [CLAUDE.md](../CLAUDE.md).
For rules and policies, see the main [CLAUDE.md](../CLAUDE.md) file.

**Navigation:** [README](../README.md) | [CLAUDE.md](../CLAUDE.md)

---

## Table of Contents

1. [Testing Patterns](#testing-patterns)
2. [Security Examples](#security-examples)
3. [CSS Organization Examples](#css-organization-examples)
4. [Git Workflow Examples](#git-workflow-examples)
5. [CI/CD Configuration Examples](#cicd-configuration-examples)
6. [Hot Reload Scenarios](#hot-reload-scenarios)

---

## Testing Patterns

### Writing Component Tests

**Rule:** See [README.md - Testing](../README.md#testing) for commands and [CLAUDE.md - Hot Reload Limitations](../CLAUDE.md#hot-reload-limitations) for the policy on testing before hot reload.

**bUnit Component Test Example:**

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
        Assert.notNull(cut.Find("button"));
    }
}
```

**Test Structure:**
- `BobsComponent.Library.Tests` - xUnit + bUnit tests for component library
- Tests are organized by component in `Components/` folder
- Example: `Components/AsyncButtonTests.cs` tests the AsyncButton component

### Test-Driven Development Workflow

**Rule:** IMPORTANT - Always run tests BEFORE starting `dotnet watch` to catch issues that would cause hot reload errors.

**Workflow:**
1. Write unit tests for new component functionality
2. Run tests to verify they fail (`dotnet test`)
3. Implement the component feature
4. Run tests again to verify they pass
5. ONLY THEN start `dotnet watch run` for manual testing

**Why:** This prevents hot reload errors by catching issues before they reach the runtime.

---

## Security Examples

**Rule:** See [CLAUDE.md - Security Best Practices](../CLAUDE.md#security-best-practices) for complete security policies.

### XSS (Cross-Site Scripting) Protection

#### JavaScript Interop - SECURE Pattern

**JavaScript (wwwroot/theme.js):**
```javascript
// SECURE: Dedicated function with input sanitization
window.BobsComponents = window.BobsComponents || {};
window.BobsComponents.setTheme = function(theme) {
    // Sanitize input
    const sanitized = theme.replace(/[^a-zA-Z0-9\-_]/g, '');
    document.documentElement.setAttribute('data-theme', sanitized);
};
```

**C# (ThemeService.cs):**
```csharp
// SECURE: Call dedicated JavaScript function
await _jsRuntime.InvokeVoidAsync("BobsComponents.setTheme", theme);
```

**Never do this:**
- ❌ NEVER use `eval()` for DOM manipulation or JavaScript execution
- ❌ NEVER pass unsanitized user input to JavaScript
- ✅ ALWAYS create dedicated JavaScript functions for specific tasks
- ✅ ALWAYS sanitize input before passing to JavaScript

#### Blazor Rendering

**SECURE:**
```razor
@* Blazor automatically HTML-encodes strings *@
<div>@UserInput</div>
<div>@variable</div>
```

**INSECURE:**
```razor
@* NEVER use MarkupString with unsanitized user input *@
<div>@((MarkupString)userInput)</div>  ❌ DANGEROUS
```

**Rules:**
- ✅ Use `@variable` syntax - Blazor automatically HTML-encodes strings
- ❌ NEVER use `@((MarkupString)userInput)` with unsanitized user input
- ✅ RenderFragment is safe - framework handles sanitization

#### CSS Injection Protection

**Rule:** Sanitize CSS values when building inline styles from parameters.

**Example:** See `SkeletonLoader.razor` for sanitization implementation.

```csharp
// Use regex to allow only safe characters (alphanumeric, %, -, ., units)
private string SanitizeCssValue(string value)
{
    return Regex.Replace(value, @"[^a-zA-Z0-9%\-\.\s]", "");
}
```

### Error Handling Examples

#### Production Error Handling - SECURE

```csharp
catch (Exception ex)
{
    // Log server-side (production)
    _logger.LogError(ex, "Failed to process request");

    // Return generic message to client
    return AsyncOperationResult.CreateError("An error occurred. Please try again.");
}
```

**Rules:**
- ❌ NEVER expose `exception.Message` or stack traces to clients in production
- ✅ Log errors server-side with full details
- ✅ Return generic error messages to clients

#### Demo Service Error Handling - Acceptable

```csharp
/// <remarks>
/// WARNING: This service exposes exception details for demo/testing purposes.
/// In production services, NEVER expose exception.Message to clients.
/// </remarks>
public class MockApiService
{
    public async Task<AsyncOperationResult> FetchData()
    {
        try
        {
            // ... demo logic
        }
        catch (Exception ex)
        {
            // OK for demos - clearly documented as test/demo code
            return AsyncOperationResult.CreateError($"API Error: {ex.Message}");
        }
    }
}
```

**Rule:** For demo/test services (like `MockApiService`), clearly document that they expose errors for educational purposes.

### Input Validation Examples

**Component Parameters:**
```csharp
[Parameter]
public ButtonStyle Style { get; set; } = ButtonStyle.Primary;  // ✅ Use enums

[Parameter]
public string CustomClass { get; set; } = "";  // ✅ Validate/sanitize before use
```

**Rules:**
- ✅ Validate all user-controllable parameters
- ✅ Use enums for predefined options (e.g., `ButtonStyle`, `ThemeInfo`)
- ✅ Sanitize string inputs used in styles, attributes, or JavaScript
- ❌ NEVER trust user input, even from other components

### Secrets Management

**Configuration - SECURE:**
```bash
# Use user secrets for local development
dotnet user-secrets set "ApiKey" "your-key-here"
```

```csharp
// Access secrets from configuration
var apiKey = _configuration["ApiKey"];
```

**Code Review Checklist:**
```bash
# Scan for potential secrets before committing
grep -r "password\|api[_-]?key\|secret\|token" --include="*.cs" --include="*.json"
```

**Rules:**
- ❌ NEVER commit API keys, passwords, or credentials to the repository
- ✅ Use environment variables or Azure Key Vault for secrets
- ✅ Add `.env` files to `.gitignore`
- ✅ Use user secrets for local development: `dotnet user-secrets`

### Security Audit Checklist

Before releasing new features, verify:

**1. XSS Protection:**
- [ ] No `eval()` usage in JavaScript
- [ ] No `MarkupString` with unsanitized input
- [ ] All JavaScript interop uses dedicated functions

**2. Information Disclosure:**
- [ ] No exception details exposed to clients (production code)
- [ ] Error messages are generic
- [ ] No sensitive data in logs sent to client

**3. Input Validation:**
- [ ] All user inputs validated/sanitized
- [ ] CSS values sanitized if building inline styles
- [ ] Enum values used for predefined options

**4. Secrets:**
- [ ] No hardcoded credentials
- [ ] No API keys in code or config files
- [ ] Sensitive config in environment variables/user secrets

**5. Dependencies:**
- [ ] NuGet packages up to date
- [ ] No known vulnerabilities in dependencies
- [ ] Run `dotnet list package --vulnerable`

---

## CSS Organization Examples

**Rule:** See [CLAUDE.md - CSS Organization](../CLAUDE.md#critical-css-organization-for-component-library) for complete CSS rules.

### Correct File Structure

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

### Color System Usage Examples

**CORRECT:**
```css
.my-button {
    background: var(--color-primary);
    color: var(--color-primary-text);
}

.my-button:hover {
    background: var(--color-primary-hover);
}

.success-message {
    background: var(--color-success);
    color: var(--color-success-text);
}
```

**WRONG - Hardcoded:**
```css
/* ❌ WRONG - Hardcoded colors */
.my-button {
    background: #0d6efd;
    color: white;
}

/* ❌ WRONG - Hardcoded z-index */
.my-dropdown {
    z-index: 1000;
}
```

**WRONG - Custom Variables:**
```css
/* ❌ WRONG - Component-specific color variables */
.async-button {
    background: var(--btn-primary);  /* Should use --color-primary */
    color: var(--async-text);        /* Should use --color-primary-text */
}
```

**Rule:** ALWAYS use `--color-*` variables, NEVER hardcode colors or create component-specific color variables.

---

## Git Workflow Examples

**Rule:** See [CLAUDE.md - Git Workflow](../CLAUDE.md#git-workflow) for complete git policies.

### Resolving Merge Conflicts

**When you encounter conflicts after `git merge master`:**

1. **Identify conflicting files:**
   ```bash
   git status
   ```

2. **Open conflicts in your editor**
   - Look for conflict markers:
   ```
   <<<<<<< HEAD
   Your changes
   =======
   Changes from master
   >>>>>>> master
   ```

3. **Resolve each conflict:**
   - Choose the correct code
   - Remove conflict markers (`<<<<<<<`, `=======`, `>>>>>>>`)
   - Ensure the code makes sense after merging

4. **Stage resolved files:**
   ```bash
   git add <resolved-file>
   ```

5. **Complete the merge:**
   ```bash
   git commit
   ```

### Pre-Push Testing Workflow

**CRITICAL:** Before pushing to remote as the FINAL action, ALWAYS verify your changes.

**1. Run the application locally:**
```bash
cd src/BobsComponent.Client
dotnet watch run
```

**Verify:**
- Application starts without errors
- Test affected functionality manually
- Check for console errors or warnings
- Test in browser at https://localhost:7017

**2. Run tests:**
```bash
cd src
dotnet test
```

**Verify:**
- All tests pass
- No test failures or warnings
- Fix any failing tests before pushing

**3. Manual verification:**
- Ask user to verify changes if needed
- Document what was tested in commit messages or PR description

**Remember:** Never push broken code to the remote repository. Local testing is mandatory before every push.

---

## CI/CD Configuration Examples

**Rule:** See [CLAUDE.md - CI/CD Pipeline Changes](../CLAUDE.md#cicd-pipeline-changes) for complete CI/CD policies.

### Build Command Sequence

**Example from current workflow:**
```bash
dotnet workload install wasm-tools
dotnet restore src/BobsComponents.sln
dotnet build src/BobsComponents.sln --configuration Release --no-restore -m
dotnet test src/BobsComponents.sln --configuration Release --verbosity normal
dotnet publish src/BobsComponent.Client/BobsComponent.Client.csproj --configuration Release --output publish
```

### Validating Workflow Changes Locally

**1. Validate YAML syntax:**
```bash
# Install yamllint if not already installed
pip install yamllint

# Validate workflow syntax
yamllint .github/workflows/deploy.yml
```

**2. Test build commands:**
- Extract the exact commands from the workflow file
- Run them in the same sequence locally
- Verify each command succeeds before pushing

**3. Verify command flags:**
```bash
# Check valid flags before using them
dotnet build --help
dotnet restore --help
```

### Common Pitfalls

**Invalid flags:**
- ❌ `--parallel` on `dotnet restore` (invalid flag)
- ❌ `-maxcpucount` (should be `-m` for MSBuild)
- ✅ `-m` for parallel builds
- ✅ `--no-restore` with `dotnet build`

**Mixing contexts:**
- ❌ Using `--no-build` with different project contexts
- ✅ Ensure `--no-build` references the same project that was just built

**Incremental testing:**
- ❌ NEVER batch multiple untested workflow changes
- ✅ Make ONE change at a time
- ✅ Push and verify it works before adding more changes
- ✅ Create feature branch to test multiple changes together

**Why this matters:** CI/CD failures waste GitHub Actions minutes, delay deployments, and can block the entire team.

---

## Hot Reload Scenarios

**Rule:** See [CLAUDE.md - Hot Reload Limitations](../CLAUDE.md#hot-reload-limitations) for complete hot reload policies.

### Changes Requiring Manual Restart

| Change Type | Example | Why Restart Required |
|-------------|---------|----------------------|
| **Component References** | Adding/removing `@ref` attributes | Hot reload can't update component reference infrastructure |
| | Changing component reference types | Component binding is compiled |
| **Component Parameters** | Adding/removing `[Parameter]` attributes | Parameter binding is compiled, not runtime |
| | Changing parameter types | Metadata changes require recompilation |
| **Public API Changes** | Adding new public methods to components | Component metadata changes (sometimes works, sometimes fails) |
| | Changing method signatures | Public API compilation issues |
| **Render Fragment Changes** | Significant changes to `RenderFragment` | Render tree compilation issues |
| | Modifying `ChildContent` structure | Framework render tree updates |

### Safe for Hot Reload

| Change Type | Notes |
|-------------|-------|
| **CSS changes** | Always safe - hot reload handles CSS perfectly |
| **Markup changes** | Usually safe in `.razor` files |
| **Private method implementations** | Safe - internal logic changes |
| **Logic inside existing methods** | Safe - doesn't change method signatures |

### When You See Mono Assertion Errors

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

---

**Navigation:** [Back to README](../README.md) | [Back to CLAUDE.md](../CLAUDE.md)
