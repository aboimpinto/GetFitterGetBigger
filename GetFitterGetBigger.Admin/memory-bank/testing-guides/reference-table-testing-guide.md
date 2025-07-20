# Reference Table Testing Guide

This guide describes the testing patterns for the three types of reference tables in the GetFitterGetBigger Admin project.

## Types of Reference Tables

### 1. Pure ReadOnly Reference Tables (e.g., BodyParts)
- Simple value/description tables
- Never change after initial setup
- Display as basic table with two columns
- No CRUD operations

### 2. ReadOnly with Extended Data (e.g., ExerciseWeightTypes, Workout Reference Data)
- More complex data beyond value/description
- Display in card-based layouts
- Include icons, colors, validation rules, etc.
- No CRUD operations but richer display

### 3. Maintainable Reference Tables (e.g., Equipment, MuscleGroups)
- Full CRUD operations
- User can add, edit, delete records
- May have relationships with other tables
- More complex state management

## Testing Patterns

### Pattern 1: Card-Based Reference Tables (Type 2)

These reference tables are integrated into `ReferenceTableDetail.razor` and display data in card grids.

#### Component Location
- Component logic: `/Components/Pages/ReferenceTableDetail.razor`
- Test location: `/Tests/Components/Pages/[TableName]Tests.cs`

#### Test Setup
```csharp
public class WorkoutReferenceDataTests : TestContext
{
    private readonly Mock<IReferenceDataService> _referenceDataServiceMock;
    private readonly Mock<IEquipmentStateService> _equipmentStateServiceMock;
    private readonly Mock<IMuscleGroupsStateService> _muscleGroupsStateServiceMock;
    private readonly Mock<IExerciseWeightTypeStateService> _exerciseWeightTypeStateServiceMock;
    private readonly Mock<IWorkoutReferenceDataStateService> _workoutReferenceDataStateServiceMock;
    private readonly Mock<NavigationManager> _navigationManagerMock;

    public WorkoutReferenceDataTests()
    {
        // Mock all required services
        _referenceDataServiceMock = new Mock<IReferenceDataService>();
        _equipmentStateServiceMock = new Mock<IEquipmentStateService>();
        _muscleGroupsStateServiceMock = new Mock<IMuscleGroupsStateService>();
        _exerciseWeightTypeStateServiceMock = new Mock<IExerciseWeightTypeStateService>();
        _workoutReferenceDataStateServiceMock = new Mock<IWorkoutReferenceDataStateService>();
        _navigationManagerMock = new Mock<NavigationManager>();

        // Register all services
        Services.AddSingleton(_referenceDataServiceMock.Object);
        Services.AddSingleton(_equipmentStateServiceMock.Object);
        Services.AddSingleton(_muscleGroupsStateServiceMock.Object);
        Services.AddSingleton(_exerciseWeightTypeStateServiceMock.Object);
        Services.AddSingleton(_workoutReferenceDataStateServiceMock.Object);
        Services.AddSingleton(_navigationManagerMock.Object);
        
        // Add authorization
        this.AddTestAuthorization().SetAuthorized("test-user");
    }
}
```

#### Key Testing Requirements

1. **Always use data-testid attributes**
   ```razor
   <div data-testid="objectives-grid">
   <div data-testid="objective-card">
   <button data-testid="objectives-retry-button">Retry</button>
   ```

2. **Render through ReferenceTableDetail**
   ```csharp
   var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
       .Add(p => p.TableName, "WorkoutObjectives"));
   ```

3. **Mock the specific state service methods**
   ```csharp
   _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(objectives);
   _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
   _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns((string?)null);
   ```

#### Essential Test Cases

1. **Title and Description Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_RendersCorrectTitle()
   {
       // Arrange - mock data
       // Act - render component
       // Assert
       component.Find("h2").TextContent.Should().Be("Workout Objectives");
       component.Find("p").TextContent.Should().Be("View workout objectives for training programs");
   }
   ```

2. **Loading State Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_ShowsLoadingState()
   {
       // Arrange
       _mockStateService.Setup(x => x.IsLoadingObjectives).Returns(true);
       
       // Act & Assert
       component.Find(".animate-spin").Should().NotBeNull();
       loadingTexts.Any(p => p.TextContent.Contains("Loading...")).Should().BeTrue();
   }
   ```

3. **Error State Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_DisplaysErrorMessage()
   {
       // Arrange
       _mockStateService.Setup(x => x.ObjectivesError).Returns("Failed to load");
       
       // Assert
       var errorDiv = component.Find(".bg-red-50");
       errorDiv.QuerySelector("[data-testid='objectives-retry-button']").Should().NotBeNull();
   }
   ```

4. **Data Display Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_DisplaysObjectivesInCards()
   {
       // Assert card structure
       var cards = component.FindAll("[data-testid='objective-card']");
       cards.Should().HaveCount(3);
       
       var firstCard = cards[0];
       firstCard.QuerySelector("h3")!.TextContent.Should().Be("Muscle Building");
   }
   ```

5. **Empty State Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_ShowsEmptyStateWhenNoData()
   {
       // Arrange
       _mockStateService.Setup(x => x.FilteredWorkoutObjectives).Returns(new List<ReferenceDataDto>());
       
       // Assert
       component.Find(".text-center").TextContent.Should().Contain("No workout objectives found.");
   }
   ```

6. **Retry Action Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_CallsLoadObjectivesOnRetryButton()
   {
       // Act
       var retryButton = component.Find("[data-testid='objectives-retry-button']");
       retryButton.Click();
       
       // Assert
       _mockStateService.Verify(x => x.LoadWorkoutObjectivesAsync(), Times.Once);
   }
   ```

7. **Responsive Layout Test**
   ```csharp
   [Fact]
   public void WorkoutObjectives_ResponsiveGridLayout()
   {
       // Assert
       var gridContainer = component.Find("[data-testid='objectives-grid']");
       (gridContainer.GetAttribute("class") ?? "").Should()
           .Contain("grid-cols-1")
           .And.Contain("md:grid-cols-2")
           .And.Contain("lg:grid-cols-3");
   }
   ```

### Common Pitfalls to Avoid

1. **Never use generic selectors**
   ```csharp
   // BAD
   component.Find("button").Click();
   
   // GOOD
   component.Find("[data-testid='retry-button']").Click();
   ```

2. **Always mock all required services**
   - Even if your test doesn't use a service, ReferenceTableDetail requires it
   - Missing services will cause InvalidOperationException

3. **Use the correct component**
   ```csharp
   // BAD - Don't render the sub-component directly
   var component = RenderComponent<WorkoutObjectives>();
   
   // GOOD - Render through ReferenceTableDetail
   var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
       .Add(p => p.TableName, "WorkoutObjectives"));
   ```

4. **Match the exact text format**
   ```csharp
   // Be aware of whitespace and formatting
   firstCard.TextContent.Should().Contain("Time-Based:");
   firstCard.TextContent.Should().Contain("Yes");
   // NOT: firstCard.TextContent.Should().Contain("Time-Based: Yes");
   ```

### Test Organization

- Group related tests in regions
- Name tests descriptively: `[ComponentName]_[Action]_[ExpectedResult]`
- Keep tests focused on single behaviors
- Use consistent arrange/act/assert pattern

### Debugging Failed Tests

1. **Check data-testid attributes exist in component**
2. **Verify all services are mocked and registered**
3. **Print component markup for debugging**
   ```csharp
   Console.WriteLine(component.Markup);
   ```
4. **Check for exact text matches including whitespace**
5. **Ensure state service methods return appropriate values**

### Example: Complete Test Structure

```csharp
#region Workout Objectives Tests

[Fact]
public void WorkoutObjectives_RendersCorrectTitle() { }

[Fact]
public void WorkoutObjectives_ShowsLoadingState() { }

[Fact]
public void WorkoutObjectives_DisplaysErrorMessage() { }

[Fact]
public void WorkoutObjectives_DisplaysObjectivesInCards() { }

[Fact]
public void WorkoutObjectives_ShowsEmptyStateWhenNoData() { }

[Fact]
public void WorkoutObjectives_CallsLoadObjectivesOnRetryButton() { }

[Fact]
public void WorkoutObjectives_ResponsiveGridLayout() { }

#endregion
```

## Summary

The key to successful reference table testing is:
1. Understanding which type of reference table you're testing
2. Using the correct component architecture (ReferenceTableDetail for Type 2)
3. Always using data-testid attributes for element selection
4. Mocking all required services completely
5. Following established patterns consistently