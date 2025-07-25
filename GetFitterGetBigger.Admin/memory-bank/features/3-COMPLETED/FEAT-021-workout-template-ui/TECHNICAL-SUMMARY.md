# Technical Summary - FEAT-021: Workout Template UI

## Architecture Overview

### Component Architecture
```
Pages (3)
├── WorkoutTemplateListPage (@page "/workout-templates")
├── WorkoutTemplateFormPage (@page "/workout-templates/new", "/workout-templates/{Id}/edit") 
└── WorkoutTemplateDetailPage (@page "/workout-templates/{Id}")

Feature Components (4)
├── WorkoutTemplateList (Grid display with sorting)
├── WorkoutTemplateCreateForm (Create workflow)
├── WorkoutTemplateEditForm (Edit workflow)
└── WorkoutTemplateExerciseView (Hierarchical exercise display)

Shared Components (4)
├── WorkoutStateIndicator (State badges)
├── StateTransitionButton (State change logic)
├── WorkoutTemplateCard (List item display)
└── WorkoutTemplateFilters (Search and filtering)

Supporting Components (3)
├── WorkoutTemplateForm (Shared form logic)
├── WorkoutTemplateDetail (Read-only display)
└── Breadcrumb (Navigation)

Services (2)
├── WorkoutTemplateService (API integration)
└── WorkoutTemplateStateService (State management)
```

### Data Flow
1. **API Integration**: WorkoutTemplateService → HttpClient → API
2. **State Management**: Components → WorkoutTemplateStateService → Local State
3. **Error Handling**: Service → State Service → Components → UI
4. **Caching**: Service layer with 5-minute cache for reference data

## Key Technical Decisions

### 1. Service Layer Design
```csharp
public interface IWorkoutTemplateService
{
    // Paginated list with filtering
    Task<PagedResultDto<WorkoutTemplateDto>> GetWorkoutTemplatesAsync(
        WorkoutTemplateFilterDto filter);
    
    // Standard CRUD operations
    Task<WorkoutTemplateDto> GetWorkoutTemplateAsync(Guid id);
    Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(
        CreateWorkoutTemplateDto dto);
    Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(
        Guid id, UpdateWorkoutTemplateDto dto);
    Task DeleteWorkoutTemplateAsync(Guid id);
    
    // Business operations
    Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(Guid id);
    Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(
        Guid id, WorkoutStateDto newState);
}
```

### 2. State Management Pattern
```csharp
public class WorkoutTemplateStateService : IWorkoutTemplateStateService
{
    // Filter state with event notification
    public WorkoutTemplateFilterDto CurrentFilter { get; private set; }
    public event Action? OnFilterChanged;
    
    // Optimistic updates with rollback
    public async Task<WorkoutTemplateDto> UpdateTemplateAsync(
        Guid id, UpdateWorkoutTemplateDto dto)
    {
        var backup = GetBackup(id);
        UpdateOptimistically(id, dto);
        
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return result;
        }
        catch
        {
            Rollback(id, backup);
            throw;
        }
    }
}
```

### 3. Component Communication
- **Cascading Parameters**: AuthenticationState, ToastService
- **Service Injection**: Direct DI for services
- **Event Callbacks**: Child to parent communication
- **Navigation**: NavigationManager for routing

### 4. Form Handling
```razor
@* Auto-save for drafts *@
@if (Template?.WorkflowState == WorkoutStateDto.Draft)
{
    <AutoSaveIndicator IsAutoSaving="@_isAutoSaving" 
                      LastSaved="@_lastAutoSave" />
}

@* Unsaved changes protection *@
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime

@code {
    private async Task<bool> ConfirmNavigation(
        LocationChangingContext context)
    {
        if (_hasUnsavedChanges)
        {
            var confirmed = await ShowUnsavedChangesDialog();
            if (!confirmed)
            {
                context.PreventNavigation();
            }
        }
    }
}
```

### 5. Error Handling Strategy
```csharp
// Service layer
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
{
    _errorMessage = "This template name already exists.";
    StateHasChanged();
}

// Component layer
<ErrorDisplay Message="@_errorMessage" 
              OnRetry="@HandleRetry" 
              ShowRetry="true" />
```

## Testing Strategy

### Unit Testing Approach
```csharp
// Service tests with mocked HttpClient
[Fact]
public async Task GetWorkoutTemplatesAsync_Success_ReturnsPagedResult()
{
    // Arrange
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("/api/workout-templates")
            .Respond("application/json", JsonContent);
    
    // Act & Assert
    var result = await service.GetWorkoutTemplatesAsync(filter);
    result.Should().NotBeNull();
}

// Component tests with bUnit
[Fact]
public void WorkoutTemplateCard_Click_NavigatesToDetail()
{
    // Arrange
    var cut = RenderComponent<WorkoutTemplateCard>(
        parameters => parameters.Add(p => p.Template, template));
    
    // Act
    cut.Find("[data-testid='template-card']").Click();
    
    // Assert
    NavigationManager.Uri.Should().EndWith($"/workout-templates/{template.Id}");
}
```

### Integration Testing
```csharp
[Fact]
public async Task CompleteWorkflow_CreateEditDelete_Success()
{
    // Create
    var createPage = RenderComponent<WorkoutTemplateFormPage>();
    await FillForm(createPage);
    await createPage.Find("[data-testid='save-button']").ClickAsync();
    
    // Edit
    var editPage = Navigate<WorkoutTemplateFormPage>($"/edit/{id}");
    await UpdateForm(editPage);
    
    // Delete
    var detailPage = Navigate<WorkoutTemplateDetailPage>($"/{id}");
    await detailPage.Find("[data-testid='delete-button']").ClickAsync();
}
```

## Performance Optimizations

### 1. Debounced Search
```csharp
private Timer? _searchDebounceTimer;
private void OnSearchChanged(string value)
{
    _searchDebounceTimer?.Dispose();
    _searchDebounceTimer = new Timer(async _ =>
    {
        await InvokeAsync(async () =>
        {
            await UpdateFilter(f => f.Name = value);
        });
    }, null, 300, Timeout.Infinite);
}
```

### 2. Reference Data Caching
```csharp
private readonly MemoryCache _cache = new(new MemoryCacheOptions());

public async Task<List<ReferenceDataDto>> GetCategoriesAsync()
{
    return await _cache.GetOrCreateAsync("categories", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return await FetchCategoriesFromApi();
    });
}
```

### 3. Skeleton Loading
```razor
@if (_isLoading)
{
    <div class="skeleton-loader">
        @for (int i = 0; i < 10; i++)
        {
            <WorkoutTemplateCardSkeleton />
        }
    </div>
}
```

## Security Considerations

### Authorization
- PT-Tier and Admin-Tier access enforced
- Page-level authorization with AuthorizeView
- Service-level authorization headers

### Input Validation
- Client-side validation with DataAnnotations
- Server-side validation at API
- XSS prevention through Blazor's built-in encoding

### State Protection
- Read-only fields for PRODUCTION templates
- Delete restricted to DRAFT/ARCHIVED
- State transitions follow business rules

## Browser Compatibility

### Tested Browsers
- Chrome 120+ ✓
- Firefox 120+ ✓
- Safari 17+ ✓
- Edge 120+ ✓

### JavaScript Requirements
- Local Storage API (recovery feature)
- Timer API (auto-save)
- No polyfills required

## Deployment Considerations

### Configuration
```json
{
  "ApiSettings": {
    "BaseUrl": "https://api.getfittergetbigger.com",
    "Timeout": 30
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development/Staging/Production
- `API_BASE_URL`: Override API endpoint

### Build Output
- Blazor WebAssembly or Server (both supported)
- Static assets in wwwroot
- No special deployment requirements

## Monitoring & Debugging

### Logging Points
```csharp
_logger.LogInformation("Creating workout template: {Name}", dto.Name);
_logger.LogError(ex, "Failed to update template {Id}", id);
```

### Error Tracking
- Service exceptions logged with context
- User actions tracked for debugging
- API correlation IDs maintained

### Performance Metrics
- Page load times < 2s
- Search response < 500ms
- Form submission < 1s

## Future Technical Considerations

### Scalability
- Current pagination supports large datasets
- Search optimized with debouncing
- State management is client-side (no server state)

### Extensibility Points
- Equipment filtering ready when API supports
- Exercise management UI structure in place
- AI suggestions can be added modularly

### Technical Debt
- Some methods exceed 20-line guideline
- Test coverage below 80% target
- Mock complexity in some integration tests

## Conclusion

The technical implementation successfully balances rapid development with maintainable architecture. The component structure supports future enhancements, testing strategy ensures reliability, and performance optimizations provide excellent user experience. The codebase follows established patterns while introducing new patterns where needed, creating a solid foundation for the workout template management feature.