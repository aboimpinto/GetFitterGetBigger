# BUG-001: Exercise List Pagination Reset on Filter Change

## Bug Details
- **ID**: BUG-001
- **Created**: 2025-06-29
- **Reporter**: System Admin
- **Severity**: Medium
- **Priority**: Medium
- **Status**: OPEN
- **Component**: Exercise Management
- **Related Feature**: FEAT-009 (Exercise CRUD)

## Description
When users apply filters on the Exercise List page and then change the filter criteria, the pagination does not reset to page 1. This causes confusion as users might be viewing page 3 of filtered results, apply a new filter, and see "No results" because they're still on page 3 of the new filtered set.

## Steps to Reproduce
1. Navigate to Exercise List page
2. Scroll through several pages (e.g., go to page 3)
3. Apply a filter (e.g., select "Warmup" exercise type)
4. Change the filter (e.g., select "Workout" exercise type)
5. Observe that pagination remains on page 3

## Expected Behavior
When any filter is changed, pagination should automatically reset to page 1 to show the first results of the newly filtered set.

## Actual Behavior
Pagination remains on the current page number, potentially showing no results even when filtered results exist.

## Technical Details
- Component: `/Components/Pages/Exercises/ExerciseList.razor`
- Service: `ExerciseStateService.cs`
- The filter change handler needs to reset `CurrentPage` to 1

## Proposed Solution
In `ExerciseList.razor`, modify the filter change handlers to reset pagination:
```csharp
private async Task OnFilterChanged()
{
    StateService.CurrentPage = 1; // Add this line
    await LoadExercises();
}
```

## Workarounds
Users can manually click on page 1 after changing filters.

## Screenshots/Logs
N/A - UI behavior issue

## Environment
- Browser: All browsers
- OS: All operating systems
- Version: Current main branch

## Additional Notes
This is a common UX issue in paginated lists with filters. Consider implementing this pattern across all paginated lists in the application.