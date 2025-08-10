# Repository Pattern - Data Access Layer

**🎯 PURPOSE**: This document defines the **MANDATORY** repository pattern standards for the GetFitterGetBigger API data access layer.

## Overview

Repositories provide a clean abstraction over data access with:
- **NO business logic** - pure data operations only
- Clear interface contracts
- Testable data layer
- Entity Framework Core abstraction

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: Repository Rules - MUST be followed            │
├──────────────────────────────────────────────────────────────┤
│ 1. Repositories handle data access ONLY                     │
│ 2. NO business logic or validation in repositories          │
│ 3. Return entities with Empty pattern (never null)          │
│ 4. Use AsNoTracking() for read operations                   │
│ 5. Each service accesses ONLY its own repository            │
└──────────────────────────────────────────────────────────────┘
```

## Repository Interface Pattern

### ❌ BAD - Business Logic in Repository

```csharp
public interface IEquipmentRepository : IRepositoryBase<Equipment, EquipmentId>
{
    // VIOLATION: Business logic methods
    Task<bool> ValidateUniqueNameAsync(string name);
    Task<decimal> CalculateUsageHoursAsync(EquipmentId id);
    Task<bool> CanBeDeletedAsync(EquipmentId id);
    Task<Equipment> CreateWithValidationAsync(Equipment entity);
}

public class EquipmentRepository : IEquipmentRepository
{
    // VIOLATION: Validation in repository
    public async Task<Equipment> CreateWithValidationAsync(Equipment entity)
    {
        if (await ExistsByNameAsync(entity.Name))
            throw new InvalidOperationException("Name already exists");
            
        _context.Equipment.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}
```

### ✅ GOOD - Pure Data Access

```csharp
public interface IEquipmentRepository : IRepositoryBase<Equipment, EquipmentId>
{
    // Pure data queries only
    Task<Equipment> GetByNameAsync(string name);
    Task<IEnumerable<Equipment>> GetActiveAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<Equipment>> GetByTypeAsync(EquipmentType type);
}

public class EquipmentRepository : RepositoryBase<Equipment, EquipmentId>, IEquipmentRepository
{
    public EquipmentRepository(FitnessDbContext context) : base(context) { }
    
    public async Task<Equipment> GetByNameAsync(string name)
    {
        var entity = await _context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name == name);
            
        return entity ?? Equipment.Empty;  // Never return null
    }
    
    public async Task<IEnumerable<Equipment>> GetActiveAsync()
    {
        return await _context.Equipment
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
}
```

## Base Repository Implementation

```csharp
public abstract class RepositoryBase<TEntity, TId> : IRepositoryBase<TEntity, TId>
    where TEntity : class, IEmptyEntity<TEntity>
    where TId : ISpecializedId<TId>
{
    protected readonly FitnessDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    
    protected RepositoryBase(FitnessDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }
    
    public virtual async Task<TEntity> GetByIdAsync(TId id)
    {
        if (id.IsEmpty)
            return TEntity.Empty;
            
        var entity = await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<TId>(e, "Id") == id);
            
        return entity ?? TEntity.Empty;
    }
    
    public virtual async Task<bool> ExistsAsync(TId id)
    {
        if (id.IsEmpty)
            return false;
            
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(e => EF.Property<TId>(e, "Id") == id);
    }
    
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public virtual async Task DeleteAsync(TId id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null && !entity.IsEmpty)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
    
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync();
    }
}
```

## Query Patterns

### Simple Queries

```csharp
public async Task<Equipment> GetByNameAsync(string name)
{
    var entity = await _context.Equipment
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Name == name);
        
    return entity ?? Equipment.Empty;
}
```

### Queries with Includes

```csharp
public async Task<Exercise> GetWithDetailsAsync(ExerciseId id)
{
    var entity = await _context.Exercises
        .AsNoTracking()
        .Include(e => e.PrimaryMuscles)
        .Include(e => e.SecondaryMuscles)
        .Include(e => e.RequiredEquipment)
        .FirstOrDefaultAsync(e => e.Id == id);
        
    return entity ?? Exercise.Empty;
}
```

### Paged Queries

```csharp
public async Task<PagedResult<Equipment>> GetPagedAsync(int page, int pageSize)
{
    var query = _context.Equipment
        .AsNoTracking()
        .Where(e => e.IsActive)
        .OrderBy(e => e.Name);
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Equipment>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### Filtered Queries

```csharp
public async Task<IEnumerable<Exercise>> SearchAsync(ExerciseSearchCriteria criteria)
{
    var query = _context.Exercises.AsNoTracking();
    
    // Apply filters - no business logic, just data filtering
    if (!string.IsNullOrWhiteSpace(criteria.Name))
        query = query.Where(e => e.Name.Contains(criteria.Name));
    
    if (criteria.Type.HasValue)
        query = query.Where(e => e.Type == criteria.Type.Value);
    
    if (criteria.MinDifficulty.HasValue)
        query = query.Where(e => e.Difficulty >= criteria.MinDifficulty.Value);
    
    if (criteria.MuscleGroupIds?.Any() == true)
        query = query.Where(e => e.PrimaryMuscles.Any(m => criteria.MuscleGroupIds.Contains(m)));
    
    return await query
        .OrderBy(e => e.Name)
        .ToListAsync();
}
```

## Unit of Work Integration

Repositories are accessed through Unit of Work:

```csharp
// In service layer
public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    
    return entity.IsEmpty
        ? ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty,
            ServiceError.NotFound("Equipment", id.ToString()))
        : ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}
```

## Performance Considerations

### Use AsNoTracking for Queries

```csharp
// ✅ GOOD - No tracking for read operations
public async Task<IEnumerable<Equipment>> GetAllAsync()
{
    return await _context.Equipment
        .AsNoTracking()  // Essential for performance
        .Where(e => e.IsActive)
        .ToListAsync();
}
```

### Explicit Includes

```csharp
// ✅ GOOD - Explicit includes prevent N+1 queries
public async Task<WorkoutTemplate> GetWithExercisesAsync(WorkoutTemplateId id)
{
    return await _context.WorkoutTemplates
        .AsNoTracking()
        .Include(t => t.Exercises)
            .ThenInclude(e => e.Exercise)
        .FirstOrDefaultAsync(t => t.Id == id)
        ?? WorkoutTemplate.Empty;
}
```

### Projection for Performance

```csharp
// When you only need specific fields
public async Task<IEnumerable<EquipmentSummary>> GetSummariesAsync()
{
    return await _context.Equipment
        .AsNoTracking()
        .Where(e => e.IsActive)
        .Select(e => new EquipmentSummary
        {
            Id = e.Id,
            Name = e.Name,
            Type = e.Type
        })
        .ToListAsync();
}
```

## Testing Repositories

```csharp
public class EquipmentRepositoryTests
{
    private readonly FitnessDbContext _context;
    private readonly EquipmentRepository _repository;
    
    public EquipmentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new FitnessDbContext(options);
        _repository = new EquipmentRepository(_context);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsEntity()
    {
        // Arrange
        var equipment = Equipment.Create(EquipmentId.New(), "Test Equipment");
        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByIdAsync(equipment.Id);
        
        // Assert
        Assert.False(result.IsEmpty);
        Assert.Equal(equipment.Name, result.Name);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsEmpty()
    {
        // Arrange
        var id = EquipmentId.New();
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.True(result.IsEmpty);
    }
}
```

## Common Mistakes to Avoid

### ❌ Business Logic in Repository

```csharp
// WRONG - Validation is business logic
public async Task<Equipment> CreateAsync(Equipment entity)
{
    if (await ExistsByNameAsync(entity.Name))
        throw new InvalidOperationException("Duplicate name");
        
    _dbSet.Add(entity);
    await _context.SaveChangesAsync();
    return entity;
}
```

### ❌ Returning Null

```csharp
// WRONG - Can cause NullReferenceException
public async Task<Equipment?> GetByIdAsync(EquipmentId id)
{
    return await _dbSet.FindAsync(id);
}
```

### ❌ Not Using AsNoTracking

```csharp
// WRONG - Tracks entities unnecessarily
public async Task<IEnumerable<Equipment>> GetAllAsync()
{
    return await _context.Equipment.ToListAsync();  // Missing AsNoTracking()
}
```

### ❌ Service Logic in Repository

```csharp
// WRONG - Caching is service concern
public async Task<Equipment> GetByIdWithCacheAsync(EquipmentId id)
{
    var cached = _cache.Get<Equipment>(id.ToString());
    if (cached != null)
        return cached;
        
    var entity = await GetByIdAsync(id);
    _cache.Set(id.ToString(), entity);
    return entity;
}
```

## Repository Checklist

When implementing a repository:

- [ ] Extends RepositoryBase<TEntity, TId>
- [ ] Only data access methods (no business logic)
- [ ] Returns Empty pattern (never null)
- [ ] Uses AsNoTracking() for queries
- [ ] Explicit includes for related data
- [ ] No validation logic
- [ ] No caching logic
- [ ] No service dependencies
- [ ] Proper async/await usage
- [ ] Unit tests with in-memory database

## Key Principles

1. **Pure Data Access**: Repositories only handle database operations
2. **No Business Logic**: All validation and business rules in services
3. **Empty Over Null**: Always return Empty objects, never null
4. **Performance First**: AsNoTracking by default, explicit includes
5. **Testable**: Easy to mock or test with in-memory database

## Related Documentation

- `/memory-bank/unitOfWorkPattern.md` - Unit of Work integration
- `/memory-bank/CodeQualityGuidelines/ServiceRepositoryBoundaries.md` - Service-repository boundaries
- `/memory-bank/CodeQualityGuidelines/EmptyObjectPattern.md` - Empty pattern implementation
- `/memory-bank/databaseModelPattern.md` - Database design patterns