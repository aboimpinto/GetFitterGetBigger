# Testing Standards - API Test Guidelines

**🎯 PURPOSE**: This document defines **MANDATORY** testing standards and patterns for the GetFitterGetBigger API to ensure reliable, maintainable tests.

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: Testing Rules - MUST be followed               │
├──────────────────────────────────────────────────────────────┤
│ 1. NEVER test error message content - only error codes      │
│ 2. Mock all dependencies in unit tests                      │
│ 3. Use TestIds for consistent test data                     │
│ 4. One assert per test preferred                            │
│ 5. Test behavior, not implementation                        │
└──────────────────────────────────────────────────────────────┘
```

## No Magic Strings in Tests

**NEVER test error message content - only test ServiceErrorCode:**

### ❌ VIOLATION - Testing Error Message Content

```csharp
[Fact]
public async Task ChangeStateAsync_WithEmptyStateId_ReturnsFailure()
{
    // Act
    var result = await _service.ChangeStateAsync(_testTemplateId, WorkoutStateId.Empty);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("GUID format", result.Errors.First()); // ← VIOLATION! Brittle test
}

[Fact]
public async Task CreateAsync_WithDuplicateName_ReturnsConflictError()
{
    // Setup
    _mockRepository.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
        .ReturnsAsync(true);
    
    // Act
    var result = await _service.CreateAsync(new CreateCommand { Name = "Test" });
    
    // Assert
    Assert.Contains("already exists", result.Errors.First()); // ← VIOLATION! Language-dependent
}
```

### ✅ CORRECT - Testing Error Codes Only

```csharp
[Fact]
public async Task ChangeStateAsync_WithEmptyStateId_ReturnsFailure()
{
    // Act
    var result = await _service.ChangeStateAsync(_testTemplateId, WorkoutStateId.Empty);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode); // ← CORRECT! Stable
}

[Fact]
public async Task CreateAsync_WithDuplicateName_ReturnsConflictError()
{
    // Setup
    _mockRepository.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
        .ReturnsAsync(true);
    
    // Act
    var result = await _service.CreateAsync(new CreateCommand { Name = "Test" });
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ServiceErrorCode.Conflict, result.PrimaryErrorCode); // ← CORRECT! Localization-ready
}
```

## Why This Is Critical

1. **Localization**: Error messages will change for different languages
2. **Maintenance**: Changing error text shouldn't break tests
3. **API Contract**: The error code IS the contract, not the message
4. **Test Stability**: Tests remain stable across message updates

## Unit Test Standards

### Test Structure

```csharp
public class EquipmentServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
    private readonly Mock<IEquipmentRepository> _mockRepository;
    private readonly Mock<ILogger<EquipmentService>> _mockLogger;
    private readonly EquipmentService _service;
    
    // Test IDs for consistency
    private readonly EquipmentId _testId = EquipmentId.New();
    private readonly string _testName = "Test Equipment";
    
    public EquipmentServiceTests()
    {
        // Setup mocks
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IEquipmentRepository>();
        _mockLogger = new Mock<ILogger<EquipmentService>>();
        
        // Wire up dependencies
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IEquipmentRepository>())
            .Returns(_mockRepository.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IEquipmentRepository>())
            .Returns(_mockRepository.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
        
        // Create service under test
        _service = new EquipmentService(
            _mockUnitOfWorkProvider.Object,
            _mockLogger.Object);
    }
}
```

### One Assert Per Test

```csharp
// ❌ BAD - Multiple asserts testing different things
[Fact]
public async Task GetByIdAsync_WhenFound_ReturnsSuccessWithData()
{
    // Arrange
    var entity = Equipment.Create(_testId, _testName);
    _mockRepository.Setup(x => x.GetByIdAsync(_testId))
        .ReturnsAsync(entity);
    
    // Act
    var result = await _service.GetByIdAsync(_testId);
    
    // Assert - Testing multiple things
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(_testId.ToString(), result.Data.Id);
    Assert.Equal(_testName, result.Data.Name);
    Assert.Empty(result.Errors);
}

// ✅ GOOD - Focused tests with single responsibility
[Fact]
public async Task GetByIdAsync_WhenFound_ReturnsSuccess()
{
    // Arrange
    var entity = Equipment.Create(_testId, _testName);
    _mockRepository.Setup(x => x.GetByIdAsync(_testId))
        .ReturnsAsync(entity);
    
    // Act
    var result = await _service.GetByIdAsync(_testId);
    
    // Assert
    Assert.True(result.IsSuccess);
}

[Fact]
public async Task GetByIdAsync_WhenFound_ReturnsCorrectData()
{
    // Arrange
    var entity = Equipment.Create(_testId, _testName);
    _mockRepository.Setup(x => x.GetByIdAsync(_testId))
        .ReturnsAsync(entity);
    
    // Act
    var result = await _service.GetByIdAsync(_testId);
    
    // Assert
    Assert.Equal(_testName, result.Data.Name);
}
```

## Testing What to Assert

### ✅ Test These

- `result.IsSuccess` - Operation success/failure
- `result.PrimaryErrorCode` - Specific error codes
- `result.Data` properties - Returned data structure
- Method call counts - Verify expected calls
- State changes - Entity state after operations

### ❌ Never Test These

- Error message content
- Log message content
- Private method behavior
- Implementation details
- Framework behavior

## Mock Verification Patterns

### Verify ReadOnly vs Writable UnitOfWork Usage

```csharp
[Fact]
public async Task GetByIdAsync_UsesReadOnlyUnitOfWork()
{
    // Arrange
    _mockRepository.Setup(x => x.GetByIdAsync(_testId))
        .ReturnsAsync(Equipment.Create(_testId, _testName));
    
    // Act
    await _service.GetByIdAsync(_testId);
    
    // Assert
    _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
    _mockUnitOfWorkProvider.Verify(x => x.CreateWritable(), Times.Never);
}

[Fact]
public async Task UpdateAsync_UsesWritableUnitOfWork()
{
    // Arrange
    var command = new UpdateEquipmentCommand { Name = "Updated" };
    _mockRepository.Setup(x => x.GetByIdAsync(_testId))
        .ReturnsAsync(Equipment.Create(_testId, _testName));
    
    // Act
    await _service.UpdateAsync(_testId, command);
    
    // Assert
    _mockUnitOfWorkProvider.Verify(x => x.CreateWritable(), Times.Once);
    _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
}
```

## Testing Empty Pattern

```csharp
[Fact]
public async Task GetByIdAsync_WhenNotFound_ReturnsEmptyWithNotFoundError()
{
    // Arrange
    _mockRepository.Setup(x => x.GetByIdAsync(_testId))
        .ReturnsAsync(Equipment.Empty);
    
    // Act
    var result = await _service.GetByIdAsync(_testId);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    Assert.True(result.Data.IsEmpty);  // Data is Empty, not null
}
```

## Testing Cache Integration

```csharp
[Fact]
public async Task GetAllActiveAsync_WhenCacheHit_DoesNotCallRepository()
{
    // Arrange
    var cachedData = new List<EquipmentDto> { /* test data */ };
    _mockCacheService
        .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
        .ReturnsAsync(CacheResult<IEnumerable<EquipmentDto>>.Hit(cachedData));
    
    // Act
    var result = await _service.GetAllActiveAsync();
    
    // Assert
    Assert.True(result.IsSuccess);
    _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
}

[Fact]
public async Task GetAllActiveAsync_WhenCacheMiss_CallsRepositoryAndCaches()
{
    // Arrange
    _mockCacheService
        .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
        .ReturnsAsync(CacheResult<IEnumerable<EquipmentDto>>.Miss());
    
    var entities = new List<Equipment> { Equipment.Create(_testId, _testName) };
    _mockRepository.Setup(x => x.GetAllActiveAsync())
        .ReturnsAsync(entities);
    
    // Act
    var result = await _service.GetAllActiveAsync();
    
    // Assert
    Assert.True(result.IsSuccess);
    _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
    _mockCacheService.Verify(x => x.SetAsync(
        It.IsAny<string>(), 
        It.IsAny<IEnumerable<EquipmentDto>>()), 
        Times.Once);
}
```

## Testing Query Counts and Performance Metrics

### ⚠️ CRITICAL: Non-Cumulative Query Counting Pattern

**PROBLEM**: When testing database query counts (for cache verification, performance testing, or optimization), cumulative counts make tests ambiguous and hard to understand.

#### The Problem with Cumulative Counts

```gherkin
# ❌ BAD - Ambiguous cumulative counting
Scenario: Calling get by ID twice should only hit database once
    Given I reset the database query counter
    When I send a GET request to "/api/items/123"
    Then the database query count should be 1
    When I send a GET request to "/api/items/123"  
    Then the database query count should be 1  # Still 1? Or cumulative 2?
```

This test is ambiguous:
- Does "1" mean no additional queries (cache working)?
- Does "1" mean we're asserting wrong and cache isn't working?
- Reader must mentally track cumulative state

#### The Solution: Reset and Recount Pattern

```gherkin
# ✅ GOOD - Clear, explicit counting with resets
Scenario: Calling get by ID twice should only hit database once
    Given I reset the database query counter
    # First call should hit the database
    When I send a GET request to "/api/items/123"
    Then the response status should be 200
    And the database query count should be 1
    # Reset counter to clearly show second call uses cache
    Given I reset the database query counter
    # Second call should use cache and NOT hit the database
    When I send a GET request to "/api/items/123"
    Then the response status should be 200
    And the database query count should be 0  # Crystal clear: NO database hit!
```

#### Benefits of Reset Pattern

1. **Unambiguous**: `0` clearly means "no database hit"
2. **Self-documenting**: Comments + reset make intent obvious
3. **Easier debugging**: Each phase is isolated
4. **Better readability**: No mental math required
5. **Failure clarity**: When it fails, you know exactly which call had the issue

#### Applying to Different Scenarios

##### Cache Testing
```gherkin
Scenario: GetAll then GetById should use separate caches
    Given I reset the database query counter
    When I send a GET request to "/api/items"
    Then the database query count should be 1
    Given I reset the database query counter
    When I send a GET request to "/api/items/123"
    Then the database query count should be 1  # Different cache key
    Given I reset the database query counter
    When I send a GET request to "/api/items/123"
    Then the database query count should be 0  # Now cached
```

##### Performance Testing
```gherkin
Scenario: Bulk operation should use optimized queries
    Given I reset the database query counter
    When I bulk create 10 items
    Then the database query count should be 2  # 1 for validation, 1 for insert
    Given I reset the database query counter
    When I request all 10 items
    Then the database query count should be 1  # Single query with includes
```

##### N+1 Query Detection
```gherkin
Scenario: Loading items with relationships should avoid N+1
    Given 5 items exist with categories
    And I reset the database query counter
    When I request all items with categories
    Then the database query count should be 1  # Not 6 (1+5 N+1 problem)
```

#### Implementation Pattern

```csharp
[Given(@"I reset the database query counter")]
public void GivenIResetTheDatabaseQueryCounter()
{
    var tracker = _fixture.Factory.GetQueryTracker();
    tracker?.Reset();
}

[Then(@"the database query count should be (.*)")]
public void ThenTheDatabaseQueryCountShouldBe(int expectedCount)
{
    var tracker = _fixture.Factory.GetQueryTracker();
    var actualCount = tracker.GetQueryCountForTable(_tableName);
    
    Assert.Equal(expectedCount, actualCount, 
        $"Expected {expectedCount} queries but found {actualCount}. " +
        $"Queries executed: {string.Join(", ", tracker.ExecutedQueries)}");
}
```

#### Key Principles

1. **Reset between measurements**: Always reset before measuring a new operation
2. **Comment your intent**: Explain what each measurement is verifying
3. **Use 0 for "no queries"**: Makes cache hits unmistakable
4. **Group related assertions**: Reset marks boundaries between test phases
5. **Be explicit**: Better to be verbose and clear than terse and ambiguous

#### When to Apply This Pattern

- ✅ Cache effectiveness testing
- ✅ Query optimization verification
- ✅ N+1 query prevention
- ✅ Batch operation efficiency
- ✅ Lazy loading verification
- ✅ Any performance-critical path testing

This pattern isn't limited to database queries - apply it to any cumulative metric:
- API call counts
- Event emissions
- Message queue operations
- File system operations
- Network requests

## Integration Test Standards

### BDD Approach with SpecFlow

```gherkin
Feature: Equipment Management
    As a Personal Trainer
    I want to manage equipment
    So that I can assign it to exercises

Scenario: Creating new equipment
    Given I am authenticated as a Personal Trainer
    When I create equipment with name "Barbell"
    Then the equipment should be created successfully
    And the equipment should appear in the list

Scenario: Duplicate equipment name
    Given equipment with name "Dumbbell" already exists
    When I try to create equipment with name "Dumbbell"
    Then I should receive a conflict error
    And the error code should be "Conflict"
```

### ⚠️ CRITICAL: Feature File Test Isolation

**PROBLEM**: Tests within the same `.feature` file share the same `IClassFixture` instance in xUnit/SpecFlow, which means they share:
- The same PostgreSQL test container
- The same WebApplicationFactory instance  
- The same database state (unless explicitly reset)

This can cause test failures when tests have conflicting requirements or when cache state from one test affects another.

#### Symptoms of Fixture Sharing Issues

1. **Tests pass individually but fail when run together**
2. **Cache tests showing 0 queries instead of expected 1**
3. **Database state from Test 1 affecting Test 3**
4. **Tests pass in different order but fail in CI/CD**

#### Solutions for Test Isolation

##### Solution 1: Separate Feature Files (RECOMMENDED)

When tests have conflicting requirements, separate them into different `.feature` files:

```gherkin
# File: DifficultyLevelCaching.feature
Feature: Difficulty Level Caching
  # Basic caching scenarios that work well together
  
  Scenario: Calling get all twice should only hit database once
  Scenario: Calling get by ID twice should only hit database once

# File: DifficultyLevelAdvancedCaching.feature  
Feature: Difficulty Level Advanced Caching
  # Advanced scenarios that need isolation
  
  Scenario: Different IDs should result in separate cache entries
  Scenario: Get by value should also use cache
```

Each `.feature` file gets its own:
- `IClassFixture<FeatureName.FixtureData>` instance
- PostgreSQL container
- WebApplicationFactory
- Complete isolation from other feature files

##### Solution 2: Test Collections (LIMITED SUCCESS)

Adding `@collection:Serial` tags does NOT provide fixture isolation:

```gherkin
# ⚠️ This doesn't isolate fixtures - tests still share resources
@collection:Serial
Scenario: Test that needs isolation
```

Test collections only control parallel execution, not fixture sharing within a feature file.

##### Solution 3: Explicit State Reset (COMPLEX)

Reset state between tests in the same feature:
- Clear memory cache using reflection
- Reset database to known state
- Clear any singleton services

This is complex and error-prone - prefer Solution 1.

#### Real-World Example

**Problem Encountered**: DifficultyLevelCaching tests failing after migration from PureReferenceService
- Tests 1 & 2: Passed when run together
- Tests 1 & 3: Failed when run together (Test 1 failed)
- Tests 3 & 4: Always problematic

**Investigation Process**:
1. Added extensive logging - didn't reveal issue
2. Changed service lifetimes (Singleton → Scoped) - didn't help
3. Added test collections - didn't provide isolation
4. Verified BodyParts tests passed - proved migration was correct

**Root Cause**: All 4 tests in same feature file sharing fixtures

**Solution Applied**: Moved Tests 3 & 4 to `DifficultyLevelAdvancedCaching.feature`

**Result**: All tests passed with proper isolation

### Integration Test Structure

```csharp
public class EquipmentIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public EquipmentIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateEquipment_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateEquipmentDto 
        { 
            Name = "Test Equipment",
            Description = "Test Description"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/equipment", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<EquipmentDto>();
        Assert.NotNull(content);
        Assert.Equal(request.Name, content.Name);
    }
}
```

## Error Message Organization

```csharp
// ✅ CORRECT - Centralized error messages
public static class EquipmentErrorMessages
{
    public static class Validation
    {
        public const string RequestCannotBeNull = "Request cannot be null";
        public const string NameCannotBeEmpty = "Equipment name cannot be empty";
        public const string NameTooLong = "Equipment name exceeds maximum length";
    }
    
    public const string InvalidIdFormat = "Invalid EquipmentId format";
    public const string NotFound = "Equipment not found";
    public const string AlreadyExists = "Equipment with this name already exists";
}

// Service uses constants
return ServiceResult<EquipmentDto>.Failure(
    EquipmentDto.Empty,
    ServiceError.NotFound(EquipmentErrorMessages.NotFound));
```

## Testing Rules Summary

- ✅ Test `ServiceErrorCode` values
- ✅ Test `result.IsSuccess` boolean
- ✅ Test returned data structure
- ✅ Mock all external dependencies
- ✅ Use TestIds for consistency
- ❌ NEVER test error message content
- ❌ NEVER use `Assert.Contains()` on error messages
- ❌ NEVER depend on specific error text
- ❌ NEVER test private methods directly
- ❌ NEVER test framework behavior

## Test Naming Convention

```csharp
// Pattern: MethodName_Scenario_ExpectedResult

[Fact]
public async Task GetByIdAsync_WhenEntityExists_ReturnsSuccess() { }

[Fact]
public async Task GetByIdAsync_WhenEntityNotFound_ReturnsNotFoundError() { }

[Fact]
public async Task CreateAsync_WithDuplicateName_ReturnsConflictError() { }

[Fact]
public async Task UpdateAsync_WithValidData_UpdatesAndReturnsSuccess() { }
```

## Key Principles

1. **Test Behavior, Not Implementation**: Focus on what, not how
2. **Isolation**: Each test should be independent
3. **Clarity**: Test names should clearly describe the scenario
4. **Stability**: Tests should not break with refactoring
5. **Speed**: Unit tests should run fast (mock external dependencies)

## Related Documentation

- `/memory-bank/PracticalGuides/TestingQuickReference.md` - Common test failures and solutions
- `/memory-bank/PracticalGuides/CommonTestingErrorsAndSolutions.md` - Detailed testing patterns
- `/memory-bank/Overview/TestingGuidelines.md` - Overall testing strategy
- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards