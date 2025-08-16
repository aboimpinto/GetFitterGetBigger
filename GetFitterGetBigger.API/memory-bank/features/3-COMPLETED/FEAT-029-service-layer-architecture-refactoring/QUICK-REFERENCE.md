# FEAT-029: Quick Reference - DataService Pattern

## Pattern At A Glance

**Purpose**: Separate data access from business logic in services

**Structure**:
```
Controller → Service → DataService → Repository → Database
```

## Creating a New DataService

### 1. Create DataService Interface
```csharp
public interface IEntityDataService
{
    Task<ServiceResult<EntityDto>> GetByIdAsync(EntityId id);
    Task<ServiceResult<IEnumerable<EntityDto>>> GetAllAsync();
    Task<ServiceResult<bool>> ExistsByIdAsync(EntityId id);
    Task<ServiceResult<bool>> ExistsByNameAsync(string name);
}
```

### 2. Implement DataService
```csharp
public class EntityDataService : IEntityDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    public async Task<ServiceResult<EntityDto>> GetByIdAsync(EntityId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEntityRepository>();
        
        var entity = await repository.GetByIdAsync(id);
        
        return entity?.IsActive == true
            ? ServiceResult<EntityDto>.Success(entity.ToDto())
            : ServiceResult<EntityDto>.Success(EntityDto.Empty);
    }
}
```

### 3. Update Service to Use DataService
```csharp
public class EntityService : IEntityService
{
    private readonly IEntityDataService _dataService;  // Not UnitOfWork!
    
    public async Task<ServiceResult<EntityDto>> GetByIdAsync(EntityId id)
    {
        return await ServiceValidate.For<EntityDto>()
            .EnsureNotEmpty(id, "Invalid ID")
            .MatchAsync(async () => await _dataService.GetByIdAsync(id));
    }
}
```

### 4. Register in DI Container
```csharp
services.AddScoped<IEntityDataService, EntityDataService>();
```

## Common Patterns

### Query Pattern (No Validation)
```csharp
public async Task<ServiceResult<IEnumerable<EntityDto>>> GetAllAsync()
{
    return await ServiceValidate.Build<IEnumerable<EntityDto>>()
        .WhenValidAsync(async () => await _dataService.GetAllAsync());
}
```

### Query Pattern (With Validation)
```csharp
public async Task<ServiceResult<EntityDto>> GetByIdAsync(EntityId id)
{
    return await ServiceValidate.For<EntityDto>()
        .EnsureNotEmpty(id, "Invalid ID")
        .MatchAsync(async () => await _dataService.GetByIdAsync(id));
}
```

### Existence Check Pattern
```csharp
public async Task<ServiceResult<bool>> ExistsAsync(EntityId id)
{
    return await ServiceValidate.For<bool>()
        .EnsureNotEmpty(id, "Invalid ID")
        .MatchAsync(async () => await _dataService.ExistsByIdAsync(id));
}
```

## Testing Pattern

### Service Test Setup
```csharp
public class EntityServiceTests
{
    private readonly Mock<IEntityDataService> _dataService = new();
    private readonly EntityService _service;
    
    public EntityServiceTests()
    {
        _service = new EntityService(_dataService.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsEntity()
    {
        // Arrange
        var id = EntityId.From("valid-id");
        var dto = new EntityDto { Id = id };
        
        _dataService.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(ServiceResult<EntityDto>.Success(dto));
        
        // Act
        var result = await _service.GetByIdAsync(id);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
    }
}
```

## Do's and Don'ts

### ✅ DO's
- Keep DataService methods focused on data access
- Return ServiceResult from DataService methods
- Use Empty pattern for not-found scenarios
- Handle exceptions in DataService, not Service
- Use ReadOnly UnitOfWork for queries

### ❌ DON'Ts
- Don't put business logic in DataService
- Don't use UnitOfWork in Service layer
- Don't return null from DataService
- Don't throw exceptions for business errors
- Don't mix read and write operations

## Quick Conversion Checklist

When converting a service to use DataService:

- [ ] Create IEntityDataService interface
- [ ] Create EntityDataService implementation
- [ ] Move all UnitOfWork code from Service to DataService
- [ ] Update Service constructor to accept IEntityDataService
- [ ] Replace UnitOfWork usage with DataService calls
- [ ] Update Service tests to mock DataService
- [ ] Register DataService in DI container
- [ ] Run tests to verify behavior unchanged

## Common DataService Methods

```csharp
// Queries
GetByIdAsync(EntityId id)
GetAllAsync()
GetAllActiveAsync()
GetByNameAsync(string name)
GetPagedAsync(int page, int pageSize)

// Existence Checks
ExistsByIdAsync(EntityId id)
ExistsByNameAsync(string name)
IsActiveAsync(EntityId id)

// Aggregates
GetCountAsync()
GetCountByStatusAsync(Status status)

// Commands (when applicable)
CreateAsync(CreateCommand command)
UpdateAsync(EntityId id, UpdateCommand command)
DeleteAsync(EntityId id)
```

## Error Handling

### In DataService
```csharp
public async Task<ServiceResult<EntityDto>> GetByIdAsync(EntityId id)
{
    try
    {
        // Data access logic
        return ServiceResult<EntityDto>.Success(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get entity {Id}", id);
        return ServiceResult<EntityDto>.Failure(
            EntityDto.Empty,
            ServiceError.InternalError("Database error"));
    }
}
```

### In Service
```csharp
// Service doesn't need try-catch for data access!
public async Task<ServiceResult<EntityDto>> GetByIdAsync(EntityId id)
{
    return await ServiceValidate.For<EntityDto>()
        .EnsureNotEmpty(id, "Invalid ID")
        .MatchAsync(async () => await _dataService.GetByIdAsync(id));
}
```

## Migration Example

### Before (Service with UnitOfWork)
```csharp
public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
{
    if (id.IsEmpty)
        return ServiceResult<BodyPartDto>.Failure(/*...*/);
    
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IBodyPartRepository>();
    var entity = await repository.GetByIdAsync(id);
    
    return entity != null
        ? ServiceResult<BodyPartDto>.Success(entity.ToDto())
        : ServiceResult<BodyPartDto>.Failure(/*...*/);
}
```

### After (Service with DataService)
```csharp
public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
{
    return await ServiceValidate.For<BodyPartDto>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidId)
        .MatchAsync(async () => await _dataService.GetByIdAsync(id));
}
```

## Benefits Summary

1. **Simpler Tests**: Mock one DataService instead of UnitOfWork + Repository
2. **Clear Boundaries**: Business logic vs Data access
3. **Better Maintainability**: Changes isolated to appropriate layer
4. **Consistent Pattern**: All services follow same structure
5. **Reduced Complexity**: Service methods become trivial

## Remember

> "If your service knows about UnitOfWork, you're doing it wrong. Services should only know about business logic."