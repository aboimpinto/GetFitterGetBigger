# Refactor "Success but Empty" Pattern in Reference Services

## Problem Statement
Currently, reference services (like `WorkoutObjectiveService`) return `ServiceResult.Success(ReferenceDataDto.Empty)` when an entity is not found or inactive. This creates semantic confusion:
- Success should mean the operation completed successfully with valid data
- Empty data in a Success result is ambiguous - was it not found? Inactive? Deleted?

## Current Behavior
```csharp
// In WorkoutObjectiveService.LoadByIdFromDatabaseAsync
return entity.IsActive
    ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
    : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);  // Confusing!

// Then at API layer
if (result.IsSuccess && result.Data.IsEmpty)
{
    return ServiceResult<ReferenceDataDto>.Failure(
        ReferenceDataDto.Empty,
        ServiceError.NotFound(errorMessage));
}
```

## Proposed Solution
Services should return clear semantic results:
- `Success` = Operation succeeded with valid data
- `Failure(NotFound)` = Resource doesn't exist or is inactive
- `Failure(Other)` = Operation failed (network, validation, etc.)

## Implementation Plan
1. Update all reference services to return `Failure(NotFound)` instead of `Success(Empty)`
2. Remove the "Success but Empty" conversion logic from API layer
3. Update tests to expect the new behavior
4. Ensure caching logic handles failures correctly

## Affected Services
- WorkoutObjectiveService
- WorkoutCategoryService
- WorkoutStateService
- DifficultyLevelService
- ExerciseTypeService
- KineticChainTypeService
- MuscleRoleService
- All other reference data services

## Benefits
- Clearer semantics
- Simpler code (no conversion needed)
- Better alignment with REST principles
- Easier to understand and maintain