# Feature/Bug Workflow Process Changes

This document summarizes all the recent changes to the Feature/Bug implementation and Workflow process that have been implemented in the API project and must be propagated to Admin and Clients projects.

## Critical Changes

### 1. Mandatory 0-SUBMITTED State

**MOST IMPORTANT CHANGE**: ALL features MUST now start in `0-SUBMITTED` state - NO EXCEPTIONS.

#### What Changed
- Added new `0-SUBMITTED` folder as the mandatory starting point for all features
- Features cannot be created directly in `1-READY_TO_DEVELOP` anymore
- Applies to ALL features: API-originated, user-requested, team-identified, bug-to-feature conversions

#### Why This Change
- Ensures proper planning and refinement before development
- Provides clear separation between propagation and implementation
- Forces teams to analyze impact before committing to development

#### Folder Structure Update
```
memory-bank/features/
├── 0-SUBMITTED/         # NEW - Mandatory starting state
├── 1-READY_TO_DEVELOP/  # Can only move here after refinement
├── 2-IN_PROGRESS/       
├── 3-COMPLETED/         
├── 4-BLOCKED/          
└── 5-SKIPPED/          
```

### 2. Enhanced Time Tracking

**Format**: `[Implemented: <hash> | Started: YYYY-MM-DD HH:MM | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]`

#### Requirements
- Track estimated vs actual time for all tasks
- Calculate AI assistance impact as percentage reduction
- Use 24-hour format timestamps
- Round to nearest 5-minute increment
- Add summary section with totals and AI impact

#### Example
```markdown
### Time Summary
- **Estimated Time**: 3h 
- **Actual Time**: 1h 25m
- **Time Saved**: 1h 35m (52.8% reduction)
- **AI Assistance Impact**: High - Significant time savings through code generation and testing assistance
```

### 3. Boy Scout Rule - Now MANDATORY

When working on ANY task (bug or feature), you MUST:
- Fix ALL failing tests encountered (not just related ones)
- Fix ALL build/lint warnings in touched files  
- Clean up code smells and dead code
- Update outdated documentation
- Leave the codebase better than you found it

**NO EXCEPTIONS** - This is now a mandatory quality gate.

## Process Updates

### Feature Workflow Process

#### State Transitions
1. **0-SUBMITTED**: Feature documented but not refined
   - Only contains `feature-description.md`
   - No tasks file yet
   
2. **1-READY_TO_DEVELOP**: Feature refined with tasks
   - Contains both description and tasks files
   - All tasks defined with estimates
   
3. **2-IN_PROGRESS**: Active development
   - Tasks being implemented
   - Time tracking active
   
4. **3-COMPLETED**: Fully implemented
   - All tasks completed
   - Tests passing
   - User explicitly approved
   
5. **4-BLOCKED**: Cannot proceed
   - Clear blocker documented
   
6. **5-SKIPPED**: Deferred/cancelled
   - Reason documented

#### Key Requirements
- Use Feature IDs (FEAT-XXX) in all references, not folder paths
- User must explicitly approve before moving to COMPLETED
- All tests must pass before completion
- Time tracking mandatory for all tasks

### Bug Workflow Process

#### Folder Structure (No Change)
```
memory-bank/bugs/
├── 1-OPEN/          
├── 1-TODO/          
├── 2-IN_PROGRESS/   
├── 3-FIXED/         
├── 4-BLOCKED/       
└── 5-WONT_FIX/     
```

#### Key Requirements
- Test-first approach mandatory
- Boy Scout Rule applies to all bug fixes
- Add `## Related Feature` section to bug reports
- Reference features by ID (FEAT-XXX)

### Task Status Indicators

Consistent across all projects:
- `[ReadyToDevelop]` or `[TODO]` - Task identified
- `[PROGRESS]` - Currently working on task
- `[Implemented: <hash>]` - Completed with commit reference
- `[Implemented: <full-time-tracking>]` - With time tracking
- `[BLOCKED: <reason>]` - Cannot proceed
- `[Skipped]` - Determined unnecessary

## Quality Standards

### Testing Requirements
- Branch coverage target: >80%
- Test all scenarios: happy path, edge cases, error cases
- Comprehensive validation testing
- Concurrent operation testing
- Write failing tests first for bugs

### Build and Development
- All tests must pass before task completion
- Fix all build warnings in touched files
- No regression in existing functionality
- Lint must pass (where applicable)

## Documentation Requirements

### Feature Documentation
Each feature must have:
- `feature-description.md` with Feature ID field
- Clear business value and user stories
- API contract details (for API features)
- Impact analysis

### Bug Documentation  
Each bug must have:
- `bug-report.md` with reproduction steps
- Related Feature reference
- Impact assessment
- `bug-tasks.md` with test-first tasks

## Implementation Guidelines

### For API Team
1. Create features in `0-SUBMITTED` when ready to propagate
2. Include full API contract documentation
3. Don't include implementation details
4. Let consuming teams refine based on their needs

### For Admin/Clients Teams
1. Review `0-SUBMITTED` features regularly
2. Analyze impact on your project
3. Create implementation tasks during refinement
4. Move to `1-READY_TO_DEVELOP` only when fully planned
5. Follow the enhanced time tracking format
6. Apply Boy Scout Rule to all work

## Migration Path

For existing projects:
1. Create `0-SUBMITTED` folder in features directory
2. Update any in-flight features to use new time tracking
3. Apply Boy Scout Rule going forward
4. Use Feature IDs in all new references
5. Ensure test-first approach for all new bugs

## Benefits

1. **Better Planning**: 0-SUBMITTED state ensures proper refinement
2. **Improved Quality**: Mandatory Boy Scout Rule
3. **Clear Metrics**: Time tracking with AI impact
4. **Consistency**: Same process across all projects
5. **Traceability**: Feature IDs and commit tracking

## Quick Reference

### DO's
- ✅ Start ALL features in 0-SUBMITTED
- ✅ Track time for all tasks
- ✅ Fix all issues encountered (Boy Scout Rule)
- ✅ Use Feature IDs in references
- ✅ Get user approval for COMPLETED

### DON'Ts
- ❌ Create features directly in 1-READY_TO_DEVELOP
- ❌ Skip time tracking
- ❌ Ignore failing tests or warnings
- ❌ Use folder paths in references
- ❌ Mark COMPLETED without user approval

## See Also
- `development-workflow-process.md` - Unified workflow details
- `workflow-0-submitted-state.md` - Deep dive on 0-SUBMITTED
- `TESTING-QUICK-REFERENCE.md` - Testing patterns and solutions