# API Reference

This page provides detailed documentation of the main APIs in the Idasen Desk Core library.

## Core Interfaces

### Device Discovery

#### IDevices
Main interface for discovering and managing Bluetooth LE devices.

```csharp
public interface IDevices
{
    // Observables
    IObservable<IDevice> DeviceDiscovered { get; }
    IObservable<IDevice> DeviceLost { get; }
    IObservable<IDevice> DeviceUpdated { get; }
    
    // Properties
    IEnumerable<IDevice> DetectedDevices { get; }
    
    // Methods
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
var devices = container.Resolve<IDevices>();

// Subscribe to device discovery
devices.DeviceDiscovered.Subscribe(device =>
{
    Console.WriteLine($"Found: {device.Name} - {device.Address}");
});

// Start discovering
await devices.StartAsync();
```

#### IDevice
Represents a discovered Bluetooth LE device.

```csharp
public interface IDevice
{
    // Properties
    string Name { get; }
    ulong Address { get; }
    DeviceInformation DeviceInformation { get; }
    IObservable<ConnectionStatus> ConnectionStatusChanged { get; }
    
    // Methods
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
var desk = devices.DetectedDevices.FirstOrDefault();
if (desk != null)
{
    await desk.ConnectAsync();
    
    desk.ConnectionStatusChanged.Subscribe(status =>
    {
        Console.WriteLine($"Connection status: {status}");
    });
}
```

### Service Discovery

#### IGattServices
Interface for discovering GATT services on a connected device.

```csharp
public interface IGattServices
{
    Task<IEnumerable<IGattService>> DiscoverAsync(
        IDevice device,
        CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
var gattServices = container.Resolve<IGattServices>();
var services = await gattServices.DiscoverAsync(device);

foreach (var service in services)
{
    Console.WriteLine($"Service: {service.Uuid}");
}
```

#### IGattService
Represents a GATT service with characteristics.

```csharp
public interface IGattService
{
    Guid Uuid { get; }
    string Name { get; }
    IEnumerable<IGattCharacteristic> Characteristics { get; }
}
```

### Characteristics

#### ICharacteristic
Base interface for BLE characteristic operations.

```csharp
public interface ICharacteristic
{
    Guid Uuid { get; }
    IObservable<byte[]> ValueChanged { get; }
    
    Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
    Task WriteAsync(byte[] value, CancellationToken cancellationToken = default);
    Task<bool> SubscribeToNotificationsAsync(
        CancellationToken cancellationToken = default);
    Task UnsubscribeFromNotificationsAsync(
        CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
// Read characteristic value
var value = await characteristic.ReadAsync();

// Write characteristic value
await characteristic.WriteAsync(new byte[] { 0x01, 0x02 });

// Subscribe to notifications
await characteristic.SubscribeToNotificationsAsync();
characteristic.ValueChanged.Subscribe(data =>
{
    Console.WriteLine($"Value changed: {BitConverter.ToString(data)}");
});
```

## Linak Protocol (Idasen Desk)

### ILinakDesk
High-level interface for controlling an Idasen desk (specific implementation depends on project).

```csharp
public interface ILinakDesk
{
    // Properties
    IObservable<ushort> HeightChanged { get; }
    IObservable<DeskMovementStatus> MovementStatusChanged { get; }
    ushort MinHeight { get; }
    ushort MaxHeight { get; }
    ushort CurrentHeight { get; }
    
    // Methods
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task MoveToHeightAsync(ushort heightMm, CancellationToken cancellationToken = default);
    Task MoveUpAsync(CancellationToken cancellationToken = default);
    Task MoveDownAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
```

**Usage Example**:
```csharp
var desk = container.Resolve<ILinakDesk>();

// Connect to desk
await desk.ConnectAsync();

// Subscribe to height changes
desk.HeightChanged.Subscribe(height =>
{
    Console.WriteLine($"Height: {height}mm");
});

// Move to specific height
await desk.MoveToHeightAsync(1000); // Move to 1000mm

// Move up/down
await desk.MoveUpAsync();
await Task.Delay(2000);
await desk.StopAsync();
```

### DeskMovementStatus
Enum representing desk movement states.

```csharp
public enum DeskMovementStatus
{
    Stopped,
    MovingUp,
    MovingDown,
    Error
}
```

## Device Monitoring

### IDeviceMonitor
Monitors device availability and status.

```csharp
public interface IDeviceMonitor
{
    IObservable<IDevice> DeviceAdded { get; }
    IObservable<IDevice> DeviceRemoved { get; }
    IObservable<IDevice> DeviceStatusChanged { get; }
    
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync(CancellationToken cancellationToken = default);
}
```

### IWatcher
Low-level Bluetooth LE device watcher.

```csharp
public interface IWatcher
{
    IObservable<DeviceInformation> Added { get; }
    IObservable<DeviceInformationUpdate> Updated { get; }
    IObservable<DeviceInformationUpdate> Removed { get; }
    
    void Start();
    void Stop();
}
```

## Aspect-Oriented Programming

### LogAspect
Automatic logging for method entry, exit, and duration.

**Usage**:
```csharp
[Intercept(typeof(LogAspect))]
public interface IMyService
{
    Task DoSomethingAsync();
}
```

**Logged Information**:
- Method name
- Parameter values (masked if sensitive)
- Return value
- Execution duration
- Exceptions

### LogExceptionAspect
Automatic exception logging and rethrowing.

**Usage**:
```csharp
[Intercept(typeof(LogExceptionAspect))]
public interface IMyService
{
    Task RiskyOperationAsync();
}
```

### IInvocationToTextConverter
Converts method invocations to readable strings for logging.

```csharp
public interface IInvocationToTextConverter
{
    string Convert(IInvocation invocation);
}
```

## Dependency Injection

### Container Setup
```csharp
using Autofac;

var builder = new ContainerBuilder();

// Register core modules
builder.RegisterModule<BluetoothLeModule>();
builder.RegisterModule<LinakModule>();
builder.RegisterModule<AopModule>();

// Build container
var container = builder.Build();

// Resolve services
using var scope = container.BeginLifetimeScope();
var devices = scope.Resolve<IDevices>();
```

### Module Registration
```csharp
public class MyModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<MyService>()
            .As<IMyService>()
            .SingleInstance()
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(LogAspect));
    }
}
```

## Logging

### Serilog Configuration
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/idasen-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

### Structured Logging
```csharp
// Use structured logging with named properties
Log.Information("Device {DeviceName} connected at {Height}mm", 
    device.Name, desk.CurrentHeight);

// Context enrichment
using (LogContext.PushProperty("DeskId", deskId))
{
    Log.Information("Processing desk operation");
}
```

## Data Types

### DeviceInformation
Windows-provided device information.

```csharp
public class DeviceInformation
{
    public string Id { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public bool IsDefault { get; }
    // ... other Windows-specific properties
}
```

### ConnectionStatus
Enum for device connection status.

```csharp
public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting
}
```

## Error Handling

### Exceptions

#### Common Exceptions
```csharp
// Device not found
throw new DeviceNotFoundException("Device not found");

// Connection failed
throw new ConnectionException("Failed to connect to device");

// Operation timeout
throw new TimeoutException("Operation timed out");

// Invalid operation state
throw new InvalidOperationException("Device is not connected");
```

#### Exception Handling Example
```csharp
try
{
    await desk.ConnectAsync();
}
catch (DeviceNotFoundException ex)
{
    Log.Error(ex, "Desk not found");
}
catch (ConnectionException ex)
{
    Log.Error(ex, "Failed to connect to desk");
}
catch (TimeoutException ex)
{
    Log.Warning(ex, "Connection timed out, will retry");
}
```

## Reactive Extensions Patterns

### Subscribing to Events
```csharp
// Simple subscription
var subscription = observable.Subscribe(
    onNext: value => Console.WriteLine($"Next: {value}"),
    onError: error => Console.WriteLine($"Error: {error}"),
    onCompleted: () => Console.WriteLine("Completed")
);

// Dispose when done
subscription.Dispose();
```

### Combining Observables
```csharp
// Merge multiple observables
var allEvents = Observable.Merge(
    devices.DeviceDiscovered,
    devices.DeviceUpdated
);

// Filter events
var idasenDesks = devices.DeviceDiscovered
    .Where(d => d.Name.Contains("Idasen"));

// Transform events
var heights = desk.HeightChanged
    .Select(h => h / 10.0) // Convert mm to cm
    .DistinctUntilChanged();
```

### Throttling and Buffering
```csharp
// Throttle rapid updates
var throttled = desk.HeightChanged
    .Throttle(TimeSpan.FromMilliseconds(100));

// Buffer events
var buffered = desk.HeightChanged
    .Buffer(TimeSpan.FromSeconds(1));
```

## Best Practices

### Resource Management
```csharp
// Always dispose of subscriptions
var subscription = observable.Subscribe(...);
try
{
    // Use subscription
}
finally
{
    subscription.Dispose();
}

// Or use using statement
using var subscription = observable.Subscribe(...);

// Lifetime scopes
using var scope = container.BeginLifetimeScope();
var service = scope.Resolve<IService>();
```

### Async/Await
```csharp
// Always use async/await for I/O
await device.ConnectAsync();

// Use CancellationToken for long operations
await device.ConnectAsync(cancellationToken);

// Don't block on async code
// BAD: device.ConnectAsync().Wait();
// GOOD: await device.ConnectAsync();
```

### Error Handling
```csharp
// Handle specific exceptions
try
{
    await operation();
}
catch (SpecificException ex) when (ex.ErrorCode == 123)
{
    // Handle specific error
}

// Always log errors
catch (Exception ex)
{
    Log.Error(ex, "Operation failed");
    throw;
}
```

## Extension Methods

### Useful Extensions
```csharp
// Converting byte arrays
byte[] bytes = heightMm.ToByteArray();
ushort value = bytes.ToUInt16();

// Masking sensitive data
string masked = sensitiveData.Mask();

// Guard clauses
Guard.AgainstNull(parameter, nameof(parameter));
Guard.AgainstNegative(value, nameof(value));
```

## Configuration

### App Configuration
```csharp
// appsettings.json
{
  "Bluetooth": {
    "ScanTimeout": "00:00:30",
    "ConnectionTimeout": "00:00:10"
  },
  "Desk": {
    "MinHeight": 620,
    "MaxHeight": 1270,
    "DefaultHeight": 1000
  }
}
```

### Reading Configuration
```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var scanTimeout = config.GetValue<TimeSpan>("Bluetooth:ScanTimeout");
var minHeight = config.GetValue<ushort>("Desk:MinHeight");
```

---

For more examples and use cases, see the [Getting Started Guide](Getting-Started.md) and the [ConsoleApp example](https://github.com/tschroedter/idasen-desk-core/tree/main/src/Idasen.ConsoleApp).
