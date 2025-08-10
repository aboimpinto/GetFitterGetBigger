# Claude Slash Commands Reference

This directory contains custom slash commands for Claude to use in the GetFitterGetBigger API project.

## Overview

Slash commands provide quick access to common development workflows. They ensure consistent processes and help maintain code quality throughout the development lifecycle.

## Code Review Commands

### `/feature-code-review`
Performs a comprehensive final code review for the entire current feature.
- Uses: `FINAL-CODE-REVIEW-TEMPLATE.md`
- Output: Complete feature review report with status (APPROVED/APPROVED_WITH_NOTES/REQUIRES_CHANGES)
- Location: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/`
- When to use: Before moving a feature to COMPLETED status

### `/file-code-review`
Performs a detailed code review of a specific file.
- Uses: `CODE_QUALITY_STANDARDS.md` and `CodeQualityGuidelines/`
- Output: Detailed file analysis with line-by-line feedback
- When to use: When you need to review a single file in detail

## Feature Development Commands

### `/start-implementing [FEAT-XXX]`
Begin implementing a feature from the READY_TO_DEVELOP folder.
- Creates feature branch
- Moves feature to IN_PROGRESS
- Starts with first task category
- Pre-checks: No other feature in progress, clean working directory

### `/continue-implementation`
Continue working on the current in-progress feature.
- Reviews remaining tasks
- Continues from last checkpoint
- Stops at next checkpoint for review
- Updates task progress

### `/continue-implementation-nonstop`
Continue implementation without stopping for user confirmation.
- Implements ALL remaining tasks
- Auto-commits at logical points
- Runs tests between components
- Only stops on errors or completion
- ⚠️ Warning: No user review until completion

### `/complete-feature`
Complete the current feature and move it to COMPLETED status.
- Validates all tasks completed
- Ensures all tests pass
- Performs final code review
- Updates documentation
- Pre-requisites: All acceptance criteria met

### `/refine-feature [FEAT-XXX]`
Refine a SUBMITTED feature to prepare for implementation.
- Enhances requirements
- Creates detailed task breakdown
- Identifies dependencies
- Moves to READY_TO_DEVELOP when complete

## Bug Management Commands

### `/new-bug [description]`
Create a new bug report in the proper structure.
- Generates next bug ID
- Creates bug folder in 1-OPEN
- Includes reproduction steps and proposed solution
- Creates implementation tasks

## Other Commands

### `/catch-up`
Read the memory-bank and catch up on project context.
- Reviews current feature status
- Checks recent changes
- Identifies blockers
- Prepares for next action

### `/new-feature [description]`
Create a new feature request in the proper structure.
- Generates next feature ID
- Creates in 0-SUBMITTED status
- Includes user story and acceptance criteria
- Ready for refinement

## Command Flow

### Typical Feature Workflow:
1. `/new-feature` - Create feature request
2. `/refine-feature` - Prepare for development
3. `/start-implementing` - Begin implementation
4. `/continue-implementation` - Work through tasks
5. `/feature-code-review` - Review before completion
6. `/complete-feature` - Mark as done

### Bug Fix Workflow:
1. `/new-bug` - Report the bug
2. `/start-implementing` - Begin fixing
3. `/file-code-review` - Review specific fixes
4. `/complete-feature` - Mark as resolved

## Best Practices

1. **Always `/catch-up` first** when starting a session
2. **Use `/continue-implementation`** for controlled progress with checkpoints
3. **Reserve `/continue-implementation-nonstop`** for well-understood features
4. **Run `/feature-code-review`** before `/complete-feature`**
5. **Create bugs immediately** when found during development

## Command Arguments

Commands that accept arguments use the `$ARGUMENTS` placeholder:
- `/new-feature $ARGUMENTS` - Feature description
- `/new-bug $ARGUMENTS` - Bug description
- `/refine-feature $ARGUMENT` - Feature ID or name
- `/start-implementing $ARGUMENT` - Feature ID or selection

## Integration with Development Process

All commands follow the established processes:
- `DEVELOPMENT_PROCESS.md` - Overall workflow
- `FEATURE_WORKFLOW_PROCESS.md` - Feature states
- `BUG_WORKFLOW_PROCESS.md` - Bug lifecycle
- `CODE_QUALITY_STANDARDS.md` - Quality requirements
- `CODE_REVIEW_PROCESS.md` - Review procedures

## Tips

- Commands can be chained: `/catch-up` → `/continue-implementation`
- Use `/file-code-review` during development for quick checks
- `/feature-code-review` is mandatory before completion
- Check command output for next steps and recommendations