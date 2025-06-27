# GetFitterGetBigger.Admin.Tests

This project contains unit tests for the GetFitterGetBigger Admin application.

## Test Framework and Libraries

- **xUnit**: Test framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library for more readable test assertions
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing for ASP.NET Core applications

## Test Organization

### Folder Structure
```
GetFitterGetBigger.Admin.Tests/
├── Builders/           # Test data builders using Builder pattern
├── Helpers/            # Test utilities and mock implementations
└── Services/           # Service unit tests
    └── Authentication/ # Authentication-related service tests
```

### Naming Conventions

#### Test Classes
- Test class names should end with `Tests`
- Example: `AuthServiceTests` for testing `AuthService`

#### Test Methods
- Follow the pattern: `MethodName_StateUnderTest_ExpectedBehavior`
- Examples:
  - `GetCurrentUserAsync_WhenUserAuthenticated_ReturnsAuthUser`
  - `IsAuthenticatedAsync_WhenUserNotAuthenticated_ReturnsFalse`
  - `GetClaimsAsync_WhenApiReturnsError_ThrowsHttpRequestException`

### Test Structure

All tests follow the **Arrange-Act-Assert (AAA)** pattern:

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    var expectedValue = "test";
    _mockService.Setup(x => x.Method()).Returns(expectedValue);

    // Act
    var result = await _serviceUnderTest.MethodToTest();

    // Assert
    result.Should().Be(expectedValue);
}
```

## Test Helpers

### MockHttpMessageHandler
A custom HTTP message handler for mocking HTTP requests in unit tests:

```csharp
var handler = new MockHttpMessageHandler();
handler.SetupResponse(HttpStatusCode.OK, responseData);

var httpClient = new HttpClient(handler);
// Use httpClient in your tests

// Verify requests
handler.VerifyRequest(req => req.RequestUri.ToString().Contains("expected-url"));
```

### Test Data Builders
Use builder pattern for creating test data:

```csharp
var user = new AuthUserBuilder()
    .WithEmail("test@example.com")
    .WithDisplayName("Test User")
    .Build();

var claims = new ClaimBuilder()
    .AsAdminTier()
    .Build();
```

## Running Tests

### Recommended: Use the test scripts (automatically generates HTML coverage report)

**For Linux/macOS:**
```bash
./test.sh
```

**For Windows PowerShell:**
```powershell
./test.ps1
```

These scripts will:
1. Run all tests
2. Show pass/fail results
3. Automatically generate HTML coverage report
4. Display coverage summary

### Manual commands

**Run all tests (without HTML report):**
```bash
dotnet test
```

**Run tests with filter:**
```bash
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
```

**Run tests with code coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Code Coverage

### Quick Start

We've provided scripts to automatically generate HTML coverage reports:

**For Linux/macOS:**
```bash
./generate-coverage-report.sh
```

**For Windows PowerShell:**
```powershell
./generate-coverage-report.ps1
```

These scripts will:
1. Run all tests with code coverage
2. Generate an HTML report
3. Open the report in your default browser

### Manual Steps

If you prefer to run the commands manually:

1. Install ReportGenerator globally (one-time setup):
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

2. Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

3. Generate HTML report:
```bash
reportgenerator -reports:TestResults/*/coverage.cobertura.xml -targetdir:TestResults/CoverageReport -reporttypes:Html
```

4. Open the report:
   - Location: `TestResults/CoverageReport/index.html`

### Coverage Goals

- **Target**: >80% code coverage for all services
- **Current**: Check the latest report for current metrics
- **Excluded**: Auto-generated files (`*.g.cs`), UI components

## Best Practices

1. **Isolation**: Each test should be independent and not rely on other tests
2. **Mocking**: Mock external dependencies using Moq
3. **Assertions**: Use FluentAssertions for readable assertions
4. **Coverage**: Aim for >80% code coverage for services
5. **Edge Cases**: Test both positive and negative scenarios
6. **Async Testing**: Use `async/await` properly in tests
7. **Deterministic**: Tests should produce the same results every time

## Common Testing Scenarios

### Testing Services with HTTP Calls
```csharp
var handler = new MockHttpMessageHandler();
handler.SetupResponse(HttpStatusCode.OK, expectedData);
var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.com") };
```

### Testing Services with Caching
```csharp
var cache = new MemoryCache(new MemoryCacheOptions());
// Pre-populate cache if testing cache hits
cache.Set("key", cachedData);
```

### Testing Authentication States
```csharp
var identity = new ClaimsIdentity(claims, "TestAuth");
var principal = new ClaimsPrincipal(identity);
var authState = new AuthenticationState(principal);
```

## Continuous Integration

Tests are automatically run on:
- Pull request creation
- Commits to feature branches
- Merges to master branch

All tests must pass before merging to master.