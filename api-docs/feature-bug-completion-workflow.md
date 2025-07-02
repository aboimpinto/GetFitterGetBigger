# Feature/Bug Completion Workflow

## CRITICAL: Completion is NEVER Automatic

This document defines the **mandatory** workflow for completing features and bugs across all GetFitterGetBigger projects.

## Key Principles

### 1. Manual Testing is MANDATORY
- **NO feature or bug can be marked as COMPLETE without manual testing**
- Manual testing must be performed by the user/developer
- AI assistants CANNOT perform manual testing
- AI assistants CANNOT move items to COMPLETE state without explicit user confirmation

### 2. Completion Process is Human-Driven
- The transition to COMPLETE state is **NEVER AUTOMATIC**
- It requires explicit human validation and approval
- Manual testing results must be satisfactory before completion

## Completion Workflow Steps

### Step 1: Code Complete
When all implementation tasks are finished:
1. Ensure all unit tests pass
2. Ensure build succeeds
3. Ensure linting passes
4. Update documentation as needed

### Step 2: Manual Testing Phase
**THIS STEP IS MANDATORY AND CANNOT BE SKIPPED**

The user/developer must:
1. Run the application locally
2. Test all affected functionality manually
3. Verify the feature/bug fix works as expected
4. Test edge cases and error scenarios
5. Ensure no regressions were introduced

### Step 3: Request Completion
Only after successful manual testing:
1. The user/developer explicitly requests to move to COMPLETE state
2. This request must be explicit (e.g., "Move FEAT-XXX to complete" or "Mark BUG-XXX as complete")
3. AI assistants should NOT suggest moving to complete without being asked

### Step 4: Transition to COMPLETE
After receiving explicit user approval:
1. Move the feature/bug folder to the appropriate COMPLETED state folder
2. Create/update the completion-summary.md
3. Update any tracking documents

### Step 5: Post-Completion Actions
After successful completion:
1. Merge changes to master branch
2. After successful merge, delete the feature/bug branch locally
3. Delete the feature/bug branch remotely
4. Update project documentation if needed

## Important Reminders for AI Assistants

When working on features or bugs:
- **NEVER** automatically transition to COMPLETE state
- **ALWAYS** wait for explicit user approval after manual testing
- **REMIND** users about manual testing requirements when implementation is done
- **ASK** for manual testing results before proceeding with completion

## Example Workflow Dialogue

```
AI: I've completed all implementation tasks for FEAT-123. The code is ready for manual testing.

User: [Performs manual testing]

User: Manual tests passed. Please move FEAT-123 to complete.

AI: Moving FEAT-123 to COMPLETED state and updating documentation.
```

## References
- [Development Workflow Process](./development-workflow-process.md)
- [Feature Workflow Process](../GetFitterGetBigger.API/memory-bank/feature-workflow-process.md)
- [Bug Workflow Process](../GetFitterGetBigger.API/memory-bank/bug-workflow-process.md)