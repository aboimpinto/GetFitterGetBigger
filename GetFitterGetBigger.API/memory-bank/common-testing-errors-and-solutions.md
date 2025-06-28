# Common Testing Errors and Solutions

This document captures lessons learned from debugging test failures, particularly from BUG-005 where 6 tests were failing due to test infrastructure issues rather than application bugs.

## 1. Specialized ID Format Errors

### Problem
Tests use incorrect ID formats when working with specialized ID types (e.g., ExerciseTypeId, EquipmentId).

### Example Error
```csharp
// ❌ WRONG - Including type name in the ID
"exercisetype-rest-11111111-1111-1111-1111-111111111111"
"exercisetype-warmup-22222222-2222-2222-2222-222222222222"

// ✅ CORRECT - Standard format
"exercisetype-11111111-1111-1111-1111-111111111111"
"exercisetype-22222222-2222-2222-2222-222222222222"
```

### Root Cause
The specialized ID parsing methods (e.g., `ExerciseTypeId.TryParse`) expect the format `"{entitytype}-{guid}"` without any additional descriptors.

### Solution
Always use the standard format: `"{entitytype}-{guid}"`

### How to Detect
- Check if `TryParse` methods are returning false
- Look for ArgumentNullException when entities should exist
- Verify ID format matches the pattern expected by the parsing method

## 2. Missing Mock Setups in Unit Tests

### Problem
Unit tests fail because repository mocks are not configured to return entities.

### Example Error
```csharp
// Test expects 3 exercise types but gets 0
Assert.Equal(3, result.ExerciseTypes.Count); // Fails: Expected 3, Actual 0
```

### Root Cause
When a repository method is called but not mocked, it returns null by default.

### Solution
```csharp
// Mock all repository calls used by the service
_exerciseTypeRepositoryMock.Setup(r => r.GetByIdAsync(typeId))
    .ReturnsAsync(exerciseType);
```

### How to Detect
- Collections are empty when they should have items
- NullReferenceException in service methods
- Tests expecting entities but getting null

## 3. Navigation Properties Not Loaded After Insert

### Problem
After creating an entity with EF Core, navigation properties are null even though foreign keys are set.

### Example Error
```csharp
// After insert, ExerciseType.Value is null even though ExerciseTypeId is set
Assert.Equal("Workout", createdExercise.ExerciseTypes[0].Value); // Fails: Expected "Workout", Actual ""
```

### Root Cause
EF Core doesn't automatically load navigation properties after insert operations.

### Solution
```csharp
// Explicitly load navigation properties after insert
foreach (var eet in exercise.ExerciseExerciseTypes)
{
    await Context.Entry(eet)
        .Reference(x => x.ExerciseType)
        .LoadAsync();
}
```

### How to Detect
- Navigation properties are null after insert
- DTO mapping returns empty strings for related entity properties
- Foreign keys are set but related entities are not populated

## 4. Integration Tests vs Unit Tests Confusion

### Problem
Assuming test infrastructure works the same way for integration tests and unit tests.

### Key Differences
- **Integration Tests**: Use real database (or in-memory), seeded data is available
- **Unit Tests**: Use mocks, only mocked data is available

### Solution
- For integration tests: Ensure test data is properly seeded
- For unit tests: Mock every external dependency call

### How to Detect
- Unit tests fail but integration tests pass (or vice versa)
- Repository calls return unexpected results
- Different behavior between test types

## 5. Test Data Consistency

### Problem
Test data IDs don't match between test setup and test execution.

### Example
```csharp
// Test setup seeds one ID
ExerciseTypeId.From(Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00"))

// But test uses different ID
"exercisetype-99999999-9999-9999-9999-999999999999"
```

### Solution
- Use constants for test IDs
- Reference the same seeded data IDs in tests
- Document which IDs correspond to which test entities

## Best Practices for Debugging Test Failures

### 1. Check Test Infrastructure First
Before assuming application bugs, verify:
- ID formats are correct
- All mocks are properly configured
- Test data matches expectations
- Navigation properties are loaded when needed

### 2. Differentiate Test Types
- Understand if you're working with unit tests (mocks) or integration tests (real database)
- Apply appropriate patterns for each test type

### 3. Use Descriptive Test Names
Good test names help identify what's being tested and expected behavior

### 4. Mock Completely
In unit tests, mock every external dependency:
- Repository calls
- Service calls
- Any external system interaction

### 5. Verify Navigation Property Loading
When working with EF Core:
- Check if navigation properties need explicit loading
- Use Include() in queries when needed
- Load navigation properties after insert/update operations

## Common Patterns to Remember

### ID Format Pattern
```
{entitytype}-{guid}
```
Examples:
- `exercise-12345678-1234-1234-1234-123456789012`
- `exercisetype-12345678-1234-1234-1234-123456789012`
- `equipment-12345678-1234-1234-1234-123456789012`

### Mock Setup Pattern
```csharp
_repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<TId>()))
    .ReturnsAsync(entity);
```

### Navigation Property Loading Pattern
```csharp
await Context.Entry(entity)
    .Reference(e => e.NavigationProperty)
    .LoadAsync();
```

## Debugging Checklist

When tests fail:
1. ✓ Check ID formats match expected patterns
2. ✓ Verify all repository calls are mocked (for unit tests)
3. ✓ Ensure test data is properly seeded (for integration tests)
4. ✓ Check if navigation properties need explicit loading
5. ✓ Verify mock return values match test expectations
6. ✓ Confirm test is using correct test data IDs
7. ✓ Check for differences between unit and integration test setup

## Prevention Strategies

1. **Create Test Constants**: Define common test IDs in a shared location
2. **Use Test Builders**: Create builder patterns for complex test objects
3. **Document Test Data**: Maintain documentation of what test data represents
4. **Consistent Patterns**: Use the same patterns across all tests
5. **Code Reviews**: Review test code as carefully as production code

Remember: Test failures don't always indicate application bugs. Often, the test infrastructure itself needs adjustment.