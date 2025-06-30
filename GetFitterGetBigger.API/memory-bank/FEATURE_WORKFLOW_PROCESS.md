# Feature Workflow Process

This document defines the complete lifecycle of features, from inception to release, using a state-based folder structure.

## Feature States and Folder Structure

```
memory-bank/features/
├── 0-SUBMITTED/            # Features propagated but not refined (rarely used in API)
├── 1-READY_TO_DEVELOP/     # Features planned and ready to implement
├── 2-IN_PROGRESS/          # Features currently being developed
├── 3-COMPLETED/            # Features fully implemented and tested
├── 4-BLOCKED/              # Features blocked by dependencies
└── 5-SKIPPED/              # Features deferred or cancelled
```

## Important File Management Rules

**⚠️ CRITICAL: Only create the REQUIRED files for each state! ⚠️**

- **DO NOT create unnecessary files** like README.md, notes.txt, or other documentation
- **Each state has SPECIFIC required files** - stick to these requirements
- **Avoid file duplication** - don't create files that duplicate information already in required files
- **Focus on the necessary** - extra files add confusion and maintenance overhead

Required files by state:
- **0-SUBMITTED**: Only `feature-description.md`
- **1-READY_TO_DEVELOP**: `feature-description.md` and `feature-tasks.md`
- **2-IN_PROGRESS**: Same files + completion artifacts as work progresses
- **3-COMPLETED**: Same files + `completion-summary.md`

## Feature Lifecycle

### 0. Feature Submission (SUBMITTED) - MANDATORY STARTING POINT
**IMPORTANT**: ALL features MUST start in 0-SUBMITTED state, even in the API project:
1. Assign the next Feature ID from `/memory-bank/features/NEXT_FEATURE_ID.txt`
2. Create folder: `memory-bank/features/0-SUBMITTED/FEAT-XXX-[feature-name]/`
   - Replace XXX with the assigned Feature ID (e.g., FEAT-009-new-feature)
3. Add only `feature-description.md` with the feature details (must include Feature ID)
4. Update `NEXT_FEATURE_ID.txt` to the next number
5. NO task files yet - these are created during refinement
6. When ready to plan, create tasks and move to `1-READY_TO_DEVELOP`

This ensures consistent workflow tracking across all projects.

### 1. Feature Planning (READY_TO_DEVELOP)
When a new feature is identified:
1. Assign the next Feature ID from `/memory-bank/features/NEXT_FEATURE_ID.txt`
2. Create a folder: `memory-bank/features/1-READY_TO_DEVELOP/FEAT-XXX-[feature-name]/`
   - Replace XXX with the assigned Feature ID (e.g., FEAT-007-integration-tests-fix)
3. Create two files in the folder:
   - `feature-description.md` - Detailed feature specification (must include Feature ID)
   - `feature-tasks.md` - Implementation task list
4. Update `NEXT_FEATURE_ID.txt` to the next number
5. Optional: Add any supporting documents, mockups, or scripts

**Folder Structure Example:**
```
1-READY_TO_DEVELOP/
└── FEAT-008-exercise-management/
    ├── feature-description.md
    ├── feature-tasks.md
    └── api-endpoints-spec.md
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
└── FEAT-008-exercise-management/
    ├── feature-description.md
    ├── feature-tasks.md (with commit hashes)
    ├── api-endpoints-spec.md
    └── test-exercise-endpoints.sh
```

### 3. Feature Completion (COMPLETED)
**⚠️ CRITICAL: ONLY THE USER CAN MOVE FEATURES TO COMPLETED! ⚠️**

The AI assistant should NEVER move features to COMPLETED. When all implementation tasks are done:

1. **AI ASSISTANT RESPONSIBILITIES:**
   - Verify ALL tasks in feature-tasks.md are marked as `[Implemented]` with commit hashes
   - Ensure all automated tests are green
   - Calculate final time metrics in feature-tasks.md
   - Update all checkboxes to checked [x] in feature-tasks.md
   - Create `completion-summary.md` with implementation summary
   - **NOTIFY THE USER** that implementation is complete and ready for review

2. **USER RESPONSIBILITIES (ONLY THE USER CAN DO THESE):**
   - Perform manual testing and validation
   - Review all code changes
   - Verify acceptance criteria are met
   - Make the decision to move to COMPLETED
   - Move entire folder from `2-IN_PROGRESS` to `3-COMPLETED`
   - Add completion date to feature-description.md
   - Approve for PI release

**IMPORTANT**: Features CANNOT be moved to COMPLETED if:
- Any tasks remain in `[ReadyToDevelop]` or `[InProgress]` state
- User has not performed manual testing
- Code review is not complete
- User has not explicitly approved the completion

### 4. Blocked Features (BLOCKED)
If a feature cannot proceed:
1. Move folder from current location to `4-BLOCKED`
2. Add `BLOCKED_REASON.md` file explaining:
   - What is blocking the feature
   - Dependencies needed
   - Link to blocking bug/feature
3. When unblocked, move back to `2-IN_PROGRESS`

### 5. Skipped Features (SKIPPED)
If a feature is deferred or cancelled:
1. Move folder to `5-SKIPPED`
2. Add `SKIPPED_REASON.md` file explaining:
   - Why feature was skipped
   - Future considerations
   - Any partial work completed

## File Templates

### feature-description.md Template
```markdown
# Feature: [Feature Name]

## Feature ID: FEAT-[XXX]
## Created: [Date]
## Status: [Current State]
## Target PI: [PI-YYYY-QX]

## Description
[Detailed description of the feature]

## Business Value
[Why this feature is important]

## User Stories
- As a [user type], I want to [action] so that [benefit]

## Acceptance Criteria
- [ ] Criteria 1
- [ ] Criteria 2

## Technical Specifications
[Any technical details, API specs, etc.]

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
[Tasks organized by logical groupings with time estimates]

### Progress Tracking
- All tasks start as `[ReadyToDevelop]` with time estimate
- Update to `[InProgress: Started: YYYY-MM-DD HH:MM]` when starting
- Update to `[Implemented: <hash> | Started: <time> | Finished: <time> | Duration: Xh Ym]` when complete
- Use `[Blocked: reason]` if blocked

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
2. **To COMPLETED**: Only when ALL tasks are implemented and tests pass
3. **To BLOCKED**: As soon as a blocking issue is identified
4. **To SKIPPED**: Only with explicit user decision
5. **From BLOCKED**: Only when blocking issue is resolved
6. **From SKIPPED**: Rare, but possible if feature is revived

## Integration with Release Process

- Only features in `3-COMPLETED` are included in releases
- Features must be completed before PI end date
- Release notes are generated from completed features
- Each feature folder contains all documentation needed for release notes

## Best Practices

1. **One Feature Per Folder**: Keep features atomic and focused
2. **Complete Documentation**: All files should be comprehensive
3. **Test Scripts**: Include all test scripts in the feature folder
4. **Clean Transitions**: Move entire folder, don't leave artifacts
5. **Audit Trail**: Preserve all history in the moved files

## Monitoring and Reporting

To get current feature status:
- Count folders in each state directory
- Generate reports from feature-description.md files
- Track velocity by completion dates
- Identify bottlenecks in BLOCKED folder
- Generate AI impact reports:
  - Average time reduction percentage across features
  - Total hours saved with AI assistance
  - Productivity metrics comparison
  - Feature complexity vs time savings analysis

## Cleanup Policy

- COMPLETED features older than 2 PIs can be archived
- SKIPPED features older than 4 PIs can be archived
- Archive location: `memory-bank/archive/features/[year]/`

## Feature ID Management

1. **Feature ID Format**: FEAT-XXX (e.g., FEAT-001, FEAT-007, FEAT-099)
2. **Assignment**: Always use the next available ID from `NEXT_FEATURE_ID.txt`
3. **Folder Naming**: All feature folders MUST be prefixed with their Feature ID
4. **Consistency**: Feature ID must appear in:
   - Folder name: `FEAT-007-integration-tests-fix/`
   - feature-description.md: `## Feature ID: FEAT-007`
   - Bug references: "Fixed in FEAT-007"
   - Commit messages: "feat(FEAT-007): implement PostgreSQL tests"

## Common Mistakes to Avoid

1. **Not using Feature IDs**: All features MUST have a Feature ID prefix in their folder name
2. **Starting work without proper folder structure**: Always create the feature folder in the appropriate state directory BEFORE starting implementation
3. **Creating task files in wrong location**: Task files must be inside the feature folder, not directly in `memory-bank/features/`
4. **Not moving folders between states**: Remember to move the entire feature folder as it progresses through states
5. **Missing feature-description.md**: Both description and tasks files are required from the start
6. **Using folder paths in references**: Always use Feature IDs (FEAT-XXX) not folder paths when referencing features

### Example of Incorrect Start:
```
❌ memory-bank/features/my-feature-tasks.md  # Wrong location
❌ memory-bank/features/1-READY_TO_DEVELOP/my-feature/  # Missing Feature ID
```

### Example of Correct Start:
```
✅ memory-bank/features/1-READY_TO_DEVELOP/FEAT-009-my-feature/
   ├── feature-description.md  # Contains Feature ID: FEAT-009
   └── feature-tasks.md
```

## Feature ID Sequence

- Use sequential numbering: FEAT-001, FEAT-002, etc.
- Maintain a `NEXT_FEATURE_ID.txt` file in features folder
- Never reuse feature IDs
- Format: FEAT-[3-digit-number]
- Feature IDs are permanent and don't change when features move between states