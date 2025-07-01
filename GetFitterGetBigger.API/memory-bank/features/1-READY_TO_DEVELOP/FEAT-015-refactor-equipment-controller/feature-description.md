# Feature: Refactor Equipment Controller to Use Service Layer

## Feature ID: FEAT-015
## Created: 2025-01-07
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description
Refactor the EquipmentController to comply with the new architectural rules. The controller currently violates the architecture by directly accessing IUnitOfWorkProvider and IEquipmentRepository. This needs to be refactored to use a service layer instead.

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
- [ ] Create IEquipmentService interface
- [ ] Create EquipmentService implementation
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
- Controller directly accesses IEquipmentRepository
- Controller handles transaction commits

### Target State
- Controller only injects IEquipmentService
- Service handles all UnitOfWork creation and management
- Service accesses IEquipmentRepository
- Service manages transaction commits
- Controller focuses only on HTTP concerns

### Methods to Move to Service
- GetAll()
- GetById(string id)
- GetByName(string name)
- GetByValue(string value)
- Create(CreateEquipmentDto request)
- Update(string id, UpdateEquipmentDto request)
- Delete(string id)

## Dependencies
- **PREREQUISITE**: FEAT-017 must be completed first
- Must follow the patterns established in ExerciseService
- Must maintain compatibility with existing DTOs
- Must preserve caching functionality
- Should inherit from ReferenceTableServiceBase<Equipment>

## Technical Details
### Service Implementation
- EquipmentService should inherit from ReferenceTableServiceBase<Equipment>
- Most functionality will be inherited from the base class
- May need to override methods for equipment-specific validation

### Validation Requirements
- Equipment name must be unique
- Equipment value must be unique
- Validate equipment not in use before deletion

### Caching Strategy
- Inherit caching behavior from base service
- Cache equipment by ID, name, and value
- Invalidate cache on any modification

## Notes
- This is part of a larger architectural refactoring effort
- Simpler than MuscleGroups as it's a pure reference table
- Good example of standard reference table service pattern
- Can serve as a template for other simple reference tables