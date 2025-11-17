# Architecture and Design

This document provides an in-depth look at the architecture and design principles of the Idasen Desk Core library.

## Overview

The Idasen Desk Core library is built using modern .NET practices with a focus on:
- **Modularity**: Clear separation of concerns across multiple projects
- **Testability**: Heavy use of interfaces and dependency injection
- **Reactive Programming**: Event-driven architecture using System.Reactive
- **Aspect-Oriented Programming**: Cross-cutting concerns handled via interceptors
- **Platform-Specific**: Windows-only implementation using native BLE APIs

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  (Idasen.ConsoleApp, Idasen.Launcher)                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Linak Protocol Layer                            │
│         (Idasen.BluetoothLE.Linak)                          │
│  • Desk-specific implementations                             │
│  • Height control                                            │
│  • Movement commands                                         │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│           BLE Characteristics Layer                          │
│    (Idasen.BluetoothLE.Characteristics)                     │
│  • Generic BLE characteristic handling                       │
│  • Read/Write operations                                     │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Core Bluetooth Layer                            │
│         (Idasen.BluetoothLE.Core)                           │
│  • Device discovery                                          │
│  • Connection management                                     │
│  • Service discovery                                         │
│  • Windows BLE API wrappers                                  │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│          Cross-Cutting Concerns                              │
│              (Idasen.Aop)                                    │
│  • Logging aspects                                           │
│  • Exception handling                                        │
│  • Method interception                                       │
└──────────────────────────────────────────────────────────────┘
```

## Project Structure

### Idasen.BluetoothLE.Core
**Purpose**: Core Bluetooth LE functionality and device management

**Key Components**:
- **Device Discovery**
  - `IDevices`: Main interface for discovering BLE devices
  - `IWatcher`: Monitors for new devices
  - `IDeviceMonitor`: Manages device lifecycle
  
- **Service Discovery**
  - `IGattServices`: Discovers GATT services on connected devices
  - `IOfficialGattServicesCollection`: Mapping of official GATT service UUIDs
  
- **Connection Management**
  - `IDevice`: Represents a BLE device
  - Connection state tracking
  - Event-driven status updates

**Technologies**:
- Windows.Devices.Bluetooth APIs
- System.Reactive for event handling
- Autofac for dependency injection

### Idasen.BluetoothLE.Characteristics
**Purpose**: Generic BLE characteristic operations

**Key Components**:
- `ICharacteristic`: Base interface for BLE characteristics
- Generic read/write operations
- Value change notifications
- Data conversion utilities

**Pattern**: Abstract characteristic operations that can be extended for specific use cases

### Idasen.BluetoothLE.Linak
**Purpose**: Linak-specific protocol implementation (Idasen desk manufacturer)

**Key Components**:
- **Desk Control**
  - Height reading
  - Height setting
  - Movement commands (up/down)
  - Speed control
  
- **Desk State**
  - Current height
  - Min/Max height limits
  - Movement status
  - Error states

**Protocol**: Implements the proprietary Linak protocol used by Idasen desks

### Idasen.Aop
**Purpose**: Aspect-oriented programming utilities

**Key Components**:
- **LogAspect**: Automatic method entry/exit logging
- **LogExceptionAspect**: Exception logging and handling
- **InvocationToTextConverter**: Converts method invocations to readable strings
- **MaskExtensions**: Data masking for sensitive information

**Pattern**: Uses Autofac's DynamicProxy to intercept method calls

**Benefits**:
- Clean separation of logging from business logic
- Consistent logging format
- Reduced boilerplate code

### Idasen.Launcher
**Purpose**: Application initialization and bootstrapping

**Key Components**:
- `ContainerProvider`: Sets up Autofac container
- `LoggerProvider`: Configures Serilog
- Caller enrichment for structured logging
- Module registration

### Idasen.ConsoleApp
**Purpose**: Example console application

**Use**: Demonstrates how to use the library in a real application

## Design Patterns

### 1. Dependency Injection (DI)
**Framework**: Autofac

**Usage**:
```csharp
var builder = new ContainerBuilder();
builder.RegisterModule<BluetoothLeModule>();
var container = builder.Build();
```

**Benefits**:
- Testability via interface mocking
- Loose coupling between components
- Centralized configuration

### 2. Reactive Extensions (Rx)
**Framework**: System.Reactive

**Usage**:
```csharp
IObservable<IDevice> DeviceDiscovered { get; }
IObservable<IDevice> DeviceLost { get; }
IObservable<IDevice> DeviceUpdated { get; }
```

**Benefits**:
- Event-driven architecture
- Composable event streams
- Built-in error handling
- Thread safety

### 3. Aspect-Oriented Programming (AOP)
**Framework**: Autofac.Extras.DynamicProxy

**Usage**:
```csharp
[Intercept(typeof(LogAspect))]
public interface IMyService
{
    Task DoSomethingAsync();
}
```

**Benefits**:
- Cross-cutting concerns (logging, exceptions)
- Reduced code duplication
- Centralized error handling

### 4. Repository Pattern
**Application**: Device and service management

**Benefits**:
- Abstraction over data access
- Testable without hardware
- Consistent interface

### 5. Factory Pattern
**Usage**: Device and characteristic creation

**Benefits**:
- Encapsulated object creation
- Easy to mock in tests
- Flexible instantiation

## Key Architectural Decisions

### Windows-Only Implementation
**Decision**: Target Windows 10/11 exclusively

**Rationale**:
- Native Windows BLE APIs are mature and well-supported
- Avoids cross-platform abstraction complexity
- Better performance with native APIs
- Primary use case is Windows desktop apps

### Reactive Programming
**Decision**: Use System.Reactive for event handling

**Rationale**:
- Natural fit for Bluetooth events (connect, disconnect, data received)
- Composable and testable
- Thread-safe by design
- Rich operator library

### Aspect-Oriented Logging
**Decision**: Use AOP for logging instead of manual logging

**Rationale**:
- Reduces boilerplate in business logic
- Consistent logging format
- Easy to enable/disable
- Centralizes logging concerns

### Interface-Driven Design
**Decision**: Heavy use of interfaces throughout

**Rationale**:
- Enables unit testing with mocks
- Supports dependency injection
- Allows for multiple implementations
- Clear contracts between components

### Record Types for Data
**Decision**: Use C# records for data structures

**Rationale**:
- Immutability by default
- Value semantics
- Concise syntax
- Built-in equality comparison

## Data Flow

### Device Discovery Flow
```
1. Application starts device monitoring
   └→ IDevices.StartAsync()

2. Watcher detects BLE advertisement
   └→ IWatcher raises DeviceDiscovered event

3. Device is added to detected devices
   └→ IDevices.DeviceDiscovered observable fires

4. Application can connect to device
   └→ IDevice.ConnectAsync()

5. Services are discovered on connection
   └→ IGattServices.DiscoverAsync()

6. Application can read/write characteristics
   └→ ICharacteristic.ReadAsync() / WriteAsync()
```

### Height Control Flow (Linak-specific)
```
1. Application requests height change
   └→ ILinakDesk.MoveToHeightAsync(height)

2. Command is converted to byte array
   └→ Protocol-specific encoding

3. Command is written to BLE characteristic
   └→ ICharacteristic.WriteAsync(bytes)

4. Desk sends height updates
   └→ Height characteristic notifications

5. Updates are observed by application
   └→ ILinakDesk.HeightChanged observable
```

## Testing Strategy

### Unit Tests
- Mock all interfaces
- Test business logic in isolation
- Fast execution
- No external dependencies

### Integration Tests
- Require actual Bluetooth hardware
- Test end-to-end scenarios
- Validate against real desk
- Slower execution

### Test Projects
- `*.Tests` - Unit tests for each project
- `Idasen.BluetoothLE.Integration.Tests` - Integration tests

## Concurrency and Threading

### Thread Safety
- Reactive observables handle thread safety
- Autofac lifetime scopes for per-request state
- Immutable data structures (records)

### Async/Await
- All I/O operations are async
- Bluetooth operations use async APIs
- No blocking calls in library code

## Error Handling

### Strategy
1. **Exceptions**: Thrown for programming errors
2. **Result Types**: Used for expected failures (when applicable)
3. **Observables**: Error events in observable streams
4. **Logging**: All errors logged via AOP

### Exception Types
- `ArgumentException` - Invalid parameters
- `InvalidOperationException` - Invalid state
- `TimeoutException` - Operation timeout
- Platform-specific exceptions from Windows BLE APIs

## Performance Considerations

### Bluetooth Operations
- Asynchronous to avoid blocking
- Connection pooling avoided (BLE typically single-connection)
- Minimal data conversions

### Memory Management
- Proper disposal of BLE resources
- Unsubscribe from observables when done
- Autofac lifetime scopes for cleanup

### Logging
- Structured logging for performance
- Configurable log levels
- Async logging to avoid blocking

## Extension Points

The library can be extended in several ways:

1. **Custom Characteristics**: Implement `ICharacteristic` for new BLE services
2. **Custom Devices**: Extend device discovery for other BLE desks
3. **Custom Protocols**: Implement other manufacturer protocols
4. **Custom Aspects**: Add new AOP interceptors for cross-cutting concerns

## Dependencies

### Core Dependencies
- **Autofac**: Dependency injection
- **Serilog**: Structured logging
- **System.Reactive**: Reactive extensions
- **CsvHelper**: Data parsing (GATT services)

### Windows Dependencies
- **Windows.Devices.Bluetooth**: Native BLE APIs
- **.NET 8.0-windows10.0.19041.0**: Platform target

## Future Considerations

### Potential Enhancements
- Support for other standing desk manufacturers
- Enhanced error recovery mechanisms
- Performance optimizations for large-scale deployments
- Additional health monitoring features
- Bluetooth 5.0 features utilization

### Maintainability
- Keep projects focused and cohesive
- Maintain high test coverage
- Document all public APIs
- Follow semantic versioning

---

For more information on specific components, see the [API Reference](API-Reference.md).
