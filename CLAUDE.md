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
