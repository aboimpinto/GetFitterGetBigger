# ServiceValidate Pattern - Fluent Validation API

**🎯 PURPOSE**: Comprehensive guide for using the ServiceValidate fluent API for all validation logic in the GetFitterGetBigger API.

## Overview

ServiceValidate is a **MANDATORY** fluent validation API that replaces all manual validation logic. It provides:
- Single exit points
- Consistent error handling
- Composable validation chains
- Mix of sync/async operations
- Integration with ServiceResult pattern

## Basic Usage

### ❌ BAD - Manual Validation Patterns to Avoid

```csharp
// Using string.IsNullOrWhiteSpace with switch expressions
public async Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name)
{
    var result = string.IsNullOrWhiteSpace(name) switch
    {
        true => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, "Name cannot be empty"),
        false => await ProcessGetByNameAsync(name)
    };
    return result;
}

// Manual validation with multiple returns
protected override async Task<ValidationResult> ValidateCreateCommand(CreateEquipmentCommand command)
{
    var errors = new List<string>();
    
    if (command == null)
        errors.Add("Command cannot be null");
    
    if (string.IsNullOrWhiteSpace(command?.Name))
        errors.Add("Name cannot be empty");
        
    if (command?.Name?.Length > 100)
        errors.Add("Name too long");
    
    if (await CheckDuplicateNameAsync(command.Name))
        errors.Add("Name already exists");
    
    return new ValidationResult(errors);
}
```

### ✅ GOOD - ServiceValidate Patterns

```csharp
// Using ServiceValidate for parameter validation
public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
{
    return await ServiceValidate.For<BodyPartDto>()
        .EnsureNotWhiteSpace(value, BodyPartErrorMessages.ValueCannotBeEmpty)
        .MatchAsync(
            whenValid: async () => await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value)
        );
}

// Fluent validation with chaining
protected override async Task<ValidationResult> ValidateCreateCommand(CreateEquipmentCommand command)
{
    return await ServiceValidate.Build()
        .EnsureNotNull(command, EquipmentErrorMessages.Validation.RequestCannotBeNull)
        .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
        .Ensure(() => command?.Name?.Length <= 100, EquipmentErrorMessages.Validation.NameTooLong)
        .EnsureAsync(
            async () => command == null || !await CheckDuplicateNameAsync(command.Name),
            ServiceError.AlreadyExists("Equipment", command?.Name ?? string.Empty))
        .ToValidationResultAsync();
}
```

## Advanced Features

### Mixed Sync/Async Validation Chains

```csharp
// Mix sync and async validations in one chain
public async Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command)
{
    return await ServiceValidate.For<AuthenticationResponse>()
        .EnsureNotNull(command, AuthenticationErrorMessages.Validation.RequestCannotBeNull)  // Sync
        .EnsureNotWhiteSpace(command?.Email, AuthenticationErrorMessages.Validation.EmailCannotBeEmpty)  // Sync
        .AsAsync()  // Bridge to async operations when needed
        .EnsureServiceResultAsync(() => ValidateUserAsync(command.Email))  // Async
        .ThenMatchAsync(
            whenValid: async () => await ProcessAuthenticationAsync(command!)
        );
}
```

### ServiceResult Integration with EnsureServiceResultAsync

```csharp
// Chain ServiceResult validations
private async Task<ServiceResult<AuthenticationResponse>> HandleNewUserAsync(string email)
{
    return await ServiceValidate.For<AuthenticationResponse>()
        .EnsureServiceResultAsync(() => CreateNewUserAccountAsync(email))  // Validates ServiceResult<bool>
        .ThenMatchAsync(
            whenValid: async () =>
            {
                var userResult = await LoadUserByEmailAsync(email);
                return userResult.IsSuccess 
                    ? GenerateAuthTokenAsync(userResult.Data)
                    : ServiceResult<AuthenticationResponse>.Failure(
                        AuthenticationResponse.Empty,
                        userResult.Errors);
            });
}
```

### ThenMatchAsync for Fluent Chain Continuation

```csharp
// Complete fluent chain without intermediate variables
public async Task<ServiceResult<WorkoutTemplateDto>> CreateTemplateAsync(CreateTemplateCommand command)
{
    return await ServiceValidate.For<WorkoutTemplateDto>()
        .EnsureNotNull(command, "Command cannot be null")
        .EnsureNotWhiteSpace(command?.Name, "Name is required")
        .Ensure(() => command?.Name?.Length <= 100, "Name too long")
        .AsAsync()  // Bridge to async when mixing sync/async
        .EnsureServiceResultAsync(() => ValidateCategoryAsync(command.CategoryId))
        .EnsureServiceResultAsync(() => ValidateExercisesAsync(command.ExerciseIds))
        .ThenMatchAsync(
            whenValid: async () => await CreateAndSaveTemplateAsync(command)
        );
}
```

### WithServiceResultAsync and ThenMatchDataAsync - Advanced Data Flow

```csharp
// Load data and branch on Empty pattern
public async Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command)
{
    return await ServiceValidate.For<AuthenticationResponse>()
        .WithServiceResultAsync(() => LoadUserByEmailAsync(command.Email))  // Returns ServiceResult<UserDto>
        .ThenMatchDataAsync<AuthenticationResponse, UserDto>(
            whenEmpty: async () => await HandleNewUserAsync(command.Email),  // UserDto.IsEmpty == true
            whenNotEmpty: userData => Task.FromResult(GenerateAuthTokenAsync(userData))  // Process existing user
        );
}

// Chain multiple data loads with Empty pattern branching
public async Task<ServiceResult<WorkoutSummaryDto>> GetWorkoutWithDetailsAsync(WorkoutId id)
{
    return await ServiceValidate.For<WorkoutSummaryDto>()
        .WithServiceResultAsync(() => LoadWorkoutAsync(id))
        .ThenMatchDataAsync<WorkoutSummaryDto, WorkoutDto>(
            whenEmpty: () => Task.FromResult(
                ServiceResult<WorkoutSummaryDto>.Failure(
                    WorkoutSummaryDto.Empty, 
                    ServiceError.NotFound("Workout", id))),
            whenNotEmpty: async workout => await EnrichWorkoutWithDetailsAsync(workout)
        );
}
```

## Flow Patterns - Complete Architecture

### Core Flow Scenarios

```csharp
// Scenario 1: No Pre-validation → Execution → No Post-validation
public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
{
    return await ServiceValidate.Build<IEnumerable<BodyPartDto>>()
        .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
}

// Scenario 2: With Pre-validation → Execution → No Post-validation
public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
{
    return await ServiceValidate.For<BodyPartDto>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)  // Pre-validation
        .MatchAsync(
            whenValid: async () => await LoadByIdFromDatabaseAsync(id)  // Execution
        );
}

// Scenario 3: With Pre-validation → Execution → With Post-validation
public async Task<ServiceResult<WorkoutTemplateDto>> CreateTemplateAsync(CreateTemplateCommand command)
{
    return await ServiceValidate.For<WorkoutTemplateDto>()
        .EnsureNotNull(command, "Command cannot be null")  // Pre-validation
        .EnsureNotWhiteSpace(command?.Name, "Name is required")
        .AsAsync()
        .EnsureServiceResultAsync(() => ValidateCategoryAsync(command.CategoryId))
        .WithServiceResultAsync(() => CreateTemplateInDatabaseAsync(command))  // Execution
        .ThenMatchDataAsync<WorkoutTemplateDto, WorkoutTemplateDto>(
            whenEmpty: () => Task.FromResult(
                ServiceResult<WorkoutTemplateDto>.Failure(
                    WorkoutTemplateDto.Empty, 
                    ServiceError.InternalError("Template creation failed"))),
            whenNotEmpty: async template => await ValidateTemplateStateAsync(template)  // Post-validation
        );
}

// Scenario 4: No Pre-validation → Execution → With Post-validation
public async Task<ServiceResult<SystemHealthDto>> CheckSystemHealthAsync()
{
    return await ServiceValidate.For<SystemHealthDto>()
        .WithServiceResultAsync(() => LoadSystemMetricsAsync())  // Execution
        .ThenMatchDataAsync<SystemHealthDto, SystemHealthDto>(
            whenEmpty: () => Task.FromResult(
                ServiceResult<SystemHealthDto>.Failure(
                    SystemHealthDto.Empty, 
                    ServiceError.ServiceUnavailable())),
            whenNotEmpty: async metrics => await ValidateHealthThresholdsAsync(metrics)  // Post-validation
        );
}
```

## Critical Validation Pattern Rules 🚨

### Use Error Message Strings Directly

```csharp
// ✅ CORRECT
.EnsureNotNull(command, ErrorMessages.RequestCannotBeNull)

// ❌ WRONG - Never wrap in ServiceError.ValidationFailed()
.EnsureNotNull(command, ServiceError.ValidationFailed(...))
```

ServiceValidate methods handle ServiceError creation internally.

### Atomic Validation Rule - One Validation Per Aspect

```csharp
// ❌ BAD - Complex conditional mixing multiple validations
.EnsureAsync(
    async () => command == null || command.BodyPartId.IsEmpty ||
               (await _bodyPartService.ExistsAsync(command.BodyPartId)).Data,
    ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidBodyPartId))

// ✅ GOOD - Each validation validates one specific aspect
.EnsureNotNull(command, MuscleGroupErrorMessages.Validation.InvalidCommand)
.EnsureNotEmpty(command.BodyPartId, MuscleGroupErrorMessages.Validation.InvalidBodyPartId)  
.EnsureAsync(
    async () => (await _bodyPartService.ExistsAsync(command.BodyPartId)).Data,
    MuscleGroupErrorMessages.Validation.BodyPartDoesNotExist)
```

**Benefits of Atomic Validations:**
- **Clarity**: Each line has a single, clear purpose
- **Specific Errors**: Users get precise error messages about what failed
- **Maintainability**: Easy to add, remove, or modify individual validations
- **Testability**: Each validation can be tested independently
- **Readability**: No mental parsing of complex boolean logic

## MatchAsync vs WhenValidAsync - Important Distinction

```csharp
// ⚠️ DEPRECATED - WhenValidAsync uses default(T) which violates Empty pattern
public async Task<ServiceResult<bool>> ExistsAsync(BodyPartId id)
{
    return await ServiceValidate.Build<bool>()
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
        .WhenValidAsync(async () =>  // Uses default(bool) = false on failure
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IBodyPartRepository>();
            var exists = await repository.ExistsAsync(id);
            return ServiceResult<bool>.Success(exists);
        });
}

// ✅ PREFERRED - Use MatchAsync for consistency with Empty pattern
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
{
    return await ServiceValidate.For<BooleanResultDto>()  // Note: For<T> not Build<T>
        .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
        .MatchAsync(  // Uses BooleanResultDto.Empty on failure
            whenValid: async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IBodyPartRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = exists });
            });
}
```

## Key Extension Methods Reference

- `.AsAsync()` - Converts sync validation chain to async for continuation
- `.EnsureServiceResultAsync()` - Validates a ServiceResult and adds errors to chain
- `.ThenMatchAsync()` - Continues chain from async operations without breaking fluency
- `.WithServiceResultAsync()` - Loads data via ServiceResult and prepares for pattern matching
- `.ThenMatchDataAsync<TResult, TData>()` - Pattern matches on loaded data's Empty state
- `.MatchAsync()` ✅ PREFERRED - For executing logic after validation when T implements IEmptyDto
- `.WhenValidAsync()` ⚠️ DEPRECATED - Legacy method that uses default(T) instead of Empty pattern

## When to Use Each Pattern

- **`EnsureServiceResultAsync`** - When you need to validate that an operation succeeded before continuing
- **`ThenMatchAsync`** - When validation is complete and you need to execute the main logic
- **`WithServiceResultAsync` + `ThenMatchDataAsync`** - When you need to load data and branch based on whether it's Empty
- **`AsAsync`** - When transitioning from synchronous to asynchronous validation chains

## Migration Strategy

1. Replace `ServiceValidate.Build<T>()` with `ServiceValidate.For<T>()` where T implements IEmptyDto
2. Replace `.WhenValidAsync()` with `.MatchAsync()`
3. For primitive types (bool, int, etc.), wrap in appropriate DTOs that implement IEmptyDto
4. This ensures consistent Empty pattern usage throughout the codebase

## Special Cases

- For collections (`IEnumerable<T>`) that don't implement IEmptyDto, use `ServiceValidate.Build<IEnumerable<T>>().WhenValidAsync()`
- This is acceptable because collections have a natural empty state (empty list) that doesn't require the Empty pattern

## Future Pattern - Multiple Chained Executions

**Vision**: Enable multiple execution stages where each stage receives the results of previous stages, maintaining the ServiceResult chain throughout.

```csharp
// FUTURE PATTERN - Not yet implemented
// Scenario: No Pre-validation → Execution 1 → Execution 2 → Execution 3 → With Post-validation
public async Task<ServiceResult<WorkoutSessionDto>> StartWorkoutSessionAsync(StartSessionCommand command)
{
    return await ServiceValidate.For<WorkoutSessionDto>()
        // Execution 1: Create session
        .ExecuteAsync(async () => await CreateSessionInDatabaseAsync(command))
        // Execution 2: Receives result from Execution 1
        .ThenExecuteAsync<SessionDto>(async (session) => 
            await LoadExercisesForSessionAsync(session.TemplateId))
        // Execution 3: Receives results from Execution 1 & 2
        .ThenExecuteAsync<SessionDto, ExerciseListDto>(async (session, exercises) => 
            await InitializeTrackingAsync(session.Id, exercises))
        // Post-validation: Receives all previous results
        .ThenValidateAsync<SessionDto, ExerciseListDto, TrackingDto>(
            async (session, exercises, tracking) => 
                await ValidateSessionReadyAsync(session, exercises, tracking))
        // Final transformation
        .ThenMapAsync<SessionDto, ExerciseListDto, TrackingDto, WorkoutSessionDto>(
            (session, exercises, tracking) => new WorkoutSessionDto
            {
                Session = session,
                Exercises = exercises,
                Tracking = tracking
            });
}
```

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/EmptyObjectPattern.md` - Empty pattern integration
- `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - ServiceResult usage