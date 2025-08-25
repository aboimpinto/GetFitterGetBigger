# Claude AI Assistant Guidelines

This document contains project-specific instructions for Claude AI when working on the GetFitterGetBigger project.

## Role in API Project

When working in the API project folder, the AI assistant can perform **full implementation work** including:
- Creating and modifying code
- Writing tests
- Implementing features
- Following the FEATURE_IMPLEMENTATION_PROCESS.md

Note: When in the main repository folder, only documentation work is performed.

## Git Commit Messages

When creating git commits, use the following signature format at the end of the commit message:

```
🤖 Generated with [Claude Code](https://claude.ai/code)

Authored-By: Paulo Aboim Pinto <aboimpinto@gmail.com>
```

This signature properly attributes the work as authored by Paulo Aboim Pinto while acknowledging that Claude was used as the tool to generate the content.

## Project Structure

The GetFitterGetBigger ecosystem consists of:
- **API Project**: Backend service providing all endpoints
- **Admin Project**: Web application for Personal Trainers
- **Clients Project**: Contains mobile (Android, iOS), web, and desktop applications

## Documentation Propagation

When propagating API documentation:
1. Update only these two memory-banks:
   - `/GetFitterGetBigger.Admin/memory-bank/`
   - `/GetFitterGetBigger.Clients/memory-bank/`
2. Do NOT update individual sub-project memory-banks
3. Follow the rules defined in `/api-docs/documentation-propagation-rules.md`

## API Configuration

- Development URL: `http://localhost:5214/`
- Swagger Documentation: `http://localhost:5214/swagger`
- Production URL: Not yet assigned

## Authorization Claims

- Admin access: `PT-Tier` or `Admin-Tier`
- Client access: `Free-Tier`, `WorkoutPlan-Tier` (future), `DietPlan-Tier` (future)

## Testing Guidelines

**IMPORTANT**: When debugging test failures, ALWAYS check `/memory-bank/PracticalGuides/TestingQuickReference.md` first! It contains critical patterns learned from fixing 87 test failures, including:
- Common ID format errors
- Missing mock setups
- Navigation property loading issues
- Quick debugging checklist

### Understanding and Reducing CRAP Score

**CRAP Score** (Change Risk Anti-Patterns) measures code maintainability by combining:
- **Cyclomatic Complexity**: How complex the code logic is
- **Code Coverage**: How well tested the code is

#### Strategy for Reducing CRAP Score:

**1. When CRAP Score is High (>30):**
   - **First Priority**: Add test coverage
   - Adding tests is the fastest way to reduce CRAP score
   - Example: WorkoutTemplateExerciseService went from high CRAP score to acceptable just by increasing coverage from 1% to 93.9%

**2. When Cyclomatic Complexity is Very High (>10):**
   - **First Priority**: Refactor to reduce complexity
   - Extract methods, use pattern matching, remove nested ifs
   - **Then**: Add comprehensive test coverage

**3. Optimal Approach:**
   ```
   CRAP Score = Complexity² × (1 - Coverage)³ + Complexity
   
   If Complexity > 10: Refactor first, then test
   If Complexity ≤ 10: Test first (biggest impact)
   ```

**Key Insight**: Test coverage has exponential impact on CRAP score. A method with complexity 5 and 0% coverage has CRAP score of 30, but with 80% coverage drops to 6!

### Test Coverage Reports

To generate a comprehensive test coverage report for the project, use the **test-coverage-reporter** agent instead of running commands manually:

```
Use the test-coverage-reporter agent to generate a test coverage report
```

This agent will:
- Clean previous test results
- Run all tests with coverage collection
- Generate HTML and Markdown reports
- Provide actionable coverage insights

**Manual Command Reference** (if needed):
```bash
# Clean and run tests with coverage
dotnet clean
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report (requires ReportGenerator tool)
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html
```

The coverage report will be available at `TestResults/CoverageReport/index.html`

## Memory Bank Structure

The `/memory-bank/` directory contains all project knowledge and processes:

### Naming Conventions
- **Folders**: Use CamelCase (e.g., `CodeQualityGuidelines/`, `ServicePatterns/`)
- **Files**: Use appropriate naming for the content type:
  - Process docs: SCREAMING_SNAKE_CASE (e.g., `FEATURE_IMPLEMENTATION_PROCESS.md`)
  - Guidelines: CamelCase (e.g., `ServiceValidatePattern.md`)
  - Context files: camelCase (e.g., `productContext.md`)

### Temporary Files Policy
**IMPORTANT**: ALL temporary files, analysis documents, migration plans, or helper files MUST be created in `/memory-bank/temp/` folder:
- This folder is gitignored and won't be committed
- Use for: analysis files, migration plans, summaries, work-in-progress documents
- Examples: `DOCUMENT_ANALYSIS.md`, `MIGRATION_PLAN.md`, `TODO_LIST.md`
- Never create temporary files in the main memory-bank folder

### Process Documentation
- **`BUG_IMPLEMENTATION_PROCESS.md`** - How to fix bugs systematically
- **`BUG_WORKFLOW_PROCESS.md`** - Bug lifecycle and folder structure
- **`FEATURE_IMPLEMENTATION_PROCESS.md`** - How to implement new features
- **`FEATURE_WORKFLOW_PROCESS.md`** - Feature lifecycle and states
- **`RELEASE_PROCESS.md`** - Release management and PI planning

### Practical Guides
**Located in `/memory-bank/PracticalGuides/`** - CHECK FIRST when debugging!
- **`TestingQuickReference.md`** ⚡ - Common test failures and solutions (CHECK FIRST!)
- **`CommonTestingErrorsAndSolutions.md`** - Detailed testing patterns
- **`CommonImplementationPitfalls.md`** ⚠️ - Critical implementation mistakes to avoid
- **`ServiceImplementationChecklist.md`** 📋 - Quick checklist for EVERY service implementation

**CRITICAL REMINDERS**:
1. Always check `PracticalGuides/CommonImplementationPitfalls.md` before implementing service methods
2. Use `PracticalGuides/ServiceImplementationChecklist.md` as a quick reference while coding
3. Remember: ReadOnlyUnitOfWork for queries, WritableUnitOfWork for modifications ONLY!

### Bug Management (`/bugs/`)
- **`1-OPEN/`** - New bugs awaiting work
- **`2-IN_PROGRESS/`** - Bugs being actively fixed
- **`3-FIXED/`** - Completed bug fixes
- **`4-BLOCKED/`** - Bugs waiting on dependencies
- **`5-WONT_FIX/`** - Bugs that won't be addressed

**IMPORTANT**: Always use the `/memory-bank/bugs/` folder structure for bug tracking. Do NOT use or create a `BUGS.md` file in the memory-bank root. Each bug should have its own folder with proper documentation following the BUG_WORKFLOW_PROCESS.md.

### Feature Management (`/features/`)
- **`1-READY_TO_DEVELOP/`** - Features ready to implement
- **`2-IN_PROGRESS/`** - Features being developed
- **`3-COMPLETED/`** - Finished features
- **`4-BLOCKED/`** - Features waiting on dependencies
- **`5-SKIPPED/`** - Features postponed

### System Overview (`/Overview/`)
Architecture and system documentation organized in one place:
- **`SystemPatterns.md`** - Architecture patterns and rules
- **`DatabaseModelPattern.md`** - Database design patterns  
- **`ThreeTierEntityArchitecture.md`** - Entity classification system (Pure/Enhanced/Domain)
- **`ReferenceTablesOverview.md`** - Complete reference table documentation
- **`CacheConfiguration.md`** - Caching system configuration
- **`CacheInvalidationStrategy.md`** - Cache invalidation patterns
- **`UnitVsIntegrationTests.md`** - Testing architecture and separation strategy
- **`TestingGuidelines.md`** - Overall testing philosophy and branch coverage
- **`TestBuilderPattern.md`** - Architectural pattern for test data creation

### Context Files
- **`productContext.md`** - Product vision and goals
- **`techContext.md`** - Technical stack and decisions
- **`projectbrief.md`** - Project overview
- **`activeContext.md`** - Current work context

**💡 Pro Tip**: When debugging, start with PracticalGuides/, then check relevant process docs!

## ⚠️ CRITICAL: Validation Pattern Rules

### 1. NO Double Negations in Validation
**NEVER use `!(await something)` in validation predicates!** This is confusing and error-prone.

```csharp
// ❌ BAD - Double negation
.EnsureNameIsUniqueAsync(
    async () => !(await _dataService.ExistsByNameAsync(name)).Data.Value,
    "Entity", name)

// ✅ GOOD - Positive assertion with helper
.EnsureNameIsUniqueAsync(
    async () => await IsNameUniqueAsync(name),
    "Entity", name)

private async Task<bool> IsNameUniqueAsync(string name)
{
    var exists = await _dataService.ExistsByNameAsync(name);
    return !exists.Data.Value; // Returns true when unique
}
```

### 2. Validation Methods Are QUESTIONS, Not COMMANDS
**Validation methods should ask questions (IsValid), not give commands (Validate)!**

```csharp
// ❌ BAD - Command-like names
ValidateExerciseTypesAsync()
ValidateKineticChainAsync()
CheckDuplicateNameAsync()

// ✅ GOOD - Question format
AreExerciseTypesValidAsync()
IsKineticChainValidAsync()
IsNameUniqueAsync()
```

### 3. NO Magic Strings
**ALL error messages and string literals must be constants!**

```csharp
// ❌ BAD - Hardcoded string
"REST exercises cannot have kinetic chain"

// ✅ GOOD - Constant
ExerciseErrorMessages.InvalidKineticChainForExerciseType
```

## ⚠️ CRITICAL: No Try-Catch Anti-Pattern

**NEVER use blanket try-catch blocks around entire methods!** This is an anti-pattern that shows lack of control over the code.

### Why Try-Catch is an Anti-Pattern:
- **Hides Real Issues**: Blanket try-catch masks where failures actually occur
- **Poor Readability**: Makes code flow unpredictable and hard to follow
- **Lack of Control**: Shows we don't understand when/where our code can fail
- **Test Coverage**: Makes it harder to test specific failure scenarios

### The Right Approach:
- **Know Your Failure Points**: Understand exactly where and why code can fail
- **Surgical Try-Catch**: Use try-catch ONLY when you know specific operations can fail
- **Let Framework Handle**: ServiceValidate and other patterns handle errors properly
- **Test Coverage**: Write tests for known failure scenarios

### Example:
```csharp
// ❌ BAD - Blanket try-catch anti-pattern
private async Task<ServiceResult<UserDto>> LoadUserByEmailAsync(string email)
{
    try
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IUserRepository>();
        var entity = await repository.GetByEmailAsync(email);
        var dto = entity?.ToDto() ?? UserDto.Empty;
        return ServiceResult<UserDto>.Success(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading user");
        return ServiceResult<UserDto>.Failure(UserDto.Empty, ServiceError.InternalError());
    }
}

// ✅ GOOD - No try-catch, let validation patterns handle it
private async Task<ServiceResult<UserDto>> LoadUserByEmailAsync(string email)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IUserRepository>();
    var entity = await repository.GetByEmailAsync(email);
    
    // Handle null/empty states explicitly
    var dto = entity?.ToDto() ?? UserDto.Empty;
    return ServiceResult<UserDto>.Success(dto);
}
```

## ⚠️ CRITICAL: No Bulk Scripts Policy

**NEVER use scripts for bulk file modifications!** This lesson was learned the hard way when a script-based refactoring caused 6394 build errors.

### What NOT to do:
- ❌ Python scripts for bulk file modifications
- ❌ Bash scripts with sed/awk for multi-file changes
- ❌ Any automated find-and-replace across multiple files
- ❌ Scripts that attempt to "understand" code patterns

### What to do instead:
1. ✅ List all files that need to be changed
2. ✅ Change files **one-by-one manually** using Read/Edit tools
3. ✅ Verify each file builds correctly before moving to the next
4. ✅ Track progress using TodoWrite tool
5. ✅ Accept that it takes longer - this is the correct trade-off

### Why manual is better:
- **Context Awareness**: Each file might have subtle differences that scripts miss
- **Immediate Validation**: Can check each change is correct before proceeding
- **Safety**: Prevents cascading errors across the entire codebase
- **Learning**: Manual changes help understand the codebase better

### Remember:
> "It's better to spend 2 hours changing 20 files manually than to spend 6 hours fixing 6000+ errors from a script gone wrong."

When propagating refactoring patterns (like from BodyPartService to other reference services), ALWAYS do it file-by-file manually. The time "saved" by scripts is an illusion when dealing with code modifications.