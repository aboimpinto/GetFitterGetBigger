# Four-Way Exercise Linking Implementation Guide

## Overview
This guide provides a comprehensive implementation roadmap for the Four-Way Exercise Linking feature (FEAT-030) in the GetFitterGetBigger Admin Blazor application based on UX research and wireframe specifications.

## Quick Reference

### Link Types & Rules
| Link Type | From | To | Rules | UI Behavior |
|-----------|------|----|-------|-------------|
| WARMUP | Workout | Warmup exercises | Max 10, ordered | Add/Remove/Reorder |
| COOLDOWN | Workout | Cooldown exercises | Max 10, ordered | Add/Remove/Reorder |
| WORKOUT | Warmup/Cooldown | Workout exercises | Auto-generated reverse | View-only |
| ALTERNATIVE | Any type | Same type exercise | Unlimited, unordered | Add/Remove only |

### Context-Based UI Display
| Exercise Type | Shows Sections |
|--------------|----------------|
| Workout only | Warmups, Cooldowns, Alternative Workouts |
| Warmup only | Workouts (using this), Alternative Warmups |
| Cooldown only | Workouts (using this), Alternative Cooldowns |
| Multi-type | Context tabs with relevant sections |

## Component Architecture

### 1. Primary Component Replacement
Replace `ExerciseLinkManager.razor` with `FourWayExerciseLinkManager.razor`:

```razor
@* FourWayExerciseLinkManager.razor *@
@if (ShowComponent)  @* Show for ANY exercise type now *@
{
    <div class="mt-6" data-testid="four-way-linking-manager">
        @if (HasMultipleContexts)
        {
            <ExerciseContextSelector 
                Contexts="@AvailableContexts"
                ActiveContext="@ActiveContext"
                OnContextChange="@HandleContextChange" />
        }
        
        <ContextualLinkSections 
            Exercise="@Exercise"
            ActiveContext="@ActiveContext"
            StateService="@StateService" />
    </div>
}

@code {
    private bool ShowComponent => Exercise.ExerciseTypes?.Any() == true;
    private bool HasMultipleContexts => Exercise.ExerciseTypes?.Count() > 1;
}
```

### 2. Context-Aware Sections Component
Create `ContextualLinkSections.razor`:

```razor
@switch (DetermineContextView())
{
    case "Workout":
        <WorkoutContextView />  @* Warmups, Cooldowns, Alternatives *@
        break;
    case "Warmup":
        <WarmupContextView />   @* Workouts, Alternative Warmups *@
        break;
    case "Cooldown":
        <CooldownContextView /> @* Workouts, Alternative Cooldowns *@
        break;
}
```

### 3. Alternative Exercise Card
Create `AlternativeExerciseCard.razor`:

```razor
@* Different from ExerciseLinkCard - no reordering *@
<div class="bg-white border border-purple-200 p-4 rounded-lg">
    <div class="flex items-start justify-between">
        <div class="flex-1">
            <h4>@Link.TargetExercise.Name</h4>
            @* Exercise details *@
        </div>
        <div class="flex-shrink-0">
            <button @onclick="RemoveLink">Remove</button>
        </div>
    </div>
</div>
```

## State Service Enhancements

### Extended Interface
```csharp
public interface IExerciseLinkStateService 
{
    // NEW: Alternative links
    IEnumerable<ExerciseLinkDto> AlternativeLinks { get; }
    IEnumerable<ExerciseLinkDto> WorkoutLinks { get; } // For reverse links
    
    // NEW: Context management
    string ActiveContext { get; set; }
    Task SwitchContextAsync(string context);
    
    // NEW: Link type specific operations
    Task CreateLinkAsync(CreateExerciseLinkDto dto, ExerciseLinkType type);
}
```

### Link Type Detection
```csharp
private string DetermineLinkType(string sourceType, string targetType, string action)
{
    return (sourceType, action) switch
    {
        ("Workout", "add-warmup") => "WARMUP",
        ("Workout", "add-cooldown") => "COOLDOWN",
        (_, "add-alternative") => "ALTERNATIVE",
        _ => throw new InvalidOperationException($"Unknown link combination")
    };
}
```

## API Integration

### Creating Links
```csharp
// Adding alternative exercise
var createDto = new CreateExerciseLinkDto
{
    SourceExerciseId = Exercise.Id,
    TargetExerciseId = selectedExercise.Id,
    LinkType = "ALTERNATIVE"  // API handles bidirectional creation
};

await ExerciseService.CreateLinkAsync(createDto);
```

### Fetching Links by Type
```csharp
// Get alternative links
var alternatives = await ExerciseService.GetLinksAsync(
    exerciseId, 
    linkType: "ALTERNATIVE",
    includeExerciseDetails: true);

// Get workout links (reverse) for warmup/cooldown
var workouts = await ExerciseService.GetLinksAsync(
    exerciseId,
    linkType: "WORKOUT",
    includeExerciseDetails: true);
```

## Validation Implementation

### Client-Side Validation
```csharp
public class ExerciseLinkValidator
{
    public bool CanAddAlternative(ExerciseDto source, ExerciseDto target)
    {
        // Must share at least one type
        var sharedTypes = source.ExerciseTypes
            .Select(t => t.Value)
            .Intersect(target.ExerciseTypes.Select(t => t.Value));
            
        return sharedTypes.Any() && source.Id != target.Id;
    }
    
    public bool CanAddToList(string linkId, IEnumerable<ExerciseLinkDto> existing)
    {
        return !existing.Any(e => e.TargetExerciseId == linkId);
    }
}
```

## UI Color Scheme

### Context-Based Styling
```css
/* Add to app.css or component styles */
.context-workout { @apply bg-emerald-50 border-emerald-200; }
.context-warmup { @apply bg-orange-50 border-orange-200; }
.context-cooldown { @apply bg-blue-50 border-blue-200; }
.context-alternative { @apply bg-purple-50 border-purple-200; }
.context-multi { @apply bg-indigo-50 border-indigo-200; }
```

## Migration Path

### Phase 1: Infrastructure (Week 1)
1. Create new components without removing old ones
2. Extend IExerciseLinkStateService
3. Add API integration for ALTERNATIVE type
4. Implement validation services

### Phase 2: UI Implementation (Week 2)
1. Replace ExerciseLinkManager in ExerciseDetail.razor
2. Implement context detection logic
3. Add alternative exercise sections
4. Implement multi-type context switching

### Phase 3: Testing & Polish (Week 3)
1. Write bUnit tests for new components
2. Test all exercise type combinations
3. Optimize loading performance
4. Add accessibility features

## Testing Scenarios

### Critical Test Cases
1. **Single Type Exercise**: Verify correct sections display
2. **Multi-Type Exercise**: Test context switching
3. **Alternative Validation**: Ensure type matching works
4. **Bidirectional Links**: Verify reverse link creation
5. **Maximum Limits**: Test 10-link limit for warmup/cooldown
6. **Self-Reference**: Prevent exercise linking to itself
7. **Duplicate Prevention**: Can't add same link twice

### Edge Cases
- Exercise with 5+ types
- Removing last alternative
- API failure during link creation
- Concurrent link modifications
- Large exercise libraries (1000+ items)

## Performance Considerations

### Caching Strategy
```csharp
// Cache compatible exercises for alternatives
private Dictionary<string, List<ExerciseListDto>> _compatibleExercisesCache = new();

private async Task<List<ExerciseListDto>> GetCompatibleExercisesAsync(string exerciseType)
{
    if (!_compatibleExercisesCache.ContainsKey(exerciseType))
    {
        var compatible = await ExerciseService.GetExercisesByTypeAsync(exerciseType);
        _compatibleExercisesCache[exerciseType] = compatible;
    }
    return _compatibleExercisesCache[exerciseType];
}
```

### Optimistic UI Updates
```csharp
// Add to UI immediately, rollback on error
private async Task AddLinkOptimistically(ExerciseLinkDto link)
{
    // Add to UI
    _tempLinks.Add(link);
    StateHasChanged();
    
    try
    {
        await ExerciseService.CreateLinkAsync(link);
        // Move from temp to permanent
    }
    catch
    {
        // Remove from UI
        _tempLinks.Remove(link);
        ShowError("Failed to add link");
    }
}
```

## Accessibility Requirements

### ARIA Attributes
```razor
<div role="region" 
     aria-label="Exercise relationships management"
     aria-busy="@IsLoading">
     
    @* Context tabs for multi-type *@
    <div role="tablist">
        <button role="tab" 
                aria-selected="@IsWorkoutContext"
                aria-controls="workout-panel">
            As Workout
        </button>
    </div>
    
    @* Live announcements *@
    <div aria-live="polite" class="sr-only">
        @ScreenReaderMessage
    </div>
</div>
```

### Keyboard Navigation
- Tab: Navigate between sections
- Enter/Space: Activate buttons
- Arrow keys: Navigate context tabs
- Escape: Close modals

## Success Metrics

### User Experience Goals
- 50% reduction in exercise linking time
- Zero invalid link creations
- <2 seconds to load all links
- 100% accessibility compliance

### Technical Goals
- <200ms optimistic UI response
- Zero duplicate API calls
- Proper error recovery
- Full test coverage

## Common Pitfalls to Avoid

1. **Don't assume exercise has single type** - Always check for multiple types
2. **Don't hardcode link limits** - Use constants for warmup/cooldown max
3. **Don't forget reverse links** - API creates them automatically
4. **Don't allow self-referencing** - Validate before API call
5. **Don't mix link types** - Alternatives must match types
6. **Don't ignore loading states** - Users need feedback
7. **Don't skip accessibility** - PTs may have various needs

## Developer Checklist

- [ ] Review UX research findings
- [ ] Study wireframe specifications
- [ ] Understand API FEAT-030 implementation
- [ ] Create new components
- [ ] Extend state service
- [ ] Implement validation
- [ ] Add context detection
- [ ] Handle multi-type exercises
- [ ] Implement alternative linking
- [ ] Add optimistic updates
- [ ] Write tests
- [ ] Test accessibility
- [ ] Optimize performance
- [ ] Document changes

## Support Resources

- UX Research: `/memory-bank/temp/four-way-linking-ux-research.md`
- Wireframes: `/memory-bank/temp/four-way-linking-wireframes.md`
- API Docs: Check FEAT-030 in API project
- UI Standards: `/memory-bank/ui-standards/`

This implementation guide provides the complete roadmap for successfully implementing the Four-Way Exercise Linking feature while maintaining code quality and user experience standards.