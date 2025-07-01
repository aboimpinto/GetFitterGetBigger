# Feature: Refactor MuscleGroups Controller to Use Service Layer

## Feature ID: FEAT-016
## Created: 2025-01-07
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description
Refactor the MuscleGroupsController to comply with the new architectural rules. The controller currently violates the architecture by directly accessing IUnitOfWorkProvider, IMuscleGroupRepository, and IBodyPartRepository. This needs to be refactored to use a service layer instead.

## Business Value
- Improves code maintainability by enforcing separation of concerns
- Enables better transaction management through centralized service layer
- Makes the codebase more testable by isolating business logic in services
- Ensures consistency across all controllers in the application

## User Stories
- As a developer, I want controllers to only communicate with services so that business logic is properly isolated
- As a system architect, I want consistent architectural patterns so that the codebase is maintainable
- As a QA engineer, I want better testability so that I can write more comprehensive unit tests

## Acceptance Criteria
- [ ] Create IMuscleGroupService interface
- [ ] Create MuscleGroupService implementation
- [ ] Move all repository access from controller to service
- [ ] Move all UnitOfWork usage from controller to service
- [ ] Update controller to only use the service
- [ ] Maintain all existing functionality
- [ ] All existing tests must pass
- [ ] Add unit tests for the new service
- [ ] No direct repository or UnitOfWork access in controller

## Technical Specifications
### Current State
- Controller directly injects IUnitOfWorkProvider<FitnessDbContext>
- Controller creates UnitOfWork instances (both ReadOnly and Writable)
- Controller directly accesses IMuscleGroupRepository and IBodyPartRepository
- Controller handles transaction commits
- Complex logic for adding/removing body parts

### Target State
- Controller only injects IMuscleGroupService
- Service handles all UnitOfWork creation and management
- Service accesses repositories
- Service manages transaction commits
- Service encapsulates business logic for body part associations
- Controller focuses only on HTTP concerns

### Methods to Move to Service
- GetAll()
- GetByBodyPart(string bodyPartId)
- GetById(string id)
- AddBodyPart(string id, AddBodyPartRequest request)
- RemoveBodyPart(string id, string bodyPartId)

## Dependencies
- Must follow the patterns established in ExerciseService
- Must maintain compatibility with existing DTOs
- Must handle cross-repository operations (MuscleGroup and BodyPart)

## Notes
- This controller has more complex business logic than simple reference tables
- Involves cross-entity operations between MuscleGroups and BodyParts
- Good example of why service layer is needed for complex operations