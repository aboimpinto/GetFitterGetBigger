# Single Exit Point Pattern - Pattern Matching for Clean Code

**🎯 PURPOSE**: This document defines the **MANDATORY** single exit point pattern using pattern matching for all service methods in the GetFitterGetBigger API.

## Overview

Single exit point with pattern matching provides:
- Predictable code flow
- Lower cyclomatic complexity
- Better readability
- Easier debugging
- Consistent method structure

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: Single Exit Point Rules - MUST be followed     │
├──────────────────────────────────────────────────────────────┤
│ 1. ONE return statement per method                          │
│ 2. Use pattern matching for branching logic                 │
│ 3. Extract complex logic to helper methods                  │
│ 4. No early returns in service methods                      │
│ 5. Store result in variable, return at end                  │
└──────────────────────────────────────────────────────────────┘
```

## Basic Pattern

### ❌ VIOLATION - Multiple Exit Points

```csharp
public async Task<ServiceResult<WorkoutStateDto>> GetFromCacheOrLoadAsync(
    string cacheKey, 
    Func<Task<WorkoutState>> loadFunc)
{
    var cacheResult = await _cacheService.GetAsync<WorkoutStateDto>(cacheKey);
    if (cacheResult.IsHit)
        return ServiceResult<WorkoutStateDto>.Success(cacheResult.Value);  // Exit 1
    
    var entity = await loadFunc();
    if (entity.IsEmpty)
        return ServiceResult<WorkoutStateDto>.Failure(                    // Exit 2
            WorkoutStateDto.Empty, 
            ServiceError.NotFound());
    
    var dto = MapToDto(entity);
    await _cacheService.SetAsync(cacheKey, dto);
    
    return ServiceResult<WorkoutStateDto>.Success(dto);                    // Exit 3
}
```

### ✅ CORRECT - Single Exit with Pattern Matching

```csharp
public async Task<ServiceResult<WorkoutStateDto>> GetFromCacheOrLoadAsync(
    string cacheKey, 
    Func<Task<WorkoutState>> loadFunc)
{
    var cacheResult = await _cacheService.GetAsync<WorkoutStateDto>(cacheKey);
    
    var result = cacheResult.IsHit
        ? ServiceResult<WorkoutStateDto>.Success(cacheResult.Value)
        : await ProcessUncachedEntity(await loadFunc(), cacheKey);
        
    return result;  // Single exit point
}

private async Task<ServiceResult<WorkoutStateDto>> ProcessUncachedEntity(
    WorkoutState entity, 
    string cacheKey) =>
    entity switch
    {
        { IsEmpty: true } => ServiceResult<WorkoutStateDto>.Failure(
            WorkoutStateDto.Empty, 
            ServiceError.NotFound()),
        _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
    };
```

## Pattern Matching Techniques

### Switch Expressions

```csharp
public ServiceResult<string> GetStatusMessage(WorkoutState state)
{
    var message = state switch
    {
        WorkoutState.Draft => "Workout is in draft state",
        WorkoutState.Published => "Workout is published and active",
        WorkoutState.Archived => "Workout has been archived",
        _ => "Unknown workout state"
    };
    
    return ServiceResult<string>.Success(message);
}
```

### Property Pattern Matching

```csharp
public async Task<ServiceResult<EquipmentDto>> ProcessEquipmentAsync(Equipment equipment)
{
    var result = equipment switch
    {
        { IsEmpty: true } => ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty, 
            ServiceError.NotFound("Equipment")),
            
        { IsActive: false } => ServiceResult<EquipmentDto>.Failure(
            MapToDto(equipment), 
            ServiceError.ValidationFailed("Equipment is inactive")),
            
        { Type: EquipmentType.Barbell, Weight: > 100 } => await ProcessHeavyBarbellAsync(equipment),
        
        { Type: EquipmentType.Dumbbell } => await ProcessDumbbellAsync(equipment),
        
        _ => ServiceResult<EquipmentDto>.Success(MapToDto(equipment))
    };
    
    return result;
}
```

### Tuple Pattern Matching

```csharp
public async Task<ServiceResult<WorkoutPlanDto>> ValidateAndCreatePlanAsync(
    CreatePlanCommand command)
{
    var userExists = await _userService.ExistsAsync(command.UserId);
    var templateExists = await _templateService.ExistsAsync(command.TemplateId);
    
    var result = (userExists, templateExists) switch
    {
        (false, _) => ServiceResult<WorkoutPlanDto>.Failure(
            WorkoutPlanDto.Empty,
            ServiceError.InvalidReference("UserId", command.UserId)),
            
        (_, false) => ServiceResult<WorkoutPlanDto>.Failure(
            WorkoutPlanDto.Empty,
            ServiceError.InvalidReference("TemplateId", command.TemplateId)),
            
        (true, true) => await CreatePlanAsync(command)
    };
    
    return result;
}
```

## Complex Service Method Example

```csharp
public async Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(
    WorkoutTemplateId id, 
    UpdateTemplateCommand command)
{
    // Step 1: Validate
    var validationResult = await ValidateUpdateAsync(id, command);
    
    // Step 2: Process based on validation
    var result = validationResult.IsValid
        ? await ProcessUpdateAsync(id, command)
        : ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            validationResult.ServiceError);
    
    return result;  // Single exit
}

private async Task<ServiceResult<WorkoutTemplateDto>> ProcessUpdateAsync(
    WorkoutTemplateId id, 
    UpdateTemplateCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    
    var result = entity switch
    {
        { IsEmpty: true } => ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            ServiceError.NotFound("WorkoutTemplate", id)),
            
        { State: WorkoutTemplateState.Published } => ServiceResult<WorkoutTemplateDto>.Failure(
            MapToDto(entity),
            ServiceError.ValidationFailed("Cannot update published template")),
            
        _ => await ApplyUpdateAndSaveAsync(entity, command, repository, unitOfWork)
    };
    
    return result;
}
```

## Conditional Logic Patterns

### Ternary Operator for Simple Cases

```csharp
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var exists = await repository.ExistsAsync(id);
    
    var result = exists
        ? ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true))
        : ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false));
        
    return result;
}
```

### Nested Pattern Matching

```csharp
public async Task<ServiceResult<ExerciseDto>> GetExerciseWithValidationAsync(
    ExerciseId id,
    UserId userId)
{
    var exercise = await LoadExerciseAsync(id);
    var userAccess = await CheckUserAccessAsync(userId);
    
    var result = (exercise, userAccess) switch
    {
        ({ IsEmpty: true }, _) => ServiceResult<ExerciseDto>.Failure(
            ExerciseDto.Empty,
            ServiceError.NotFound("Exercise", id)),
            
        (_, { HasAccess: false }) => ServiceResult<ExerciseDto>.Failure(
            ExerciseDto.Empty,
            ServiceError.Forbidden()),
            
        ({ Difficulty: DifficultyLevel.Expert }, { UserLevel: < UserLevel.Advanced }) => 
            ServiceResult<ExerciseDto>.Failure(
                MapToDto(exercise),
                ServiceError.ValidationFailed("Insufficient user level for expert exercises")),
                
        _ => ServiceResult<ExerciseDto>.Success(MapToDto(exercise))
    };
    
    return result;
}
```

## Helper Method Extraction

```csharp
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    var validationResult = await ValidateCreateAsync(command);
    
    var result = validationResult.IsValid
        ? await ExecuteCreateAsync(command)
        : ConvertValidationFailure(validationResult);
        
    return result;
}

// Helper methods keep main method clean
private async Task<ValidationResult> ValidateCreateAsync(CreateEquipmentCommand command)
{
    return await ServiceValidate.Build()
        .EnsureNotNull(command, "Command cannot be null")
        .EnsureNotWhiteSpace(command?.Name, "Name is required")
        .EnsureAsync(
            async () => !await CheckDuplicateNameAsync(command?.Name),
            "Name already exists")
        .ToValidationResultAsync();
}

private async Task<ServiceResult<EquipmentDto>> ExecuteCreateAsync(CreateEquipmentCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = Equipment.Create(command.Name, command.Description);
    entity = await repository.CreateAsync(entity);
    await unitOfWork.CommitAsync();
    
    await InvalidateCacheAsync();
    
    return ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}

private ServiceResult<EquipmentDto> ConvertValidationFailure(ValidationResult validation)
{
    return ServiceResult<EquipmentDto>.Failure(
        EquipmentDto.Empty,
        validation.ServiceError ?? ServiceError.ValidationFailed(validation.Errors));
}
```

## Benefits of Single Exit Point

### Before - Multiple Returns (Complex Flow)

```csharp
public async Task<ServiceResult<T>> ComplexMethod(params)
{
    if (condition1)
        return failure1;
        
    var data = await LoadData();
    if (data == null)
        return failure2;
        
    if (!ValidateData(data))
        return failure3;
        
    var processed = ProcessData(data);
    if (processed.HasError)
        return failure4;
        
    return success;  // Where's the actual success path in all this?
}
```

### After - Single Exit (Clear Flow)

```csharp
public async Task<ServiceResult<T>> ComplexMethod(params)
{
    var result = await DetermineResult(params);
    return result;
}

private async Task<ServiceResult<T>> DetermineResult(params)
{
    return condition1 ? failure1
        : await LoadAndProcessData(params);
}

private async Task<ServiceResult<T>> LoadAndProcessData(params)
{
    var data = await LoadData();
    
    return data switch
    {
        null => failure2,
        _ when !ValidateData(data) => failure3,
        _ => ProcessAndReturnResult(data)
    };
}
```

## Metrics Improvement

Using single exit point pattern typically results in:

- **30-50% reduction** in cyclomatic complexity
- **20-40% shorter** method length
- **Easier testing** - fewer code paths to test
- **Better readability** - linear flow is easier to follow

## Anti-Patterns to Avoid

### ❌ Guard Clauses with Returns

```csharp
// WRONG - Multiple returns
public async Task<ServiceResult<T>> Method(params)
{
    if (param1 == null)
        return ServiceResult<T>.Failure(...);  // Return 1
        
    if (!IsValid(param2))
        return ServiceResult<T>.Failure(...);  // Return 2
        
    // ... more returns
}
```

### ❌ Nested If-Else Chains

```csharp
// WRONG - Deep nesting
public async Task<ServiceResult<T>> Method(params)
{
    if (condition1)
    {
        if (condition2)
        {
            if (condition3)
            {
                return success;
            }
            else
            {
                return failure3;
            }
        }
        else
        {
            return failure2;
        }
    }
    else
    {
        return failure1;
    }
}
```

## Testing Single Exit Methods

```csharp
[Fact]
public async Task UpdateAsync_WithVariousConditions_ReturnsSingleResult()
{
    // The beauty of single exit: only one return to verify
    
    // Arrange
    var testCases = new[]
    {
        (condition: "empty_id", expected: ServiceErrorCode.InvalidFormat),
        (condition: "not_found", expected: ServiceErrorCode.NotFound),
        (condition: "published", expected: ServiceErrorCode.ValidationFailed),
        (condition: "valid", expected: ServiceErrorCode.None)
    };
    
    foreach (var testCase in testCases)
    {
        // Act
        var result = await ArrangeAndExecute(testCase.condition);
        
        // Assert - Always checking the same return point
        if (testCase.expected == ServiceErrorCode.None)
            Assert.True(result.IsSuccess);
        else
            Assert.Equal(testCase.expected, result.PrimaryErrorCode);
    }
}
```

## Migration Checklist

When refactoring to single exit point:

- [ ] Identify all return statements
- [ ] Create result variable at method start
- [ ] Convert if-return chains to pattern matching
- [ ] Extract complex logic to helper methods
- [ ] Use switch expressions for multiple conditions
- [ ] Ensure single return at method end
- [ ] Verify all code paths set result
- [ ] Update tests to verify single exit

## Key Principles

1. **One Return**: Every method has exactly one return statement
2. **Pattern Matching**: Use switch expressions for branching
3. **Helper Methods**: Extract complex logic for clarity
4. **Result Variable**: Store outcome, return at end
5. **Linear Flow**: Code reads top to bottom

## Summary

Single exit point with pattern matching:
- Makes code more predictable
- Reduces complexity
- Improves maintainability
- Enforces consistent structure
- Simplifies debugging

> "If your method has more than one return statement, you're doing it wrong."

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - Validation patterns
- `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - Result patterns
- `/memory-bank/CodeQualityGuidelines/ModernCSharpPatterns.md` - Pattern matching features