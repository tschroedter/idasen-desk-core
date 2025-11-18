# CI/CD Workflows

This document describes the continuous integration and deployment workflows used in the Idasen Desk Core project.

## Overview

The project uses GitHub Actions for automated building, testing, code quality checks, and package publishing. All workflows are defined in `.github/workflows/`.

## Available Workflows

### 1. Build and Test (ci.yml)

**Trigger**: Push to any branch, Pull requests

**Purpose**: Continuous integration - builds and tests the code

**Badge**: [![Build and Test](https://github.com/tschroedter/idasen-desk-core/actions/workflows/ci.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/ci.yml)

**Steps**:
1. Checkout code
2. Setup .NET 8.0
3. Restore dependencies
4. Build solution (Release configuration)
5. Run tests
6. Upload test results

**Configuration**:
```yaml
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
```

**When it runs**:
- Every commit pushed to any branch
- Every pull request opened or updated
- Can be manually triggered from Actions tab

### 2. Code Quality (code-quality.yml)

**Trigger**: Push to main, Pull requests, Scheduled (weekly)

**Purpose**: Static analysis and security scanning

**Badge**: [![Code Quality](https://github.com/tschroedter/idasen-desk-core/actions/workflows/code-quality.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/code-quality.yml)

**Steps**:
1. Checkout code
2. Setup .NET 8.0
3. Restore dependencies
4. Build solution
5. Run .NET analyzers
6. Check code formatting (`dotnet format --verify-no-changes`)
7. Run security analysis

**Code Quality Checks**:
- **Code Formatting**: Ensures consistent style via `.editorconfig`
- **Static Analysis**: .NET analyzers find potential bugs
- **Security**: Scans for known vulnerabilities
- **Warnings as Errors**: Enforces high code quality

**When it runs**:
- Push to main branch
- Pull requests to main
- Every Monday at 6:00 AM UTC (scheduled)
- Manual trigger available

### 3. SonarCloud Analysis (sonarcloud.yml)

**Trigger**: Push to main, Pull requests

**Purpose**: Advanced code quality and security analysis

**Badge**: [![SonarCloud](https://github.com/tschroedter/idasen-desk-core/actions/workflows/sonarcloud.yml/badge.svg)](https://github.com/tschroedter/idasen-desk-core/actions/workflows/sonarcloud.yml)

**Steps**:
1. Checkout code
2. Setup Java (for SonarCloud scanner)
3. Setup .NET 8.0
4. Initialize SonarCloud scanner
5. Build solution
6. Run tests with coverage
7. Upload coverage to SonarCloud
8. Complete SonarCloud analysis

**Metrics Tracked**:
- Code smells
- Bugs
- Vulnerabilities
- Security hotspots
- Code coverage
- Technical debt
- Code duplication

**Quality Gate**:
- New code coverage > 80%
- No new bugs
- No new vulnerabilities
- Maintainability rating A

**When it runs**:
- Push to main branch
- Pull requests to main
- Provides PR decoration with analysis results

**Setup**: See [SonarCloud Setup Guide](https://github.com/tschroedter/idasen-desk-core/blob/main/.github/SONARCLOUD_SETUP.md)

### 4. PR Validation (pr-validation.yml)

**Trigger**: Pull requests

**Purpose**: Additional validation for pull requests

**Steps**:
1. Checkout code
2. Validate PR title format
3. Check for breaking changes
4. Verify changelog updated
5. Run integration tests (if applicable)
6. Comment on PR with validation results

**Validation Rules**:
- PR title follows conventional commits format
- No merge conflicts
- All CI checks pass
- Code review approved

### 5. Release and Publish (release.yml)

**Trigger**: Manual workflow dispatch, Push tags (v*)

**Purpose**: Automated versioning and NuGet package publishing

**Steps**:
1. Checkout code
2. Setup .NET 8.0
3. Determine version from tag or manual input
4. Update version in project files
5. Build solution (Release)
6. Run tests
7. Create NuGet package
8. Publish to NuGet.org
9. Create GitHub release
10. Generate release notes

**Version Format**: Semantic Versioning (e.g., v1.2.3)

**Artifacts**:
- NuGet package (`.nupkg`)
- Symbols package (`.snupkg`)
- Release notes

**When it runs**:
- Manual trigger from Actions tab (specify version)
- Automatically on version tag push (e.g., `git tag v1.2.3`)

**Requirements**:
- `NUGET_API_KEY` secret configured
- Version must follow semantic versioning

### 6. Dependency Updates (dependency-update.yml)

**Trigger**: Scheduled (weekly), Manual

**Purpose**: Security audits and dependency updates

**Steps**:
1. Checkout code
2. Setup .NET 8.0
3. Check for outdated packages
4. Check for vulnerable packages
5. Create issue if vulnerabilities found
6. Optionally create PR with updates

**When it runs**:
- Every Monday at 3:00 AM UTC
- Manual trigger available

**Notifications**:
- Creates GitHub issue if vulnerabilities detected
- Tags maintainers for review
- Includes severity and remediation info

## Workflow Secrets

### Required Secrets

Configure these in repository Settings → Secrets and variables → Actions:

1. **NUGET_API_KEY**
   - Purpose: Publishing packages to NuGet.org
   - How to get: https://www.nuget.org/account/apikeys
   - Used by: `release.yml`

2. **SONAR_TOKEN**
   - Purpose: SonarCloud authentication
   - How to get: SonarCloud → My Account → Security
   - Used by: `sonarcloud.yml`

3. **GITHUB_TOKEN**
   - Purpose: GitHub API access
   - Automatically provided by GitHub Actions
   - Used by: All workflows

## Environment Variables

Common environment variables used across workflows:

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  SOLUTION_PATH: 'src/idasen-desk-core.sln'
  BUILD_CONFIGURATION: 'Release'
  NUGET_PACKAGES: ${{ github.workspace }}/.nuget/packages
```

## Caching

Workflows use caching to improve performance:

### NuGet Package Cache
```yaml
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
```

Benefits:
- Faster restore times
- Reduced network usage
- Consistent builds

## Artifacts

### Build Artifacts

Each workflow uploads artifacts:

**CI Workflow**:
- Test results (`.trx` files)
- Code coverage reports

**Code Quality Workflow**:
- Analysis reports
- Formatting issues

**Release Workflow**:
- NuGet packages (`.nupkg`)
- Symbol packages (`.snupkg`)
- Build logs

### Artifact Retention

- Test results: 30 days
- Coverage reports: 90 days
- Release artifacts: 1 year

## Status Checks

### Required Checks

Pull requests require these checks to pass:

1. ✅ Build and Test
2. ✅ Code Quality
3. ✅ SonarCloud Analysis
4. ✅ PR Validation

### Optional Checks

These checks run but don't block merging:

- Dependency audit (informational)
- Integration tests (if hardware available)

## Notifications

### Workflow Failures

- Failed workflows send notifications to repository admins
- Email notifications for main branch failures
- GitHub notifications for PR check failures

### Configuration

Users can configure notifications in GitHub Settings → Notifications

## Monitoring and Metrics

### Workflow Metrics

View workflow metrics:
1. Repository → Insights → Actions
2. See:
   - Workflow run times
   - Success/failure rates
   - Most run workflows

### SonarCloud Dashboard

View code quality trends:
1. Visit [SonarCloud Dashboard](https://sonarcloud.io/dashboard?id=tschroedter_idasen-desk-core)
2. Monitor:
   - Quality gate status
   - Coverage trends
   - New issues over time

## Local Workflow Testing

### Act (Run workflows locally)

Install [Act](https://github.com/nektos/act):
```powershell
winget install nektos.act
```

Run workflows locally:
```bash
# List available workflows
act -l

# Run specific workflow
act -j build

# Run with secrets
act -j build --secret-file .secrets
```

**Note**: Some workflows may not work locally due to GitHub-specific features.

## Troubleshooting Workflows

### Workflow Fails to Start

**Problem**: Workflow doesn't trigger

**Solutions**:
1. Check workflow file syntax
2. Verify trigger conditions match
3. Check branch protection rules

### Build Fails

**Problem**: Build succeeds locally but fails in CI

**Solutions**:
1. Check .NET version matches
2. Verify all dependencies committed
3. Review workflow logs for specific errors

### Tests Fail in CI

**Problem**: Tests pass locally but fail in CI

**Solutions**:
1. Check for timing issues (use `Task.Delay` carefully)
2. Verify no dependencies on local state
3. Check integration tests require hardware (skip in CI)

### Secrets Not Working

**Problem**: Workflow can't access secrets

**Solutions**:
1. Verify secret name matches exactly (case-sensitive)
2. Check secret is configured in repository settings
3. Ensure workflow has permission to access secrets

### Cache Issues

**Problem**: Build is slow or uses stale cache

**Solutions**:
```yaml
# Invalidate cache by changing key
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-v2-${{ hashFiles('**/*.csproj') }}
```

## Best Practices

### Workflow Design

1. **Keep workflows focused**: Each workflow has single responsibility
2. **Use caching**: Cache dependencies for faster runs
3. **Fail fast**: Put quick checks first
4. **Parallelize**: Run independent jobs in parallel
5. **Use artifacts**: Share data between jobs

### Security

1. **Use secrets**: Never hardcode credentials
2. **Limit permissions**: Use minimal required permissions
3. **Audit dependencies**: Regularly check for vulnerabilities
4. **Review workflow changes**: Carefully review changes to workflows

### Maintenance

1. **Keep actions updated**: Use latest action versions
2. **Test workflow changes**: Use pull requests to test changes
3. **Monitor success rates**: Address flaky tests
4. **Document workflows**: Keep this documentation updated

## Customizing Workflows

### Modify Existing Workflow

1. Edit workflow file in `.github/workflows/`
2. Test changes in a branch
3. Create pull request
4. Review workflow runs
5. Merge when successful

### Add New Workflow

1. Create new `.yml` file in `.github/workflows/`
2. Define triggers and jobs
3. Test thoroughly
4. Document in this guide

### Workflow Template

```yaml
name: My Workflow

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  my-job:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Build
      run: dotnet build
    
    - name: Test
      run: dotnet test
```

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET in GitHub Actions](https://docs.github.com/en/actions/guides/building-and-testing-net)
- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [Development Guide](Development-Guide.md)

---

Need help with workflows? Open an issue on [GitHub](https://github.com/tschroedter/idasen-desk-core/issues).
