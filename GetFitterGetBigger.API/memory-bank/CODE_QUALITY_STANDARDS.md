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

### Quality Assurance

#### [Testing Standards](./CodeQualityGuidelines/TestingStandards.md)
- **CRITICAL** No magic strings rule
- Test error codes, not messages
- Mock patterns and verification
- BDD integration testing approach
- **NEW**: Feature file test isolation patterns
- **NEW**: Non-cumulative query counting pattern (Reset & Recount)

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
| Clean validation approach | Positive assertions | [CleanValidationPattern.md](./CodeQualityGuidelines/CleanValidationPattern.md) |
| Validation mistakes | Avoid anti-patterns | [ValidationAntiPatterns.md](./CodeQualityGuidelines/ValidationAntiPatterns.md) |
| Null handling | Null Object Pattern | [NullObjectPattern.md](./CodeQualityGuidelines/NullObjectPattern.md) |
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

## 📊 Code Review Checklist

Before approving any PR, verify:

- [ ] All service methods return `ServiceResult<T>`
- [ ] No null returns (Empty pattern used)
- [ ] ServiceValidate used for validation
- [ ] Single exit points in all methods
- [ ] Validation uses positive assertions (no double negatives)
- [ ] Minimal defensive null checking (trust boundaries)
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
- [BUG_IMPLEMENTATION_PROCESS.md](./DevelopmentGuidelines/BugImplementationProcess.md)
- [FEATURE_IMPLEMENTATION_PROCESS.md](./DevelopmentGuidelines/FeatureImplementationProcess.md)
- [RELEASE_PROCESS.md](./DevelopmentGuidelines/ReleaseProcess.md)

### Practical Guides & Quick References
- [TestingQuickReference.md](./PracticalGuides/TestingQuickReference.md) ⚡ - Common test failures & instant solutions
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

## 💡 Getting Started

1. **New to the project?** Start with the [Golden Rules](#-golden-rules---non-negotiable)
2. **Implementing a service?** Check [ServiceImplementationChecklist.md](./PracticalGuides/ServiceImplementationChecklist.md)
3. **Fixing test failures?** See [TestingQuickReference.md](./PracticalGuides/TestingQuickReference.md)
4. **Setting up integration tests?** Check [IntegrationTestingSetup.md](./PracticalGuides/IntegrationTestingSetup.md)
5. **Avoiding common mistakes?** Review [CommonImplementationPitfalls.md](./PracticalGuides/CommonImplementationPitfalls.md)
6. **Analyzing problems accurately?** Follow [AccuracyInFailureAnalysis.md](./PracticalGuides/AccuracyInFailureAnalysis.md)
7. **Refactoring code?** Review relevant pattern guidelines in [CodeQualityGuidelines/](./CodeQualityGuidelines/)

## ⚠️ Remember

> "These standards are not suggestions - they are requirements. Following them ensures our codebase remains maintainable, testable, and scalable."

---

*This is a living document. As patterns evolve, detailed guidelines are maintained in the [CodeQualityGuidelines/](./CodeQualityGuidelines/) folder.*