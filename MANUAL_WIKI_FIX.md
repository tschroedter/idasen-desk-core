# Manual Wiki Fix Guide

If you prefer to fix the wiki pages manually through the GitHub web interface, follow these steps for each file.

## Quick Fix Pattern

For each wiki page listed below, find all occurrences of these patterns and replace them:

| Find | Replace |
|------|---------|
| `(Getting-Started.md)` | `(Getting-Started)` |
| `(Getting-Started.md#` | `(Getting-Started#` |
| `(Development-Guide.md)` | `(Development-Guide)` |
| `(Architecture-and-Design.md)` | `(Architecture-and-Design)` |
| `(Testing-Guide.md)` | `(Testing-Guide)` |
| `(Contributing.md)` | `(Contributing)` |
| `(API-Reference.md)` | `(API-Reference)` |
| `(CI-CD-Workflows.md)` | `(CI-CD-Workflows)` |
| `(Troubleshooting.md)` | `(Troubleshooting)` |
| `(Home.md)` | `(Home)` |

**Important:** Do NOT change URLs that point to the GitHub repository (those starting with `https://github.com/`).

---

## Detailed File-by-File Instructions

### 1. Home.md
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Home/_edit

Find and replace:
- `(Getting-Started.md)` → `(Getting-Started)`
- `(Getting-Started.md#installation)` → `(Getting-Started#installation)`
- `(Getting-Started.md#prerequisites)` → `(Getting-Started#prerequisites)`
- `(Development-Guide.md)` → `(Development-Guide)`
- `(Architecture-and-Design.md)` → `(Architecture-and-Design)`
- `(Testing-Guide.md)` → `(Testing-Guide)`
- `(Contributing.md)` → `(Contributing)`
- `(API-Reference.md)` → `(API-Reference)`
- `(CI-CD-Workflows.md)` → `(CI-CD-Workflows)`
- `(Troubleshooting.md)` → `(Troubleshooting)`

### 2. Getting Started
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Getting-Started/_edit

Find and replace:
- `(API-Reference.md)` → `(API-Reference)`
- `(Architecture-and-Design.md)` → `(Architecture-and-Design)`
- `(Development-Guide.md)` → `(Development-Guide)`
- `(Troubleshooting.md)` → `(Troubleshooting)`
- `(Contributing.md)` → `(Contributing)`

### 3. Development Guide
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Development-Guide/_edit

Find and replace:
- `(Architecture-and-Design.md)` → `(Architecture-and-Design)`
- `(Testing-Guide.md)` → `(Testing-Guide)`
- `(API-Reference.md)` → `(API-Reference)`
- `(Contributing.md)` → `(Contributing)`

### 4. API Reference
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/API-Reference/_edit

Find and replace:
- `(Getting-Started.md)` → `(Getting-Started)`

### 5. Architecture and Design
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Architecture-and-Design/_edit

Find and replace:
- `(API-Reference.md)` → `(API-Reference)`

### 6. CI/CD Workflows
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/CI-CD-Workflows/_edit

Find and replace:
- `(Development-Guide.md)` → `(Development-Guide)`

### 7. Testing Guide
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Testing-Guide/_edit

Find and replace:
- `(Development-Guide.md)` → `(Development-Guide)`
- `(Contributing.md)` → `(Contributing)`

### 8. Troubleshooting
Visit: https://github.com/tschroedter/idasen-desk-core/wiki/Troubleshooting/_edit

Find and replace:
- `(Getting-Started.md)` → `(Getting-Started)`
- `(Development-Guide.md)` → `(Development-Guide)`
- `(API-Reference.md)` → `(API-Reference)`
- `(Testing-Guide.md)` → `(Testing-Guide)`

---

## Verification Checklist

After making all the changes, verify:

- [ ] Click on "Home" in the wiki sidebar
- [ ] Click each link in the "Documentation Overview" section
- [ ] Verify each page loads with proper formatting (not raw markdown)
- [ ] Verify anchor links jump to the correct section
- [ ] Check that no raw markdown symbols (`#`, `-`, etc.) appear at the start of lines

## Tips

1. Use your browser's Find & Replace (Ctrl+H or Cmd+H) in the edit view
2. Make sure you don't accidentally change links to GitHub repository files
3. Save each page after making changes
4. You can preview changes before saving using the "Preview" tab

---

**Estimated time:** 5-10 minutes to fix all pages manually.
