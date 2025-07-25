# Quick Reference - FEAT-021: Workout Template UI

## Feature Access
- **URL**: `/workout-templates`
- **Menu**: Training > Workout Templates
- **Authorization**: PT-Tier or Admin-Tier required

## Key Components & Their Purposes

### Pages
| Component | Route | Purpose |
|-----------|-------|---------|
| WorkoutTemplateListPage | /workout-templates | Main list with search/filter |
| WorkoutTemplateFormPage | /workout-templates/new | Create new template |
| WorkoutTemplateFormPage | /workout-templates/{id}/edit | Edit existing template |
| WorkoutTemplateDetailPage | /workout-templates/{id} | View template details |

### Services
| Service | Purpose | Key Methods |
|---------|---------|-------------|
| IWorkoutTemplateService | API integration | GetWorkoutTemplatesAsync, CreateAsync, UpdateAsync |
| IWorkoutTemplateStateService | State management | UpdateFilter, GetStoredPage, ClearError |

## Common Tasks

### Create New Template
```csharp
Navigate to: /workout-templates/new
Required fields: Name, Category, Difficulty, Duration
Optional: Description, IsPublic
Auto-saves drafts every 5 seconds
```

### Search Templates
```csharp
// In list view, use search box
// Debounced by 300ms
// Searches by name (partial match)
```

### Filter Templates
```csharp
// Available filters:
- Category (dropdown)
- Difficulty (dropdown)
- State (All/Draft/Production/Archived)
- Visibility (All/Public/Private)
// Filters combine with AND logic
```

### Change Template State
```csharp
// Draft → Production: "Move to Production" button
// Production → Archived: "Archive" button
// Archived: No state changes allowed
// Restrictions apply based on current state
```

### Duplicate Template
```csharp
// From detail view: Click "Duplicate"
// Creates copy with "(Copy)" suffix
// New template starts in DRAFT state
// Redirects to edit form
```

## State-Based Restrictions

### DRAFT State
- ✅ All fields editable
- ✅ Can delete
- ✅ Can move to Production
- ✅ Auto-save enabled

### PRODUCTION State
- ❌ Name not editable
- ❌ Category not editable
- ❌ Difficulty not editable
- ✅ Duration, Description, IsPublic editable
- ❌ Cannot delete
- ✅ Can archive

### ARCHIVED State
- ❌ Cannot edit
- ✅ Can delete
- ❌ No state transitions
- ✅ Can duplicate

## Component Patterns

### Form Validation
```razor
@* Required field with validation *@
<label class="block text-sm font-medium mb-1">
    Name <span class="text-red-500">*</span>
</label>
<InputText @bind-Value="Model.Name" 
           class="@GetFieldCssClass(() => Model.Name)"
           data-testid="template-name-input" />
<ValidationMessage For="() => Model.Name" />
```

### Loading States
```razor
@if (_isLoading)
{
    <WorkoutTemplateCardSkeleton />
}
else
{
    <WorkoutTemplateCard Template="template" />
}
```

### Error Handling
```razor
@if (!string.IsNullOrEmpty(_errorMessage))
{
    <ErrorAlert Message="@_errorMessage" 
                OnDismiss="() => _errorMessage = null"
                OnRetry="LoadData" />
}
```

### Success Notifications
```csharp
ToastService.ShowSuccess("Template created successfully");
```

## API Endpoints Used

| Operation | Method | Endpoint |
|-----------|--------|----------|
| List | GET | /api/workout-templates?{filters} |
| Get One | GET | /api/workout-templates/{id} |
| Create | POST | /api/workout-templates |
| Update | PUT | /api/workout-templates/{id} |
| Delete | DELETE | /api/workout-templates/{id} |
| Duplicate | POST | /api/workout-templates/{id}/duplicate |
| Change State | PUT | /api/workout-templates/{id}/workflow-state |

## Testing Helpers

### Key data-testid Attributes
```
template-card
template-name-input
save-button
cancel-button
delete-button
search-input
category-filter
difficulty-filter
state-filter
visibility-filter
clear-filters-button
```

### Common Test Patterns
```csharp
// Render component
var cut = RenderComponent<WorkoutTemplateCard>(
    p => p.Add(x => x.Template, template));

// Find and click
cut.Find("[data-testid='save-button']").Click();

// Wait for async
await cut.WaitForAssertion(() => 
    mockService.Verify(x => x.CreateAsync(It.IsAny<CreateDto>()), Times.Once));
```

## Troubleshooting

### "Name already exists" Error
- Template names must be unique
- Check both active and archived templates
- Consider adding identifier to name

### Cannot Edit Template
- Check template state (PRODUCTION restrictions)
- Verify you have PT-Tier or Admin-Tier access
- ARCHIVED templates cannot be edited

### Auto-save Not Working
- Only works for DRAFT templates
- Check browser console for errors
- Verify localStorage is enabled

### Missing Equipment Information
- Known limitation - API doesn't aggregate equipment yet
- Shows "Equipment information coming soon"
- This is expected behavior

## Keyboard Shortcuts
- **Tab**: Navigate between form fields
- **Enter**: Submit forms (when valid)
- **Escape**: Close dialogs
- **Arrow keys**: Navigate dropdowns

## Performance Tips
- Use pagination for large lists (10, 25, 50 items)
- Search is debounced - wait 300ms after typing
- Reference data is cached for 5 minutes
- Skeleton loaders show during data fetch

## Related Features
- Exercise Management (view exercises in templates)
- Equipment Management (future integration)
- AI Suggestions (future feature)

## Support & Feedback
For issues or questions:
1. Check manual testing guide for expected behavior
2. Verify API is running at correct endpoint
3. Clear browser cache if experiencing odd behavior
4. Report bugs with specific reproduction steps