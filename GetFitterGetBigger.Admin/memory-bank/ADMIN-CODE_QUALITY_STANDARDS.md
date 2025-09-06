# Admin-Specific Code Quality Standards - GetFitterGetBigger

**🎯 PURPOSE**: Blazor-specific code quality standards that extend the universal standards for the GetFitterGetBigger Admin project. These standards are mandatory for all Blazor implementations.

## 📋 Prerequisites

This document extends the universal `CODE_QUALITY_STANDARDS.md`. Read that first, then apply these Blazor-specific standards.

---

## 🎯 Blazor-Specific Patterns

### 1. **Component Lifecycle Management**
Properly manage component lifecycle to prevent memory leaks:

```csharp
// ❌ BAD - Not disposing subscriptions
@code {
    protected override void OnInitialized()
    {
        StateService.OnChange += StateHasChanged;
    }
}

// ✅ GOOD - Proper disposal
@implements IDisposable
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

### 2. **State Management Pattern**
Use proper state management to avoid inconsistencies:

```csharp
// ❌ BAD - Direct state manipulation in components
@code {
    private List<Equipment> _equipment = new();
    
    private async Task DeleteEquipment(string id)
    {
        _equipment.RemoveAll(e => e.Id == id);
        await EquipmentService.DeleteAsync(id);
    }
}

// ✅ GOOD - Centralized state management
@code {
    [Inject] private IEquipmentState EquipmentState { get; set; }
    
    private async Task DeleteEquipment(string id)
    {
        await EquipmentState.DeleteEquipmentAsync(id);
        // State handles both API call and local state update
    }
}
```

### 3. **Component Communication**
Use proper patterns for parent-child communication:

```csharp
// ❌ BAD - Direct parent manipulation
<ChildComponent Parent="this" />

@code {
    public void UpdateFromChild(string data) { }
}

// ✅ GOOD - Event callbacks
<ChildComponent OnDataChanged="HandleDataChange" />

@code {
    private async Task HandleDataChange(string data)
    {
        // Handle the change
    }
}
```

---

## 🏗️ Blazor Architecture Standards

### 1. **Component Organization**

#### Component Structure
```
/Components/
├── Shared/           # Reusable components
│   ├── LoadingSpinner.razor
│   └── ConfirmDialog.razor
├── Equipment/        # Feature-specific components
│   ├── EquipmentList.razor
│   ├── EquipmentForm.razor
│   └── EquipmentCard.razor
└── Layout/          # Layout components
    ├── MainLayout.razor
    └── NavMenu.razor
```

#### Component Rules
- One component per file
- Component name matches filename
- Keep components focused (< 200 lines)
- Extract complex logic to code-behind or services

### 2. **Service Layer Pattern**
Services handle business logic and API communication:

```csharp
public interface IEquipmentService
{
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync();
    Task<ServiceResult<EquipmentDto>> GetByIdAsync(string id);
    Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentDto dto);
    Task<ServiceResult<EquipmentDto>> UpdateAsync(string id, UpdateEquipmentDto dto);
    Task<ServiceResult<bool>> DeleteAsync(string id);
}

// Implementation
public class EquipmentService : IEquipmentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EquipmentService> _logger;
    
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/equipment");
            
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<IEnumerable<EquipmentDto>>();
                return ServiceResult<IEnumerable<EquipmentDto>>.Success(data);
            }
            
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return ServiceResult<IEnumerable<EquipmentDto>>.Failure(error.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching equipment");
            return ServiceResult<IEnumerable<EquipmentDto>>.Failure("Network error occurred");
        }
    }
}
```

### 3. **Navigation Patterns**
Follow proper navigation patterns for predictable browser behavior:

**Key Principles:**
- Use standard HTML links (`<a href>`) for user-initiated navigation
- Reserve `NavigationManager.NavigateTo()` for programmatic navigation
- Maintain proper browser history for back/forward navigation
- See `patterns/BLAZOR-NAVIGATION-BEST-PRACTICES.md` for complete guidelines

```csharp
// ✅ GOOD - Standard link for user navigation
<a href="@($"/exercises/{exercise.Id}")" class="link-class">View Exercise</a>

// ✅ GOOD - Programmatic navigation after operation
private async Task HandleValidSubmit()
{
    var result = await Service.SaveAsync(model);
    if (result.IsSuccess)
    {
        NavigationManager.NavigateTo("/exercises");
    }
}
```

### 4. **Form Handling**
Use proper form validation and submission patterns:

```csharp
// ❌ BAD - Manual validation, no error handling
<EditForm Model="@model" OnSubmit="@Submit">
    <InputText @bind-Value="model.Name" />
    <button type="submit">Save</button>
</EditForm>

@code {
    private async Task Submit()
    {
        if (string.IsNullOrEmpty(model.Name))
        {
            // Show error somehow
            return;
        }
        
        await Service.CreateAsync(model);
        NavigationManager.NavigateTo("/equipment");
    }
}

// ✅ GOOD - Proper validation and error handling
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label>Name</label>
        <InputText @bind-Value="model.Name" class="form-control" />
        <ValidationMessage For="@(() => model.Name)" />
    </div>
    
    <button type="submit" disabled="@_isSubmitting">
        @if (_isSubmitting)
        {
            <span class="spinner-border spinner-border-sm mr-2"></span>
        }
        Save
    </button>
</EditForm>

@code {
    private bool _isSubmitting;
    private string _errorMessage;
    
    private async Task HandleValidSubmit()
    {
        _isSubmitting = true;
        _errorMessage = null;
        
        var result = await Service.CreateAsync(model);
        
        if (result.IsSuccess)
        {
            NavigationManager.NavigateTo($"/equipment/{result.Data.Id}");
        }
        else
        {
            _errorMessage = result.ErrorMessage;
            _isSubmitting = false;
        }
    }
}
```

---

## 🚀 Performance Standards

### 1. **Component Rendering Optimization**
Minimize unnecessary re-renders:

```csharp
// ❌ BAD - Causes unnecessary renders
@foreach (var item in GetFilteredItems())
{
    <ItemComponent Item="@item" />
}

@code {
    private IEnumerable<Item> GetFilteredItems() => 
        _items.Where(i => i.IsActive);  // Recalculated every render
}

// ✅ GOOD - Computed once when needed
@foreach (var item in _filteredItems)
{
    <ItemComponent Item="@item" />
}

@code {
    private IEnumerable<Item> _filteredItems = new List<Item>();
    
    protected override void OnParametersSet()
    {
        _filteredItems = _items.Where(i => i.IsActive).ToList();
    }
}
```

### 2. **Async Operations**
Handle async operations properly:

```csharp
// ❌ BAD - Blocking async calls
protected override void OnInitialized()
{
    _data = DataService.GetDataAsync().Result; // Blocks thread!
}

// ✅ GOOD - Proper async handling
protected override async Task OnInitializedAsync()
{
    _isLoading = true;
    
    try
    {
        _data = await DataService.GetDataAsync();
    }
    catch (Exception ex)
    {
        _error = "Failed to load data";
        _logger.LogError(ex, "Error loading data");
    }
    finally
    {
        _isLoading = false;
    }
}
```

### 3. **Memory Management**
Prevent memory leaks:

```csharp
@implements IDisposable
@code {
    private Timer _timer;
    private CancellationTokenSource _cts = new();
    
    protected override void OnInitialized()
    {
        _timer = new Timer(Refresh, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

---

## 🎨 UI/UX Standards

### 1. **Loading States**
Always show loading feedback:

```razor
@if (_isLoading)
{
    <LoadingSpinner />
}
else if (_hasError)
{
    <ErrorMessage Message="@_errorMessage" OnRetry="@LoadData" />
}
else if (!_data.Any())
{
    <EmptyState Message="No equipment found" />
}
else
{
    <EquipmentGrid Data="@_data" />
}
```

### 2. **Error Handling UI**
Provide clear error feedback:

```csharp
// Centralized error display component
<ErrorBoundary>
    <ChildContent>
        @ChildContent
    </ChildContent>
    <ErrorContent Context="exception">
        <div class="alert alert-danger">
            <h4>Something went wrong</h4>
            <p>@exception.Message</p>
            <button class="btn btn-primary" @onclick="Recover">Try Again</button>
        </div>
    </ErrorContent>
</ErrorBoundary>
```

### 3. **Form Feedback**
Immediate validation feedback:

```razor
<div class="form-group">
    <label for="name">Equipment Name</label>
    <InputText id="name" @bind-Value="model.Name" 
               class="form-control @GetValidationClass(() => model.Name)" />
    <ValidationMessage For="@(() => model.Name)" />
</div>

@code {
    private string GetValidationClass(Expression<Func<object>> field)
    {
        var fieldIdentifier = FieldIdentifier.Create(field);
        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
        
        if (editContext.IsModified(fieldIdentifier))
        {
            return isValid ? "is-valid" : "is-invalid";
        }
        
        return "";
    }
}
```

---

## 📊 Blazor-Specific Review Checklist

### ✅ Component Standards
- [ ] Proper lifecycle management (IDisposable when needed)
- [ ] No memory leaks (event handler cleanup)
- [ ] Components focused and < 200 lines
- [ ] Proper parameter usage ([Parameter] attribute)
- [ ] Child content properly handled

### ✅ State Management
- [ ] Centralized state for shared data
- [ ] No direct cross-component communication
- [ ] State changes trigger appropriate updates
- [ ] Proper cascade value usage
- [ ] Navigation state preserved when appropriate

### ✅ Performance
- [ ] Unnecessary re-renders minimized
- [ ] Large lists use virtualization
- [ ] Images properly optimized and lazy loaded
- [ ] JavaScript interop minimized
- [ ] Async operations don't block UI

### ✅ Forms & Validation
- [ ] EditForm with proper validation
- [ ] Loading states during submission
- [ ] Error feedback displayed clearly
- [ ] Validation messages near inputs
- [ ] Form state preserved appropriately

### ✅ Error Handling
- [ ] ErrorBoundary usage for component errors
- [ ] Network errors handled gracefully
- [ ] User-friendly error messages
- [ ] Retry mechanisms where appropriate
- [ ] Logging for debugging

### ✅ Accessibility
- [ ] Proper ARIA attributes
- [ ] Keyboard navigation support
- [ ] Screen reader friendly
- [ ] Color contrast compliance
- [ ] Focus management

### ✅ Security
- [ ] XSS prevention (no raw HTML)
- [ ] Authentication state checked
- [ ] Sensitive data not in URLs
- [ ] CORS properly configured
- [ ] Authorization on components

---

## 🧪 Testing Standards

### Component Tests
```csharp
[Fact]
public void EquipmentList_RendersCorrectly_WithData()
{
    // Arrange
    var equipment = new[] { 
        new EquipmentDto { Id = "equipment-1", Name = "Barbell" },
        new EquipmentDto { Id = "equipment-2", Name = "Dumbbell" }
    };
    
    using var ctx = new TestContext();
    ctx.Services.AddSingleton<IEquipmentService>(new MockEquipmentService(equipment));
    
    // Act
    var component = ctx.RenderComponent<EquipmentList>();
    
    // Assert
    Assert.Equal(2, component.FindAll("tr.equipment-row").Count);
    Assert.Contains("Barbell", component.Markup);
    Assert.Contains("Dumbbell", component.Markup);
}
```

### E2E Tests
- Test critical user journeys
- Use Playwright or Selenium
- Test across different browsers
- Include mobile viewport tests

---

## 🔗 Related Documents

- Universal: `CODE_QUALITY_STANDARDS.md`
- Process: `CODE_REVIEW_PROCESS.md`
- Navigation: `patterns/BLAZOR-NAVIGATION-BEST-PRACTICES.md`
- Performance: `patterns/blazor-shouldrender-optimization-pattern.md`
- Testing: `patterns/comprehensive-blazor-testing-patterns.md`
- Accessibility: `guides/accessibility-automation-guide.md`
- Blazor Docs: https://docs.microsoft.com/blazor

---

Remember: These standards ensure our Blazor application is performant, maintainable, and provides an excellent user experience.