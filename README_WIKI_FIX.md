# Wiki Fix Instructions

## Problem Summary

The GitHub wiki pages may show raw markdown text instead of being properly rendered when accessed through links on the Home page. Additionally, old-format wiki URLs need to be updated.

## Root Cause

1. Internal wiki links were written with `.md` extensions (e.g., `[Getting Started](Getting-Started.md)`), which causes GitHub to display the raw file instead of rendering it as a wiki page.

2. Old-format wiki URLs using `https://githubusercontent.com/wiki/tschroedter/idasen-desk-core/` should be updated to the new format `https://github.com/tschroedter/idasen-desk-core/wiki/` for consistency.

## Solution

1. Remove the `.md` extension from all internal wiki page links.
2. Convert old-format githubusercontent.com/wiki URLs to the new github.com format.

## How to Apply the Fix

Choose one of the following methods:

### Method 1: Automated Script (Fastest - Recommended)

**On Windows (PowerShell):**
```powershell
.\update-wiki.ps1
```

**On Linux/macOS (Bash):**
```bash
chmod +x update-wiki.sh
./update-wiki.sh
```

**What it does:**
- Clones the wiki repository
- Automatically fixes all wiki links (removes .md extensions)
- Converts old-format githubusercontent.com/wiki URLs to new format
- Commits and pushes changes

**Time:** ~1 minute

### Method 2: Manual Web Interface (No Git Required)

Follow the step-by-step guide in: **[MANUAL_WIKI_FIX.md](MANUAL_WIKI_FIX.md)**

**What it does:**
- Provides exact find/replace patterns for each wiki page
- Can be done entirely through GitHub's web interface
- No command line or Git knowledge needed

**Time:** ~5-10 minutes

### Method 3: Git Command Line (Advanced)

If you prefer manual control with Git:

```bash
# Clone the wiki
git clone https://github.com/tschroedter/idasen-desk-core.wiki.git
cd idasen-desk-core.wiki

# Make the fixes manually or use sed/PowerShell
# (See WIKI_FIX.md for detailed patterns)

# Commit and push
git add .
git commit -m "Fix wiki links to prevent raw text display"
git push
```

## Files in This Fix

- **README_WIKI_FIX.md** (this file) - Quick start guide
- **WIKI_FIX.md** - Detailed technical documentation
- **MANUAL_WIKI_FIX.md** - Step-by-step manual fix guide
- **update-wiki.sh** - Bash automation script
- **update-wiki.ps1** - PowerShell automation script

## What Gets Fixed

The fix updates 9 wiki files:
1. Home.md
2. README.md  
3. Getting-Started.md
4. Development-Guide.md
5. API-Reference.md
6. Architecture-and-Design.md
7. CI-CD-Workflows.md
8. Testing-Guide.md
9. Troubleshooting.md

**Total changes:** 50 lines (removing `.md` from internal links)

## Verification

After applying the fix:

1. Visit: https://github.com/tschroedter/idasen-desk-core/wiki
2. Click on each link in the "Documentation Overview" section
3. Verify pages display with proper formatting
4. Confirm no raw markdown text is visible

## Need Help?

- For technical details, see: **[WIKI_FIX.md](WIKI_FIX.md)**
- For manual fixes, see: **[MANUAL_WIKI_FIX.md](MANUAL_WIKI_FIX.md)**
- For issues, open a GitHub issue

---

**Note:** These scripts and instructions fix the wiki repository at:
`https://github.com/tschroedter/idasen-desk-core.wiki`

This is separate from the main code repository.
