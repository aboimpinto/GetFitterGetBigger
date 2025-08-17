# Logging Hierarchy Pattern

**🎯 PURPOSE**: Keep service methods clean and readable by pushing logging responsibilities down to where operations actually occur, eliminating unnecessary if-statements that exist solely for logging.

## Problem Statement

Service methods often become cluttered with logging logic that obscures the business flow:

```csharp
// ❌ ANTI-PATTERN - Service cluttered with logging
private async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateEntityAsync(CreateWorkoutTemplateCommand command)
{
    var entityResult = WorkoutTemplate.Handler.CreateNew(...);

    // Unnecessary if-statement just for logging!
    if (!entityResult.IsSuccess)
    {
        _logger.LogError("Entity creation failed: {Errors}", string.Join(", ", entityResult.Errors));
        return ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)));
    }
    
    var result = await _commandDataService.CreateAsync(entityResult.Value);
    
    // Another if-statement just for logging!
    if (result.IsSuccess)
    {
        _logger.LogInformation("Created workout template {TemplateId}", result.Data.Id);
    }
    
    return result;
}
```

### Issues with This Approach:
1. **Verbose**: Multiple if-statements that exist only for logging
2. **Mixed Concerns**: Service orchestration mixed with logging
3. **Poor Readability**: Business logic obscured by infrastructure concerns
4. **Violation of Single Responsibility**: Service doing both orchestration AND logging
5. **Harder Testing**: Tests need to verify logging behavior

## Solution: Push Logging Down the Hierarchy

Move logging to where the actual operations occur, keeping service methods clean:

```csharp
// ✅ CORRECT - Clean service with pattern matching
private async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateEntityAsync(CreateWorkoutTemplateCommand command)
{
    var draftStateResult = await _workoutStateService.GetByValueAsync(WorkoutStateConstants.Draft);
    var defaultWorkoutStateId = WorkoutStateId.ParseOrEmpty(draftStateResult.Data.Id);
    
    var entityResult = WorkoutTemplate.Handler.CreateNew(
        command.Name,
        command.Description,
        command.CategoryId,
        command.DifficultyId,
        command.EstimatedDurationMinutes,
        command.Tags,
        command.IsPublic,
        defaultWorkoutStateId);

    return entityResult.IsSuccess switch
    {
        true => await _commandDataService.CreateAsync(entityResult.Value),
        false => ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)))
    };
}
```

## The Logging Hierarchy Principle

### Where Logging Should Occur

```
Service Layer (Orchestration)
    ↓ No logging - just orchestrates
DataService Layer (Data Operations)
    ↓ Logs data operations (CRUD success/failure)
Repository Layer (Database Access)
    ↓ Logs SQL/database issues
Entity Handlers (Business Rules)
    ↓ Logs validation/business rule violations
Infrastructure (Cross-cutting)
    ↓ Logs technical issues
```

### Logging Responsibility by Layer

#### 1. **Service Layer** - NO LOGGING
- Purpose: Orchestration only
- Focus: Business flow clarity
- Pattern: Use pattern matching for control flow

#### 2. **DataService Layer** - OPERATION LOGGING
```csharp
public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(WorkoutTemplate entity)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    
    var createdEntity = await repository.CreateAsync(entity);
    await unitOfWork.CommitAsync();
    
    var dto = createdEntity.ToDto();
    
    // Log the successful operation HERE
    _logger.LogInformation("Created workout template {TemplateId} with name '{Name}'", 
        dto.Id, dto.Name);
    
    return ServiceResult<WorkoutTemplateDto>.Success(dto);
}
```

#### 3. **Entity Handlers** - VALIDATION LOGGING
```csharp
public static EntityResult<WorkoutTemplate> CreateNew(...)
{
    var validationErrors = new List<string>();
    
    if (string.IsNullOrWhiteSpace(name))
        validationErrors.Add("Name is required");
    
    if (validationErrors.Any())
    {
        // Log validation failures HERE
        _logger?.LogWarning("WorkoutTemplate creation validation failed: {Errors}", 
            string.Join(", ", validationErrors));
        return EntityResult<WorkoutTemplate>.Failure(validationErrors);
    }
    
    return EntityResult<WorkoutTemplate>.Success(new WorkoutTemplate { ... });
}
```

#### 4. **Repository Layer** - DATABASE LOGGING
```csharp
public async Task<WorkoutTemplate> CreateAsync(WorkoutTemplate entity)
{
    try
    {
        Context.WorkoutTemplates.Add(entity);
        await Context.SaveChangesAsync();
        
        _logger.LogDebug("Persisted WorkoutTemplate to database with ID {Id}", entity.Id);
        return entity;
    }
    catch (DbException ex)
    {
        _logger.LogError(ex, "Database error creating WorkoutTemplate");
        throw;
    }
}
```

## Pattern Recognition

### Signs You Need This Pattern

Look for these code smells:

1. **If-statements that only contain logging**
```csharp
// ❌ SMELL - If-statement exists only for logging
if (result.IsSuccess)
{
    _logger.LogInformation("Operation succeeded");
}
return result;
```

2. **Duplicate error information**
```csharp
// ❌ SMELL - Logging the same error that's already in the result
if (!entityResult.IsSuccess)
{
    _logger.LogError("Failed: {Errors}", entityResult.Errors);
    return ServiceResult.Failure(entityResult.Errors);
}
```

3. **Multiple exit points due to logging**
```csharp
// ❌ SMELL - Multiple returns just to log different cases
if (!result.IsSuccess)
{
    _logger.LogError("Failed");
    return result;
}

_logger.LogInformation("Succeeded");
return result;
```

## Refactoring Guidelines

### Step 1: Identify Logging-Only If-Statements
Look for if-statements where removing the logging would eliminate the need for the if.

### Step 2: Determine Proper Logging Location
- **Creation/Update/Delete success** → DataService
- **Validation failures** → Entity Handler or ServiceValidate
- **Database errors** → Repository
- **External service calls** → Integration layer

### Step 3: Refactor to Pattern Matching
Replace if-statements with switch expressions or ternary operators:

```csharp
// Before - Verbose with logging
if (!entityResult.IsSuccess)
{
    _logger.LogError("Failed: {Errors}", entityResult.Errors);
    return ServiceResult.Failure(entityResult.Errors);
}

var result = await _dataService.CreateAsync(entityResult.Value);

if (result.IsSuccess)
{
    _logger.LogInformation("Created {Id}", result.Data.Id);
}

return result;

// After - Clean pattern matching
return entityResult.IsSuccess switch
{
    true => await _dataService.CreateAsync(entityResult.Value),
    false => ServiceResult.Failure(entityResult.Errors)
};
```

### Step 4: Add Logging at Appropriate Layer
Move the logging to where the operation actually happens.

## Benefits

### 1. **Cleaner Services**
- Services focus on orchestration
- Business logic is immediately visible
- Less code to maintain

### 2. **Better Separation of Concerns**
- Each layer has clear responsibilities
- Logging happens where context is richest
- Easier to change logging strategy

### 3. **Improved Testability**
- Services can be tested without mocking loggers
- Tests focus on business behavior
- Logging can be tested separately

### 4. **Enhanced Readability**
- Pattern matching shows all paths clearly
- No infrastructure concerns in business logic
- Code reads like specification

## Real-World Example

### Before (20 lines with logging)
```csharp
private async Task<ServiceResult<ExerciseDto>> CreateExerciseEntityAsync(CreateExerciseCommand command)
{
    var entityResult = Exercise.Handler.CreateNew(
        command.Name,
        command.Description,
        command.DifficultyId,
        command.ExerciseTypeIds);

    if (!entityResult.IsSuccess)
    {
        _logger.LogError("Exercise entity creation failed: {Errors}", 
            string.Join(", ", entityResult.Errors));
        return ServiceResult<ExerciseDto>.Failure(
            ExerciseDto.Empty,
            ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)));
    }
    
    var result = await _commandDataService.CreateAsync(entityResult.Value);
    
    if (result.IsSuccess)
    {
        _logger.LogInformation("Created exercise {ExerciseId} with name '{Name}'", 
            result.Data.Id, result.Data.Name);
    }
    else
    {
        _logger.LogError("Failed to persist exercise: {Error}", result.Error);
    }
    
    return result;
}
```

### After (8 lines, clean and focused)
```csharp
private async Task<ServiceResult<ExerciseDto>> CreateExerciseEntityAsync(CreateExerciseCommand command)
{
    var entityResult = Exercise.Handler.CreateNew(
        command.Name,
        command.Description,
        command.DifficultyId,
        command.ExerciseTypeIds);

    return entityResult.IsSuccess switch
    {
        true => await _commandDataService.CreateAsync(entityResult.Value),
        false => ServiceResult<ExerciseDto>.Failure(
            ExerciseDto.Empty,
            ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)))
    };
}
```

## Testing Considerations

### Service Tests (Simplified)
```csharp
[Fact]
public async Task CreateAsync_WithValidData_ReturnsSuccess()
{
    // Arrange - No logger mocking needed!
    var command = new CreateWorkoutTemplateCommand { ... };
    
    // Act
    var result = await service.CreateAsync(command);
    
    // Assert - Focus on business behavior
    result.IsSuccess.Should().BeTrue();
    result.Data.Name.Should().Be(command.Name);
}
```

### DataService Tests (With Logging)
```csharp
[Fact]
public async Task CreateAsync_Success_LogsInformation()
{
    // Arrange
    var entity = WorkoutTemplate.Empty;
    
    // Act
    var result = await dataService.CreateAsync(entity);
    
    // Assert - Verify logging at appropriate layer
    logger.Received(1).LogInformation(
        Arg.Is<string>(s => s.Contains("Created workout template")),
        Arg.Any<object[]>());
}
```

## Summary

The Logging Hierarchy Pattern ensures:
- **Services remain clean** and focused on orchestration
- **Logging occurs where context is richest**
- **Pattern matching** replaces verbose if-statements
- **Single responsibility** is maintained at each layer
- **Tests are simpler** and more focused

By pushing logging down to where operations occur, we achieve cleaner, more maintainable code that clearly expresses business intent without infrastructure clutter.

## Quick Checklist

When reviewing service methods:
- [ ] Are there if-statements that exist only for logging?
- [ ] Can logging be moved to DataService/Repository/Handler?
- [ ] Can if-statements be replaced with pattern matching?
- [ ] Is the service method focused on orchestration only?
- [ ] Are infrastructure concerns separated from business logic?

Remember: **"If it's just for logging, it doesn't belong in the service!"**