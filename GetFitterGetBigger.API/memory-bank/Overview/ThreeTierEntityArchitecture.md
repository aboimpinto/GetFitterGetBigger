# Three-Tier Entity Architecture

## Overview

This document describes the three-tier entity classification system implemented in the GetFitterGetBigger API. This architecture provides a clear mental model for categorizing entities based on their mutability, caching requirements, and access patterns.

## The Three Tiers

### 1. Pure References ("Eternal Constants")

**Philosophy**: These are the "laws of physics" of your domain - they define what IS possible in the system.

**Examples**: 
- `BodyPart` (Chest, Back, Legs, etc.)
- `DifficultyLevel` (Beginner, Intermediate, Advanced)
- `WorkoutObjective` (Strength, Hypertrophy, Endurance)

**Characteristics**:
- Seeded at system inception
- Change requires system evolution (new deployment)
- No user can modify them
- They ARE the business rules
- Implement `IPureReference` interface

**Caching**: 
- Cache forever (until app restart)
- 365-day TTL
- Never invalidated

**Access Pattern**: 
- Read-only API
- Always served from cache after first load

### 2. Enhanced References ("Governed Variables")

**Philosophy**: Admin-controlled vocabulary that shapes user experience.

**Examples**: 
- `MuscleGroup` (has relationship to BodyPart)
- `ExerciseType` (Push, Pull, Static)
- `Equipment` (Barbell, Dumbbell, Machine)

**Characteristics**:
- Admin/Personal Trainer can add new ones
- Have relationships to Pure References
- Define "available options" for users
- Infrequent changes
- Implement `IEnhancedReference` interface

**Caching**: 
- Cache aggressively with invalidation hooks
- 1-hour TTL by default
- Invalidated on any CUD operation

**Access Pattern**: 
- Read-heavy, occasional admin writes
- Full CRUD for admins only

### 3. Domain Entities ("Living Data")

**Philosophy**: The actual user data - constantly evolving.

**Examples**: 
- `Exercise` (user-created exercises)
- `Workout` (workout plans)
- `User` (user accounts)
- `WorkoutSession` (logged workouts)

**Characteristics**:
- User-created and modified
- Complex relationships
- Business logic and workflows
- Frequent changes
- Implement `IDomainEntity` interface

**Caching**: 
- Selective or no caching
- 5-minute TTL if cached
- Complex invalidation patterns

**Access Pattern**: 
- Full CRUD for authorized users
- Complex queries and filters

## Technical Implementation

### Interface Hierarchy

```csharp
// Core interfaces
public interface IEntity
{
    string Id { get; }
    bool IsActive { get; }
}

public interface ITrackedEntity : IEntity
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
}

public interface IReferenceEntity : IEntity
{
    string Value { get; }
    string? Description { get; }
}

// Tier-specific interfaces
public interface IPureReference : IReferenceEntity, ICacheableEntity { }
public interface IEnhancedReference : IReferenceEntity, ICacheableEntity, ITrackedEntity { }
public interface IDomainEntity : ITrackedEntity { }
```

### Service Base Classes

Each tier has a corresponding service base class:

1. **PureReferenceService<TEntity, TDto>**
   - Read-only operations
   - Eternal caching
   - No invalidation logic

2. **EnhancedReferenceService<TEntity, TDto, TCreateCommand, TUpdateCommand>**
   - Full CRUD operations
   - Cache invalidation on modifications
   - Admin authorization checks

3. **DomainEntityService<TEntity, TDto, TCreateCommand, TUpdateCommand>** (Reference Implementation)
   - EXISTS but NOT USED in practice
   - Domain entities have too much unique business logic for generic abstraction
   - Each service (ExerciseService, WorkoutTemplateService) implements its interface directly
   - The base class serves as documentation and a template pattern
   - Key characteristics of domain services:
     - Full CRUD with complex business logic
     - Minimal or no caching
     - Complex validation rules
     - Cross-domain operations
     - Unique workflows per entity

### Repository Interfaces

```csharp
// Pure References - read only
public interface IPureReferenceRepository<T> : IReadOnlyRepository<T> 
    where T : class, IPureReference { }

// Enhanced References - full CRUD
public interface IEnhancedReferenceRepository<T> : IWritableRepository<T> 
    where T : class, IEnhancedReference { }

// Domain Entities - full CRUD with complex queries
public interface IDomainRepository<T> : IWritableRepository<T> 
    where T : class, IDomainEntity { }
```

## Implementation Example: BodyPart

### 1. Entity Definition

```csharp
public record BodyPart : ReferenceDataBase, IPureReference
{
    public BodyPartId BodyPartId { get; init; }
    public string Id => BodyPartId.ToString();
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
}
```

### 2. Service Implementation

```csharp
public class BodyPartService : PureReferenceService<BodyPart, BodyPartDto>, IBodyPartService
{
    // Inherits all caching and read-only behavior from base class
    // Only needs to implement entity-specific mapping and loading
}
```

### 3. Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
public class BodyPartsController : ControllerBase
{
    private readonly IBodyPartService _bodyPartService;
    
    // No direct access to repositories or UnitOfWork
    // All operations go through the service layer
}
```

## Benefits

1. **Clear Mental Model**: Developers instantly understand entity behavior
2. **Appropriate Optimization**: Each tier gets optimal caching strategy
3. **Evolution-Friendly**: Easy to promote entities between tiers
4. **Self-Documenting**: Interfaces declare intent
5. **Performance**: Pure refs cached forever, enhanced refs smartly invalidated
6. **Consistency**: Same patterns, different behaviors

## Migration Guide

When refactoring existing entities:

1. **Classify the Entity**: Determine which tier it belongs to
2. **Implement the Interface**: Add appropriate interface (IPureReference, IEnhancedReference, or IDomainEntity)
3. **Create/Update Service**: Extend the appropriate service base class
4. **Update Repository**: Implement tier-specific repository interface
5. **Refactor Controller**: Remove direct repository access, use service instead

## Configuration

Caching behavior can be customized via configuration:

```json
{
  "EntityConfiguration": {
    "Entities": {
      "BodyPart": {
        "Tier": "PureReference",
        "CacheStrategy": "Eternal",
        "RequiresAdminForWrite": true
      },
      "MuscleGroup": {
        "Tier": "EnhancedReference", 
        "CacheStrategy": "Invalidatable",
        "CacheDuration": "01:00:00",
        "RequiresAdminForWrite": true
      },
      "Exercise": {
        "Tier": "DomainEntity",
        "CacheStrategy": "None",
        "RequiresAdminForWrite": false
      }
    }
  }
}
```

## Related Documentation

- [CONTROLLER-SERVICE-CLEAN-ARCHITECTURE.md](./CONTROLLER-SERVICE-CLEAN-ARCHITECTURE.md) - Clean architecture patterns
- [SERVICE-LAYER-PATTERNS.md](./SERVICE-LAYER-PATTERNS.md) - Service layer implementation
- [SERVICE-RESULT-PATTERN.md](./SERVICE-RESULT-PATTERN.md) - Error handling patterns
- [common-implementation-pitfalls.md](./common-implementation-pitfalls.md) - Common mistakes to avoid

---

**Remember**: This architecture creates a robust, maintainable system where each entity type gets exactly the caching and access patterns it needs, while maintaining consistent coding patterns across the entire codebase.