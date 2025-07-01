# Unit of Work Pattern

## ⚠️ CRITICAL: ReadOnly vs Writable Usage

### **NEVER use WritableUnitOfWork for validation queries!**

This is one of the most common and dangerous mistakes in our codebase. Using `WritableUnitOfWork` for validation queries causes Entity Framework to track entities unnecessarily, leading to:

1. **Unwanted database updates** - EF may update entities you only meant to validate
2. **Performance issues** - Tracked entities consume memory and slow down operations
3. **Confusing SQL generation** - Multiple UPDATE statements when you expect only one

### ✅ Correct Pattern

```csharp
public async Task<SomeDto> UpdateSomethingAsync(string id, UpdateDto request)
{
    // STEP 1: Use ReadOnlyUnitOfWork for ALL validation queries
    using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
    {
        var validationRepo = readOnlyUow.GetRepository<IValidationRepository>();
        
        // Check if related entity exists (NOT TRACKED!)
        var relatedEntity = await validationRepo.GetByIdAsync(request.RelatedId);
        if (relatedEntity == null || !relatedEntity.IsActive)
        {
            throw new ArgumentException("Related entity not found or inactive");
        }
    }
    
    // STEP 2: Use WritableUnitOfWork ONLY for the actual update
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var repository = writableUow.GetRepository<IMainRepository>();
    
    // Perform the update
    var entity = await repository.GetByIdAsync(id);
    var updated = Entity.Handler.Update(entity, request.NewValue);
    await repository.UpdateAsync(updated);
    
    await writableUow.CommitAsync();
}
```

### ❌ Common Mistake

```csharp
public async Task<SomeDto> UpdateSomethingAsync(string id, UpdateDto request)
{
    // WRONG: Using WritableUnitOfWork for everything
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var mainRepo = unitOfWork.GetRepository<IMainRepository>();
    var relatedRepo = unitOfWork.GetRepository<IRelatedRepository>();
    
    // This tracks the related entity!
    var relatedEntity = await relatedRepo.GetByIdAsync(request.RelatedId);
    
    // Later update causes BOTH entities to be updated in the database!
    var entity = await mainRepo.GetByIdAsync(id);
    var updated = Entity.Handler.Update(entity, request.NewValue);
    await mainRepo.UpdateAsync(updated);
    
    await unitOfWork.CommitAsync();
    // Result: TWO UPDATE statements instead of one!
}
```

## Overview

The Unit of Work pattern is implemented in our project using the Olimpo.EntityFramework.Persistency library. This pattern provides a structured way to interact with the database, ensuring proper transaction management and separation of concerns.

## Key Components

### 1. UnitOfWorkProvider

The `UnitOfWorkProvider<TContext>` is the entry point for creating units of work. It's registered in the dependency injection container:

```csharp
services.AddTransient<IUnitOfWorkProvider<FitnessDbContext>, UnitOfWorkProvider<FitnessDbContext>>();
```

### 2. ReadOnlyUnitOfWork

Used for read-only operations where no data modifications are needed:

```csharp
using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
var repository = unitOfWork.GetRepository<IRepositoryInterface>();
var data = await repository.GetDataAsync();
```

Key characteristics:
- No transaction management
- Cannot commit changes
- Optimized for read operations
- Uses `AsNoTracking()` for better performance

### 3. WritableUnitOfWork

Used for operations that modify data:

```csharp
using var unitOfWork = _unitOfWorkProvider.CreateWritable();
var repository = unitOfWork.GetRepository<IRepositoryInterface>();
await repository.ModifyDataAsync();
await unitOfWork.CommitAsync();
```

Key characteristics:
- Manages transactions
- Requires explicit commit
- All changes are rolled back if not committed or if an exception occurs

## Repository Pattern Integration

The Unit of Work pattern works in conjunction with the Repository pattern:

### 1. Repository Interfaces

Each entity type has a corresponding repository interface that defines available operations:

```csharp
public interface IEquipmentRepository : IRepository
{
    Task<IEnumerable<Equipment>> GetAllAsync();
    Task<Equipment?> GetByIdAsync(EquipmentId id);
    Task<Equipment?> GetByNameAsync(string name);
}
```

### 2. Repository Implementations

Repository implementations inherit from `RepositoryBase<TContext>` and implement the corresponding interface:

```csharp
public class EquipmentRepository : RepositoryBase<FitnessDbContext>, IEquipmentRepository
{
    public async Task<IEnumerable<Equipment>> GetAllAsync() =>
        await Context.Equipment.AsNoTracking().ToListAsync();

    public async Task<Equipment?> GetByIdAsync(EquipmentId id) =>
        await Context.Equipment.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id);

    public async Task<Equipment?> GetByNameAsync(string name) =>
        await Context.Equipment.AsNoTracking().SingleOrDefaultAsync(e => e.Name == name);
}
```

### 3. Repository Registration

Repositories are registered in the dependency injection container:

```csharp
services.AddTransient<IEquipmentRepository, EquipmentRepository>();
services.AddTransient<IMetricTypeRepository, MetricTypeRepository>();
// ... other repositories
```

## Usage in Controllers

Controllers inject the `IUnitOfWorkProvider<TContext>` and use it to create units of work:

```csharp
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;

    public EquipmentController(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = await repository.GetAllAsync();
        
        // Map to DTOs
        var result = equipment.Select(e => new ReferenceDataDto
        {
            Id = e.Id.ToString(),
            Value = e.Name
        });
        
        return Ok(result);
    }
}
```

## Best Practices

1. **Use ReadOnlyUnitOfWork for queries**: When you only need to read data, use `CreateReadOnly()` for better performance.

2. **Use WritableUnitOfWork for modifications**: When you need to modify data, use `CreateWritable()` and don't forget to call `CommitAsync()`.

3. **Dispose units of work**: Always use the `using` statement to ensure proper disposal.

4. **Transaction scope**: All operations within a writable unit of work are part of the same transaction.

5. **Repository access**: Always access repositories through the unit of work using `GetRepository<T>()`.

6. **Repository design**: Keep repositories focused on data access operations for a specific entity type.

## Example Workflow

### Read-Only Operation

```csharp
// In a controller or service
public async Task<IEnumerable<EquipmentDto>> GetAllEquipmentAsync()
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    var equipment = await repository.GetAllAsync();
    
    return equipment.Select(e => new EquipmentDto
    {
        Id = e.Id.ToString(),
        Name = e.Name
    });
}
```

### Write Operation

```csharp
// In a controller or service
public async Task<bool> CreateEquipmentAsync(string name)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var equipment = Equipment.Handler.CreateNew(name);
    await repository.AddAsync(equipment);
    
    await unitOfWork.CommitAsync();
    return true;
}
```

### Complex Operation with Multiple Repositories

```csharp
// In a controller or service
public async Task<bool> AssignEquipmentToExerciseAsync(ExerciseId exerciseId, EquipmentId equipmentId)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    
    var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>();
    var equipmentRepository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var exercise = await exerciseRepository.GetByIdAsync(exerciseId);
    var equipment = await equipmentRepository.GetByIdAsync(equipmentId);
    
    if (exercise == null || equipment == null)
        return false;
    
    await exerciseRepository.AddEquipmentAsync(exercise, equipment);
    
    await unitOfWork.CommitAsync();
    return true;
}
```

## Testing Considerations

When testing components that use the Unit of Work pattern, consider the following:

1. **Empty Database Handling**: Tests should be designed to handle scenarios where the database might be empty. This ensures robustness across different test environments.

2. **Skip Tests When Necessary**: For tests that require specific data to be present, implement conditional logic to skip the test if the required data is not available:

```csharp
[Fact]
public async Task GetById_WithValidId_ReturnsMuscleRole()
{
    // First get all muscle roles to find a valid ID
    var allResponse = await _client.GetAsync("/api/ReferenceTables/MuscleRoles");
    allResponse.EnsureSuccessStatusCode();
    var muscleRoles = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
    Assert.NotNull(muscleRoles);
    
    // Skip the test if there are no muscle roles
    if (!muscleRoles.Any())
    {
        return; // Skip the test
    }
    
    // Continue with the test using the first available item
    var firstMuscleRole = muscleRoles.First();
    
    // Act
    var response = await _client.GetAsync($"/api/ReferenceTables/MuscleRoles/{firstMuscleRole.Id}");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var muscleRole = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
    
    Assert.NotNull(muscleRole);
    Assert.Equal(firstMuscleRole.Id, muscleRole.Id);
    Assert.Equal(firstMuscleRole.Value, muscleRole.Value);
}
```

3. **Avoid Hard-Coded Test Data**: Instead of asserting specific counts or values, make tests adaptable to the actual data present in the test database:

```csharp
[Fact]
public async Task GetAll_ReturnsAllMuscleRoles()
{
    // Act
    var response = await _client.GetAsync("/api/ReferenceTables/MuscleRoles");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var muscleRoles = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
    
    Assert.NotNull(muscleRoles);
    // We don't assert Count or specific values because the database might be empty in some test environments
}
```

4. **Test Structure Validation**: For tests that verify the structure of responses rather than specific values, iterate through available endpoints to find valid data:

```csharp
[Fact]
public async Task GetById_VerifyJsonStructure()
{
    // First get all reference data types to find valid IDs
    var endpoints = new[]
    {
        "/api/ReferenceTables/BodyParts",
        "/api/ReferenceTables/DifficultyLevels",
        // ... other endpoints
    };
    
    foreach (var endpoint in endpoints)
    {
        // Get all items of this type
        var allResponse = await _client.GetAsync(endpoint);
        allResponse.EnsureSuccessStatusCode();
        var allItems = await allResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        Assert.NotNull(allItems);
        
        // Skip if there are no items
        if (!allItems.Any())
        {
            continue;
        }
        
        // Get the first item and verify its structure
        var firstItem = allItems.First();
        
        // Act - get by ID
        var response = await _client.GetAsync($"{endpoint}/{firstItem.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify JSON structure
        // ...
    }
}
```

These testing approaches ensure that tests remain robust across different environments, regardless of the data present in the test database.
