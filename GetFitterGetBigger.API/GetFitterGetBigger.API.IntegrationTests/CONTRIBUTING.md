# Contributing to BDD Integration Tests

This guide helps you write effective BDD tests for the GetFitterGetBigger API.

## 📝 Writing Effective Scenarios

### The Three Amigos Approach

Before writing scenarios, consider these three perspectives:
1. **Business**: What value does this feature provide?
2. **Development**: How will this be implemented?
3. **Testing**: What could go wrong?

### Scenario Structure

Follow the Given-When-Then pattern:
- **Given**: The initial context (setup)
- **When**: The action being performed
- **Then**: The expected outcome

```gherkin
Scenario: Create exercise with valid data
  Given I am authenticated as a "PT-Tier"
  And the database has reference data
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "Bench Press",
      "description": "Chest exercise"
    }
    """
  Then the response status should be 201
  And the response should have property "id"
```

## 📛 Naming Conventions

### Feature Files
- Use PascalCase: `ExerciseManagement.feature`
- Group by domain: `Exercise/`, `Equipment/`, `Authentication/`
- Be descriptive but concise

### Scenarios
- Start with the expected outcome
- Be specific about the context
- Avoid technical jargon

✅ Good:
```gherkin
Scenario: Successfully create exercise with all required fields
Scenario: Reject exercise creation when name already exists
Scenario: Require authentication for exercise creation
```

❌ Bad:
```gherkin
Scenario: Test POST endpoint
Scenario: Exercise test 1
Scenario: Check validation
```

### Step Definitions
- Use consistent parameter names
- Group related steps in the same class
- Keep implementations simple and focused

## 🎯 Step Definition Best Practices

### 1. Reusability

Create generic steps that can be reused:

```csharp
// ✅ Good - Reusable
[When(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)""")]
public async Task WhenISendARequestTo(string method, string endpoint)

// ❌ Bad - Too specific
[When(@"I create an exercise named ""Bench Press""")]
public async Task WhenICreateBenchPressExercise()
```

### 2. Parameter Types

Use appropriate parameter types:

```csharp
[Then(@"the response status should be (\d+)")]
public void ThenTheResponseStatusShouldBe(int statusCode)  // ✅ int, not string

[Given(@"I wait for ([\d.]+) seconds")]
public async Task GivenIWaitForSeconds(double seconds)     // ✅ double for decimals
```

### 3. Async/Await

Use async/await for I/O operations:

```csharp
[When(@"I send a request")]
public async Task WhenISendARequest()  // ✅ async Task
{
    var response = await _httpClient.GetAsync(endpoint);
}
```

### 4. Error Messages

Provide helpful error messages:

```csharp
actualCount.Should().Be(expectedCount, 
    $"expected {expectedCount} {entityType} records but found {actualCount}");  // ✅
```

## 🧪 Test Data Management

### Use Builders

Leverage the test builders for consistent data:

```gherkin
Given the following exercise exists:
  | Name        | Description    | DifficultyLevel |
  | Bench Press | Chest exercise | Beginner        |
```

### Reference Data

Always ensure reference data is available:

```gherkin
Background:
  Given the database has reference data
```

### Placeholder Usage

Use placeholders for dynamic values:

```gherkin
When I send a GET request to "/api/exercises/<BenchPress.Id>"
Then the response should have property "id" with value "<BenchPress.Id>"
```

## 🔒 Authorization Testing

### Always Specify Roles

Be explicit about authorization requirements:

```gherkin
# ⚠️ TODO: Verify with stakeholders - which roles can manage exercises?
Given I am authenticated as a "PT-Tier"
```

### Test Multiple Roles

Use scenario outlines for role-based testing:

```gherkin
Scenario Outline: Verify authorization for exercise management
  Given I am authenticated as a "<role>"
  When I send a POST request to "/api/exercises" with valid data
  Then the response status should be <expectedStatus>
  
  Examples:
    | role       | expectedStatus |
    | PT-Tier    | 201           |
    | Admin-Tier | 201           |
    | Free-Tier  | 403           |
```

## 🏷️ Tagging Strategy

Use tags to categorize and filter tests:

```gherkin
@smoke
Scenario: Basic connectivity test

@authorization
Scenario: Verify role-based access

@slow @database
Scenario: Complex workflow with multiple database operations
```

Common tags:
- `@smoke` - Quick tests for basic functionality
- `@authorization` - Tests focusing on auth/authz
- `@slow` - Tests that take longer to run
- `@wip` - Work in progress (exclude from CI)

## 🐛 Debugging Tips

### 1. Use Hooks for Logging

The `WebApiHooks` class logs all requests and responses automatically.

### 2. Capture Test Context

Store relevant data in ScenarioContext:

```csharp
_scenarioContext.Add("CreatedExerciseId", response.Id);
```

### 3. Use Meaningful Assertions

```csharp
// ✅ Good - explains what failed
response.StatusCode.Should().Be(HttpStatusCode.Created, 
    "exercise creation should return 201 Created");

// ❌ Bad - no context
Assert.Equal(201, (int)response.StatusCode);
```

## 📋 Code Review Checklist

Before submitting a PR, ensure:

- [ ] Scenarios follow Given-When-Then pattern
- [ ] Step definitions are reusable
- [ ] Authorization requirements are documented
- [ ] Test data uses builders/tables
- [ ] Placeholders are used for dynamic values
- [ ] Appropriate tags are applied
- [ ] No hardcoded IDs or URLs
- [ ] Error messages are descriptive
- [ ] Tests are isolated (no dependencies between scenarios)
- [ ] Build passes with no warnings

## 🚀 Advanced Patterns

### Background Sections

Use for common setup:

```gherkin
Background:
  Given I am authenticated as a "PT-Tier"
  And the database has reference data
  And the following equipment exists:
    | Name     | Description |
    | Barbell  | Standard barbell |
    | Dumbbell | Adjustable dumbbell |
```

### Scenario Outlines

Use for data-driven tests:

```gherkin
Scenario Outline: Validate exercise name length
  When I send a POST request to "/api/exercises" with body:
    """
    {
      "name": "<name>",
      "description": "Test"
    }
    """
  Then the response status should be <status>
  
  Examples:
    | name | status | reason |
    | A    | 400    | Too short |
    | AB   | 201    | Valid minimum |
    | #{"A" * 101} | 400 | Too long |
```

### Custom Transformations

For complex data transformations:

```csharp
[StepArgumentTransformation]
public DateTime TransformDate(string dateString)
{
    return DateTime.Parse(dateString);
}
```

## 📚 Resources

- [Gherkin Best Practices](https://cucumber.io/docs/bdd/better-gherkin/)
- [SpecFlow Documentation](https://docs.specflow.org/)
- [FluentAssertions Best Practices](https://fluentassertions.com/tips/)

## 🤝 Getting Help

- Check existing scenarios for examples
- Review the [STEP-DEFINITIONS.md](STEP-DEFINITIONS.md) file
- Ask the team if unsure about authorization requirements
- Use `// TODO:` comments for pending clarifications