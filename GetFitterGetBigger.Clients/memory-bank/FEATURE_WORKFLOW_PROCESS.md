# Feature Workflow Process

This document defines the complete lifecycle of features, from inception to release, using a state-based folder structure.

## Feature States and Folder Structure

```
memory-bank/features/
├── 0-SUBMITTED/            # Features propagated from API, needs refinement
├── 1-READY_TO_DEVELOP/     # Features planned and ready to implement
├── 2-IN_PROGRESS/          # Features currently being developed
├── 3-COMPLETED/            # Features fully implemented and tested
├── 4-BLOCKED/              # Features blocked by dependencies
└── 5-SKIPPED/              # Features deferred or cancelled
```

## Feature Lifecycle

### 0. Feature Submission (SUBMITTED)
When features are propagated from API changes:
1. Root folder creates: `memory-bank/features/0-SUBMITTED/[feature-name]/`
2. Contains only `feature-description.md` with:
   - API endpoint changes
   - Request/response formats
   - Business context
   - Potential impact on client platforms
3. NO task files yet - these are created during refinement

### 1. Feature Refinement & Planning (READY_TO_DEVELOP)
When the team is ready to implement:
1. Review the submitted feature documentation
2. Analyze platform-specific requirements (Mobile, Web, Desktop)
3. Create `feature-tasks.md` with platform-specific tasks
4. Move entire folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`
5. The team decides when this transition happens based on:
   - Current priorities
   - Resource availability per platform
   - Dependencies readiness

#### Direct Feature Creation (Without API Propagation)
When a new feature is identified:
1. Create a folder: `memory-bank/features/1-READY_TO_DEVELOP/[feature-name]/`
2. Create two files in the folder:
   - `feature-description.md` - Detailed feature specification
   - `feature-tasks.md` - Implementation task list
3. Optional: Add any supporting documents, mockups, or platform-specific designs

**Folder Structure Example:**
```
1-READY_TO_DEVELOP/
└── workout-tracking/
    ├── feature-description.md
    ├── feature-tasks.md
    ├── mobile-ui-mockups.png
    └── desktop-wireframes.pdf
```

### 2. Feature Development (IN_PROGRESS)
When development begins:
1. Move entire feature folder from `1-READY_TO_DEVELOP` to `2-IN_PROGRESS`
2. Create feature branch as specified in tasks file
3. Update task statuses as work progresses
4. Add commit hashes to completed tasks
5. Include any test scripts created during development

**Folder Structure Example:**
```
2-IN_PROGRESS/
└── workout-tracking/
    ├── feature-description.md
    ├── feature-tasks.md (with commit hashes)
    ├── mobile-ui-mockups.png
    ├── test-mobile-navigation.sh
    └── test-desktop-sync.ps1
```

### 3. Feature Completion (COMPLETED)
When all tasks are implemented and tested:
1. Ensure all tests are green on all platforms
2. Get user acceptance (if manual testing phase)
3. Calculate final time metrics in feature-tasks.md:
   - Sum all actual durations
   - Calculate AI assistance impact percentage
   - Add final summary of time savings
4. Move entire folder from `2-IN_PROGRESS` to `3-COMPLETED`
5. Add completion date and time metrics to feature-description.md
6. Feature is now eligible for next PI release

### 4. Blocked Features (BLOCKED)
If a feature cannot proceed:
1. Move folder from current location to `4-BLOCKED`
2. Add `BLOCKED_REASON.md` file explaining:
   - What is blocking the feature
   - Dependencies needed
   - Affected platforms
   - Link to blocking bug/feature
3. When unblocked, move back to `2-IN_PROGRESS`

### 5. Skipped Features (SKIPPED)
If a feature is deferred or cancelled:
1. Move folder to `5-SKIPPED`
2. Add `SKIPPED_REASON.md` file explaining:
   - Why feature was skipped
   - Future considerations
   - Any partial work completed
   - Platform-specific considerations

## File Templates

### feature-description.md Template
```markdown
# Feature: [Feature Name]

## Feature ID: FEAT-[XXX]
## Created: [Date]
## Status: [Current State]
## Target PI: [PI-YYYY-QX]
## Platforms: [Mobile, Web, Desktop, or specific subset]

## Description
[Detailed description of the feature]

## Business Value
[Why this feature is important]

## User Stories
- As a [user type], I want to [action] so that [benefit]

## Acceptance Criteria
- [ ] Criteria 1
- [ ] Criteria 2

## Platform-Specific Requirements
### Mobile
- [Mobile-specific requirements]

### Web
- [Web-specific requirements]

### Desktop
- [Desktop-specific requirements]

## Technical Specifications
[Any technical details, API requirements, etc.]

## Dependencies
- [List any dependencies]

## Notes
[Additional context or considerations]
```

### feature-tasks.md Template
```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`
## Estimated Total Time: [X days / Y hours]
## Actual Total Time: [To be calculated at completion]

### Task Categories
[Tasks organized by platform and logical groupings with time estimates]

### Progress Tracking
- All tasks start as `[ReadyToDevelop]` with time estimate
- Update to `[InProgress: Started: YYYY-MM-DD HH:MM]` when starting
- Update to `[Implemented: <hash> | Started: <time> | Finished: <time> | Duration: Xh Ym]` when complete
- Use `[Blocked: reason]` if blocked
- Use `[N/A - Platform]` if not applicable

### Tasks
[Detailed task list following FEATURE_IMPLEMENTATION_PROCESS.md with estimates]

### Time Tracking Summary
- **Total Estimated Time:** [Sum of all estimates]
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]
```

## State Transition Rules

1. **To IN_PROGRESS**: Only when developer is actively working on it
2. **To COMPLETED**: Only when ALL tasks are implemented and tests pass on ALL platforms
3. **To BLOCKED**: As soon as a blocking issue is identified
4. **To SKIPPED**: Only with explicit user decision
5. **From BLOCKED**: Only when blocking issue is resolved
6. **From SKIPPED**: Rare, but possible if feature is revived

## Integration with Release Process

- Only features in `3-COMPLETED` are included in releases
- Features must be completed before PI end date
- Release notes are generated from completed features
- Each feature folder contains all documentation needed for release notes
- Platform-specific completion can be tracked separately

## Best Practices

1. **One Feature Per Folder**: Keep features atomic and focused
2. **Complete Documentation**: All files should be comprehensive
3. **Test Scripts**: Include platform-specific test scripts
4. **Clean Transitions**: Move entire folder, don't leave artifacts
5. **Audit Trail**: Preserve all history in the moved files
6. **Platform Clarity**: Always specify which platforms are affected

## Monitoring and Reporting

To get current feature status:
- Count folders in each state directory
- Generate reports from feature-description.md files
- Track velocity by completion dates
- Identify bottlenecks in BLOCKED folder
- Monitor platform-specific progress
- Generate AI impact reports:
  - Average time reduction percentage across features
  - Total hours saved with AI assistance
  - Productivity metrics comparison by platform
  - Feature complexity vs time savings analysis

## Cleanup Policy

- COMPLETED features older than 2 PIs can be archived
- SKIPPED features older than 4 PIs can be archived
- Archive location: `memory-bank/archive/features/[year]/`

## Common Mistakes to Avoid

1. **Starting work without proper folder structure**: Always create the feature folder in the appropriate state directory BEFORE starting implementation
2. **Creating task files in wrong location**: Task files must be inside the feature folder, not directly in `memory-bank/features/`
3. **Not moving folders between states**: Remember to move the entire feature folder as it progresses through states
4. **Missing feature-description.md**: Both description and tasks files are required from the start

### Example of Incorrect Start:
```
❌ memory-bank/features/my-feature-tasks.md  # Wrong location
```

### Example of Correct Start:
```
✅ memory-bank/features/1-READY_TO_DEVELOP/my-feature/
   ├── feature-description.md
   └── feature-tasks.md
```