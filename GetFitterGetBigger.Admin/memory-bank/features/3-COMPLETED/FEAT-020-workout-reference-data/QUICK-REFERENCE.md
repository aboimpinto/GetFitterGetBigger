# FEAT-020: Workout Reference Data - Quick Reference

## Feature At a Glance
**What**: Read-only views for Workout Objectives, Categories, and Execution Protocols  
**Where**: Admin → Reference Tables → [Workout Objectives | Workout Categories | Execution Protocols]  
**Who**: Personal Trainers viewing reference data  
**Status**: ✅ COMPLETED  

## Key Components

### 1. Workout Objectives
- **URL**: `/reference-tables/WorkoutObjectives`
- **Display**: Card grid with name and description
- **Search**: By name or description
- **Details**: Modal with full description

### 2. Workout Categories  
- **URL**: `/reference-tables/WorkoutCategories`
- **Display**: Color-coded cards with icons
- **Search**: By name or muscle groups
- **Special**: Shows primary muscle groups

### 3. Execution Protocols
- **URL**: `/reference-tables/ExecutionProtocols`
- **Display**: Detailed cards with protocol info
- **Search**: By name, code, or description
- **Special**: Intensity level badges, time/rep indicators

## Technical Quick Reference

### Services
```csharp
// Inject in components
@inject IWorkoutReferenceDataService WorkoutReferenceDataService
@inject WorkoutReferenceDataStateService WorkoutReferenceDataStateService

// Usage
await WorkoutReferenceDataStateService.InitializeAsync();
var objectives = WorkoutReferenceDataStateService.FilteredWorkoutObjectives;
```

### API Endpoints
```
GET /api/ReferenceTables/WorkoutObjectives
GET /api/ReferenceTables/WorkoutCategories  
GET /api/ReferenceTables/ExecutionProtocols
```

### Component Structure
```
Pages/
  └── ReferenceTableDetail.razor (Router component)
      
Services/
  ├── IWorkoutReferenceDataService.cs
  └── WorkoutReferenceDataService.cs
  
StateServices/
  └── WorkoutReferenceDataStateService.cs

Shared/
  ├── ReferenceDataSearchBar.razor
  ├── ReferenceDataDetailModal.razor
  └── VirtualizedGrid.razor (Future use)
```

## Common Tasks

### Adding a New Reference Type
1. Add endpoint to `IWorkoutReferenceDataService`
2. Implement in `WorkoutReferenceDataService`
3. Add state properties to `WorkoutReferenceDataStateService`
4. Add UI section in `ReferenceTableDetail.razor`
5. Create skeleton loader component
6. Add tests

### Modifying Search Behavior
```csharp
// In WorkoutReferenceDataStateService
public IEnumerable<T> FilteredItems =>
    Items.Where(item => 
        item.Value.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
        // Add additional search fields here
    );
```

### Changing Cache Duration
```csharp
// In WorkoutReferenceDataService constructor
_cacheOptions = new MemoryCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Change here
};
```

## UI Patterns

### Loading States
- Skeleton loaders match exact layout
- Show immediately on mount
- Smooth transition to content

### Error Handling
- Inline error messages
- Retry button always available
- Errors isolated per data type

### Search
- 300ms debounce delay
- Real-time result count
- Clear button when text present

### Modals
- Click outside to close
- Escape key to close
- Smooth open/close animations

## Accessibility Features

### Keyboard Support
- Tab navigation through all elements
- Enter/Space activates cards
- Escape closes modals
- Focus indicators on all interactive elements

### Screen Reader Support
- Descriptive ARIA labels
- Semantic HTML structure
- Result counts announced
- Loading states announced

## Performance Tips

### Current Optimizations
- 1-hour client-side cache
- CSS containment on cards
- Specific CSS transitions
- Debounced search input

### When to Consider Virtual Scrolling
- More than 50 items in view
- Performance degradation noticed
- Use included `VirtualizedGrid` component

## Testing

### Run Tests
```bash
# All tests
dotnet test

# Specific to this feature
dotnet test --filter "WorkoutReference"
```

### Key Test Files
- `WorkoutReferenceDataServiceTests.cs`
- `WorkoutReferenceDataStateServiceTests.cs`
- `WorkoutReferenceDataTests.cs`
- `ReferenceDataSearchBarTests.cs`
- `ReferenceDataDetailModalTests.cs`

## Troubleshooting

### Data Not Loading
1. Check browser console for errors
2. Verify API is running on http://localhost:5214
3. Check network tab for failed requests
4. Look for CORS errors

### Search Not Working
1. Check if data loaded successfully first
2. Verify search term property binding
3. Check console for binding errors

### Modal Not Opening
1. Ensure selected item is set
2. Check for JavaScript errors
3. Verify event handlers are wired

### Performance Issues
1. Check number of items rendered
2. Use browser performance profiler
3. Consider implementing virtual scrolling
4. Verify CSS containment is applied

## Configuration

### appsettings.json
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5214"
  }
}
```

### Dependency Injection (Program.cs)
```csharp
// HTTP Client
builder.Services.AddHttpClient<IWorkoutReferenceDataService, WorkoutReferenceDataService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});

// Services
builder.Services.AddScoped<WorkoutReferenceDataStateService>();

// Caching
builder.Services.AddMemoryCache();
```

## Future Enhancements
- [ ] Server-side filtering for large datasets
- [ ] Export to CSV/PDF functionality
- [ ] Sorting options for all views
- [ ] Advanced filter UI
- [ ] Real-time updates via SignalR
- [ ] Offline support with IndexedDB

## Related Documentation
- [Completion Report](./COMPLETION-REPORT.md)
- [Technical Summary](./TECHNICAL-SUMMARY.md)
- [Lessons Learned](./LESSONS-LEARNED.md)
- [API Documentation](../../../api-endpoints-verification.md)