# Unified Development Process - GetFitterGetBigger

This document defines the core development process shared by all GetFitterGetBigger projects. Project-specific variations are documented in each project's FEATURE_IMPLEMENTATION_PROCESS.md file.

## Technology Stack
- **API**: C# Minimal API
- **Admin**: C# Blazor  
- **Clients**: C# Avalonia
- **Build System**: dotnet CLI for all projects
- **Test Framework**: xUnit
- **Component Testing**: bUnit (for Blazor)

## Core Principles

1. **Quality First**: Every feature must meet quality standards before completion
2. **Documentation as Code**: All processes must be documented and maintained
3. **Consistent Workflow**: All projects follow the same state-based workflow
4. **Measurable Progress**: Time tracking and metrics for continuous improvement

## Feature Origin and Flow

### 1. Feature Origins
Features can originate from any project based on business or technical needs:

#### API-First Flow (Most Common)
```
Business Request → API Design → API Implementation → Admin/Clients Propagation
```

#### UI-First Flow (When UI needs drive API changes)
```
Admin/Clients Need → API Feature Request → API Implementation → UI Implementation
```

### 2. Feature Types
- **Business Features**: New functionality requested by stakeholders
- **Technical Features**: Performance, security, or architectural improvements
- **Integration Features**: Changes required for project interoperability

### 3. Propagation Rules
- **API Changes**: Must be propagated to Admin and Clients projects
- **Admin → API**: Create API feature request first, then implement Admin UI
- **Clients → API**: Create API feature request first, then implement client UI
- **Shared Components**: Changes must be coordinated across all affected projects

## Mandatory File Management Rules

**⚠️ CRITICAL: Maintain focus on REQUIRED files only! ⚠️**

### Prohibited Files (NEVER CREATE):
- ❌ `README.md` in feature folders (use feature-description.md)
- ❌ `notes.txt`, `todo.txt`, or similar informal notes
- ❌ Duplicate documentation files
- ❌ Personal workspace files
- ❌ Commented-out code files

### Required Files by State:
- **0-SUBMITTED**: Only `feature-description.md`
- **1-READY_TO_DEVELOP**: Add `feature-tasks.md`
- **2-IN_PROGRESS**: Add test scripts and implementation artifacts
- **3-COMPLETED**: Add four mandatory completion reports:
  - `COMPLETION-REPORT.md`
  - `TECHNICAL-SUMMARY.md`
  - `LESSONS-LEARNED.md`
  - `QUICK-REFERENCE.md`

### Optional Allowed Files:
- ✅ API specification files (`.json`, `.yaml`)
- ✅ Test scripts (`.sh`, `.ps1`)
- ✅ Mockups/wireframes (`.png`, `.jpg`)
- ✅ Architecture diagrams

## Universal Process Steps

### Phase 1: Feature Submission (0-SUBMITTED)
**MANDATORY STARTING POINT for ALL features across ALL projects**

1. Assign Feature ID from `NEXT_FEATURE_ID.txt`
2. Create folder: `0-SUBMITTED/FEAT-XXX-feature-name/`
3. Create `feature-description.md` with business context
4. Update `NEXT_FEATURE_ID.txt`
5. NO implementation details yet

### Phase 2: Planning (1-READY_TO_DEVELOP)
1. Analyze requirements and technical approach
2. Create `feature-tasks.md` with detailed task breakdown
3. Estimate time for each task
4. Move folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`

### Phase 3: Implementation (2-IN_PROGRESS)
1. Move folder to `2-IN_PROGRESS`
2. Create feature branch
3. **MANDATORY**: Baseline health check before starting
4. Implement tasks sequentially
5. **MANDATORY**: Unit tests immediately after each implementation
6. **MANDATORY**: Continuous build/test verification
7. Update task status with commit hashes

### Phase 4: Quality Assurance
1. **MANDATORY**: All automated tests must pass (100% green)
2. **MANDATORY**: Code review for each category (APPROVED status)
3. **MANDATORY**: Final overall code review (use FINAL-CODE-REVIEW-TEMPLATE.md)
4. **MANDATORY**: Manual testing by user
5. Create four completion reports
6. Calculate time metrics and AI impact

### Phase 5: Completion (3-COMPLETED)
**⚠️ ONLY after explicit user acceptance AND final code review approval! ⚠️**

1. Final code review must be:
   - **APPROVED**: Can proceed directly
   - **APPROVED_WITH_NOTES**: Requires user approval
   - **REQUIRES_CHANGES**: Must fix and re-review
2. User provides explicit acceptance ("tests passed", "feature accepted")
3. Update completion reports with acceptance details
4. Move folder to `3-COMPLETED`
5. Update feature tracking documents
6. Prepare for release

## Baseline Health Check (MANDATORY)

Before ANY implementation:

```bash
# All projects use dotnet CLI
dotnet clean     # Clean build artifacts to ensure all warnings are visible
dotnet build     # Must succeed with minimal warnings
dotnet test      # Must show 100% pass rate
```

Document results in feature-tasks.md:
```markdown
## Baseline Health Check Report
**Date/Time**: YYYY-MM-DD HH:MM
**Branch**: feature/branch-name

### Build Status
- **Result**: ✅ Success / ❌ Failed
- **Warnings**: X (BOY SCOUT RULE: maintain baseline level or better)

### Test Status  
- **Total Tests**: X
- **Passed**: X (MUST equal Total)
- **Failed**: 0 (MUST be zero)

### Decision
- [ ] All tests passing (REQUIRED)
- [ ] Build successful (REQUIRED)
- [ ] Warnings at baseline level or better (BOY SCOUT RULE)

**Proceed**: Yes/No
```

## Task Status Tracking

### Status Progression
1. `[ReadyToDevelop]` - Initial state
2. `[InProgress: Started: YYYY-MM-DD HH:MM]` - Work begun
3. `[Implemented: <hash> | Started: <time> | Finished: <time> | Duration: Xh Ym]` - Complete
4. `[Blocked: <reason>]` - Cannot proceed
5. `[Skipped]` - Not needed

### Checkpoint Requirements
After EVERY category:
- ✅ Code compiles without errors (run `dotnet clean && dotnet build`)
- ✅ All tests pass - no failures, no skips (run `dotnet clean && dotnet test`)
- ✅ Build warnings at baseline level or better (BOY SCOUT RULE - zero warnings)
- ✅ Feature remains functional
- ✅ **MANDATORY Code Review**: All changed files must be reviewed using:
  - `/memory-bank/API-CODE_QUALITY_STANDARDS.md` for API projects
  - `/memory-bank/CODE-REVIEW-TEMPLATE.md` for review documentation
  - Status must be APPROVED or APPROVED_WITH_NOTES (and user confirms)
  
**🛑 CRITICAL**: You CANNOT proceed to the next category if checkpoint is not GREEN:
- Build must have 0 errors and 0 warnings
- All tests must be green/pass
- Code review must be APPROVED (or APPROVED_WITH_NOTES confirmed by user)
- Checkpoint status must be marked as ✅ PASSED before continuing

## Manual Testing Policy

**🚨 MANDATORY for ALL projects - NO EXCEPTIONS 🚨**

### When Manual Testing Occurs
- After ALL implementation tasks complete
- Before creating completion reports
- Before requesting code review
- Before moving to COMPLETED

### What to Test
1. All acceptance criteria from feature-description.md
2. Integration with existing features
3. Error scenarios and edge cases
4. Performance under realistic load
5. Security implications

### Skipping Manual Testing
**ONLY allowed when ALL conditions are met:**
- User explicitly requests skip AT FEATURE START
- Feature is purely technical (no user-facing changes)
- Comprehensive automated tests cover all scenarios
- User provides written acceptance of risk

## Time Tracking Standards

### Format
```
Duration: Xh Ym (e.g., "2h 30m", "0h 45m")
```

### Recording Rules
- Track actual work time only
- Exclude breaks and interruptions
- Round to nearest 5 minutes
- Note if task was interrupted

### AI Impact Calculation
```
AI Impact = ((Estimated - Actual) / Estimated) × 100%
```

## Code Review Standards

### When Code Reviews Are Required
1. **Category Completion**: After each category in feature-tasks.md
2. **Bug Fixes**: After implementation, before testing
3. **Final Review**: Before moving feature to COMPLETED

### Code Review Process

#### Category Reviews
1. Use `CODE-REVIEW-TEMPLATE.md` for each category
2. Store in `/2-IN_PROGRESS/FEAT-XXX/code-reviews/Category_X/`
3. File naming: `Code-Review-Category-X-YYYY-MM-DD-HH-MM-{STATUS}.md`
4. Review all files created/modified in that category
5. Check compliance with `CODE_QUALITY_STANDARDS.md`

#### Final Overall Review
1. Use `FINAL-CODE-REVIEW-TEMPLATE.md`
2. Store in `/2-IN_PROGRESS/FEAT-XXX/code-reviews/`
3. File naming: `Final-Code-Review-YYYY-MM-DD-HH-MM-{STATUS}.md`
4. Combine all category reviews
5. Scan entire feature against quality standards
6. Verify no technical debt accumulation

### Review Outcomes and Actions

#### APPROVED ✅
- All critical checks passed
- No blocking issues
- Can proceed to next phase

#### APPROVED_WITH_NOTES ⚠️
- Minor issues documented
- Not blocking progress
- For final review: requires user approval to proceed
- Document issues for future cleanup

#### REQUIRES_CHANGES ❌
- Critical issues found
- Must fix before proceeding
- Create new review after fixes
- Cannot move to next category/phase

### Code Review Storage Structure
```
2-IN_PROGRESS/FEAT-XXX/code-reviews/
├── Category_1/
│   ├── Code-Review-Category-1-2025-01-16-09-00-REQUIRES_CHANGES.md
│   └── Code-Review-Category-1-2025-01-16-10-00-APPROVED.md
├── Category_2/
│   └── Code-Review-Category-2-2025-01-16-14-00-APPROVED.md
├── Category_3/
│   └── Code-Review-Category-3-2025-01-16-16-00-APPROVED_WITH_NOTES.md
└── Final-Code-Review-2025-01-16-18-00-APPROVED.md
```

### Review Focus Areas
1. **Architecture Compliance**: Layer separation, DDD principles
2. **Pattern Consistency**: Empty pattern, ServiceResult pattern
3. **Code Quality**: No nulls, pattern matching, method length
4. **Testing Standards**: Proper separation, no magic strings
5. **Performance & Security**: Caching, validation, no SQL injection
6. **Documentation**: XML comments, clear naming

## Cross-Project Coordination

### Coordination Points
1. **API → UI Projects**: When endpoints change
2. **UI → API**: When new data needed
3. **Shared Components**: When updating common libraries
4. **Breaking Changes**: Requires team synchronization

### Communication Requirements
- Document breaking changes in feature-description.md
- Note cross-project dependencies
- Create linked features in affected projects
- Coordinate release timing

## Quality Standards

### Code Quality
- **Build Warnings**: Maximum 10 new warnings
- **Test Coverage**: Maintain or improve
- **Code Review**: Required before completion
- **Documentation**: All public APIs documented

### Process Quality
- **Task Granularity**: 4 hours maximum per task
- **Commit Frequency**: At least once per task
- **Status Updates**: Real-time in task file
- **Time Tracking**: Required for metrics

## Common Pitfalls to Avoid

1. **Starting without 0-SUBMITTED state** - Breaks workflow tracking
2. **Skipping baseline health check** - Inherits technical debt
3. **Deferring tests** - Creates quality risks
4. **Creating unnecessary files** - Clutters project
5. **Moving to COMPLETED without user acceptance** - Violates process
6. **Forgetting completion reports** - Loses knowledge
7. **Not tracking time** - Can't measure improvement

## Maintenance

This unified process should be:
- Reviewed quarterly
- Updated based on retrospectives
- Version controlled
- Synchronized across projects

## References

- Project-specific implementations: `FEATURE_IMPLEMENTATION_PROCESS.md`
- Workflow details: `FEATURE_WORKFLOW_PROCESS.md`
- Technical patterns: `/memory-bank/Overview/SystemPatterns.md`
- Testing guidelines: `PracticalGuides/TestingQuickReference.md`