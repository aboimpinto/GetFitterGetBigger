# Unit Tests vs Integration Tests - Complete Guide

## 🎯 Overview

This document provides clear guidelines for distinguishing between unit tests and integration tests in the GetFitterGetBigger API project. It enforces the architectural separation established during FEAT-024.

**Key Rule**: Tests that require database connections are ALWAYS integration tests and MUST be in the `API.IntegrationTests` project.

## 📊 Test Type Definitions

### Unit Tests (API.Tests Project)

**Definition**: Tests that verify isolated units of code without external dependencies.

**Characteristics**:
- ✅ Use mocked dependencies (repositories, services, etc.)
- ✅ Run in-memory without database connections
- ✅ Test single classes/methods in isolation
- ✅ Very fast execution (milliseconds)
- ✅ No Docker requirement

**What to Test**:
- Business logic in services (with ALL dependencies mocked)
- Controller actions with mocked services (NEVER real services)
- DTOs and mappers (pure functions only)
- Validators and utilities (isolated validation logic)
- Entity business rules (without ANY persistence)

**Example**:
```csharp
// Unit test with EVERYTHING mocked
public class ExerciseServiceTests
{
    private readonly Mock<IExerciseRepository> _mockRepository;
    private readonly Mock<IExerciseTypeRepository> _mockTypeRepository;
    private readonly Mock<IValidator<Exercise>> _mockValidator;
    private readonly Mock<ILogger<ExerciseService>> _mockLogger;
    private readonly ExerciseService _service; // ONLY real class
    
    public ExerciseServiceTests()
    {
        // Mock EVERY dependency
        _mockRepository = new Mock<IExerciseRepository>();
        _mockTypeRepository = new Mock<IExerciseTypeRepository>();
        _mockValidator = new Mock<IValidator<Exercise>>();
        _mockLogger = new Mock<ILogger<ExerciseService>>();
        
        // ONLY the service under test is real
        _service = new ExerciseService(
            _mockRepository.Object,
            _mockTypeRepository.Object,
            _mockValidator.Object,
            _mockLogger.Object
        );
    }
    
    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange - Mock ALL dependencies
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
            
        _mockTypeRepository.Setup(r => r.GetByIdAsync(It.IsAny<ExerciseTypeId>()))
            .ReturnsAsync(new ExerciseType());
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Exercise>()))
            .ReturnsAsync((Exercise e) => e);
            
        // Act - Call ONLY the method under test
        var result = await _service.CreateAsync(request);
        
        // Assert - Verify behavior and interactions
        Assert.True(result.IsSuccess);
        _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<Exercise>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockTypeRepository.Verify(r => r.GetByIdAsync(It.IsAny<ExerciseTypeId>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }
}
```

### Integration Tests (API.IntegrationTests Project)

**Definition**: Tests that verify how multiple components work together, including database interactions.

**Characteristics**:
- ✅ Use real database (TestContainers or In-Memory)
- ✅ Test full API endpoints through HTTP
- ✅ Verify complete workflows
- ✅ Include database transactions
- ✅ Slower execution (seconds)
- ✅ May require Docker

**What to Test**:
- Complete API endpoint workflows
- Database operations and transactions
- Entity Framework queries
- Data persistence and retrieval
- Authentication and authorization flows
- Complex business scenarios

**Example**:
```csharp
// Integration test with real database
public class ExerciseIntegrationTests : IClassFixture<SharedDatabaseTestFixture>
{
    [Fact]
    public async Task POST_Exercise_Creates_And_GET_Retrieves()
    {
        // Arrange - Prepare request
        var createRequest = new { name = "Push Up", ... };
        
        // Act - Call real API endpoint
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var getResponse = await _client.GetAsync($"/api/exercises/{id}");
        
        // Assert - Verify full workflow
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
```

## 🚨 Critical Rules

### Rule 1: Database = Integration Test

**ANY test that requires database access MUST be an integration test**:
- ❌ NEVER mock DbContext in unit tests
- ❌ NEVER use TestContainers in API.Tests project
- ❌ NEVER test repository implementations in unit tests
- ✅ ALWAYS move database tests to API.IntegrationTests

### Rule 2: Test Isolation

**Unit tests MUST be completely isolated**:
- Mock ALL external dependencies (EVERY single one!)
- Mock ALL internal dependencies except the class under test
- Use TestIds for consistent test data
- No file system access
- No network calls
- No database connections
- No real service implementations (even internal ones)

### Rule 3: BDD Tests Replace Integration Tests

**When migrating to BDD**:
- Old integration tests in API.Tests → BDD scenarios in API.IntegrationTests
- Use Gherkin syntax for business scenarios
- Cover all edge cases identified in original tests

## 📝 Migration Checklist

When you find a test in the wrong project:

### From API.Tests → API.IntegrationTests

1. **Identify database dependency**:
   ```csharp
   // Signs of integration test in wrong place:
   - Uses TestContainers
   - Creates real DbContext
   - Tests repository methods directly
   - Uses SeedDataBuilder with database
   ```

2. **Convert to BDD format**:
   ```gherkin
   Feature: Exercise Management
   
   Scenario: Create exercise with equipment
       Given I am authenticated as "PT-Tier"
       And equipment "Barbell" exists
       When I create an exercise with equipment "Barbell"
       Then the exercise should be created successfully
       And the exercise should have equipment "Barbell"
   ```

3. **Remove from API.Tests**:
   - Delete the old test file
   - Remove any TestContainers references
   - Update project dependencies

## 🏗️ Architecture Enforcement

### Project Structure

```
GetFitterGetBigger.API.Tests/
├── Controllers/          # Controller tests with mocked services
├── Services/            # Service tests with mocked repositories
├── Validators/          # Validator tests
├── TestBuilders/        # Test data builders (NO database)
│   └── TestIds.cs      # Static test IDs
└── NO Database Tests!   # All database tests in IntegrationTests

GetFitterGetBigger.API.IntegrationTests/
├── Features/           # BDD feature files
├── StepDefinitions/    # Step implementations
├── TestInfrastructure/ # Database fixtures
│   ├── SharedDatabaseTestFixture.cs
│   └── PostgreSqlApiTestFixture.cs
└── All Database Tests! # Repository, workflow, persistence tests
```

### Dependency Rules

**API.Tests project MUST NOT reference**:
- ❌ TestContainers.PostgreSQL
- ❌ Npgsql
- ❌ Microsoft.AspNetCore.Mvc.Testing (use mocked controllers)

**API.IntegrationTests project CAN reference**:
- ✅ TestContainers.PostgreSQL
- ✅ SpecFlow for BDD
- ✅ Microsoft.AspNetCore.Mvc.Testing

## 🔄 Test Creation Process

### During Feature Planning (feature-tasks.md)

1. **Add BDD Scenarios Section**:
   ```markdown
   ## BDD Test Scenarios
   
   ### Scenario 1: Happy Path
   Given [precondition]
   When [action]
   Then [expected result]
   
   ### Scenario 2: Validation Error
   Given [invalid data]
   When [action]
   Then [error response]
   ```

2. **Create Test Tasks**:
   ```markdown
   ### Testing Tasks
   - Task X.1: Write unit tests for service logic [ReadyToDevelop]
   - Task X.2: Create BDD feature file [ReadyToDevelop]
   - Task X.3: Implement BDD step definitions [ReadyToDevelop]
   - Task X.4: Migrate any existing integration tests [ReadyToDevelop]
   ```

### During Implementation

1. **Unit Tests First** (in API.Tests):
   - Test business logic with mocks
   - Verify service behavior
   - Test error handling

2. **Integration Tests Second** (in API.IntegrationTests):
   - Implement BDD scenarios
   - Test complete workflows
   - Verify database operations

## 📋 Quick Decision Guide

**Where does my test belong?**

| Test Scenario | Project | Test Type |
|--------------|---------|-----------|
| Service method with ALL dependencies mocked | API.Tests | Unit Test |
| Controller action with mocked service (NO real services) | API.Tests | Unit Test |
| DTO validation logic (pure functions only) | API.Tests | Unit Test |
| Testing a single method with everything else mocked | API.Tests | Unit Test |
| Complete CRUD workflow | API.IntegrationTests | BDD Integration |
| Repository implementation | API.IntegrationTests | Integration Test |
| Database query optimization | API.IntegrationTests | Integration Test |
| Authentication flow | API.IntegrationTests | BDD Integration |
| Complex business scenario | API.IntegrationTests | BDD Integration |
| Any test that uses real implementations | API.IntegrationTests | Integration Test |

## 🚀 Benefits of This Separation

1. **Faster Development**:
   - Unit tests run instantly
   - No Docker required for basic testing
   - Quick feedback loop

2. **Better Architecture**:
   - Forces proper mocking
   - Prevents tight coupling
   - Clear separation of concerns

3. **Improved Test Quality**:
   - BDD tests document requirements
   - Integration tests catch real issues
   - Unit tests ensure logic correctness

## 📚 Related Documentation

- [TESTING-QUICK-REFERENCE.md](TESTING-QUICK-REFERENCE.md) - Common test patterns
- [TestingGuidelines.md](TestingGuidelines.md) - Branch coverage guidelines
- [FEATURE_IMPLEMENTATION_PROCESS.md](FEATURE_IMPLEMENTATION_PROCESS.md) - Feature development process
- [MissingIntegrationTests.md](MissingIntegrationTests.md) - Tests to implement

## 🎯 Summary

**Remember**: 
- Database = Integration Test = API.IntegrationTests project
- EVERYTHING mocked = Unit Test = API.Tests project
- Real implementations = Integration Test = API.IntegrationTests project
- When in doubt, ask: "Am I testing ONLY this one method?"

**Unit Test Golden Rule**: 
If you're testing a method in class A, then:
- ✅ Class A is the ONLY real implementation
- ✅ EVERYTHING else is mocked (repositories, services, validators, loggers, etc.)
- ✅ You're verifying the behavior of that ONE method only
- ❌ NO real database, file system, or network access
- ❌ NO other real service implementations