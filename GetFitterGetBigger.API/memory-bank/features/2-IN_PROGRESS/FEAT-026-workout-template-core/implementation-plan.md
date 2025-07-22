# Workout Template Core - Implementation Plan

## Overview
This document outlines the technical implementation plan for the Workout Template Core feature following clean architecture principles and established project patterns.

## Implementation Phases

### Phase 1: Database Foundation
1. **Entity Creation**
   - Create `WorkoutTemplate` entity
   - Create `WorkoutTemplateExercise` entity
   - Create `SetConfiguration` entity
   - Create `WorkoutState` entity (reference table)

2. **EF Core Configuration**
   - Configure relationships and constraints
   - Set up cascade delete rules
   - Configure indexes for performance
   - Add audit fields (CreatedAt, LastModified)

3. **Database Migration**
   - Create initial migration
   - Seed WorkoutState reference data
   - Test migration rollback/forward

### Phase 2: Repository Layer
1. **Repository Interfaces**
   - `IWorkoutTemplateRepository`
   - `IWorkoutTemplateExerciseRepository`
   - `ISetConfigurationRepository`
   - `IWorkoutStateRepository` (extends IEnhancedReferenceTableRepository)

2. **Repository Implementations**
   - Implement all repository interfaces
   - Include eager loading for navigation properties
   - Add specification pattern support
   - Implement soft delete logic

### Phase 3: Service Layer
1. **Service Interfaces**
   - `IWorkoutTemplateService`
   - `IWorkoutTemplateExerciseService`
   - `ISetConfigurationService`
   - `IWorkoutStateService` (extends IEnhancedReferenceTableService)

2. **Service Implementations**
   - Implement business logic with ServiceResult pattern
   - Add validation logic
   - Implement state transition rules
   - Add equipment aggregation logic
   - Implement exercise suggestion algorithm

3. **DTOs and Mapping**
   - Create all DTOs (Request/Response)
   - Configure AutoMapper profiles
   - Add validation attributes

### Phase 4: API Controllers
1. **Controller Implementation**
   - `WorkoutTemplatesController`
   - `WorkoutStatesController` (extends BaseEnhancedReferenceTableController)

2. **Endpoint Implementation**
   - Implement all CRUD endpoints
   - Add state transition endpoint
   - Add exercise suggestion endpoint
   - Implement duplicate functionality

3. **Swagger Documentation**
   - Add XML documentation
   - Configure Swagger examples
   - Document error responses

### Phase 5: Testing
1. **Unit Tests**
   - Entity validation tests
   - Service logic tests
   - State transition tests
   - Business rule validation

2. **Integration Tests**
   - API endpoint tests
   - Database operation tests
   - Transaction rollback tests
   - Caching behavior tests

3. **BDD Tests**
   - Template creation scenarios
   - State transition scenarios
   - Exercise management scenarios
   - Permission validation scenarios

## Technical Architecture

### Entity Relationships
```
WorkoutTemplate (1) --> (N) WorkoutTemplateExercise
WorkoutTemplateExercise (1) --> (N) SetConfiguration
WorkoutTemplate --> WorkoutCategory (Reference)
WorkoutTemplate --> WorkoutObjective (Reference)
WorkoutTemplate --> ExecutionProtocol (Reference)
WorkoutTemplate --> WorkoutState (Reference)
WorkoutTemplateExercise --> Exercise (Reference)
SetConfiguration --> ExecutionProtocol (Reference)
```

### Service Layer Patterns
- **ServiceResult Pattern**: All service methods return ServiceResult<T>
- **UnitOfWork Pattern**: ReadOnlyUnitOfWork for queries, WritableUnitOfWork for commands
- **Repository Pattern**: One repository per aggregate root
- **Specification Pattern**: For complex queries

### Caching Strategy
```csharp
// Reference data - eternal cache
[OutputCache(Duration = 31536000)] // 365 days
public async Task<IActionResult> GetWorkoutStates()

// Template lists - short cache
[OutputCache(Duration = 300)] // 5 minutes
public async Task<IActionResult> GetTemplates()

// Individual templates - medium cache
[OutputCache(Duration = 3600)] // 1 hour
public async Task<IActionResult> GetTemplate(Guid id)
```

### Error Handling Approach
```csharp
// Service layer
return ServiceResult<T>.Failure("Error message", ErrorType.Validation);

// Controller layer
return result.ToActionResult();
```

## Key Implementation Considerations

### State Machine Implementation
```csharp
public class WorkoutStateTransitionValidator
{
    private readonly Dictionary<(string from, string to), Func<WorkoutTemplate, bool>> _transitions;
    
    public ServiceResult ValidateTransition(WorkoutTemplate template, string targetState)
    {
        // Implement transition rules
    }
}
```

### Equipment Aggregation
```csharp
public class EquipmentAggregator
{
    public List<string> AggregateEquipment(WorkoutTemplate template)
    {
        return template.Exercises
            .SelectMany(e => e.Exercise.Equipment)
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }
}
```

### Exercise Suggestion Algorithm
```csharp
public class ExerciseSuggestionService
{
    public List<ExerciseSuggestion> GetSuggestions(
        WorkoutTemplate template, 
        string zone, 
        int limit = 10)
    {
        // Consider:
        // - Workout category alignment
        // - Push/pull balance
        // - Equipment availability
        // - Muscle group coverage
        // - Associated warmups/cooldowns
    }
}
```

## Dependencies
- Entity Framework Core 9.0
- AutoMapper
- FluentValidation
- xUnit for testing
- Moq for mocking
- Respawn for integration tests

## Migration Strategy
1. Create feature branch from latest main
2. Implement in isolated namespace
3. Run all existing tests to ensure no regression
4. Deploy to staging for testing
5. Gradual rollout with feature flags

## Performance Considerations
- Index on frequently queried fields (CategoryId, ObjectiveId, CreatorId)
- Eager loading for template details endpoint
- Pagination for list endpoints
- Caching for reference data
- Async/await throughout

## Security Implementation
- Role-based authorization attributes
- Owner validation for modifications
- Private template visibility rules
- Audit logging for all changes

## Monitoring and Logging
- Log all state transitions
- Track template creation metrics
- Monitor cache hit rates
- Alert on validation failures

## Rollback Plan
1. Feature flag to disable new endpoints
2. Keep old tables if migrating from legacy
3. Database migration rollback scripts
4. Cache invalidation procedures