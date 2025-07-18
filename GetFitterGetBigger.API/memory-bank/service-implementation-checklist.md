# Service Implementation Checklist

This is a quick reference checklist to use **EVERY TIME** you implement or modify a service method.

## 🔍 Pre-Implementation Review
- [ ] Have I read `/memory-bank/common-implementation-pitfalls.md`?
- [ ] Do I understand the ReadOnly vs Writable UnitOfWork pattern?

## ✅ Implementation Checklist

### 1. Validation Queries
- [ ] Am I checking if related entities exist? → Use `ReadOnlyUnitOfWork`
- [ ] Am I validating foreign keys? → Use `ReadOnlyUnitOfWork`
- [ ] Am I loading data for display/validation only? → Use `ReadOnlyUnitOfWork`

### 2. Data Modifications
- [ ] Am I creating new entities? → Use `WritableUnitOfWork`
- [ ] Am I updating existing entities? → Use `WritableUnitOfWork`
- [ ] Am I deleting entities? → Use `WritableUnitOfWork`

### 3. Correct Pattern Template (Simplified after BUG-009)
```csharp
public async Task<ServiceResult<TDto>> UpdateAsync(string id, TUpdateCommand command)
{
    // STEP 1: Validate existence using existing GetByIdAsync (uses ReadOnly internally)
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return existingResult;
    
    // STEP 2: Validate command
    var validationResult = await ValidateUpdateCommand(id, command);
    if (!validationResult.IsValid)
    {
        return ServiceResult<TDto>.Failure(
            CreateEmptyDto(),
            validationResult.Errors);
    }
    
    // STEP 3: Load entity using regular LoadEntityByIdAsync (uses ReadOnly)
    var existingEntity = await LoadEntityByIdAsync(id);
    
    // STEP 4: Perform update with WritableUnitOfWork
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    
    // Update entity (repository handles tracking via Detach/Attach)
    var updatedEntity = await UpdateEntityAsync(unitOfWork, existingEntity, command);
    await unitOfWork.CommitAsync();
    
    // STEP 5: Invalidate cache
    await InvalidateCacheAsync();
    
    return ServiceResult<TDto>.Success(MapToDto(updatedEntity));
}
```

### For Loading Entities (Simplified Pattern)
```csharp
// Single method for ALL loading - creates its own ReadOnlyUnitOfWork
protected override async Task<Equipment> LoadEntityByIdAsync(string id)
{
    var equipmentId = EquipmentId.ParseOrEmpty(id);
    
    return equipmentId.IsEmpty switch
    {
        true => Equipment.Empty,
        false => await LoadFromRepositoryAsync(equipmentId)
    };
}

// Note: No separate LoadEntityByIdForUpdateAsync needed!
// The repository handles entity tracking internally using Detach/Attach pattern
```

### 4. Post-Implementation Verification
- [ ] Did I use separate UnitOfWork instances for validation and modification?
- [ ] Did I dispose ReadOnlyUnitOfWork before starting WritableUnitOfWork?
- [ ] Did I call CommitAsync() on WritableUnitOfWork?
- [ ] Did I invalidate relevant caches after modifications?

## 🚨 Red Flags to Watch For
- Using the same UnitOfWork for both validation and updates
- Validating foreign keys inside a WritableUnitOfWork
- Loading reference entities (like BodyPart, Equipment) in WritableUnitOfWork
- Forgetting to invalidate cache after updates

## 💡 Quick Rule
**If you're not saving it, don't track it!** Use ReadOnlyUnitOfWork for everything except the actual save operation.