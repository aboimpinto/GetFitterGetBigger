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