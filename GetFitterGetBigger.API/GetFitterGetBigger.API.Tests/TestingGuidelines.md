# Testing Guidelines - Branch Coverage

## Why Branch Coverage Matters

Branch coverage measures how many decision paths in your code are tested. A branch is created whenever your code can take different paths based on conditions (if/else, switch, try/catch, etc.).

## Current Status
- **Branch Coverage**: 58.7% (107 of 182 branches)
- **Goal**: >80% branch coverage

## Testing Principles

### 1. Test All Scenarios, Not Just Happy Path

Every method should be tested for:
- **Happy Path**: Normal, expected behavior
- **Edge Cases**: Boundary conditions, empty collections, etc.
- **Error Cases**: Invalid inputs, null values, exceptions
- **Validation Failures**: When business rules are violated

### 2. Common Patterns to Test

#### For Repository Methods
```csharp
// Happy path
- Valid ID returns entity
- Valid data creates/updates successfully

// Error cases
- Null/empty ID returns null
- Non-existent ID returns null
- Database exceptions are handled
- Concurrent operations
```

#### For Controller Actions
```csharp
// Happy path
- Valid requests return Ok(200)
- Created resources return Created(201)

// Error cases
- Invalid model state returns BadRequest(400)
- Missing resources return NotFound(404)
- Exceptions return InternalServerError(500)
- Unauthorized access returns Unauthorized(401)
```

#### For Validation Methods
```csharp
// Test all validation branches
- Valid input passes
- Null input fails
- Empty string fails
- Too long/short input fails
- Invalid format fails
- Boundary values
```

### 3. Example: Comprehensive TryParse Testing

```csharp
[Fact]
public void TryParse_ValidInput_ReturnsTrue() { } // Happy path

[Fact]
public void TryParse_NullInput_ReturnsFalse() { } // Null check branch

[Fact]
public void TryParse_EmptyInput_ReturnsFalse() { } // Empty check branch

[Fact]
public void TryParse_InvalidFormat_ReturnsFalse() { } // Format validation branch

[Fact]
public void TryParse_WrongPrefix_ReturnsFalse() { } // Prefix check branch
```

## Areas Needing Branch Coverage Improvement

Based on the coverage report, these areas need attention:

1. **Controller Error Handling** (50-62% branch coverage)
   - Missing NotFound scenarios
   - Missing BadRequest scenarios
   - Missing exception handling tests

2. **Repository Methods** (50% branch coverage)
   - Missing null parameter tests
   - Missing database exception tests
   - Missing concurrent operation tests

3. **JSON Converters** (0% branch coverage)
   - No tests for serialization/deserialization
   - Missing invalid JSON tests

4. **WritableUnitOfWork** (0% branch coverage)
   - Missing disposal tests
   - Missing transaction tests

## Testing Checklist

Before considering a class fully tested, verify:

- [ ] All public methods have tests
- [ ] Each if/else branch is covered
- [ ] Each switch case is tested
- [ ] Try/catch blocks test both success and exception paths
- [ ] Null/empty inputs are tested
- [ ] Boundary conditions are tested
- [ ] Business rule violations are tested
- [ ] Concurrent scenarios are considered

## Running Branch Coverage Report

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator -reports:coverage.cobertura.xml -targetdir:TestResults/CoverageReport -reporttypes:Html

# View report
open TestResults/CoverageReport/index.html
```

## Remember

**High branch coverage != bug-free code**, but it does mean:
- More confidence in refactoring
- Fewer surprises in production
- Better understanding of edge cases
- Documentation through tests