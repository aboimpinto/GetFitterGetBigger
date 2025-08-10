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