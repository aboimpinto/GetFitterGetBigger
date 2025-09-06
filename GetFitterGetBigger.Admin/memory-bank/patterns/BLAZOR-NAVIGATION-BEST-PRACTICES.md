# Blazor Navigation Best Practices

## Overview
This document outlines best practices for implementing navigation in Blazor applications, particularly for Interactive Server render mode, based on real-world issues and solutions encountered in the GetFitterGetBigger Admin project.

## Key Principles

### 1. Choose the Right Navigation Method

#### Use Standard HTML Links (`<a href>`)
**When to use:**
- User-initiated navigation between pages
- When you need predictable browser history behavior
- For navigation that should work with browser back/forward buttons

**Example:**
```razor
<!-- Good: Standard navigation -->
<a href="@($"/exercises/{exercise.Id}")" class="link-class">View Exercise</a>

<!-- Good: Navigation to static routes -->
<a href="/exercises" class="link-class">Back to List</a>
```

#### Use NavigationManager.NavigateTo()
**When to use:**
- Programmatic navigation after operations (e.g., after saving/deleting)
- Conditional navigation based on business logic
- When you need to force a page reload
- Navigation from code-behind or event handlers

**Example:**
```csharp
// After successful save operation
private async Task SaveExercise()
{
    await ExerciseService.SaveAsync(exercise);
    NavigationManager.NavigateTo("/exercises");
}

// Conditional navigation
private void NavigateBasedOnRole()
{
    if (User.IsInRole("Admin"))
        NavigationManager.NavigateTo("/admin/dashboard");
    else
        NavigationManager.NavigateTo("/user/dashboard");
}
```

### 2. Avoid Common Pitfalls

#### Don't Mix Navigation Approaches Unnecessarily
```razor
<!-- Bad: Mixing approaches can cause history issues -->
<a @onclick="() => NavigationManager.NavigateTo('/exercises')" 
   @onclick:preventDefault="true" 
   href="#">
   Go to Exercises
</a>

<!-- Good: Use one approach -->
<a href="/exercises">Go to Exercises</a>
```

#### Don't Clear State Too Early
```csharp
// Bad: Clearing state immediately after use
public async Task LoadWithStoredState()
{
    if (_storedState != null)
    {
        var state = _storedState;
        _storedState = null; // Too early! Breaks back navigation
        await LoadData(state);
    }
}

// Good: Keep state for navigation
public async Task LoadWithStoredState()
{
    if (_storedState != null)
    {
        await LoadData(_storedState);
        // Let navigation service clear when leaving section
    }
}
```

## Browser History Management

### Understanding Blazor Interactive Server Mode
- Maintains SignalR connection to server
- Page updates happen via SignalR, not full page loads
- Browser history can be tricky with programmatic navigation
- Standard links create proper history entries

### Best Practices for History Management

1. **Test Browser Navigation**
   - Always test back/forward buttons
   - Verify state preservation
   - Check deep linking works

2. **State Persistence Pattern**
   ```csharp
   // Store state when navigating away
   private void NavigateToDetail(string id)
   {
       StateService.StoreCurrentPageState();
       // Use standard link, not NavigateTo
   }
   
   // Restore state when returning
   protected override async Task OnInitializedAsync()
   {
       if (StateService.HasStoredState)
       {
           await StateService.RestorePageState();
       }
       else
       {
           await StateService.InitializeDefault();
       }
   }
   ```

3. **Use OnAfterRenderAsync for State Storage**
   ```csharp
   protected override async Task OnAfterRenderAsync(bool firstRender)
   {
       if (firstRender && HasDataLoaded)
       {
           // Store state after data is loaded
           StateService.StoreCurrentPageState();
       }
   }
   ```

## Navigation Patterns

### List → Detail → List Pattern
```razor
<!-- ExerciseList.razor -->
<a href="@($"/exercises/{item.Id}")" class="item-link">
    @item.Name
</a>

<!-- ExerciseDetail.razor -->
<button @onclick="NavigateBack" class="back-button">
    ← Back to List
</button>

@code {
    private void NavigateBack()
    {
        // Use standard navigation for predictable history
        NavigationManager.NavigateTo("/exercises");
    }
}
```

### Form Navigation Pattern
```csharp
// After successful form submission
private async Task HandleValidSubmit()
{
    var result = await Service.SaveAsync(Model);
    if (result.IsSuccess)
    {
        // Programmatic navigation after operation
        NavigationManager.NavigateTo("/exercises");
    }
}

// Cancel navigation
private void HandleCancel()
{
    // Check for unsaved changes first
    if (HasUnsavedChanges)
    {
        // Show confirmation dialog
        ShowConfirmDialog();
    }
    else
    {
        NavigationManager.NavigateTo("/exercises");
    }
}
```

## State Management Across Navigation

### Service Pattern for State Persistence
```csharp
public interface IPageStateService
{
    void StoreState<T>(string key, T state);
    T GetState<T>(string key);
    void ClearState(string key);
    bool HasState(string key);
}

public class ExerciseListStateService
{
    private ExerciseFilter _storedFilter;
    
    public void StoreCurrentFilter()
    {
        _storedFilter = CurrentFilter.Clone();
    }
    
    public void RestoreFilter()
    {
        if (_storedFilter != null)
        {
            CurrentFilter = _storedFilter;
            // Don't clear immediately - wait for navigation away
        }
    }
    
    public void ClearStoredFilter()
    {
        _storedFilter = null;
    }
}
```

### Navigation Service for Cleanup
```csharp
public class NavigationService
{
    private string _currentSection;
    
    public void OnNavigating(string toPath)
    {
        var newSection = GetSection(toPath);
        
        // Clear state when leaving section
        if (_currentSection == "exercises" && newSection != "exercises")
        {
            _exerciseStateService.ClearStoredFilter();
        }
        
        _currentSection = newSection;
    }
}
```

## Testing Navigation

### Key Test Scenarios
1. **Browser Back Button**: Navigate forward, then back
2. **Browser Forward Button**: Navigate back, then forward
3. **Deep Linking**: Direct URL access should work
4. **State Preservation**: Filters, pagination, scroll position
5. **Multiple Tabs**: Each tab should maintain independent state

### Example Test
```csharp
[Fact]
public async Task BrowserBack_ReturnsToListWithState()
{
    // Arrange
    var listPage = await NavigateToExerciseList();
    await ApplyFilter("strength");
    await NavigateToPage(2);
    
    // Act
    await ClickExerciseLink("exercise-1");
    await BrowserBack();
    
    // Assert
    Assert.Equal("/exercises", CurrentPath);
    Assert.Equal("strength", GetFilterValue());
    Assert.Equal(2, GetCurrentPage());
}
```

## Common Issues and Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Back button skips pages | Using NavigateTo with replace | Use standard links for user navigation |
| State lost on back | Clearing state too early | Keep state until leaving section |
| History not updating | preventDefault on links | Remove preventDefault, use href |
| Duplicate history entries | Multiple NavigateTo calls | Consolidate navigation logic |

## Summary

1. **Prefer standard links** for user-initiated navigation
2. **Use NavigationManager** for programmatic navigation
3. **Test browser navigation** thoroughly
4. **Manage state carefully** across navigation events
5. **Don't mix approaches** unless necessary
6. **Document navigation patterns** in your application

Remember: The browser is good at managing its own history. Let it do its job by using standard HTML navigation where possible.