# Unit Test Specifications for Workout Reference Data

This document defines the unit test specifications for the Workout Reference Data feature, following the established testing patterns and BDD approach used in the GetFitterGetBigger project.

## Test Coverage Goals

- **Target Coverage**: 95%+ code coverage
- **Critical Path Coverage**: 100% for business logic
- **Edge Case Coverage**: All validation scenarios and error conditions
- **Performance Tests**: Response time and caching effectiveness

## API Layer Unit Tests

### WorkoutObjective Controller Tests

#### Test Suite: WorkoutObjectivesController
```gherkin
Feature: Workout Objectives API
  As a fitness application
  I want to retrieve workout objective reference data
  So that trainers can select appropriate training goals

  Background:
    Given the workout objectives reference table is seeded
    And I have valid authentication credentials

  Scenario: Get all active workout objectives
    When I send a GET request to "/api/workout-objectives"
    Then the response status should be 200
    And the response should contain 7 workout objectives
    And each objective should have the required fields
    And objectives should be ordered by displayOrder

  Scenario: Get workout objectives including inactive
    Given there is 1 inactive workout objective
    When I send a GET request to "/api/workout-objectives?includeInactive=true"
    Then the response status should be 200
    And the response should contain 8 workout objectives

  Scenario: Get specific workout objective by ID
    Given a workout objective with ID "strength-objective-id"
    When I send a GET request to "/api/workout-objectives/strength-objective-id"
    Then the response status should be 200
    And the response should contain objective "Muscular Strength"
    And the description should contain programming guidance

  Scenario: Get non-existent workout objective
    When I send a GET request to "/api/workout-objectives/invalid-id"
    Then the response status should be 404
    And the error message should indicate objective not found

  Scenario: Unauthorized access to workout objectives
    Given I have no authentication credentials
    When I send a GET request to "/api/workout-objectives"
    Then the response status should be 401
```

#### Controller Unit Tests
```csharp
// Test class structure following project patterns
public class WorkoutObjectivesControllerTests
{
    [Fact]
    public async Task GetWorkoutObjectives_ShouldReturnAllActiveObjectives()
    
    [Fact]
    public async Task GetWorkoutObjectives_WithIncludeInactive_ShouldReturnAllObjectives()
    
    [Fact]
    public async Task GetWorkoutObjectiveById_WithValidId_ShouldReturnObjective()
    
    [Fact]
    public async Task GetWorkoutObjectiveById_WithInvalidId_ShouldReturn404()
    
    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    public async Task GetWorkoutObjectiveById_WithInvalidGuid_ShouldReturnBadRequest(string id)
}
```

### WorkoutCategory Controller Tests

#### Test Suite: WorkoutCategoriesController
```gherkin
Feature: Workout Categories API
  As a fitness application
  I want to retrieve workout category reference data
  So that trainers can categorize workout templates

  Background:
    Given the workout categories reference table is seeded
    And I have valid authentication credentials

  Scenario: Get all workout categories with visual data
    When I send a GET request to "/api/workout-categories"
    Then the response status should be 200
    And the response should contain 8 workout categories
    And each category should have icon and color information
    And categories should include primary muscle groups

  Scenario: Category visual data validation
    When I retrieve all workout categories
    Then each category should have a unique icon
    And each category should have a unique color
    And colors should be valid hex codes
    And icons should reference existing UI assets
```

#### Controller Unit Tests
```csharp
public class WorkoutCategoriesControllerTests
{
    [Fact]
    public async Task GetWorkoutCategories_ShouldReturnAllActiveCategories()
    
    [Fact]
    public async Task GetWorkoutCategories_ShouldIncludeVisualProperties()
    
    [Fact]
    public async Task GetWorkoutCategoryById_ShouldReturnCategoryWithMuscleGroups()
    
    [Fact]
    public async Task ValidateUniqueConstraints_ShouldEnforceIconUniqueness()
    
    [Fact]
    public async Task ValidateUniqueConstraints_ShouldEnforceColorUniqueness()
}
```

### ExecutionProtocol Controller Tests

#### Test Suite: ExecutionProtocolsController
```gherkin
Feature: Execution Protocols API
  As a fitness application
  I want to retrieve execution protocol reference data
  So that trainers can prescribe appropriate training methodologies

  Background:
    Given the execution protocols reference table is seeded
    And I have valid authentication credentials

  Scenario: Get all execution protocols with technical details
    When I send a GET request to "/api/execution-protocols"
    Then the response status should be 200
    And the response should contain 8 execution protocols
    And each protocol should have code and technical characteristics
    And protocols should include implementation instructions

  Scenario: Get execution protocol by code
    When I send a GET request to "/api/execution-protocols/by-code/STANDARD"
    Then the response status should be 200
    And the response should contain protocol "Standard"
    And the code should be "STANDARD"

  Scenario: Protocol code validation
    When I retrieve all execution protocols
    Then each protocol code should be unique
    And codes should follow UPPERCASE_UNDERSCORE format
    And all codes should be valid programmatic identifiers
```

#### Controller Unit Tests
```csharp
public class ExecutionProtocolsControllerTests
{
    [Fact]
    public async Task GetExecutionProtocols_ShouldReturnAllActiveProtocols()
    
    [Fact]
    public async Task GetExecutionProtocolByCode_WithValidCode_ShouldReturnProtocol()
    
    [Fact]
    public async Task GetExecutionProtocolByCode_WithInvalidCode_ShouldReturn404()
    
    [Theory]
    [InlineData("STANDARD")]
    [InlineData("AMRAP")]
    [InlineData("EMOM")]
    public async Task GetExecutionProtocolByCode_WithKnownCodes_ShouldReturnExpectedProtocol(string code)
    
    [Fact]
    public async Task ValidateCodeFormat_ShouldEnforceUppercaseUnderscore()
}
```

## Service Layer Unit Tests

### WorkoutReferenceDataService Tests

#### Test Suite: Reference Data Business Logic
```gherkin
Feature: Workout Reference Data Service
  As a business service
  I want to provide validated reference data operations
  So that the API can serve consistent and accurate information

  Scenario: Validate display order uniqueness
    Given multiple reference items with the same displayOrder
    When I attempt to save the reference data
    Then a validation error should be thrown
    And the error should specify displayOrder conflict

  Scenario: Validate required field constraints
    Given a reference item with missing required fields
    When I attempt to save the reference item
    Then a validation error should be thrown
    And the error should list all missing fields

  Scenario: Cache invalidation on data updates
    Given reference data is cached
    When reference data is updated
    Then the cache should be invalidated
    And subsequent requests should return fresh data
```

#### Service Unit Tests
```csharp
public class WorkoutReferenceDataServiceTests
{
    [Fact]
    public async Task GetWorkoutObjectives_ShouldReturnCachedData_WhenAvailable()
    
    [Fact]
    public async Task GetWorkoutObjectives_ShouldRefreshCache_WhenExpired()
    
    [Fact]
    public async Task ValidateDisplayOrder_ShouldThrowException_WhenDuplicate()
    
    [Fact]
    public async Task ValidateValue_ShouldThrowException_WhenDuplicate()
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task ValidateRequiredFields_ShouldThrowException_WhenInvalid(string value)
}
```

## Repository Layer Unit Tests

### Reference Data Repository Tests

#### Test Suite: Data Access Operations
```gherkin
Feature: Reference Data Repository
  As a data access layer
  I want to perform CRUD operations on reference tables
  So that services can access reference data efficiently

  Scenario: Query performance optimization
    Given a large dataset of reference items
    When I query for active items
    Then the query should use appropriate indexes
    And response time should be under 100ms

  Scenario: Filtering by active status
    Given reference items with mixed active status
    When I query for active items only
    Then only active items should be returned
    And inactive items should be excluded

  Scenario: Ordering by display order
    Given reference items with various display orders
    When I query all items
    Then items should be ordered by displayOrder ascending
    And the order should be consistent across queries
```

#### Repository Unit Tests
```csharp
public class WorkoutReferenceDataRepositoryTests
{
    [Fact]
    public async Task GetActiveWorkoutObjectives_ShouldReturnOnlyActiveItems()
    
    [Fact]
    public async Task GetWorkoutObjectives_ShouldOrderByDisplayOrder()
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectItem_WhenExists()
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    
    [Fact]
    public async Task GetExecutionProtocolByCode_ShouldReturnCorrectProtocol()
    
    [Fact]
    public async Task ValidateUniqueConstraints_ShouldThrowException_OnViolation()
}
```

## Validation Layer Unit Tests

### Reference Data Validation Tests

#### Test Suite: Business Rule Validation
```gherkin
Feature: Reference Data Validation
  As a validation service
  I want to enforce business rules on reference data
  So that data integrity is maintained

  Scenario: Workout objective description validation
    Given a workout objective with description under 10 characters
    When I validate the objective
    Then a validation error should be thrown
    And the error should specify minimum description length

  Scenario: Execution protocol code format validation
    Given an execution protocol with lowercase code
    When I validate the protocol
    Then a validation error should be thrown
    And the error should specify uppercase requirement

  Scenario: Category color validation
    Given a workout category with invalid hex color
    When I validate the category
    Then a validation error should be thrown
    And the error should specify valid color format
```

#### Validation Unit Tests
```csharp
public class ReferenceDataValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData("Short")]
    [InlineData("A")]
    public async Task ValidateDescription_ShouldFail_WhenTooShort(string description)
    
    [Theory]
    [InlineData("#FF6B35")]
    [InlineData("#4ECDC4")]
    [InlineData("#45B7D1")]
    public async Task ValidateColor_ShouldPass_WhenValidHex(string color)
    
    [Theory]
    [InlineData("FF6B35")]
    [InlineData("#GG6B35")]
    [InlineData("invalid")]
    public async Task ValidateColor_ShouldFail_WhenInvalidHex(string color)
    
    [Theory]
    [InlineData("STANDARD")]
    [InlineData("AMRAP")]
    [InlineData("FOR_TIME")]
    public async Task ValidateCode_ShouldPass_WhenUppercaseUnderscore(string code)
    
    [Theory]
    [InlineData("standard")]
    [InlineData("For-Time")]
    [InlineData("am rap")]
    public async Task ValidateCode_ShouldFail_WhenInvalidFormat(string code)
}
```

## Caching Layer Unit Tests

### Cache Service Tests

#### Test Suite: Reference Data Caching
```gherkin
Feature: Reference Data Caching
  As a caching service
  I want to efficiently cache reference data
  So that API performance is optimized

  Scenario: Cache hit performance
    Given reference data is cached
    When I request the same data
    Then the response should come from cache
    And response time should be under 10ms

  Scenario: Cache miss handling
    Given reference data is not cached
    When I request the data
    Then the data should be fetched from database
    And the result should be cached for future requests

  Scenario: Cache expiration
    Given reference data is cached with 1-hour TTL
    When the cache expires
    Then subsequent requests should refresh the cache
    And new data should be cached again
```

#### Cache Unit Tests
```csharp
public class ReferenceDataCacheTests
{
    [Fact]
    public async Task GetWorkoutObjectives_ShouldReturnFromCache_WhenAvailable()
    
    [Fact]
    public async Task GetWorkoutObjectives_ShouldUpdateCache_WhenMiss()
    
    [Fact]
    public async Task InvalidateCache_ShouldClearAllReferenceData()
    
    [Fact]
    public async Task CacheExpiration_ShouldRefreshAutomatically()
    
    [Theory]
    [InlineData("workout-objectives")]
    [InlineData("workout-categories")]
    [InlineData("execution-protocols")]
    public async Task CacheKeys_ShouldBeConsistent_AcrossServices(string cacheKey)
}
```

## Model Validation Unit Tests

### Entity Validation Tests

#### Test Suite: Entity Property Validation
```gherkin
Feature: Entity Property Validation
  As an entity framework
  I want to validate entity properties
  So that database constraints are enforced

  Scenario: Required field validation
    Given an entity with null required field
    When I attempt to save the entity
    Then a validation exception should be thrown
    And the exception should specify the required field

  Scenario: String length validation
    Given an entity with field exceeding maximum length
    When I attempt to save the entity
    Then a validation exception should be thrown
    And the exception should specify the maximum length

  Scenario: Unique constraint validation
    Given two entities with the same unique field value
    When I attempt to save both entities
    Then a unique constraint exception should be thrown
```

#### Entity Unit Tests
```csharp
public class ReferenceDataEntityTests
{
    [Fact]
    public void WorkoutObjective_ShouldRequire_Value()
    
    [Fact]
    public void WorkoutObjective_ShouldRequire_Description()
    
    [Fact]
    public void WorkoutCategory_ShouldRequire_Icon()
    
    [Fact]
    public void WorkoutCategory_ShouldRequire_Color()
    
    [Fact]
    public void ExecutionProtocol_ShouldRequire_Code()
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DisplayOrder_ShouldReject_NonPositiveValues(int displayOrder)
    
    [Fact]
    public void Value_ShouldEnforce_MaxLength()
    
    [Fact]
    public void Description_ShouldEnforce_MaxLength()
}
```

## Performance Unit Tests

### Performance Validation Tests

#### Test Suite: Performance Requirements
```gherkin
Feature: Performance Requirements
  As a performance monitoring system
  I want to validate response times
  So that SLA requirements are met

  Scenario: API response time validation
    Given a reference data request
    When I measure the response time
    Then the response should be under 200ms for uncached requests
    And under 50ms for cached requests

  Scenario: Memory usage validation
    Given multiple concurrent reference data requests
    When I monitor memory usage
    Then memory usage should remain stable
    And there should be no memory leaks

  Scenario: Database query optimization
    Given reference data queries
    When I analyze query execution plans
    Then queries should use appropriate indexes
    And query complexity should be minimal
```

#### Performance Unit Tests
```csharp
public class ReferenceDataPerformanceTests
{
    [Fact]
    public async Task GetWorkoutObjectives_ShouldComplete_Under200ms()
    
    [Fact]
    public async Task CachedRequests_ShouldComplete_Under50ms()
    
    [Fact]
    public async Task ConcurrentRequests_ShouldNotDegradePerformance()
    
    [Fact]
    public async Task DatabaseQueries_ShouldUseIndexes()
    
    [Fact]
    public async Task MemoryUsage_ShouldRemainStable_UnderLoad()
}
```

## Error Handling Unit Tests

### Error Scenario Tests

#### Test Suite: Error Handling
```gherkin
Feature: Error Handling
  As an error handling system
  I want to provide meaningful error responses
  So that clients can handle errors appropriately

  Scenario: Database connection failure
    Given the database is unavailable
    When I request reference data
    Then a 500 error should be returned
    And the error message should indicate service unavailability

  Scenario: Invalid GUID format
    Given a malformed GUID in the request
    When I request a specific reference item
    Then a 400 error should be returned
    And the error message should specify invalid ID format

  Scenario: Authentication failure
    Given invalid authentication credentials
    When I request reference data
    Then a 401 error should be returned
    And the error message should indicate authentication required
```

#### Error Handling Unit Tests
```csharp
public class ReferenceDataErrorHandlingTests
{
    [Fact]
    public async Task DatabaseUnavailable_ShouldReturn500()
    
    [Fact]
    public async Task InvalidGuid_ShouldReturn400()
    
    [Fact]
    public async Task NoAuthentication_ShouldReturn401()
    
    [Fact]
    public async Task InsufficientPermissions_ShouldReturn403()
    
    [Theory]
    [InlineData("malformed-guid")]
    [InlineData("")]
    [InlineData("not-a-guid")]
    public async Task InvalidIdFormats_ShouldReturnBadRequest(string invalidId)
}
```

## Test Data Setup

### Test Data Builders
```csharp
public class WorkoutObjectiveTestDataBuilder
{
    public static WorkoutObjective CreateValidObjective()
    public static List<WorkoutObjective> CreateStandardObjectives()
    public static WorkoutObjective CreateObjectiveWithId(string id)
}

public class WorkoutCategoryTestDataBuilder
{
    public static WorkoutCategory CreateValidCategory()
    public static List<WorkoutCategory> CreateStandardCategories()
    public static WorkoutCategory CreateCategoryWithVisualData()
}

public class ExecutionProtocolTestDataBuilder
{
    public static ExecutionProtocol CreateValidProtocol()
    public static List<ExecutionProtocol> CreateStandardProtocols()
    public static ExecutionProtocol CreateProtocolWithCode(string code)
}
```

### Test Utilities
```csharp
public static class ReferenceDataTestHelpers
{
    public static async Task<TestServer> CreateTestServer()
    public static void SeedReferenceData(DbContext context)
    public static void AssertValidReferenceDataStructure(object referenceData)
    public static void AssertPerformanceRequirements(TimeSpan elapsed)
}
```

## Continuous Integration

### Test Execution Strategy
- **Fast Tests**: Unit tests run on every commit
- **Integration Tests**: Run on pull requests
- **Performance Tests**: Run nightly
- **Coverage Reports**: Generated on all test runs

### Quality Gates
- **Minimum Coverage**: 95% line coverage
- **Performance Threshold**: All tests under 200ms
- **Zero Failures**: All tests must pass
- **Code Quality**: No critical code issues