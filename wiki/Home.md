# Welcome to the Idasen Desk Core Wiki

Welcome to the comprehensive documentation for the **Idasen Desk Core** library - a .NET solution for controlling Ikea's Idasen Desk via Bluetooth LE on Windows.

## 📚 Documentation Overview

### Getting Started
- **[Getting Started](Getting-Started.md)** - Quick start guide for using the library
- **[Installation](Getting-Started.md#installation)** - How to install and set up the library
- **[Prerequisites](Getting-Started.md#prerequisites)** - System requirements and dependencies

### Development
- **[Development Guide](Development-Guide.md)** - Complete guide for developing with and contributing to the project
- **[Architecture and Design](Architecture-and-Design.md)** - Understanding the project architecture
- **[Testing Guide](Testing-Guide.md)** - How to write and run tests
- **[Contributing](Contributing.md)** - Guidelines for contributing to the project

### Reference
- **[API Reference](API-Reference.md)** - Detailed API documentation
- **[CI/CD Workflows](CI-CD-Workflows.md)** - Information about automated workflows
- **[Troubleshooting](Troubleshooting.md)** - Common issues and solutions

## 🚀 About the Project

This repository contains low-level code to detect and control Ikea's Idasen standing desk using Bluetooth Low Energy (BLE) on Windows 10/11. The code is packaged as a NuGet package and is used by:

- [idasen-desk](https://github.com/tschroedter/idasen-desk) - Windows system tray application
- [idasen-desk-rest-api](https://github.com/tschroedter/idasen-desk-rest-api) - REST API for desk control

## 🔧 Key Features

- **Bluetooth LE Communication** - Direct control of Idasen desk via BLE
- **Windows 10/11 Support** - Native Windows BLE APIs
- **Reactive Design** - Built with System.Reactive for event-driven architecture
- **Aspect-Oriented Logging** - Comprehensive logging with Serilog and AOP
- **Comprehensive Testing** - Unit and integration tests with high coverage
- **NuGet Package** - Easy integration into your projects

## 🏗️ Project Structure

The solution consists of several key projects:

- **Idasen.BluetoothLE.Core** - Core Bluetooth LE functionality
- **Idasen.BluetoothLE.Linak** - Linak-specific implementations (Idasen desk protocol)
- **Idasen.BluetoothLE.Characteristics** - BLE characteristics handling
- **Idasen.Aop** - Aspect-oriented programming utilities
- **Idasen.Launcher** - Application launcher and initialization
- **Idasen.ConsoleApp** - Console application example

## 📦 NuGet Package

Install from NuGet:
```xml
<PackageReference Include="Idasen.Desk.Core" Version="1.0.0" />
```

Or via Package Manager Console:
```powershell
Install-Package Idasen.Desk.Core
```

## 🤝 Community

- **Issues**: Report bugs and request features on [GitHub Issues](https://github.com/tschroedter/idasen-desk-core/issues)
- **Discussions**: Ask questions and share ideas in [GitHub Discussions](https://github.com/tschroedter/idasen-desk-core/discussions)
- **Contributing**: See our [Contributing Guide](Contributing.md)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/tschroedter/idasen-desk-core/blob/main/LICENSE) file for details.

## 🔗 Related Projects

- [idasen-desk](https://github.com/tschroedter/idasen-desk) - Windows desktop application
- [idasen-desk-rest-api](https://github.com/tschroedter/idasen-desk-rest-api) - REST API wrapper

---

*Last updated: November 2024*
