# API Code Quality Standards - Overview

**🎯 PURPOSE**: Central index for API-specific code quality standards in the GetFitterGetBigger API project. This overview links to detailed guidelines organized by topic.

## 🚨 GOLDEN RULES - NON-NEGOTIABLE

```
┌─────────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: These API rules MUST be followed - NO EXCEPTIONS  │
├─────────────────────────────────────────────────────────────────┤
│ 1. Single Exit Point per method - USE PATTERN MATCHING         │
│ 2. ServiceResult<T> for ALL service methods                    │
│ 3. No null returns - USE EMPTY PATTERN                         │
│ 4. ReadOnlyUnitOfWork for queries, WritableUnitOfWork for mods │
│ 5. Pattern matching in controllers for ServiceResult handling  │
│ 6. No try-catch for business logic - ANTI-PATTERN             │
│ 7. No bulk scripts for refactoring - MANUAL ONLY              │
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
- Fluent validation API replacing manual validation
- Single exit points
- Mixed sync/async chains
- Integration with ServiceResult

#### [Empty Object Pattern](./CodeQualityGuidelines/EmptyObjectPattern.md)
- Null Object Pattern implementation
- IEmptyDto<T> interface requirements
- Layer-specific behavior (Database vs API)
- Decision framework for tests vs production

### Architecture Patterns

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

### Code Style

#### [Modern C# Patterns](./CodeQualityGuidelines/ModernCSharpPatterns.md)
- C# 12+ features (MANDATORY for new code)
- Collection expressions for empty collections
- Primary constructors for DI
- Pattern matching and switch expressions

### Quality Assurance

#### [Testing Standards](./CodeQualityGuidelines/TestingStandards.md)
- **CRITICAL** No magic strings rule
- Test error codes, not messages
- Mock patterns and verification
- BDD integration testing approach

### Data Access Patterns

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

### No Bulk Scripts Policy
**NEVER use scripts for bulk file modifications!** Change files one-by-one manually.

> "Better to spend 2 hours changing 20 files manually than 6 hours fixing 6000+ errors from a script gone wrong."

## 🎯 Quick Decision Guide

| Scenario | Pattern to Use | Documentation |
|----------|---------------|---------------|
| Service method return type | ServiceResult<T> | [ServiceResultPattern.md](./CodeQualityGuidelines/ServiceResultPattern.md) |
| Input validation | ServiceValidate fluent API | [ServiceValidatePattern.md](./CodeQualityGuidelines/ServiceValidatePattern.md) |
| Null handling | Empty Object Pattern | [EmptyObjectPattern.md](./CodeQualityGuidelines/EmptyObjectPattern.md) |
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

## 📊 Code Review Checklist

Before approving any PR, verify:

- [ ] All service methods return `ServiceResult<T>`
- [ ] No null returns (Empty pattern used)
- [ ] ServiceValidate used for validation
- [ ] Single exit points in all methods
- [ ] Controllers have no business logic
- [ ] ReadOnly for queries, Writable for modifications
- [ ] Services only access their own repositories
- [ ] Specialized IDs used (not strings)
- [ ] No try-catch for business logic
- [ ] Tests check error codes, not messages
- [ ] Modern C# patterns applied (C# 12+)
- [ ] Cache integration follows patterns

## 🔗 Related Documents

### Process Documentation
- [BUG_IMPLEMENTATION_PROCESS.md](./BUG_IMPLEMENTATION_PROCESS.md)
- [FEATURE_IMPLEMENTATION_PROCESS.md](./FEATURE_IMPLEMENTATION_PROCESS.md)
- [RELEASE_PROCESS.md](./RELEASE_PROCESS.md)

### Quick References
- [TESTING-QUICK-REFERENCE.md](./TESTING-QUICK-REFERENCE.md) ⚡
- [common-implementation-pitfalls.md](./common-implementation-pitfalls.md) ⚠️
- [service-implementation-checklist.md](./service-implementation-checklist.md) 📋

### Architecture Documentation
- [systemPatterns.md](./systemPatterns.md)
- [databaseModelPattern.md](./databaseModelPattern.md)
- [unitOfWorkPattern.md](./unitOfWorkPattern.md)

## 💡 Getting Started

1. **New to the project?** Start with the [Golden Rules](#-golden-rules---non-negotiable)
2. **Implementing a service?** Check [service-implementation-checklist.md](./service-implementation-checklist.md)
3. **Fixing test failures?** See [TESTING-QUICK-REFERENCE.md](./TESTING-QUICK-REFERENCE.md)
4. **Refactoring code?** Review relevant pattern guidelines in [CodeQualityGuidelines/](./CodeQualityGuidelines/)

## ⚠️ Remember

> "These standards are not suggestions - they are requirements. Following them ensures our codebase remains maintainable, testable, and scalable."

---

*This is a living document. As patterns evolve, detailed guidelines are maintained in the [CodeQualityGuidelines/](./CodeQualityGuidelines/) folder.*