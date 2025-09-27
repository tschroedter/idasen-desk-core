# idasen-desk-core

[![Build and Test](https://github.com/tschroedter/idasen-desk-core/actions/workflows/ci.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/ci.yml)
[![Code Quality](https://github.com/tschroedter/idasen-desk-core/actions/workflows/code-quality.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/code-quality.yml)
[![NuGet](https://img.shields.io/nuget/v/Idasen.Desk.Core.svg)](https://www.nuget.org/packages/Idasen.Desk.Core/)

This repository is about controlling Ikea's Idasen Desk using Windows 10 and BluetoothLE. Ikea only provides an Android and IOs app to control the desk. I thought it would be far more convenient to control the desk using a Windows 10 system tray application. This repository contains all the low level code to detect and control the desk. The code is build into a NuGet package and is currently used by:
- [idasen-desk](https://github.com/tschroedter/idasen-desk)
- [idasen-desk-rest-api](https://github.com/tschroedter/idasen-desk-rest-api)

## 🚀 Quick Start

### Prerequisites
- Windows 10/11 (required for Bluetooth LE APIs)
- .NET 8.0 SDK
- PowerShell 5.1 or PowerShell Core

### Building the Project
```powershell
# Clone the repository
git clone https://github.com/tschroedter/idasen-desk-core.git
cd idasen-desk-core

# Build and test (using provided script)
.\build.ps1 -Action all

# Or manually:
cd src
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

## 📦 NuGet Package

Install from NuGet:
```xml
<PackageReference Include="Idasen.Desk.Core" Version="1.0.0" />
```

## 🔄 CI/CD Workflows

This project uses comprehensive GitHub Actions workflows for automated building, testing, and releasing:

### Available Workflows:
- **🏗️ Build and Test** (`ci.yml`) - Continuous integration on every push/PR
- **🚀 Release and Publish** (`release.yml`) - Automated versioning and NuGet publishing  
- **🔍 Code Quality** (`code-quality.yml`) - Static analysis and security scanning
- **📊 PR Validation** (`pr-validation.yml`) - Pull request validation and reporting
- **🛡️ Dependency Updates** (`dependency-update.yml`) - Weekly security audits

For detailed information about the workflows, see [Workflows Documentation](.github/WORKFLOWS.md).

### Local Development
Use the provided PowerShell script for common development tasks:
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
