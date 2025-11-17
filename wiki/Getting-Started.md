# Getting Started

This guide will help you get started with the Idasen Desk Core library.

## Prerequisites

Before you begin, ensure you have the following:

### System Requirements
- **Operating System**: Windows 10 version 19041 or later, or Windows 11
- **Bluetooth**: Bluetooth Low Energy (BLE) adapter
- **.NET SDK**: .NET 8.0 SDK or later
- **PowerShell**: PowerShell 5.1 or PowerShell Core 7.x

### Hardware Requirements
- **Idasen Desk**: Ikea Idasen standing desk with Bluetooth connectivity
- **Bluetooth Adapter**: Your computer must have a working Bluetooth adapter that supports BLE

### Development Tools (Optional)
- **Visual Studio 2022** or **JetBrains Rider** for IDE support
- **Git** for version control

## Installation

### Option 1: Install via NuGet Package

The easiest way to use Idasen Desk Core in your project is via NuGet:

#### Using .NET CLI
```bash
dotnet add package Idasen.Desk.Core
```

#### Using Package Manager Console
```powershell
Install-Package Idasen.Desk.Core
```

#### Using PackageReference in .csproj
```xml
<ItemGroup>
  <PackageReference Include="Idasen.Desk.Core" Version="1.0.0" />
</ItemGroup>
```

### Option 2: Build from Source

If you want to contribute or customize the library:

#### 1. Clone the Repository
```bash
git clone https://github.com/tschroedter/idasen-desk-core.git
cd idasen-desk-core
```

#### 2. Build the Solution

Using the provided PowerShell script:
```powershell
.\build.ps1 -Action build
```

Or using .NET CLI:
```bash
cd src
dotnet restore
dotnet build --configuration Release
```

#### 3. Run Tests
```powershell
# Using the build script
.\build.ps1 -Action test

# Or manually
cd src
dotnet test --configuration Release --no-build
```

#### 4. Create NuGet Package
```powershell
.\build.ps1 -Action pack
```

The NuGet package will be created in `src/Idasen.BluetoothLE.Core/bin/Release/`.

## Quick Start Example

Here's a simple example to get you started with controlling an Idasen desk:

### 1. Basic Setup

```csharp
using Idasen.BluetoothLE.Core;
using Autofac;

// Set up the dependency injection container
var builder = new ContainerBuilder();
// Register your services here
var container = builder.Build();
```

### 2. Discover Devices

```csharp
// Create a device monitor to discover Idasen desks
using var scope = container.BeginLifetimeScope();
var devices = scope.Resolve<IDevices>();

// Subscribe to device discovered events
devices.DeviceDiscovered
    .Subscribe(device => 
    {
        Console.WriteLine($"Discovered desk: {device.Name}");
    });

// Start discovering
await devices.StartAsync();
```

### 3. Connect to Desk

```csharp
// Connect to a specific desk
var desk = devices.DetectedDevices.FirstOrDefault();
if (desk != null)
{
    await desk.ConnectAsync();
    Console.WriteLine("Connected to desk!");
}
```

### 4. Control Desk Height

```csharp
// Move desk to a specific height (in millimeters)
// Note: Actual height control depends on desk characteristics
await desk.MoveToHeightAsync(1000); // Move to 1000mm
```

## Configuration

### Logging Configuration

The library uses Serilog for logging. Configure logging in your application:

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/idasen-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Bluetooth Configuration

The library automatically uses Windows' native Bluetooth LE APIs. Ensure your Bluetooth adapter is enabled and working:

1. Open Windows Settings
2. Go to **Devices** → **Bluetooth & other devices**
3. Ensure Bluetooth is turned **On**
4. Make sure your Idasen desk is powered on and in pairing mode

## Verifying Installation

To verify that everything is set up correctly:

### 1. Check Bluetooth
```powershell
# In PowerShell
Get-PnpDevice -Class Bluetooth
```

### 2. Run Integration Tests (if building from source)
```powershell
.\build.ps1 -Action test
```

### 3. Run Console App Example
```powershell
cd src/Idasen.ConsoleApp
dotnet run
```

## Next Steps

Now that you have the library installed and working:

1. **Explore the [API Reference](API-Reference.md)** to learn about available classes and methods
2. **Read the [Architecture Guide](Architecture-and-Design.md)** to understand how the library works
3. **Check out the [Development Guide](Development-Guide.md)** if you want to contribute
4. **See [Troubleshooting](Troubleshooting.md)** if you encounter any issues

## Common First-Time Issues

### Issue: Desk Not Discovered
**Solution**: Ensure the desk is powered on and Bluetooth is enabled on your Windows machine.

### Issue: Connection Fails
**Solution**: Make sure no other application is connected to the desk (e.g., Ikea's official app).

### Issue: Build Fails
**Solution**: Ensure you have .NET 8.0 SDK installed. Run `dotnet --version` to check.

### Issue: Tests Fail
**Solution**: Some integration tests require actual Bluetooth hardware. Unit tests should work without hardware.

## Additional Resources

- [API Reference](API-Reference.md) - Detailed API documentation
- [Architecture and Design](Architecture-and-Design.md) - Understanding the codebase
- [Contributing Guide](Contributing.md) - How to contribute
- [Troubleshooting](Troubleshooting.md) - Common problems and solutions

---

**Need Help?** Open an issue on [GitHub](https://github.com/tschroedter/idasen-desk-core/issues) or start a discussion.
