# BUG-003: Exercise Update Sets IsActive to False When Field Not Provided

## Bug ID: BUG-003
## Reported: 2025-01-27
## Status: FIXED
## Severity: High
## Affected Version: Current
## Fixed Version: Current (Implemented in ExerciseService.UpdateAsync)

## Description
When updating an exercise through the API, if the `IsActive` field is not included in the update request (common when the Admin UI doesn't send all fields), the API sets `IsActive` to `false`. This causes exercises to disappear from the default view which filters out inactive exercises.

## Error Message
No error message - exercises silently become inactive after updates.

## Reproduction Steps
1. Create an exercise with IsActive = true
2. Update the exercise from Admin UI without sending the IsActive field
3. The API sets IsActive = false (default bool value)
4. Exercise no longer appears in GET /api/Exercises (unless IncludeInactive=true)
5. This creates the appearance that exercises are "disappearing"

## Root Cause
The UpdateExerciseRequest DTO had non-nullable bool properties for `IsActive` and `IsUnilateral`. When these fields are not sent in the request, they default to `false`. The update logic was replacing the entire entity instead of doing a partial update.

## Impact
- Users affected: All users updating exercises
- Features affected: Exercise management, exercise visibility
- Business impact: Exercises become invisible after any update that doesn't explicitly include IsActive=true

## Workaround
Always include IsActive=true in update requests, or use IncludeInactive=true when fetching exercises.

## Test Data
Any exercise update that doesn't include the IsActive field.

## Fix Summary
1. Changed `IsActive` and `IsUnilateral` to nullable bool? in UpdateExerciseRequest
2. Modified UpdateAsync to fetch the existing exercise first
3. Use existing values for nullable fields when not provided in the request
4. This implements proper partial update semantics

## Fix Verification
The fix has been implemented in:
- `UpdateExerciseRequest.cs:51,56` - Both `IsUnilateral` and `IsActive` are now nullable bool? properties
- `ExerciseService.cs:253-254` - The UpdateAsync method uses null-coalescing to preserve existing values:
  ```csharp
  request.IsUnilateral ?? existingExercise.IsUnilateral,
  request.IsActive ?? existingExercise.IsActive,
  ```

This ensures that when these fields are not provided in the request (null), the existing values are preserved rather than defaulting to false.