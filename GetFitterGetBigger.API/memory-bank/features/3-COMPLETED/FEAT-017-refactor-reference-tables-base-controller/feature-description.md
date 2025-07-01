# Feature: Refactor ReferenceTablesBase Controller and Create Generic Service Pattern

## Feature ID: FEAT-017
## Created: 2025-01-07
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description
Refactor the ReferenceTablesBaseController and all inheriting controllers to comply with the new architectural rules. This is a comprehensive refactoring that will establish a generic service pattern for all reference table controllers.

## Business Value
- Establishes a reusable pattern for all reference table operations
- Reduces code duplication across multiple controllers
- Improves maintainability through consistent patterns
- Enables better transaction management across all reference tables

## User Stories
- As a developer, I want a generic service pattern for reference tables so that I can quickly implement new reference table endpoints
- As a system architect, I want consistent patterns across all similar controllers so that the codebase is maintainable
- As a QA engineer, I want predictable patterns so that I can write comprehensive tests

## Acceptance Criteria
- [ ] Create IReferenceTableService<T> generic interface
- [ ] Create ReferenceTableServiceBase<T> generic implementation
- [ ] Update ReferenceTablesBaseController to use service pattern
- [ ] Create specific services for each reference table
- [ ] Refactor all inheriting controllers
- [ ] Maintain all existing functionality
- [ ] All existing tests must pass
- [ ] Add unit tests for the new services
- [ ] No direct repository or UnitOfWork access in any controller

## Technical Specifications
### Current State
- ReferenceTablesBaseController directly uses IUnitOfWorkProvider
- 7 controllers inherit from this base controller:
  - BodyPartsController
  - DifficultyLevelsController
  - ExerciseTypesController
  - KineticChainTypesController
  - MetricTypesController
  - MovementPatternsController
  - MuscleRolesController

### Target State
- Generic service interface for reference tables
- Base service implementation with common operations
- Specific service implementations for each reference table
- Controllers only communicate with services
- Caching logic moved to service layer

### Reference Table Controllers to Refactor
1. BodyPartsController → IBodyPartService
2. DifficultyLevelsController → IDifficultyLevelService
3. ExerciseTypesController → IExerciseTypeService
4. KineticChainTypesController → IKineticChainTypeService
5. MetricTypesController → IMetricTypeService
6. MovementPatternsController → IMovementPatternService
7. MuscleRolesController → IMuscleRoleService

## Dependencies
- Must be completed FIRST before FEAT-015 and FEAT-016
- Must maintain compatibility with existing caching strategy
- Must preserve all existing endpoints and functionality
- Requires understanding of existing ExerciseService pattern

## Technical Details
### Generic Interface Definition
```csharp
public interface IReferenceTableService<T> where T : class, IReferenceTable
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T?> GetByNameAsync(string name);
    Task<T?> GetByValueAsync(string value);
    Task<T> CreateAsync(CreateReferenceTableDto<T> dto);
    Task<T> UpdateAsync(string id, UpdateReferenceTableDto<T> dto);
    Task DeleteAsync(string id);
}
```

### Key Implementation Requirements
- Virtual methods in base service for customization
- Proper generic constraints (where T : class, IReferenceTable)
- Caching strategy integration
- Transaction management in service layer
- Validation logic centralization

## Notes
- This is the HIGHEST PRIORITY refactoring as it creates the pattern for all others
- Implementation order: FEAT-017 → FEAT-015 → FEAT-016
- The generic pattern should support special cases through virtual methods
- Equipment and MuscleGroups services can inherit from this base pattern