# Wiki Link Fix - Raw Text Display Issue

## Problem

When clicking on links from the GitHub wiki main page (Home), some wiki pages were being displayed as raw markdown text instead of being properly rendered.

## Root Cause

GitHub wikis expect internal links to be written **without** the `.md` file extension. When links include the `.md` extension (e.g., `[Getting Started](Getting-Started.md)`), GitHub treats them as links to raw files instead of wiki pages, causing the raw markdown to be displayed.

## Solution

All internal wiki page links have been updated to remove the `.md` extension:

**Before:**
```markdown
- **[Getting Started](Getting-Started.md)** - Quick start guide
- **[Installation](Getting-Started.md#installation)** - How to install
```

**After:**
```markdown
- **[Getting Started](Getting-Started)** - Quick start guide
- **[Installation](Getting-Started#installation)** - How to install
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
- **Change pattern:** Replaced all occurrences of `*.md)` with `)` for internal wiki links
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
