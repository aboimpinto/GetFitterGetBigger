# Testing Guidelines

This document provides comprehensive testing guidelines for all GetFitterGetBigger projects. These guidelines ensure consistent, high-quality testing across API, Admin, and Clients projects.

## Core Testing Principles

### 1. Test Coverage Requirements
- **Branch Coverage Target**: >80% for all new code
- **Focus Areas**: Business logic, validation, error handling
- **Measurement**: Use project-specific coverage tools

### 2. Test-First Approach (Mandatory for Bugs)
1. Write failing tests that reproduce the bug
2. Implement minimal fix to make tests pass
3. Refactor if needed while keeping tests green
4. Add edge case tests to prevent regression

### 3. Test Categories

#### Unit Tests
- Test individual components in isolation
- Mock all external dependencies
- Fast execution (<100ms per test)
- Cover all public methods and edge cases

#### Integration Tests
- Test component interactions
- Use real implementations where practical
- Test database operations with test database
- Verify API endpoints end-to-end

#### Validation Tests
- Test all validation rules explicitly
- Cover both valid and invalid inputs
- Test boundary conditions
- Verify error messages are correct

## Project-Specific Guidelines

### API Project (.NET/C#)

#### Test Structure
```
GetFitterGetBigger.API.Tests/
├── Unit/
│   ├── Controllers/
│   ├── Services/
│   ├── Validators/
│   └── Repositories/
├── Integration/
│   ├── Controllers/
│   └── Repositories/
└── TestHelpers/
```

#### Key Patterns
```csharp
// ID Format Pattern
var id = Guid.NewGuid();
var entity = new Entity { Id = $"{entityType}-{id}" };

// Mock Setup Pattern
_mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
    .ReturnsAsync((string id) => testData.FirstOrDefault(x => x.Id == id));

// Navigation Loading Pattern
await _repository.InsertAsync(entity);
var saved = await _repository.GetByIdAsync(entity.Id);
// saved will have navigation properties loaded
```

#### Common Commands
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test category
dotnet test --filter Category=Unit
```

### Admin Project (React/TypeScript)

#### Test Structure
```
src/
├── components/
│   └── __tests__/
├── services/
│   └── __tests__/
└── utils/
    └── __tests__/
```

#### Key Patterns
```typescript
// Component Testing
import { render, screen, fireEvent } from '@testing-library/react';

// Mock API calls
jest.mock('../services/api', () => ({
  getExercises: jest.fn().mockResolvedValue([])
}));

// Test user interactions
fireEvent.click(screen.getByText('Submit'));
await waitFor(() => expect(mockFn).toHaveBeenCalled());
```

#### Common Commands
```bash
# Run all tests
npm test

# Run with coverage
npm test -- --coverage

# Run in watch mode
npm test -- --watch

# Run specific test file
npm test -- ExerciseList.test.tsx
```

### Clients Project (Multi-platform)

#### Platform-Specific Testing
- **iOS**: XCTest framework
- **Android**: JUnit + Espresso
- **Web**: Same as Admin project
- **Desktop**: Platform-specific frameworks

## Common Testing Patterns

### 1. Arrange-Act-Assert (AAA)
```csharp
// Arrange
var input = new CreateExerciseDto { Name = "Test" };
var mockRepository = new Mock<IRepository>();

// Act
var result = await service.CreateAsync(input);

// Assert
Assert.NotNull(result);
Assert.Equal("Test", result.Name);
```

### 2. Test Naming Convention
```
MethodName_StateUnderTest_ExpectedBehavior

Examples:
- GetById_ValidId_ReturnsEntity
- Create_DuplicateName_ThrowsValidationException
- Update_ConcurrentModification_ThrowsConflictException
```

### 3. Edge Case Testing
Always test:
- Null/empty inputs
- Boundary values
- Concurrent operations
- Permission scenarios
- Network failures (for integration tests)

## Boy Scout Rule in Testing

When working on ANY test file:
- Fix all failing tests in the file
- Add missing test cases noticed
- Improve test readability
- Update outdated assertions
- Remove redundant tests

## Debugging Failed Tests

### Common Issues and Solutions

#### 1. ID Format Mismatches
**Problem**: Tests expect wrong ID format
**Solution**: Use `{entityType}-{guid}` format only

#### 2. Navigation Properties Null
**Problem**: Related entities not loaded
**Solution**: Reload entity after insert/update

#### 3. Mock Setup Not Matching
**Problem**: Mock expectations don't match actual calls
**Solution**: Use `It.IsAny<>()` for flexible matching

#### 4. Async Timing Issues
**Problem**: Test completes before async operation
**Solution**: Use proper async/await throughout

#### 5. Test Data Conflicts
**Problem**: Tests interfere with each other
**Solution**: Use unique test data per test

## Quick Reference Checklist

### Before Committing
- [ ] All tests pass locally
- [ ] New code has >80% branch coverage
- [ ] No skipped tests without justification
- [ ] Test names clearly describe behavior
- [ ] No hardcoded test data that could break

### When Adding New Feature
- [ ] Unit tests for all public methods
- [ ] Integration tests for API endpoints
- [ ] Validation tests for all rules
- [ ] Error scenario tests
- [ ] Performance tests for critical paths

### When Fixing Bugs
- [ ] Failing test reproduces the bug
- [ ] Fix makes the test pass
- [ ] Additional tests prevent regression
- [ ] All related tests still pass
- [ ] Boy Scout Rule applied

## Test Quality Metrics

### Good Tests Are:
- **Fast**: Execute quickly (<1s for unit tests)
- **Isolated**: Don't depend on other tests
- **Repeatable**: Same result every time
- **Self-Checking**: Clear pass/fail result
- **Timely**: Written with or before code

### Coverage Goals
- **Unit Tests**: 90%+ coverage
- **Integration Tests**: All happy paths + key error paths
- **End-to-End Tests**: Critical user journeys only

## Continuous Integration

All projects must:
1. Run tests automatically on PR
2. Block merge if tests fail
3. Report coverage metrics
4. Run tests on multiple platforms (where applicable)

## Resources

- API Testing: See `TESTING-QUICK-REFERENCE.md` in API project
- Admin Testing: See React Testing Library docs
- Clients Testing: See platform-specific guides
- Coverage Tools: Project-specific documentation

Remember: Quality tests are an investment in maintainability and confidence in the codebase.