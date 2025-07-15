# Critical Code Review Prompt: {EntityName} Empty Pattern Refactor

## Instructions for Use
Replace all instances of `{EntityName}` with your actual entity name (e.g., "Equipment", "ExerciseType")
Replace all instances of `{entityname}` with lowercase entity name (e.g., "equipment", "exercisetype")
Replace all instances of `{EntityNameId}` with your ID type name (e.g., "EquipmentId", "ExerciseTypeId")

## Review Objective
Perform a critical code review of the {EntityName} refactor to ensure it fully implements the Empty/Null Object Pattern without any null propagation or exception throwing. This review must verify strict adherence to established patterns from BodyPart and MovementPattern implementations.

## Review Scope

### Files to Review
1. **Entity Layer**
   - `/GetFitterGetBigger.API/Models/Entities/{EntityName}.cs`
   - `/GetFitterGetBigger.API/Models/SpecializedIds/{EntityNameId}.cs`

2. **Repository Layer**
   - Any changes to repository implementation for {EntityName}
   - Verify inheritance from `EmptyEnabledReferenceDataRepository`

3. **Service Layer**
   - `/GetFitterGetBigger.API/Services/Interfaces/I{EntityName}Service.cs`
   - `/GetFitterGetBigger.API/Services/Implementations/{EntityName}Service.cs`

4. **Controller Layer**
   - `/GetFitterGetBigger.API/Controllers/{EntityName}sController.cs`

5. **Dependency Injection**
   - `/GetFitterGetBigger.API/Program.cs` (service registration)

6. **Tests**
   - `/GetFitterGetBigger.API.Tests/Services/{EntityName}ServiceTests.cs`
   - `/GetFitterGetBigger.API.Tests/Controllers/{EntityName}sControllerTests.cs`
   - `/GetFitterGetBigger.API.IntegrationTests/Features/ReferenceData/{EntityName}s.feature`

## Critical Review Checklist

### 1. NULL Propagation Check
**CRITICAL**: Report ANY instance of null handling, including:
- [ ] Null checks (`if (x == null)`, `x?.Property`)
- [ ] Null coalescing operators (`??`, `??=`)
- [ ] Nullable reference types that aren't part of DTOs
- [ ] Methods returning null
- [ ] Any `FirstOrDefault()` without `.Empty` fallback
- [ ] Any `Find()` or `SingleOrDefault()` usage

**Expected**: Zero null handling - everything should use Empty pattern

### 2. Exception Throwing Check
**CRITICAL**: Report ANY exception throwing:
- [ ] Direct `throw new Exception()`
- [ ] `throw new ArgumentException()`
- [ ] Any custom exceptions
- [ ] Exception handling that re-throws

**Expected**: Zero exceptions - all errors via ServiceResult pattern

### 3. Obsolete Method Check
**CRITICAL**: Report ANY usage of obsolete methods:
- [ ] No [Obsolete] attributed methods are used
- [ ] No [Obsolete] attributed methods are called
- [ ] No deprecated patterns from old implementations

**Expected**: Zero obsolete method usage - only current patterns

### 4. Pattern Adherence Verification

#### Entity Implementation
- [ ] Implements `IEmptyEntity<{EntityName}>`
- [ ] Has static `Empty` property
- [ ] Has `IsEmpty` property
- [ ] Implements `IPureReference`
- [ ] No nullable properties (except Description as per pattern)

#### ID Type Implementation
- [ ] `ParseOrEmpty` returns Empty, never throws
- [ ] `TryParse` is private
- [ ] No public parsing methods that throw
- [ ] Proper ToString() implementation

#### Service Implementation
- [ ] Extends `EmptyEnabledPureReferenceService<{EntityName}, ReferenceDataDto>`
- [ ] Pattern matching: `id.IsEmpty ? ValidationFailed : GetByIdAsync(id.ToString())`
- [ ] Returns ServiceResult<T> for all methods
- [ ] No database calls for empty IDs
- [ ] Proper caching with IEmptyEnabledCacheService

#### Controller Implementation
- [ ] Simple pass-through to service
- [ ] Pattern matching on ServiceResult
- [ ] Returns appropriate HTTP status codes (200/400/404)
- [ ] No business logic in controller

### 5. Code Flow Analysis

Trace the complete flow for these scenarios:

#### Scenario A: Valid ID Request
```
GET /api/ReferenceTables/{EntityName}s/{entityname}-{valid-guid}
```
- [ ] Controller receives string ID
- [ ] {EntityNameId}.ParseOrEmpty converts to ID
- [ ] Service checks if ID.IsEmpty (false)
- [ ] Service calls base GetByIdAsync with string
- [ ] Base service checks cache
- [ ] If not cached, loads from DB
- [ ] Returns ServiceResult.Success with DTO
- [ ] Controller returns 200 OK

#### Scenario B: Invalid Format ID
```
GET /api/ReferenceTables/{EntityName}s/{invalid-format}
```
- [ ] Controller receives string ID
- [ ] {EntityNameId}.ParseOrEmpty returns Empty
- [ ] Service checks if ID.IsEmpty (true)
- [ ] Service returns ServiceResult.Failure with ValidationFailed
- [ ] Controller returns 400 Bad Request

#### Scenario C: Valid Format, Non-existent ID
```
GET /api/ReferenceTables/{EntityName}s/{entityname}-{non-existent-guid}
```
- [ ] Controller receives string ID
- [ ] {EntityNameId}.ParseOrEmpty converts to ID
- [ ] Service checks if ID.IsEmpty (false)
- [ ] Service calls base GetByIdAsync
- [ ] Base service checks cache/DB
- [ ] Entity not found, returns NotFound error
- [ ] Controller returns 404 Not Found

### 6. Comparison with Reference Implementations

Compare line-by-line with:
- [ ] BodyPartService.cs - Service pattern
- [ ] BodyPartsController.cs - Controller pattern
- [ ] BodyPartId.cs - ID type pattern

Report ANY deviations from these patterns.

## Review Output Format

```markdown
# {EntityName} Refactor Code Review

## Executive Summary
- **Null Handling Found**: Yes/No (count)
- **Exceptions Found**: Yes/No (count)
- **Pattern Compliance**: Full/Partial/Non-compliant
- **Ready for Merge**: Yes/No

## Critical Issues

### Null Propagation Instances
1. **File**: [path]
   **Line**: [number]
   **Code**: `[offending code]`
   **Issue**: [description]
   **Fix**: [recommended fix]

### Exception Throwing Instances
1. **File**: [path]
   **Line**: [number]
   **Code**: `[offending code]`
   **Issue**: [description]
   **Fix**: [recommended fix]

### Pattern Violations
1. **File**: [path]
   **Issue**: [deviation from reference pattern]
   **Reference**: [how BodyPart/MovementPattern does it]
   **Fix**: [required change]

### Obsolete Method Usage
1. **File**: [path]
   **Line**: [number]
   **Code**: `[offending code]`
   **Issue**: [obsolete method usage]
   **Fix**: [use current pattern instead]

## Code Flow Verification
- [ ] Valid ID flow: PASS/FAIL
- [ ] Invalid format flow: PASS/FAIL
- [ ] Non-existent ID flow: PASS/FAIL

## Recommendations
[List any improvements or concerns]

## Sign-off Checklist
- [ ] No null handling present
- [ ] No exceptions thrown
- [ ] No obsolete methods used
- [ ] Follows Empty pattern exactly
- [ ] Matches reference implementations
- [ ] All tests updated appropriately
- [ ] Ready for production
```

## Additional Review Focus

### Performance Considerations
- Verify no unnecessary database calls for invalid IDs
- Check caching implementation matches reference pattern
- Ensure pattern matching prevents redundant operations

### Test Quality
- Tests should NOT check magic strings
- Tests should verify behavior, not implementation
- Integration tests should match BodyParts.feature pattern

### Documentation
- Verify all public methods have XML documentation
- Check that documentation mentions Empty pattern behavior
- Ensure no documentation references null returns

## Magic String Analysis
- [ ] Review test files for magic string usage
- [ ] Check if TestConstants class exists for {EntityName}
- [ ] Verify Test Builder pattern implementation
- [ ] Ensure consistent test data across integration tests

## Security Review
- [ ] No direct SQL queries - uses EF Core
- [ ] Proper input validation
- [ ] No sensitive data exposure
- [ ] Follows established security patterns

## Maintainability Assessment
- [ ] Consistent with established patterns
- [ ] Clear separation of concerns
- [ ] Proper error handling
- [ ] Easy to extend and modify

## Code Quality Metrics
- **Cyclomatic Complexity**: [Low/Medium/High]
- **Code Duplication**: [None/Minimal/Concerning]
- **Naming Conventions**: [Consistent/Inconsistent]
- **SOLID Principles**: [Fully adhered to/Partially/Not followed]

## Files Reviewed
- `/Models/Entities/{EntityName}.cs`
- `/Models/SpecializedIds/{EntityNameId}.cs`
- `/Repositories/Interfaces/I{EntityName}Repository.cs`
- `/Repositories/Implementations/{EntityName}Repository.cs`
- `/Services/Interfaces/I{EntityName}Service.cs`
- `/Services/Implementations/{EntityName}Service.cs`
- `/Controllers/{EntityName}sController.cs`
- `/Program.cs` (DI registration)
- `/Tests/Services/{EntityName}ServiceTests.cs`
- `/Tests/Controllers/{EntityName}sControllerTests.cs`

This code review should be thorough and critical. Any deviation from the established patterns must be reported, even if tests are passing.

**Final Verdict**: [APPROVED FOR MERGE / REQUIRES CHANGES / BLOCKED]

---

**Review Completed**: [Date]  
**Status**: [APPROVED / REQUIRES CHANGES / BLOCKED]