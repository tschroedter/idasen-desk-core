# Copilot Instructions for idasen-desk-core

## Project Overview

This repository contains a .NET library for controlling Ikea's Idasen Desk via Bluetooth LE on Windows. The library is built as a NuGet package and used by desktop applications and REST APIs.

**Key Purpose**: Low-level Bluetooth LE communication to detect and control Idasen desks from Windows 10/11.

## Technology Stack

- **Language**: C# 12
- **Framework**: .NET 8.0
- **Platform**: Windows 10/11 (required for Bluetooth LE APIs)
- **Build Tools**: dotnet CLI, PowerShell
- **Testing**: xUnit
- **Code Quality**: .NET Analyzers, SerilogAnalyzer, dotnet format
- **Logging**: Serilog
- **AOP**: Autofac with DynamicProxy for aspect-oriented programming

## Architecture Patterns

- **Dependency Injection**: Uses Autofac container
- **Aspect-Oriented Programming**: Logging aspects via interceptors
- **Interfaces**: Heavy use of interfaces for testability and modularity
- **Characteristics Pattern**: Bluetooth characteristics encapsulated in dedicated classes

## Build and Development

### Local Build Commands

Use the provided `build.ps1` PowerShell script for all development tasks:

```powershell
# Build the solution
.\build.ps1 -Action build

# Run tests with coverage
.\build.ps1 -Action test

# Check code formatting
.\build.ps1 -Action format

# Create NuGet package
.\build.ps1 -Action pack

# Clean build artifacts
.\build.ps1 -Action clean

# Run everything
.\build.ps1 -Action all
```

### Manual dotnet Commands

```powershell
cd src
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
dotnet format --verify-no-changes --verbosity diagnostic
```

## Code Style Guidelines

### General Principles

- **Nullable Reference Types**: Enabled throughout the project
- **Implicit Usings**: Enabled
- **Line Endings**: CRLF (Windows standard)
- **Encoding**: UTF-8
- **Indentation**: 4 spaces
- **Warnings as Errors**: Enabled in builds

### C# Style (from .editorconfig)

- **var usage**: Use `var` for built-in types and when type is apparent
- **Using directives**: Outside namespace, system directives first
- **Expression-bodied members**: Preferred where appropriate
- **Spacing**: Unusual style with spaces around parentheses and operators (see existing code)
- **Modifiers**: `private` keyword required on all members

### Code Examples

```csharp
// Typical spacing style in this codebase
public string Convert ( IInvocation invocation )
{
    if ( value is null )
        return "null" ;
        
    var builder = new StringBuilder ( ) ;
    _ = builder.Append ( $"arg{i}=" ) ;
}

// Interface naming
public interface IInvocationToTextConverter { }

// Records for data structures
public readonly record struct StopDetails ( bool ShouldStop, Direction Desired ) ;
```

## Testing Guidelines

- **Framework**: xUnit
- **Naming**: Test classes end with `Tests` suffix
- **Location**: Test projects mirror source structure (e.g., `Idasen.Aop` → `Idasen.Aop.Tests`)
- **Integration Tests**: Separate `Idasen.BluetoothLE.Integration.Tests` project
- **Coverage**: Tests should cover core functionality and edge cases

## Security Considerations

- **Sensitive Data**: The codebase includes data masking for logging (see `InvocationToTextConverter`)
- **No Secrets**: Never commit secrets or sensitive data
- **Large Arrays**: Safeguards exist for processing large data structures

## CI/CD Workflows

The project uses GitHub Actions for automation:

- **Build and Test** (`ci.yml`): Runs on every push/PR
- **Release and Publish** (`release.yml`): Automated versioning and NuGet publishing
- **Code Quality** (`code-quality.yml`): Static analysis and security scanning
- **PR Validation** (`pr-validation.yml`): Pull request checks
- **Dependency Updates** (`dependency-update.yml`): Weekly security audits

## Important Constraints

- **Windows-Only**: This project requires Windows 10/11 for Bluetooth LE APIs
- **Bluetooth Hardware**: Physical Bluetooth adapter required for integration tests
- **Idasen Desk**: Full integration testing requires actual Idasen desk hardware

## Common Pitfalls to Avoid

1. **Platform-Specific Code**: Always remember this is Windows-only; don't add cross-platform abstractions
2. **Bluetooth LE**: Be careful with async/await patterns around Bluetooth operations
3. **Code Formatting**: The project has unusual spacing style; follow existing patterns
4. **Analyzers**: Don't disable analyzer warnings without good reason; fix the code instead
5. **Package Locks**: `RestorePackagesWithLockFile` is enabled; commit package lock files

## Making Changes

When contributing:

1. Run `.\build.ps1 -Action format` before committing
2. Ensure all tests pass with `.\build.ps1 -Action test`
3. Follow existing code style and spacing patterns
4. Update XML documentation comments for public APIs
5. Don't modify unrelated code or files
6. Keep changes minimal and focused

## Documentation

- **XML Comments**: Required for public APIs
- **README.md**: Keep usage examples up to date
- **Code Comments**: Only when explaining complex logic, not obvious code

## References

- Primary consumers: [idasen-desk](https://github.com/tschroedter/idasen-desk), [idasen-desk-rest-api](https://github.com/tschroedter/idasen-desk-rest-api)
- NuGet Package: `Idasen.Desk.Core`
- License: MIT
