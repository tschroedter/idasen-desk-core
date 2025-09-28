# GitHub Actions Workflows Documentation

This repository uses comprehensive GitHub Actions workflows for automated building, testing, and releasing.

## 🔄 Available Workflows

### 🏗️ Build and Test (`ci.yml`)
**Trigger**: Push/PR to main or develop branches
- Restores NuGet dependencies
- Builds the solution in Release configuration  
- Runs unit tests (excludes integration tests)
- Uploads test results as artifacts
- **Runner**: Windows (required for Windows 10 Bluetooth LE APIs)

### 🔍 Code Quality (`code-quality.yml`)
**Trigger**: Push/PR to main or develop branches
- Validates code formatting using .editorconfig rules
- Runs static code analysis with .NET analyzers
- Ensures code style compliance
- Generates detailed analysis reports
- **Runner**: Windows

### 🚀 Release and Publish (`release.yml`)
**Trigger**: Git tags (`v*.*.*`) or manual workflow dispatch
- Updates version numbers in project files
- Builds and tests the solution
- Creates NuGet packages
- Creates GitHub releases with changelogs
- Publishes to NuGet (if API key configured)
- **Runner**: Windows

### 📊 PR Validation (`pr-validation.yml`)
**Trigger**: Pull requests to main or develop branches
- Validates code formatting compliance
- Ensures successful build
- Runs comprehensive test suite
- Provides validation summary
- **Runner**: Windows

### 🛡️ Dependency Updates (`dependency-update.yml`)
**Trigger**: Weekly schedule (Sundays) or manual dispatch
- Audits dependencies for security vulnerabilities
- Lists outdated packages
- Generates dependency audit reports
- **Runner**: Windows

## 🚀 Getting Started

### Prerequisites
- **Windows runner required** - The project uses Windows 10 Bluetooth LE APIs
- .NET 8.0 SDK
- NuGet package sources configured

### Configuration

#### Secrets Required
Add these secrets in your repository settings:

- `NUGET_API_KEY` (optional): For automated NuGet publishing
- `GITHUB_TOKEN`: Automatically provided by GitHub Actions

#### Workflow Customization
You can customize workflow behavior by:

1. **Modifying trigger conditions** in the `on:` sections
2. **Adjusting build configurations** (Debug/Release)
3. **Changing test filters** for different test suites
4. **Updating version patterns** in release workflow

### Manual Workflows

#### Creating a Release
1. **Via Git Tag**:
   ```bash
   git tag v1.0.1
   git push origin v1.0.1
   ```

2. **Via GitHub Interface**:
   - Go to Actions → Release and Publish
   - Click "Run workflow"
   - Enter version number (e.g., `1.0.1`)

#### Running Security Audit
- Go to Actions → Dependency Updates
- Click "Run workflow" to run immediately

## 🛠️ Local Development

The workflows mirror functionality available in the local `build.ps1` script:

```powershell
# Build and test locally
.\build.ps1 -Action build
.\build.ps1 -Action test

# Check code formatting
.\build.ps1 -Action format

# Create packages
.\build.ps1 -Action pack

# Run everything
.\build.ps1 -Action all
```

## 📊 Status Badges

Add these to your README.md:

```markdown
[![Build and Test](https://github.com/tschroedter/idasen-desk-core/actions/workflows/ci.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/ci.yml)
[![Code Quality](https://github.com/tschroedter/idasen-desk-core/actions/workflows/code-quality.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/code-quality.yml)
```

## 🔧 Troubleshooting

### Common Issues

**Build Failures on Non-Windows**:
- Ensure workflows use `runs-on: windows-latest`
- The project requires Windows 10 APIs for Bluetooth functionality

**Test Failures**:
- Check test results artifacts for detailed error information
- Integration tests are excluded by default (`Integration.Tests` filter)

**Code Formatting Issues**:
- Run `dotnet format` locally to fix formatting
- Ensure .editorconfig rules are followed

**NuGet Publishing Issues**:
- Verify `NUGET_API_KEY` secret is configured
- Check package version conflicts

### Getting Help
- Check workflow run logs for detailed error information
- Review the build.ps1 script for local testing equivalents
- Ensure all prerequisites are met for Windows-specific APIs