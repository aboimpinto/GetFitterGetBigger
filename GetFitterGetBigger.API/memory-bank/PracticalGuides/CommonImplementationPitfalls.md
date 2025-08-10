# Common Implementation Pitfalls

This document lists common mistakes and pitfalls encountered during implementation, along with their solutions.

## 1. ⚠️ Using WritableUnitOfWork for Validation Queries

### The Problem
One of the most frequent and dangerous mistakes is using `WritableUnitOfWork` for ALL operations in a service method, including validation queries. This causes Entity Framework to track entities that should not be tracked.

### Symptoms
- Multiple UPDATE statements in the database when you expect only one
- Unwanted updates to reference tables (e.g., BodyPart being updated when updating a MuscleGroup)
- Performance degradation due to unnecessary entity tracking
- Confusing SQL logs showing updates to tables you didn't intend to modify

### Real Example
```csharp
// ❌ WRONG - This caused BodyPart to be updated when updating MuscleGroup
public async Task<MuscleGroupDto> UpdateMuscleGroupAsync(string id, UpdateDto request)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var muscleGroupRepo = unitOfWork.GetRepository<IMuscleGroupRepository>();
    var bodyPartRepo = unitOfWork.GetRepository<IBodyPartRepository>();
    
    // This tracks the BodyPart entity!
    var bodyPart = await bodyPartRepo.GetByIdAsync(request.BodyPartId);
    if (bodyPart == null) throw new ArgumentException("Body part not found");
    
    var muscleGroup = await muscleGroupRepo.GetByIdAsync(id);
    var updated = MuscleGroup.Handler.Update(muscleGroup, request.Name, request.BodyPartId);
    await muscleGroupRepo.UpdateAsync(updated);
    
    await unitOfWork.CommitAsync();
    // Result: BOTH MuscleGroup AND BodyPart were updated!
}
```

### The Solution
```csharp
// ✅ CORRECT - Only MuscleGroup is updated
public async Task<MuscleGroupDto> UpdateMuscleGroupAsync(string id, UpdateDto request)
{
    // Use ReadOnly for validation
    using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
    {
        var bodyPartRepo = readOnlyUow.GetRepository<IBodyPartRepository>();
        var bodyPart = await bodyPartRepo.GetByIdAsync(request.BodyPartId);
        if (bodyPart == null) throw new ArgumentException("Body part not found");
    }
    
    // Use Writable only for the update
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var muscleGroupRepo = writableUow.GetRepository<IMuscleGroupRepository>();
    
    var muscleGroup = await muscleGroupRepo.GetByIdAsync(id);
    var updated = MuscleGroup.Handler.Update(muscleGroup, request.Name, request.BodyPartId);
    await muscleGroupRepo.UpdateAsync(updated);
    
    await writableUow.CommitAsync();
    // Result: Only MuscleGroup is updated!
}
```

### Key Rules
1. **Always use `ReadOnlyUnitOfWork` for:**
   - Checking if entities exist
   - Validating foreign key references
   - Any query that doesn't modify data

2. **Only use `WritableUnitOfWork` for:**
   - Creating new entities
   - Updating existing entities
   - Deleting entities

3. **Never mix validation queries with update operations in the same UnitOfWork**

## 2. Accessing Repositories from Controllers

### The Problem
Controllers directly accessing repositories or UnitOfWork violates the architectural separation of concerns.

### Symptoms
- Business logic mixed with HTTP handling
- Difficult to test controllers
- Inconsistent transaction management

### Solution
- Controllers should ONLY call service methods
- All repository access must be through the service layer

## 3. Not Using Specialized ID Types

### The Problem
Using raw GUIDs or strings for entity IDs instead of the specialized ID types.

### Symptoms
- Type safety issues (passing wrong ID to wrong method)
- Inconsistent ID formatting
- Runtime errors from invalid ID formats

### Solution
- Always use specialized ID types (e.g., `MuscleGroupId`, `BodyPartId`)
- Use `TryParse` for validation
- Let the ID types handle formatting

## 4. Forgetting to Load Navigation Properties

### The Problem
After creating or updating an entity, forgetting to load navigation properties before returning DTOs.

### Symptoms
- Null navigation properties in returned DTOs
- Missing related data in API responses

### Solution
```csharp
// After save, load navigation properties
await Context.Entry(entity)
    .Reference(e => e.NavigationProperty)
    .LoadAsync();
```

## 5. Not Invalidating Cache After Updates

### The Problem
Forgetting to invalidate cache after create, update, or delete operations.

### Symptoms
- API returns stale data
- Inconsistencies between different endpoints
- Users seeing outdated information

### Solution
- Always invalidate relevant cache keys after modifications
- Consider cache dependencies when updating related entities

## 6. ⚠️ Using Exceptions for Control Flow

### The Problem
Using exceptions to handle business logic and validation creates performance overhead and unclear contracts. Exceptions should be reserved for exceptional circumstances, not regular control flow.

### Symptoms
- Multiple try-catch blocks in controllers
- Services throwing `ArgumentException` for validation
- Performance degradation from exception overhead
- Unclear service contracts about what exceptions can be thrown

### Real Example
```csharp
// ❌ WRONG - Using exceptions for validation
public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
{
    if (string.IsNullOrEmpty(request.Name))
        throw new ArgumentException("Name is required");
    
    if (await repository.ExistsAsync(request.Name))
        throw new InvalidOperationException("Exercise already exists");
    
    // Controller needs multiple catch blocks
}
```

### The Solution
```csharp
// ✅ CORRECT - Using ServiceResult pattern
public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
{
    var validationResult = await ValidateCreateCommand(command);
    if (!validationResult.IsValid)
    {
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, validationResult.Errors);
    }
    
    if (await repository.ExistsAsync(command.Name))
    {
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, "Exercise already exists");
    }
    
    // Success path
    return ServiceResult<ExerciseDto>.Success(dto);
}
```

### Key Rules
1. **Use ServiceResult<T> for all service methods**
2. **Return validation errors as data, not exceptions**
3. **Reserve exceptions for truly exceptional cases (e.g., database connection failure)**
4. **Use pattern matching in controllers for clean response handling**

## 7. Early Returns Based on ID Validation in Controllers

### The Problem
Controllers making business decisions about ID validity creates coupling and inconsistent behavior. The service layer should handle all business logic, including ID validation.

### Symptoms
- Controllers checking if IDs are empty and returning early
- Business logic leaked into the presentation layer
- Inconsistent validation across different controllers

### Real Example
```csharp
// ❌ WRONG - Controller decides about ID validity
public async Task<IActionResult> DeleteExercise(string id)
{
    var exerciseId = ExerciseId.ParseOrEmpty(id);
    if (exerciseId.IsEmpty)
    {
        return NotFound(); // Controller making business decision!
    }
    
    var result = await _exerciseService.DeleteAsync(exerciseId);
}
```

### The Solution
```csharp
// ✅ CORRECT - Service handles all validation
public async Task<IActionResult> DeleteExercise(string id)
{
    var result = await _exerciseService.DeleteAsync(ExerciseId.ParseOrEmpty(id));
    
    return result switch
    {
        { IsSuccess: true } => NoContent(),
        { Errors: var errors } when errors.Any(e => e.Contains("Invalid exercise ID")) => NotFound(),
        { Errors: var errors } => BadRequest(new { errors })
    };
}
```

### Key Rules
1. **Always pass IDs to services, even if empty**
2. **Let services decide validity and return appropriate errors**
3. **Use pattern matching to handle different error scenarios**
4. **Keep controllers focused on HTTP concerns only**

## 8. String-Based Error Pattern Matching

### The Problem
Using string contains checks for error handling is fragile and breaks when error messages change. It also makes internationalization difficult.

### Symptoms
- Pattern matching with `errors.Any(e => e.Contains("already exists"))`
- Broken error handling when messages are updated
- Difficult to maintain error message consistency

### Real Example
```csharp
// ❌ WRONG - Fragile string matching
return result switch
{
    { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => Conflict(),
    { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound()
};
```

### The Solution
```csharp
// ✅ CORRECT - Use error codes
public record ServiceError(string Code, string Message);

// In service
return ServiceResult<T>.Failure(data, new ServiceError("EXERCISE_EXISTS", "Exercise already exists"));

// In controller
return result switch
{
    { Errors: var errors } when errors.Any(e => e.Code == "EXERCISE_EXISTS") => Conflict(),
    { Errors: var errors } when errors.Any(e => e.Code == "NOT_FOUND") => NotFound()
};
```

### Key Rules
1. **Use structured error objects with codes**
2. **Define error code constants**
3. **Match on codes, not messages**
4. **Keep human-readable messages separate from logic**

## 9. ⚠️ Creating New Entities in Update Methods

### The Problem
Using entity factory methods (like `CreateNew`) in update operations creates a completely new entity, losing all relationships and data that wasn't explicitly provided in the update request.

### Symptoms
- Related data disappearing after updates
- Foreign key constraint violations
- Loss of audit fields (CreatedAt, etc.)
- Incomplete updates

### Real Example
```csharp
// ❌ WRONG - Creates new entity, loses all relationships
public async Task<ServiceResult<ExerciseDto>> UpdateAsync(ExerciseId id, UpdateCommand command)
{
    var updatedExercise = Exercise.Handler.CreateNew(
        command.Name,
        command.Description,
        command.DifficultyId
    ); // This is a NEW exercise - all muscle groups, equipment, etc. are LOST!
    
    var finalExercise = updatedExercise with { Id = id };
    await repository.UpdateAsync(finalExercise);
}
```

### The Solution
```csharp
// ✅ CORRECT - Modify existing entity
public async Task<ServiceResult<ExerciseDto>> UpdateAsync(ExerciseId id, UpdateCommand command)
{
    var exercise = await repository.GetByIdAsync(id);
    if (exercise.IsEmpty)
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, "Not found");
    
    // Use 'with' to create modified copy preserving all data
    var updatedExercise = exercise with {
        Name = command.Name,
        Description = command.Description,
        DifficultyId = command.DifficultyId,
        
        // Explicitly update collections
        MuscleGroups = MapToMuscleGroups(command.MuscleGroups),
        Equipment = MapToEquipment(command.EquipmentIds)
    };
    
    await repository.UpdateAsync(updatedExercise);
}
```

### Key Rules
1. **Always load existing entity first**
2. **Use record `with` syntax to create modified copies**
3. **Explicitly handle all relationships**
4. **Never use `CreateNew` for updates**
5. **Preserve audit fields and unchanged data**

## 10. Null Checks When Using ParseOrEmpty

### The Problem
Adding null checks when using ParseOrEmpty defeats the purpose of the pattern and adds unnecessary complexity.

### Symptoms
- Ternary operators checking for null/empty before calling ParseOrEmpty
- Redundant null checks throughout the code
- Inconsistent handling of empty values

### Real Example
```csharp
// ❌ WRONG - Redundant null check
KineticChainId = string.IsNullOrWhiteSpace(request.KineticChainId) 
    ? null 
    : KineticChainTypeId.ParseOrEmpty(request.KineticChainId)
```

### The Solution
```csharp
// ✅ CORRECT - ParseOrEmpty handles everything
KineticChainId = KineticChainTypeId.ParseOrEmpty(request.KineticChainId)
```

### Key Rules
1. **ParseOrEmpty handles null, empty, and invalid inputs**
2. **No preprocessing needed before calling ParseOrEmpty**
3. **Trust the Null Object Pattern**
4. **Let Empty values flow through the system**