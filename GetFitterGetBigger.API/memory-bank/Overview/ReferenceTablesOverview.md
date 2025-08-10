# Reference Tables Overview

## What Are Reference Tables?

Reference tables are lookup tables that contain relatively static data used throughout the application. They provide standardized values for various attributes and ensure data consistency across the system.

## Complete List of Reference Tables

### 1. Core Reference Tables (Inheriting from ReferenceDataBase)

These tables use the standardized ReferenceDataBase pattern with consistent fields:

| Table | Entity | Purpose | Cache Duration |
|-------|--------|---------|----------------|
| BodyParts | BodyPart | Body regions (Upper Body, Lower Body, Core, Full Body) | 24 hours |
| DifficultyLevels | DifficultyLevel | Exercise difficulty ratings | 24 hours |
| ExerciseTypes | ExerciseType | Exercise categories (Warmup, Workout, Cooldown, Rest) | 24 hours |
| KineticChainTypes | KineticChainType | Movement types (Open Chain, Closed Chain) | 24 hours |
| MetricTypes | MetricType | Measurement units (Repetitions, Time, Distance, Weight) | 24 hours |
| MovementPatterns | MovementPattern | Movement classifications (Push, Pull, Squat, etc.) | 24 hours |
| MuscleRoles | MuscleRole | Muscle involvement levels (Primary, Secondary, Stabilizer) | 24 hours |
| **ExerciseWeightTypes** | ExerciseWeightType | Weight handling rules (NEW - Added in FEAT-023) | 24 hours |

### 2. Dynamic Reference Tables (Custom Implementation)

These tables have custom structures and may support CRUD operations:

| Table | Entity | Purpose | Cache Duration | CRUD Enabled |
|-------|--------|---------|----------------|--------------|
| MuscleGroups | MuscleGroup | Specific muscle groups | 1 hour | Yes |
| Equipment | Equipment | Exercise equipment types | 1 hour | Yes |

### 3. System Tables (Not True Reference Tables)

These are system entities that provide reference-like functionality:

| Table | Entity | Purpose | Notes |
|-------|--------|---------|-------|
| Claims | Claim | User authorization claims | Not cached |
| Users | User | System users | Not cached |

## Reference Table Patterns

### 1. ReferenceDataBase Pattern

Most reference tables inherit from `ReferenceDataBase`:

```csharp
public abstract record ReferenceDataBase<TId>
{
    public TId Id { get; init; }
    public string Code { get; init; }  // Uppercase, underscore-separated
    public string Value { get; init; } // Human-readable name
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}
```

### 2. Controller Pattern

Reference table controllers inherit from `BaseReferenceTableController<TEntity, TEntityId>`:
- Provides standard GET endpoints
- Implements caching automatically
- Follows consistent routing pattern: `/api/ReferenceTables/{TableName}`

### 3. Service Pattern

Each reference table has a corresponding service implementing `IReferenceTableService<T>`:
- Handles all business logic
- Manages caching
- Controls repository access

## API Endpoints

### Standard Endpoints (All Reference Tables)

```
GET /api/ReferenceTables/{TableName}              # Get all items
GET /api/ReferenceTables/{TableName}/{id}         # Get by ID
GET /api/ReferenceTables/{TableName}/ByValue/{value}  # Get by value (case-insensitive)
```

### Additional Endpoints (Some Tables)

```
GET /api/ReferenceTables/{TableName}/ByCode/{code}    # Get by code (ExerciseWeightType, ExerciseType)
```

### CRUD Endpoints (Dynamic Tables Only)

```
POST /api/{TableName}                    # Create new item
PUT /api/{TableName}/{id}               # Update item
DELETE /api/{TableName}/{id}            # Delete/deactivate item
```

## Caching Strategy

### Static Reference Tables (24-hour cache)
- Data rarely changes
- Cached for performance
- Cache key pattern: `reference_table_{tableName}_all`

### Dynamic Reference Tables (1-hour cache)
- Data can be modified by users
- Shorter cache duration
- Cache invalidated on mutations

## Authorization

### Current State
- All GET endpoints are public (no authentication required)

### Future State
- GET endpoints: Require authentication
- CRUD operations: Require `ReferenceData-Management` claim

## Database Seeding

Reference tables are seeded during migrations with predefined data:
- Each table has specific seed data
- IDs are predetermined for consistency
- Seed data includes sort orders for proper display

## Integration Points

### Exercise Entity
- Uses: DifficultyLevel, KineticChainType, ExerciseWeightType
- Many-to-many: ExerciseTypes

### MuscleGroup Entity
- References: BodyPart

### User Entity
- Many-to-many: Claims

## Recent Changes

### FEAT-023: ExerciseWeightType (In Progress)
- Added new reference table for exercise weight validation
- 5 predefined types: BODYWEIGHT_ONLY, BODYWEIGHT_OPTIONAL, WEIGHT_REQUIRED, MACHINE_WEIGHT, NO_WEIGHT
- Integrated with Exercise entity
- Includes weight validation service

## Future Enhancements

1. **Custom Reference Tables**: Allow PTs to create custom reference values
2. **Localization**: Multi-language support for reference table values
3. **Versioning**: Track changes to reference data over time
4. **Bulk Operations**: Import/export reference data
5. **Validation Rules**: Define complex validation rules for reference data usage

## Best Practices

1. **Always use strongly-typed IDs** for reference tables
2. **Cache static data aggressively** (24-hour minimum)
3. **Use codes for programmatic access**, values for display
4. **Maintain backward compatibility** when modifying reference data
5. **Document all reference table changes** in migration guides
6. **Test cache invalidation** when implementing CRUD operations

## Related Documentation

- `/memory-bank/REFERENCE_TABLE_CRUD_PROCESS.md` - How to convert read-only tables to CRUD
- `/memory-bank/Overview/CacheConfiguration.md` - Caching configuration details
- `/memory-bank/Overview/CacheInvalidationStrategy.md` - Cache invalidation patterns
- `/api-docs/reference-tables/` - Individual table API documentation