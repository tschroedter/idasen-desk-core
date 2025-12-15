# Update GitHub Wiki - Fix Raw Text Display Issue
# PowerShell script to update wiki links

Write-Host "Updating GitHub Wiki..." -ForegroundColor Green
Write-Host ""
Write-Host "This script will:"
Write-Host "1. Clone the wiki repository"
Write-Host "2. Apply the fix to remove .md extensions from internal links"
Write-Host "3. Convert old-format githubusercontent.com/wiki URLs to new format"
Write-Host "4. Commit and push the changes to GitHub"
Write-Host ""

$wikiDir = "idasen-desk-core.wiki"

# Clone or update wiki repository
if (Test-Path $wikiDir) {
    Write-Host "Wiki repository already exists, pulling latest changes..." -ForegroundColor Yellow
    Push-Location $wikiDir
    git pull
    Pop-Location
} else {
    Write-Host "Cloning wiki repository..." -ForegroundColor Yellow
    git clone https://github.com/tschroedter/idasen-desk-core.wiki.git
}

Push-Location $wikiDir

Write-Host "Applying wiki link fixes..." -ForegroundColor Yellow

# Function to fix old-format githubusercontent.com/wiki URLs
function Fix-OldWikiUrls {
    Write-Host "  Fixing old-format githubusercontent.com/wiki URLs..." -ForegroundColor Cyan
    
    Get-ChildItem -Path . -Filter "*.md" -File | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $modified = $false
        
        # Convert https://githubusercontent.com/wiki/tschroedter/idasen-desk-core/Page-Name.md
        # to https://github.com/tschroedter/idasen-desk-core/wiki/Page-Name
        # Pattern matches typical wiki page names (alphanumeric, hyphens, underscores, and hash for anchors)
        if ($content -match 'https://githubusercontent\.com/wiki/tschroedter/idasen-desk-core/') {
            # First: Handle URLs with .md extension
            $content = $content -replace 'https://githubusercontent\.com/wiki/tschroedter/idasen-desk-core/([A-Za-z0-9_#-]+)\.md', 'https://github.com/tschroedter/idasen-desk-core/wiki/$1'
            # Second: Handle URLs without .md extension
            $content = $content -replace 'https://githubusercontent\.com/wiki/tschroedter/idasen-desk-core/([A-Za-z0-9_#-]+)', 'https://github.com/tschroedter/idasen-desk-core/wiki/$1'
            $modified = $true
        }
        
        if ($modified) {
            Set-Content $_.FullName $content -NoNewline
            Write-Host "    Fixed old URLs in: $($_.Name)" -ForegroundColor Green
        }
    }
}

Fix-OldWikiUrls

# Function to fix wiki links in a file
function Fix-WikiLinks {
    param (
        [string]$FilePath
    )
    
    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw
        
        # Replace .md) with ) for internal wiki links
        # But preserve links to GitHub repository files (http URLs)
        $content = $content -replace '(?<!https?://[^\s\)]*)(Getting-Started|Development-Guide|Architecture-and-Design|Testing-Guide|Contributing|API-Reference|CI-CD-Workflows|Troubleshooting|Home)\.md([#\)])', '$1$2'
        
        Set-Content $FilePath $content -NoNewline
        Write-Host "  Fixed: $FilePath" -ForegroundColor Cyan
    }
}

# Fix all wiki files
Write-Host "Processing wiki files..." -ForegroundColor Yellow
Fix-WikiLinks "Home.md"
Fix-WikiLinks "README.md"
Fix-WikiLinks "Getting-Started.md"
Fix-WikiLinks "Development-Guide.md"
Fix-WikiLinks "API-Reference.md"
Fix-WikiLinks "Architecture-and-Design.md"
Fix-WikiLinks "CI-CD-Workflows.md"
Fix-WikiLinks "Testing-Guide.md"
Fix-WikiLinks "Troubleshooting.md"

# Commit and push
Write-Host ""
Write-Host "Committing changes..." -ForegroundColor Yellow
git add .
git commit -m "Fix wiki links to prevent raw text display

Remove .md extensions from internal wiki page links. GitHub wiki expects 
links without the .md extension to properly render pages instead of 
showing raw markdown text."

Write-Host ""
Write-Host "Pushing changes to GitHub..." -ForegroundColor Yellow
git push

Pop-Location

Write-Host ""
Write-Host "Wiki update complete!" -ForegroundColor Green
Write-Host "Visit https://github.com/tschroedter/idasen-desk-core/wiki to verify the changes." -ForegroundColor Green
