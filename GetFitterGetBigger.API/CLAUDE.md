# Claude AI Assistant Guidelines

This document contains project-specific instructions for Claude AI when working on the GetFitterGetBigger project.

## ⚠️ CRITICAL: Communication and Decision Making

### 1. Always Discuss Alternative Solutions
**After discussing a solution, if you find there is another way to solve it:**
- **STOP and request attention from the user**
- Present your new solution with clear pros and cons
- Compare it with the original approach
- Wait for user confirmation before proceeding
- Never silently change approaches mid-implementation

### 2. DO NOT MAKE ASSUMPTIONS - ASK!
**Better to ask than develop in one direction and need to rollback the work:**
- If you have ANY doubts about implementation details - **ASK**
- If you're unsure about architectural decisions - **ASK**
- If multiple approaches seem valid - **ASK** which one to use
- If requirements are ambiguous - **ASK** for clarification
- Never assume what the user wants - always confirm

**Examples of when to ASK:**
- "Should empty entities return Success or Failure?"
- "Which pattern should I use for this validation?"
- "Should this be async or sync?"
- "Is this the correct ID format?"

## 🔴 CRITICAL: Code Review Integrity and Status Changes

### NEVER Change Feature Status Without PROPERLY EXECUTED Review
**This is a MANDATORY quality control requirement - NO EXCEPTIONS!**

### 🚨 Code Reviews MUST Be Actually Executed - NOT Fabricated!

**ABSOLUTELY FORBIDDEN Actions:**
1. **❌ NEVER** create a "fake" APPROVED review without running actual validation
2. **❌ NEVER** write "APPROVED" without analyzing the code
3. **❌ NEVER** generate review reports without using proper review tools/agents
4. **❌ NEVER** skip review execution and just create a file saying "looks good"
5. **❌ NEVER** manually write review files - ALWAYS use review agents/tools

**Every Code Review MUST:**
- 🔍 **Actually analyze** all relevant files against CODE_QUALITY_STANDARDS.md
- 📊 **Produce real metrics**: files reviewed, violations found, compliance %
- 📍 **Show specific issues**: file paths, line numbers, violation descriptions
- ⏰ **Include execution timestamp**: when the review was actually run
- 📝 **List analyzed files**: prove which files were actually reviewed

**How AI Must Execute Reviews:**
1. **USE** the feature-code-reviewer agent or appropriate review tool
2. **WAIT** for the tool to complete its analysis
3. **READ** the generated report to verify it contains real analysis
4. **VERIFY** the report has specific metrics and findings
5. **ONLY THEN** use the review status for decisions

**Verification Checklist - Is This a Real Review?**
- ✅ Contains specific file paths that were analyzed
- ✅ Shows violation counts and types (even if 0)
- ✅ Includes compliance percentage calculations
- ✅ Has concrete examples when violations found
- ✅ Timestamp matches current session
- ❌ Generic statements like "code looks good"
- ❌ Missing specific metrics
- ❌ No file list or analysis details

**When a code review has status REQUIRES_CHANGES or REJECTED:**
1. **❌ FORBIDDEN**: Marking feature/checkpoint as COMPLETED/PASSED without new APPROVED review
2. **❌ FORBIDDEN**: "Partially" fixing issues and changing status
3. **❌ FORBIDDEN**: Creating fake "APPROVED" review after fixes
4. **❌ FORBIDDEN**: Any status change without ACTUAL review execution

**MANDATORY Process After Failed Review:**
1. **FIX** all identified issues completely
2. **EXECUTE** a NEW code review using proper tools (not manually create!)
3. **VERIFY** review was actually run (check for real metrics/analysis)
4. **CONFIRM** review status is APPROVED based on actual validation
5. **ONLY THEN** update feature/checkpoint status

**AI Agents and Commands MUST:**
- **ALWAYS** use review agents/tools for code reviews
- **NEVER** fabricate review results or create fake reports
- **NEVER** change status without executing actual review
- **ALWAYS** provide evidence that review was properly executed
- **ALWAYS** include real metrics and findings in reviews

**Remember:** Code reviews are ACTUAL VALIDATIONS, not paperwork. An APPROVED review means the code was ACTUALLY ANALYZED and found compliant, not that someone just wrote "APPROVED" in a file.

## Role in API Project

When working in the API project folder, the AI assistant can perform **full implementation work** including:
- Creating and modifying code
- Writing tests
- Implementing features
- Following the FEATURE_IMPLEMENTATION_PROCESS.md

Note: When in the main repository folder, only documentation work is performed.

## Claude Code Agent and Command Documentation

### Creating Agents
**IMPORTANT**: Before creating any new agent, ALWAYS read the official documentation first:
- **Official Agent Documentation**: https://docs.anthropic.com/en/docs/claude-code/sub-agents

This documentation contains:
- How to create and configure sub-agents
- Best practices for agent design
- Available agent capabilities and limitations
- Examples of effective agent implementations

### Creating Slash Commands
**IMPORTANT**: Before creating any new slash command, ALWAYS read the official documentation first:
- **Official Slash Commands Documentation**: https://docs.anthropic.com/en/docs/claude-code/slash-commands#custom-slash-commands

This documentation covers:
- How to define custom slash commands
- Command syntax and parameters
- Integration with Claude Code workflow
- Best practices for command implementation

### Available Custom Agents and Commands

#### feature-code-reviewer Agent
**Purpose**: Performs comprehensive code review against CODE_QUALITY_STANDARDS.md
- Reviews ALL commits in a feature
- Checks 28 Golden Rules and all patterns
- Generates detailed violation reports with solutions
- Calculates approval rates per file and overall
- **IMPORTANT**: Code review reports must be saved in the appropriate phase folder:
  - Location: `memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/Phase_X_[PhaseName]/`
  - Naming: `Code-Review-Phase-X-[PhaseName]-YYYY-MM-DD-HH-mm-[STATUS].md`
  - Status: `APPROVED`, `REQUIRES_CHANGES`, or `REJECTED`
  - The checkpoint in the current phase's feature-tasks.md must be updated with the review results

#### feature-commit-push Agent
**Purpose**: Commits, pushes, and updates feature checkpoint with commit hash
- Analyzes changes and creates meaningful commit messages
- Pushes to remote repository
- Finds active checkpoint in current feature
- Updates checkpoint with commit hash and summary
- Handles user-provided highlights for commit message

#### /review-feature Command
**Usage**: `/review-feature FEAT-XXX`
- Triggers feature-code-reviewer agent for specified feature
- Generates timestamped report in appropriate phase folder
- Updates feature-tasks.md with review results at current phase checkpoint
- Can be run anytime during feature development
- Report location: `code-reviews/Phase_X_[PhaseName]/`
- Checkpoint update: Adds review status and file path to current phase checkpoint

#### /commit-feature Command
**Usage**: `/commit-feature [feature] [checkpoint] [highlights]`
- Triggers feature-commit-push agent
- Commits and pushes current changes
- Updates feature checkpoint with commit hash
- Auto-detects feature and checkpoint if not provided
- Examples:
  - `/commit-feature` - Auto-detect everything
  - `/commit-feature FEAT-030` - Specify feature only
  - `/commit-feature FEAT-030 "Phase 3" "fixed validation"` - Full specification

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