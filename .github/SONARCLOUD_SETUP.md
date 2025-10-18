# SonarCloud Setup Guide

This guide explains how to set up SonarCloud for the idasen-desk-core project.

## Prerequisites

1. **SonarCloud Account**: Create a free account at [sonarcloud.io](https://sonarcloud.io)
2. **GitHub Integration**: Link your GitHub account to SonarCloud

## Setup Steps

### 1. Create SonarCloud Project

1. Log in to [SonarCloud](https://sonarcloud.io)
2. Click on the **+** icon in the top right and select "Analyze new project"
3. Select the `tschroedter/idasen-desk-core` repository
4. Choose "With GitHub Actions" as the analysis method

### 2. Configure GitHub Secrets

Add the following secret to your GitHub repository:

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secret:
   - **Name**: `SONAR_TOKEN`
   - **Value**: Your SonarCloud token (obtained from SonarCloud → My Account → Security)

### 3. Verify Project Configuration

The project uses the following configuration:

- **Project Key**: `tschroedter_idasen-desk-core`
- **Organization**: `tschroedter`
- **Coverage Tool**: dotnet-coverage (for .NET code coverage)
- **Scanner**: dotnet-sonarscanner

Configuration files:
- `.github/workflows/sonarcloud.yml` - GitHub Actions workflow
- `sonar-project.properties` - SonarCloud project configuration

### 4. Workflow Triggers

The SonarCloud workflow runs automatically on:
- Push to the `main` branch
- Pull requests targeting the `main` branch

### 5. View Results

After the workflow runs successfully:
1. Visit [SonarCloud Dashboard](https://sonarcloud.io/dashboard?id=tschroedter_idasen-desk-core)
2. View code quality metrics, security vulnerabilities, and technical debt
3. Check the Quality Gate status in the README badges

## Features

The SonarCloud integration provides:

- **Code Quality Analysis**: Detects bugs, code smells, and vulnerabilities
- **Code Coverage**: Tracks test coverage across the codebase
- **Security Analysis**: Identifies security hotspots and vulnerabilities
- **Technical Debt**: Estimates effort to fix issues
- **Pull Request Decoration**: Automatic comments on PRs with analysis results
- **Quality Gate**: Pass/fail criteria for code quality

## Troubleshooting

### Workflow Fails with "SONAR_TOKEN not found"

**Solution**: Ensure the `SONAR_TOKEN` secret is properly configured in GitHub repository secrets.

### Coverage Reports Not Showing

**Solution**: Verify that:
1. `dotnet-coverage` tool is installed
2. Tests are running successfully
3. The `coverage.xml` file is generated

### Analysis Takes Too Long

**Solution**: SonarCloud analysis typically takes 2-5 minutes for this project size. If it takes longer, check the workflow logs for errors.

## Additional Resources

- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [.NET Analysis Documentation](https://docs.sonarcloud.io/advanced-setup/languages/csharp-vb-net/)
- [GitHub Actions Integration](https://docs.sonarcloud.io/advanced-setup/ci-based-analysis/github-actions/)
