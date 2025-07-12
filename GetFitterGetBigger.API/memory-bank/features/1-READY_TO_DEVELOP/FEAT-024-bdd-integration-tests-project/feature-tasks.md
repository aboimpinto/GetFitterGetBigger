# Feature Tasks: FEAT-024 - BDD Integration Tests Project

## Overview
Create a new BDD-based integration test project using SpecFlow and TestContainers.PostgreSQL.

## Prerequisites
- [ ] Verify Docker is installed and running on development machine
- [ ] Review existing integration tests in GetFitterGetBigger.API.Tests
- [ ] Understand current test infrastructure patterns

## Task List

### 1. Project Setup and Configuration
**Estimated Time**: 2 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Create new test project `GetFitterGetBigger.API.IntegrationTests`
  - Use .NET 9.0
  - Project type: xUnit Test Project
- [ ] Add project to solution file
- [ ] Configure project SDK and target framework in .csproj:
  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>net9.0</TargetFramework>
      <IsPackable>false</IsPackable>
      <IsTestProject>true</IsTestProject>
    </PropertyGroup>
  </Project>
  ```
- [ ] Add NuGet packages:
  - SpecFlow (4.0.x or latest)
  - SpecFlow.xUnit (4.0.x or latest)
  - SpecFlow.Tools.MsBuild.Generation (4.0.x or latest)
  - Testcontainers.PostgreSql (3.10.x or latest)
  - Microsoft.AspNetCore.Mvc.Testing (9.0.x)
  - FluentAssertions (6.12.x or latest)
  - xunit (2.9.x or latest)
  - xunit.runner.visualstudio (2.8.x or latest)
- [ ] Add project reference to API project
- [ ] Configure SpecFlow.json settings:
  ```json
  {
    "bindingCulture": {
      "language": "en-US"
    },
    "generator": {
      "allowDebugGeneratedFiles": true,
      "allowRowTests": true
    },
    "runtime": {
      "missingOrPendingStepsOutcome": "Error"
    }
  }
  ```

### 2. Test Infrastructure Setup
**Estimated Time**: 4 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Create folder structure:
  ```
  TestInfrastructure/
  ├── Fixtures/
  ├── Helpers/
  └── Configuration/
  ```
- [ ] Implement `IntegrationTestWebApplicationFactory.cs`:
  ```csharp
  public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
  {
      private readonly PostgreSqlContainer _postgresContainer;
      
      protected override void ConfigureWebHost(IWebHostBuilder builder)
      {
          // Override database configuration
          // Configure test services
          // Setup authentication mocks
      }
  }
  ```
- [ ] Create `PostgreSqlTestFixture.cs`:
  - Implement IAsyncLifetime for container lifecycle
  - PostgreSQL container setup with TestContainers
  - Connection string generation
  - Database migration execution
  - Reference data seeding
- [ ] Implement `TestDatabaseSeeder.cs`:
  - Seed all reference data (DifficultyLevels, BodyParts, etc.)
  - Create test data factory methods
  - Implement data cleanup strategies
- [ ] Create `ScenarioContextExtensions.cs`:
  - Store/retrieve test entities by key
  - Type-safe access to test data
  - HTTP response storage
  - Authentication token management

### 3. Core Step Definitions
**Estimated Time**: 4 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Create `StepDefinitions` folder structure:
  ```
  StepDefinitions/
  ├── Authentication/
  │   └── AuthenticationSteps.cs
  ├── Api/
  │   ├── RequestSteps.cs
  │   └── ResponseSteps.cs
  ├── Database/
  │   └── DatabaseSteps.cs
  └── Common/
      └── CommonSteps.cs
  ```
- [ ] Implement Authentication steps (`AuthenticationSteps.cs`):
  ```csharp
  [Given(@"I am authenticated as a ""(.*)""")]
  public async Task GivenIAmAuthenticatedAs(string role)
  
  [Given(@"I am not authenticated")]
  public void GivenIAmNotAuthenticated()
  
  [Given(@"I have a valid JWT token")]
  public void GivenIHaveAValidJwtToken()
  ```
  
  **⚠️ IMPORTANT AUTHENTICATION NOTE:**
  - Current known roles: "PT-Tier", "Admin-Tier", "Free-Tier"
  - **ALWAYS ASK** which role/claim is required for each endpoint
  - **DO NOT ASSUME** authorization requirements
  - When writing test scenarios, explicitly verify with stakeholders:
    - What role can access this endpoint?
    - What happens when unauthorized users try to access?
    - Are there different behaviors for different roles?
  - Document authorization requirements in each feature file
- [ ] Implement API request steps (`RequestSteps.cs`):
  ```csharp
  [When(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)""")]
  public async Task WhenISendARequestTo(string method, string endpoint)
  
  [When(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)"" with body:")]
  public async Task WhenISendARequestWithBody(string method, string endpoint, string body)
  
  [When(@"I add header ""(.*)"" with value ""(.*)""")]
  public void WhenIAddHeader(string name, string value)
  ```
- [ ] Implement response validation steps (`ResponseSteps.cs`):
  ```csharp
  [Then(@"the response status should be (\d+)")]
  public void ThenTheResponseStatusShouldBe(int statusCode)
  
  [Then(@"the response should contain ""(.*)""")]
  public void ThenTheResponseShouldContain(string text)
  
  [Then(@"the response should have property ""(.*)"" with value ""(.*)""")]
  public void ThenTheResponseShouldHaveProperty(string jsonPath, string expectedValue)
  
  [Then(@"the response should be empty")]
  public void ThenTheResponseShouldBeEmpty()
  ```
- [ ] Implement database steps (`DatabaseSteps.cs`):
  ```csharp
  [Given(@"the following (.*) exists:")]
  public async Task GivenTheFollowingEntityExists(string entityType, Table table)
  
  [Then(@"the database should contain (\d+) (.*) records?")]
  public async Task ThenTheDatabaseShouldContain(int count, string entityType)
  
  [Then(@"the (.*) with id ""(.*)"" should exist")]
  public async Task ThenTheEntityWithIdShouldExist(string entityType, string id)
  ```

### 4. Hooks and Utilities
**Estimated Time**: 2 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Create `Hooks` folder
- [ ] Implement `DatabaseHooks.cs`:
  - Before/After scenario database cleanup
  - Transaction management
- [ ] Implement `WebApiHooks.cs`:
  - HTTP client initialization
  - Request/Response logging
- [ ] Create `Utilities` folder:
  - `ApiClient.cs` - HTTP request builder
  - `JsonHelper.cs` - JSON serialization utilities
  - `TestDataBuilder.cs` - Fluent test data creation

### 5. Example Feature Implementation
**Estimated Time**: 3 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Create `Features` folder structure:
  ```
  Features/
  ├── Exercise/
  │   └── ExerciseManagement.feature
  ├── Equipment/
  │   └── EquipmentManagement.feature
  └── Authentication/
      └── Authentication.feature
  ```
- [ ] Implement `ExerciseManagement.feature`:
  ```gherkin
  Feature: Exercise Management
    As a personal trainer
    I want to manage exercises
    So that I can create workout plans

  Background:
    # ⚠️ TODO: Verify with stakeholders - which roles can manage exercises?
    # Currently assuming PT-Tier based on existing tests, but MUST CONFIRM
    Given I am authenticated as a "PT-Tier"
    And the following difficulty levels exist:
      | Name     | Value | Description                  |
      | Beginner | 1     | Suitable for beginners       |
      | Advanced | 3     | Suitable for advanced users  |
    And the following equipment exists:
      | Name       | Description                  |
      | Barbell    | Standard Olympic barbell     |
      | Dumbbell   | Adjustable dumbbell          |

  Scenario: Create a new exercise successfully
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Bench Press",
        "description": "Chest exercise",
        "difficultyLevelId": "difficultylevel-<Beginner.Id>",
        "equipmentIds": ["equipment-<Barbell.Id>"]
      }
      """
    Then the response status should be 201
    And the response should have property "name" with value "Bench Press"
    And the database should contain 1 exercise records

  Scenario: Fail to create duplicate exercise
    Given the following exercise exists:
      | Name        | Description      | DifficultyLevel |
      | Bench Press | Chest exercise   | Beginner        |
    When I send a POST request to "/api/exercises" with body:
      """
      {
        "name": "Bench Press",
        "description": "Another chest exercise",
        "difficultyLevelId": "difficultylevel-<Beginner.Id>"
      }
      """
    Then the response status should be 409
    And the response should contain "already exists"

  Scenario: Update existing exercise
    Given the following exercise exists:
      | Name        | Description    | DifficultyLevel |
      | Squat       | Leg exercise   | Beginner        |
    When I send a PUT request to "/api/exercises/<Squat.Id>" with body:
      """
      {
        "name": "Back Squat",
        "description": "Updated leg exercise",
        "difficultyLevelId": "difficultylevel-<Advanced.Id>"
      }
      """
    Then the response status should be 200
    And the response should have property "name" with value "Back Squat"
    And the response should have property "difficultyLevel.value" with value "3"
  ```
- [ ] Implement `Authentication.feature` with various auth scenarios:
  ```gherkin
  Feature: Authentication and Authorization
    # ⚠️ IMPORTANT: These scenarios are EXAMPLES ONLY
    # MUST verify actual authorization requirements with stakeholders
    
    Scenario Outline: Access control for exercise management
      Given I am authenticated as a "<role>"
      When I send a POST request to "/api/exercises" with valid data
      Then the response status should be <status>
      
      Examples:
        | role       | status | notes                               |
        | PT-Tier    | 201    | # TODO: Confirm PT can create      |
        | Admin-Tier | 201    | # TODO: Confirm Admin can create   |
        | Free-Tier  | 403    | # TODO: Confirm Free cannot create |
        
    Scenario: Unauthenticated access attempt
      Given I am not authenticated
      When I send a GET request to "/api/exercises"
      Then the response status should be 401  # TODO: Or is it 403?
  ```
- [ ] Implement `EquipmentManagement.feature` for CRUD operations
- [ ] Test all example features
- [ ] Verify test reports are generated correctly

### 6. Documentation and Guidelines
**Estimated Time**: 1 hour
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Create `README.md` in test project:
  - Project structure overview
  - How to run tests
  - Writing new features guide
- [ ] Document available step definitions
- [ ] Create `CONTRIBUTING.md`:
  - Naming conventions
  - Step definition guidelines
  - Best practices
- [ ] Add example snippets for common scenarios

### 7. CI/CD Integration
**Estimated Time**: 1 hour
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Update test discovery in CI pipeline
- [ ] Configure test reporting for SpecFlow
- [ ] Ensure Docker is available in CI environment
- [ ] Add test result artifacts

### 8. Migration Planning and Tracking
**Estimated Time**: 2 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Analyze existing integration tests in GetFitterGetBigger.API.Tests:
  - Count total number of integration tests
  - Categorize tests by domain (Exercise, Equipment, Auth, etc.)
  - Identify test coverage metrics
- [ ] Create migration tracking document:
  ```markdown
  # Integration Test Migration Tracker
  
  ## Current Coverage Baseline
  - Total Integration Tests: X
  - Code Coverage: X%
  
  ## Migration Status
  | Original Test | Feature File | Status | Coverage |
  |--------------|--------------|---------|----------|
  | ExercisesControllerTests.CreateExercise | ExerciseManagement.feature | ❌ Not Started | - |
  | ... | ... | ... | ... |
  ```
- [ ] Define migration order (prioritize by):
  - Critical business flows
  - Most frequently failing tests
  - Tests with external dependencies
- [ ] Create test coverage comparison script
- [ ] Document migration process for team

### 9. Test-by-Test Migration Phase
**Estimated Time**: 20-30 hours (separate from initial setup)
**Status**: [ ] Not Started

#### Migration Process (Per Test):
- [ ] For each test in API.Tests:
  1. Identify the test scenario and business value
  2. Write equivalent Gherkin scenario
  3. Implement missing step definitions
  4. Run both old and new tests in parallel
  5. Verify equivalent coverage
  6. Mark as migrated in tracking document
  7. Keep original test active until full migration

#### Example Migration:
```csharp
// Original Test in API.Tests
[Fact]
public async Task CreateExercise_WithValidData_ReturnsCreated()
{
    // Arrange & Act & Assert
}

// Becomes in BDD:
Scenario: Create exercise with valid data
  Given I am authenticated as a "PT-Tier"
  When I send a POST request to "/api/exercises" with valid data
  Then the response status should be 201
```

### 10. Verification and Cleanup
**Estimated Time**: 3 hours
**Status**: [ ] Not Started

#### Subtasks:
- [ ] Run full test suite comparison:
  - Execute all original integration tests
  - Execute all BDD integration tests
  - Compare coverage reports
  - Ensure coverage is equal or better
- [ ] Performance comparison:
  - Measure execution time of old vs new tests
  - Document any performance improvements/regressions
- [ ] Final verification checklist:
  - [ ] All integration tests migrated
  - [ ] Coverage >= original coverage
  - [ ] No failing tests
  - [ ] CI/CD pipeline updated
- [ ] Cleanup tasks:
  - [ ] Remove old integration test infrastructure from API.Tests
  - [ ] Remove obsolete test helpers
  - [ ] Update project references
  - [ ] Archive migration tracking document
- [ ] Team notification and knowledge transfer

## Total Estimated Time: 
- Initial Setup: 18 hours
- Migration Phase: 20-30 hours (done incrementally)
- Verification & Cleanup: 3 hours
- **Total: 41-51 hours**

## Success Criteria
- [ ] New project builds successfully
- [ ] Example feature passes all tests
- [ ] TestContainers starts and stops correctly
- [ ] Documentation is complete and clear
- [ ] Other developers can add new features easily
- [ ] CI/CD pipeline runs BDD tests successfully
- [ ] Test reports are generated in human-readable format

## Technical Notes
- Use `<EntityName.PropertyName>` syntax in Gherkin for referencing test data
- Ensure all step definitions use dependency injection for services
- Keep scenarios focused on business behavior, not technical implementation
- Use tags (@smoke, @integration) for test categorization
- Consider parallel execution settings for faster test runs

## Risk Mitigation
- **Docker dependency**: Provide fallback instructions for developers without Docker
- **Test performance**: Monitor container startup time, consider reusing containers
- **Learning curve**: Provide comprehensive examples and pair programming sessions

## Definition of Done
- [ ] All tasks completed
- [ ] Code reviewed by team lead
- [ ] Documentation reviewed and approved
- [ ] Example tests passing in CI/CD
- [ ] Team training session conducted