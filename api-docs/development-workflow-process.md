# Development Workflow Process

This document describes the unified development workflow used across all GetFitterGetBigger projects (API, Admin, Clients). All projects follow the same state-based folder structure and processes for features and bugs.

## Overview

The workflow process is designed to:
- Provide clear visibility of work status
- Maintain audit trails through commit tracking
- Ensure quality through mandatory testing
- Support both continuous and interrupted development
- Enable easy release management

## Feature Development Workflow

### Folder Structure
All projects use the same state-based folder structure for features:

```
memory-bank/features/
├── 0-SUBMITTED/            # Features propagated but not refined (NEW)
├── 1-READY_TO_DEVELOP/     # Features planned and ready to implement
├── 2-IN_PROGRESS/          # Features currently being developed
├── 3-COMPLETED/            # Features fully implemented and tested
├── 4-BLOCKED/              # Features blocked by dependencies
└── 5-SKIPPED/              # Features deferred or cancelled
```

### Feature Lifecycle

1. **SUBMITTED**: Feature propagated from another project, needs refinement
2. **READY_TO_DEVELOP**: Feature is fully specified with tasks defined
3. **IN_PROGRESS**: Active development with commit tracking
4. **COMPLETED**: All tasks done, tests passing, user accepted
5. **BLOCKED**: Cannot proceed due to dependencies
6. **SKIPPED**: Deferred or cancelled with documented reasons

### Required Files

Each feature folder must contain different files based on state:

#### 0-SUBMITTED
- `feature-description.md` - API changes, endpoints, impact analysis
- No tasks file yet (created during refinement)

#### 1-READY_TO_DEVELOP through 5-SKIPPED  
- `feature-description.md` - Business value, user stories, acceptance criteria
- `feature-tasks.md` - Detailed implementation tasks with status tracking

### Feature Creation Process

**IMPORTANT**: ALL features MUST start in `0-SUBMITTED` state:

1. **Initial Creation**: Create feature in `0-SUBMITTED` with description only
2. **No Direct Creation**: NEVER create features directly in `1-READY_TO_DEVELOP`
3. **Refinement Required**: Teams must refine and add tasks before moving forward
4. **Transition Control**: Only move to `1-READY_TO_DEVELOP` when fully planned

This applies to ALL features:
- API-originated features
- User-requested features  
- Team-identified features
- Bug-to-feature conversions

### Implementation Process

1. **Refinement** (0-SUBMITTED → 1-READY_TO_DEVELOP): Analyze impact, create tasks
2. **Planning**: Create comprehensive task list
3. **Development**: Execute tasks sequentially
4. **Testing**: Write tests immediately after implementation
5. **Verification**: Ensure all tests pass and build succeeds
6. **Acceptance**: Get user approval before completion

## Bug Management Workflow

### Folder Structure
All projects use the same state-based folder structure for bugs:

```
memory-bank/bugs/
├── 1-OPEN/          # Newly reported bugs awaiting triage
├── 1-TODO/          # Triaged bugs ready to work on  
├── 2-IN_PROGRESS/   # Bugs currently being fixed
├── 3-FIXED/         # Bugs that have been resolved
├── 4-BLOCKED/       # Bugs blocked by dependencies
└── 5-WONT_FIX/     # Bugs that won't be addressed
```

### Bug Lifecycle

1. **OPEN/TODO**: Bug reported and triaged
2. **IN_PROGRESS**: Fix being developed with test-first approach
3. **FIXED**: Tests passing, fix verified by user
4. **BLOCKED**: Cannot fix due to dependencies
5. **WONT_FIX**: Decision not to fix with justification

### Required Files

Each bug folder must contain:
- `bug-report.md` - Description, reproduction steps, impact
- `bug-tasks.md` - Fix implementation tasks with test-first approach

### Test-First Approach

1. **Write failing tests** that reproduce the bug
2. **Implement minimal fix** to make tests pass
3. **Verify all tests** are green before completion
4. **Create manual test scripts** for verification

## Common Rules Across All Projects

### Boy Scout Rule (MANDATORY)
When working on ANY task:
- Fix ALL failing tests encountered
- Fix ALL build/lint warnings in touched files
- Clean up code smells and dead code
- Update outdated documentation
- Leave the codebase better than you found it

### Task Status Tracking
All tasks use consistent status indicators:
- `[ReadyToDevelop]` or `[TODO]` - Task identified
- `[PROGRESS]` - Currently working on task
- `[Implemented: <hash>]` or `[IMPLEMENTED: <hash>]` - Completed with commit reference
- `[BLOCKED: <reason>]` - Cannot proceed
- `[Skipped]` - Determined unnecessary

### Quality Gates
Before marking any task complete:
- Implementation code is complete
- Tests are written and passing
- Build succeeds with minimal warnings
- Linting passes (where applicable)
- No regression in existing functionality

## Project-Specific Considerations

### API Project
- Use `dotnet build` and `dotnet test`
- Follow C# coding standards
- Ensure Swagger documentation
- Reference: `/GetFitterGetBigger.API/memory-bank/`

### Admin Project  
- Use `npm run build`, `npm test`, `npm run lint`
- Follow React/Blazor patterns
- Ensure responsive design
- Reference: `/GetFitterGetBigger.Admin/memory-bank/`

### Clients Project
- Platform-specific build commands
- Follow platform UI guidelines
- Test on target devices
- Reference: `/GetFitterGetBigger.Clients/memory-bank/`

## Release Process Integration

- Only features in `3-COMPLETED` are included in releases
- Only bugs in `3-FIXED` are included in release notes
- Features and bugs are organized by PI (Program Increment)
- Release notes are generated from completed work

## Quick Reference Documents

Each project maintains a `TESTING-QUICK-REFERENCE.md` with:
- Common test failures and solutions
- Project-specific testing patterns
- Debugging checklists
- Tool-specific commands

## Benefits of This Process

1. **Consistency**: Same workflow across all projects
2. **Visibility**: Clear status of all work items
3. **Quality**: Mandatory testing and cleanup
4. **Traceability**: Commit tracking for all changes
5. **Flexibility**: Supports various development scenarios

## Getting Started

1. Check the project's memory-bank for existing features/bugs
2. Follow the appropriate workflow process document
3. Reference TESTING-QUICK-REFERENCE when debugging
4. Maintain status updates as work progresses
5. Move folders through states as work advances

Remember: The goal is sustainable, high-quality development with clear communication and documentation.