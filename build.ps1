#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Local development helper script for the Idasen Desk Core project.

.DESCRIPTION
    This script provides common development tasks that mirror the GitHub Actions workflows:
    - Building the solution
    - Running tests  
    - Code formatting
    - Package creation
    - Version management

.PARAMETER Action
    The action to perform: build, test, format, pack, version, clean, all

.PARAMETER Configuration
    Build configuration: Debug or Release (default: Release)

.PARAMETER Verbose
    Enable verbose output

.EXAMPLE
    .\build.ps1 -Action build
    Builds the solution in Release configuration

.EXAMPLE
    .\build.ps1 -Action test -Configuration Debug
    Runs tests in Debug configuration

.EXAMPLE  
    .\build.ps1 -Action all -Verbose
    Performs all actions with verbose output
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('build', 'test', 'format', 'pack', 'version', 'clean', 'all')]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose
)

# Script configuration
$SolutionFile = "src/idasen-desk-core.sln"
$ProjectFile = "src/Idasen.Launcher/Idasen.Launcher.csproj"
$OutputDir = "output"

# Helper function for colored output
function Write-ColoredHost($Message, $ForegroundColor = "White") {
    Write-Host $Message -ForegroundColor $ForegroundColor
}

# Helper function to run commands and check exit codes
function Invoke-CommandWithCheck($Command, $Description) {
    Write-ColoredHost "=== $Description ===" "Green"
    Write-Host "Running: $Command" -ForegroundColor Gray
    
    Invoke-Expression $Command
    if ($LASTEXITCODE -ne 0) {
        Write-ColoredHost "ERROR: $Description failed with exit code $LASTEXITCODE" "Red"
        exit $LASTEXITCODE
    }
    Write-ColoredHost "SUCCESS: $Description completed" "Green"
    Write-Host ""
}

# Check if running on Windows (required for this project)
if (-not $IsWindows -and -not $env:OS.Contains("Windows")) {
    Write-ColoredHost "ERROR: This project requires Windows to build due to Windows-specific Bluetooth APIs." "Red"
    exit 1
}

# Verify solution file exists
if (-not (Test-Path $SolutionFile)) {
    Write-ColoredHost "ERROR: Solution file not found: $SolutionFile" "Red"
    exit 1
}

Write-ColoredHost "Idasen Desk Core - Local Development Script" "Cyan"
Write-ColoredHost "Action: $Action" "Yellow"
Write-ColoredHost "Configuration: $Configuration" "Yellow"
Write-Host ""

# Set verbosity
$VerbosityLevel = if ($Verbose) { "normal" } else { "minimal" }

function Build-Solution {
    Write-ColoredHost "Building solution..." "Blue"
    
    # Restore dependencies
    Invoke-CommandWithCheck "dotnet restore `"$SolutionFile`" --verbosity $VerbosityLevel" "Restore Dependencies"
    
    # Build solution
    Invoke-CommandWithCheck "dotnet build `"$SolutionFile`" --configuration $Configuration --no-restore --verbosity $VerbosityLevel" "Build Solution"
}

function Test-Solution {
    Write-ColoredHost "Running tests..." "Blue"
    
    # Find test projects
    $testProjects = Get-ChildItem -Path "src" -Recurse -Filter "*.Tests.csproj" | Where-Object { $_.Name -notmatch 'Integration\.Tests' }
    
    if (-not $testProjects) {
        Write-ColoredHost "No unit test projects found." "Yellow"
        return
    }
    
    # Create output directory for test results
    $testResultsDir = "TestResults"
    if (Test-Path $testResultsDir) {
        Remove-Item $testResultsDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $testResultsDir | Out-Null
    
    # Run tests for each project
    $failed = $false
    foreach ($proj in $testProjects) {
        Write-ColoredHost "Testing: $($proj.Name)" "Cyan"
        
        $testCommand = "dotnet test `"$($proj.FullName)`" --configuration $Configuration --no-build --logger `"trx;LogFileName=$($proj.BaseName)-results.trx`" --collect:`"XPlat Code Coverage`" --results-directory `"$testResultsDir`" --verbosity $VerbosityLevel"
        
        try {
            Invoke-Expression $testCommand
            if ($LASTEXITCODE -ne 0) {
                $failed = $true
                Write-ColoredHost "FAILED: $($proj.Name)" "Red"
            } else {
                Write-ColoredHost "PASSED: $($proj.Name)" "Green"
            }
        } catch {
            Write-ColoredHost "ERROR: Failed to run tests for $($proj.Name): $_" "Red"
            $failed = $true
        }
    }
    
    if ($failed) {
        Write-ColoredHost "Some tests failed. Check the test results in the $testResultsDir directory." "Red"
        exit 1
    } else {
        Write-ColoredHost "All tests passed!" "Green"
    }
}

function Format-Code {
    Write-ColoredHost "Checking code formatting..." "Blue"
    
    # Check formatting
    $formatCommand = "dotnet format `"$SolutionFile`" --verify-no-changes --verbosity diagnostic"
    
    Write-Host "Checking if code is properly formatted..." -ForegroundColor Gray
    Invoke-Expression $formatCommand
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColoredHost "Code formatting issues detected." "Yellow"
        Write-Host "Run 'dotnet format `"$SolutionFile`"' to fix formatting issues." -ForegroundColor Yellow
        
        # Ask user if they want to fix formatting
        $response = Read-Host "Would you like to fix formatting issues now? (y/n)"
        if ($response -eq 'y' -or $response -eq 'Y') {
            Invoke-CommandWithCheck "dotnet format `"$SolutionFile`"" "Fix Code Formatting"
        }
    } else {
        Write-ColoredHost "Code formatting is correct!" "Green"
    }
}

function Create-Package {
    Write-ColoredHost "Creating NuGet package..." "Blue"
    
    # Create output directory
    if (Test-Path $OutputDir) {
        Remove-Item $OutputDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
    
    # Create package
    Invoke-CommandWithCheck "dotnet pack `"$ProjectFile`" --configuration $Configuration --no-build --output `"$OutputDir`" --verbosity $VerbosityLevel" "Create Package"
    
    # Display package information
    $packages = Get-ChildItem $OutputDir -Filter "*.nupkg"
    if ($packages) {
        Write-ColoredHost "Created packages:" "Green"
        foreach ($package in $packages) {
            $sizeInMB = [math]::Round($package.Length / 1MB, 2)
            Write-Host "  - $($package.Name) (Size: $sizeInMB MB)" -ForegroundColor Cyan
        }
    }
}

function Show-Version {
    Write-ColoredHost "Current version information:" "Blue"
    
    if (Test-Path $ProjectFile) {
        [xml]$proj = Get-Content $ProjectFile
        $versionNode = $proj.Project.PropertyGroup | Where-Object { $_.VersionPrefix } | Select-Object -First 1
        
        if ($versionNode -and $versionNode.VersionPrefix) {
            Write-Host "Current Version: $($versionNode.VersionPrefix)" -ForegroundColor Cyan
        } else {
            Write-ColoredHost "VersionPrefix not found in project file." "Yellow"
        }
    }
    
    # Show .NET version
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK Version: $dotnetVersion" -ForegroundColor Cyan
}

function Clean-Solution {
    Write-ColoredHost "Cleaning solution..." "Blue"
    
    # Clean solution
    Invoke-CommandWithCheck "dotnet clean `"$SolutionFile`" --configuration $Configuration --verbosity $VerbosityLevel" "Clean Solution"
    
    # Remove additional directories
    $dirsToClean = @("TestResults", $OutputDir, "packages")
    foreach ($dir in $dirsToClean) {
        if (Test-Path $dir) {
            Write-Host "Removing directory: $dir" -ForegroundColor Gray
            Remove-Item $dir -Recurse -Force
        }
    }
}

function Run-All {
    Write-ColoredHost "Running complete build and test cycle..." "Blue"
    
    Clean-Solution
    Build-Solution
    Format-Code
    Test-Solution
    Create-Package
    Show-Version
    
    Write-ColoredHost "All tasks completed successfully!" "Green"
}

# Execute the requested action
switch ($Action) {
    'build' { Build-Solution }
    'test' { Test-Solution }
    'format' { Format-Code }
    'pack' { Create-Package }
    'version' { Show-Version }
    'clean' { Clean-Solution }
    'all' { Run-All }
}

Write-ColoredHost "Script completed successfully!" "Green"