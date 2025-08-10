# ServiceResult Pattern - Consistent Operation Outcomes

**🎯 PURPOSE**: This document defines the **MANDATORY** ServiceResult pattern for all service methods in the GetFitterGetBigger API, ensuring consistent error handling and operation outcomes.

## Overview

`ServiceResult<T>` is the standard return type for ALL service methods. It provides:
- Consistent success/failure semantics
- Structured error information
- Integration with Empty pattern
- Clean controller mappings

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: ServiceResult Rules - MUST be followed         │
├──────────────────────────────────────────────────────────────┤
│ 1. ALL service methods return ServiceResult<T>              │
│ 2. NEVER throw exceptions for business logic                │
│ 3. Use ServiceError for structured error information        │
│ 4. Always provide Empty object on failure                   │
│ 5. Test error codes, not error messages                     │
└──────────────────────────────────────────────────────────────┘
```

## Basic Usage

### ❌ BAD - Throwing Exceptions or Raw Returns

```csharp
// Throwing exceptions for business logic
public async Task<EquipmentDto> CreateAsync(CreateEquipmentCommand command)
{
    if (!IsValid(command))
        throw new ValidationException("Invalid command");
    
    var entity = await _repository.CreateAsync(command.ToEntity());
    return entity.ToDto();
}

// Returning null on failure
public async Task<EquipmentDto?> GetByIdAsync(EquipmentId id)
{
    var entity = await _repository.GetByIdAsync(id);
    return entity?.ToDto();
}
```

### ✅ GOOD - ServiceResult Pattern

```csharp
// Using ServiceResult for all outcomes
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    var validation = await ValidateCreateCommand(command);
    if (!validation.IsValid)
        return ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty, 
            validation.ServiceError ?? ServiceError.ValidationFailed(validation.Errors));
    
    var entity = await _repository.CreateAsync(command.ToEntity());
    return ServiceResult<EquipmentDto>.Success(entity.ToDto());
}

// Consistent failure handling with Empty pattern
public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
{
    var entity = await _repository.GetByIdAsync(id);
    
    return entity.IsEmpty
        ? ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty, 
            ServiceError.NotFound("Equipment", id.ToString()))
        : ServiceResult<EquipmentDto>.Success(entity.ToDto());
}
```

## ServiceResult Structure

```csharp
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public IReadOnlyList<string> Errors { get; }
    public ServiceErrorCode PrimaryErrorCode { get; }
    public IReadOnlyList<StructuredError> StructuredErrors { get; }
    
    // Factory methods
    public static ServiceResult<T> Success(T data);
    public static ServiceResult<T> Failure(T emptyData, ServiceError error);
    public static ServiceResult<T> Failure(T emptyData, params string[] errors);
}
```

## ServiceError Types

### Standard Error Codes

```csharp
public enum ServiceErrorCode
{
    None = 0,
    ValidationFailed = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    InternalError = 6,
    InvalidFormat = 7,
    InvalidReference = 8,
    AlreadyExists = 9,
    ServiceUnavailable = 10
}
```

### Creating ServiceErrors

```csharp
// Validation errors
ServiceError.ValidationFailed("Name cannot be empty")
ServiceError.ValidationFailed(validationErrors)

// Not found errors
ServiceError.NotFound("Equipment", id.ToString())
ServiceError.NotFound("Workout template not found")

// Conflict errors
ServiceError.AlreadyExists("Equipment", name)
ServiceError.Conflict("Resource is in use")

// Invalid reference errors
ServiceError.InvalidReference("CategoryId", categoryId.ToString())

// Internal errors
ServiceError.InternalError("Unexpected error occurred")

// Service unavailable
ServiceError.ServiceUnavailable("External service is down")
```

## Integration with ServiceValidate

ServiceValidate automatically creates ServiceResult with appropriate errors:

```csharp
public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
{
    return await ServiceValidate.For<BodyPartDto>()
        .EnsureNotWhiteSpace(value, BodyPartErrorMessages.ValueCannotBeEmpty)
        .MatchAsync(
            whenValid: async () => await LoadByValueFromDatabaseAsync(value)
        );
}
```

### Important: Error Message Strings vs ServiceError

When using ServiceValidate methods:
- ✅ **CORRECT**: Pass error message strings directly
- ❌ **WRONG**: Never wrap in ServiceError.ValidationFailed()

```csharp
// ✅ CORRECT
.EnsureNotNull(command, ErrorMessages.RequestCannotBeNull)

// ❌ WRONG - ServiceValidate handles ServiceError creation internally
.EnsureNotNull(command, ServiceError.ValidationFailed(ErrorMessages.RequestCannotBeNull))
```

## Pattern Matching in Controllers

Controllers use pattern matching to map ServiceResult to HTTP responses:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id) =>
    await _service.GetByIdAsync(EquipmentId.ParseOrEmpty(id)) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
        { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(new { errors = result.Errors }),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

## Empty Pattern Integration

Always provide Empty object on failure:

```csharp
// ✅ CORRECT - Always provide Empty on failure
return ServiceResult<EquipmentDto>.Failure(
    EquipmentDto.Empty,  // Never null!
    ServiceError.NotFound("Equipment"));

// ❌ WRONG - Null on failure
return ServiceResult<EquipmentDto>.Failure(
    null,  // VIOLATION!
    ServiceError.NotFound("Equipment"));
```

## Collections and ServiceResult

For collections, return empty collection on success with no items:

```csharp
public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync()
{
    var entities = await repository.GetAllAsync();
    
    // Empty collection is still a success
    if (!entities.Any())
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(new List<EquipmentDto>());
    
    var dtos = entities.Select(MapToDto).ToList();
    return ServiceResult<IEnumerable<EquipmentDto>>.Success(dtos);
}
```

## Boolean Results

Use BooleanResultDto for boolean operations:

```csharp
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id)
{
    var exists = await repository.ExistsAsync(id);
    return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
}

// Not this:
public async Task<ServiceResult<bool>> ExistsAsync(EquipmentId id)  // ❌ Primitive type
```

## Testing ServiceResult

### Test Error Codes, Not Messages

```csharp
// ✅ CORRECT - Test error codes
[Fact]
public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
{
    // Arrange
    var id = EquipmentId.New();
    _mockRepository.Setup(x => x.GetByIdAsync(id))
        .ReturnsAsync(Equipment.Empty);
    
    // Act
    var result = await _service.GetByIdAsync(id);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);  // Test code!
    Assert.True(result.Data.IsEmpty);
}

// ❌ WRONG - Testing error message content
Assert.Contains("not found", result.Errors.First());  // Brittle!
```

## Common Patterns

### Validation Then Execute

```csharp
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    // Validate first
    var validation = await ValidateCreateCommand(command);
    if (!validation.IsValid)
        return ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty,
            validation.ServiceError);
    
    // Then execute
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = Equipment.CreateNew(command.Name, command.Description);
    entity = await repository.CreateAsync(entity);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}
```

### Load and Check

```csharp
public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    
    // Check and return appropriate result
    return entity.IsEmpty
        ? ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty,
            ServiceError.NotFound("Equipment", id.ToString()))
        : ServiceResult<EquipmentDto>.Success(MapToDto(entity));
}
```

### Chaining Operations

```csharp
public async Task<ServiceResult<WorkoutSessionDto>> StartSessionAsync(StartSessionCommand command)
{
    // Validate references
    var templateResult = await _workoutTemplateService.GetByIdAsync(command.TemplateId);
    if (!templateResult.IsSuccess)
        return ServiceResult<WorkoutSessionDto>.Failure(
            WorkoutSessionDto.Empty,
            templateResult.Errors);
    
    var userResult = await _userService.GetByIdAsync(command.UserId);
    if (!userResult.IsSuccess)
        return ServiceResult<WorkoutSessionDto>.Failure(
            WorkoutSessionDto.Empty,
            userResult.Errors);
    
    // Create session
    var session = WorkoutSession.Create(templateResult.Data, userResult.Data);
    // ... save and return
}
```

## Migration Checklist

When adopting ServiceResult pattern:

- [ ] Change method signature to return `ServiceResult<T>`
- [ ] Replace exceptions with `ServiceResult.Failure()`
- [ ] Replace null returns with Empty pattern
- [ ] Add appropriate ServiceError codes
- [ ] Update controller to use pattern matching
- [ ] Update tests to check error codes, not messages
- [ ] Ensure Empty object is provided on failure

## Key Principles

1. **Predictable Outcomes**: Every service method has success or failure
2. **No Exceptions**: Business logic never throws
3. **Structured Errors**: Clear error codes and messages
4. **Empty Over Null**: Always return Empty objects on failure
5. **Testable**: Error codes are stable test points

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - ServiceValidate integration
- `/memory-bank/CodeQualityGuidelines/EmptyObjectPattern.md` - Empty pattern usage
- `/memory-bank/CodeQualityGuidelines/ControllerPatterns.md` - Controller handling