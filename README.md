# BobsBlazingComponents

> A modern Blazor WebAssembly component library emphasizing real-world patterns, theming flexibility, and developer education.

## Project Goal

Create reusable, themeable components that are open to user customization while providing base themes and functionality.

**Key Objectives:**
- Provide "amazing" examples of real-world usage, especially combining popular packages
- Demonstrate practical integration patterns that developers can learn from and adapt
- Focus on components that solve real problems, not just basic examples

---

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- Visual Studio Code (recommended) or your preferred IDE
- Docker Desktop (optional, for dev container)

### Running the Application

```bash
cd src/BobsComponent.Client
dotnet watch run
```

The application will be available at:
- **HTTPS:** https://localhost:7017
- **HTTP:** http://localhost:5096

### Using Dev Container (Optional)

1. Install Remote Containers VS Code Extension
2. Install Docker Desktop
3. Open workspace in VS Code
4. Select "Reopen in Container"

The container includes .NET 10.0, Node.js LTS, and the C# extension.

---

## Project Overview

### Architecture

BobsBlazingComponents uses a two-project architecture:

- **BobsComponent.Library**: Razor component library (SDK: Microsoft.NET.Sdk.Razor) that houses reusable Blazor components
- **BobsComponent.Client**: Blazor WebAssembly client application for testing/showcasing the component library

Both projects reference BobsComponent.Library - the Client uses both a PackageReference and a ProjectReference to the Library.

### Application Sections

The client application is organized into three distinct sections:

1. **Documentation Area**: Comprehensive documentation for components, APIs, and usage patterns
2. **Sandbox**: Interactive playground for experimenting with components in real-time
3. **Learning Section**: Structured progression-based learning path for users to master the component library

### User Sessions

The application includes user session management:
- Email-based validation (no password authentication)
- No PII (Personally Identifiable Information) stored
- No security-sensitive data - session tracking is purely for progression data

---

## Development Setup

### .NET Version & Requirements

This project targets **.NET 10.0** (LTS release, supported until November 2028) with **C# 14** language features.

**C# 14 Features in Use:**
- **Collection expressions**: `[]` syntax for cleaner collection initialization
- **Field-backed properties**: Using `field` keyword in custom accessors (planned for future components)

When contributing, prefer modern C# 14 patterns over legacy syntax.

### Required Workloads

For Blazor WebAssembly development, install the wasm-tools workload:

```bash
dotnet workload install wasm-tools
```

### Build Configuration

The solution uses centralized build configuration via `src/Directory.Build.props`:
- Target framework: net10.0 (configurable via `$(NetVersion)`)
- AOT compilation is enabled (`RunAOTCompilation: true`)

---

## Common Commands

### Running the Application

```bash
cd src/BobsComponent.Client
dotnet watch run
```

### Building

```bash
# Build solution
cd src
dotnet build BobsComponents.sln

# Debug build (default)
dotnet build

# Release build with AOT
dotnet build -c Release
```

### Testing

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
- See [docs/EXAMPLES.md](docs/EXAMPLES.md#testing-patterns) for test code examples

### Hot Reload Limitations

Some code changes cannot be hot-reloaded and require a manual restart (Ctrl+C then `dotnet watch run`):

- Adding/removing `@ref` attributes or `[Parameter]` attributes
- Changing component reference types or parameter types
- Adding new public methods to components
- Significant changes to `RenderFragment` or `ChildContent`

**Safe for hot reload:** CSS changes, markup changes in `.razor` files, private method implementations

See [CLAUDE.md - Hot Reload Limitations](CLAUDE.md#hot-reload-limitations) for complete details.

---

## Architecture Details

### Component Library Structure

The Library project (BobsComponent.Library):
- Uses `_Imports.razor` for shared namespace imports
- Contains a `wwwroot` folder with assets and JavaScript interop files
- Provides themeable, customizable components

### Import Structure

The Client project's `_Imports.razor` establishes global namespace access:
- Standard Blazor namespaces (Components.Forms, Components.Routing, etc.)
- BobsComponent.Client and BobsComponent.Client.Shared
- **BobsComponent.Library** - making library components available throughout the client

New components in the Library are automatically available in Client pages without additional imports.

### Theming Approach

The project emphasizes user-toggleable theming:
- `themes.css`: Semantic color variables and z-index system
- `app.css`: Main application styles
- `jc-reset.css`: CSS reset
- Bootstrap integration
- Open Iconic icon library

**CSS Organization:**
- Component-specific styles use scoped CSS (`.razor.css` files)
- Global theme variables in `wwwroot/css/themes.css`
- See [CLAUDE.md - CSS Organization](CLAUDE.md#critical-css-organization-for-component-library) for complete CSS rules

---

## Component Development

### Development Philosophy

When creating new components for this library:

- **Real-world examples**: Components should showcase practical, production-ready usage patterns
- **Package integration**: Demonstrate how to combine popular NuGet packages effectively (e.g., FluentValidation, AutoMapper, charting libraries, state management)
- **Educational value**: Code should serve as a learning resource, showing best practices and integration patterns
- **Practical over trivial**: Focus on components that solve real problems developers face

### Security-First Principles

This project follows security-first development:
- XSS protection in JavaScript interop and rendering
- Information disclosure prevention
- Input validation for all component parameters
- Secure secrets management

See [CLAUDE.md - Security Best Practices](CLAUDE.md#security-best-practices) for complete security rules and [docs/EXAMPLES.md - Security Examples](docs/EXAMPLES.md#security-examples) for code examples.

### Refactoring Policy

**Do NOT rename existing files, functions, classes, or components without explicit approval.**

- Never rename existing code elements unless explicitly requested
- Always ask before refactoring that involves renaming or moving code
- This prevents breaking changes and maintains git history

See [CLAUDE.md - Refactoring Policy](CLAUDE.md#refactoring-policy) for details.

---

## Contributing

### Git Workflow

**Before pushing to remote:**

1. **Sync with master:**
   ```bash
   git pull origin master
   git checkout your-branch
   git merge master
   ```

2. **Resolve conflicts locally** (if any)

3. **Run tests:**
   ```bash
   dotnet test
   ```

4. **Test locally:**
   ```bash
   cd src/BobsComponent.Client
   dotnet watch run
   ```

5. **Verify in browser**, then push

**CRITICAL:** Never push broken code. Local testing is mandatory before every push.

See [CLAUDE.md - Git Workflow](CLAUDE.md#git-workflow) for complete git policies and [docs/EXAMPLES.md - Git Workflow Examples](docs/EXAMPLES.md#git-workflow-examples) for detailed examples.

### Pre-Push Testing

Always verify your changes before pushing:
1. Run the application locally and test affected functionality
2. Run all tests with `dotnet test`
3. Check for console errors or warnings
4. Ask user to verify changes if needed

---

## Documentation

### For Developers

- **[README.md](README.md)** (this file) - Getting started, commands, architecture
- **[docs/EXAMPLES.md](docs/EXAMPLES.md)** - Detailed code examples and patterns
- Component API references (coming soon)

### For AI Assistants

- **[CLAUDE.md](CLAUDE.md)** - AI assistant instructions and coding rules
  - CSS organization rules
  - Security policies
  - Git workflow rules
  - Refactoring policies
  - Windows development requirements

---

## Tech Stack

- **.NET 10.0** (LTS until November 2028)
- **C# 14** language features
- **Blazor WebAssembly** (client-side rendering)
- **bUnit + xUnit** for component testing
- **Bootstrap** for base styling
- **Open Iconic** for icons

---

**For AI Assistants:** See [CLAUDE.md](CLAUDE.md) for detailed coding rules and policies.
