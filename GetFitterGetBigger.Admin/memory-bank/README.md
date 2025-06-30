# Memory Bank Structure

This directory contains the central knowledge base for the GetFitterGetBigger Admin project.

## Root Directory Files

### Process Documentation (Essential)
- **FEATURE_IMPLEMENTATION_PROCESS.md** - How to implement features
- **FEATURE_WORKFLOW_PROCESS.md** - Feature lifecycle and states
- **BUG_IMPLEMENTATION_PROCESS.md** - How to fix bugs
- **BUG_WORKFLOW_PROCESS.md** - Bug lifecycle and states
- **documentation-workflow.md** - How to maintain documentation

### Testing Documentation (Essential)
- **TestingGuidelines.md** - Comprehensive testing strategy
- **TESTING-QUICK-REFERENCE.md** - Quick reference for common test scenarios
- **testing-strategy.md** - Overall testing approach

### Project Context (Essential)
- **activeContext.md** - Current project state and AI assistant context
- **productContext.md** - Product vision and business context
- **techContext.md** - Technical architecture and decisions
- **projectbrief.md** - High-level project overview
- **systemPatterns.md** - Coding patterns and conventions
- **progress.md** - Overall project progress tracking

## Directory Structure

```
memory-bank/
├── features/          # Feature documentation by state
├── bugs/             # Bug reports by state
├── docs/             # General documentation
├── api-patterns/     # API design patterns
└── scripts/          # Utility scripts
```

## Recent Cleanup (2025-06-29)

All feature-specific documentation has been moved to appropriate feature folders:
- API integration guides → Feature folders
- Implementation documentation → Feature folders
- Setup guides → Feature folders

This keeps the root directory focused on cross-cutting concerns and processes.