# ServiceResult Pattern Documentation

## 🎯 **Overview**

The ServiceResult pattern provides a consistent, exception-free way to handle operation results in the service layer. It replaces exception-based control flow with a type-safe result object that can represent both success and failure states.

## 🚨 **Problem: Exception-Based Control Flow**

### **Traditional Approach (Anti-Pattern)**
```csharp
// ❌ BAD: Using exceptions for business logic
public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
{
    // Validation throws exception
    if (string.IsNullOrEmpty(request.Name))
        throw new ArgumentException("Name is required");
    
    // Business rule throws exception
    if (await ExerciseExists(request.Name))
        throw new InvalidOperationException("Exercise already exists");
    
    // Repository might throw exception
    var exercise = await _repository.AddAsync(...);
    
    return MapToDto(exercise);
}

// Controller has to catch multiple exception types
try
{
    var result = await _service.CreateAsync(request);
    return CreatedAtAction(...);
}
catch (ArgumentException ex)
{
    return BadRequest(ex.Message);
}
catch (InvalidOperationException ex)
{
    return Conflict(ex.Message);
}
catch (Exception ex)
{
    return StatusCode(500, "Internal error");
}
```

### **Problems with Exception-Based Flow**
1. **Performance overhead** - Exceptions are expensive
2. **Unclear contracts** - What exceptions can be thrown?
3. **Complex error handling** - Multiple catch blocks
4. **Mixing concerns** - Business rules and exceptional cases
5. **Testing difficulty** - Must test exception scenarios

## ✅ **Solution: ServiceResult Pattern**

### **ServiceResult Implementation**
```csharp
namespace GetFitterGetBigger.API.Services.Results;

/// <summary>
/// Represents the result of a service operation including validation errors
/// </summary>
public record ServiceResult<T>
{
    /// <summary>
    /// The data returned by the operation (may be empty if validation failed)
    /// </summary>
    public required T Data { get; init; }
    
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// List of validation errors (empty if operation was successful)
    /// </summary>
    public List<string> Errors { get; init; } = new();
    
    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ServiceResult<T> Success(T data) => new()
    {
        Data = data,
        IsSuccess = true,
        Errors = new List<string>()
    };
    
    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, params string[] errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors.ToList()
    };
    
    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> Failure(T emptyData, List<string> errors) => new()
    {
        Data = emptyData,
        IsSuccess = false,
        Errors = errors
    };
}
```

### **ValidationResult Helper**
```csharp
/// <summary>
/// Represents a validation result
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    
    public static ValidationResult Success() => new()
    {
        IsValid = true,
        Errors = new List<string>()
    };
    
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}
```

## 📋 **Implementation Patterns**

### **1. Service Method Pattern**
```csharp
public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
{
    // Step 1: Validation
    var validationResult = await ValidateCreateCommand(command);
    if (!validationResult.IsValid)
    {
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, validationResult.Errors);
    }
    
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var repository = writableUow.GetRepository<IExerciseRepository>();
    
    // Step 2: Business rules
    if (await repository.ExistsAsync(command.Name, null))
    {
        return ServiceResult<ExerciseDto>.Failure(
            ExerciseDto.Empty, 
            $"Exercise with name '{command.Name}' already exists.");
    }
    
    // Step 3: Perform operation
    var exercise = Exercise.Handler.CreateNew(...);
    var created = await repository.AddAsync(exercise);
    await writableUow.CommitAsync();
    
    // Step 4: Return success
    return ServiceResult<ExerciseDto>.Success(MapToExerciseDto(created));
}
```

### **2. Validation Method Pattern**
```csharp
private async Task<ValidationResult> ValidateCreateCommand(CreateExerciseCommand command)
{
    var errors = new List<string>();
    
    // Required field validation
    if (string.IsNullOrWhiteSpace(command.Name))
        errors.Add("Exercise name is required");
        
    if (command.DifficultyId.IsEmpty)
        errors.Add("Difficulty level is required");
    
    // Complex business validation
    if (isRestExercise && !command.MuscleGroups.IsEmpty)
        errors.Add("REST exercises cannot have muscle groups");
    
    return errors.Any() 
        ? ValidationResult.Failure(errors.ToArray()) 
        : ValidationResult.Success();
}
```

### **3. Controller Pattern Matching**
```csharp
[HttpPost]
public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
{
    var result = await _exerciseService.CreateAsync(request.ToCommand());
    
    // Pattern matching for clean response handling
    return result switch
    {
        { IsSuccess: true } => 
            CreatedAtAction(nameof(GetExercise), new { id = result.Data.Id }, result.Data),
            
        { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => 
            Conflict(new { errors }),
            
        { Errors: var errors } => 
            BadRequest(new { errors })
    };
}
```

### **4. Update Method Pattern**
```csharp
public async Task<ServiceResult<ExerciseDto>> UpdateAsync(ExerciseId id, UpdateExerciseCommand command)
{
    // Validate ID
    if (id.IsEmpty)
    {
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, "Invalid exercise ID");
    }
    
    // Validate command
    var validationResult = await ValidateUpdateCommand(command);
    if (!validationResult.IsValid)
    {
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, validationResult.Errors);
    }
    
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var repository = writableUow.GetRepository<IExerciseRepository>();
    
    // Check existence
    var exercise = await repository.GetByIdAsync(id);
    if (exercise.IsEmpty)
    {
        return ServiceResult<ExerciseDto>.Failure(ExerciseDto.Empty, "Exercise not found");
    }
    
    // Perform update
    var updated = exercise with { /* updates */ };
    await repository.UpdateAsync(updated);
    await writableUow.CommitAsync();
    
    return ServiceResult<ExerciseDto>.Success(MapToExerciseDto(updated));
}
```

### **5. Delete Method Pattern**
```csharp
public async Task<ServiceResult<bool>> DeleteAsync(ExerciseId id)
{
    if (id.IsEmpty)
    {
        return ServiceResult<bool>.Failure(false, "Invalid exercise ID");
    }
    
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var repository = writableUow.GetRepository<IExerciseRepository>();
    
    var exercise = await repository.GetByIdAsync(id);
    if (exercise.IsEmpty)
    {
        return ServiceResult<bool>.Failure(false, "Exercise not found");
    }
    
    // Soft delete
    var softDeleted = exercise with { IsActive = false };
    await repository.UpdateAsync(softDeleted);
    await writableUow.CommitAsync();
    
    return ServiceResult<bool>.Success(true);
}
```

## 🎯 **Advanced Patterns**

### **1. Error Codes Instead of String Matching**
```csharp
public record ServiceResult<T>
{
    public List<ServiceError> Errors { get; init; } = new();
}

public record ServiceError(string Code, string Message);

// Usage
return ServiceResult<ExerciseDto>.Failure(
    ExerciseDto.Empty, 
    new ServiceError("EXERCISE_EXISTS", $"Exercise '{name}' already exists"));

// Controller
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { Errors: var errors } when errors.Any(e => e.Code == "EXERCISE_EXISTS") => Conflict(errors),
    { Errors: var errors } => BadRequest(errors)
};
```

### **2. Generic Result Extensions**
```csharp
public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this ServiceResult<T> result,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Data);
            
        return new BadRequestObjectResult(new { result.Errors });
    }
}

// Usage
return await _service.CreateAsync(command)
    .ToActionResult(data => CreatedAtAction(nameof(Get), new { id = data.Id }, data));
```

### **3. Async Result Chaining**
```csharp
public static class ServiceResultExtensions
{
    public static async Task<ServiceResult<TOut>> ThenAsync<TIn, TOut>(
        this Task<ServiceResult<TIn>> resultTask,
        Func<TIn, Task<ServiceResult<TOut>>> next)
    {
        var result = await resultTask;
        if (!result.IsSuccess)
        {
            return ServiceResult<TOut>.Failure(default!, result.Errors);
        }
        
        return await next(result.Data);
    }
}
```

## 📋 **Testing ServiceResult**

### **Success Case Testing**
```csharp
[Fact]
public async Task CreateAsync_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new CreateExerciseCommand { /* valid data */ };
    
    // Act
    var result = await _service.CreateAsync(command);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Errors);
    Assert.Equal("Exercise Name", result.Data.Name);
}
```

### **Failure Case Testing**
```csharp
[Fact]
public async Task CreateAsync_DuplicateName_ReturnsFailure()
{
    // Arrange
    _repository.Setup(r => r.ExistsAsync(It.IsAny<string>(), null))
        .ReturnsAsync(true);
    
    // Act
    var result = await _service.CreateAsync(command);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ExerciseDto.Empty, result.Data);
    Assert.Contains("already exists", result.Errors.First());
}
```

### **Multiple Errors Testing**
```csharp
[Fact]
public async Task CreateAsync_MultipleValidationErrors_ReturnsAllErrors()
{
    // Arrange
    var command = new CreateExerciseCommand 
    { 
        Name = "", // Empty name
        DifficultyId = DifficultyLevelId.Empty // Empty difficulty
    };
    
    // Act
    var result = await _service.CreateAsync(command);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(2, result.Errors.Count);
    Assert.Contains("name is required", result.Errors, StringComparer.OrdinalIgnoreCase);
    Assert.Contains("difficulty level is required", result.Errors, StringComparer.OrdinalIgnoreCase);
}
```

## ✅ **Benefits**

1. **No Exception Overhead**: Better performance, especially for validation
2. **Clear Contracts**: Return type explicitly shows success/failure possibility
3. **Type Safety**: Compile-time checking of result handling
4. **Testability**: Easy to test both success and failure paths
5. **Consistency**: Same pattern for all service operations
6. **Rich Error Information**: Can return multiple errors with context

## ❌ **Anti-Patterns to Avoid**

1. **Throwing Exceptions in ServiceResult Methods**
   ```csharp
   // ❌ NEVER DO THIS
   public async Task<ServiceResult<T>> CreateAsync(...)
   {
       if (!valid)
           throw new ArgumentException(); // Defeats the purpose!
   }
   ```

2. **Ignoring IsSuccess Check**
   ```csharp
   // ❌ NEVER DO THIS
   var result = await service.CreateAsync(...);
   return Ok(result.Data); // What if it failed?
   ```

3. **Returning Null Data on Success**
   ```csharp
   // ❌ NEVER DO THIS
   return ServiceResult<ExerciseDto>.Success(null); // Data should never be null
   ```

4. **Empty Errors on Failure**
   ```csharp
   // ❌ NEVER DO THIS
   return ServiceResult<T>.Failure(emptyData); // Always provide error reason
   ```

## 📚 **Related Patterns**

- **Empty Object Pattern**: Use `ExerciseDto.Empty` instead of null
- **Command Pattern**: ServiceResult works well with command objects
- **Pattern Matching**: Modern C# pattern matching for result handling
- **Validation Pattern**: Separate validation logic with ValidationResult

---

**Remember**: ServiceResult eliminates exceptions from normal control flow, making your code more predictable, testable, and performant. Always check `IsSuccess` before using `Data`, and always provide meaningful error messages on failure.