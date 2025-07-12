# Integration Test Specifications for Workout Reference Data

This document defines the integration test specifications for the Workout Reference Data feature, following the BDD (Behavior-Driven Development) approach established in the GetFitterGetBigger project using SpecFlow.

## Test Framework Setup

### Testing Infrastructure
- **Framework**: SpecFlow 4.0.31-beta
- **Test Container**: PostgreSQL for isolated database testing
- **Authentication**: JWT token-based testing
- **Test Data**: Comprehensive reference data seeding
- **Assertion Library**: FluentAssertions for readable test assertions

### Test Configuration
```csharp
[Binding]
public class WorkoutReferenceDataTestContext : IDisposable
{
    public TestServer TestServer { get; private set; }
    public HttpClient Client { get; private set; }
    public PostgreSqlContainer DatabaseContainer { get; private set; }
    
    public async Task InitializeAsync()
    {
        // Setup test container with PostgreSQL
        // Configure test server with real database
        // Seed reference data
    }
}
```

## Feature: Workout Objectives Integration

### Background Setup
```gherkin
Feature: Workout Objectives Integration Tests
  As a fitness application
  I want to provide workout objective reference data through API endpoints
  So that Personal Trainers can access training goal information

  Background:
    Given the application is running with a test database
    And the workout objectives reference table is seeded with standard data
    And I have a valid JWT token with Free-Tier claims
```

### Scenario: Complete Workout Objectives Workflow
```gherkin
  Scenario: Personal Trainer accesses workout objectives for template creation
    Given I am authenticated as a Personal Trainer
    When I request all workout objectives
    Then I should receive 7 standard workout objectives
    And each objective should contain complete programming guidance
    And the objectives should be ordered by display order
    And the response should include caching headers

    When I request the "Muscular Strength" objective by ID
    Then I should receive detailed strength training programming information
    And the description should include rep ranges, sets, and intensity guidance
    And the response should be properly formatted JSON

    When I request a non-existent objective ID
    Then I should receive a 404 Not Found response
    And the error message should be user-friendly
```

### Scenario: Workout Objectives Caching Behavior
```gherkin
  Scenario: Workout objectives caching improves performance
    Given I am authenticated as a Personal Trainer
    When I request all workout objectives for the first time
    Then the response time should be under 200ms
    And the response should include cache control headers

    When I request the same workout objectives again
    Then the response should be returned from cache
    And the response time should be under 50ms
    And the ETag should match the previous response
```

### Scenario: Workout Objectives Authentication and Authorization
```gherkin
  Scenario: Authentication is required for workout objectives access
    Given I have no authentication token
    When I request all workout objectives
    Then I should receive a 401 Unauthorized response
    And the error should indicate authentication is required

  Scenario: Insufficient permissions are handled appropriately
    Given I have a token with insufficient claims
    When I request all workout objectives
    Then I should receive a 403 Forbidden response
    And the error should indicate insufficient permissions
```

## Feature: Workout Categories Integration

### Background and Core Scenarios
```gherkin
Feature: Workout Categories Integration Tests
  As a fitness application
  I want to provide workout category reference data with visual elements
  So that Personal Trainers can categorize workout templates effectively

  Background:
    Given the application is running with a test database
    And the workout categories reference table is seeded with 8 standard categories
    And I have a valid JWT token with Free-Tier claims

  Scenario: Personal Trainer browses workout categories with visual data
    Given I am authenticated as a Personal Trainer
    When I request all workout categories
    Then I should receive 8 standard workout categories
    And each category should include icon and color information
    And each category should specify primary muscle groups
    And the categories should be ordered by display order

    When I request the "HIIT" category by ID
    Then I should receive the HIIT category details
    And the response should include the timer icon
    And the color should be a valid hex code
    And the primary muscle groups should be "Full Body"
```

### Scenario: Visual Data Validation
```gherkin
  Scenario: Workout categories maintain visual data integrity
    Given I am authenticated as a Personal Trainer
    When I request all workout categories
    Then each category should have a unique icon identifier
    And each category should have a unique color code
    And all colors should be valid hex format
    And all icons should reference existing UI assets

    When I validate the category visual consistency
    Then the HIIT category should have color "#FF6B35"
    And the Arms category should have color "#4ECDC4"
    And the Legs category should have color "#45B7D1"
    And each color should provide sufficient contrast for accessibility
```

### Scenario: Category Muscle Group Integration
```gherkin
  Scenario: Categories correctly specify targeted muscle groups
    Given I am authenticated as a Personal Trainer
    When I request all workout categories
    Then the Arms category should target "Biceps, Triceps, Forearms"
    And the Legs category should target "Quadriceps, Glutes, Hamstrings, Calves"
    And the Back category should target "Latissimus Dorsi, Trapezius, Rhomboids"
    And the Full Body category should target "All Major Muscle Groups"
```

## Feature: Execution Protocols Integration

### Background and Protocol Testing
```gherkin
Feature: Execution Protocols Integration Tests
  As a fitness application
  I want to provide execution protocol reference data with technical specifications
  So that Personal Trainers can prescribe appropriate training methodologies

  Background:
    Given the application is running with a test database
    And the execution protocols reference table is seeded with 8 standard protocols
    And I have a valid JWT token with Free-Tier claims

  Scenario: Personal Trainer accesses execution protocols for advanced programming
    Given I am authenticated as a Personal Trainer
    When I request all execution protocols
    Then I should receive 8 standard execution protocols
    And each protocol should include code and technical characteristics
    And each protocol should specify time-base and rep-base properties
    And the protocols should include implementation instructions

    When I request the "STANDARD" protocol by code
    Then I should receive the Standard protocol details
    And the code should be "STANDARD"
    And the protocol should be rep-based but not time-based
    And the rest pattern should be "Fixed"
    And the intensity level should be "Medium"
```

### Scenario: Protocol Code Validation
```gherkin
  Scenario: Execution protocol codes follow consistent formatting
    Given I am authenticated as a Personal Trainer
    When I request all execution protocols
    Then each protocol code should be unique
    And each code should follow UPPERCASE_UNDERSCORE format
    And all codes should be valid programmatic identifiers

    When I request protocols by their standard codes
    Then "STANDARD" should return the Standard protocol
    And "AMRAP" should return the AMRAP protocol
    And "EMOM" should return the EMOM protocol
    And "FOR_TIME" should return the For Time protocol
    And "TABATA" should return the Tabata protocol
```

### Scenario: Protocol Technical Characteristics
```gherkin
  Scenario: Protocols correctly specify technical characteristics
    Given I am authenticated as a Personal Trainer
    When I request all execution protocols
    Then the STANDARD protocol should be rep-based only
    And the AMRAP protocol should be both time-based and rep-based
    And the TABATA protocol should be time-based only
    And the CLUSTER protocol should have "Micro-rests" rest pattern
    And high-intensity protocols should include AMRAP, FOR_TIME, and TABATA
```

## Feature: Cross-Reference Integration

### Multi-Table Relationship Testing
```gherkin
Feature: Cross-Reference Data Integration Tests
  As a fitness application
  I want to ensure consistency across all workout reference tables
  So that reference data provides a coherent foundation for workout programming

  Background:
    Given the application is running with a test database
    And all workout reference tables are seeded with standard data
    And I have a valid JWT token with Free-Tier claims

  Scenario: Reference data consistency across tables
    Given I am authenticated as a Personal Trainer
    When I request data from all workout reference tables
    Then all tables should follow the standard reference data pattern
    And all display orders should be unique within each table
    And all values should be unique within each table
    And all descriptions should provide comprehensive information

  Scenario: Reference data supports workout template creation workflow
    Given I am authenticated as a Personal Trainer
    When I access workout objectives for goal selection
    And I access workout categories for organization
    And I access execution protocols for methodology
    Then I should have all information needed for informed template creation
    And the data should guide appropriate programming decisions
    And objectives should align with available execution protocols
```

## Feature: Performance Integration

### Performance and Scalability Testing
```gherkin
Feature: Workout Reference Data Performance Integration Tests
  As a fitness application
  I want to ensure reference data APIs meet performance requirements
  So that Personal Trainers experience responsive interfaces

  Background:
    Given the application is running with a test database
    And all workout reference tables contain production-level data volumes
    And I have a valid JWT token with Free-Tier claims

  Scenario: API response time requirements are met
    Given I am authenticated as a Personal Trainer
    When I request workout objectives for the first time
    Then the response should complete within 200ms
    And the database query should use appropriate indexes

    When I request the same data from cache
    Then the response should complete within 50ms
    And the response should include cache hit indicators

  Scenario: Concurrent request handling
    Given I am authenticated as a Personal Trainer
    When I make 10 concurrent requests for workout reference data
    Then all requests should complete successfully
    And the average response time should remain under 200ms
    And no database connection pool exhaustion should occur

  Scenario: Large dataset handling
    Given the reference tables contain maximum expected data volumes
    When I request all reference data simultaneously
    Then each request should complete within SLA requirements
    And memory usage should remain within acceptable limits
    And database performance should not degrade
```

## Feature: Error Handling Integration

### Comprehensive Error Scenario Testing
```gherkin
Feature: Workout Reference Data Error Handling Integration Tests
  As a fitness application
  I want to handle error conditions gracefully
  So that Personal Trainers receive helpful error messages and can recover

  Background:
    Given the application is running with a test database
    And I have a valid JWT token with Free-Tier claims

  Scenario: Database connectivity issues are handled gracefully
    Given the database connection is temporarily unavailable
    When I request workout objectives
    Then I should receive a 500 Internal Server Error response
    And the error message should indicate service unavailability
    And the response should include retry guidance

  Scenario: Invalid request parameters are validated properly
    Given I am authenticated as a Personal Trainer
    When I request a workout objective with malformed GUID
    Then I should receive a 400 Bad Request response
    And the error should specify the invalid ID format

    When I request an execution protocol with invalid code format
    Then I should receive a 400 Bad Request response
    And the error should specify valid code format requirements

  Scenario: Non-existent resource requests return appropriate errors
    Given I am authenticated as a Personal Trainer
    When I request a workout objective that doesn't exist
    Then I should receive a 404 Not Found response
    And the error message should be user-friendly

    When I request an execution protocol with non-existent code
    Then I should receive a 404 Not Found response
    And the error should indicate the code was not found
```

## Feature: Data Seeding Integration

### Reference Data Initialization Testing
```gherkin
Feature: Workout Reference Data Seeding Integration Tests
  As a fitness application
  I want to ensure reference data is properly initialized
  So that the system starts with complete and accurate fitness information

  Scenario: Database seeding creates complete reference data
    Given a fresh database instance
    When the application starts and runs data seeding
    Then 7 workout objectives should be created
    And 8 workout categories should be created
    And 8 execution protocols should be created
    And all data should pass validation rules

  Scenario: Seeded data matches fitness industry standards
    Given the database has been seeded
    When I validate the seeded workout objectives
    Then the objectives should cover all major training goals
    And the programming guidance should be scientifically accurate
    And the rep ranges and intensity levels should align with exercise science

    When I validate the seeded workout categories
    Then the categories should cover all major muscle groups
    And the visual design elements should be consistent
    And the muscle group assignments should be anatomically correct

    When I validate the seeded execution protocols
    Then the protocols should represent established training methodologies
    And the implementation instructions should be clear and accurate
    And the technical characteristics should be correctly specified
```

## Feature: Cache Integration

### Caching Strategy Validation
```gherkin
Feature: Workout Reference Data Caching Integration Tests
  As a fitness application
  I want to optimize performance through effective caching
  So that Personal Trainers experience fast response times

  Background:
    Given the application is running with Redis cache
    And all workout reference tables are seeded
    And I have a valid JWT token with Free-Tier claims

  Scenario: Reference data caching improves performance
    Given the cache is empty
    And I am authenticated as a Personal Trainer
    When I request workout objectives for the first time
    Then the data should be fetched from the database
    And the data should be cached with appropriate TTL
    And the response should include cache miss indicators

    When I request the same data again
    Then the data should be served from cache
    And the response time should be significantly faster
    And the response should include cache hit indicators

  Scenario: Cache invalidation works correctly
    Given workout objectives are cached
    When reference data is updated in the database
    Then the cache should be invalidated automatically
    And subsequent requests should fetch fresh data
    And the new data should be cached appropriately

  Scenario: Cache failure handling
    Given the cache service is unavailable
    When I request workout reference data
    Then the request should fallback to database
    And the response should still be successful
    And performance should degrade gracefully
```

## Step Definitions

### Common Step Definitions
```csharp
[Binding]
public class WorkoutReferenceDataStepDefinitions
{
    private readonly WorkoutReferenceDataTestContext _context;
    private HttpResponseMessage _response;
    private string _responseContent;

    [Given(@"the application is running with a test database")]
    public async Task GivenTheApplicationIsRunningWithTestDatabase()
    {
        await _context.InitializeAsync();
    }

    [Given(@"I have a valid JWT token with Free-Tier claims")]
    public void GivenIHaveValidJWTToken()
    {
        _context.Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", TestTokenHelper.CreateValidToken());
    }

    [When(@"I request all workout objectives")]
    public async Task WhenIRequestAllWorkoutObjectives()
    {
        _response = await _context.Client.GetAsync("/api/workout-objectives");
        _responseContent = await _response.Content.ReadAsStringAsync();
    }

    [Then(@"I should receive (.*) standard workout objectives")]
    public void ThenIShouldReceiveStandardWorkoutObjectives(int expectedCount)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var objectives = JsonSerializer.Deserialize<WorkoutObjective[]>(_responseContent);
        objectives.Should().HaveCount(expectedCount);
    }
}
```

### Performance Step Definitions
```csharp
[Binding]
public class PerformanceStepDefinitions
{
    private readonly Stopwatch _stopwatch = new();

    [When(@"I request (.*) for the first time")]
    public async Task WhenIRequestForFirstTime(string endpoint)
    {
        _stopwatch.Start();
        // Make request
        _stopwatch.Stop();
    }

    [Then(@"the response should complete within (.*)ms")]
    public void ThenResponseShouldCompleteWithinMs(int maxMilliseconds)
    {
        _stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(maxMilliseconds);
    }
}
```

### Authentication Step Definitions
```csharp
[Binding]
public class AuthenticationStepDefinitions
{
    [Given(@"I have no authentication token")]
    public void GivenIHaveNoAuthenticationToken()
    {
        _context.Client.DefaultRequestHeaders.Authorization = null;
    }

    [Given(@"I have a token with insufficient claims")]
    public void GivenIHaveTokenWithInsufficientClaims()
    {
        _context.Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", TestTokenHelper.CreateTokenWithoutClaims());
    }
}
```

## Test Data Management

### Reference Data Builders
```csharp
public static class WorkoutReferenceDataSeeder
{
    public static async Task SeedAllReferenceData(DbContext context)
    {
        await SeedWorkoutObjectives(context);
        await SeedWorkoutCategories(context);
        await SeedExecutionProtocols(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedWorkoutObjectives(DbContext context)
    {
        var objectives = new[]
        {
            new WorkoutObjective
            {
                Value = "Muscular Strength",
                Description = "Develops maximum force production capabilities...",
                DisplayOrder = 1,
                IsActive = true
            },
            // ... additional objectives
        };
        
        await context.WorkoutObjectives.AddRangeAsync(objectives);
    }
}
```

## Continuous Integration

### Test Execution Pipeline
```yaml
integration-tests:
  runs-on: ubuntu-latest
  services:
    postgres:
      image: postgres:15
      env:
        POSTGRES_PASSWORD: test
      options: >-
        --health-cmd pg_isready
        --health-interval 10s
        --health-timeout 5s
        --health-retries 5
  
  steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Run Integration Tests
      run: dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"
    
    - name: Generate Test Report
      uses: dorny/test-reporter@v1
      with:
        name: Integration Test Results
        path: '**/*.trx'
        reporter: dotnet-trx
```

### Quality Gates
- **Test Success Rate**: 100% pass rate required
- **Performance Thresholds**: All response times under SLA
- **Coverage Requirements**: 90%+ integration path coverage
- **Data Validation**: All seeded data passes validation