# Wiki Link Fix - Raw Text Display Issue

## Problem

When clicking on links from the GitHub wiki main page (Home), some wiki pages were being displayed as raw markdown text instead of being properly rendered.

Additionally, old-format wiki URLs using `githubusercontent.com/wiki/` should be updated to the new format.

## Root Cause

1. GitHub wikis expect internal links to be written **without** the `.md` file extension. When links include the `.md` extension (e.g., `[Getting Started](Getting-Started.md)`), GitHub treats them as links to raw files instead of wiki pages, causing the raw markdown to be displayed.

2. Old-format wiki URLs (`https://githubusercontent.com/wiki/tschroedter/idasen-desk-core/Page-Name.md`) should use the new format (`https://github.com/tschroedter/idasen-desk-core/wiki/Page-Name`) for consistency and to avoid `.md` extensions.

## Solution

All internal wiki page links have been updated to remove the `.md` extension, and old-format URLs are converted to the new format:

**Before:**
```markdown
- **[Getting Started](Getting-Started.md)** - Quick start guide
- **[Installation](Getting-Started.md#installation)** - How to install
- Visit: https://githubusercontent.com/wiki/tschroedter/idasen-desk-core/Getting-Started.md
```

**After:**
```markdown
- **[Getting Started](Getting-Started)** - Quick start guide
- **[Installation](Getting-Started#installation)** - How to install
- Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Getting-Started
```

## Files Updated

The following wiki files were updated:
1. `Home.md` - Main wiki page with navigation links
2. `README.md` - Wiki directory README with quick links
3. `Getting-Started.md` - Links to other guide pages
4. `Development-Guide.md` - Links to related documentation
5. `API-Reference.md` - Link to Getting Started guide
6. `Architecture-and-Design.md` - Link to API Reference
7. `CI-CD-Workflows.md` - Link to Development Guide
8. `Testing-Guide.md` - Links to Development and Contributing guides
9. `Troubleshooting.md` - Links to all guide pages

## Changes Summary

- **Total files changed:** 9
- **Total lines changed:** 50 insertions(+), 50 deletions(-)
- **Change pattern:** 
  1. Replaced all occurrences of `*.md)` with `)` for internal wiki links
  2. Converted `https://githubusercontent.com/wiki/tschroedter/idasen-desk-core/` URLs to `https://github.com/tschroedter/idasen-desk-core/wiki/`
  3. Removed `.md` extensions from any githubusercontent.com/wiki URLs
- **External links preserved:** Links to repository files (e.g., `https://github.com/.../SONARCLOUD_SETUP.md`) were **not** changed

## How to Apply

> **Note:** Since the wiki is a separate git repository, you'll need to have push access to apply these changes.

### Option 1: Use the Automated Script (Recommended)

**For Bash/Linux/macOS:**
```bash
chmod +x update-wiki.sh
./update-wiki.sh
```

**For Windows PowerShell:**
```powershell
.\update-wiki.ps1
```

These scripts will:
1. Clone the wiki repository (if not already present)
2. Apply all the fixes automatically
3. Commit and push the changes to GitHub

### Option 2: Manual Application Using Patch

1. Clone the wiki repository:
   ```bash
   git clone https://github.com/tschroedter/idasen-desk-core.wiki.git
   cd idasen-desk-core.wiki
   ```

2. Apply the patch file (if you have it):
   ```bash
   git apply /path/to/0001-Fix-wiki-links-to-prevent-raw-text-display.patch
   ```

3. Commit and push:
   ```bash
   git add .
   git commit -m "Fix wiki links to prevent raw text display"
   git push
   ```

### Option 3: Manual Edit in GitHub Wiki UI

For each wiki page listed above, edit it through the GitHub wiki interface and:
1. Find all internal wiki links that end with `.md)`
2. Remove the `.md` extension but keep the `)`
3. Also fix anchor links like `.md#section)` to `#section)`
4. Save the changes

## Verification

After applying the fix, verify that:
1. All links on the Home page render the target page correctly
2. Pages appear with proper markdown formatting (headers, code blocks, lists, etc.)
3. No pages show raw markdown text (lines starting with `#`, `-`, etc.)
4. Anchor links (e.g., `Getting-Started#installation`) still jump to the correct section

## Notes

- **External repository links** should keep the `.md` extension as they link to actual files in the repository
- **Internal wiki links** should never have the `.md` extension
- This is a GitHub wiki-specific behavior; regular markdown files in repositories work differently
