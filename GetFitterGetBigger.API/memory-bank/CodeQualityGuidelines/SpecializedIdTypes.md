# Specialized ID Types - Type-Safe Entity Identifiers

**🎯 PURPOSE**: This document defines the **MANDATORY** specialized ID type patterns that provide type safety and prevent ID misuse in the GetFitterGetBigger API.

## Overview

Specialized ID types provide:
- **Type Safety**: Compile-time prevention of ID misuse
- **Domain Clarity**: Clear entity relationships
- **Validation**: Built-in format validation
- **Empty Pattern**: Consistent handling of invalid IDs

## ID Format Standard

All specialized IDs follow the format: `{entitytype}-{guid}`

Examples:
- `equipment-550e8400-e29b-41d4-a716-446655440000`
- `exercise-123e4567-e89b-12d3-a456-426614174000`
- `bodypart-987fcdeb-51a2-43f1-9abc-def012345678`

## Basic Usage

### ❌ BAD - String IDs Everywhere

```csharp
// No type safety - any string can be passed
public async Task<ServiceResult<ExerciseDto>> GetExercise(string id) { }
public async Task<ServiceResult<bool>> AddMuscleToExercise(string exerciseId, string muscleId) { }

// Easy to mix up IDs
var result = await AddMuscleToExercise(muscleId, exerciseId); // Oops! Wrong order
```

### ✅ GOOD - Specialized ID Types

```csharp
// Type-safe - compiler prevents ID misuse
public async Task<ServiceResult<ExerciseDto>> GetExercise(ExerciseId id) { }
public async Task<ServiceResult<bool>> AddMuscleToExercise(ExerciseId exerciseId, MuscleGroupId muscleId) { }

// Compiler error if IDs are mixed up
var result = await AddMuscleToExercise(muscleId, exerciseId); // Compile error!
```

## Implementing Specialized ID Types

```csharp
public record EquipmentId : ISpecializedId<EquipmentId>
{
    private const string Prefix = "equipment";
    
    public Guid Value { get; }
    
    private EquipmentId(Guid value) => Value = value;
    
    // Factory methods
    public static EquipmentId New() => new(Guid.NewGuid());
    public static EquipmentId Empty => new(Guid.Empty);
    
    // Parsing
    public static EquipmentId ParseOrEmpty(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Empty;
            
        var parts = id.Split('-');
        if (parts.Length != 6 || parts[0] != Prefix)
            return Empty;
            
        var guidString = string.Join("-", parts.Skip(1));
        return Guid.TryParse(guidString, out var guid) 
            ? new EquipmentId(guid) 
            : Empty;
    }
    
    public static bool TryParse(string? id, out EquipmentId result)
    {
        result = ParseOrEmpty(id);
        return !result.IsEmpty;
    }
    
    // Properties
    public bool IsEmpty => Value == Guid.Empty;
    
    // String representation
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{Value}";
}
```

## 🎯 ParseOrEmpty Validation Pattern

**CRITICAL**: Always use `ParseOrEmpty` for ID validation. It handles ALL invalid inputs uniformly.

### ❌ REDUNDANT - Double Validation Anti-Pattern

```csharp
public async Task<ServiceResult<TDto>> GetByIdAsync(string id)
{
    return await ServiceValidate.For<TDto>()
        .EnsureNotWhiteSpace(id, ErrorMessages.InvalidIdFormat)  // First validation
        .MatchAsync(
            whenValid: async () =>
            {
                var specializedId = TId.ParseOrEmpty(id);
                
                return await ServiceValidate.For<TDto>()
                    .EnsureNotEmpty(specializedId, ErrorMessages.InvalidIdFormat)  // Redundant!
                    .MatchAsync(
                        whenValid: async () => await LoadByIdFromDatabaseAsync(specializedId)
                    );
            }
        );
}
```

### ✅ OPTIMAL - Parse Once, Validate Once

```csharp
public async Task<ServiceResult<TDto>> GetByIdAsync(string id)
{
    var specializedId = TId.ParseOrEmpty(id);  // Handles all invalid inputs
    
    return await ServiceValidate.For<TDto>()
        .EnsureNotEmpty(specializedId, ErrorMessages.InvalidIdFormat)  // Single validation
        .MatchAsync(
            whenValid: async () => await LoadByIdFromDatabaseAsync(specializedId)
        );
}
```

## ParseOrEmpty Behavior

`ParseOrEmpty` returns `TId.Empty` for ALL invalid inputs:

| Input | Result |
|-------|--------|
| `null` | `TId.Empty` |
| `""` (empty string) | `TId.Empty` |
| `"   "` (whitespace) | `TId.Empty` |
| `"invalid-format"` | `TId.Empty` |
| `"wrong-prefix-guid"` | `TId.Empty` |
| `"entity-valid-guid"` | Valid `TId` |

## Controller Usage Pattern

Controllers should use `ParseOrEmpty` when calling services:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id) =>
    await _service.GetByIdAsync(EquipmentId.ParseOrEmpty(id)) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
        { Errors: var errors } => BadRequest(new { errors })
    };

[HttpPut("{id}")]
public async Task<IActionResult> Update(string id, [FromBody] UpdateEquipmentDto dto) =>
    await _service.UpdateAsync(
        EquipmentId.ParseOrEmpty(id),  // Parse in controller
        dto.ToCommand()) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

## Service Method Patterns

### With Specialized ID Parameter

```csharp
public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
{
    return await ServiceValidate.For<EquipmentDto>()
        .EnsureNotEmpty(id, EquipmentErrorMessages.InvalidIdFormat)
        .MatchAsync(
            whenValid: async () => await LoadByIdFromDatabaseAsync(id)
        );
}
```

### With String ID Overload

```csharp
// String overload for convenience
public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(string id)
{
    var equipmentId = EquipmentId.ParseOrEmpty(id);
    return await GetByIdAsync(equipmentId);  // Delegate to specialized version
}
```

## Non-ID String Validation

**IMPORTANT**: Use `EnsureNotWhiteSpace` for regular string parameters (not IDs):

```csharp
// ✅ CORRECT - Use EnsureNotWhiteSpace for non-ID strings
public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
{
    return await ServiceValidate.For<ReferenceDataDto>()
        .EnsureNotWhiteSpace(value, ErrorMessages.ValueCannotBeEmpty)  // String validation!
        .MatchAsync(
            whenValid: async () => await LoadByValueFromDatabaseAsync(value)
        );
}

// ✅ CORRECT - Use EnsureNotWhiteSpace for names, descriptions, etc.
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    return await ServiceValidate.For<EquipmentDto>()
        .EnsureNotNull(command, ErrorMessages.CommandCannotBeNull)
        .EnsureNotWhiteSpace(command?.Name, ErrorMessages.NameCannotBeEmpty)  // String property!
        .EnsureNotWhiteSpace(command?.Description, ErrorMessages.DescriptionCannotBeEmpty)
        .MatchAsync(
            whenValid: async () => await CreateEquipmentAsync(command)
        );
}
```

## The Validation Rule

- **For SpecializedId**: Parse first with `ParseOrEmpty`, then `EnsureNotEmpty`
- **For regular strings**: Use `EnsureNotWhiteSpace` directly

## Repository Usage

Repositories work directly with specialized IDs:

```csharp
public interface IEquipmentRepository
{
    Task<Equipment> GetByIdAsync(EquipmentId id);
    Task<bool> ExistsAsync(EquipmentId id);
    Task<Equipment> CreateAsync(Equipment entity);
    Task<Equipment> UpdateAsync(Equipment entity);
    Task DeleteAsync(EquipmentId id);
}
```

## Entity Usage

Entities use specialized IDs as properties:

```csharp
public record Equipment : IEmptyEntity<Equipment>
{
    public EquipmentId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    
    public bool IsEmpty => Id.IsEmpty;
    
    public static Equipment Empty => new()
    {
        Id = EquipmentId.Empty,
        Name = string.Empty,
        Description = string.Empty
    };
}
```

## DTO Mapping

DTOs use string representation for API responses:

```csharp
public class EquipmentDto : IEmptyDto<EquipmentDto>
{
    public string Id { get; set; }  // String for JSON serialization
    public string Name { get; set; }
    
    public bool IsEmpty => string.IsNullOrEmpty(Id);
    
    public static EquipmentDto Empty => new()
    {
        Id = string.Empty,
        Name = string.Empty
    };
}

// Mapping
public static EquipmentDto ToDto(this Equipment entity) => new()
{
    Id = entity.Id.ToString(),  // Convert to string
    Name = entity.Name
};
```

## Benefits Summary

1. **Type Safety**: Impossible to pass wrong ID type
2. **Clear Intent**: Method signatures show exactly what IDs are expected
3. **Built-in Validation**: ParseOrEmpty handles all edge cases
4. **Consistent Pattern**: Same approach across all entities
5. **Refactoring Safety**: Compiler catches ID type changes

## Migration Checklist

When implementing specialized IDs:

- [ ] Create specialized ID type implementing ISpecializedId<T>
- [ ] Update entity to use specialized ID type
- [ ] Update repository interface to use specialized ID
- [ ] Update service methods to accept specialized ID
- [ ] Add ParseOrEmpty usage in controllers
- [ ] Update tests to use specialized IDs
- [ ] Remove string ID validation in favor of ParseOrEmpty

## Key Principles

> "Parse once at the boundary, work with type-safe IDs internally."

## Common Mistakes to Avoid

- ❌ Double validation (string then specialized)
- ❌ Using string IDs in service/repository layers
- ❌ Forgetting to implement IsEmpty property
- ❌ Not using ParseOrEmpty in controllers
- ❌ Mixing up ID types in method calls

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - Validation patterns
- `/memory-bank/CodeQualityGuidelines/EmptyObjectPattern.md` - Empty pattern integration