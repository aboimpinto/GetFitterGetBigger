# EntityResult Pattern - Entity Creation Validation

**🎯 PURPOSE**: This document defines the **MANDATORY** EntityResult pattern for all entity creation methods in the GetFitterGetBigger API, ensuring validation without exceptions.

## Overview

`EntityResult<T>` is the standard return type for ALL entity Handler methods. It provides:
- Validation without exceptions
- Fluent validation API
- Clean conversion to ServiceResult
- Consistent error handling at domain layer

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: EntityResult Rules - MUST be followed          │
├──────────────────────────────────────────────────────────────┤
│ 1. ALL Handler methods return EntityResult<T>               │
│ 2. NEVER throw exceptions for validation                    │
│ 3. Use Validate.For<T>() fluent API                        │
│ 4. Convert to ServiceResult at service layer               │
│ 5. Entities handle their own creation validation           │
└──────────────────────────────────────────────────────────────┘
```

## Basic Usage

### ❌ BAD - Throwing Exceptions in Entity Creation

```csharp
public static class Handler
{
    public static WorkoutTemplate CreateNew(string name, string description, UserId createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
            
        if (name.Length < 3 || name.Length > 100)
            throw new ArgumentException("Name must be between 3 and 100 characters", nameof(name));
            
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
            
        return new WorkoutTemplate 
        { 
            Id = WorkoutTemplateId.New(),
            Name = name,
            Description = description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### ✅ GOOD - EntityResult Pattern with Fluent Validation

```csharp
public static class Handler
{
    public static EntityResult<WorkoutTemplate> CreateNew(string name, string description, UserId createdBy)
    {
        return Validate.For<WorkoutTemplate>()
            .EnsureNotEmpty(name, "Name cannot be empty")
            .EnsureLength(name, 3, 100, "Name must be between 3 and 100 characters")
            .EnsureNotEmpty(description, "Description cannot be empty")
            .EnsureNotEmpty(createdBy, "CreatedBy is required")
            .OnSuccess(() => new WorkoutTemplate 
            { 
                Id = WorkoutTemplateId.New(),
                Name = name,
                Description = description,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                State = WorkoutTemplateState.Draft
            });
    }
    
    public static EntityResult<WorkoutTemplate> UpdateName(WorkoutTemplate entity, string newName)
    {
        return Validate.For<WorkoutTemplate>()
            .EnsureNotEmpty(newName, "Name cannot be empty")
            .EnsureLength(newName, 3, 100, "Name must be between 3 and 100 characters")
            .Ensure(() => newName != entity.Name, "Name is unchanged")
            .OnSuccess(() => entity with 
            { 
                Name = newName,
                UpdatedAt = DateTime.UtcNow
            });
    }
}
```

## Service Layer Integration

Convert EntityResult to ServiceResult at the service layer:

```csharp
public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command)
{
    // Validate command at service level
    var validation = await ValidateCreateCommand(command);
    if (!validation.IsValid)
        return ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            validation.ServiceError);
    
    // Create entity using Handler
    var entityResult = WorkoutTemplate.Handler.CreateNew(
        command.Name,
        command.Description,
        command.CreatedBy);
    
    // Convert EntityResult to ServiceResult
    if (!entityResult.IsSuccess)
        return ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            ServiceError.ValidationFailed(entityResult.Errors));
    
    // Save to database
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    
    var savedEntity = await repository.CreateAsync(entityResult.Value);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<WorkoutTemplateDto>.Success(MapToDto(savedEntity));
}
```

## Validation Methods Available

The `Validate.For<T>()` fluent API provides:

```csharp
// String validations
.EnsureNotEmpty(value, errorMessage)
.EnsureLength(value, min, max, errorMessage)
.EnsureMaxLength(value, max, errorMessage)
.EnsureMinLength(value, min, errorMessage)
.EnsureRegex(value, pattern, errorMessage)

// Numeric validations
.EnsureRange(value, min, max, errorMessage)
.EnsureGreaterThan(value, threshold, errorMessage)
.EnsureLessThan(value, threshold, errorMessage)

// Object validations
.EnsureNotNull(value, errorMessage)
.EnsureNotEmpty(id, errorMessage)  // For specialized IDs

// Custom validations
.Ensure(predicate, errorMessage)
.EnsureAsync(asyncPredicate, errorMessage)

// Execution
.OnSuccess(() => entity)
.OnSuccessAsync(async () => await CreateEntityAsync())
```

## Complex Entity Creation Example

```csharp
public static class Handler
{
    public static EntityResult<Exercise> CreateNew(
        string name,
        string description,
        ExerciseType type,
        DifficultyLevel difficulty,
        IEnumerable<MuscleGroupId> primaryMuscles,
        IEnumerable<MuscleGroupId> secondaryMuscles,
        IEnumerable<EquipmentId> requiredEquipment)
    {
        return Validate.For<Exercise>()
            // Basic validations
            .EnsureNotEmpty(name, "Exercise name is required")
            .EnsureLength(name, 3, 100, "Name must be between 3 and 100 characters")
            .EnsureNotEmpty(description, "Description is required")
            
            // Enum validations
            .Ensure(() => Enum.IsDefined(typeof(ExerciseType), type), 
                "Invalid exercise type")
            .Ensure(() => Enum.IsDefined(typeof(DifficultyLevel), difficulty), 
                "Invalid difficulty level")
            
            // Collection validations
            .Ensure(() => primaryMuscles?.Any() == true, 
                "At least one primary muscle group is required")
            .Ensure(() => primaryMuscles?.All(m => !m.IsEmpty) == true, 
                "Invalid primary muscle group ID")
            .Ensure(() => secondaryMuscles?.All(m => !m.IsEmpty) != false, 
                "Invalid secondary muscle group ID")
            .Ensure(() => requiredEquipment?.All(e => !e.IsEmpty) != false, 
                "Invalid equipment ID")
            
            // Business rule validations
            .Ensure(() => !primaryMuscles?.Intersect(secondaryMuscles ?? []).Any() ?? true,
                "A muscle group cannot be both primary and secondary")
            
            // Create entity on success
            .OnSuccess(() => new Exercise
            {
                Id = ExerciseId.New(),
                Name = name,
                Description = description,
                Type = type,
                Difficulty = difficulty,
                PrimaryMuscles = primaryMuscles?.ToList() ?? [],
                SecondaryMuscles = secondaryMuscles?.ToList() ?? [],
                RequiredEquipment = requiredEquipment?.ToList() ?? [],
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
    }
}
```

## State Transition with EntityResult

```csharp
public static class Handler
{
    public static EntityResult<WorkoutTemplate> TransitionToDraft(WorkoutTemplate entity)
    {
        return Validate.For<WorkoutTemplate>()
            .Ensure(() => entity.State == WorkoutTemplateState.New,
                "Can only transition to Draft from New state")
            .Ensure(() => !string.IsNullOrWhiteSpace(entity.Name),
                "Template must have a name before transitioning to Draft")
            .OnSuccess(() => entity with 
            { 
                State = WorkoutTemplateState.Draft,
                UpdatedAt = DateTime.UtcNow
            });
    }
    
    public static EntityResult<WorkoutTemplate> Publish(WorkoutTemplate entity, UserId publishedBy)
    {
        return Validate.For<WorkoutTemplate>()
            .Ensure(() => entity.State == WorkoutTemplateState.Draft,
                "Can only publish from Draft state")
            .Ensure(() => entity.Exercises?.Any() == true,
                "Template must have at least one exercise")
            .EnsureNotEmpty(publishedBy, "PublishedBy is required")
            .OnSuccess(() => entity with 
            { 
                State = WorkoutTemplateState.Published,
                PublishedAt = DateTime.UtcNow,
                PublishedBy = publishedBy,
                UpdatedAt = DateTime.UtcNow
            });
    }
}
```

## Testing EntityResult

```csharp
[Fact]
public void CreateNew_WithValidData_ReturnsSuccess()
{
    // Arrange
    var name = "Test Workout";
    var description = "Test Description";
    var createdBy = UserId.New();
    
    // Act
    var result = WorkoutTemplate.Handler.CreateNew(name, description, createdBy);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(name, result.Value.Name);
    Assert.Equal(WorkoutTemplateState.Draft, result.Value.State);
}

[Fact]
public void CreateNew_WithEmptyName_ReturnsFailure()
{
    // Arrange
    var name = "";
    var description = "Test Description";
    var createdBy = UserId.New();
    
    // Act
    var result = WorkoutTemplate.Handler.CreateNew(name, description, createdBy);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Name cannot be empty", result.Errors);
}

[Fact]
public void TransitionToDraft_FromNewState_ReturnsSuccess()
{
    // Arrange
    var entity = new WorkoutTemplate
    {
        Id = WorkoutTemplateId.New(),
        Name = "Test",
        State = WorkoutTemplateState.New
    };
    
    // Act
    var result = WorkoutTemplate.Handler.TransitionToDraft(entity);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(WorkoutTemplateState.Draft, result.Value.State);
}
```

## Migration Checklist

When adopting EntityResult pattern:

- [ ] Move all validation logic to Handler methods
- [ ] Replace exceptions with `Validate.For<T>()`
- [ ] Return `EntityResult<T>` from all Handler methods
- [ ] Convert to ServiceResult at service layer
- [ ] Update tests to check EntityResult
- [ ] Remove try-catch blocks around entity creation
- [ ] Ensure all validation messages are clear

## Key Principles

1. **Entities Own Their Validation**: All entity creation/update validation happens in Handler
2. **No Exceptions**: Validation never throws
3. **Fluent API**: Chain validations for readability
4. **Service Conversion**: Services convert EntityResult to ServiceResult
5. **Testable**: Easy to test validation logic

## Common Patterns

### Factory Method Pattern

```csharp
public static class Handler
{
    public static EntityResult<Equipment> CreateBarbell(string name, decimal weight)
    {
        return CreateNew(name, $"Barbell - {weight}kg", EquipmentType.Barbell)
            .Chain(e => ValidateWeight(e, weight));
    }
    
    public static EntityResult<Equipment> CreateDumbbell(string name, decimal weight)
    {
        return CreateNew(name, $"Dumbbell - {weight}kg", EquipmentType.Dumbbell)
            .Chain(e => ValidateWeight(e, weight));
    }
    
    private static EntityResult<Equipment> CreateNew(string name, string description, EquipmentType type)
    {
        return Validate.For<Equipment>()
            .EnsureNotEmpty(name, "Name is required")
            .EnsureNotEmpty(description, "Description is required")
            .OnSuccess(() => new Equipment
            {
                Id = EquipmentId.New(),
                Name = name,
                Description = description,
                Type = type
            });
    }
    
    private static EntityResult<Equipment> ValidateWeight(Equipment equipment, decimal weight)
    {
        return Validate.For<Equipment>()
            .Ensure(() => weight > 0, "Weight must be positive")
            .Ensure(() => weight <= 500, "Weight cannot exceed 500kg")
            .OnSuccess(() => equipment with { Weight = weight });
    }
}
```

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - ServiceResult conversion
- `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - Service-level validation