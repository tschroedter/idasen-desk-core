# Testing Guide

This guide provides comprehensive information about testing in the Idasen Desk Core project.

## Table of Contents
- [Testing Philosophy](#testing-philosophy)
- [Test Types](#test-types)
- [Running Tests](#running-tests)
- [Writing Tests](#writing-tests)
- [Test Organization](#test-organization)
- [Mocking and Fakes](#mocking-and-fakes)
- [Code Coverage](#code-coverage)

## Testing Philosophy

### Testing Principles

The project follows these testing principles:

1. **High Coverage**: Aim for 80%+ code coverage
2. **Fast Tests**: Unit tests should run in milliseconds
3. **Reliable Tests**: Tests should not be flaky
4. **Independent Tests**: Each test should be independent
5. **Readable Tests**: Tests should be easy to understand
6. **Maintainable Tests**: Tests should be easy to maintain

### Test Pyramid

```
        /\
       /  \      E2E/Integration (Few)
      /____\
     /      \    Integration (Some)
    /________\
   /          \  Unit (Many)
  /____________\
```

- **Many Unit Tests**: Fast, focused, isolated
- **Some Integration Tests**: Test component interactions
- **Few E2E Tests**: Test complete scenarios (require hardware)

## Test Types

### Unit Tests

**Purpose**: Test individual components in isolation

**Characteristics**:
- Fast execution (< 100ms per test)
- No external dependencies
- Use mocks/fakes for dependencies
- Test single responsibility

**Example Projects**:
- `Idasen.Aop.Tests`
- `Idasen.BluetoothLE.Core.Tests`
- `Idasen.BluetoothLE.Characteristics.Tests`
- `Idasen.BluetoothLE.Linak.Tests`

**Example**:
```csharp
public class DeviceComparerTests
{
    [Fact]
    public void Equals_SameAddress_ReturnsTrue()
    {
        // Arrange
        var device1 = new Device { Address = 123 };
        var device2 = new Device { Address = 123 };
        var comparer = new DeviceComparer();
        
        // Act
        var result = comparer.Equals(device1, device2);
        
        // Assert
        Assert.True(result);
    }
}
```

### Integration Tests

**Purpose**: Test interactions between components

**Characteristics**:
- Slower execution (can be several seconds)
- May require real Bluetooth hardware
- Test component integration
- Verify external dependencies work correctly

**Example Project**:
- `Idasen.BluetoothLE.Integration.Tests`

**Example**:
```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task ConnectToDesk_WithRealHardware_Succeeds()
{
    // Arrange
    var devices = CreateDevices();
    await devices.StartAsync();
    
    // Wait for discovery
    await Task.Delay(TimeSpan.FromSeconds(5));
    
    var desk = devices.DetectedDevices
        .FirstOrDefault(d => d.Name.Contains("Desk"));
    
    // Act
    var result = await desk.ConnectAsync();
    
    // Assert
    Assert.True(result);
}
```

### Smoke Tests

**Purpose**: Quick sanity checks that basic functionality works

**Example**:
```csharp
[Fact]
public void Constructor_CreatesInstance_NotNull()
{
    // Act
    var device = new Device();
    
    // Assert
    Assert.NotNull(device);
}
```

## Running Tests

### Using PowerShell Build Script

```powershell
# Run all tests
.\build.ps1 -Action test

# The script runs tests with these parameters:
# --configuration Release
# --no-build
# Generates coverage reports
```

### Using .NET CLI

```powershell
# Run all tests
dotnet test

# Run tests in specific project
dotnet test src/Idasen.BluetoothLE.Core.Tests

# Run with specific configuration
dotnet test --configuration Debug

# Run tests without building
dotnet test --no-build

# Verbose output
dotnet test --verbosity detailed
```

### Using Visual Studio

#### Run All Tests
- Test → Run All Tests (Ctrl+R, A)

#### Run Specific Test
- Right-click test method → Run Test(s)
- Click play button next to test in Test Explorer

#### Debug Test
- Right-click test method → Debug Test(s)
- Set breakpoints in test or code under test

### Using Rider

#### Run Tests
- Click run icon next to test class or method
- Use keyboard shortcut (Ctrl+Shift+F10)

#### Debug Tests
- Click debug icon next to test
- Set breakpoints as needed

### Filtering Tests

#### By Category
```powershell
# Run only unit tests (exclude integration)
dotnet test --filter "Category!=Integration"

# Run only integration tests
dotnet test --filter "Category=Integration"
```

#### By Name
```powershell
# Run tests with "Device" in name
dotnet test --filter "FullyQualifiedName~Device"

# Run specific test class
dotnet test --filter "FullyQualifiedName~DeviceTests"
```

#### By Trait
```csharp
[Fact]
[Trait("Category", "Fast")]
public void FastTest() { }

[Fact]
[Trait("Category", "Slow")]
public void SlowTest() { }
```

```powershell
# Run only fast tests
dotnet test --filter "Category=Fast"
```

## Writing Tests

### Test Naming Conventions

#### Pattern
```
MethodName_Scenario_ExpectedBehavior
```

#### Examples
```csharp
// Good names
ConnectAsync_WhenDeviceNotFound_ThrowsDeviceNotFoundException()
GetHeight_WhenConnected_ReturnsCurrentHeight()
Equals_DifferentAddresses_ReturnsFalse()

// Bad names
TestConnect()
Test1()
ItWorks()
```

### Test Structure (AAA Pattern)

#### Arrange-Act-Assert
```csharp
[Fact]
public async Task MoveToHeight_ValidHeight_UpdatesPosition()
{
    // Arrange - Set up test data and dependencies
    var desk = CreateDesk();
    var targetHeight = 1000;
    
    // Act - Execute the method under test
    await desk.MoveToHeightAsync(targetHeight);
    
    // Assert - Verify the expected outcome
    Assert.Equal(targetHeight, desk.CurrentHeight);
}
```

### Common Test Patterns

#### Testing Exceptions
```csharp
[Fact]
public async Task ConnectAsync_NullDevice_ThrowsArgumentNullException()
{
    // Arrange
    var service = CreateService();
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => service.ConnectAsync(null));
}
```

#### Testing with Theory
```csharp
[Theory]
[InlineData(500, false)]   // Below min
[InlineData(750, true)]    // Valid
[InlineData(1500, false)]  // Above max
public void IsValidHeight_VariousHeights_ReturnsExpected(
    int height, bool expected)
{
    // Arrange
    var validator = new HeightValidator(minHeight: 620, maxHeight: 1270);
    
    // Act
    var result = validator.IsValidHeight(height);
    
    // Assert
    Assert.Equal(expected, result);
}
```

#### Testing Observables
```csharp
[Fact]
public async Task DeviceDiscovered_WhenDeviceFound_RaisesEvent()
{
    // Arrange
    var devices = CreateDevices();
    IDevice discoveredDevice = null;
    
    devices.DeviceDiscovered.Subscribe(device =>
    {
        discoveredDevice = device;
    });
    
    // Act
    await devices.StartAsync();
    await Task.Delay(TimeSpan.FromSeconds(2));
    
    // Assert
    Assert.NotNull(discoveredDevice);
}
```

#### Testing Async Code
```csharp
[Fact]
public async Task ConnectAsync_ValidDevice_CompletesSuccessfully()
{
    // Arrange
    var device = CreateDevice();
    
    // Act
    await device.ConnectAsync();
    
    // Assert
    Assert.True(device.IsConnected);
}
```

### Test Data Builders

Create helper methods to build test data:

```csharp
public class DeviceTestBuilder
{
    private string _name = "Test Device";
    private ulong _address = 123456789;
    
    public DeviceTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public DeviceTestBuilder WithAddress(ulong address)
    {
        _address = address;
        return this;
    }
    
    public IDevice Build()
    {
        return new Device
        {
            Name = _name,
            Address = _address
        };
    }
}

// Usage
var device = new DeviceTestBuilder()
    .WithName("Idasen Desk")
    .WithAddress(987654321)
    .Build();
```

## Test Organization

### File Structure

```
Idasen.BluetoothLE.Core.Tests/
├── DevicesDiscovery/
│   ├── DevicesTests.cs
│   ├── DeviceTests.cs
│   ├── DeviceMonitorTests.cs
│   └── WatcherTests.cs
├── ServicesDiscovery/
│   ├── GattServicesTests.cs
│   └── GattServicesProviderTests.cs
├── Helpers/
│   ├── TestDataBuilder.cs
│   └── MockHelper.cs
└── GuardTests.cs
```

### Test Class Organization

```csharp
public class DeviceTests
{
    // Test fixture / shared setup
    private readonly ITestOutputHelper _output;
    
    public DeviceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    // Happy path tests
    [Fact]
    public void Method_ValidInput_ReturnsExpected() { }
    
    // Edge cases
    [Fact]
    public void Method_EdgeCase_HandlesCorrectly() { }
    
    // Error cases
    [Fact]
    public void Method_InvalidInput_ThrowsException() { }
    
    // Helper methods
    private Device CreateDevice() => new Device();
}
```

## Mocking and Fakes

### Using NSubstitute

#### Basic Mocking
```csharp
[Fact]
public async Task Test_WithMockedDependency_Works()
{
    // Arrange
    var mockLogger = Substitute.For<ILogger>();
    var service = new MyService(mockLogger);
    
    // Act
    await service.DoSomethingAsync();
    
    // Assert
    mockLogger.Received(1).Information(Arg.Any<string>());
}
```

#### Return Values
```csharp
var mockDevice = Substitute.For<IDevice>();
mockDevice.Name.Returns("Test Device");
mockDevice.Address.Returns(123456789ul);
```

#### Async Methods
```csharp
var mockService = Substitute.For<IService>();
mockService.GetDataAsync().Returns(Task.FromResult(42));
```

#### Throwing Exceptions
```csharp
var mockDevice = Substitute.For<IDevice>();
mockDevice.ConnectAsync()
    .Returns(Task.FromException<bool>(
        new ConnectionException("Connection failed")));
```

#### Callbacks
```csharp
var mockDevice = Substitute.For<IDevice>();
mockDevice.When(x => x.ConnectAsync())
    .Do(x => Console.WriteLine("Connect called"));
```

### Test Doubles

#### Fake Implementation
```csharp
public class FakeDeviceMonitor : IDeviceMonitor
{
    private readonly Subject<IDevice> _deviceAdded = new();
    
    public IObservable<IDevice> DeviceAdded => _deviceAdded;
    
    public void SimulateDeviceAdded(IDevice device)
    {
        _deviceAdded.OnNext(device);
    }
}
```

## Code Coverage

### Generating Coverage

#### Using dotnet test
```powershell
# Generate coverage
dotnet test /p:CollectCoverage=true

# Specify format
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Exclude files
dotnet test /p:CollectCoverage=true /p:Exclude="[*.Tests]*"
```

#### Using Build Script
```powershell
.\build.ps1 -Action test
# Coverage is automatically generated
```

### Viewing Coverage

#### Generate HTML Report
```powershell
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report
reportgenerator `
    -reports:coverage.cobertura.xml `
    -targetdir:coverage-report `
    -reporttypes:Html

# Open report
Start-Process coverage-report/index.html
```

#### Visual Studio Coverage
1. Test → Analyze Code Coverage for All Tests
2. View results in Code Coverage Results window
3. Use Coverage Highlighting to see covered/uncovered lines

#### SonarCloud
- View coverage on SonarCloud dashboard
- Tracks coverage over time
- Shows coverage per file and per line

### Coverage Goals

#### Target Coverage
- **Overall**: 80%+
- **Core Business Logic**: 90%+
- **UI/Infrastructure**: 70%+

#### What to Cover
- All business logic
- Edge cases and error handling
- Critical paths
- Public APIs

#### What Not to Cover
- Auto-generated code
- Simple getters/setters
- Framework code
- Third-party libraries

## Testing Best Practices

### Do's ✅

1. **Write Clear Tests**: Test name should describe what is being tested
2. **Test One Thing**: Each test should verify one behavior
3. **Keep Tests Small**: Short, focused tests are easier to maintain
4. **Use AAA Pattern**: Arrange-Act-Assert structure
5. **Mock External Dependencies**: Isolate the code under test
6. **Clean Up Resources**: Dispose of resources properly
7. **Test Edge Cases**: Don't just test happy paths

### Don'ts ❌

1. **Don't Test Implementation**: Test behavior, not implementation details
2. **Don't Share State**: Avoid static state or shared test data
3. **Don't Test Framework Code**: Don't test third-party library code
4. **Don't Make Tests Flaky**: Avoid random data, timing issues
5. **Don't Ignore Failing Tests**: Fix or remove failing tests
6. **Don't Couple Tests**: Tests should be independent

## Continuous Integration

### GitHub Actions

Tests run automatically on:
- Every push to a branch
- Every pull request
- Scheduled (if configured)

#### View Results
1. Go to repository on GitHub
2. Click "Actions" tab
3. Select workflow run
4. View test results in "Tests" section

#### Test Summary
GitHub Actions provides a test summary showing:
- Total tests run
- Passed/Failed/Skipped
- Test duration
- Coverage changes

## Troubleshooting Tests

### Common Issues

#### Tests Timeout
```csharp
// Increase timeout for slow tests
[Fact(Timeout = 10000)] // 10 seconds
public async Task SlowTest()
{
    // Test code
}
```

#### Tests Are Flaky
- Remove dependencies on timing (e.g., `Task.Delay`)
- Don't use random data
- Ensure proper cleanup
- Avoid shared state

#### Integration Tests Fail
- Check Bluetooth hardware is available
- Ensure desk is powered on
- Verify no other apps connected
- Check Windows Bluetooth settings

#### Mocks Not Working
- Verify interfaces are used (not concrete classes)
- Check method signatures match
- Ensure async methods return Task

### Debugging Failed Tests

```csharp
public class MyTests
{
    private readonly ITestOutputHelper _output;
    
    public MyTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void MyTest()
    {
        _output.WriteLine("Debug information");
        // Test code
    }
}
```

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [Development Guide](Development-Guide.md)
- [Contributing Guide](Contributing.md)

---

Need help with testing? Open an issue on [GitHub](https://github.com/tschroedter/idasen-desk-core/issues).
