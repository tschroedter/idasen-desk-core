# Wiki Sync Setup Instructions

## Overview
This repository now includes an automated GitHub Action workflow that syncs the wiki content from the `/wiki` directory to the GitHub Wiki.

## Prerequisites

Before the automated sync can work, **GitHub Wiki must be enabled** for the repository:

### Enabling GitHub Wiki

1. Go to repository Settings: https://github.com/tschroedter/idasen-desk-core/settings
2. Scroll down to the "Features" section
3. Check the box next to "Wikis" to enable it
4. Click "Save" if needed

Once enabled, the wiki will be available at: https://github.com/tschroedter/idasen-desk-core/wiki

## How the Automated Sync Works

The workflow (`.github/workflows/sync-wiki.yml`) will:
1. Trigger automatically when changes are pushed to the `wiki/` directory on the main branch
2. Can also be triggered manually from the Actions tab
3. Use the `SwiftDocOrg/github-wiki-publish-action` to sync content
4. Push all markdown files from `/wiki` to the GitHub Wiki repository

## Testing the Sync

After enabling the wiki and merging this PR:

1. The workflow will run automatically on the next push to main that includes wiki changes
2. You can also manually trigger it:
   - Go to Actions tab: https://github.com/tschroedter/idasen-desk-core/actions
   - Select "Sync Wiki" workflow
   - Click "Run workflow"
   - Select the main branch
   - Click "Run workflow" button

3. After the workflow completes, visit: https://github.com/tschroedter/idasen-desk-core/wiki
4. You should see all the wiki pages populated with content from the `/wiki` directory

## Files Synced

The following markdown files will be synced:
- `Home.md` - Main landing page
- `Getting-Started.md` - Installation and setup guide
- `API-Reference.md` - API documentation
- `Architecture-and-Design.md` - Architecture details
- `Development-Guide.md` - Development environment guide
- `Testing-Guide.md` - Testing documentation
- `Contributing.md` - Contribution guidelines
- `CI-CD-Workflows.md` - CI/CD information
- `Troubleshooting.md` - Common issues and solutions

## Troubleshooting

### Wiki Pages Not Showing Up

If the wiki pages don't appear after running the workflow:

1. **Verify Wiki is Enabled**: Check repository settings to ensure the Wiki feature is enabled
2. **Check Workflow Status**: Review the Actions tab to see if the workflow completed successfully
3. **Review Permissions**: Ensure the workflow has `contents: write` permission (already configured)
4. **Manual Sync**: As a fallback, you can manually clone and push to the wiki repository:
   ```bash
   git clone https://github.com/tschroedter/idasen-desk-core.wiki.git
   cd idasen-desk-core.wiki
   cp ../wiki/*.md .
   git add .
   git commit -m "Initial wiki content"
   git push
   ```

### Workflow Fails

If the workflow fails:
1. Check the workflow logs in the Actions tab
2. Verify the `GITHUB_TOKEN` has sufficient permissions
3. Ensure the wiki repository is accessible

## Benefits of Automated Sync

- **Always Up-to-Date**: Wiki automatically updates when documentation changes
- **Version Control**: All wiki content is version controlled in the main repository
- **Easy Editing**: Edit wiki content like any other file with pull requests
- **No Manual Steps**: No need to manually copy files to the wiki
