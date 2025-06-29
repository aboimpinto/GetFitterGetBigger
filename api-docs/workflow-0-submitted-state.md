# The 0-SUBMITTED Workflow State

## Purpose

The `0-SUBMITTED` state represents features that have been propagated from one project to another but are not yet ready for development. This state serves as a holding area for features that need project-specific refinement before implementation can begin.

## Key Concepts

### Separation of Concerns
- **Root Folder**: Documents and propagates API changes/features
- **Individual Projects**: Refine and plan implementation based on their specific needs
- **API as Black Box**: Admin and Client projects only know the API contract, not implementation details

### When to Use 0-SUBMITTED

**IMPORTANT**: ALL features MUST start in `0-SUBMITTED` state, regardless of source:
- API changes propagated to Admin/Client projects
- New features requested by users
- Features identified by the development team
- Cross-project dependencies
- Any feature request, no matter how simple or complex

**No exceptions**: Even if tasks seem obvious, the feature must go through 0-SUBMITTED for proper workflow tracking.

## Workflow States (Updated)

```
memory-bank/features/
├── 0-SUBMITTED/         # Features propagated but not refined (NEW)
├── 1-READY_TO_DEVELOP/  # Features refined with tasks and estimates
├── 2-IN_PROGRESS/       # Features currently being developed
├── 3-COMPLETED/         # Features fully implemented and tested
├── 4-BLOCKED/          # Features blocked by dependencies
└── 5-SKIPPED/          # Features deferred or cancelled
```

## Content Requirements for 0-SUBMITTED

### What to Include
- **Feature Description**: Detailed explanation of what changed/is available
- **API Contract** (when applicable):
  - Endpoint URLs
  - HTTP methods
  - Request parameters and format
  - Response format and examples
  - Authentication/authorization requirements
  - Error responses
- **Business Context**: Why this feature exists
- **Impact Analysis**: What this might affect in the current project

### What NOT to Include
- Implementation tasks (these come during refinement)
- Time estimates
- Technical implementation details
- Project-specific UI/UX decisions

## Transition Criteria

### From 0-SUBMITTED to 1-READY_TO_DEVELOP

The development team moves a feature when:
1. They understand the feature requirements completely
2. All dependencies are identified and available
3. Implementation tasks are defined
4. Time estimates are added
5. The team is confident they have everything needed to start

### Who Triggers Transition
- **Individual project teams** decide when to move features
- Not automatic or mandated by root folder
- Based on project priorities and readiness

## Example Structure

```
0-SUBMITTED/
└── muscle-group-crud-api/
    └── feature-description.md    # API changes and impact

1-READY_TO_DEVELOP/
└── muscle-group-inline-creation/
    ├── feature-description.md    # Refined with project context
    └── feature-tasks.md          # Detailed implementation plan
```

## Benefits

1. **Clear Communication**: API changes are documented immediately
2. **Project Autonomy**: Each team refines based on their needs
3. **Better Planning**: Upcoming changes are visible early
4. **Quality Gate**: Forces proper planning before development

## Implementation Guidelines

### For Root Folder (Propagation)
1. Document API changes comprehensively
2. Focus on contract, not implementation
3. Place in `0-SUBMITTED` folder of target projects
4. No need to create tasks or estimates

### For Project Teams (Refinement)
1. Review submitted features regularly
2. Analyze impact on current project
3. Create implementation tasks
4. Move to `1-READY_TO_DEVELOP` when ready

## Important Notes

- Features can be propagated from API whether `3-COMPLETED` or `2-IN_PROGRESS`
- Admin/Clients treat API as external service (black box)
- Root folder propagates based on developer/team indication
- No automatic transitions between states