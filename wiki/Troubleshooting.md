# Troubleshooting Guide

This guide helps you diagnose and resolve common issues when working with the Idasen Desk Core library.

## Table of Contents
- [Device Discovery Issues](#device-discovery-issues)
- [Connection Problems](#connection-problems)
- [Height Control Issues](#height-control-issues)
- [Build and Compilation Errors](#build-and-compilation-errors)
- [Test Failures](#test-failures)
- [Bluetooth Problems](#bluetooth-problems)
- [Performance Issues](#performance-issues)

## Device Discovery Issues

### Problem: Desk Not Discovered

#### Symptoms
- `DetectedDevices` collection is empty
- `DeviceDiscovered` observable never fires
- Application can't find the desk

#### Solutions

**1. Check Bluetooth Status**
```powershell
# PowerShell: Check Bluetooth adapters
Get-PnpDevice -Class Bluetooth

# Expected output should show Bluetooth devices
```

**2. Verify Desk is Powered On**
- Ensure desk is plugged in and powered on
- Check desk control panel shows normal operation
- Try pressing buttons on desk control panel

**3. Check Bluetooth Range**
- Move computer closer to desk
- Ensure no metal barriers between computer and desk
- Ideal distance: < 10 feet (3 meters)

**4. Unpair Desk from Other Devices**
- Disconnect desk from Ikea app
- Remove desk from other Bluetooth connections
- Restart desk (unplug and plug back in)

**5. Enable Bluetooth in Windows**
```powershell
# Check Windows Bluetooth status
Get-Service bthserv

# If not running, start it
Start-Service bthserv
```

**6. Increase Discovery Timeout**
```csharp
// Allow more time for discovery
await devices.StartAsync();
await Task.Delay(TimeSpan.FromSeconds(10)); // Wait longer

var desk = devices.DetectedDevices.FirstOrDefault();
```

### Problem: Multiple Devices Detected

#### Symptoms
- Multiple devices appear in `DetectedDevices`
- Unable to identify correct desk

#### Solutions

**1. Filter by Device Name**
```csharp
var idasenDesks = devices.DetectedDevices
    .Where(d => d.Name.Contains("Desk") || 
                d.Name.Contains("Idasen"))
    .ToList();
```

**2. Filter by Service UUID**
```csharp
// Idasen desks typically advertise specific GATT services
var desk = devices.DetectedDevices
    .FirstOrDefault(d => d.AdvertisedServices
        .Any(s => s == LinakServiceUuid));
```

**3. Remember Device Address**
```csharp
// Save the address once identified
var knownDeskAddress = 0x123456789ABC;

var myDesk = devices.DetectedDevices
    .FirstOrDefault(d => d.Address == knownDeskAddress);
```

### Problem: Desk Discovered but Immediately Lost

#### Symptoms
- `DeviceDiscovered` fires
- `DeviceLost` fires immediately after
- Can't maintain device in list

#### Solutions

**1. Check Signal Strength**
- Weak signal can cause intermittent detection
- Move closer to desk
- Remove interference sources (WiFi routers, microwaves)

**2. Increase Device Expiry Timeout**
```csharp
// Configure longer expiry time for devices
var monitorConfig = new DeviceMonitorConfig
{
    ExpiryTimeout = TimeSpan.FromMinutes(5)
};
```

## Connection Problems

### Problem: Connection Fails

#### Symptoms
- `ConnectAsync()` throws exception
- Connection timeout
- `ConnectionStatusChanged` shows disconnected

#### Solutions

**1. Check Desk is Not Already Connected**
```csharp
// Disconnect first if already connected
if (device.IsConnected)
{
    await device.DisconnectAsync();
    await Task.Delay(1000); // Wait before reconnecting
}

await device.ConnectAsync();
```

**2. Verify Bluetooth Permissions**
- Windows Settings → Privacy → Bluetooth
- Ensure app has permission to use Bluetooth

**3. Restart Bluetooth Service**
```powershell
# PowerShell: Restart Bluetooth
Restart-Service bthserv
```

**4. Add Retry Logic**
```csharp
async Task<bool> ConnectWithRetry(IDevice device, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await device.ConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Connection attempt {Attempt} failed", i + 1);
            if (i < maxRetries - 1)
                await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
    return false;
}
```

### Problem: Connection Drops Unexpectedly

#### Symptoms
- Connected successfully
- Connection lost during operation
- Frequent disconnections

#### Solutions

**1. Monitor Connection Status**
```csharp
device.ConnectionStatusChanged.Subscribe(status =>
{
    Log.Information("Connection status: {Status}", status);
    
    if (status == ConnectionStatus.Disconnected)
    {
        // Attempt reconnection
        _ = ReconnectAsync(device);
    }
});
```

**2. Check Power Management**
- Windows may put Bluetooth to sleep
- Windows Settings → Bluetooth & Devices → Settings
- Disable "Allow Windows to turn off Bluetooth to save power"

**3. Keep Connection Active**
```csharp
// Periodically read a characteristic to keep connection alive
var keepAliveTimer = Observable
    .Interval(TimeSpan.FromSeconds(30))
    .Subscribe(async _ =>
    {
        if (device.IsConnected)
        {
            await device.ReadHeightAsync(); // or similar
        }
    });
```

### Problem: Cannot Connect After Restart

#### Symptoms
- App worked before
- After restart, cannot connect
- Desk appears but connection fails

#### Solutions

**1. Clear Bluetooth Cache**
```powershell
# PowerShell: Clear and restart
Stop-Service bthserv
# Manually delete: C:\Windows\System32\config\systemprofile\AppData\Local\Microsoft\Windows\Bluetooth
Start-Service bthserv
```

**2. Re-discover Device**
```csharp
// Don't rely on cached device information
await devices.StopAsync();
await Task.Delay(1000);
await devices.StartAsync();
// Wait for fresh discovery
```

## Height Control Issues

### Problem: Height Not Updating

#### Symptoms
- `MoveToHeightAsync()` completes but desk doesn't move
- `CurrentHeight` doesn't change
- No movement observed

#### Solutions

**1. Verify Connection**
```csharp
if (!desk.IsConnected)
{
    await desk.ConnectAsync();
}

await desk.MoveToHeightAsync(targetHeight);
```

**2. Check Height Range**
```csharp
// Ensure height is within valid range
var targetHeight = Math.Clamp(desiredHeight, desk.MinHeight, desk.MaxHeight);
await desk.MoveToHeightAsync(targetHeight);
```

**3. Subscribe to Height Updates**
```csharp
desk.HeightChanged.Subscribe(height =>
{
    Log.Information("Current height: {Height}mm", height);
});

await desk.MoveToHeightAsync(1000);
```

### Problem: Desk Moves But Stops Prematurely

#### Symptoms
- Desk starts moving
- Stops before reaching target height
- No error thrown

#### Solutions

**1. Check for Obstacles**
- Physical obstacle blocking desk
- Desk safety feature engaged
- Check desk error LED

**2. Monitor Movement Status**
```csharp
desk.MovementStatusChanged.Subscribe(status =>
{
    Log.Information("Movement status: {Status}", status);
    
    if (status == DeskMovementStatus.Error)
    {
        Log.Error("Desk encountered an error");
    }
});
```

**3. Increase Command Timeout**
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await desk.MoveToHeightAsync(targetHeight, cts.Token);
```

## Build and Compilation Errors

### Problem: Cannot Build Solution

#### Error: "SDK not found"
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 8.0
```

**Solution**: Install .NET 8.0 SDK
```powershell
# Download from https://dotnet.microsoft.com/download/dotnet/8.0
# Or use winget
winget install Microsoft.DotNet.SDK.8
```

#### Error: "Windows SDK not found"
```
error: The Windows SDK version 10.0.19041.0 was not found
```

**Solution**: Install Windows SDK
```powershell
# Visual Studio Installer → Modify
# Select "Windows 10 SDK (10.0.19041.0)"
```

#### Error: "Package restore failed"
```
error NU1102: Unable to find package 'PackageName'
```

**Solution**: Clear NuGet cache
```powershell
dotnet nuget locals all --clear
dotnet restore
```

### Problem: Code Formatting Errors

#### Error: "Code is not formatted correctly"

**Solution**: Format code
```powershell
# Fix formatting
dotnet format

# Or use build script
.\build.ps1 -Action format
```

### Problem: Analyzer Warnings

#### Warning: CA1234, SA5678, etc.

**Solutions**:

**1. Fix the Warning**
```csharp
// Follow the suggested fix in the warning message
```

**2. Suppress if False Positive**
```csharp
#pragma warning disable CA1234
// Code here
#pragma warning restore CA1234
```

**3. Configure in .editorconfig**
```ini
[*.cs]
# Disable specific rule
dotnet_diagnostic.CA1234.severity = none
```

## Test Failures

### Problem: Unit Tests Fail

#### Symptoms
- Tests fail locally
- CI tests pass or vice versa

#### Solutions

**1. Clean and Rebuild**
```powershell
.\build.ps1 -Action clean
.\build.ps1 -Action build
.\build.ps1 -Action test
```

**2. Check Test Dependencies**
```csharp
// Verify all dependencies are properly mocked
var mockService = Substitute.For<IService>();
// Configure mock properly
```

**3. Run Tests in Isolation**
```powershell
# Run single test to isolate issue
dotnet test --filter "FullyQualifiedName~SpecificTest"
```

### Problem: Integration Tests Fail

#### Error: "Bluetooth adapter not found"

**Solution**: Ensure Bluetooth hardware present
```powershell
# Check for Bluetooth
Get-PnpDevice -Class Bluetooth

# Integration tests require hardware
dotnet test --filter "Category!=Integration"
```

#### Error: "Device not found"

**Solution**: Ensure desk is available
- Desk powered on
- Bluetooth enabled
- Desk in range
- Not connected to other devices

## Bluetooth Problems

### Problem: Bluetooth Adapter Not Working

#### Symptoms
- No Bluetooth devices found
- Adapter shows error in Device Manager
- Services fail to start

#### Solutions

**1. Update Bluetooth Drivers**
- Device Manager → Bluetooth
- Right-click adapter → Update driver
- Restart computer

**2. Reinstall Bluetooth Drivers**
- Device Manager → Bluetooth
- Uninstall device
- Restart computer (auto-reinstalls)

**3. Check Windows Services**
```powershell
# Ensure Bluetooth service is running
Get-Service bthserv
Start-Service bthserv -ErrorAction SilentlyContinue
```

### Problem: Bluetooth Permission Denied

#### Error: "Access denied" when accessing Bluetooth

**Solution**: Grant Bluetooth permissions
1. Windows Settings → Privacy & Security
2. Bluetooth → On
3. Find your app → Allow Bluetooth access

### Problem: Multiple Bluetooth Adapters

#### Symptoms
- Multiple adapters in Device Manager
- Unpredictable behavior

#### Solutions

**1. Disable Extra Adapters**
- Keep only primary adapter enabled
- Device Manager → Disable unused adapters

**2. Specify Adapter** (if library supports)
```csharp
// Configure specific adapter (depends on implementation)
var config = new BluetoothConfig
{
    AdapterId = "specific-adapter-id"
};
```

## Performance Issues

### Problem: Slow Device Discovery

#### Symptoms
- Takes long time to find devices
- High CPU usage during discovery

#### Solutions

**1. Limit Discovery Duration**
```csharp
await devices.StartAsync();
await Task.Delay(TimeSpan.FromSeconds(5)); // Limit discovery time
await devices.StopAsync();
```

**2. Filter Devices Early**
```csharp
devices.DeviceDiscovered
    .Where(d => d.Name.Contains("Desk"))
    .Subscribe(desk => 
    {
        // Process only relevant devices
    });
```

### Problem: High Memory Usage

#### Symptoms
- Memory usage increases over time
- Eventually runs out of memory

#### Solutions

**1. Dispose Subscriptions**
```csharp
// Always dispose
using var subscription = observable.Subscribe(...);

// Or manually
var subscription = observable.Subscribe(...);
subscription.Dispose();
```

**2. Dispose Device Resources**
```csharp
// Implement IDisposable
public void Dispose()
{
    subscription?.Dispose();
    device?.DisconnectAsync().Wait();
}
```

**3. Use Lifetime Scopes**
```csharp
using var scope = container.BeginLifetimeScope();
var service = scope.Resolve<IService>();
// Resources cleaned up when scope disposed
```

## Logging and Diagnostics

### Enable Verbose Logging

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.File("logs/debug-.txt", 
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Capture Bluetooth Events

```csharp
// Log all Bluetooth events
devices.DeviceDiscovered.Subscribe(d => 
    Log.Debug("Discovered: {Name}", d.Name));
devices.DeviceLost.Subscribe(d => 
    Log.Debug("Lost: {Name}", d.Name));
devices.DeviceUpdated.Subscribe(d => 
    Log.Debug("Updated: {Name}", d.Name));
```

### Windows Event Viewer

1. Open Event Viewer
2. Applications and Services Logs → Microsoft → Windows
3. Check:
   - Bluetooth
   - Bluetooth-BthLEPrepairing
   - Bluetooth-MTPEnum

## Getting More Help

### Before Asking for Help

1. **Check this guide** for your specific issue
2. **Search existing issues** on GitHub
3. **Enable verbose logging** and review logs
4. **Reproduce the issue** consistently
5. **Gather system information**:
   - Windows version
   - .NET SDK version
   - Bluetooth adapter model
   - Desk model

### Creating an Issue

Include:
- **Clear description** of the problem
- **Steps to reproduce**
- **Expected behavior** vs actual behavior
- **Environment details**
- **Log output** (use verbose logging)
- **Code sample** if possible

### Where to Get Help

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and community support
- **Wiki Documentation**: Comprehensive guides
- **Source Code**: Review examples in ConsoleApp

## Additional Resources

- [Getting Started Guide](Getting-Started.md)
- [Development Guide](Development-Guide.md)
- [API Reference](API-Reference.md)
- [Testing Guide](Testing-Guide.md)

---

**Still having issues?** Open an issue on [GitHub](https://github.com/tschroedter/idasen-desk-core/issues) with detailed information.
