Continue implementing the current feature with intelligent context awareness and task-specific guidelines.

## Intelligent Continuation Process

1. **Context Discovery**:
   - Identify current feature in `/memory-bank/features/2-IN_PROGRESS/`
   - Read feature-tasks.md to find current position
   - Check git status for uncommitted changes
   - Review previous checkpoint feedback if any

2. **Task Analysis**:
   - Find next uncompleted task (status: [ReadyToDevelop])
   - Detect task type using patterns from implementation-guidelines-map.md
   - Load relevant guidelines for the specific task type
   - Check if approaching a checkpoint

3. **Context-Specific Setup**:
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

1. **Pre-Task Checks**:
   ```bash
   dotnet build  # Must pass
   dotnet test   # Must be green
   ```

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