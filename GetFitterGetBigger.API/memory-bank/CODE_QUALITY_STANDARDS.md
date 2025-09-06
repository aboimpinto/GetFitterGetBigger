# API Code Quality Standards - Overview

**🎯 PURPOSE**: Central index for API-specific code quality standards in the GetFitterGetBigger API project. This overview links to detailed guidelines organized by topic.

## 🚨 GOLDEN RULES - NON-NEGOTIABLE

```
┌─────────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: These API rules MUST be followed - NO EXCEPTIONS  │
├─────────────────────────────────────────────────────────────────┤
│ 1. Single Exit Point per method AND inside MatchAsync         │
│ 2. ServiceResult<T> for ALL service methods                    │
│ 3. No null returns - USE EMPTY PATTERN                         │
│ 4. ReadOnlyUnitOfWork for queries, WritableUnitOfWork for mods │
│ 5. Pattern matching in controllers for ServiceResult handling  │
│ 6. No try-catch for business logic - ANTI-PATTERN             │
│ 7. No bulk scripts for refactoring - MANUAL ONLY              │
│ 8. POSITIVE validation assertions - NO double negations        │
│ 9. Validation methods are QUESTIONS (IsValid) not COMMANDS    │
│ 10. NO magic strings - ALL messages in constants              │
│ 11. Chain ALL validations in ServiceValidate, not MatchAsync  │
│ 12. ALL repositories MUST inherit from base classes           │
│ 13. TEST INDEPENDENCE - NO shared mocks at class level         │
│ 14. Use PRODUCTION error constants in tests - NO recreating    │
│ 15. Test Builder Pattern MANDATORY for ALL DTOs and entities   │
│ 16. Mock setups ONLY via fluent extension methods             │
│ 17. Focus Principle: Set ONLY properties under test           │
│ 18. NO ServiceError.ValidationFailed wrapper in Ensure methods │
│ 19. Replace ALL symbolic expressions with semantic extensions  │
│ 20. Parse IDs ONCE, pass specialized types consistently       │
│ 21. Load entities ONCE per request - use Dual-Entity Pattern  │
│ 22. NEVER return entities from DataServices - DTOs ONLY       │
│ 23. Entity manipulation ONLY inside DataServices              │
│ 24. Update entities via Func<T,T> transformation functions    │
│ 25. VALIDATE ONCE, TRUST FOREVER - No redundant checks        │
│ 26. Each layer validates its responsibility, then TRUSTS      │
│ 27. NEVER test logging - it's an implementation detail        │
│ 28. ALL private fields use _ prefix, access with this.        │
│ 29. Primary constructors for ALL DI services - NO exceptions  │
└─────────────────────────────────────────────────────────────────┘
```

## 📚 Detailed Guidelines by Topic

### Core Patterns

#### [ServiceResult Pattern](./CodeQualityGuidelines/ServiceResultPattern.md)
- **MANDATORY** return type for all service methods
- Structured error handling with ServiceErrorCode
- Integration with Empty pattern
- Controller mapping patterns

#### [ServiceValidate Pattern](./CodeQualityGuidelines/ServiceValidatePattern.md)
- **CRITICAL**: Use `Build<T>()` if ANY validation is async, `For<T>()` only for all-sync
- Fluent validation API replacing manual validation
- Single exit points
- Mixed sync/async chains
- Integration with ServiceResult

#### [Validation Extensions Catalog](./CodeQualityGuidelines/ValidationExtensionsCatalog.md)
- **COMPLETE REFERENCE** All validation extensions
- EnsureNotEmpty, EnsureMaxLength, EnsureNameIsUniqueAsync
- EnsureHasValidAsync, EnsureNotWhiteSpace patterns
- **NEW**: ThenEnsure conditional validation pattern
- Quick reference table and examples

#### [Service Validation Extension Patterns](./CodeQualityGuidelines/ServiceValidationExtensionPatterns.md)
- **CRITICAL** No ServiceError.ValidationFailed wrapper for validation errors
- **MANDATORY** Replace ALL symbolic expressions with semantic extensions
- Parse IDs ONCE at method entry, use consistently
- Commands must contain specialized IDs, not strings
- Natural language methods over symbols (!=, >=, etc.)

#### [Dual-Entity Validation Pattern](./CodeQualityGuidelines/DualEntityValidationPattern.md)
- **OPTIMIZE** Load entities ONCE, validate many times
- Eliminate redundant database calls (67% reduction)
- Semantic extension methods for relationship validation
- Carry loaded entities through validation chain
- Blueprint for ExerciseLinks, Prerequisites, Dependencies

#### [Clean Validation Pattern](./CodeQualityGuidelines/CleanValidationPattern.md)
- **NEW** Positive assertions and trust boundaries
- Simplified validation with smart overloads
- Minimal defensive programming
- Helper methods with positive naming

#### [Trust the Validation Chain](./CodeQualityGuidelines/TrustTheValidationChain.md)
- **CRITICAL** Validate once, trust forever principle
- Validation responsibility matrix by layer
- How to identify redundant validation
- Performance benefits of trusting the chain

#### [Positive Validation Pattern](./CodeQualityGuidelines/PositiveValidationPattern.md)
- **CRITICAL** Use positive assertions (IS, HAS, CAN)
- No double negations in validations
- EnsureNameIsUniqueAsync, EnsureHasValidAsync patterns
- Helper methods return true for positive state

#### [No Null Command Pattern](./CodeQualityGuidelines/NoNullCommandPattern.md)
- **CRITICAL** Commands passed to services are NEVER null
- Controllers always create valid commands via ToCommand()
- No null checks on command parameters - EVER
- Trust the architecture, validate business rules only

#### [Validation Anti-Patterns](./CodeQualityGuidelines/ValidationAntiPatterns.md)
- **NEW** Common validation mistakes to avoid
- Execution-Validation-Execution anti-pattern
- Defensive null-checking proliferation
- Solutions with pattern matching

#### [Empty Pattern - Complete Guide](./CodeQualityGuidelines/EmptyPatternComplete.md)
- **🚨 CRITICAL** Universal Empty Pattern - NO EXCEPTIONS
- ALL entities must implement Empty pattern
- ALL repositories return Empty, never null
- Common misconceptions debunked
- Implementation checklist and violation detection

#### [Search Operation Error Handling](./CodeQualityGuidelines/SearchOperationErrorHandling.md)
- **SECURITY** Search/list operations always return 200 OK
- No error exposure to prevent information leakage
- Server-side logging for debugging
- Help desk workflow for issue resolution
- Applies to GET list endpoints only

#### [Null Object Pattern](./CodeQualityGuidelines/NullObjectPattern.md)
- **CRITICAL** Empty Object Pattern implementation
- IEmptyDto<T> interface requirements
- Layer-specific behavior (Database vs API)
- Avoiding over-validation anti-patterns

### Architecture Patterns

#### [Layered Architecture Rules](./CodeQualityGuidelines/ArchitecturalPatterns/LayeredArchitectureRules.md)
- **MANDATORY** separation of concerns
- Controller → Service → Repository layers
- No direct repository access from controllers
- Transaction management in service layer

#### [Request-Command Separation](./CodeQualityGuidelines/ArchitecturalPatterns/RequestCommandSeparation.md)
- Clean separation between web and service layers
- Request DTOs → Commands → Domain objects
- Type-safe ID handling
- Assembly independence

#### [Service Repository Boundaries](./CodeQualityGuidelines/ServiceRepositoryBoundaries.md)
- **CRITICAL** Single Repository Rule
- Service-to-service communication patterns
- Violation detection and refactoring guide
- Domain boundary enforcement

#### [Unit of Work Pattern](./CodeQualityGuidelines/UnitOfWorkPattern.md)
- **CRITICAL** ReadOnly vs Writable usage
- Entity tracking issues and solutions
- Transaction management patterns
- One UnitOfWork per method rule

#### [Controller Patterns](./CodeQualityGuidelines/ControllerPatterns.md)
- Thin pass-through layer requirements
- NO business logic in controllers
- Pure error message pass-through
- Pattern matching for HTTP status codes

#### [Controller HTTP Response Pattern Matching](./CodeQualityGuidelines/ControllerPatternMatchingOptimization.md)
- **OPTIMIZE** Group switch cases by HTTP status code, not error code
- Eliminate redundant cases returning same HTTP status
- Use default case for most common error response (BadRequest)
- Quick decision guide for when to add specific cases

### Type System

#### [Specialized ID Types](./CodeQualityGuidelines/SpecializedIdTypes.md)
- Type-safe entity identifiers
- ParseOrEmpty validation pattern
- ID format: `{entitytype}-{guid}`
- Controller and service usage patterns

#### [ID Validation Pattern](./CodeQualityGuidelines/IdValidationPattern.md)
- **CONSISTENT** Use EnsureNotEmpty for all ID validation
- No lambda expressions with negation for IDs
- Returns ValidationFailed (not InvalidFormat)
- Simplifies controller error handling

### Code Style

#### [Modern C# Patterns](./CodeQualityGuidelines/ModernCSharpPatterns.md)
- C# 12+ features (MANDATORY for new code)
- Collection expressions for empty collections
- Primary constructors for DI
- Pattern matching and switch expressions

#### [Pattern Matching Over If Statements](./CodeQualityGuidelines/PatternMatchingOverIfStatements.md)
- **PREFERRED** Use pattern matching instead of if statements
- Single exit point per method
- Switch expressions for success/failure checks
- Ternary operators for simple conditionals
- Eliminates early returns and nested blocks

#### [Extension Method Pattern](./CodeQualityGuidelines/ExtensionMethodPattern.md)
- **MANDATORY** Extract static helper methods as extensions
- Reduces service file size by 20-40%
- Improves readability and reusability
- "Too Much Information = No Information" principle

#### [Fluent Query Extensions Pattern](./CodeQualityGuidelines/FluentQueryExtensionsPattern.md)
- **PREFERRED** Replace complex if-statement chains with FluentAPI
- Clean, composable query building for repositories
- Individual filter methods with chaining capability
- Improves readability and maintainability of complex queries

#### [Fluent Sorting Pattern](./CodeQualityGuidelines/FluentSortingPattern.md)
- **NEW** Elegant conditional sorting with state tracking
- SortableQuery wrapper for intelligent default handling
- Replaces verbose switch/if statements for sorting
- Clean fluent chain showing all sortable fields

#### [Logging Hierarchy Pattern](./CodeQualityGuidelines/LoggingHierarchyPattern.md)
- **NEW** Push logging down to where operations occur
- Eliminates if-statements that exist only for logging
- Services focus on orchestration, not infrastructure
- Pattern matching replaces verbose logging conditions

### Quality Assurance

#### [Testing Standards](./CodeQualityGuidelines/TestingStandards.md)
- **🚨 CRITICAL** Test Independence - NO shared mocks at class level
- **🚨 CRITICAL** Use production error constants - NO recreating in tests
- **🚨 CRITICAL** NEVER test logging - it changes based on debugging needs
- **CRITICAL** No magic strings rule
- Test error codes, not messages
- Mock patterns and verification with AutoMocker
- BDD integration testing approach
- **NEW**: Feature file test isolation patterns
- **NEW**: Non-cumulative query counting pattern (Reset & Recount)

#### CRAP Score Reduction Strategy
**CRAP (Change Risk Anti-Patterns) Score** combines Cyclomatic Complexity and Code Coverage to measure maintainability risk.

**Formula**: `CRAP Score = Complexity² × (1 - Coverage)³ + Complexity`

**Action Guidelines**:
1. **CRAP Score > 30**: HIGH RISK - Immediate action required
2. **CRAP Score 21-30**: Medium risk - Plan refactoring
3. **CRAP Score < 21**: Acceptable

**Reduction Strategy**:
- **If Complexity ≤ 10**: Add tests FIRST (exponential impact on score)
- **If Complexity > 10**: Refactor FIRST, then add tests
- **Real Example**: WorkoutTemplateExerciseService reduced CRAP score from >100 to <10 by increasing coverage from 1% to 93.9%

**Why Test Coverage First (for reasonable complexity)**:
- Coverage has CUBIC impact: `(1 - Coverage)³`
- Method with complexity 5:
  - 0% coverage = CRAP score 30
  - 80% coverage = CRAP score 6
  - 95% coverage = CRAP score 5.03

### Data Access Patterns

#### [Repository Base Class Architecture](./Overview/RepositoryBaseClassArchitecture.md)
- **MANDATORY** All repositories must inherit from base classes
- Compile-time Empty pattern enforcement via generic constraints
- Eliminates null returns at architectural level
- Base class tests ensure compliance

#### [Repository Pattern](./CodeQualityGuidelines/RepositoryPattern.md)
- Pure data access layer
- NO business logic in repositories
- Empty pattern for entities
- AsNoTracking() for queries

#### [Entity Result Pattern](./CodeQualityGuidelines/EntityResultPattern.md)
- Entity creation validation
- Handler methods with EntityResult<T>
- Fluent validation for entities
- No exceptions in domain layer

#### [Single Exit Point Pattern](./CodeQualityGuidelines/SingleExitPointPattern.md)
- **MANDATORY** one return per method
- Pattern matching for branching
- Helper method extraction
- Reduced cyclomatic complexity

### Advanced Patterns

#### [Cache Integration Pattern](./CodeQualityGuidelines/CacheIntegrationPattern.md)
- Direct cache integration in services
- CacheLoad fluent API usage
- IEternalCacheService vs ICacheService
- Testing cache hit/miss scenarios

## 🚫 Critical Anti-Patterns to Avoid

### Redundant Entity Loading
**NEVER load the same entity multiple times in a validation chain!** This is a performance killer.

```csharp
// ❌ ANTI-PATTERN - Loading same entity 3+ times
.EnsureAsync(async () => await ExerciseExistsAsync(id), ...)      // Load 1
.EnsureAsync(async () => await IsExerciseActiveAsync(id), ...)     // Load 2
.EnsureAsync(async () => await IsNotRestTypeAsync(id), ...)        // Load 3
.EnsureAsync(async () => await HasValidMuscleGroupsAsync(id), ...) // Load 4

// ✅ CORRECT - Load once, validate many
.AsExerciseValidation()
.EnsureExerciseExists(service, id, "Not found")        // Load ONCE
.EnsureExerciseIsActive("Exercise is inactive")         // Use loaded entity
.EnsureExerciseIsNotRestType("Cannot be REST")          // Use loaded entity
.EnsureExerciseHasValidMuscleGroups("Invalid groups")   // Use loaded entity
```

**Key Rule**: Each entity should be loaded from the database EXACTLY ONCE per request. Use the Dual-Entity Validation Pattern for relationship validations.

### Double Negation in Validation
**NEVER use double negations in validation predicates!** They're hard to read and error-prone.

```csharp
// ❌ ANTI-PATTERN - Double negation is confusing
.EnsureNameIsUniqueAsync(
    async () => !(await _queryDataService.ExistsByNameAsync(name)).Data.Value,
    "Exercise", name)

// ✅ CORRECT - Positive assertion with helper method
.EnsureNameIsUniqueAsync(
    async () => await IsExerciseNameUniqueAsync(name),
    "Exercise", name)

private async Task<bool> IsExerciseNameUniqueAsync(string name)
{
    var existsResult = await _queryDataService.ExistsByNameAsync(name);
    return !existsResult.Data.Value; // Returns true when unique
}
```

### Command-like Validation Methods
**Validation methods should be QUESTIONS, not COMMANDS!**

```csharp
// ❌ ANTI-PATTERN - Sounds like a command
private async Task<bool> ValidateExerciseTypesAsync(ids)
private async Task<bool> ValidateKineticChainAsync(types, id)

// ✅ CORRECT - Clear question format
private async Task<bool> AreExerciseTypesValidAsync(ids)
private async Task<bool> IsKineticChainValidAsync(types, id)
```

### Magic Strings in Code
**NEVER hardcode error messages or other strings!** Use constants.

```csharp
// ❌ ANTI-PATTERN - Hardcoded error message
.EnsureHasValidAsync(
    async () => await IsKineticChainValidAsync(types, id),
    "REST exercises cannot have kinetic chain")

// ✅ CORRECT - Centralized constant
.EnsureHasValidAsync(
    async () => await IsKineticChainValidAsync(types, id),
    ExerciseErrorMessages.InvalidKineticChainForExerciseType)
```

### Conditional Validation with ThenEnsure Pattern
**USE ThenEnsure methods for dependent validations!** Prevents null reference exceptions and provides cleaner validation chains.

#### The Problem
When validating nullable objects, subsequent validations can fail with null reference exceptions:

```csharp
// ❌ DANGEROUS - Null reference if command is null
.EnsureNotNull(command, "Command cannot be null")
.EnsureNotWhiteSpace(command?.Email, "Email cannot be empty")  // Need ?. operator
.Ensure(() => IsValidEmail(command?.Email), "Invalid format")  // Need ?. operator everywhere!
```

#### The Solution: ThenEnsure Pattern
ThenEnsure methods only execute if the validation chain is still valid:

```csharp
// ✅ SAFE - No null reference possible
.EnsureNotNull(command, "Command cannot be null")
.ThenEnsureNotWhiteSpace(command.Email, "Email cannot be empty")  // Safe! No ?. needed
.ThenEnsureEmailIsValid(command.Email, "Invalid format")          // Safe! Chain continues only if valid
```

#### Available ThenEnsure Methods
- `ThenEnsure(predicate, error)` - Generic conditional validation
- `ThenEnsureAsync(asyncPredicate, error)` - Async conditional validation
- `ThenEnsureNotWhiteSpace(value, error)` - String validation after null check
- `ThenEnsureEmailIsValid(email, error)` - Email format validation
- `ThenEnsureNotEmpty(id, error)` - ID validation after null check

#### Key Benefits
1. **No Null Reference Exceptions** - Subsequent validations skip if previous ones fail
2. **Cleaner Code** - No need for `?.` null-conditional operators
3. **Better User Experience** - Users get the most relevant error first
4. **Progressive Validation** - Validates step-by-step, stopping at first failure

#### Real-World Example
```csharp
public async Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command)
{
    return await ServiceValidate.For<AuthenticationResponse>()
        .EnsureNotNull(command, AuthenticationErrorMessages.Validation.RequestCannotBeNull)
        .ThenEnsureNotWhiteSpace(command.Email, AuthenticationErrorMessages.Validation.EmailCannotBeEmpty)
        .ThenEnsureEmailIsValid(command.Email, AuthenticationErrorMessages.Validation.InvalidEmailFormat)
        .MatchAsync(
            whenValid: async () => await ProcessAuthenticationAsync(command)
        );
}
```

In this example:
- If `command` is null → returns "Request cannot be null"
- If `command.Email` is empty → returns "Email cannot be empty"  
- If email format is invalid → returns "Invalid email format"
- Otherwise → proceeds with authentication

### No Try-Catch Anti-Pattern
**NEVER use blanket try-catch blocks!** Shows lack of control over code flow.

```csharp
// ❌ ANTI-PATTERN
try {
    // ... entire method
} catch (Exception ex) {
    _logger.LogError(ex, "Error");
    return ServiceResult.Failure(...);
}

// ✅ CORRECT - Let patterns handle errors
return await ServiceValidate.For<T>()
    .EnsureNotNull(...)
    .MatchAsync(...);
```

### Multiple Exit Points Inside MatchAsync
**NEVER have multiple exit points inside MatchAsync!** All validations must be in the ServiceValidate chain.

```csharp
// ❌ ANTI-PATTERN - Multiple returns inside MatchAsync
return await ServiceValidate.Build<BooleanResultDto>()
    .EnsureNotEmpty(id, ErrorMessages.InvalidId)
    .MatchAsync(
        whenValid: async () =>
        {
            // VIOLATION: Multiple exit points inside MatchAsync!
            if (!await ExistsAsync(id))
                return ServiceResult<BooleanResultDto>.Failure(...);
            
            if (!await CanDeleteAsync(id))
                return ServiceResult<BooleanResultDto>.Failure(...);
            
            return await _dataService.DeleteAsync(id);
        }
    );

// ✅ CORRECT - Chain ALL validations before MatchAsync
return await ServiceValidate.Build<bool>()
    .EnsureNotEmpty(id, ErrorMessages.InvalidId)
    .EnsureAsync(
        async () => await ExistsAsync(id),
        ServiceError.NotFound("Equipment", id.ToString()))
    .EnsureAsync(
        async () => await CanDeleteAsync(id),
        ServiceError.DependencyExists("Equipment", "dependencies"))
    .MatchAsync(
        whenValid: async () => (await _dataService.DeleteAsync(id)).Data
    );
```

**Key Rule**: MatchAsync.whenValid must contain ONLY the single operation to perform when all validations pass. No if statements, no multiple returns, no business logic checks.

### No Bulk Scripts Policy
**NEVER use scripts for bulk file modifications!** Change files one-by-one manually.

> "Better to spend 2 hours changing 20 files manually than 6 hours fixing 6000+ errors from a script gone wrong."

### Nested Conditionals and Loops With If Statements
**NEVER use nested if statements or loops filled with conditionals!** This is a clear job for ServiceValidator pattern or extension methods.

#### The Problem: Foreach Loops with Nested Ifs
When you have a foreach loop with multiple if statements and early returns, it violates the single exit point principle and makes code hard to follow.

```csharp
// ❌ ANTI-PATTERN - Foreach with nested conditions and multiple exit points
foreach (var linkType in possibleReverseLinkTypes)
{
    var linksResult = await queryDataService.GetBySourceExerciseWithEnumAsync(
        workoutExerciseId, linkType);
    
    if (!linksResult.IsSuccess || linksResult.Data == null)
    {
        continue;  // Multiple flow control points
    }
    
    var reverseLink = linksResult.Data.FirstOrDefault(
        link => link.TargetExerciseId == originalSourceId);
    
    if (reverseLink != null)
    {
        logger.LogInformation("Found reverse link");  // Conditional logging
        return ServiceResult<ExerciseLinkDto>.Success(reverseLink);  // Early return
    }
}

// ✅ CORRECT - Clean extension method with single exit point
var reverseLink = await queryDataService.FindFirstMatchingReverseLinkAsync(
    workoutExerciseId,
    ExerciseLinkType.WORKOUT.GetPossibleReverseTypes(),
    originalSourceId,
    logger);

return reverseLink.ToServiceResult();
```

#### The Solution: Extension Methods Pattern
Create focused extension methods that encapsulate the conditional logic:

```csharp
// Extension method handles all the complexity internally
public static async Task<ExerciseLinkDto> FindFirstMatchingReverseLinkAsync(
    this IExerciseLinkQueryDataService queryDataService,
    ExerciseId targetId,
    IEnumerable<ExerciseLinkType> possibleTypes,
    string originalSourceId,
    ILogger logger)
{
    foreach (var linkType in possibleTypes)
    {
        var reverseLink = await queryDataService.TryFindReverseLinkAsync(
            targetId, linkType, originalSourceId, logger);
        
        if (reverseLink != null)
            return reverseLink;
    }
    
    // Logging pushed down to operation level
    logger.LogWarning("No reverse link found");
    return ExerciseLinkDto.Empty;
}
```

#### When You See Nested Ifs - It's ServiceValidator Time!
If you find yourself writing nested if statements for validation, STOP! Use ServiceValidator:

```csharp
// ❌ ANTI-PATTERN - Nested validation ifs
if (command != null)
{
    if (!string.IsNullOrEmpty(command.Email))
    {
        if (IsValidEmail(command.Email))
        {
            // Do the work
        }
        else
            return Error("Invalid email");
    }
    else
        return Error("Email required");
}
else
    return Error("Command required");

// ✅ CORRECT - ServiceValidator chain
return await ServiceValidate.For<Result>()
    .EnsureNotNull(command, "Command required")
    .ThenEnsureNotWhiteSpace(command.Email, "Email required")
    .ThenEnsureEmailIsValid(command.Email, "Invalid email")
    .MatchAsync(whenValid: async () => await DoWorkAsync());
```

#### Key Lessons from BidirectionalLinkHandler Refactoring

1. **44 lines → 7 lines**: The `FindReverseWorkoutLinkAsync` method was reduced by 84% through proper patterns
2. **Extension Methods Save the Day**: Creating focused extension methods eliminates complex control flow
3. **Logging Belongs at Operation Level**: Don't clutter service methods with conditional logging
4. **Pattern Matching Over If-Else**: Use switch expressions for cleaner branching
5. **Single Exit Point Always**: Every method should have exactly one return statement

**Red Flags to Watch For:**
- `continue` statements in loops → Extract to extension method
- Multiple `if` checks in sequence → Use ServiceValidator chain
- Early returns in loops → Violates single exit principle
- Conditional logging → Push down to operation level
- Nested if statements → Clear ServiceValidator opportunity

**The Golden Rule**: If you're writing nested conditionals or loops with multiple if statements, STOP and refactor using extension methods or ServiceValidator pattern.

### Complex If-Statement Chains in Queries
**AVOID complex conditional query building!** Use Fluent Query Extensions instead.

```csharp
// ❌ ANTI-PATTERN - Hard to read and maintain
if (!includeInactive)
    query = query.Where(e => e.IsActive);
if (!string.IsNullOrEmpty(name))
    query = query.Where(e => e.Name.Contains(name));
if (!difficultyId.IsEmpty)
    query = query.Where(e => e.DifficultyId == difficultyId);
// ... more conditions

// ✅ CORRECT - Fluent Query Extensions for filtering
query = query
    .FilterByActiveStatus(includeInactive)
    .FilterByNamePattern(name)
    .FilterByDifficulty(difficultyId);
```

### Verbose Sorting Logic
**AVOID verbose switch/if statements for sorting!** Use Fluent Sorting Pattern instead.

```csharp
// ❌ ANTI-PATTERN - Verbose sorting logic
var isDescending = sortOrder?.ToLower() == "desc";
query = (sortBy?.ToLower()) switch
{
    "name" => query.SortByName(isDescending),
    "date" => query.SortByDate(isDescending),
    _ => query.OrderBy(x => x.Id)
};

// ✅ CORRECT - Fluent Sorting Pattern
query = query
    .ToSortable()
    .ApplySortByName(sortBy, sortOrder)
    .ApplySortByDate(sortBy, sortOrder)
    .WithDefaultSort(q => q.OrderBy(x => x.Id));
```

### Combined Filter/Sort Methods
**NEVER hide multiple operations in a single method!** Keep filters and sorts visible.

```csharp
// ❌ ANTI-PATTERN - Hides what's being filtered/sorted
query = query.ApplyFilters(namePattern, categoryId, objectiveId, difficultyId, stateId);
query = query.ApplyFluentSorting(sortBy, sortOrder);

// ✅ CORRECT - Each operation is visible
query = query
    .FilterByNamePattern(namePattern)
    .FilterByCategory(categoryId)
    .FilterByObjective(objectiveId)
    .ToSortable()
    .ApplySortByName(sortBy, sortOrder)
    .WithDefaultSort(q => q.OrderBy(x => x.UpdatedAt));
```

See [FluentQueryExtensionsPattern.md](./CodeQualityGuidelines/FluentQueryExtensionsPattern.md) and [FluentSortingPattern.md](./CodeQualityGuidelines/FluentSortingPattern.md) for implementation details.

### Redundant Pattern Matching in Controllers
**AVOID multiple switch cases that return the same HTTP status!** Group by HTTP status code, not error code.

```csharp
// ❌ ANTI-PATTERN - Multiple cases for same HTTP status
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
    { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};

// ✅ CORRECT - One case per HTTP status
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })  // Handles ALL 400 errors
};
```

**Key Rule**: Controllers care about HTTP status codes, not business error codes. Only add specific cases for DIFFERENT HTTP statuses.

See [ControllerPatternMatchingOptimization.md](./CodeQualityGuidelines/ControllerPatternMatchingOptimization.md) for detailed guidelines.

### Logging-Only If-Statements
**AVOID if-statements that exist only for logging!** Push logging down to where operations occur.

```csharp
// ❌ ANTI-PATTERN - If-statement exists only for logging
if (!entityResult.IsSuccess)
{
    _logger.LogError("Entity creation failed: {Errors}", entityResult.Errors);
    return ServiceResult.Failure(entityResult.Errors);
}

var result = await _dataService.CreateAsync(entityResult.Value);

if (result.IsSuccess)
{
    _logger.LogInformation("Created {Id}", result.Data.Id);
}

return result;

// ✅ CORRECT - Pattern matching without logging clutter
return entityResult.IsSuccess switch
{
    true => await _dataService.CreateAsync(entityResult.Value),
    false => ServiceResult.Failure(entityResult.Errors)
};
```

**Key Principle**: Services orchestrate, they don't log. Logging belongs in DataServices, Repositories, and Handlers where operations actually occur.

See [LoggingHierarchyPattern.md](./CodeQualityGuidelines/LoggingHierarchyPattern.md) for implementation details.

### Defensive Code Anti-Pattern - Trust the Architecture
**STOP writing defensive code when the architecture guarantees safety!** We follow the NULL OBJECT PATTERN throughout the codebase.

#### Core Principle: We Are a NOT NULL Codebase
This codebase follows the **Empty Object Pattern** (Null Object Pattern). If a method returns null, there's a bug - it should return an Empty object instead.

#### Before Writing Defensive Code - CHECK THE SOURCE
Before adding defensive checks, **ALWAYS review the method that generates the entity**:
1. Check if the repository method can return null
2. Check if the service method handles Empty correctly
3. Check if MapToDto handles Empty/null cases

#### Real Example from DataServices

```csharp
// ❌ ANTI-PATTERN - Unnecessary defensive code
public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
    var entity = await repository.GetByValueAsync(value);
    
    // DEFENSIVE: Checking IsActive when repository ALWAYS returns Empty (never null)
    var dto = entity?.IsActive == true ? MapToDto(entity) : ExecutionProtocolDto.Empty;
    
    return ServiceResult<ExecutionProtocolDto>.Success(dto);
}

// ❌ ANTI-PATTERN - Over-defensive ToDto() check
public async Task<ServiceResult<ExerciseLinkDto>> GetByIdAsync(ExerciseLinkId id)
{
    using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
    
    var link = await repository.GetByIdAsync(id);
    // OVER-DEFENSIVE: ToDto() already checks IsEmpty internally!
    var dto = link.IsEmpty ? ExerciseLinkDto.Empty : link.ToDto();
    
    return ServiceResult<ExerciseLinkDto>.Success(dto);
}

// ✅ CORRECT - Trust the architecture
public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
    var entity = await repository.GetByValueAsync(value);
    
    // MapToDto ALREADY handles Empty/null cases internally
    var dto = MapToDto(entity);
    
    return ServiceResult<ExecutionProtocolDto>.Success(dto);
}

// ✅ CORRECT - Trust ToDto() to handle Empty
public async Task<ServiceResult<ExerciseLinkDto>> GetByIdAsync(ExerciseLinkId id)
{
    using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IExerciseLinkRepository>();
    
    var link = await repository.GetByIdAsync(id);
    var dto = link.ToDto(); // ToDto() handles Empty internally
    
    return ServiceResult<ExerciseLinkDto>.Success(dto);
}

// Why this works - MapToDto implementation:
private static ExecutionProtocolDto MapToDto(ExecutionProtocol entity)
{
    // MapToDto already handles both null AND IsEmpty cases!
    if (entity == null || entity.IsEmpty)
        return ExecutionProtocolDto.Empty;
        
    return new ExecutionProtocolDto { /* mapping */ };
}
```

#### Key Rules for Defensive Code

1. **Repositories Return Empty, Never Null**
   - All repository methods return Empty entities when not found
   - No need to check for null from repository calls
   - Trust the repository pattern

2. **MapToDto Handles Empty**
   - All MapToDto methods check for null/IsEmpty
   - Returns appropriate Empty DTO when needed
   - No need for defensive checks before calling MapToDto

3. **Commands Are Never Null**
   - Controllers create commands via ToCommand()
   - Services receive non-null commands
   - No null checks on command parameters

4. **ServiceResult Always Has Data**
   - Success results always have data (may be Empty)
   - Failure results always have Empty data
   - No need to check Data for null

#### When Defensive Code IS Appropriate

✅ **DO use defensive code when:**
- Integrating with external APIs
- Processing user input at system boundaries
- Dealing with legacy code that may return null
- Working with third-party libraries

❌ **DON'T use defensive code when:**
- Calling repository methods (they return Empty)
- Calling service methods (they return ServiceResult)
- Working with commands (guaranteed non-null)
- Inside MapToDto calls (already handles Empty)
- **Before calling ToDto() methods** (they handle Empty internally)

#### Checklist Before Adding Defensive Code

- [ ] Did I check if the source method can actually return null?
- [ ] Did I verify the Empty pattern is implemented?
- [ ] Is this at a system boundary (controller/external API)?
- [ ] Am I duplicating checks that exist elsewhere?
- [ ] Would a unit test prove this check is unnecessary?

#### The Trust Hierarchy

```
Controllers → Services → DataServices → Repositories → Database
    ↓            ↓            ↓             ↓            ↓
  Trust       Trust        Trust         Trust      Empty Pattern
```

Each layer trusts the layer below it because:
1. **Controllers trust Services** - Services return ServiceResult
2. **Services trust DataServices** - DataServices handle data access
3. **DataServices trust Repositories** - Repositories return Empty
4. **Repositories implement Empty Pattern** - Never return null

> **Remember**: Defensive code is a code smell. If you're adding null checks everywhere, you don't trust your architecture. Fix the architecture, don't bandage it with defensive code.

### Trust the Infrastructure - No Redundant Validation
**NEVER repeat validations that have already been done in the validation chain!** This violates the "Trust the Infrastructure" principle.

```csharp
// ❌ ANTI-PATTERN - Redundant validation in internal method
public async Task<ServiceResult<T>> UpdateAsync(Command command)
{
    return await ServiceValidate.Build<T>()
        .EnsureAsync(async () => await ExistsAsync(command.Id), NotFound())
        .MatchAsync(whenValid: async () => await UpdateInternalAsync(command));
}

private async Task<ServiceResult<T>> UpdateInternalAsync(Command command)
{
    var entity = await GetByIdAsync(command.Id);
    if (entity.IsEmpty) // REDUNDANT! Already validated above
    {
        return ServiceResult<T>.Failure(T.Empty, NotFound());
    }
    // ... update logic
}

// ❌ ANTI-PATTERN - DataService re-validating what Service already validated
public async Task<ServiceResult<T>> UpdateAsync(T dto)
{
    var id = ParseId(dto.Id);
    if (id.IsEmpty) // REDUNDANT! Service already validated
    {
        return ServiceResult<T>.Failure(T.Empty, ValidationFailed());
    }
    
    var entity = await repository.GetByIdAsync(id);
    if (entity.IsEmpty) // REDUNDANT! Service confirmed existence
    {
        return ServiceResult<T>.Failure(T.Empty, NotFound());
    }
}

// ✅ CORRECT - Trust the validation chain
public async Task<ServiceResult<T>> UpdateAsync(Command command)
{
    return await ServiceValidate.Build<T>()
        .EnsureAsync(async () => await ExistsAsync(command.Id), NotFound())
        .MatchAsync(whenValid: async () => await UpdateInternalAsync(command));
}

private async Task<ServiceResult<T>> UpdateInternalAsync(Command command)
{
    // TRUST! No defensive checks - validation already done
    var entity = await GetByIdAsync(command.Id);
    // ... update logic
}

// ✅ CORRECT - DataService trusts Service validation
public async Task<ServiceResult<T>> UpdateAsync(T dto)
{
    // TRUST! Parse and use directly
    var id = ParseId(dto.Id);
    var entity = await repository.GetByIdAsync(id);
    // ... update logic
}
```

#### Key Principle: Validation Happens ONCE
- **Service Layer**: All business validation in ServiceValidate chain
- **Internal Methods**: Trust the validation chain, no redundant checks
- **DataService Layer**: Trust that Service has validated everything
- **Repository Layer**: Just execute, no validation

### Entity Leakage Anti-Pattern - CRITICAL VIOLATION
**NEVER return entities from DataServices to Services!** This violates the fundamental layered architecture.

#### The Problem: Entities Escaping Data Layer

```csharp
// ❌ ANTI-PATTERN - DataService returns entity to Service
public async Task<ServiceResult<ExerciseLink>> GetEntityByIdAsync(ExerciseLinkId id)
{
    var link = await repository.GetByIdAsync(id);
    return ServiceResult<ExerciseLink>.Success(link); // WRONG! Entity leaked!
}

// ❌ ANTI-PATTERN - Service receives and manipulates entity
var entityResult = await queryDataService.GetEntityByIdAsync(linkId);
var existingLink = entityResult.Data;
var updatedLink = ExerciseLink.Handler.Create(...); // Service creating entities!
return await commandDataService.UpdateAsync(updatedLink); // Passing entity!

// ❌ ANTI-PATTERN - CommandDataService takes entity as parameter
public interface IExerciseLinkCommandDataService
{
    Task<ServiceResult<ExerciseLinkDto>> UpdateAsync(ExerciseLink exerciseLink); // Takes entity!
}
```

#### The Correct Pattern: Transformation Functions

```csharp
// ✅ CORRECT - CommandDataService with transformation function
public interface IExerciseCommandDataService
{
    Task<ServiceResult<ExerciseDto>> UpdateAsync(
        ExerciseId id,
        Func<Exercise, Exercise> updateAction); // Function, not entity!
}

// ✅ CORRECT - Service passes transformation logic, never sees entity
return await _commandDataService.UpdateAsync(id, exercise =>
{
    return exercise with {
        Name = command.Name,
        Description = command.Description
    };
});

// ✅ CORRECT - DataService handles entity internally
public async Task<ServiceResult<ExerciseDto>> UpdateAsync(
    ExerciseId id,
    Func<Exercise, Exercise> updateAction)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IExerciseRepository>();
    
    var entity = await repository.GetByIdAsync(id); // Entity stays here
    var updated = updateAction(entity);             // Transform internally
    await repository.UpdateAsync(updated);          // Save internally
    await unitOfWork.SaveChangesAsync();
    
    return ServiceResult<ExerciseDto>.Success(updated.ToDto()); // Return DTO
}
```

#### Architecture Rules for Entity Boundaries

1. **Entities are Data Layer Citizens**: They NEVER leave DataServices
2. **Services Work with DTOs/Commands**: Never with entities
3. **DataServices Are the Boundary**: They encapsulate ALL entity operations
4. **Use Transformation Functions**: Pass `Func<T,T>` for updates, not entities
5. **Entity Creation in DataService**: `Handler.Create` calls belong in DataService

#### The Layered Architecture Contract

```
Controllers → Services → DataServices → Repositories → Database
     ↓           ↓            ↓              ↓            ↓
  Requests   Commands      Internal      Entities     Tables
  Responses    DTOs      Operations       Empty      Records
              
  NEVER: Services ← Entities (This is the violation!)
```

#### Checklist for Entity Boundary Compliance

- [ ] No `GetEntityByIdAsync` methods returning entities to services
- [ ] No service methods receiving entity parameters
- [ ] No `Entity.Handler.Create` calls in service layer
- [ ] All entity manipulation inside DataServices
- [ ] Update operations use `Func<Entity, Entity>` pattern
- [ ] Only DTOs and Commands cross service boundaries

> **Remember**: If a service can see an entity, the architecture is broken. DataServices are the guardians of the entity boundary.

## 🚨 Test Code Smells - Fix Immediately

### Verbose Object Creation
**THIS IS AN ANTI-PATTERN!** Creating test objects with all properties is a maintenance nightmare.

```csharp
// ❌ ANTI-PATTERN - Verbose object creation
var exerciseDto = new ExerciseDto
{
    Id = sourceId.ToString(),
    Name = "Warmup Exercise",
    Description = "Test warmup exercise",
    IsActive = true,
    ExerciseTypes = [
        new() { Id = "type-1", Value = "Warmup", Description = "Warmup type" }
    ],
    Difficulty = new ReferenceDataDto { Id = "diff-1", Value = "Easy", Description = "Easy" },
    // ... 10 more properties not relevant to the test
};

// ✅ CORRECT - Test Builder Pattern
var exerciseDto = ExerciseDtoTestBuilder.WarmupExercise()
    .WithId(sourceId)
    .Build();
```

### Inline Comments for Parameters
**Parameters needing comments = BAD CODE!** Use builders with named methods.

```csharp
// ❌ ANTI-PATTERN - Comments explaining parameters
ExecutionProtocol.Handler.Create(
    ExecutionProtocolId.From(Guid.Parse("11111111-1111-1111-1111-111111111111")),
    "Standard",                               // value
    "Traditional set and rep scheme",         // description
    "STANDARD",                               // code
    false,                                    // timeBase
    true,                                     // repBase
    "Fixed Rest",                             // restPattern
    "Moderate",                               // intensityLevel
    1,                                        // displayOrder
    true)                                     // isActive

// ✅ CORRECT - Self-documenting builder
ExecutionProtocolTestBuilder.Standard()
    .WithDescription("Custom description")
    .WithIntensityLevel("High")
    .Build();
```

### Test Constants for Irrelevant Data
**40+ lines of constants = WRONG!** Use builders with sensible defaults.

```csharp
// ❌ ANTI-PATTERN - Unnecessary constants
public class ExerciseLinkServiceTests
{
    private const string WarmupExerciseName = "Warmup Exercise";
    private const string WarmupExerciseDescription = "Test warmup exercise";
    private const string WorkoutExerciseName = "Workout Exercise";
    // ... 40 more lines of constants

// ✅ CORRECT - Defaults in builders
public class ExerciseLinkServiceTests
{
    // NO constants - builders have defaults
    // Tests only specify what they're testing
```

### Verbose Mock Setup
**Inline Setup() calls = POOR READABILITY!** Use extension methods.

```csharp
// ❌ ANTI-PATTERN - Verbose mock setup
exerciseServiceMock
    .Setup(x => x.GetByIdAsync(sourceId))
    .ReturnsAsync(ServiceResult<ExerciseDto>.Success(sourceExerciseDto));
    
exerciseServiceMock
    .Setup(x => x.GetByIdAsync(targetId))
    .ReturnsAsync(ServiceResult<ExerciseDto>.Success(targetExerciseDto));

// ✅ CORRECT - Fluent extension methods
exerciseServiceMock
    .SetupExerciseById(sourceId, sourceExerciseDto)
    .SetupExerciseById(targetId, targetExerciseDto);
```

### Setting Properties Not Under Test
**FOCUS VIOLATION!** Only set what you're testing.

```csharp
// ❌ ANTI-PATTERN - Setting everything
var dto = new ExerciseDto
{
    Id = id,
    Name = "Test",           // Not under test
    Description = "Desc",    // Not under test
    VideoUrl = "http://...", // Not under test
    ImageUrl = "http://...", // Not under test
    IsActive = true          // THIS is what we're testing
};

// ✅ CORRECT - Focus on what matters
var dto = ExerciseDtoTestBuilder.Default()
    .WithId(id)
    .AsActive()  // Only what we're testing
    .Build();
```

### Testing Logging - DON'T DO IT!
**NEVER mock or test logging!** Logging is an implementation detail that changes based on debugging needs.

```csharp
// ❌ ANTI-PATTERN - Setting up logger mocks
var loggerFactoryMock = autoMocker.GetMock<ILoggerFactory>();
loggerFactoryMock
    .Setup(x => x.CreateLogger(It.IsAny<string>()))
    .Returns(Mock.Of<ILogger>());

loggerMock.Verify(x => x.LogInformation(...), Times.Once); // WRONG!

// ✅ CORRECT - Let AutoMocker handle it automatically
var testee = autoMocker.CreateInstance<MyService>(); // AutoMocker provides logger mocks
// NO verification of logger calls - we don't care!
```

**Why logging should NEVER be tested:**
1. **It changes frequently** - More logs when debugging, fewer in production
2. **Not business logic** - Logging is infrastructure, not behavior
3. **Brittle tests** - Tests break when log messages change
4. **No value** - Testing logging doesn't prove correctness
5. **AutoMocker handles it** - The framework provides mocks automatically

## 🎯 Quick Decision Guide

| Scenario | Pattern to Use | Documentation |
|----------|---------------|---------------|
| High CRAP Score (>30) | Add test coverage first (if complexity ≤ 10) | See CRAP Score Reduction Strategy above |
| High Complexity (>10) | Refactor first, then test | See CRAP Score Reduction Strategy above |
| Nested if statements | ServiceValidator pattern or Extension methods | See Nested Conditionals section above |
| Foreach with multiple ifs | Extract to extension method | See Nested Conditionals section above |
| Loops with conditionals and early returns | Extension methods with single exit | See Nested Conditionals section above |
| Redundant validation across layers | Trust the Validation Chain | [TrustTheValidationChain.md](./CodeQualityGuidelines/TrustTheValidationChain.md) |
| Service method return type | ServiceResult<T> | [ServiceResultPattern.md](./CodeQualityGuidelines/ServiceResultPattern.md) |
| Input validation (all sync) | ServiceValidate.For<T>() | [ServiceValidatePattern.md](./CodeQualityGuidelines/ServiceValidatePattern.md) |
| Input validation (any async) | ServiceValidate.Build<T>() | [ServiceValidatePattern.md](./CodeQualityGuidelines/ServiceValidatePattern.md) |
| Dependent validations | ThenEnsure methods | See Conditional Validation section above |
| Multiple business validations | Chain EnsureAsync calls, NOT inside MatchAsync | See examples in anti-patterns section |
| Relationship validation | Dual-Entity Pattern | [DualEntityValidationPattern.md](./CodeQualityGuidelines/DualEntityValidationPattern.md) |
| Avoiding redundant DB calls | Load once, validate many | [DualEntityValidationPattern.md](./CodeQualityGuidelines/DualEntityValidationPattern.md) |
| Clean validation approach | Positive assertions | [CleanValidationPattern.md](./CodeQualityGuidelines/CleanValidationPattern.md) |
| Validation mistakes | Avoid anti-patterns | [ValidationAntiPatterns.md](./CodeQualityGuidelines/ValidationAntiPatterns.md) |
| Null handling | Null Object Pattern | [NullObjectPattern.md](./CodeQualityGuidelines/NullObjectPattern.md) |
| Static helper methods | Extension methods | [ExtensionMethodPattern.md](./CodeQualityGuidelines/ExtensionMethodPattern.md) |
| Layer separation | Layered Architecture | [LayeredArchitectureRules.md](./CodeQualityGuidelines/ArchitecturalPatterns/LayeredArchitectureRules.md) |
| Web to service mapping | Request-Command Pattern | [RequestCommandSeparation.md](./CodeQualityGuidelines/ArchitecturalPatterns/RequestCommandSeparation.md) |
| Query operations | ReadOnlyUnitOfWork | [UnitOfWorkPattern.md](./CodeQualityGuidelines/UnitOfWorkPattern.md) |
| Modifications | WritableUnitOfWork | [UnitOfWorkPattern.md](./CodeQualityGuidelines/UnitOfWorkPattern.md) |
| Cross-domain access | Service dependencies | [ServiceRepositoryBoundaries.md](./CodeQualityGuidelines/ServiceRepositoryBoundaries.md) |
| ID parameters | Specialized ID types | [SpecializedIdTypes.md](./CodeQualityGuidelines/SpecializedIdTypes.md) |
| Controller logic | Pattern matching only | [ControllerPatterns.md](./CodeQualityGuidelines/ControllerPatterns.md) |
| Controller error responses | Group by HTTP status | [ControllerPatternMatchingOptimization.md](./CodeQualityGuidelines/ControllerPatternMatchingOptimization.md) |
| Test data creation | Test Builder Pattern | [TestBuilderPattern.md](./Overview/TestBuilderPattern.md) |
| Mock setup | Fluent extension methods | [TestingStandards.md](./CodeQualityGuidelines/TestingStandards.md) |
| Test object properties | Focus Principle - only set what's tested | [TestBuilderPattern.md](./Overview/TestBuilderPattern.md) |
| Error testing | Test codes, not messages | [TestingStandards.md](./CodeQualityGuidelines/TestingStandards.md) |
| Logging in tests | NEVER test or mock logging | See Testing Logging section above |
| Reference data caching | IEternalCacheService | [CacheIntegrationPattern.md](./CodeQualityGuidelines/CacheIntegrationPattern.md) |
| Single return statement | Pattern matching | [SingleExitPointPattern.md](./CodeQualityGuidelines/SingleExitPointPattern.md) |
| Entity creation | EntityResult<T> | [EntityResultPattern.md](./CodeQualityGuidelines/EntityResultPattern.md) |
| Data access layer | Pure repositories | [RepositoryPattern.md](./CodeQualityGuidelines/RepositoryPattern.md) |
| Repository implementation | Inherit from base class | [RepositoryBaseClassPattern.md](./CodeQualityGuidelines/RepositoryBaseClassPattern.md) |
| Query filtering | Fluent extensions | [FluentQueryExtensionsPattern.md](./CodeQualityGuidelines/FluentQueryExtensionsPattern.md) |
| Conditional sorting | SortableQuery pattern | [FluentSortingPattern.md](./CodeQualityGuidelines/FluentSortingPattern.md) |
| Logging in services | Push down to operations | [LoggingHierarchyPattern.md](./CodeQualityGuidelines/LoggingHierarchyPattern.md) |

## 📊 Code Review Checklist

Before approving any PR, verify:

- [ ] All service methods return `ServiceResult<T>`
- [ ] No null returns (Empty pattern used)
- [ ] ServiceValidate used for validation
- [ ] Single exit points in all methods
- [ ] **NO nested if statements** (use ServiceValidator or extension methods)
- [ ] **NO foreach loops with multiple ifs** (extract to extension methods)
- [ ] **Single operation only in MatchAsync.whenValid** (no if statements or multiple returns)
- [ ] **All validations chained in ServiceValidate** (not inside MatchAsync)
- [ ] **NO redundant entity loading** (each entity loaded ONCE per request)
- [ ] **Dual-Entity Pattern used for relationship validations**
- [ ] **NO double negations in validation predicates** (`!(await something)` is WRONG)
- [ ] **Validation methods are questions** (IsValid, HasPermission, CanDelete)
- [ ] **Helper methods use positive naming** (returns true for positive state)
- [ ] **NO magic strings** - All error messages in constants
- [ ] Minimal defensive null checking (trust boundaries)
- [ ] Controllers have no business logic
- [ ] **Controller switch expressions group by HTTP status** (no duplicate BadRequest cases)
- [ ] ReadOnly for queries, Writable for modifications
- [ ] Services only access their own repositories
- [ ] **Services NEVER receive entities** (only DTOs and Commands)
- [ ] **DataServices NEVER return entities** (transformation functions for updates)
- [ ] **Repositories inherit from base classes** (compile-time Empty enforcement)
- [ ] Specialized IDs used (not strings)
- [ ] No try-catch for business logic
- [ ] Tests check error codes, not messages
- [ ] **Tests have complete independence** (no shared mocks at class level)
- [ ] **Tests use production error constants** (not recreated local constants)
- [ ] **Test Builder Pattern used for ALL test data** (no verbose object creation)
- [ ] **Mock setups use fluent extension methods** (no inline Setup() calls)
- [ ] **Focus Principle applied** (only properties under test are set)
- [ ] **No test constants for irrelevant data** (use builder defaults)
- [ ] Modern C# patterns applied (C# 12+)
- [ ] Static helpers extracted as extension methods
- [ ] Cache integration follows patterns
- [ ] **Query filtering uses individual extension methods** (not combined ApplyFilters)
- [ ] **Conditional sorting uses SortableQuery pattern** (not verbose switch statements)
- [ ] **No logging in service layer** (push logging down to operations)
- [ ] **No if-statements that exist only for logging**
- [ ] **NEVER test or verify logging calls** (implementation detail)

## 🔗 Related Documents

### Process Documentation
- [BUG_IMPLEMENTATION_PROCESS.md](./DevelopmentGuidelines/BugImplementationProcess.md)
- [FEATURE_IMPLEMENTATION_PROCESS.md](./DevelopmentGuidelines/FeatureImplementationProcess.md)
- [RELEASE_PROCESS.md](./DevelopmentGuidelines/ReleaseProcess.md)

### Practical Guides & Quick References
- [TestingQuickReference.md](./PracticalGuides/TestingQuickReference.md) ⚡ - Common test failures & instant solutions
- [UnitTestingWithAutoMocker.md](./PracticalGuides/UnitTestingWithAutoMocker.md) 🎯 - Modern unit testing with AutoMocker & FluentAssertions
- [CommonImplementationPitfalls.md](./PracticalGuides/CommonImplementationPitfalls.md) ⚠️ - Critical mistakes to avoid
- [ServiceImplementationChecklist.md](./PracticalGuides/ServiceImplementationChecklist.md) 📋 - Step-by-step checklist
- [CommonTestingErrorsAndSolutions.md](./PracticalGuides/CommonTestingErrorsAndSolutions.md) - Detailed testing patterns
- [IntegrationTestingSetup.md](./PracticalGuides/IntegrationTestingSetup.md) 🐳 - Docker + PostgreSQL setup guide
- [AccuracyInFailureAnalysis.md](./PracticalGuides/AccuracyInFailureAnalysis.md) 🎯 - How to analyze failures accurately (never speculate!)

### Architecture Documentation
- [System Patterns](./Overview/SystemPatterns.md)
- [Database Model Pattern](./Overview/DatabaseModelPattern.md)
- [Three-Tier Entity Architecture](./Overview/ThreeTierEntityArchitecture.md)
- [Reference Tables Overview](./Overview/ReferenceTablesOverview.md)
- [Cache Configuration](./Overview/CacheConfiguration.md)
- [AutoMocker Testing Pattern](./Overview/AutoMockerTestingPattern.md) 📊 - 12% code reduction & 6,240% ROI proof

## 💡 Getting Started

1. **New to the project?** Start with the [Golden Rules](#-golden-rules---non-negotiable)
2. **Writing unit tests?** Follow [UnitTestingWithAutoMocker.md](./PracticalGuides/UnitTestingWithAutoMocker.md)
3. **Implementing a service?** Check [ServiceImplementationChecklist.md](./PracticalGuides/ServiceImplementationChecklist.md)
4. **Fixing test failures?** See [TestingQuickReference.md](./PracticalGuides/TestingQuickReference.md)
5. **Setting up integration tests?** Check [IntegrationTestingSetup.md](./PracticalGuides/IntegrationTestingSetup.md)
6. **Avoiding common mistakes?** Review [CommonImplementationPitfalls.md](./PracticalGuides/CommonImplementationPitfalls.md)
7. **Analyzing problems accurately?** Follow [AccuracyInFailureAnalysis.md](./PracticalGuides/AccuracyInFailureAnalysis.md)
8. **Refactoring code?** Review relevant pattern guidelines in [CodeQualityGuidelines/](./CodeQualityGuidelines/)

## ⚠️ Remember

> "These standards are not suggestions - they are requirements. Following them ensures our codebase remains maintainable, testable, and scalable."

---

*This is a living document. As patterns evolve, detailed guidelines are maintained in the [CodeQualityGuidelines/](./CodeQualityGuidelines/) folder.*