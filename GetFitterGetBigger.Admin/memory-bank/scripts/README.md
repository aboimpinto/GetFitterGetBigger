# Memory Bank Scripts

This directory contains utility scripts for managing the memory bank.

## Available Scripts

### generate-feature-registry.sh
Generates a feature registry report by scanning all feature folders.

**Usage:**
```bash
./generate-feature-registry.sh > ../features/FEATURE_REGISTRY.md
```

**Output:** Markdown report with all features organized by stage, including IDs and descriptions.

### generate-bug-registry.sh
Generates a bug registry report by scanning all bug folders.

**Usage:**
```bash
./generate-bug-registry.sh > ../bugs/BUG_REGISTRY.md
```

**Output:** Markdown report with all bugs organized by stage, including severity and priority.

## Why These Scripts?

Instead of maintaining duplicate registry files that can get out of sync, these scripts generate reports on-demand from the actual folder structure. This follows the principle of having a single source of truth (the folders themselves).

## When to Use

- Before planning meetings to get an overview
- When creating release notes
- For status reports
- When you need a quick count of features/bugs by stage

The generated reports are temporary and should not be committed to version control.