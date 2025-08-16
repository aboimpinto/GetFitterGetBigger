# Repository Base Class Pattern

## 🚨 CRITICAL: All Repositories MUST Inherit from Base Classes

This is **NOT optional**. Every repository in the system must inherit from an appropriate base class that enforces the Empty pattern through generic constraints. This provides compile-time null safety that cannot be bypassed.

## Core Principle: Compile-Time Enforcement

> **If it compiles, it's null-safe. No exceptions.**

The base class generic constraints make it impossible to create repositories that return null:

```csharp
// The constraint enforces Empty pattern at compile time
where TEntity : IEmptyEntity<TEntity>
```

## Repository Categories and Base Classes

### 1. Reference Data Repositories

For lookup/reference tables (Equipment, BodyPart, DifficultyLevel, etc.):

```csharp
public class EquipmentRepository : ReferenceDataRepository<Equipment, EquipmentId, FitnessDbContext>, 
    IEquipmentRepository
{
    // Inherits: GetAll, GetAllActive, GetById, GetByValue, ExistsAsync
    // All methods guaranteed to never return null
}
```

**Base Class**: `ReferenceDataRepository<TEntity, TId, TContext>`
**Interface**: `IReferenceDataRepository<TEntity, TId>`

### 2. Domain Entity Repositories

For business domain entities (User, WorkoutTemplate, Exercise, etc.):

```csharp
public class UserRepository : DomainRepository<User, UserId, FitnessDbContext>, 
    IUserRepository
{
    // Inherits common domain operations with Empty pattern enforcement
    
    // Add specialized methods
    public async Task<User> GetByEmailAsync(string email)
    {
        var user = await Context.Users
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Email == email);
        
        return user ?? User.Empty;  // Must follow Empty pattern
    }
}
```

**Base Class**: `DomainRepository<TEntity, TId, TContext>` (to be created)
**Interface**: `IDomainRepository<TEntity, TId>`

## Implementation Requirements

### Step 1: Entity Must Implement IEmptyEntity

```csharp
public record Equipment : IEmptyEntity<Equipment>
{
    public EquipmentId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    
    // REQUIRED: Empty instance
    public static Equipment Empty => new()
    {
        Id = EquipmentId.Empty,
        Name = string.Empty,
        IsActive = false
    };
    
    // REQUIRED: IsEmpty check
    public bool IsEmpty => Id.IsEmpty;
}
```

### Step 2: Repository Inherits from Base Class

```csharp
public class EquipmentRepository : 
    ReferenceDataRepository<Equipment, EquipmentId, FitnessDbContext>,
    IEquipmentRepository
{
    // Constructor if needed for DI
    public EquipmentRepository(FitnessDbContext context) : base(context) { }
    
    // Only add specialized methods not in base class
    public async Task<IEnumerable<Equipment>> GetByTypeAsync(string type)
    {
        return await Context.Equipment
            .Where(e => e.Type == type && e.IsActive)
            .ToListAsync() ?? [];  // Never return null collections
    }
}
```

### Step 3: Interface Extends Base Interface

```csharp
public interface IEquipmentRepository : IReferenceDataRepository<Equipment, EquipmentId>
{
    // Only declare specialized methods
    Task<IEnumerable<Equipment>> GetByTypeAsync(string type);
}
```

## What the Base Class Provides

### ReferenceDataRepository Base Methods

```csharp
// All these methods are implemented with Empty pattern guarantee
Task<IEnumerable<TEntity>> GetAllAsync();        // Never returns null
Task<IEnumerable<TEntity>> GetAllActiveAsync();  // Never returns null  
Task<TEntity> GetByIdAsync(TId id);              // Returns Empty if not found
Task<TEntity> GetByValueAsync(string value);     // Returns Empty if not found
Task<bool> ExistsAsync(TId id);                  // Efficient existence check
```

### Compile-Time Guarantees

```csharp
// This WON'T compile - Entity doesn't implement IEmptyEntity
public class BrokenEntity { }
public class BrokenRepository : ReferenceDataRepository<BrokenEntity, Guid, DbContext>
{
    // Compiler Error: BrokenEntity doesn't satisfy constraint
}

// This WON'T compile - Wrong ID type
public class WrongIdRepository : ReferenceDataRepository<Equipment, string, DbContext>
{
    // Compiler Error: string doesn't satisfy 'struct' constraint
}
```

## Testing Strategy

### 1. Base Class Test Suite

Create once, inherit everywhere:

```csharp
[TestClass]
public abstract class ReferenceDataRepositoryTestBase<TRepo, TEntity, TId>
    where TRepo : IReferenceDataRepository<TEntity, TId>
    where TEntity : ReferenceDataBase, IEmptyEntity<TEntity>, new()
    where TId : struct
{
    protected abstract TRepo CreateRepository();
    protected abstract TId CreateValidId();
    protected abstract TId CreateInvalidId();
    
    [TestMethod]
    public async Task GetByIdAsync_WithInvalidId_ReturnsEmpty()
    {
        // Arrange
        var repo = CreateRepository();
        var invalidId = CreateInvalidId();
        
        // Act
        var result = await repo.GetByIdAsync(invalidId);
        
        // Assert
        result.Should().NotBeNull("Repository must never return null");
        result.IsEmpty.Should().BeTrue("Must return Empty for non-existent ID");
    }
    
    [TestMethod]
    public async Task GetAllAsync_WithNoData_ReturnsEmptyList()
    {
        // Arrange
        var repo = CreateRepository();
        
        // Act
        var result = await repo.GetAllAsync();
        
        // Assert
        result.Should().NotBeNull("Must return empty list, not null");
        result.Should().BeEmpty("Empty database should return empty list");
    }
}
```

### 2. Concrete Repository Tests

```csharp
[TestClass]
public class EquipmentRepositoryTests : ReferenceDataRepositoryTestBase<
    IEquipmentRepository, Equipment, EquipmentId>
{
    private FitnessDbContext _context;
    
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FitnessDbContext(options);
    }
    
    protected override IEquipmentRepository CreateRepository()
        => new EquipmentRepository(_context);
    
    protected override EquipmentId CreateValidId()
        => EquipmentId.New();
    
    protected override EquipmentId CreateInvalidId()
        => EquipmentId.From(Guid.NewGuid());
    
    // Add Equipment-specific tests
    [TestMethod]
    public async Task GetByTypeAsync_ReturnsCorrectEquipment()
    {
        // Equipment-specific test implementation
    }
}
```

## Common Patterns and Examples

### Pattern 1: Adding Specialized Query Methods

```csharp
public class ExerciseRepository : DomainRepository<Exercise, ExerciseId, FitnessDbContext>
{
    // Base provides GetById, but we need GetByName
    public async Task<Exercise> GetByNameAsync(string name)
    {
        var exercise = await Context.Exercises
            .Include(e => e.ExerciseTypes)
            .Include(e => e.MuscleGroups)
            .FirstOrDefaultAsync(e => e.Name == name);
        
        return exercise ?? Exercise.Empty;  // Always return Empty, not null
    }
    
    // Complex query with multiple includes
    public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroupId muscleGroupId)
    {
        return await Context.Exercises
            .Include(e => e.MuscleGroups)
            .Where(e => e.MuscleGroups.Any(mg => mg.Id == muscleGroupId))
            .ToListAsync() ?? [];  // Empty list, not null
    }
}
```

### Pattern 2: Overriding Base Methods for Additional Logic

```csharp
public class WorkoutTemplateRepository : DomainRepository<WorkoutTemplate, WorkoutTemplateId, FitnessDbContext>
{
    public override async Task<WorkoutTemplate> GetByIdAsync(WorkoutTemplateId id)
    {
        // Need to include related data
        var template = await Context.WorkoutTemplates
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .Include(w => w.Objectives)
            .FirstOrDefaultAsync(w => w.Id == id);
        
        return template ?? WorkoutTemplate.Empty;
    }
}
```

### Pattern 3: Collection Methods Never Return Null

```csharp
public class ClaimRepository : DomainRepository<Claim, ClaimId, FitnessDbContext>
{
    public async Task<List<Claim>> GetClaimsByUserIdAsync(UserId userId)
    {
        var claims = await Context.Claims
            .Where(c => c.UserId == userId)
            .ToListAsync();
        
        return claims ?? new List<Claim>();  // Never null, always a list
    }
}
```

## Migration Checklist

When migrating an existing repository:

- [ ] Entity implements `IEmptyEntity<T>`
- [ ] Entity has `static T Empty` property
- [ ] Entity has `bool IsEmpty` property
- [ ] Repository inherits from appropriate base class
- [ ] Remove duplicate base methods (GetAll, GetById, etc.)
- [ ] All methods return Empty or empty collections (never null)
- [ ] Interface extends base interface
- [ ] Tests inherit from base test class
- [ ] No nullable return types (`Task<T?>` → `Task<T>`)

## Anti-Patterns to Avoid

### ❌ Don't Create Standalone Repositories

```csharp
// WRONG - No base class, no compile-time safety
public class BadRepository : IRepository
{
    public async Task<Equipment?> GetByIdAsync(EquipmentId id)  // Nullable!
    {
        return await _context.Equipment.FindAsync(id);  // Can return null!
    }
}
```

### ❌ Don't Return Null from Repository Methods

```csharp
// WRONG - Returns null
public async Task<User?> GetByEmailAsync(string email)
{
    return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
}

// CORRECT - Returns Empty
public async Task<User> GetByEmailAsync(string email)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    return user ?? User.Empty;
}
```

### ❌ Don't Skip the Base Class

```csharp
// WRONG - Reimplementing what base class provides
public class ManualRepository : IEquipmentRepository
{
    public async Task<IEnumerable<Equipment>> GetAllAsync()
    {
        // Why rewrite this when base class provides it?
        return await _context.Equipment.ToListAsync();
    }
    
    public async Task<Equipment> GetByIdAsync(EquipmentId id)
    {
        // Duplicating base class logic
        var equipment = await _context.Equipment.FindAsync(id);
        return equipment ?? Equipment.Empty;
    }
}
```

## Benefits of This Pattern

### 1. Compile-Time Safety
- Impossible to create repositories that return null
- Entity Empty pattern enforced by type system
- Mistakes caught before runtime

### 2. Code Reduction
- Common operations implemented once
- 80% less code in concrete repositories
- Focus on domain-specific methods only

### 3. Consistency
- All repositories behave identically for common operations
- Predictable API across the data layer
- Easier onboarding for new developers

### 4. Testability
- Base test suite covers common scenarios
- Only need to test specialized methods
- Guaranteed Empty pattern compliance

### 5. Maintainability
- Bug fixes in one place benefit all repositories
- Pattern changes propagate automatically
- Clear separation of concerns

## Summary

The Repository Base Class pattern is **mandatory** because it:

1. **Eliminates null at the architectural level** through compile-time constraints
2. **Reduces code duplication** by 80% or more
3. **Ensures consistency** across all data access
4. **Simplifies testing** through inheritance
5. **Prevents regressions** through type system enforcement

This is not a suggestion or preference - it's an architectural requirement that guarantees null safety across the entire data access layer.

## Related Documentation

- [Repository Base Class Architecture Decision](../Overview/RepositoryBaseClassArchitecture.md)
- [Empty Pattern Complete Guide](./EmptyPatternComplete.md)
- [Repository Pattern](./RepositoryPattern.md)
- [Testing Standards](./TestingStandards.md)

---

*Last Updated: 2025-01-16*
*Status: Mandatory Pattern - All repositories must comply*