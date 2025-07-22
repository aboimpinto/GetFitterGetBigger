# Refine Feature Command

This command analyzes a feature description and breaks it down into detailed implementation tasks following GetFitterGetBigger's established patterns and best practices.

## Usage

`/refine-feature` - When run in a feature folder containing a feature-description.md file

## Process Overview

The command will:
1. Analyze the feature description
2. **Study the codebase** to identify reusable patterns and similar implementations
3. Review memory-bank for relevant patterns, pitfalls, and lessons learned
4. Generate a comprehensive `feature-tasks.md` with proper task ordering
5. Include references to existing code examples
6. Incorporate lessons learned from completed features

## Key Requirements

### 1. Codebase Study Task (MANDATORY)
Every feature refinement MUST include an initial task to study the codebase:
```
Task X.X: Study codebase for similar implementations
- Search for similar entities/services/controllers
- Identify patterns to follow
- Note code that can be reused or adapted
- Document findings with specific file references
```

### 2. Task Ordering Validation
- Database migrations MUST come early (to keep integration tests green)
- Follow logical dependency order: Models → Database → Repository → Service → Controller → Tests
- Each category requires a checkpoint before proceeding

### 3. Reference Key Documents
The generated tasks should reference (not copy):
- `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md` - For task structure template
- `/memory-bank/common-implementation-pitfalls.md` - Before service implementation
- `/memory-bank/service-implementation-checklist.md` - Quick reference during coding
- `/memory-bank/SERVICE-RESULT-PATTERN.md` - For error handling
- `/memory-bank/TESTING-QUICK-REFERENCE.md` - For test implementation

### 4. Include Lessons Learned
Review and incorporate lessons from:
- `/memory-bank/features/3-COMPLETED/*/LESSONS-LEARNED.md`
- Relevant bug fixes in `/memory-bank/bugs/3-FIXED/*/`
- Recent code reviews in completed features

### 5. Task Detail Requirements
Each task should include:
- Clear description of what to implement
- Time estimate based on complexity
- **Specific code examples** from the codebase (with file paths)
- References to relevant patterns or guidelines
- Any critical warnings (e.g., "Use ReadOnlyUnitOfWork for validation")

## Implementation Steps

1. **Read and analyze** the feature-description.md
2. **Search the codebase** for similar implementations
3. **Review memory-bank** for:
   - Completed features with similar patterns
   - Relevant lessons learned
   - Common pitfalls to avoid
4. **Generate feature-tasks.md** following the structure in:
   - `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md` (Section 2)
5. **Validate task order** to ensure:
   - Database changes come early
   - Dependencies are respected
   - Tests can stay green throughout

## Output Format

The command generates a `feature-tasks.md` file with:
- Pre-implementation checklist (referencing key documents)
- BDD scenarios defined upfront
- Tasks organized by category with checkpoints
- Each task includes:
  - Description and time estimate
  - References to similar code in the codebase
  - Critical patterns to follow
  - Warnings about common pitfalls
- BOY SCOUT RULE section for improvements found during implementation

## Quality Checks

The generated task list must:
- ✅ Include initial codebase study task
- ✅ Reference (not duplicate) guidelines
- ✅ Include specific code examples with file paths
- ✅ Incorporate relevant lessons learned
- ✅ Follow proper task ordering for green tests
- ✅ Include checkpoints between categories
- ✅ Reference critical patterns (ServiceResult, UnitOfWork, etc.)

## Example Task Format

```markdown
### Task 3.2: Create IWorkoutTemplateService interface
`[Pending]` (Est: 0.5h)
- All methods must return ServiceResult<T> (see SERVICE-RESULT-PATTERN.md)
- Follow pattern from: `Services/Interfaces/IExerciseService.cs`
- Include methods for: GetAll, GetById, Create, Update, Delete
- **WARNING**: No exceptions for control flow!
```

## Success Criteria

The refined feature tasks are ready when:
- Every task has clear implementation guidance
- Critical patterns and pitfalls are highlighted
- Existing code examples are referenced
- Task order ensures tests stay green
- Lessons from similar features are incorporated