# Comprehensive Testing Guide - GetFitterGetBigger Admin

This guide consolidates all testing knowledge for the Admin project, covering Blazor component testing with bUnit and API service testing with xUnit.

## 📑 Table of Contents
1. [Blazor Component Testing (bUnit)](#blazor-component-testing-bunit)
2. [API Service Testing (xUnit)](#api-service-testing-xunit)
3. [Common Testing Principles](#common-testing-principles)
4. [Quick Reference & Debugging](#quick-reference--debugging)

---

## 🔷 Blazor Component Testing (bUnit)

### 🎯 Key Principles

1. **Testability is a Design Consideration** - Components should be built with testing in mind from the start
2. **Use Specific Selectors** - Always use data-testid or specific identifiers to avoid element misidentification
3. **Test Multiple Ways** - Complex components benefit from both UI interaction tests and direct logic tests
4. **Handle Async Properly** - Blazor components are inherently async; tests must account for this

### 📋 Pre-Test Checklist

Before writing tests for a Blazor component:

- [ ] **Add data-testid attributes** to all interactive elements in the component
- [ ] **Make key methods and fields internal** (not private) for test accessibility
- [ ] **Ensure InternalsVisibleTo** is configured in the main project's .csproj file
- [ ] **Plan for both UI and logic tests** for comprehensive coverage

### 🏗️ Component Design for Testability

#### 1. Add Data Test IDs

```razor
<!-- Good: Specific test IDs for each interactive element -->
<select @bind="selectedRole" data-testid="muscle-group-role-select">
    <option value="">Select role</option>
</select>

<button @onclick="AddItem" data-testid="add-muscle-group-button">
    Add
</button>

<!-- Bad: No test IDs, generic elements -->
<select @bind="selectedRole">
<button @onclick="AddItem">Add</button>
```

#### 2. Use Internal Visibility

```csharp
@code {
    // Good: Internal fields/methods accessible to tests
    internal string selectedRole = string.Empty;
    internal async Task AddMuscleGroup() { }
    
    // Bad: Private fields/methods inaccessible to tests
    private string selectedRole = string.Empty;
    private async Task AddMuscleGroup() { }
}
```

#### 3. Configure Test Access

In your main project's .csproj:
```xml
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>GetFitterGetBigger.Admin.Tests</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>
```

### 🧪 Blazor Testing Patterns

#### Pattern 1: UI Interaction Testing

Tests the component through simulated user interactions:

```csharp
[Fact]
public async Task Component_UIInteraction_Test()
{
    // Arrange
    var component = RenderComponent<MyComponent>(parameters => parameters
        .Add(p => p.SomeParameter, value));

    // Act - Use data-testid to find elements
    component.Find("[data-testid='role-select']").Change("Primary");
    component.Find("[data-testid='item-select']").Change("1");
    
    // Handle async operations
    await component.InvokeAsync(() => 
        component.Find("[data-testid='add-button']").Click()
    );

    // Assert
    component.Find("[data-testid='result']").TextContent
        .Should().Be("Expected Result");
}
```

#### Pattern 2: Direct Logic Testing

Tests the component's business logic directly:

```csharp
[Fact]
public async Task Component_DirectLogic_Test()
{
    // Arrange
    var component = RenderComponent<MyComponent>();
    var instance = component.Instance;

    // Act - Directly set state and call methods
    instance.selectedRole = "Primary";
    instance.selectedItem = "1";
    
    await component.InvokeAsync(async () => 
    {
        await instance.AddItem();
    });

    // Assert
    instance.Items.Should().HaveCount(1);
    instance.Items[0].Role.Should().Be("Primary");
}
```

#### Pattern 3: Service Mock Testing

Testing components that depend on services:

```csharp
public class ComponentWithServiceTests : TestContext
{
    private readonly Mock<IExerciseService> _mockService;

    public ComponentWithServiceTests()
    {
        _mockService = new Mock<IExerciseService>();
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public async Task Component_LoadsDataFromService()
    {
        // Arrange
        var testData = new List<ExerciseDto> { 
            new() { Id = "1", Name = "Push-up" } 
        };
        _mockService.Setup(x => x.GetAllAsync())
            .ReturnsAsync(testData);

        // Act
        var component = RenderComponent<ExerciseList>();

        // Assert
        await component.WaitForElement("[data-testid='exercise-list']");
        component.FindAll("[data-testid='exercise-item']")
            .Should().HaveCount(1);
    }
}
```

#### Pattern 4: Form Testing

Testing form submissions and validation:

```csharp
[Fact]
public async Task Form_SubmitsWithValidData()
{
    // Arrange
    var submittedData = new ExerciseDto();
    var component = RenderComponent<ExerciseForm>(parameters => parameters
        .Add(p => p.OnSubmit, EventCallback.Factory.Create<ExerciseDto>(
            this, dto => submittedData = dto)));

    // Act - Fill form fields
    component.Find("[data-testid='exercise-name']").Change("Push-up");
    component.Find("[data-testid='exercise-type']").Change("Strength");
    
    // Submit form
    await component.InvokeAsync(() => 
        component.Find("form").Submit()
    );

    // Assert
    submittedData.Name.Should().Be("Push-up");
    submittedData.Type.Should().Be("Strength");
}

[Fact]
public void Form_ShowsValidationErrors()
{
    // Arrange
    var component = RenderComponent<ExerciseForm>();

    // Act - Submit empty form
    component.Find("form").Submit();

    // Assert
    component.Find("[data-testid='name-error']").TextContent
        .Should().Contain("Name is required");
}
```

### ⚠️ Blazor Testing Pitfalls

#### 1. Wrong Element Selected

**Problem**: Generic selectors find unexpected elements
```csharp
// Bad: Finds the first button, which might not be the one you want
component.Find("button").Click();
```

**Solution**: Use specific selectors
```csharp
// Good: Finds specific button by test ID
component.Find("[data-testid='add-button']").Click();

// Also good: Find by text content
component.FindAll("button").First(b => b.TextContent.Trim() == "Add").Click();

// Good: Find by CSS class
component.Find("button.add-button").Click();
```

#### 2. Protection Level Access Issues

**Problem**: Test can't access private component members
```
Error: 'Component.privateField' is inaccessible due to its protection level
```

**Solution**: 
1. Make fields/methods internal in component
2. Add InternalsVisibleTo in .csproj
3. Rebuild both projects

#### 3. Async Timing Issues

**Problem**: Test assertions run before async operations complete
```csharp
// Bad: Click is async but test doesn't wait
component.Find("button").Click();
Assert.Equal(1, items.Count); // Fails - async not complete
```

**Solution**: Use InvokeAsync and await
```csharp
// Good: Properly waits for async operations
await component.InvokeAsync(() => component.Find("button").Click());
Assert.Equal(1, items.Count); // Passes
```

#### 4. Child Component Interactions

**Problem**: Testing components with complex child components
```csharp
// This might not work with custom components
component.Find("select").Change("value");
```

**Solution**: 
```csharp
// Option 1: Find the actual rendered element within child
var enhancedSelect = component.FindComponent<EnhancedReferenceSelect<T>>();
await enhancedSelect.Instance.ValueChanged.InvokeAsync("value");

// Option 2: Set state directly
component.Instance.selectedValue = "value";
await component.Instance.AddItem();

// Option 3: Use data-testid on child component elements
component.Find("[data-testid='child-select']").Change("value");
```

### 🚨 Most Common Blazor Test Failures

#### 1. Component Test Setup Issues
```csharp
// ❌ NEVER DO THIS
var component = RenderComponent<MyComponent>(); // Missing required parameters!

// ✅ ALWAYS DO THIS
var component = RenderComponent<MyComponent>(parameters => parameters
    .Add(p => p.RequiredParam, value)
    .Add(p => p.OnCallback, EventCallback.Factory.Create(...)));
```

#### 2. Missing Service Registration
```csharp
// ❌ Component needs service but test doesn't provide it
var component = RenderComponent<ComponentNeedingService>();

// ✅ Register mock service before rendering
public class MyTests : TestContext
{
    public MyTests()
    {
        var mockService = new Mock<IMyService>();
        Services.AddSingleton(mockService.Object);
    }
}
```

#### 3. Async Component Loading
```csharp
// ❌ NEVER DO THIS
var items = component.FindAll(".item"); // May not be loaded yet!

// ✅ ALWAYS DO THIS
await component.WaitForElement(".item", TimeSpan.FromSeconds(2));
var items = component.FindAll(".item");

// OR use WaitForAssertion
await component.WaitForAssertion(() =>
    component.FindAll(".item").Count.Should().BeGreaterThan(0)
);
```

---

## 🔌 API Service Testing (xUnit)

### Testing Framework Setup

- **Framework:** xUnit
- **Mocking Library:** Moq
- **Assertion Library:** FluentAssertions
- **Project Name:** `GetFitterGetBigger.Admin.Tests`

### Key Scenarios to Test

#### 1. Happy Path (Successful API Call)
```csharp
[Fact]
public async Task GetExercisesAsync_WhenApiCallIsSuccessful_ReturnsData()
{
    // Arrange
    var mockHttp = new Mock<HttpMessageHandler>();
    var response = new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("[{\"id\": \"1\", \"name\": \"Push-up\"}]")
    };

    mockHttp.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(response);

    var httpClient = new HttpClient(mockHttp.Object) 
    { 
        BaseAddress = new Uri("http://test/") 
    };
    var mockCache = new Mock<IMemoryCache>();
    var service = new ExerciseService(httpClient, mockCache.Object);

    // Act
    var result = await service.GetExercisesAsync();

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(1);
    result.First().Name.Should().Be("Push-up");
}
```

#### 2. Error Response Testing
```csharp
[Fact]
public async Task GetExercisesAsync_WhenApiReturnsError_ThrowsException()
{
    // Arrange
    var mockHttp = new Mock<HttpMessageHandler>();
    mockHttp.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("Server error")
        });

    var httpClient = new HttpClient(mockHttp.Object);
    var service = new ExerciseService(httpClient, Mock.Of<IMemoryCache>());

    // Act & Assert
    await service.Invoking(s => s.GetExercisesAsync())
        .Should().ThrowAsync<HttpRequestException>();
}
```

#### 3. Caching Behavior Testing
```csharp
[Fact]
public async Task GetExercisesAsync_WhenCalledTwice_UsesCacheOnSecondCall()
{
    // Arrange
    var mockHttp = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    var response = new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("[{\"id\": \"1\"}]")
    };

    // Setup to verify only called once
    mockHttp.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(response)
        .Verifiable();

    var httpClient = new HttpClient(mockHttp.Object);
    var cache = new MemoryCache(new MemoryCacheOptions());
    var service = new ExerciseService(httpClient, cache);

    // Act
    var result1 = await service.GetExercisesAsync();
    var result2 = await service.GetExercisesAsync();

    // Assert
    result1.Should().BeEquivalentTo(result2);
    mockHttp.Protected().Verify(
        "SendAsync",
        Times.Once(), // Only called once due to caching
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
    );
}
```

#### 4. Data Transformation Testing
```csharp
[Fact]
public async Task CreateExerciseAsync_TransformsDataCorrectly()
{
    // Arrange
    var createDto = new CreateExerciseDto
    {
        Name = "Push-up",
        Type = "Strength",
        MuscleGroups = new[] { "Chest", "Arms" }
    };

    var mockHttp = new Mock<HttpMessageHandler>();
    HttpRequestMessage capturedRequest = null;

    mockHttp.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .Callback<HttpRequestMessage, CancellationToken>((req, _) => 
            capturedRequest = req)
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Created,
            Content = new StringContent("{\"id\": \"123\"}")
        });

    var httpClient = new HttpClient(mockHttp.Object);
    var service = new ExerciseService(httpClient, Mock.Of<IMemoryCache>());

    // Act
    await service.CreateExerciseAsync(createDto);

    // Assert
    capturedRequest.Should().NotBeNull();
    var content = await capturedRequest.Content.ReadAsStringAsync();
    content.Should().Contain("Push-up");
    content.Should().Contain("Strength");
}
```

---

## 🎯 Common Testing Principles

### Test All Scenarios, Not Just Happy Path

Every component and function should be tested for:
- **Happy Path**: Normal, expected behavior
- **Edge Cases**: Empty states, loading states, error boundaries
- **Error Cases**: API failures, network errors, invalid inputs
- **User Interactions**: Clicks, form submissions, keyboard navigation
- **Accessibility**: Screen reader compatibility, keyboard-only navigation

### Areas Requiring Test Coverage

#### 1. Component Testing
- **Rendering Logic**: All conditional renders based on parameters/state
- **Event Handlers**: Button clicks, form submissions, value changes
- **Lifecycle**: OnInitializedAsync, OnParametersSetAsync, IDisposable
- **Error Boundaries**: Component error handling with try-catch

#### 2. Integration Testing
- **API Integration**: Mock API responses for all scenarios
- **Navigation**: NavigationManager usage
- **Authentication**: AuthenticationStateProvider integration
- **State Management**: Cascading values and parameters

#### 3. Accessibility Testing
- **ARIA Labels**: Proper labeling for screen readers
- **Keyboard Navigation**: Tab order, focus management
- **Form Labels**: All inputs properly labeled
- **Error Messages**: Associated with form fields

### Comprehensive Test Example

```csharp
public class ExerciseFormTests : TestContext
{
    private readonly Mock<IExerciseService> _mockService;
    private readonly Mock<INavigationManager> _mockNav;

    public ExerciseFormTests()
    {
        _mockService = new Mock<IExerciseService>();
        _mockNav = new Mock<INavigationManager>();
        
        Services.AddSingleton(_mockService.Object);
        Services.AddSingleton(_mockNav.Object);
    }

    [Fact]
    public void ExerciseForm_RendersAllFields() // Happy path
    {
        var component = RenderComponent<ExerciseForm>();
        
        component.Find("[data-testid='name-input']").Should().NotBeNull();
        component.Find("[data-testid='type-select']").Should().NotBeNull();
        component.Find("[data-testid='submit-button']").Should().NotBeNull();
    }

    [Fact]
    public void ExerciseForm_ValidatesRequiredFields() // Validation
    {
        var component = RenderComponent<ExerciseForm>();
        
        component.Find("form").Submit();
        
        component.Find("[data-testid='name-error']").TextContent
            .Should().Contain("required");
    }

    [Fact]
    public async Task ExerciseForm_HandlesApiError() // Error case
    {
        _mockService.Setup(x => x.CreateAsync(It.IsAny<CreateExerciseDto>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
            
        var component = RenderComponent<ExerciseForm>();
        component.Find("[data-testid='name-input']").Change("Test");
        
        await component.InvokeAsync(() => 
            component.Find("form").Submit()
        );
        
        component.Find("[data-testid='error-message']").TextContent
            .Should().Contain("error occurred");
    }

    [Fact]
    public async Task ExerciseForm_DisablesSubmitDuringApiCall() // Loading state
    {
        var tcs = new TaskCompletionSource<ExerciseDto>();
        _mockService.Setup(x => x.CreateAsync(It.IsAny<CreateExerciseDto>()))
            .Returns(tcs.Task);
            
        var component = RenderComponent<ExerciseForm>();
        component.Find("[data-testid='name-input']").Change("Test");
        
        var submitTask = component.InvokeAsync(() => 
            component.Find("form").Submit()
        );
        
        // Button should be disabled during submission
        component.Find("[data-testid='submit-button']")
            .GetAttribute("disabled").Should().NotBeNull();
            
        tcs.SetResult(new ExerciseDto());
        await submitTask;
    }

    [Fact]
    public void ExerciseForm_SupportsKeyboardNavigation() // Accessibility
    {
        var component = RenderComponent<ExerciseForm>();
        
        var firstInput = component.Find("[data-testid='name-input']");
        firstInput.GetAttribute("tabindex").Should().NotBe("-1");
        
        var submitButton = component.Find("[data-testid='submit-button']");
        submitButton.GetAttribute("aria-label").Should().NotBeNull();
    }
}
```

---

## 🔍 Quick Reference & Debugging

### 🔍 Debugging Checklist

When tests fail, check IN THIS ORDER:
1. **Component Parameters** - Are all required parameters provided?
2. **Service Registration** - Did you register ALL required services?
3. **Async Handling** - Are you using InvokeAsync for async operations?
4. **Element Selection** - Are you using the right selectors?
5. **Protection Levels** - Can the test access the component internals?
6. **Timing Issues** - Do you need WaitForElement or WaitForAssertion?

### Debugging Test Failures

```csharp
// Check what's actually rendered
Console.WriteLine(component.Markup);

// Pretty print for better readability
Console.WriteLine(component.Instance.GetType().Name);
Console.WriteLine(new string('-', 50));
Console.WriteLine(component.Markup);

// Verify element attributes
var button = component.Find("button");
Console.WriteLine($"Button HTML: {button.ToMarkup()}");
Console.WriteLine($"Disabled: {button.GetAttribute("disabled")}");
Console.WriteLine($"Classes: {button.GetClasses()}");

// Check component state
var instance = component.Instance;
Console.WriteLine($"Field value: {instance.someField}");

// List all elements of a type
var allButtons = component.FindAll("button");
Console.WriteLine($"Found {allButtons.Count} buttons:");
foreach (var btn in allButtons)
{
    Console.WriteLine($"  - Text: '{btn.TextContent.Trim()}' Classes: '{btn.GetClasses()}'");
}

// Check if element exists
try 
{
    var element = component.Find("[data-testid='maybe-exists']");
    Console.WriteLine("Element found!");
}
catch (ElementNotFoundException)
{
    Console.WriteLine("Element not found!");
}
```

### 🛠️ Useful Commands

#### .NET/Blazor Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test GetFitterGetBigger.Admin.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~ExerciseFormTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ExerciseFormTests.ExerciseForm_RendersAllFields"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run in verbose mode (see Console output)
dotnet test -v normal

# Run with detailed logs
dotnet test --logger "console;verbosity=detailed"

# Watch mode (re-run on file changes)
dotnet watch test
```

### 📊 Testing Tools Summary

#### Blazor/C# Stack
- **xUnit**: Test framework
- **bUnit**: Blazor component testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Expressive assertion library
- **Coverlet**: Code coverage tool

#### Common Test Patterns
```csharp
// Test class setup
public class MyComponentTests : TestContext
{
    private readonly Mock<IMyService> _mockService;

    public MyComponentTests()
    {
        _mockService = new Mock<IMyService>();
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public async Task TestName_Scenario_ExpectedResult()
    {
        // Arrange
        _mockService.Setup(x => x.Method()).ReturnsAsync(data);
        
        // Act
        var component = RenderComponent<MyComponent>();
        
        // Assert
        component.Find(".result").TextContent.Should().Be("expected");
    }
}
```

---

## 🔄 State Service Testing

State services in the Admin application follow specific patterns that require careful testing consideration.

### Key Testing Scenarios for State Services

#### 1. Error Message Persistence During Rollback

When testing operations that may fail and require rollback (e.g., optimistic updates), ensure error messages persist:

```csharp
[Fact]
public async Task CreateOperation_OnDuplicateError_PreservesErrorMessage()
{
    // Arrange
    var exerciseId = Guid.NewGuid().ToString();
    var initialState = new StateBuilder().WithExerciseId(exerciseId).Build();
    
    _mockService.SetupSequence(x => x.GetDataAsync(exerciseId))
        .ReturnsAsync(initialState)
        .ReturnsAsync(initialState); // For reload after error
    
    _mockService.Setup(x => x.CreateAsync(exerciseId, It.IsAny<CreateDto>()))
        .ThrowsAsync(new DuplicateException());
    
    await _stateService.InitializeAsync(exerciseId);
    
    // Act
    await _stateService.CreateAsync(new CreateDto());
    
    // Assert - Error message should persist after rollback
    _stateService.ErrorMessage.Should().Be("This item already exists");
    _stateService.CurrentState.Should().BeEquivalentTo(initialState);
}
```

#### 2. Optimistic Update and Rollback Testing

```csharp
[Fact]
public async Task UpdateOperation_OptimisticUpdate_RollsBackOnError()
{
    // Arrange
    var originalItem = new ItemBuilder().WithId("1").WithName("Original").Build();
    var items = new List<Item> { originalItem };
    
    _mockService.Setup(x => x.GetItemsAsync())
        .ReturnsAsync(items);
    
    _mockService.Setup(x => x.UpdateAsync("1", It.IsAny<UpdateDto>()))
        .ThrowsAsync(new ApiException("Update failed"));
    
    await _stateService.InitializeAsync();
    var originalCount = _stateService.Items.Count;
    
    // Act
    await _stateService.UpdateItemAsync("1", new UpdateDto { Name = "Updated" });
    
    // Assert - State should be rolled back
    _stateService.Items.Should().HaveCount(originalCount);
    _stateService.Items.First().Name.Should().Be("Original");
    _stateService.ErrorMessage.Should().Contain("Update failed");
}
```

#### 3. State Change Notification Testing

```csharp
[Fact]
public async Task StateChanges_NotifySubscribers()
{
    // Arrange
    var notifications = 0;
    _stateService.OnChange += () => notifications++;
    
    // Act
    await _stateService.LoadDataAsync();
    _stateService.ClearError();
    await _stateService.CreateItemAsync(new CreateDto());
    
    // Assert
    notifications.Should().BeGreaterThan(3); // Multiple state changes
}
```

### State Service Testing Best Practices

1. **Always test error message persistence** when operations may fail
2. **Verify optimistic updates are properly rolled back** on errors
3. **Test state change notifications** are fired appropriately
4. **Mock API calls with SetupSequence** when testing reload scenarios
5. **Test loading states** transition correctly (IsLoading true → false)
6. **Verify success messages** are set and cleared appropriately
7. **Test concurrent operations** are handled correctly
8. **Ensure proper cleanup** in Dispose methods

### Common State Service Testing Pattern

```csharp
public class StateServiceTests
{
    private readonly Mock<IApiService> _apiServiceMock;
    private readonly StateService _stateService;
    private int _stateChangeCount;
    
    public StateServiceTests()
    {
        _apiServiceMock = new Mock<IApiService>();
        _stateService = new StateService(_apiServiceMock.Object);
        _stateService.OnChange += () => _stateChangeCount++;
        _stateChangeCount = 0;
    }
    
    // Helper to verify state changes occurred
    private void AssertStateChanged(int minimumChanges = 1)
    {
        _stateChangeCount.Should().BeGreaterThanOrEqualTo(minimumChanges);
    }
}
```

---

## 📝 Best Practices Summary

1. **Design for testability** - Use data-testid, internal visibility, clear separation of concerns
2. **Use specific selectors** - Avoid generic element selectors that might find wrong elements
3. **Test both ways** - UI interaction tests AND direct logic tests
4. **Handle async properly** - Always use InvokeAsync for async operations
5. **Keep tests maintainable** - Clear names, good organization, single responsibility
6. **Debug systematically** - Use Console.WriteLine to understand what's happening
7. **Mock at the right level** - Mock external dependencies, not internal implementation
8. **Test edge cases** - Empty states, errors, loading, large data sets
9. **Use FluentAssertions** - More readable assertions with better error messages
10. **Isolate tests** - Each test should be independent and repeatable

---

## 🎉 Key Takeaway

**Remember**: The goal is not just to achieve high test coverage, but to create tests that:
- Give confidence in your component's behavior
- Are maintainable over time
- Serve as living documentation
- Catch regressions early
- Make refactoring safer

When tests fail, it's often the test setup, not the component code! Use the debugging techniques to understand what's actually happening before changing your component code.