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
3. **IN_PROGRESS**: Active development with commit tracking and time tracking
4. **COMPLETED**: All tasks done, tests passing, user explicitly accepted
5. **BLOCKED**: Cannot proceed due to dependencies
6. **SKIPPED**: Deferred or cancelled with documented reasons

### Required Files

Each feature folder must contain different files based on state:

#### 0-SUBMITTED
- `feature-description.md` - API changes, endpoints, impact analysis
- No tasks file yet (created during refinement)

#### 1-READY_TO_DEVELOP through 5-SKIPPED  
- `feature-description.md` - Business value, user stories, acceptance criteria, Feature ID
- `feature-tasks.md` - Detailed implementation tasks with status tracking and time estimates

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
3. **Baseline Health Check**: Mandatory health check before starting (see `baseline-health-check-process.md`)
4. **Development**: Execute tasks sequentially
5. **Testing**: Write tests immediately after implementation
6. **Verification**: Ensure all tests pass and build succeeds
7. **Quality Report**: Compare baseline to final metrics
8. **Acceptance**: Get user approval before completion

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
- `bug-report.md` - Description, reproduction steps, impact, Related Feature reference
- `bug-tasks.md` - Fix implementation tasks with test-first approach and Boy Scout cleanup

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
- `[Implemented: <hash>]` - Completed with commit reference (simple format)
- `[Implemented: <hash> | Started: YYYY-MM-DD HH:MM | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]` - With time tracking
- `[BLOCKED: <reason>]` - Cannot proceed
- `[Skipped]` - Determined unnecessary

### Time Tracking Requirements

All tasks must include time tracking with the following format:
- **Estimated Time**: Include in task definition
- **Actual Time**: Track using the extended format above
- **Time Summary**: Calculate total time and AI assistance impact
- **Format**: Use 24-hour timestamps, round to 5-minute increments

Example:
```
### Time Summary
- **Estimated Time**: 3h 
- **Actual Time**: 1h 25m
- **Time Saved**: 1h 35m (52.8% reduction)
- **AI Assistance Impact**: High - Significant time savings
```

### Quality Gates
Before marking any task complete:
- Implementation code is complete
- Tests are written and passing (>80% branch coverage target)
- Build succeeds with NO warnings in touched files
- Linting passes (where applicable)
- No regression in existing functionality
- User explicitly approves for feature completion
- Time tracking is complete with AI impact assessment

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

## Key Process Rules

1. **Feature IDs**: Always use Feature IDs (FEAT-XXX) in references, not folder paths
2. **0-SUBMITTED is Mandatory**: ALL features must start in 0-SUBMITTED, no exceptions
3. **User Approval**: Required before moving features to COMPLETED
4. **AI Impact Tracking**: Document time savings and efficiency gains
5. **Test Coverage**: Target >80% branch coverage for new code

## Getting Started

1. Check the project's memory-bank for existing features/bugs
2. Follow the appropriate workflow process document
3. Reference TESTING-QUICK-REFERENCE when debugging
4. Maintain status updates as work progresses
5. Move folders through states as work advances

Remember: The goal is sustainable, high-quality development with clear communication and documentation.