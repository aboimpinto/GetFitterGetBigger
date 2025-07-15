# Refactoring and Test Evolution Guide

## Key Principle: Tests Are Not Immutable Truth

Tests are valuable guardrails, but during refactoring, they must evolve with the code. Tests that were once meaningful can become obstacles to architectural improvements.

## When Tests Should Change

### 1. Architectural Pattern Changes
When refactoring to implement new patterns (like Empty/Null Object Pattern), tests that validate old behavior become invalid:
- **Example**: Tests expecting `null` returns when we've eliminated nulls
- **Action**: Rewrite tests to validate the new pattern

### 2. Internal Implementation Changes
- **Unit Tests**: Should change to reflect new internal structure
- **Integration Tests**: Should remain stable if external behavior is preserved
- **Exception**: When the refactor changes the contract (e.g., error codes)

### 3. Dead Code Elimination
When refactoring reveals unreachable code paths:
- **Example**: InternalError (500) responses for simple reference data queries
- **Action**: Remove tests for impossible scenarios

## Critical Questions for Each Failing Test

Before modifying code to satisfy a failing test, ask:

1. **Is this test still relevant?**
   - Does it test behavior that should exist in the new architecture?
   - Example: Tests for null handling when nulls are eliminated

2. **Should this test be deleted and replaced?**
   - Is it testing an anti-pattern we're removing?
   - Example: Tests for null returns vs Empty object returns

3. **Does this test need rewriting?**
   - Is the concept valid but the implementation outdated?
   - Example: Validation tests that need different error codes

4. **Should this test remain unchanged?**
   - Is it validating critical business behavior?
   - Example: Security and authorization tests

## Real-World Example: Empty ID Handling

### The Problem
- Integration tests expected 400 Bad Request for invalid ID formats
- Service returned 404 Not Found for empty IDs
- Initial approach: Changed service behavior to match tests

### The Better Solution
1. Use pattern matching to handle empty IDs explicitly
2. Return appropriate error (ValidationFailed) immediately
3. Avoid unnecessary database calls
4. Update both unit and integration tests to match

### Lessons Learned
- Don't break architectural patterns to satisfy tests
- Consider if the test expectation is correct
- Sometimes the test is wrong, not the implementation

## Guidelines for Test Evolution

### 1. Preserve Intent, Not Implementation
- Focus on what the test validates, not how
- Adapt assertions to new patterns while preserving business rules

### 2. Document Breaking Changes
- When tests change significantly, document why
- Include in commit messages and PR descriptions

### 3. Batch Related Changes
- Group test updates with their corresponding refactors
- Makes review and rollback easier

### 4. Consider Test Hierarchy
- Integration tests should be more stable than unit tests
- Unit tests can change freely with implementation
- Contract tests should rarely change

## Magic String Testing - A Critical Anti-Pattern

### Why Testing Specific Error Messages is Dangerous

Testing for exact error message strings is one of the most fragile testing patterns:

```gherkin
# BAD: Testing specific error message content
Then the response should contain "Invalid ID format"
And the response should contain "Expected format: 'difficultylevel-{guid}'"
```

#### Problems with Magic String Testing:
1. **Localization Breaks Tests** - Error messages may be translated
2. **Minor Text Changes Break Tests** - "Invalid ID format" vs "Invalid difficulty level ID format"
3. **Violates DRY** - Error messages defined in multiple places
4. **Tests Implementation, Not Behavior** - We care about the error code, not exact wording

### Better Approach: Test Error Codes and Status

```gherkin
# GOOD: Test behavior, not strings
Then the response status should be 400
# If error structure matters, test the structure, not content
```

```csharp
// GOOD: In unit tests, test error types
Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
```

### Real Example: DifficultyLevel Refactor

**Problem**: Integration test expected exact error message:
- Test: `response should contain "Invalid ID format"`
- Service: Returns `"Invalid difficulty level ID format"`
- Result: Test fails despite correct behavior

**Solution**: Follow BodyParts pattern - test only status codes:
```gherkin
# From BodyParts.feature - the correct pattern
Scenario: Get body part by invalid ID format returns bad request
  When I send a GET request to "/api/ReferenceTables/BodyParts/invalid-guid"
  Then the response status should be 400
  # No string checking!
```

### Guidelines for Error Testing

1. **Test Status Codes** - 400, 404, 500 etc.
2. **Test Error Codes** - Use enumerations, not strings
3. **Test Error Structure** - Presence of error array, not content
4. **Never Test Exact Messages** - They're presentation layer concerns

## Anti-Patterns to Avoid

### 1. Test-Driven Architecture Degradation
Breaking clean architecture to make tests pass:
```csharp
// BAD: Adding special cases to satisfy outdated tests
if (someConditionJustForTests) 
{
    return oldBehavior;
}
```

### 2. Preserving Obsolete Tests
Keeping tests that no longer make sense:
```csharp
// BAD: Test for null when nulls are eliminated
[Fact]
public void Method_ReturnsNull_WhenNotFound() // Obsolete concept
```

### 3. Over-Mocking to Maintain Tests
Creating complex mocks to simulate old behavior:
```csharp
// BAD: Mocking removed behavior
mock.Setup(x => x.ReturnsNull()).Returns((string)null); // Pattern removed
```

## Best Practices

### 1. Refactor Tests Alongside Code
- Update tests as part of the refactoring commit
- Don't leave broken tests for later

### 2. Use Descriptive Test Names
- Reflect new patterns in test names
- Example: `ReturnsEmpty` instead of `ReturnsNull`

### 3. Validate Business Rules, Not Implementation
- Focus on outcomes, not internal details
- Allow implementation flexibility

### 4. Regular Test Audit
- Periodically review test relevance
- Remove obsolete tests
- Consolidate redundant tests
- Check for magic string dependencies

### 5. Ask Before Breaking Architecture
When tests fail after correct implementation:
1. **STOP** - Don't modify code to satisfy tests
2. **VERIFY** - Confirm implementation matches reference patterns
3. **REPORT** - Create detailed failure report for user
4. **ASK** - Get guidance on test evolution vs code changes

## Conclusion

Tests are tools to ensure quality, not chains that prevent improvement. During refactoring:
- Be critical about test relevance
- Don't compromise architecture for tests
- Evolve tests to support better patterns
- Document why tests changed

Remember: The goal is maintainable, clean code that serves business needs, not satisfying outdated test expectations.