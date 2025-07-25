Continue implementing the current feature with intelligent context awareness and task-specific guidelines.

## Intelligent Continuation Process

1. **Health Check** (MANDATORY FIRST STEP):
   - Run `dotnet clean && dotnet build`
   - Run `dotnet test`
   - **If all passes**: Continue silently without reporting
   - **If issues found**:
     - List all errors/warnings clearly
     - Ask user: "Health check failed. Do you want to fix these issues before continuing? (yes/no)"
     - **If YES**: Create a Boy Scout task at the beginning of current phase to fix issues
     - **If NO**: Document in feature-tasks.md that user was prompted and chose to continue

2. **Context Discovery**:
   - Identify current feature in `/memory-bank/features/2-IN_PROGRESS/`
   - Read feature-tasks.md to find current position
   - Check git status for uncommitted changes
   - Review previous checkpoint feedback if any

3. **Task Analysis**:
   - Find next uncompleted task (status: [ReadyToDevelop])
   - Detect task type using patterns from implementation-guidelines-map.md
   - Load relevant guidelines for the specific task type
   - Check if approaching a checkpoint

4. **Context-Specific Setup**:
   - For each task type, automatically reference relevant documentation
   - Provide examples from well-tested existing code
   - Highlight specific requirements for that task type

## Dynamic Guidelines Loading

The command will analyze the current task and provide:

### 📁 API Service Layer Tasks
**When task contains**: "Service", "API integration", "API calls"
**Auto-load**:
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#api-service-testing-xunit
- Service patterns from ExerciseService (81% coverage)
- HTTP mocking setup examples
- Error handling patterns

### 🧩 Component Tasks
**When task contains**: "component", "View", "Form", "Panel"
**Auto-load**:
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#blazor-component-testing-bunit
- Component testability checklist
- data-testid attribute requirements
- Examples from ExerciseWeightTypeBadge (100% coverage)

### 📊 State Management Tasks
**When task contains**: "StateService", "state management"
**Auto-load**:
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#state-service-testing
- Optimistic update patterns
- Error persistence examples
- ExerciseWeightTypeStateService patterns

### 🧪 Testing Tasks
**When task contains**: "test", "unit test", "integration test"
**Auto-load**:
- Specific testing guide section based on test type
- Coverage goals (80%+ for new code)
- Test patterns from high-coverage components
- Boy Scout Rule reminders

### 🛑 Checkpoint Tasks
**When line starts with**: "## CHECKPOINT" or "CHECKPOINT:"
**Auto-load**:
- @memory-bank/FEATURE_CHECKPOINT_TEMPLATE.md
- @memory-bank/CODE_REVIEW_PROCESS.md
- Build verification commands
- Code review folder structure

### 🎨 UI/UX Tasks
**When task contains**: "loading", "responsive", "accessibility"
**Auto-load**:
- UI patterns from completed features
- Tailwind CSS conventions
- Accessibility checklist
- Responsive design guidelines

## Execution Flow

1. **Initial Health Check** (MANDATORY):
   ```bash
   dotnet clean && dotnet build  # Check for errors/warnings
   dotnet test                   # All tests must pass
   ```
   - If passes: Continue without reporting
   - If fails: Prompt user for fix decision
   - Document decision in feature-tasks.md if user chooses to continue

2. **Task Implementation**:
   - Update status to [InProgress: Started: YYYY-MM-DD HH:MM]
   - Follow loaded guidelines specific to task type
   - Implement with continuous build/test verification

3. **Task Completion**:
   - Run final build/test verification
   - Commit with descriptive message
   - Update task with [Implemented: <hash> | Started/Finished times]
   - Check if next item is a checkpoint

4. **Checkpoint Handling**:
   - STOP implementation
   - Create code review in correct folder structure
   - Update checkpoint status
   - Wait for user approval before continuing

## Smart Features

### Pattern Recognition:
- Automatically detects when you're implementing vs testing
- Knows when to stop for checkpoints
- Identifies when approaching phase boundaries

### Contextual Help:
- Shows only relevant guidelines for current task
- Provides specific code examples
- Highlights common pitfalls for that task type

### Progress Tracking:
- Shows completed vs remaining tasks
- Calculates time spent vs estimates
- Identifies if falling behind schedule

## Special Scenarios

### No In-Progress Feature:
- Check 1-READY_TO_DEVELOP folder
- Suggest using /start-implementing
- Show available features to start

### Blocked Task:
- Mark as [BLOCKED: reason]
- Skip to next available task
- Create issue documentation

### Missing Guidelines:
- Flag with ⚠️ warning
- Use general best practices
- Request pattern documentation

## Commands Reference

Always check:
- @.claude/commands/implementation-guidelines-map.md for task mappings
- @memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md for overall flow
- @memory-bank/CODE_QUALITY_STANDARDS.md for universal standards

Stop at every checkpoint for mandatory review before proceeding to next phase.

## Health Check Documentation Format

When health check fails and user chooses to continue, add to feature-tasks.md:

```markdown
### Health Check Warning - [Date/Time]
**Build/Test Status**: Failed
**Issues Found**:
- [List all errors/warnings]

**User Decision**: Chose to continue without fixing
**Reason**: [If provided by user]
```

## Boy Scout Task Format

When user chooses to fix issues first, add before current phase tasks:

```markdown
- **Boy Scout Task:** Fix health check issues [ReadyToDevelop] (Est: 30m)
  - Fix build errors: [list]
  - Fix build warnings: [list]  
  - Fix failing tests: [list]
```

## Task Tracking Format

### Task Status Format:
```markdown
# Before starting:
- **Task X.Y:** Description [ReadyToDevelop] (Est: 2h)

# When starting:
- **Task X.Y:** Description [InProgress: Started: 2025-07-24 10:30] (Est: 2h)

# When completed:
- **Task X.Y:** Description [Completed: Started: 2025-07-24 10:30, Finished: 2025-07-24 11:45] (Est: 2h, Actual: 1h 15m)
  - Git commit: `abc123f` - feat: implement feature X
```

### Checkpoint Update Format:
After completing all tasks in a phase, update checkpoint with:
- Build status (must run `dotnet clean && dotnet build`)
- Test status (must run `dotnet test`)
- List all git commits for the phase
- Time tracking summary
- Set status to PENDING until reviewed
- Run `/review-implemented-feature` to verify and approve