# Memory Bank Structure

This directory contains the central knowledge base for the GetFitterGetBigger Admin project.

## Root Directory Files

### Process Documentation (Essential)
- **DEVELOPMENT_PROCESS.md** - Main entry point for all development activities
- **FEATURE_IMPLEMENTATION_PROCESS.md** - How to implement features
- **FEATURE_WORKFLOW_PROCESS.md** - Feature lifecycle and states
- **BUG_IMPLEMENTATION_PROCESS.md** - How to fix bugs
- **BUG_WORKFLOW_PROCESS.md** - Bug lifecycle and states
- **documentation-workflow.md** - How to maintain documentation

### Standards & Guidelines (Essential)
- **CODE_QUALITY_STANDARDS.md** - Universal code quality standards
- **CODE_REVIEW_PROCESS.md** - Code review requirements and templates
- **REFERENCE_TABLES_GUIDE.md** - How to implement and use reference tables

### Testing Documentation (Essential)
- **COMPREHENSIVE-TESTING-GUIDE.md** - Consolidated testing guide for Blazor/bUnit and React/Jest
- **TEST-COVERAGE-IMPROVEMENT-STRATEGY.md** - Strategy for improving test coverage

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
├── api-patterns/     # API design patterns and troubleshooting
│   ├── id-format.md                      # ID format conventions
│   └── api-integration-troubleshooting.md # Common API issues and solutions
└── scripts/          # Utility scripts
```

## Recent Cleanup (2025-06-29)

All feature-specific documentation has been moved to appropriate feature folders:
- API integration guides → Feature folders
- Implementation documentation → Feature folders
- Setup guides → Feature folders

This keeps the root directory focused on cross-cutting concerns and processes.