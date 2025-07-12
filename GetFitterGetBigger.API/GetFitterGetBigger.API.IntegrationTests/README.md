# GetFitterGetBigger BDD Integration Tests

This project contains Behavior-Driven Development (BDD) integration tests for the GetFitterGetBigger API using SpecFlow and TestContainers.

## 🏗️ Project Structure

```
GetFitterGetBigger.API.IntegrationTests/
├── Features/                    # Gherkin feature files
│   ├── Basic/                  # Basic infrastructure tests
│   ├── Exercise/               # Exercise management scenarios
│   ├── Equipment/              # Equipment management scenarios
│   └── Authentication/         # Auth scenarios
├── StepDefinitions/            # SpecFlow step implementations
│   ├── Api/                    # HTTP request/response steps
│   ├── Authentication/         # Auth-specific steps
│   ├── Database/               # Database verification steps
│   └── Common/                 # Shared steps
├── TestInfrastructure/         # Test support code
│   ├── Fixtures/               # Test fixtures and setup
│   ├── Helpers/                # Test helpers
│   └── Configuration/          # Test configuration
├── Hooks/                      # SpecFlow hooks
├── Utilities/                  # Test utilities
└── specflow.json              # SpecFlow configuration
```

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop (for PostgreSQL TestContainers)
- Visual Studio 2022 or JetBrains Rider (recommended for SpecFlow support)

### Running Tests

```bash
# Run all BDD tests
dotnet test

# Run specific feature
dotnet test --filter "FullyQualifiedName~DatabaseConnection"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 📝 Writing New Features

### 1. Create a Feature File

Create a new `.feature` file in the appropriate folder:

```gherkin
Feature: My New Feature
    As a user
    I want to perform an action
    So that I achieve a goal

Scenario: Successful action
    Given I am authenticated as a "PT-Tier"
    When I perform the action
    Then the result should be successful
```

### 2. Run Tests to Generate Steps

Run the tests - SpecFlow will provide step definition stubs for missing steps.

### 3. Implement Step Definitions

Add the generated step definitions to the appropriate StepDefinitions class:

```csharp
[When(@"I perform the action")]
public async Task WhenIPerformTheAction()
{
    // Implementation
}
```

## 🔧 Available Step Definitions

### Authentication Steps
- `Given I am authenticated as a "{role}"`
- `Given I am not authenticated`
- `Given I have a valid JWT token`
- `When I authenticate with email "{email}"`

### API Request Steps
- `When I send a {method} request to "{endpoint}"`
- `When I send a {method} request to "{endpoint}" with body:`
- `When I add header "{name}" with value "{value}"`

### Response Validation Steps
- `Then the response status should be {statusCode}`
- `Then the response should contain "{text}"`
- `Then the response should have property "{path}" with value "{value}"`
- `Then the response should be empty`

### Database Steps
- `Given the database is empty`
- `Given the database has reference data`
- `Given the following {entity} exists:`
- `Then the database should contain {count} {entity} records`
- `Then the {entity} with id "{id}" should exist`

## 🧪 Test Infrastructure

### PostgreSQL TestContainers

Tests automatically start a PostgreSQL container for each test run. No manual database setup required!

### Test Isolation

- Each scenario starts with a clean database
- Reference data is automatically seeded
- Test data is cleaned up after each scenario

### Placeholder Resolution

Use placeholders in scenarios to reference created entities:

```gherkin
Given the following exercise exists:
    | Name | Description |
    | Squat | Leg exercise |
When I send a GET request to "/api/exercises/<Squat.Id>"
```

## 🐛 Troubleshooting

### Docker Issues

If tests fail with container startup errors:
1. Ensure Docker Desktop is running
2. Check Docker has sufficient resources allocated
3. Try `docker system prune` to clean up old containers

### Build Issues

If SpecFlow doesn't generate code from feature files:
1. Clean and rebuild the solution
2. Check `specflow.json` configuration
3. Ensure SpecFlow.Tools.MsBuild.Generation package is installed

### Test Discovery Issues

If tests aren't discovered:
1. Ensure feature files have `.feature` extension
2. Check the build action is set to "SpecFlowFeature"
3. Rebuild the project

## 📚 Additional Resources

- [SpecFlow Documentation](https://docs.specflow.org/)
- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [Gherkin Reference](https://cucumber.io/docs/gherkin/reference/)

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on:
- Writing effective scenarios
- Naming conventions
- Step definition best practices
- Code review process