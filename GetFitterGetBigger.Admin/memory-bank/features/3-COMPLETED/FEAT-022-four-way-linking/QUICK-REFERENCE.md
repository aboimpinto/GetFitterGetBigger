# Four-Way Exercise Linking - Quick Reference

## Exercise Link Types
1. **WARMUP** - Preparatory exercises before main workout
2. **COOLDOWN** - Recovery exercises after main workout  
3. **ALTERNATIVE** - Substitute exercises with similar benefits
4. **WORKOUT** - Main exercise context (implicit, not a link type)

## Context Color Codes
- 🟢 **Emerald** - Workout context (main exercises)
- 🟠 **Orange** - Warmup exercises
- 🔵 **Blue** - Cooldown exercises
- 🟣 **Purple** - Alternative exercises

## Business Rules

### What's Allowed ✅
- ✅ Unlimited alternative exercises per exercise
- ✅ Multiple warmup exercises (ordered sequence)
- ✅ Multiple cooldown exercises (ordered sequence)
- ✅ Bidirectional links for multi-type exercises
- ✅ Context switching for exercises with multiple types
- ✅ Drag-and-drop reordering for warmup/cooldown sequences

### What's NOT Allowed ❌
- ❌ Duplicate links (same target exercise in same context)
- ❌ Self-referential links (exercise linking to itself)
- ❌ Alternative links without matching exercise types
- ❌ Reordering alternative exercises (no sequence)
- ❌ Cross-context links (warmup can't link to cooldown)

## Key Components

### Main Component
```razor
<FourWayExerciseLinkManager Exercise="@exercise" />
```

### Context Views
```razor
<!-- Automatically displayed based on exercise types -->
<WorkoutContextView />     <!-- For WORKOUT type -->
<WarmupContextView />       <!-- For WARMUP type -->
<CooldownContextView />     <!-- For COOLDOWN type -->
<AlternativeExercisesList /> <!-- Always available -->
```

## API Endpoints
```
GET    /api/exercises/{id}/links           # All links
POST   /api/exercises/{sourceId}/links     # Create link
DELETE /api/exercises/links/{linkId}       # Delete link
GET    /api/exercises/{id}/alternatives    # Alternatives only
```

## JSON Request Format
```json
{
  "sourceExerciseId": 123,
  "targetExerciseId": 456,
  "linkType": "ALTERNATIVE",  // or "WARMUP", "COOLDOWN"
  "displayOrder": 1           // Only for WARMUP/COOLDOWN
}
```

## Keyboard Shortcuts
- **Tab** - Navigate between contexts
- **Enter** - Select/activate buttons
- **Delete** - Remove selected link (with confirmation)
- **Escape** - Close modals/cancel operations
- **Arrow Keys** - Navigate within lists

## Component States

### Loading States
```razor
@if (IsLoading)
{
    <LoadingSkeleton />  <!-- Skeleton loader for perceived performance -->
}
```

### Empty States
```razor
@if (!Links.Any())
{
    <EmptyState Icon="plus-circle" 
                Message="No exercises linked"
                ActionText="Add Exercise" />
}
```

### Error States
```razor
@if (HasError)
{
    <ErrorAlert Message="@ErrorMessage" 
                OnRetry="@RetryOperation" />
}
```

## State Management

### Service Registration
```csharp
// Program.cs
builder.Services.AddSingleton<IExerciseLinkStateService, ExerciseLinkStateService>();
```

### Component Subscription
```csharp
@implements IDisposable

protected override void OnInitialized()
{
    StateService.OnChange += StateHasChanged;
}

public void Dispose()
{
    StateService.OnChange -= StateHasChanged;
}
```

## Validation Patterns

### Type Compatibility Check
```csharp
bool CanLink = source.ExerciseTypes
    .Intersect(target.ExerciseTypes)
    .Any();
```

### Duplicate Prevention
```csharp
bool IsDuplicate = ExistingLinks
    .Any(l => l.TargetExerciseId == newTargetId 
           && l.LinkType == newLinkType);
```

## CSS Classes

### Context-Specific Styling
```css
.context-workout { @apply border-emerald-500; }
.context-warmup { @apply border-orange-500; }
.context-cooldown { @apply border-blue-500; }
.context-alternative { @apply border-purple-500; }
```

### Card Shadows
```css
.exercise-card { @apply shadow-md hover:shadow-lg; }
.selected-card { @apply ring-2 ring-offset-2; }
```

## Performance Tips

### Lazy Loading
```csharp
// Load alternatives only when tab is selected
if (CurrentContext == "alternative" && !AlternativesLoaded)
{
    await LoadAlternatives();
}
```

### Virtualization
```razor
<!-- For lists with 20+ items -->
<Virtualize Items="@LargeList" ItemSize="120">
    <ItemTemplate Context="item">
        <ExerciseCard Exercise="@item" />
    </ItemTemplate>
</Virtualize>
```

### Debounced Search
```csharp
private Timer? _searchTimer;
private void OnSearchChanged(string text)
{
    _searchTimer?.Dispose();
    _searchTimer = new Timer(_ => Search(text), null, 300, Timeout.Infinite);
}
```

## Common Troubleshooting

### Issue: Links not updating
**Solution**: Ensure StateHasChanged() is called after state mutations

### Issue: Duplicate link error
**Solution**: Check for existing links before creating

### Issue: Performance lag
**Solution**: Implement virtualization for large lists

### Issue: Context tabs missing
**Solution**: Verify exercise has required ExerciseTypes

## Testing Checklist

### Component Tests
- [ ] Tab navigation works correctly
- [ ] Links can be added/removed
- [ ] State updates propagate
- [ ] Validation rules enforced
- [ ] Accessibility compliant

### Integration Tests
- [ ] API calls succeed
- [ ] Error handling works
- [ ] Optimistic updates rollback
- [ ] Multi-user scenarios handled

## Quick Implementation Example

```csharp
@page "/exercises/{ExerciseId:int}/links"
@inject IExerciseLinkStateService StateService
@inject IExerciseLinkService LinkService

<FourWayExerciseLinkManager Exercise="@_exercise" />

@code {
    [Parameter] public int ExerciseId { get; set; }
    private ExerciseDto? _exercise;
    
    protected override async Task OnInitializedAsync()
    {
        _exercise = await ExerciseService.GetExercise(ExerciseId);
        await StateService.LoadLinksForExercise(ExerciseId);
    }
}
```

## Feature Flags
```json
{
  "Features": {
    "FourWayLinking": true,
    "UnlimitedAlternatives": true,
    "BidirectionalLinks": true
  }
}
```

---
*Quick reference for developers working with the Four-Way Exercise Linking System. For detailed documentation, see the full user guide.*