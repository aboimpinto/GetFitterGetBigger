# Feature: MuscleGroup DTO Refactoring

## Feature ID: FEAT-018
## Created: 2025-01-07
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description
Refactor the MuscleGroup reference table endpoints to return MuscleGroupDto instead of the generic ReferenceDataDto. This is necessary because MuscleGroup has additional properties (specifically BodyPartId) that are not included in the generic ReferenceDataDto, causing issues in the Admin application where BodyPartId is not available.

## Background
During the implementation of CRUD functionality for reference tables, it was discovered that MuscleGroup is different from other reference tables because it has a foreign key relationship with BodyPart. The current implementation returns ReferenceDataDto which only contains Id, Value, and Description fields, missing the crucial BodyPartId field needed by the Admin application.

## Business Value
- Enables proper CRUD operations for MuscleGroups in the Admin application
- Maintains data integrity by ensuring BodyPartId is always available
- Improves consistency across the API by using the appropriate DTOs for each entity
- Reduces workarounds and technical debt in client applications

## User Stories
- As a Personal Trainer, I want to see which body part a muscle group belongs to when managing muscle groups so that I can organize exercises properly
- As an Admin App developer, I want to receive complete muscle group data including BodyPartId so that I can implement proper CRUD operations without workarounds

## Acceptance Criteria
- [ ] GetAll endpoint returns MuscleGroupDto[] instead of ReferenceDataDto[]
- [ ] GetById endpoint returns MuscleGroupDto instead of ReferenceDataDto
- [ ] Create endpoint accepts CreateMuscleGroupDto and returns MuscleGroupDto
- [ ] Update endpoint accepts UpdateMuscleGroupDto and returns MuscleGroupDto
- [ ] All muscle group endpoints include BodyPartId in their responses
- [ ] Existing functionality is not broken
- [ ] All tests pass
- [ ] API documentation (Swagger) reflects the correct DTOs

## Technical Specifications
### Current Implementation Issues:
1. `MuscleGroupService.GetAllAsDtosAsync()` returns `IEnumerable<ReferenceDataDto>`
2. `MuscleGroupService.GetByIdAsDtoAsync()` returns `ReferenceDataDto`
3. These DTOs don't include BodyPartId, causing issues in client applications

### Proposed Changes:
1. Update `IMuscleGroupService` interface methods to return MuscleGroupDto
2. Update `MuscleGroupService` implementation to return proper DTOs
3. Ensure all controller endpoints use the correct DTOs
4. Update any related tests

### Affected Files:
- `Services/ReferenceData/Interfaces/IMuscleGroupService.cs`
- `Services/ReferenceData/MuscleGroupService.cs`
- `Controllers/ReferenceTables/MuscleGroupsController.cs`
- Related test files

## Dependencies
- No external dependencies
- Must maintain backward compatibility where possible

## Notes
- This is a refactoring task to fix an oversight in the initial implementation
- The MuscleGroupDto already exists and includes all necessary fields
- This change aligns with the principle that each entity should use its specific DTO when it has unique properties