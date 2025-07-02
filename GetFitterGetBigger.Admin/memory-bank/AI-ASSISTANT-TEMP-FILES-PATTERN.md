# AI Assistant Temporary Files Pattern

## Overview
When working on features or tasks, AI assistants often need to create temporary files for planning, summaries, or work-in-progress documentation. These files should not be placed in the main codebase or test directories.

## Solution: memory-bank/temp Directory

### Location
```
/memory-bank/temp/
```

### Purpose
- Temporary workspace for AI assistant sessions
- Store planning documents, summaries, and work-in-progress files
- Keep session-specific files isolated from permanent codebase

### Configuration
The `memory-bank/temp/` directory is added to `.gitignore` to ensure:
- Files are never accidentally committed
- The workspace remains clean between sessions
- Temporary files don't clutter git history

### Usage Guidelines

#### DO:
- Create feature plans (e.g., `FEAT-013-IMPLEMENTATION-PLAN.md`)
- Store test cleanup summaries
- Keep temporary analysis reports
- Use for session-specific notes
- Create draft documentation before moving to permanent location

#### DON'T:
- Store permanent documentation
- Reference temp files from code or permanent docs
- Expect files to persist between sessions
- Put anything that needs version control

### Example Workflow
```bash
# Working on a feature
memory-bank/temp/FEAT-XXX-PLAN.md           # Initial planning
memory-bank/temp/FEAT-XXX-ANALYSIS.md       # Code analysis notes
memory-bank/temp/FEAT-XXX-TEST-SUMMARY.md   # Test coverage summary

# After completion, move important content to:
memory-bank/features/FEAT-XXX/              # Permanent feature documentation
```

### File Naming Convention
- Use feature/task prefixes: `FEAT-XXX-`, `BUG-XXX-`, `TASK-XXX-`
- Use descriptive suffixes: `-PLAN`, `-SUMMARY`, `-ANALYSIS`, `-NOTES`
- Use uppercase for better visibility

### Cleanup
- Files in temp/ can be deleted at any time
- No cleanup is required due to .gitignore
- Consider it a "scratch pad" that resets

## Implementation
1. Created `/memory-bank/temp/` directory
2. Added to `.gitignore` with comment:
   ```
   # Temporary session files (AI assistant workspace)
   memory-bank/temp/
   ```
3. Added `README.md` to temp/ explaining its purpose

## Benefits
- Keeps test directories clean
- Prevents accidental commits of work-in-progress files
- Provides clear workspace for AI assistants
- Maintains separation between temporary and permanent documentation