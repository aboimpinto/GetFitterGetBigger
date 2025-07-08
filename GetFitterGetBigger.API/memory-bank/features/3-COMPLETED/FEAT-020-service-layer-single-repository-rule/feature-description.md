# Feature: Service Layer Single Repository Rule Enforcement

## Feature ID: FEAT-020
## Created: 2025-01-08
## Status: IN_PROGRESS
## Target PI: PI-2025-Q1

## Description
Implement architectural refactoring to enforce the Single Repository Rule in the service layer. Each service should only directly access its own repository. When a service needs data from another entity, it should communicate through the corresponding service rather than accessing the repository directly. This ensures proper separation of concerns and maintains clear service boundaries.

## Business Value
- **Maintainability**: Clear service boundaries make the codebase easier to understand and modify
- **Testability**: Services can be tested in isolation by mocking other service dependencies
- **Consistency**: Establishes a uniform pattern across all services
- **Encapsulation**: Business logic remains properly encapsulated within the appropriate service
- **Flexibility**: Services can evolve independently without affecting others

## User Stories
- As a developer, I want each service to have a single responsibility so that I can easily understand and modify the code
- As a developer, I want to mock service dependencies in tests so that unit tests are simpler and more focused
- As a tech lead, I want consistent architectural patterns so that the codebase is maintainable and scalable

## Acceptance Criteria
- [ ] Each service accesses only its own repository directly
- [ ] Cross-entity validations are performed through service-to-service calls
- [ ] Transactional operations properly share WritableUnitOfWork across services
- [ ] All existing tests pass after refactoring
- [ ] New tests cover service-to-service communication patterns
- [ ] No performance degradation from the refactoring
- [ ] Documentation updated to reflect the new pattern

## Technical Specifications

### Current Violations
1. **ExerciseService**: Directly accesses `IExerciseTypeRepository`
2. **MuscleGroupService**: Directly accesses `IBodyPartRepository`  
3. **AuthService**: Directly accesses `IClaimRepository`

### Required New Services
1. `IBodyPartService` / `BodyPartService`
2. `IExerciseTypeService` / `ExerciseTypeService`
3. `IClaimService` / `ClaimService`

### Transaction Patterns
1. **Read-Only Pattern**: Services call other services for validation queries
2. **Transactional Pattern**: WritableUnitOfWork passed between services for atomic operations

### Refactoring Scope
- Update 3 existing services to remove cross-repository access
- Create 3 new services for the entities being accessed
- Update all affected unit and integration tests
- Update dependency injection configuration

## Dependencies
- Existing service infrastructure must support service-to-service communication
- Unit of Work pattern must support passing between services
- All reference table entities should have corresponding services

## Notes
- This refactoring aligns with the existing architecture refactoring initiative (ARCHITECTURE-REFACTORING-INITIATIVE.md)
- No API contracts will change - this is purely an internal architecture improvement
- Consider implementing a generic reference table service pattern to reduce boilerplate
- Performance impact should be minimal as services can use caching for frequently accessed data