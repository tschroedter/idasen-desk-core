# Development Guide

This guide provides comprehensive information for developers working on the Idasen Desk Core library.

## Table of Contents
- [Development Environment Setup](#development-environment-setup)
- [Project Structure](#project-structure)
- [Building the Project](#building-the-project)
- [Development Workflow](#development-workflow)
- [Debugging](#debugging)
- [Code Quality](#code-quality)
- [Dependencies](#dependencies)

## Development Environment Setup

### Required Tools

#### Essential
- **Windows 10/11**: Required for Bluetooth LE APIs (version 19041 or later)
- **.NET 8.0 SDK**: Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Git**: Version control ([git-scm.com](https://git-scm.com/))
- **PowerShell**: 5.1 or PowerShell Core 7.x

#### Recommended IDEs
- **Visual Studio 2022** (Community, Professional, or Enterprise)
  - Workloads: .NET desktop development
  - Extensions: ReSharper (optional)
- **JetBrains Rider** (commercial, excellent for .NET development)

#### Optional Tools
- **Windows Terminal**: Modern terminal experience
- **Git GUI**: GitKraken, GitHub Desktop, or similar
- **Postman**: For testing REST API integrations

### IDE Configuration

#### Visual Studio 2022
1. Install .NET desktop development workload
2. Install recommended extensions:
   - ReSharper (optional but recommended)
   - SonarLint for code quality
3. Configure code style:
   - File → Preferences → Text Editor → C# → Code Style
   - Import `.editorconfig` settings (automatic)

#### JetBrains Rider
1. Open solution file `idasen-desk-core.sln`
2. Configure code style:
   - Settings → Editor → Code Style → C#
   - Enable "Use EditorConfig"
3. Enable inspections:
   - Settings → Editor → Inspections
   - Enable all .NET and C# inspections

### Hardware Setup

#### Bluetooth Adapter
- Bluetooth 4.0 or later with BLE support
- Verify in Device Manager:
  ```powershell
  Get-PnpDevice -Class Bluetooth
  ```

#### Idasen Desk (for integration testing)
- Ikea Idasen standing desk with Bluetooth
- Ensure desk is powered and in range
- Unpair desk from other devices before testing

## Project Structure

```
idasen-desk-core/
├── .github/                    # GitHub Actions workflows and configuration
│   ├── workflows/              # CI/CD workflow definitions
│   ├── copilot-instructions.md # GitHub Copilot configuration
│   └── SONARCLOUD_SETUP.md    # SonarCloud setup guide
├── src/                        # Source code
│   ├── Idasen.Aop/            # Aspect-oriented programming utilities
│   ├── Idasen.BluetoothLE.Core/ # Core Bluetooth LE functionality
│   ├── Idasen.BluetoothLE.Characteristics/ # BLE characteristics
│   ├── Idasen.BluetoothLE.Linak/ # Linak protocol implementation
│   ├── Idasen.Launcher/       # Application launcher
│   ├── Idasen.ConsoleApp/     # Example console application
│   ├── *.Tests/               # Unit test projects
│   └── Idasen.BluetoothLE.Integration.Tests/ # Integration tests
├── wiki/                       # Wiki documentation (this guide)
├── build.ps1                   # Build script
├── README.md                   # Repository readme
├── LICENSE                     # MIT License
└── .editorconfig              # Code style configuration
```

### Project Dependencies

```
Idasen.ConsoleApp
    └─→ Idasen.Launcher
        └─→ Idasen.BluetoothLE.Linak
            └─→ Idasen.BluetoothLE.Characteristics
                └─→ Idasen.BluetoothLE.Core
                    └─→ Idasen.Aop
```

## Building the Project

### Using PowerShell Build Script

The `build.ps1` script provides convenient commands for all build operations:

```powershell
# Build everything
.\build.ps1 -Action build

# Run tests with coverage
.\build.ps1 -Action test

# Check code formatting
.\build.ps1 -Action format

# Create NuGet package
.\build.ps1 -Action pack

# Clean build artifacts
.\build.ps1 -Action clean

# Run all steps (clean, restore, build, test, format, pack)
.\build.ps1 -Action all
```

### Using .NET CLI Directly

```powershell
# Navigate to source directory
cd src

# Restore NuGet packages
dotnet restore

# Build Debug configuration
dotnet build --configuration Debug

# Build Release configuration
dotnet build --configuration Release

# Run tests
dotnet test --configuration Debug

# Create NuGet package
dotnet pack --configuration Release --output ./artifacts
```

### Build Configurations

#### Debug
- Optimizations disabled
- Debug symbols included
- Code analysis enabled
- Detailed logging

```powershell
dotnet build --configuration Debug
```

#### Release
- Full optimizations enabled
- Code analysis enabled
- Warnings as errors
- Ready for distribution

```powershell
dotnet build --configuration Release
```

## Development Workflow

### Daily Development

#### 1. Start Your Work
```powershell
# Update your local repository
git pull upstream main

# Create a feature branch
git checkout -b feature/my-feature

# Build and test to ensure starting point is good
.\build.ps1 -Action build
.\build.ps1 -Action test
```

#### 2. Make Changes
```powershell
# Edit code in your IDE

# Build frequently to catch errors early
dotnet build

# Run specific test project
dotnet test src/Idasen.BluetoothLE.Core.Tests
```

#### 3. Test Your Changes
```powershell
# Run all tests
.\build.ps1 -Action test

# Run tests for specific category
dotnet test --filter "Category!=Integration"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

#### 4. Verify Code Quality
```powershell
# Check code formatting
.\build.ps1 -Action format

# Fix formatting issues
dotnet format

# Build with warnings as errors
dotnet build --configuration Release
```

#### 5. Commit Changes
```powershell
# Stage changes
git add .

# Commit with conventional commit message
git commit -m "feat: add new feature"

# Push to your fork
git push origin feature/my-feature
```

### Working with Packages

#### Package References
Managed via `Directory.Packages.props`:

```xml
<ItemGroup>
  <PackageReference Update="Autofac" Version="8.4.0" />
  <PackageReference Update="Serilog" Version="4.3.0" />
  <!-- More packages -->
</ItemGroup>
```

#### Adding a New Package
```powershell
# Add package to project
cd src/ProjectName
dotnet add package PackageName

# Update version in Directory.Packages.props if needed
```

#### Updating Packages
```powershell
# Update all packages
dotnet restore

# Update specific package (edit Directory.Packages.props)
# Then restore
dotnet restore
```

## Debugging

### Debugging in Visual Studio

#### 1. Set Startup Project
Right-click `Idasen.ConsoleApp` → Set as Startup Project

#### 2. Set Breakpoints
Click in the left margin of code editor

#### 3. Start Debugging
- Press **F5** or click **Start Debugging**
- Use **F10** (Step Over) and **F11** (Step Into)

#### 4. Debug Tests
Right-click test method → Debug Test

### Debugging in Rider

#### 1. Run Configuration
Rider auto-detects run configurations from projects

#### 2. Debug Session
- Click debug icon next to run configuration
- Set breakpoints by clicking in gutter
- Use **F8** (Step Over) and **F7** (Step Into)

### Debugging Tips

#### Enable Verbose Logging
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()  // Most detailed level
    .WriteTo.Console()
    .CreateLogger();
```

#### Conditional Breakpoints
Set conditions on breakpoints to break only when specific conditions are met.

#### Data Breakpoints
Break when a variable's value changes (Visual Studio Enterprise).

#### Bluetooth Debugging
Enable Bluetooth debug logging in Windows:
1. Open Event Viewer
2. Applications and Services Logs → Microsoft → Windows → Bluetooth
3. Enable debug logging

### Common Debugging Scenarios

#### Device Not Found
```csharp
// Add logging to device discovery
devices.DeviceDiscovered.Subscribe(device =>
{
    Log.Debug("Discovered: {Name} - {Address}", 
        device.Name, device.Address);
});
```

#### Connection Issues
```csharp
// Log connection attempts
device.ConnectionStatusChanged.Subscribe(status =>
{
    Log.Debug("Connection status: {Status}", status);
});
```

#### Characteristic Problems
```csharp
// Log characteristic operations
var value = await characteristic.ReadAsync();
Log.Debug("Read characteristic {Uuid}: {Value}", 
    characteristic.Uuid, BitConverter.ToString(value));
```

## Code Quality

### Static Analysis

#### .NET Analyzers
Enabled by default in all projects:

```xml
<PropertyGroup>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

#### SerilogAnalyzer
Validates Serilog usage:

```xml
<PackageReference Update="SerilogAnalyzer" Version="0.15.0" />
```

### Code Formatting

#### Check Formatting
```powershell
.\build.ps1 -Action format
# Or
dotnet format --verify-no-changes
```

#### Fix Formatting
```powershell
dotnet format
```

### Code Coverage

#### Generate Coverage Report
```powershell
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

#### View Coverage in CI
- Check the GitHub Actions workflow results
- View SonarCloud analysis for detailed metrics

### EditorConfig Settings

The `.editorconfig` file enforces code style:

```ini
# C# coding conventions
[*.cs]
csharp_using_directive_placement = outside_namespace
csharp_prefer_simple_using_statement = true
csharp_prefer_braces = when_multiline
csharp_style_var_for_built_in_types = true
```

## Dependencies

### Core Dependencies

#### Runtime Dependencies
- **Autofac** (8.4.0): Dependency injection container
- **Autofac.Extras.DynamicProxy** (7.1.0): AOP support
- **Serilog** (4.3.0): Structured logging
- **System.Reactive** (6.1.0): Reactive extensions
- **CsvHelper** (33.1.0): CSV parsing for GATT services

#### Development Dependencies
- **xUnit** (2.9.x): Testing framework
- **FluentAssertions** (6.x): Assertion library
- **NSubstitute** (5.x): Mocking framework

### Managing Dependencies

#### Check for Updates
```powershell
# List outdated packages
dotnet list package --outdated

# Update all packages to latest
# (Edit Directory.Packages.props manually)
```

#### Security Audits
```powershell
# Check for security vulnerabilities
dotnet list package --vulnerable
```

### Platform Dependencies

#### Windows-Specific
```xml
<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
```

This targets:
- Windows 10 version 2004 (build 19041) or later
- Windows 11 all versions

## Performance Profiling

### Using Visual Studio Profiler
1. Debug → Performance Profiler
2. Select profiling tools (CPU, Memory, etc.)
3. Start profiling
4. Perform operations
5. Stop and analyze results

### Using dotnet-trace
```powershell
# Install tool
dotnet tool install -g dotnet-trace

# Record trace
dotnet-trace collect --process-id <PID>

# View in Visual Studio or PerfView
```

## Troubleshooting Development Issues

### Build Failures

#### NuGet Restore Fails
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore
```

#### Compiler Errors
```powershell
# Clean and rebuild
.\build.ps1 -Action clean
.\build.ps1 -Action build
```

### Test Failures

#### Integration Tests Fail
- Ensure Bluetooth adapter is enabled
- Check desk is powered and in range
- Verify no other apps connected to desk

#### Unit Tests Fail
- Check test dependencies
- Verify mock configurations
- Review test output for details

### IDE Issues

#### IntelliSense Not Working
- Close and reopen solution
- Delete `.vs` folder (Visual Studio) or `.idea` folder (Rider)
- Rebuild solution

#### Code Analysis Warnings
- Review warning message
- Fix the issue or suppress if false positive
- Document suppressions

## Best Practices

### Development Best Practices

1. **Write Tests First**: Follow TDD when possible
2. **Commit Often**: Small, focused commits
3. **Keep PRs Small**: Easier to review and merge
4. **Document Changes**: Update XML docs and wiki
5. **Review Your Code**: Self-review before submitting PR

### Code Review Checklist

Before submitting a PR:
- [ ] All tests pass
- [ ] Code formatted correctly
- [ ] XML documentation updated
- [ ] No new warnings
- [ ] Performance impact considered
- [ ] Security implications reviewed
- [ ] Breaking changes documented

## Additional Resources

- [Architecture and Design](Architecture-and-Design.md)
- [Testing Guide](Testing-Guide.md)
- [API Reference](API-Reference.md)
- [Contributing Guide](Contributing.md)

---

Need help? Open an issue or discussion on [GitHub](https://github.com/tschroedter/idasen-desk-core/issues).
