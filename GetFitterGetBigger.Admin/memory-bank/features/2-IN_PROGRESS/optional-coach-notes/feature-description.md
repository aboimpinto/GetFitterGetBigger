# Optional Coach Notes Feature

## Feature Branch: `feature/optional-coach-notes`

## Overview
Currently, the exercise creation/edit form requires at least one coach note to be provided. This constraint is incorrect - coach notes should be completely optional, allowing Personal Trainers to decide whether to add coaching instructions or not.

## Problem Statement
The current implementation has validation that prevents exercise creation/updating if no coach notes are provided:

```csharp
// Current validation in ExerciseForm.razor
if (!model.CoachNotes.Any() || model.CoachNotes.All(cn => string.IsNullOrWhiteSpace(cn.Text)))
    validationErrors["CoachNotes"] = "At least one coach note is required";
```

This forces users to add empty or meaningless coach notes just to pass validation.

## Solution
Remove the required validation for coach notes while maintaining:
- The ability to add multiple coach notes when desired
- Proper validation for individual coach note text length (1000 characters max)
- All existing coach notes functionality (add, delete, reorder)
- Empty state handling when no coach notes are provided

## Business Rules
- **Coach notes are completely optional** - exercises can be created/updated with zero coach notes
- **Individual coach notes still have character limits** - max 1000 characters per note
- **Empty coach notes are automatically removed** - notes with only whitespace are filtered out before submission
- **Coach notes functionality remains unchanged** - add, delete, reorder all work as before
- **Empty state is properly handled** - UI shows appropriate message when no notes exist

## Technical Impact
- Remove coach notes validation from ExerciseForm
- Update form submission to filter out empty coach notes
- Update tests to reflect optional nature
- Ensure API can handle exercises with empty coach notes array

## Success Criteria
- ✅ Exercise can be created without any coach notes
- ✅ Exercise can be updated to remove all coach notes
- ✅ Coach notes editor still functions properly when notes are provided
- ✅ Form validation no longer blocks submission due to missing coach notes
- ✅ API accepts exercises with empty coach notes array
- ✅ All existing tests pass with updated validation logic