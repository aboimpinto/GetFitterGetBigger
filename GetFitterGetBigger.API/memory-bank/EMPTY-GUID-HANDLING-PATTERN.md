# Empty GUID Handling Pattern for Reference Tables

## Overview

This document defines the standard pattern for handling empty GUIDs (00000000-0000-0000-0000-000000000000) in reference table services. This pattern ensures consistent behavior across all reference table endpoints.

## Pattern Definition

### Empty GUIDs Should Be Treated as Valid IDs

When a service receives an empty GUID (e.g., `bodypart-00000000-0000-0000-0000-000000000000`), it should:

1. **NOT** return a validation error (400 Bad Request)
2. **INSTEAD** treat it as a valid ID that doesn't exist
3. **RETURN** NotFound (404) when the ID doesn't exist in the database

### Invalid Formats Should Return Bad Request

When a service receives an ID without the proper prefix (e.g., `7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c` instead of `bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c`):

1. **RETURN** a validation error (400 Bad Request)
2. **ERROR MESSAGE**: "Invalid {entity} ID format. Expected format: '{prefix}-{guid}'"

### Implementation Pattern

#### Before (Incorrect)
```csharp
public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(EntityId id) => 
    id.IsEmpty 
        ? ServiceResult<ReferenceDataDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed("Invalid entity ID"))
        : await GetByIdAsync(id.ToString());
```

#### After (Correct)
```csharp
public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(EntityId id) => 
    await GetByIdAsync(id.ToString());
```

## Rationale

1. **Consistency**: All valid GUID formats should be handled the same way
2. **Database Authority**: The database should be the authority on whether an ID exists
3. **API Predictability**: Clients expect 404 for non-existent resources, not 400
4. **Empty Pattern Compatibility**: Works seamlessly with the Empty/Null Object Pattern

## Applied To

- [x] BodyPartService
- [x] MovementPatternService
- [ ] DifficultyLevelService
- [ ] ExerciseTypeService
- [ ] KineticChainTypeService
- [ ] MuscleRoleService
- [ ] WorkoutObjectiveService (FEAT-025)
- [ ] WorkoutCategoryService (FEAT-025)
- [ ] ExecutionProtocolService (FEAT-025)
- [ ] MuscleGroupService
- [ ] EquipmentService
- [ ] MetricTypeService
- [ ] ExerciseWeightTypeService

## Testing Pattern

### Unit Tests
```csharp
[Fact]
public async Task GetByIdAsync_WithEmptyEntityId_ReturnsNotFound()
{
    // Arrange
    var emptyEntityId = EntityId.Empty;
    _mockRepository
        .Setup(x => x.GetByIdAsync(emptyEntityId))
        .ReturnsAsync(Entity.Empty);

    // Act
    var result = await _service.GetByIdAsync(emptyEntityId);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    _mockRepository.Verify(x => x.GetByIdAsync(emptyEntityId), Times.Once);
}
```

### BDD Tests
```gherkin
Scenario: Get entity by empty GUID returns not found
    When I send a GET request to "/api/ReferenceTables/Entities/entity-00000000-0000-0000-0000-000000000000"
    Then the response status should be 404
```

## Migration Notes

When migrating existing services:
1. Remove the empty ID check from GetByIdAsync
2. Update unit tests to expect NotFound instead of ValidationFailed
3. Ensure BDD tests don't check for validation error messages
4. The repository will return Entity.Empty, which the service will treat as NotFound

## Related Patterns

- [Empty/Null Object Pattern](./EMPTY-PATTERN-MIGRATION-GUIDE.md)
- [Service Result Pattern](./SERVICE-RESULT-PATTERN.md)