# Features Documentation

This directory contains detailed documentation for all features in the GetFitterGetBigger Admin Application. The documentation is organized in a feature-centric approach to make it easier to find information about specific features and their implementation status.

## Directory Structure

```
features/
├── feature-status.md (index of all features and their status)
├── FEATURE_ID_REGISTRY.md (central registry of all feature IDs)
├── NEXT_FEATURE_ID.txt (tracks next available feature ID)
├── 0-SUBMITTED/
│   └── FEAT-XXX-[feature-name]/
├── 1-READY_TO_DEVELOP/
│   └── FEAT-XXX-[feature-name]/
├── 2-IN_PROGRESS/
│   └── FEAT-XXX-[feature-name]/
├── 3-COMPLETED/
│   └── FEAT-XXX-[feature-name]/
├── 4-BLOCKED/
│   └── FEAT-XXX-[feature-name]/
└── 5-SKIPPED/
    └── FEAT-XXX-[feature-name]/
```

Each feature folder contains:
- **feature-description.md**: Core feature details and requirements
- **feature-tasks.md**: Detailed task breakdown
- **overview.md**: Feature description, status, implementation details (legacy)
- **tasks.md**: Tasks related to the feature (legacy)
- **issues.md**: Issues and their resolutions (legacy)
- **implementation-summary.md**: Summary of completed implementation
- **testing-guide.md**: Testing instructions for the feature

## How to Navigate

1. Start with the [Feature Status Dashboard](feature-status.md) to get an overview of all features and their current status.
2. For each feature, you can find detailed information in its dedicated directory:
   - **overview.md**: Description, implementation details, and related components
   - **tasks.md**: Completed, pending, and blocked tasks
   - **issues.md**: Active and resolved issues

## Feature Status Definitions

- **SUBMITTED**: Feature proposal submitted for review and approval
- **READY TO DEVELOP**: Feature approved and ready for implementation
- **IN PROGRESS**: Work has started but is not complete
- **COMPLETED**: Feature is fully built, tested, and working as expected
- **BLOCKED**: Feature cannot proceed due to dependencies or business decisions
- **SKIPPED**: Feature was cancelled or deferred indefinitely

## Task Status Definitions

- **TODO**: Task is identified but not started
- **IN PROGRESS**: Task is currently being worked on
- **COMPLETED**: Task is finished and verified
- **BLOCKED**: Task cannot proceed due to dependencies

## Documentation Process

For information about the documentation process and guidelines, please refer to the [Documentation Workflow](/memory-bank/documentation-workflow.md) file.
