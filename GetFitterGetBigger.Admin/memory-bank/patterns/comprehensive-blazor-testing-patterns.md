# Comprehensive Blazor Testing Patterns

## Overview

This guide documents the testing patterns and best practices demonstrated in the GetFitterGetBigger Admin project, particularly from the excellent Four-Way Exercise Linking feature implementation.

## Table of Contents

1. [Test Organization](#test-organization)
2. [Component Testing with bUnit](#component-testing-with-bunit)
3. [Accessibility Testing](#accessibility-testing)
4. [Performance Testing](#performance-testing)
5. [State Management Testing](#state-management-testing)
6. [Mock Patterns](#mock-patterns)
7. [Common Pitfalls and Solutions](#common-pitfalls-and-solutions)

## Test Organization

### File Structure

```
Tests/
├── Components/
│   └── Pages/
│       └── Exercises/
│           └── ExerciseLinks/
│               ├── AlternativeExerciseCardTests.cs         # Component-specific tests
│               ├── ExerciseContextSelectorTests.cs         # Navigation and interaction
│               ├── FourWayLinkedExercisesListTests.cs      # Complex state management
│               ├── FourWayLinkingAccessibilityTests.cs     # WCAG compliance
│               ├── FourWayLinkingPerformanceTests.cs       # Performance benchmarks
│               └── ShouldRenderOptimizationTests.cs        # Render optimization
```

### Test Class Pattern

```csharp
public class ComponentNameTests : TestContext, IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IServiceName> _mockService;

    public ComponentNameTests(ITestOutputHelper output)
    {
        _output = output;
        _mockService = new Mock<IServiceName>();
        
        // Register services
        Services.AddSingleton(_mockService.Object);
        
        // Setup default mock behavior
        ConfigureDefaultMocks();
    }

    private void ConfigureDefaultMocks()
    {
        // Setup common mock behaviors
    }

    // Test methods...

    public new void Dispose()
    {
        base.Dispose();
        // Additional cleanup if needed
    }
}
```

## Component Testing with bUnit

### Basic Component Rendering

```csharp
[Fact]
public void Component_RendersCorrectly()
{
    // Arrange
    var parameters = ComponentParameterCollectionBuilder<MyComponent>
        .Empty
        .Add(p => p.RequiredParam, "value")
        .Add(p => p.OptionalParam, 123)
        .Build();

    // Act
    var component = RenderComponent<MyComponent>(parameters);

    // Assert
    Assert.NotNull(component.Find("[data-testid='expected-element']"));
    component.Find("h1").TextContent.Should().Be("Expected Title");
}
```

### Testing User Interactions

```csharp
[Fact]
public async Task Component_HandlesButtonClick()
{
    // Arrange
    var clicked = false;
    var component = RenderComponent<MyComponent>(p => p
        .Add(c => c.OnClick, EventCallback.Factory.Create(this, () => clicked = true)));

    // Act
    var button = component.Find("[data-testid='action-button']");
    await button.ClickAsync();

    // Assert
    clicked.Should().BeTrue();
}
```

### Testing Async Operations

```csharp
[Fact]
public async Task Component_LoadsDataAsync()
{
    // Arrange
    var tcs = new TaskCompletionSource<List<Item>>();
    _mockService.Setup(s => s.GetItemsAsync())
        .Returns(tcs.Task);

    // Act
    var component = RenderComponent<MyComponent>();
    
    // Assert - Loading state
    component.Find("[data-testid='loading-spinner']").Should().NotBeNull();
    
    // Complete async operation
    tcs.SetResult(new List<Item> { new Item { Name = "Test" } });
    
    // Wait for render
    await component.InvokeAsync(() => { });
    
    // Assert - Loaded state
    component.FindAll("[data-testid='item']").Should().HaveCount(1);
}
```

### Testing Parameter Changes

```csharp
[Fact]
public void Component_ReactsToParameterChanges()
{
    // Arrange
    var component = RenderComponent<MyComponent>(p => p
        .Add(c => c.Value, "initial"));

    // Act
    component.SetParametersAndRender(p => p
        .Add(c => c.Value, "updated"));

    // Assert
    component.Find("[data-testid='value-display']")
        .TextContent.Should().Be("updated");
}
```

## Accessibility Testing

### WCAG Compliance Pattern

```csharp
public class AccessibilityTests : TestContext
{
    [Fact]
    public void Component_HasProperAriaLabels()
    {
        // Arrange & Act
        var component = RenderComponent<AccessibleComponent>();

        // Assert - Check ARIA attributes
        var button = component.Find("button");
        button.GetAttribute("aria-label").Should().NotBeNullOrEmpty();
        button.GetAttribute("aria-pressed").Should().BeOneOf("true", "false");
        
        var region = component.Find("[role='region']");
        region.GetAttribute("aria-labelledby").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Component_SupportsKeyboardNavigation()
    {
        // Arrange
        var component = RenderComponent<NavigableComponent>();
        var element = component.Find("[data-testid='focusable-element']");

        // Act - Simulate keyboard events
        var keyboardEvent = new KeyboardEventArgs
        {
            Key = "Enter",
            Code = "Enter"
        };
        await element.KeyDownAsync(keyboardEvent);

        // Assert
        component.Instance.KeyPressed.Should().Be("Enter");
    }

    [Fact]
    public void Component_MaintainsFocusManagement()
    {
        // Test focus trap, focus restoration, etc.
        var component = RenderComponent<FocusManagementComponent>();
        
        // Simulate tab navigation
        var firstFocusable = component.Find("[data-testid='first-focusable']");
        var lastFocusable = component.Find("[data-testid='last-focusable']");
        
        // Verify focus wrapping
        await lastFocusable.KeyDownAsync(new KeyboardEventArgs { Key = "Tab" });
        // Assert focus moved to first element
    }
}
```

### Screen Reader Testing

```csharp
[Fact]
public void Component_ProvidesScreenReaderContext()
{
    // Arrange & Act
    var component = RenderComponent<ScreenReaderFriendlyComponent>();

    // Assert - Live regions
    var liveRegion = component.Find("[aria-live]");
    liveRegion.GetAttribute("aria-live").Should().BeOneOf("polite", "assertive");
    
    // Assert - Descriptive text
    var complexControl = component.Find("[aria-describedby]");
    var description = component.Find($"#{complexControl.GetAttribute("aria-describedby")}");
    description.TextContent.Should().NotBeNullOrEmpty();
}
```

## Performance Testing

### Render Performance Pattern

```csharp
public class PerformanceTests : TestContext
{
    private readonly ITestOutputHelper _output;

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Component_ScalesWithDataSize(int itemCount)
    {
        // Arrange
        var items = GenerateTestItems(itemCount);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var component = RenderComponent<ListComponent>(p => p
            .Add(c => c.Items, items));
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Render time for {itemCount} items: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(itemCount * 2); // Linear scaling
    }

    [Fact]
    public void Component_MinimizesRerenders()
    {
        // Arrange
        var component = RenderComponent<OptimizedComponent>();
        var initialRenderCount = GetRenderCount(component);

        // Act - Trigger multiple state changes
        for (int i = 0; i < 100; i++)
        {
            component.Instance.UpdateNonVisualState(i);
        }

        // Assert - Should have minimal re-renders
        var finalRenderCount = GetRenderCount(component);
        (finalRenderCount - initialRenderCount).Should().BeLessThan(10);
    }

    private int GetRenderCount(IRenderedComponent<OptimizedComponent> component)
    {
        // Use reflection or test-specific counter
        return component.RenderCount;
    }
}
```

### Memory Usage Testing

```csharp
[Fact]
public void Component_DisposesResourcesProperly()
{
    // Arrange
    var initialMemory = GC.GetTotalMemory(true);
    IRenderedComponent<ResourceIntensiveComponent>? component = null;

    // Act
    for (int i = 0; i < 100; i++)
    {
        component?.Dispose();
        component = RenderComponent<ResourceIntensiveComponent>();
    }

    component?.Dispose();
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var finalMemory = GC.GetTotalMemory(true);

    // Assert - No significant memory leak
    var memoryIncrease = finalMemory - initialMemory;
    memoryIncrease.Should().BeLessThan(1024 * 1024); // Less than 1MB increase
}
```

## State Management Testing

### Testing State Service Integration

```csharp
public class StateManagementTests : TestContext
{
    private readonly Mock<IStateService> _mockStateService;

    [Fact]
    public void Component_ReactsToStateChanges()
    {
        // Arrange
        _mockStateService.Setup(s => s.CurrentValue).Returns("initial");
        var component = RenderComponent<StateAwareComponent>();

        // Act - Trigger state change
        _mockStateService.Setup(s => s.CurrentValue).Returns("updated");
        _mockStateService.Raise(s => s.OnChange += null);

        // Assert
        component.InvokeAsync(() =>
        {
            component.Find("[data-testid='state-display']")
                .TextContent.Should().Be("updated");
        });
    }

    [Fact]
    public void Component_CleansUpStateSubscriptions()
    {
        // Arrange
        var subscribeCount = 0;
        var unsubscribeCount = 0;
        
        _mockStateService.SetupAdd(s => s.OnChange += It.IsAny<Action>())
            .Callback(() => subscribeCount++);
        _mockStateService.SetupRemove(s => s.OnChange -= It.IsAny<Action>())
            .Callback(() => unsubscribeCount++);

        // Act
        var component = RenderComponent<StateAwareComponent>();
        component.Dispose();

        // Assert
        subscribeCount.Should().Be(1);
        unsubscribeCount.Should().Be(1);
    }
}
```

## Mock Patterns

### Service Mock Configuration

```csharp
private void ConfigureDefaultMocks()
{
    // State service mock
    _mockStateService.Setup(s => s.IsLoading).Returns(false);
    _mockStateService.Setup(s => s.ErrorMessage).Returns((string?)null);
    _mockStateService.Setup(s => s.Items).Returns(new List<Item>());

    // API service mock
    _mockApiService.Setup(s => s.GetAsync(It.IsAny<string>()))
        .ReturnsAsync(new ApiResponse { Success = true });

    // Validation service mock
    _mockValidationService.Setup(s => s.Validate(It.IsAny<object>()))
        .Returns(ValidationResult.Success());
}
```

### Async Operation Mocking

```csharp
[Fact]
public async Task Component_HandlesAsyncErrors()
{
    // Arrange
    var tcs = new TaskCompletionSource<ApiResponse>();
    _mockApiService.Setup(s => s.GetAsync(It.IsAny<string>()))
        .Returns(tcs.Task);

    // Act
    var component = RenderComponent<AsyncComponent>();
    
    // Simulate error
    tcs.SetException(new ApiException("Network error"));
    
    // Wait for error handling
    await component.InvokeAsync(() => { });

    // Assert
    component.Find("[data-testid='error-message']")
        .TextContent.Should().Contain("Network error");
}
```

## Common Pitfalls and Solutions

### Pitfall 1: Not Using InvokeAsync for Async Operations

```csharp
// ❌ Bad - May cause timing issues
_mockService.Raise(s => s.OnChange += null);
Assert.Equal("expected", component.Find("div").TextContent);

// ✅ Good - Ensures proper async handling
await component.InvokeAsync(() => 
{
    _mockService.Raise(s => s.OnChange += null);
});
Assert.Equal("expected", component.Find("div").TextContent);
```

### Pitfall 2: Testing Implementation Instead of Behavior

```csharp
// ❌ Bad - Testing internal implementation
var privateField = component.Instance.GetType()
    .GetField("_internalState", BindingFlags.NonPublic);
Assert.Equal("expected", privateField.GetValue(component.Instance));

// ✅ Good - Testing observable behavior
component.Find("[data-testid='state-indicator']")
    .TextContent.Should().Be("expected");
```

### Pitfall 3: Not Cleaning Up Resources

```csharp
// ❌ Bad - May cause test interference
public class BadTests : TestContext
{
    private static Mock<IService> _sharedMock = new();
    // Tests using shared state...
}

// ✅ Good - Isolated test state
public class GoodTests : TestContext, IDisposable
{
    private readonly Mock<IService> _mockService;
    
    public GoodTests()
    {
        _mockService = new Mock<IService>();
    }
    
    public new void Dispose()
    {
        base.Dispose();
        // Clean up test-specific resources
    }
}
```

### Pitfall 4: Ignoring Test Warnings

```csharp
// ❌ Bad - Suppressing warnings
#pragma warning disable CS0618
    var result = component.Instance.DeprecatedMethod();
#pragma warning restore CS0618

// ✅ Good - Addressing the root cause
var result = component.Instance.UpdatedMethod();
```

## Test Data Builders

### Fluent Builder Pattern

```csharp
public class ExerciseBuilder
{
    private ExerciseDto _exercise = new();

    public static ExerciseBuilder Create() => new();

    public ExerciseBuilder WithName(string name)
    {
        _exercise.Name = name;
        return this;
    }

    public ExerciseBuilder WithType(params string[] types)
    {
        _exercise.ExerciseTypes = types.Select(t => new IdNameDto { Value = t }).ToList();
        return this;
    }

    public ExerciseBuilder AsWarmup() => WithType("Warmup");
    public ExerciseBuilder AsWorkout() => WithType("Workout");
    
    public ExerciseDto Build() => _exercise;
}

// Usage
var exercise = ExerciseBuilder.Create()
    .WithName("Burpees")
    .AsWorkout()
    .Build();
```

## Assertion Helpers

### Custom Assertion Extensions

```csharp
public static class ComponentAssertions
{
    public static void ShouldHaveTestId(this IRenderedFragment component, string testId)
    {
        component.Find($"[data-testid='{testId}']").Should().NotBeNull();
    }

    public static void ShouldBeAccessible(this IElement element)
    {
        // Check for required accessibility attributes
        var role = element.GetAttribute("role");
        var ariaLabel = element.GetAttribute("aria-label");
        var ariaLabelledBy = element.GetAttribute("aria-labelledby");
        
        (ariaLabel ?? ariaLabelledBy).Should().NotBeNullOrEmpty(
            "Element should have accessible label");
    }

    public static async Task ShouldCompleteWithinAsync(
        this Task task, int milliseconds, string reason)
    {
        var completed = await Task.WhenAny(task, Task.Delay(milliseconds)) == task;
        completed.Should().BeTrue(reason);
    }
}
```

## Conclusion

These testing patterns, extracted from the GetFitterGetBigger Admin project, demonstrate professional-grade Blazor testing practices. Key takeaways:

1. **Comprehensive Coverage**: Test functionality, accessibility, performance, and edge cases
2. **Proper Isolation**: Each test is independent with its own mocks and state
3. **Behavioral Testing**: Focus on user-observable behavior, not implementation
4. **Performance Awareness**: Include performance tests for critical paths
5. **Accessibility First**: Ensure components are accessible from the start

Following these patterns ensures maintainable, reliable, and high-quality Blazor applications.