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
- Quick reference table and examples

#### [Clean Validation Pattern](./CodeQualityGuidelines/CleanValidationPattern.md)
- **NEW** Positive assertions and trust boundaries
- Simplified validation with smart overloads
- Minimal defensive programming
- Helper methods with positive naming

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

### Quality Assurance

#### [Testing Standards](./CodeQualityGuidelines/TestingStandards.md)
- **CRITICAL** No magic strings rule
- Test error codes, not messages
- Mock patterns and verification
- BDD integration testing approach
- **NEW**: Feature file test isolation patterns
- **NEW**: Non-cumulative query counting pattern (Reset & Recount)

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

// ✅ CORRECT - Fluent Query Extensions
query = query
    .FilterByActiveStatus(includeInactive)
    .FilterByNamePattern(name)
    .FilterByDifficulty(difficultyId)
    .ApplyFluentSorting(sortBy, sortOrder);
```

See [FluentQueryExtensionsPattern.md](./CodeQualityGuidelines/FluentQueryExtensionsPattern.md) for implementation details.

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

## 🎯 Quick Decision Guide

| Scenario | Pattern to Use | Documentation |
|----------|---------------|---------------|
| Service method return type | ServiceResult<T> | [ServiceResultPattern.md](./CodeQualityGuidelines/ServiceResultPattern.md) |
| Input validation (all sync) | ServiceValidate.For<T>() | [ServiceValidatePattern.md](./CodeQualityGuidelines/ServiceValidatePattern.md) |
| Input validation (any async) | ServiceValidate.Build<T>() | [ServiceValidatePattern.md](./CodeQualityGuidelines/ServiceValidatePattern.md) |
| Multiple business validations | Chain EnsureAsync calls, NOT inside MatchAsync | See examples in anti-patterns section |
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
| Error testing | Test codes, not messages | [TestingStandards.md](./CodeQualityGuidelines/TestingStandards.md) |
| Reference data caching | IEternalCacheService | [CacheIntegrationPattern.md](./CodeQualityGuidelines/CacheIntegrationPattern.md) |
| Single return statement | Pattern matching | [SingleExitPointPattern.md](./CodeQualityGuidelines/SingleExitPointPattern.md) |
| Entity creation | EntityResult<T> | [EntityResultPattern.md](./CodeQualityGuidelines/EntityResultPattern.md) |
| Data access layer | Pure repositories | [RepositoryPattern.md](./CodeQualityGuidelines/RepositoryPattern.md) |
| Repository implementation | Inherit from base class | [RepositoryBaseClassPattern.md](./CodeQualityGuidelines/RepositoryBaseClassPattern.md) |

## 📊 Code Review Checklist

Before approving any PR, verify:

- [ ] All service methods return `ServiceResult<T>`
- [ ] No null returns (Empty pattern used)
- [ ] ServiceValidate used for validation
- [ ] Single exit points in all methods
- [ ] **Single operation only in MatchAsync.whenValid** (no if statements or multiple returns)
- [ ] **All validations chained in ServiceValidate** (not inside MatchAsync)
- [ ] **NO double negations in validation predicates** (`!(await something)` is WRONG)
- [ ] **Validation methods are questions** (IsValid, HasPermission, CanDelete)
- [ ] **Helper methods use positive naming** (returns true for positive state)
- [ ] **NO magic strings** - All error messages in constants
- [ ] Minimal defensive null checking (trust boundaries)
- [ ] Controllers have no business logic
- [ ] ReadOnly for queries, Writable for modifications
- [ ] Services only access their own repositories
- [ ] **Repositories inherit from base classes** (compile-time Empty enforcement)
- [ ] Specialized IDs used (not strings)
- [ ] No try-catch for business logic
- [ ] Tests check error codes, not messages
- [ ] Modern C# patterns applied (C# 12+)
- [ ] Static helpers extracted as extension methods
- [ ] Cache integration follows patterns

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