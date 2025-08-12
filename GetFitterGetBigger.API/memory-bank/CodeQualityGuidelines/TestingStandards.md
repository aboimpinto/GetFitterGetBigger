# Testing Standards - API Test Guidelines

**🎯 PURPOSE**: This document defines **MANDATORY** testing standards and patterns for the GetFitterGetBigger API to ensure reliable, maintainable tests.

## 🚨 CRITICAL Rules - CODE REVIEW BLOCKERS

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: Testing Rules - PR WILL BE REJECTED           │
├──────────────────────────────────────────────────────────────┤
│ 1. NO MAGIC STRINGS - Extract ALL literals to constants     │
│ 2. TEST NAMES MUST EXPRESS INTENT - Method_Scenario_Outcome │
│ 3. NEVER test error message content - only error codes      │
│ 4. Constants MUST connect arrange to assert phases          │
│ 5. Mock all dependencies in unit tests                      │
│ 6. Remove unused test data - only test what you assert      │
│ 7. Test behavior, not implementation                        │
└──────────────────────────────────────────────────────────────┘
```

**⚠️ CODE REVIEW REQUIREMENT**: Any PR containing tests with magic strings or unclear test names will be marked as **"NEEDS CHANGES"** and must be fixed before merge.

## No Magic Strings in Tests - Complete Guide

### Rule 1: Extract ALL String Literals to Constants

**NEVER use magic strings in tests**. Every string literal should be extracted to a constant variable that connects the arrange and assert phases.

#### ❌ VIOLATION - Magic Strings Everywhere
```csharp
[Fact]
public async Task GetPagedAsync_ReturnsPagedResponse()  // Vague name!
{
    var filterParams = new ExerciseFilterParams
    {
        Name = "Press"  // Magic string!
    };
    
    var exercises = new List<Exercise>
    {
        ExerciseBuilder.AWorkoutExercise()
            .WithName("Bench Press")  // Magic string!
            .Build(),
        ExerciseBuilder.AWorkoutExercise()
            .WithName("Overhead Press")  // Magic string!
            .Build()
    };
    
    // ... test execution ...
    
    result.Data.Items.Should().OnlyContain(dto => dto.Name.Contains("Press")); // DIFFERENT magic string!
}
```

**What's wrong?**
- If someone changes `Name = "Press"` to `Name = "Squat"` for testing a new feature, the assertion still checks for "Press"
- The test becomes disconnected and fails mysteriously
- Takes time to debug why it's failing
- The test name doesn't express what's actually being tested

#### ✅ CORRECT - Constants That Connect
```csharp
[Fact]
public async Task GetPagedAsync_WithNameFilter_ReturnsOnlyMatchingExercises()  // Clear intent!
{
    // Arrange
    const string searchTerm = "Press";
    const string matchingExercise1 = "Bench Press";
    const string matchingExercise2 = "Overhead Press";
    
    var filterParams = new ExerciseFilterParams
    {
        Name = searchTerm  // Using constant
    };
    
    var exercises = new List<Exercise>
    {
        ExerciseBuilder.AWorkoutExercise()
            .WithName(matchingExercise1)  // Using constant
            .Build(),
        ExerciseBuilder.AWorkoutExercise()
            .WithName(matchingExercise2)  // Using constant
            .Build()
    };
    
    // ... test execution ...
    
    // Assert uses the SAME constant - automatically stays in sync!
    result.Data.Items.Should().OnlyContain(dto => dto.Name.Contains(searchTerm));
}
```

### Rule 2: Test Names Must Express Clear Intent

Test names should follow the pattern: **Method_Scenario_ExpectedOutcome**

#### ❌ VIOLATION - Misleading or Vague Test Names
```csharp
[Fact]
public async Task GetPagedAsync_ReturnsPagedResponse()  
// What scenario? With filters? Without? What's being tested?

[Fact]
public async Task CreateAsync_WithValidRequest_CreatesExercise()  
// What makes the request valid? What's the specific scenario?

[Fact]
public async Task DeleteAsync_WithInvalidId_ReturnsFailure()  
// Invalid how? Empty? Non-existent? Wrong format?
```

#### ✅ CORRECT - Clear, Specific Test Names
```csharp
[Fact]
public async Task GetPagedAsync_WithNameFilter_ReturnsOnlyMatchingExercises()
// Clear: testing name filtering functionality

[Fact]
public async Task CreateAsync_WithNewExerciseName_CreatesAndReturnsExercise()
// Clear: testing creation with a new (non-duplicate) name

[Fact]
public async Task DeleteAsync_WithNonExistentExercise_ReturnsNotFoundError()
// Clear: testing deletion when exercise doesn't exist

[Fact]
public async Task GetByIdAsync_WithEmptyId_ReturnsEmptyDto()
// Clear: testing behavior with empty ID
```

### Rule 3: Link Test Data to Assertions

Every piece of test data that's being verified should use the same constant in both arrange and assert phases.

#### ❌ VIOLATION - Disconnected Data
```csharp
[Fact]
public async Task CreateAsync_WithValidRequest_CreatesExercise()
{
    var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
        .WithName("New Exercise")  // Magic string #1
        .Build();

    var createdExercise = ExerciseBuilder.AWorkoutExercise()
        .WithName("New Exercise")  // Magic string #2 (duplicate)
        .Build();
    
    // ... test execution ...
    
    result.Data.Name.Should().Be("New Exercise");  // Magic string #3!
}
```

**Problems:**
- Three separate magic strings that happen to match
- Change one and the test breaks mysteriously
- No clear connection between setup and assertion

#### ✅ CORRECT - Connected Data with Single Source of Truth
```csharp
[Fact]
public async Task CreateAsync_WithNewExerciseName_CreatesAndReturnsExercise()
{
    // Single source of truth
    const string exerciseName = "Squat";
    
    var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
        .WithName(exerciseName)  // Same constant
        .Build();

    var createdExercise = ExerciseBuilder.AWorkoutExercise()
        .WithName(exerciseName)  // Same constant
        .Build();
    
    // ... test execution ...
    
    result.Data.Name.Should().Be(exerciseName);  // Same constant in assertion
}
```

### Rule 4: Don't Test What You Don't Assert

Remove test data that isn't being verified - it adds noise and confusion.

#### ❌ VIOLATION - Unused Test Data
```csharp
[Fact]
public async Task DeleteAsync_WithValidId_PerformsSoftDelete()
{
    var exercise = ExerciseBuilder.AWorkoutExercise()
        .WithId(exerciseId)
        .WithName("To Delete")  // Never verified!
        .WithDescription("Some description")  // Never verified!
        .WithDifficultyLevel("Beginner")  // Never verified!
        .Build();
    
    // ... test execution ...
    
    result.IsSuccess.Should().BeTrue();  // Only checking success
}
```

#### ✅ CORRECT - Only Necessary Data
```csharp
[Fact]
public async Task DeleteAsync_WithExistingExercise_PerformsSoftDelete()
{
    var exercise = ExerciseBuilder.AWorkoutExercise()
        .WithId(exerciseId)  // Only what's needed
        .Build();
    
    // ... test execution ...
    
    result.IsSuccess.Should().BeTrue();
}
```

### Rule 5: Use 'because' Clauses for Non-Obvious Assertions

When the reason for an assertion might not be immediately clear, add a `because` clause.

#### ✅ GOOD - Explaining Why
```csharp
result.IsSuccess.Should().BeFalse(because: "REST exercises cannot have weight types");

result.Data.IsEmpty.Should().BeTrue(because: "invalid ID should result in Empty ID which returns Empty DTO");

result.IsSuccess.Should().BeTrue(because: "weight type is optional for non-REST exercises");
```

### The "Future Developer" Test

Before committing any test, ask yourself:
1. **"If someone changes this test data 6 months from now, will the test still make sense?"**
2. **"If this test fails at 3 AM, will the on-call developer understand what broke?"**
3. **"Can a new team member understand what this test validates without asking?"**

### Why This Matters

#### The Copy-Paste-Forget Anti-Pattern
Developers copy tests and forget to update all magic strings, leading to tests that pass for the wrong reasons.

#### The Works-on-My-Machine Anti-Pattern  
Magic strings that happen to work with current data but break when someone innocently changes test data.

#### The Mystery Failure Anti-Pattern
Test fails and it takes 10+ minutes to figure out why because the assertion checks a hardcoded value disconnected from the test setup.

### Quick Checklist for Every Test

Before committing any test, verify:

- [ ] **No magic strings** - Every string literal is in a const variable
- [ ] **Test name describes intent** - Method_Scenario_Outcome pattern
- [ ] **Constants connect arrange to assert** - Same variable used in both
- [ ] **No unused test data** - Only set up what you assert on
- [ ] **Clear test names** - Would a new developer understand?
- [ ] **Because clauses where helpful** - Explain non-obvious assertions

### The Golden Rule

> **"Write tests as if the person who maintains them is a violent psychopath who knows where you live."**

Or more professionally:

> **"Write tests that are so clear and robust that they never need explanation, never break from innocent changes, and always clearly indicate what went wrong when they fail."**

### Rule 6: Never Test Error Message Content

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

## 🔍 Code Review Checklist for Tests

### MUST REJECT PR If Any Test Has:

- [ ] **Magic strings** - Any string literal not in a const variable
- [ ] **Vague test names** - Names that don't describe the specific scenario
- [ ] **Disconnected assertions** - Assertions using different values than setup
- [ ] **Error message testing** - Using `Assert.Contains()` on error messages
- [ ] **Unused test data** - Setting up data that's never asserted
- [ ] **Missing intent** - Test names like `Test1`, `TestMethod`, `WorksCorrectly`

### Example Code Review Comments:

```markdown
❌ NEEDS CHANGES: This test contains magic strings. Please extract all string 
literals to const variables that connect the arrange and assert phases.

❌ NEEDS CHANGES: Test name "GetPagedAsync_ReturnsPagedResponse" doesn't express 
what scenario is being tested. Please rename to describe the specific case 
(e.g., "GetPagedAsync_WithNameFilter_ReturnsOnlyMatchingExercises").

❌ NEEDS CHANGES: The assertion checks for "Press" but the filter uses a different 
string. Please use the same constant for both to prevent disconnected test failures.

❌ NEEDS CHANGES: This test is checking error message content. Please test only 
the ServiceErrorCode instead, as messages may change or be localized.

❌ NEEDS CHANGES: The test sets up Name, Description, and DifficultyLevel but 
only asserts on IsSuccess. Please remove unused test data to improve clarity.
```

### When to Approve:

✅ **APPROVE** only when ALL tests:
- Use constants for every string value
- Have clear, descriptive names following Method_Scenario_Outcome pattern
- Use the same constants in arrange and assert
- Test only error codes, not messages
- Set up only data that's being verified
- Include `because` clauses for non-obvious assertions

## Related Documentation

- `/memory-bank/PracticalGuides/TestingQuickReference.md` - Common test failures and solutions
- `/memory-bank/PracticalGuides/CommonTestingErrorsAndSolutions.md` - Detailed testing patterns
- `/memory-bank/Overview/TestingGuidelines.md` - Overall testing strategy
- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards