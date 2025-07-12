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
2. **MANDATORY**: Manual testing by user
3. **MANDATORY**: Code review
4. Create four completion reports
5. Calculate time metrics and AI impact

### Phase 5: Completion (3-COMPLETED)
**⚠️ ONLY after explicit user acceptance! ⚠️**

1. User provides explicit acceptance ("tests passed", "feature accepted")
2. Update completion reports with acceptance details
3. Move folder to `3-COMPLETED`
4. Update feature tracking documents
5. Prepare for release

## Baseline Health Check (MANDATORY)

Before ANY implementation:

```bash
# All projects use dotnet CLI
dotnet test      # Must show 100% pass rate
dotnet build     # Must succeed with minimal warnings
```

Document results in feature-tasks.md:
```markdown
## Baseline Health Check Report
**Date/Time**: YYYY-MM-DD HH:MM
**Branch**: feature/branch-name

### Build Status
- **Result**: ✅ Success / ❌ Failed
- **Warnings**: X (must be < 10)

### Test Status  
- **Total Tests**: X
- **Passed**: X (MUST equal Total)
- **Failed**: 0 (MUST be zero)

### Decision
- [ ] All tests passing (REQUIRED)
- [ ] Build successful (REQUIRED)
- [ ] Warnings acceptable (< 10)

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
After EVERY task:
- ✅ Code compiles without errors
- ✅ All tests pass (no failures, no skips)
- ✅ Build warnings < 10
- ✅ Feature remains functional

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
- Technical patterns: `systemPatterns.md`
- Testing guidelines: `TESTING-QUICK-REFERENCE.md`