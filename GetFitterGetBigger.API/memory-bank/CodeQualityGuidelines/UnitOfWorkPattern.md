# Unit of Work Pattern - Transaction Management

**🎯 PURPOSE**: This document defines the **CRITICAL** Unit of Work pattern usage that prevents data corruption and ensures proper transaction management in the GetFitterGetBigger API.

## Overview

The Unit of Work pattern manages database transactions and ensures data consistency. **Improper usage can lead to data corruption.**

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: UnitOfWork Rules - MUST be followed            │
├──────────────────────────────────────────────────────────────┤
│ 1. ReadOnlyUnitOfWork: For ALL queries (no SaveChanges)     │
│ 2. WritableUnitOfWork: ONLY for Create/Update/Delete        │
│ 3. One UnitOfWork per method                                 │
│ 4. Reuse existing query methods for validation               │
│ 5. NEVER use WritableUnitOfWork for validation              │
└──────────────────────────────────────────────────────────────┘
```

## The Problem - Entity Tracking Issues

### ❌ BAD - Using WritableUnitOfWork for Validation

```csharp
// ❌ WRONG - Using WritableUnitOfWork for validation
public async Task<ServiceResult<TDto>> UpdateAsync(TId id, TCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable(); // WRONG for validation!
    var repository = unitOfWork.GetRepository<TRepository>();
    
    var existing = await repository.GetByIdAsync(id); // Tracks entity!
    if (existing.IsEmpty)
        return ServiceResult<TDto>.Failure(TDto.Empty, ServiceError.NotFound());
    
    // Entity is now tracked, causing issues when we try to update later
    // This can lead to:
    // - Concurrency conflicts
    // - Duplicate key errors
    // - Unexpected state changes
}
```

### ✅ GOOD - Separate Concerns, Use Existing Methods

```csharp
// ✅ CORRECT - Proper separation of concerns
public async Task<ServiceResult<TDto>> UpdateAsync(TId id, TCommand command)
{
    // Use existing GetByIdAsync which uses ReadOnlyUnitOfWork internally
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return ServiceResult<TDto>.Failure(TDto.Empty, existingResult.Errors);
    
    // Now do the actual update with WritableUnitOfWork
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<TRepository>();
    
    // Entity is not tracked, we can update cleanly
    var entity = await repository.GetByIdAsync(id);
    entity = entity.Update(command);
    await repository.UpdateAsync(entity);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<TDto>.Success(MapToDto(entity));
}
```

## When to Use Each Type

### ReadOnlyUnitOfWork - For ALL Queries

```csharp
// ✅ Use for all read operations
public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    return entity.IsEmpty 
        ? ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.NotFound())
        : ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}

// ✅ Use for existence checks
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var exists = await repository.ExistsAsync(id);
    return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
}

// ✅ Use for validation queries
private async Task<bool> CheckDuplicateNameAsync(string name)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    return await repository.ExistsByNameAsync(name);
}
```

### WritableUnitOfWork - ONLY for Modifications

```csharp
// ✅ Use for CREATE operations
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    // Validation uses ReadOnly or existing methods
    if (await CheckDuplicateNameAsync(command.Name))
        return ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty, 
            ServiceError.AlreadyExists("Equipment", command.Name));
    
    // Only use Writable for actual creation
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = Equipment.CreateNew(command.Name, command.Description);
    entity = await repository.CreateAsync(entity);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}

// ✅ Use for UPDATE operations
public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
{
    // Validation uses existing GetByIdAsync (ReadOnly)
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return existingResult;
    
    // Only use Writable for actual update
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    entity = entity.Update(command.Name, command.Description);
    entity = await repository.UpdateAsync(entity);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}

// ✅ Use for DELETE operations
public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
{
    // Validation uses existing GetByIdAsync (ReadOnly)
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return ServiceResult<bool>.Failure(false, existingResult.Errors);
    
    // Only use Writable for actual deletion
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    await repository.DeleteAsync(id);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<bool>.Success(true);
}
```

## Common Patterns

### Pattern 1: Validation Before Modification

```csharp
public async Task<ServiceResult<TDto>> UpdateAsync(TId id, UpdateCommand command)
{
    // Step 1: Validate using ReadOnly operations
    var validationResult = await ValidateUpdateAsync(id, command);
    if (!validationResult.IsSuccess)
        return validationResult;
    
    // Step 2: Perform modification with Writable
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IRepository>();
    
    // ... update logic
    await unitOfWork.CommitAsync();
    
    return ServiceResult<TDto>.Success(dto);
}

private async Task<ServiceResult<TDto>> ValidateUpdateAsync(TId id, UpdateCommand command)
{
    // All validation uses ReadOnly
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IRepository>();
    
    // ... validation logic
}
```

### Pattern 2: Reusing Query Methods

```csharp
public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
{
    // Reuse existing GetByIdAsync which already uses ReadOnlyUnitOfWork
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return existingResult;
    
    // Check for duplicate name using existing method
    var existsByName = await ExistsByNameAsync(command.Name, id);
    if (existsByName.Data.Value)
        return ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty,
            ServiceError.AlreadyExists("Equipment", command.Name));
    
    // Now safe to update with WritableUnitOfWork
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    // ... update logic
}
```

### Pattern 3: One UnitOfWork Per Method

```csharp
// ❌ BAD - Multiple UnitOfWork instances
public async Task<ServiceResult<EquipmentDto>> CreateWithValidationAsync(CreateEquipmentCommand command)
{
    // First UnitOfWork for validation
    using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
    var readRepo = readOnlyUow.GetRepository<IEquipmentRepository>();
    
    if (await readRepo.ExistsByNameAsync(command.Name))
        return ServiceResult<EquipmentDto>.Failure(...);
    
    // Second UnitOfWork for creation - DIFFERENT TRANSACTION!
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var writeRepo = writableUow.GetRepository<IEquipmentRepository>();
    
    // PROBLEM: Name could be created between the two operations!
    var entity = await writeRepo.CreateAsync(...);
}

// ✅ GOOD - Single transaction for write operations
public async Task<ServiceResult<EquipmentDto>> CreateWithValidationAsync(CreateEquipmentCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    // Check and create in same transaction
    if (await repository.ExistsByNameAsync(command.Name))
        return ServiceResult<EquipmentDto>.Failure(...);
    
    var entity = await repository.CreateAsync(...);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}
```

## Entity Tracking Issues and Solutions

### Problem: Entity Already Tracked

```csharp
// ❌ This causes "entity already tracked" error
public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    // First load - entity is tracked
    var existing = await repository.GetByIdAsync(id);
    if (existing.IsEmpty)
        return ServiceResult<EquipmentDto>.Failure(...);
    
    // Some business logic...
    
    // Second load - ERROR! Entity already tracked
    var entity = await repository.GetByIdAsync(id);
    entity = entity.Update(command);
}
```

### Solution: Use ReadOnly for Validation

```csharp
// ✅ No tracking issues
public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateCommand command)
{
    // Validation with ReadOnly - no tracking
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return existingResult;
    
    // Update with Writable - clean tracking
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    entity = entity.Update(command);
    await repository.UpdateAsync(entity);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}
```

## Testing Considerations

```csharp
[Fact]
public async Task UpdateAsync_UsesReadOnlyForValidation()
{
    // Arrange
    var id = EquipmentId.New();
    
    // Setup to verify ReadOnly is used for validation
    _mockUnitOfWorkProvider
        .Setup(x => x.CreateReadOnly())
        .Returns(_mockReadOnlyUnitOfWork.Object)
        .Verifiable();
    
    // Setup to verify Writable is used for update
    _mockUnitOfWorkProvider
        .Setup(x => x.CreateWritable())
        .Returns(_mockWritableUnitOfWork.Object)
        .Verifiable();
    
    // Act
    await _service.UpdateAsync(id, new UpdateCommand());
    
    // Assert
    _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
    _mockUnitOfWorkProvider.Verify(x => x.CreateWritable(), Times.Once);
}
```

## Key Principles

1. **ReadOnlyUnitOfWork**: For ALL queries (no SaveChanges capability)
2. **WritableUnitOfWork**: ONLY for Create/Update/Delete operations
3. **One UnitOfWork per method**: Don't mix multiple UnitOfWork instances
4. **Reuse existing query methods**: Leverage existing GetById, Exists methods for validation
5. **Never track entities for validation**: Use ReadOnly to avoid tracking issues

## Common Mistakes to Avoid

- ❌ Using WritableUnitOfWork for validation queries
- ❌ Loading the same entity twice in one WritableUnitOfWork
- ❌ Mixing ReadOnly and Writable in the same logical operation
- ❌ Not committing changes in WritableUnitOfWork
- ❌ Using multiple UnitOfWork instances for related operations

## Summary

Proper UnitOfWork usage is critical for data integrity. Always use ReadOnlyUnitOfWork for queries and validation, and WritableUnitOfWork only for actual data modifications. This prevents entity tracking issues and ensures clean, predictable transactions.

## Related Documentation

- `/memory-bank/unitOfWorkPattern.md` - Detailed Unit of Work implementation
- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/ServiceRepositoryBoundaries.md` - Repository access patterns