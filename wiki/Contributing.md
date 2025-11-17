# Contributing Guide

Thank you for your interest in contributing to Idasen Desk Core! This guide will help you get started with contributing to the project.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Pull Request Process](#pull-request-process)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)

## Code of Conduct

### Our Pledge
We are committed to providing a welcoming and inclusive experience for everyone. We expect all contributors to:
- Be respectful and considerate
- Accept constructive criticism gracefully
- Focus on what is best for the community
- Show empathy towards other community members

## How Can I Contribute?

### Reporting Bugs
Before creating a bug report:
1. **Check existing issues** to avoid duplicates
2. **Use the latest version** to verify the bug still exists
3. **Gather information** about your environment

When creating a bug report, include:
- **Clear title** describing the issue
- **Detailed description** of the problem
- **Steps to reproduce** the issue
- **Expected behavior** vs actual behavior
- **Environment details**:
  - Windows version
  - .NET SDK version
  - Bluetooth adapter information
  - Desk model (if relevant)
- **Screenshots or logs** if applicable

### Suggesting Enhancements
Enhancement suggestions are welcome! Include:
- **Clear description** of the enhancement
- **Use cases** explaining why it's useful
- **Potential implementation** ideas (optional)
- **Mockups or examples** if applicable

### Contributing Code
1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Make your changes** following our coding standards
4. **Write tests** for your changes
5. **Update documentation** as needed
6. **Submit a pull request**

### Contributing Documentation
Documentation improvements are always appreciated:
- Fix typos or unclear explanations
- Add examples or use cases
- Improve API documentation
- Create tutorials or guides

## Development Setup

### Prerequisites
1. **Windows 10/11** (required for Bluetooth LE APIs)
2. **.NET 8.0 SDK** or later
3. **Git** for version control
4. **IDE**: Visual Studio 2022 or JetBrains Rider (recommended)
5. **PowerShell** 5.1 or later

### Initial Setup
```powershell
# 1. Fork and clone the repository
git clone https://github.com/YOUR_USERNAME/idasen-desk-core.git
cd idasen-desk-core

# 2. Add upstream remote
git remote add upstream https://github.com/tschroedter/idasen-desk-core.git

# 3. Restore dependencies
cd src
dotnet restore

# 4. Build the solution
dotnet build --configuration Debug

# 5. Run tests
dotnet test --configuration Debug
```

### Development Workflow
```powershell
# 1. Create a new feature branch
git checkout -b feature/my-feature

# 2. Make changes and test frequently
.\build.ps1 -Action build
.\build.ps1 -Action test

# 3. Check code formatting
.\build.ps1 -Action format

# 4. Commit changes
git add .
git commit -m "feat: add amazing feature"

# 5. Keep your branch updated
git fetch upstream
git rebase upstream/main

# 6. Push to your fork
git push origin feature/my-feature

# 7. Create a pull request on GitHub
```

## Coding Standards

### C# Style Guidelines

#### General Rules
- Follow the `.editorconfig` settings in the repository
- Use **nullable reference types** throughout
- Enable **implicit usings** (already configured)
- Prefer **explicit types** for clarity, except where type is obvious

#### Naming Conventions
```csharp
// Interfaces start with 'I'
public interface IDevice { }

// Classes use PascalCase
public class DeviceMonitor { }

// Private fields with underscore prefix
private readonly ILogger _logger;

// Methods use PascalCase
public async Task ConnectAsync() { }

// Parameters and locals use camelCase
public void Process(string deviceName) { }

// Constants use PascalCase
private const int MaxRetries = 3;
```

#### Spacing (Project-Specific Style)
**Note**: This project uses unusual spacing around parentheses. Follow existing patterns:

```csharp
// Method definitions - spaces around parentheses
public string Convert ( IInvocation invocation )
{
    // Conditionals - spaces around parentheses
    if ( value is null )
        return "null" ;
        
    // Method calls - spaces around parentheses
    var builder = new StringBuilder ( ) ;
}
```

#### Code Organization
```csharp
// 1. Using directives (outside namespace)
using System;
using Serilog;

// 2. Namespace
namespace Idasen.BluetoothLE.Core;

// 3. Interfaces
public interface IDevice
{
    // Properties first
    string Name { get; }
    
    // Methods second
    Task ConnectAsync();
}

// 4. Implementation
public class Device : IDevice
{
    // Private fields
    private readonly ILogger _logger;
    
    // Constructor
    public Device(ILogger logger)
    {
        _logger = logger;
    }
    
    // Properties
    public string Name { get; }
    
    // Public methods
    public async Task ConnectAsync()
    {
        // Implementation
    }
    
    // Private methods
    private void Validate()
    {
        // Implementation
    }
}
```

#### Async/Await Guidelines
```csharp
// Always use Async suffix for async methods
public async Task<Device> GetDeviceAsync() { }

// Use CancellationToken parameter for long-running operations
public async Task ConnectAsync(
    CancellationToken cancellationToken = default)
{
    await Task.Delay(1000, cancellationToken);
}

// Don't block on async code
// BAD:
var result = SomeAsyncMethod().Result;

// GOOD:
var result = await SomeAsyncMethod();
```

#### Null Handling
```csharp
// Use nullable reference types
public string? OptionalValue { get; }

// Use guard clauses
Guard.AgainstNull(parameter, nameof(parameter));

// Use null-conditional operators
var length = text?.Length ?? 0;
```

#### Error Handling
```csharp
// Throw specific exceptions
throw new ArgumentNullException(nameof(parameter));
throw new InvalidOperationException("Device is not connected");

// Use descriptive error messages
throw new DeviceException(
    $"Failed to connect to device '{deviceName}': {errorDetails}");
```

### Documentation Standards

#### XML Documentation
All public APIs must have XML documentation:

```csharp
/// <summary>
/// Discovers Bluetooth LE devices matching the specified criteria.
/// </summary>
/// <param name="timeout">Maximum time to wait for discovery.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A collection of discovered devices.</returns>
/// <exception cref="TimeoutException">
/// Thrown when discovery times out.
/// </exception>
public async Task<IEnumerable<IDevice>> DiscoverAsync(
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

#### Code Comments
```csharp
// Use comments sparingly - code should be self-documenting
// Only add comments to explain WHY, not WHAT

// GOOD: Explains reasoning
// Delay required by Bluetooth LE specification for stable connection
await Task.Delay(TimeSpan.FromSeconds(1));

// BAD: States the obvious
// Delay for 1 second
await Task.Delay(TimeSpan.FromSeconds(1));
```

### Testing Standards

#### Test Naming
```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public void ConnectAsync_WhenDeviceNotFound_ThrowsDeviceNotFoundException()
{
    // Arrange
    var device = CreateDevice();
    
    // Act & Assert
    await Assert.ThrowsAsync<DeviceNotFoundException>(
        () => device.ConnectAsync());
}
```

#### Test Organization
```csharp
public class DeviceTests
{
    [Fact]
    public void Test1() { }
    
    [Theory]
    [InlineData("value1")]
    [InlineData("value2")]
    public void Test2(string input) { }
}
```

## Pull Request Process

### Before Submitting
1. **Ensure tests pass**: Run `.\build.ps1 -Action test`
2. **Check code formatting**: Run `.\build.ps1 -Action format`
3. **Update documentation**: Update README.md, wiki, or XML docs as needed
4. **Add tests**: Include unit tests for new functionality
5. **Follow commit conventions**: Use conventional commits

### Commit Message Format
Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples**:
```bash
feat(bluetooth): add device discovery timeout
fix(desk): correct height calculation for non-metric units
docs(wiki): add troubleshooting guide
test(characteristics): add tests for write operations
```

### Pull Request Template
When creating a PR, include:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Refactoring

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests pass locally
- [ ] No new warnings introduced
```

### Review Process
1. **Automated checks** must pass (CI/CD workflows)
2. **Code review** by maintainer(s)
3. **Address feedback** and update PR
4. **Approval** from maintainer
5. **Merge** by maintainer

## Testing Guidelines

### Test Coverage
- **Aim for high coverage**: Target 80%+ code coverage
- **Focus on critical paths**: Business logic and edge cases
- **Mock external dependencies**: Use mocks for hardware and external services

### Test Types

#### Unit Tests
```csharp
public class DeviceComparerTests
{
    [Fact]
    public void Equals_SameAddress_ReturnsTrue()
    {
        // Arrange
        var device1 = CreateDevice(address: 123);
        var device2 = CreateDevice(address: 123);
        var comparer = new DeviceComparer();
        
        // Act
        var result = comparer.Equals(device1, device2);
        
        // Assert
        Assert.True(result);
    }
}
```

#### Integration Tests
```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task DiscoverDevices_WithRealAdapter_FindsDevices()
{
    // Note: Requires Bluetooth hardware
    var devices = new Devices();
    await devices.StartAsync();
    await Task.Delay(TimeSpan.FromSeconds(5));
    
    Assert.NotEmpty(devices.DetectedDevices);
}
```

### Running Tests
```powershell
# All tests
.\build.ps1 -Action test

# Specific project
dotnet test src/Idasen.BluetoothLE.Core.Tests

# Exclude integration tests
dotnet test --filter "Category!=Integration"

# With coverage
dotnet test /p:CollectCoverage=true
```

## Documentation

### Documentation Updates
When contributing, update relevant documentation:

1. **XML comments** for public APIs
2. **README.md** for major features
3. **Wiki pages** for detailed documentation
4. **Code examples** in the ConsoleApp project

### Documentation Style
- Use clear, concise language
- Include code examples where helpful
- Link to related documentation
- Keep formatting consistent

## Getting Help

### Resources
- **GitHub Discussions**: Ask questions and discuss ideas
- **GitHub Issues**: Report bugs or request features
- **Wiki**: Read comprehensive documentation
- **Source Code**: Review examples in ConsoleApp

### Contact
- Create an issue on GitHub
- Start a discussion on GitHub
- Tag maintainers in your PR for urgent matters

## Recognition

Contributors will be recognized in:
- GitHub contributors list
- Release notes (for significant contributions)
- README.md (for major features)

Thank you for contributing to Idasen Desk Core! 🎉

---

**Questions?** Open an issue or discussion on [GitHub](https://github.com/tschroedter/idasen-desk-core).
