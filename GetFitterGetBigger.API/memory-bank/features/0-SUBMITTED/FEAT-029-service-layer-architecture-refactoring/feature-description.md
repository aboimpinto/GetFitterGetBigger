# Feature FEAT-029: Service Layer Architecture Refactoring - Remove UnitOfWork from Services

## Feature ID
**FEAT-029**

## Title
Service Layer Architecture Refactoring - Remove UnitOfWork from Services

## Status
SUBMITTED

## Priority
High (Architectural Foundation)

## Category
Architecture / Refactoring

## Problem Statement

The current architecture has Services directly managing UnitOfWork, which violates separation of concerns:

```csharp
// Current Problem - Service knows about data access concerns
public class BodyPartService : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();  // ← Data access concern!
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        // ...
    }
}
```

**Issues:**
1. Services mix business logic with data access concerns
2. UnitOfWork management scattered across service methods
3. Transaction boundaries are implicit and hard to test
4. Difficult to mock data access for unit testing
5. No clear separation between business rules and data access rules

## Evolution of This Design

### 1. Initial ServiceValidate Pattern Discussion

We started by refactoring BodyPartService to use the new ServiceValidate fluent API patterns:

```csharp
// Initial refactoring with ServiceValidate
public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
{
    return await ServiceValidate.For<BodyPartDto>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
        .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
        .ThenMatchDataAsync<BodyPartDto, BodyPartDto>(
            whenEmpty: () => Task.FromResult(
                ServiceResult<BodyPartDto>.Failure(
                    BodyPartDto.Empty,
                    ServiceError.NotFound("BodyPart", id.ToString()))),
            whenNotEmpty: dto => Task.FromResult(
                ServiceResult<BodyPartDto>.Success(dto))
        );
}
```

### 2. Pattern Extraction Journey

We then extracted database access to private methods for consistency:

```csharp
// Before extraction - inline database access
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
{
    return await ServiceValidate.For<BooleanResultDto>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
        .MatchAsync(
            whenValid: async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IBodyPartRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
            });
}

// After extraction - consistent pattern
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
{
    return await ServiceValidate.For<BooleanResultDto>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
        .MatchAsync(
            whenValid: async () => await CheckExistsInDatabaseAsync(id)
        );
}

private async Task<ServiceResult<BooleanResultDto>> CheckExistsInDatabaseAsync(BodyPartId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IBodyPartRepository>();
    var exists = await repository.ExistsAsync(id);
    return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
}
```

### 3. Collections Handling Discovery

We discovered that collections need special handling since they don't implement IEmptyDto:

```csharp
// Collections use Build<T>().WhenValidAsync()
public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
{
    return await ServiceValidate.Build<IEnumerable<BodyPartDto>>()
        .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
}
```

### 4. The Realization

During refactoring, we realized that these private methods should actually be in a separate DataAccess layer:

**Current Structure:**
```
Controller → Service (with UnitOfWork) → Repository → Database
```

**Desired Structure:**
```
Controller → Service (Pure Business) → DataAccess → Repository → Database
```

### 5. The Transaction Challenge

The key challenge identified: How to handle transactions when business rules need to coordinate multiple data operations?

**The Problem:** 
- Business layer defines transaction boundaries (business concept)
- DataAccess layer needs the same UnitOfWork for transaction consistency
- We don't want Services to know about UnitOfWork

## Proposed Solution

### Architecture Layers

```
Controller (Entry Point)
    ↓
Service (Pure Business Logic - NO UnitOfWork!)
    ↓
ServiceValidate (Transaction Management via Scopes)
    ↓
DataAccess (Translates Scope to UnitOfWork)
    ↓
Repository (Pure Data Operations)
    ↓
Database
```

### Key Components

#### 1. Transaction Scope Abstraction

```csharp
public interface ITransactionScope
{
    // Abstraction - Service doesn't know about UnitOfWork!
}

internal class TransactionScopeImpl : ITransactionScope
{
    internal IWritableUnitOfWork UnitOfWork { get; }
    
    public TransactionScopeImpl(IWritableUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }
}
```

#### 2. Pure Service Layer

```csharp
public class BodyPartService : IBodyPartService
{
    private readonly IBodyPartDataAccess _dataAccess;
    // No UnitOfWorkProvider here!
    
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _dataAccess.GetByIdAsync(id)
            );
    }
    
    // Complex transaction example
    public async Task<ServiceResult<WorkoutDto>> CreateCompleteWorkoutAsync(CreateWorkoutCommand command)
    {
        return await ServiceValidate.For<WorkoutDto>()
            .EnsureNotNull(command, "Command cannot be null")
            .WithTransactionScopeAsync(async (scope) =>  // Transaction scope injected!
            {
                var workout = await _workoutDataAccess.CreateAsync(command.ToWorkout(), scope);
                
                foreach (var exercise in command.Exercises)
                {
                    await _exerciseDataAccess.AddToWorkoutAsync(workout.Id, exercise, scope);
                }
                
                return ServiceResult<WorkoutDto>.Success(MapToDto(workout));
            });
            // Transaction commits automatically on success
    }
}
```

#### 3. DataAccess Layer

```csharp
public class BodyPartDataAccess : IBodyPartDataAccess
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    // Simple queries - manages own UnitOfWork
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<BodyPartDto>.Success(MapToDto(entity))
            : ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty);
    }
    
    // Transactional operations - accepts scope
    public async Task<ServiceResult<BodyPartDto>> CreateAsync(BodyPart entity, ITransactionScope scope)
    {
        var unitOfWork = ((TransactionScopeImpl)scope).UnitOfWork;
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        
        var created = await repository.CreateAsync(entity);
        return ServiceResult<BodyPartDto>.Success(MapToDto(created));
    }
}
```

#### 4. ServiceValidate Transaction Extension

```csharp
public static class ServiceValidateTransactionExtensions
{
    public static async Task<ServiceResult<T>> WithTransactionScopeAsync<T>(
        this ServiceValidation<T> validation,
        Func<ITransactionScope, Task<ServiceResult<T>>> operation) 
        where T : IEmptyDto<T>
    {
        if (!validation.IsValid)
            return ServiceResult<T>.Failure(T.Empty, validation.Errors);
        
        using var unitOfWork = GetUnitOfWorkProvider().CreateWritable();
        var scope = new TransactionScopeImpl(unitOfWork);
        
        try
        {
            var result = await operation(scope);
            
            if (result.IsSuccess)
                await unitOfWork.SaveChangesAsync();
            
            return result;
        }
        catch (Exception ex)
        {
            // Automatic rollback
            return ServiceResult<T>.Failure(T.Empty, ServiceError.InternalError());
        }
    }
}
```

## ServiceValidate Flow Patterns

### Core Scenarios Identified

Through our discussion, we identified four core flow scenarios:

#### Scenario 1: No Pre-validation → Execution → No Post-validation
```csharp
public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
{
    return await ServiceValidate.Build<IEnumerable<BodyPartDto>>()
        .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
}
```

#### Scenario 2: With Pre-validation → Execution → No Post-validation
```csharp
public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
{
    return await ServiceValidate.For<BodyPartDto>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)  // Pre-validation
        .MatchAsync(
            whenValid: async () => await LoadByIdFromDatabaseAsync(id)  // Execution
        );
}
```

#### Scenario 3: With Pre-validation → Execution → With Post-validation
```csharp
public async Task<ServiceResult<WorkoutTemplateDto>> CreateTemplateAsync(CreateTemplateCommand command)
{
    return await ServiceValidate.For<WorkoutTemplateDto>()
        .EnsureNotNull(command, "Command cannot be null")  // Pre-validation
        .AsAsync()
        .WithServiceResultAsync(() => CreateTemplateInDatabaseAsync(command))  // Execution
        .ThenMatchDataAsync<WorkoutTemplateDto, WorkoutTemplateDto>(
            whenEmpty: () => /* handle empty */,
            whenNotEmpty: async template => await ValidateTemplateStateAsync(template)  // Post-validation
        );
}
```

#### Scenario 4: No Pre-validation → Execution → With Post-validation
```csharp
public async Task<ServiceResult<SystemHealthDto>> CheckSystemHealthAsync()
{
    return await ServiceValidate.For<SystemHealthDto>()
        .WithServiceResultAsync(() => LoadSystemMetricsAsync())  // Execution
        .ThenMatchDataAsync<SystemHealthDto, SystemHealthDto>(
            whenEmpty: () => /* handle empty */,
            whenNotEmpty: async metrics => await ValidateHealthThresholdsAsync(metrics)  // Post-validation
        );
}
```

### Future Vision - Multiple Chained Executions

We envision a future pattern for complex workflows:

```csharp
// FUTURE PATTERN - Not yet implemented
public async Task<ServiceResult<WorkoutSessionDto>> StartWorkoutSessionAsync(StartSessionCommand command)
{
    return await ServiceValidate.For<WorkoutSessionDto>()
        // Execution 1: Create session
        .ExecuteAsync(async () => await CreateSessionInDatabaseAsync(command))
        // Execution 2: Receives result from Execution 1
        .ThenExecuteAsync<SessionDto>(async (session) => 
            await LoadExercisesForSessionAsync(session.TemplateId))
        // Execution 3: Receives results from Execution 1 & 2
        .ThenExecuteAsync<SessionDto, ExerciseListDto>(async (session, exercises) => 
            await InitializeTrackingAsync(session.Id, exercises))
        // Post-validation: Receives all previous results
        .ThenValidateAsync<SessionDto, ExerciseListDto, TrackingDto>(
            async (session, exercises, tracking) => 
                await ValidateSessionReadyAsync(session, exercises, tracking))
        // Final transformation
        .ThenMapAsync<SessionDto, ExerciseListDto, TrackingDto, WorkoutSessionDto>(
            (session, exercises, tracking) => new WorkoutSessionDto
            {
                Session = session,
                Exercises = exercises,
                Tracking = tracking
            });
}
```

## Benefits

1. **Pure Business Logic**: Services focus only on business rules
2. **Clean Separation**: Clear boundaries between layers
3. **Transaction Management**: Declarative via ServiceValidate
4. **Testability**: Easy to mock DataAccess layer
5. **Consistency**: All services follow same pattern
6. **Flexibility**: Can handle simple and complex scenarios
7. **Type Safety**: Full compile-time checking
8. **Automatic Resource Management**: Transaction scope handles commit/rollback

## Implementation Strategy

### Phase 1: Foundation (When Needed)
1. Create ITransactionScope interface
2. Implement ServiceValidate transaction extensions
3. Create first DataAccess class as proof of concept

### Phase 2: Migration Path
1. Start with new services using this pattern
2. Refactor existing services gradually
3. Keep backward compatibility during transition

### Phase 3: Advanced Features
1. Implement chained executions when needed
2. Add parallel execution support
3. Enhance with conditional flows

## When to Implement

Implement this architecture when:
- Starting a new major feature that requires clean architecture
- Refactoring existing services that have become complex
- Need better testability for business logic
- Want to separate concerns more clearly
- Have complex transaction requirements

## Notes

- BodyPart is a PureReferenceTable and won't need create/update operations
- This pattern is most valuable for entities with complex business logic
- Start simple, add complexity only when needed
- Document usage patterns as they emerge

## Related Documentation

This feature was developed through extensive discussion and iteration, documented in:
- API-CODE_QUALITY_STANDARDS.md - Updated with ServiceValidate flow patterns
- The complete conversation history showing the evolution of this design

## Decision

**Status**: SUBMITTED for review and future implementation when appropriate

This architectural refactoring represents a significant improvement in separation of concerns and will make the codebase more maintainable, testable, and aligned with clean architecture principles.