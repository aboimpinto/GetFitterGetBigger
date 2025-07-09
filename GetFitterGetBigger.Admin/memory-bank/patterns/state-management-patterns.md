# State Management Patterns

This document describes common patterns and best practices for implementing state management services in the GetFitterGetBigger Admin application.

## Table of Contents
1. [Service-Based State Management](#service-based-state-management)
2. [Error Message Persistence Pattern](#error-message-persistence-pattern)
3. [Optimistic Updates Pattern](#optimistic-updates-pattern)
4. [State Change Notifications](#state-change-notifications)

## Service-Based State Management

The Admin application uses a service-based approach to state management, where each major feature has its own state service that manages the state for related components.

### Key Principles:
- State services are registered as Scoped services in DI
- State services expose an `OnChange` event for components to subscribe to
- State services handle all API interactions and error states
- Components should be thin and delegate all logic to state services

### Example Structure:
```csharp
public interface IFeatureStateService
{
    event Action? OnChange;
    
    // State properties
    bool IsLoading { get; }
    string? ErrorMessage { get; }
    string? SuccessMessage { get; }
    
    // Operations
    Task LoadDataAsync();
    void ClearMessages();
}
```

## Error Message Persistence Pattern

When implementing optimistic updates that may need to be rolled back, it's crucial to preserve error messages so users understand why their action failed.

### The Problem:
When an operation fails and needs to revert optimistic updates by reloading data, the reload operation typically clears error messages, leaving users without feedback about what went wrong.

### The Solution:
Implement an overloaded data loading method that can preserve error messages:

```csharp
public async Task LoadDataAsync()
{
    await LoadDataAsync(preserveErrorMessage: false);
}

private async Task LoadDataAsync(bool preserveErrorMessage)
{
    var existingErrorMessage = preserveErrorMessage ? ErrorMessage : null;
    
    try
    {
        IsLoading = true;
        if (!preserveErrorMessage)
        {
            ErrorMessage = null;
        }
        
        // Load data...
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Failed to load data: {ex.Message}";
    }
    finally
    {
        IsLoading = false;
        
        // Restore error message if preserved and no new error occurred
        if (preserveErrorMessage && string.IsNullOrEmpty(ErrorMessage) && !string.IsNullOrEmpty(existingErrorMessage))
        {
            ErrorMessage = existingErrorMessage;
        }
        
        NotifyStateChanged();
    }
}
```

### Usage in Error Handlers:
```csharp
catch (DuplicateException)
{
    ErrorMessage = "This item already exists";
    await LoadDataAsync(preserveErrorMessage: true); // Preserves the error message
}
```

### Testing Considerations:
When testing error scenarios, ensure that error messages persist through data reloads:

```csharp
[Fact]
public async Task Operation_OnError_PreservesErrorMessage()
{
    // Arrange
    _mockService.Setup(x => x.CreateAsync(It.IsAny<CreateDto>()))
        .ThrowsAsync(new DuplicateException());
    
    // Act
    await _stateService.CreateAsync(new CreateDto());
    
    // Assert
    _stateService.ErrorMessage.Should().Be("Expected error message");
    // Verify the error message persists after reload
}
```

## Optimistic Updates Pattern

Optimistic updates improve perceived performance by immediately updating the UI before the server confirms the operation.

### Implementation Steps:
1. Update local state immediately
2. Make the API call
3. On success: Reload to sync with server state
4. On failure: Reload with error message preservation to revert changes

### Example:
```csharp
public async Task CreateItemAsync(CreateDto dto)
{
    try
    {
        IsProcessing = true;
        ClearMessages();
        
        // Optimistic update
        var optimisticItem = new Item { /* temporary data */ };
        Items.Add(optimisticItem);
        NotifyStateChanged();
        
        // API call
        await _apiService.CreateAsync(dto);
        
        // Reload to get server state
        await LoadDataAsync();
        SuccessMessage = "Item created successfully";
    }
    catch (Exception ex)
    {
        ErrorMessage = GetErrorMessage(ex);
        await LoadDataAsync(preserveErrorMessage: true); // Revert with error preserved
    }
    finally
    {
        IsProcessing = false;
        NotifyStateChanged();
    }
}
```

## State Change Notifications

Components subscribe to state changes to update their UI when state changes occur.

### In State Service:
```csharp
public event Action? OnChange;

private void NotifyStateChanged() => OnChange?.Invoke();
```

### In Component:
```razor
@implements IDisposable
@inject IFeatureStateService StateService

@code {
    protected override void OnInitialized()
    {
        StateService.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        StateService.OnChange -= StateHasChanged;
    }
}
```

## Best Practices

1. **Always preserve error messages during rollback operations**
2. **Test error message persistence explicitly in unit tests**
3. **Clear messages at the start of new operations, not during data loads**
4. **Use optimistic updates for better perceived performance**
5. **Always notify state changes after any state modification**
6. **Dispose of event subscriptions to prevent memory leaks**