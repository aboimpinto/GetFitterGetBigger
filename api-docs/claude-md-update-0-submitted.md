# CLAUDE.md Update for 0-SUBMITTED State

## Update Required for All Projects

Add the following section to each project's CLAUDE.md file:

### For API Project

```markdown
## Feature Workflow States

Features progress through these states:
- `0-SUBMITTED`: Not used in API (API originates features)
- `1-READY_TO_DEVELOP`: Fully planned with tasks defined
- `2-IN_PROGRESS`: Currently being implemented
- `3-COMPLETED`: Done and tested
- `4-BLOCKED`: Dependencies preventing progress
- `5-SKIPPED`: Deferred or cancelled
```

### For Admin and Clients Projects

```markdown
## Feature Workflow States

Features progress through these states:
- `0-SUBMITTED`: Propagated from API, needs project-specific refinement
- `1-READY_TO_DEVELOP`: Refined with tasks and ready to implement
- `2-IN_PROGRESS`: Currently being implemented
- `3-COMPLETED`: Done and tested
- `4-BLOCKED`: Dependencies preventing progress
- `5-SKIPPED`: Deferred or cancelled

### Working with 0-SUBMITTED Features

When features arrive in `0-SUBMITTED`:
1. Review the API contract and changes
2. Analyze impact on this project
3. Create implementation tasks
4. Add time estimates
5. Move to `1-READY_TO_DEVELOP` when ready

Remember: You decide when a feature is ready to develop based on your project's needs.
```

## Propagation Instructions

When updating CLAUDE.md files:
1. Add the workflow states section if not present
2. Ensure the distinction between API and Admin/Clients is clear
3. Emphasize project autonomy in transition decisions