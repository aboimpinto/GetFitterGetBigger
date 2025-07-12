# Quick Reference: BDD Integration Tests with SpecFlow

## Running Tests

### Run All BDD Tests
```bash
dotnet test GetFitterGetBigger.API.IntegrationTests
```

### Run Specific Feature
```bash
dotnet test GetFitterGetBigger.API.IntegrationTests --filter "Category=exercise"
```

### Run with Detailed Output
```bash
dotnet test GetFitterGetBigger.API.IntegrationTests --logger "console;verbosity=detailed"
```

## Common Step Definitions

### Authentication
```gherkin
Given I am authenticated as a "PT-Tier"
Given I am not authenticated
```

### HTTP Requests
```gherkin
When I send a GET request to "/api/exercises"
When I send a POST request to "/api/exercises" with body:
  """
  {
    "name": "Bench Press"
  }
  """
```

### Response Validation
```gherkin
Then the response status should be 200
Then the response should contain "success"
Then the response should have property "id"
Then the response should have property "name" with value "Bench Press"
```

### Data Setup
```gherkin
Given the database has reference data
Given I have created equipment "Barbell" via API
Given the following exercises exist:
  | Name        | DifficultyLevel |
  | Push Up     | Beginner       |
  | Pull Up     | Intermediate   |
```

## Writing New Features

### Feature File Template
```gherkin
Feature: [Feature Name]
  As a [role]
  I want [action]
  So that [benefit]

  Background:
    Given I am authenticated as a "PT-Tier"
    And the database has reference data

  @tag1 @tag2
  Scenario: [Scenario description]
    Given [precondition]
    When [action]
    Then [expected outcome]
```

### Common Tags
- `@smoke` - Critical path tests
- `@integration` - Full integration tests
- `@equipment` - Equipment-related tests
- `@exercise` - Exercise-related tests
- `@reference` - Reference data tests

## Placeholder System

### Basic Usage
```gherkin
Given I have created equipment "Barbell" via API
When I send a GET request to "/api/equipment/<Barbell.id>"
```

### Nested Properties
```gherkin
Then the response should have property "difficultyLevel.value" with value "3"
```

### Storing Values
```gherkin
And I store the response property "id" as "exerciseId"
When I send a GET request to "/api/exercises/<exerciseId>"
```

## Debugging Tests

### 1. Check Docker
```bash
docker ps  # Ensure Docker is running
```

### 2. Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~CreateExerciseWithValidData"
```

### 3. Enable Logging
Add to test project:
```xml
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
```

### 4. Breakpoint in Step Definition
```csharp
[When(@"I send a GET request to ""(.*)""")]
public async Task WhenISendAGETRequestTo(string endpoint)
{
    // Set breakpoint here
    var response = await _httpClient.GetAsync(endpoint);
}
```

## Common Patterns

### CRUD Test Pattern
```gherkin
Scenario: Full CRUD flow
  # Create
  When I send a POST request to "/api/items" with body:
    """
    {"name": "Test Item"}
    """
  Then the response status should be 201
  And I store the response property "id" as "itemId"
  
  # Read
  When I send a GET request to "/api/items/<itemId>"
  Then the response status should be 200
  
  # Update
  When I send a PUT request to "/api/items/<itemId>" with body:
    """
    {"name": "Updated Item"}
    """
  Then the response status should be 200
  
  # Delete
  When I send a DELETE request to "/api/items/<itemId>"
  Then the response status should be 204
```

### Validation Test Pattern
```gherkin
Scenario Outline: Invalid input returns bad request
  When I send a POST request to "/api/items" with body:
    """
    <request_body>
    """
  Then the response status should be 400
  
  Examples:
    | request_body |
    | {"name": ""} |
    | {"name": null} |
    | {} |
```

## Project Structure

### Where to Put Files
```
Features/
├── Exercise/          # Exercise-related features
├── Equipment/         # Equipment features
├── ReferenceData/     # Reference table features
└── Authentication/    # Auth features

StepDefinitions/
├── Api/              # Generic HTTP steps
├── Exercise/         # Exercise-specific steps
└── Common/           # Shared steps
```

### Naming Conventions
- Feature files: `EntityOperation.feature` (e.g., `ExerciseCrud.feature`)
- Step classes: `EntitySteps.cs` (e.g., `ExerciseSteps.cs`)
- Use PascalCase for file names

## Troubleshooting

### Test Not Found
- Rebuild: `dotnet build`
- Check feature file has `.feature` extension
- Ensure SpecFlow generated `.feature.cs` file

### Container Issues
- Restart Docker Desktop
- Clear containers: `docker system prune`
- Check Docker memory settings (4GB+ recommended)

### Step Not Matching
- Check exact text match (including quotes)
- Verify regex patterns in step definitions
- Look for extra spaces or special characters

### Database Issues
- Check migrations ran: Look for "Database ready" in output
- Verify seed data: Reference data should be present
- Transaction rollback: Each scenario starts fresh

## Performance Tips

1. **Reuse HTTP Client**: Already handled by infrastructure
2. **Minimize Database Calls**: Use Background for shared setup
3. **Parallel Execution**: Add to `.runsettings` file
4. **Container Reuse**: Fixture handles this automatically

## CI/CD Integration

### GitHub Actions
```yaml
- name: Run BDD Tests
  run: dotnet test GetFitterGetBigger.API.IntegrationTests --logger:junit
```

### Azure DevOps
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/GetFitterGetBigger.API.IntegrationTests.csproj'
```

## Getting Help

1. **Step Definition Reference**: See `STEP-DEFINITIONS.md`
2. **Examples**: Check `EXAMPLES.md` for common scenarios
3. **Pattern Guide**: `EXERCISE-BUILDER-STEPS-PATTERN.md` for complex setups
4. **Contributing**: Read `CONTRIBUTING.md` for best practices

---
**Quick Tip**: Start with an existing feature file as a template!