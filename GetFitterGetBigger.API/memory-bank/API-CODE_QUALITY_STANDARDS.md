# API-Specific Code Quality Standards - GetFitterGetBigger

**🎯 PURPOSE**: API-specific code quality standards that extend the universal standards for the GetFitterGetBigger API project. These standards are mandatory for all API implementations.

## 📋 Prerequisites

This document extends the universal `CODE_QUALITY_STANDARDS.md`. Read that first, then apply these API-specific standards.

---

## 🆕 Latest Pattern Updates (2025)

### **New Fluent APIs for Better Code Quality**
1. **ServiceValidate** - Fluent validation API replacing manual validation logic
2. **CacheLoad** - Fluent cache handling API enforcing single exit points
3. **IEmptyDto<T>** - Mandatory interface for all DTOs to support Empty pattern

These patterns are now **MANDATORY** for all new code and should be adopted when refactoring existing code.

### **No Try-Catch Anti-Pattern** 🚨 CRITICAL
**NEVER use blanket try-catch blocks!** This is a fundamental anti-pattern that violates our code quality standards.

```csharp
// ❌ ANTI-PATTERN - Blanket try-catch shows lack of control
private async Task<ServiceResult<UserDto>> LoadUserByEmailAsync(string email)
{
    try  // ← VIOLATION: We don't know what can fail!
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IUserRepository>();
        var entity = await repository.GetByEmailAsync(email);
        var dto = entity?.ToDto() ?? UserDto.Empty;
        return ServiceResult<UserDto>.Success(dto);
    }
    catch (Exception ex)  // ← VIOLATION: Catching everything masks real issues!
    {
        _logger.LogError(ex, "Error loading user");
        return ServiceResult<UserDto>.Failure(UserDto.Empty, ServiceError.InternalError());
    }
}

// ✅ CORRECT - No try-catch, explicit handling
private async Task<ServiceResult<UserDto>> LoadUserByEmailAsync(string email)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IUserRepository>();
    var entity = await repository.GetByEmailAsync(email);
    
    // Handle Empty pattern explicitly - no exceptions
    if (entity.IsEmpty || !entity.IsActive)
    {
        return ServiceResult<UserDto>.Success(UserDto.Empty);
    }
    
    var dto = MapToDto(entity);
    return ServiceResult<UserDto>.Success(dto);
}
```

**Why This Is Critical:**
- **Code Control**: We must know exactly where and why code can fail
- **Readability**: Try-catch disrupts normal flow and makes code harder to understand
- **Testability**: Blanket try-catch makes it impossible to test specific failure scenarios
- **Performance**: Exception handling is expensive - avoid it for normal control flow
- **Debugging**: Masks the real location and cause of failures

**The Right Approach:**
- Use ServiceValidate pattern for input validation
- Use Empty pattern for null/missing data
- Use ServiceResult for operation outcomes
- Only use try-catch for KNOWN external failures (e.g., network calls, file I/O)
- Write tests to verify failure scenarios

---

## 🚨 GOLDEN RULES FOR API - NON-NEGOTIABLE

```
┌─────────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: These API rules MUST be followed - NO EXCEPTIONS  │
├─────────────────────────────────────────────────────────────────┤
│ 1. Single Exit Point per method - USE PATTERN MATCHING         │
│ 2. ServiceResult<T> for ALL service methods                    │
│ 3. No null returns - USE EMPTY PATTERN                         │
│ 4. ReadOnlyUnitOfWork for queries, WritableUnitOfWork for mods │
│ 5. Pattern matching in controllers for ServiceResult handling  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 API-Specific Patterns

### 1. **Empty/Null Object Pattern (Mandatory)**
The API uses a strict Empty Object Pattern - **NEVER** return or handle nulls directly:

```csharp
// ❌ BAD - Returning null
public async Task<Equipment?> GetByIdAsync(EquipmentId id)
{
    var entity = await _repository.GetByIdAsync(id);
    return entity ?? null;
}

// ✅ GOOD - Returning Empty
public async Task<Equipment> GetByIdAsync(EquipmentId id)
{
    var entity = await _repository.GetByIdAsync(id);
    return entity ?? Equipment.Empty;
}
```

All entities must implement `IEmptyEntity<T>` with:
- Static `Empty` property
- `IsEmpty` property
- `Id` that equals `TId.Empty` when empty

### 2. **ServiceResult Pattern**
ALL service methods must return `ServiceResult<T>`:

```csharp
// ❌ BAD - Throwing exceptions or returning raw values
public async Task<EquipmentDto> CreateAsync(CreateEquipmentCommand command)
{
    if (!IsValid(command))
        throw new ValidationException("Invalid command");
    
    var entity = await _repository.CreateAsync(command.ToEntity());
    return entity.ToDto();
}

// ✅ GOOD - ServiceResult pattern with fluent validation
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    var validation = await ServiceValidate.Build()
        .EnsureNotNull(command, EquipmentErrorMessages.Validation.RequestCannotBeNull)
        .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
        .EnsureAsync(
            async () => command == null || !await CheckDuplicateNameAsync(command.Name),
            ServiceError.AlreadyExists("Equipment", command?.Name ?? string.Empty))
        .ToValidationResultAsync();
    
    if (!validation.IsValid)
        return ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty, 
            validation.ServiceError ?? ServiceError.ValidationFailed(validation.Errors));
    
    var entity = await _repository.CreateAsync(command.ToEntity());
    return ServiceResult<EquipmentDto>.Success(entity.ToDto());
}
```

### 2.1 **Fluent Validation API (ServiceValidate)** 🚨 NEW
Use the fluent `ServiceValidate` API for ALL validation logic - **NO EXCEPTIONS**:

```csharp
// ❌ BAD - Using string.IsNullOrWhiteSpace with switch expressions
public async Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name)
{
    var result = string.IsNullOrWhiteSpace(name) switch
    {
        true => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, "Name cannot be empty"),
        false => await ProcessGetByNameAsync(name)
    };
    return result;
}

// ❌ BAD - Manual validation with multiple returns
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

// ✅ GOOD - Using ServiceValidate for parameter validation
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

// ✅ GOOD - Fluent validation with chaining
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

#### **🚀 Advanced Features (2025 Update)**

##### **Mixed Sync/Async Validation Chains**
ServiceValidate now supports seamless mixing of synchronous and asynchronous validations:

```csharp
// ✅ POWERFUL - Mix sync and async validations in one chain
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

##### **ServiceResult Integration with EnsureServiceResultAsync**
Validate ServiceResult operations and integrate them into the validation chain:

```csharp
// ✅ ELEGANT - Chain ServiceResult validations
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

##### **ThenMatchAsync for Fluent Chain Continuation**
Continue validation chains without breaking the fluent pattern:

```csharp
// ✅ BEAUTIFUL - Complete fluent chain without intermediate variables
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

##### **WithServiceResultAsync and ThenMatchDataAsync - Advanced Data Flow Pattern**
Load data and branch logic based on Empty pattern without breaking fluency:

```csharp
// ✅ ELEGANT - Load data and branch on Empty pattern
public async Task<ServiceResult<AuthenticationResponse>> AuthenticateAsync(AuthenticationCommand command)
{
    return await ServiceValidate.For<AuthenticationResponse>()
        .WithServiceResultAsync(() => LoadUserByEmailAsync(command.Email))  // Returns ServiceResult<UserDto>
        .ThenMatchDataAsync<AuthenticationResponse, UserDto>(
            whenEmpty: async () => await HandleNewUserAsync(command.Email),  // UserDto.IsEmpty == true
            whenNotEmpty: userData => Task.FromResult(GenerateAuthTokenAsync(userData))  // Process existing user
        );
}

// ✅ POWERFUL - Chain multiple data loads with Empty pattern branching
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

**Key Extension Methods:**
- `.AsAsync()` - Converts sync validation chain to async for continuation
- `.EnsureServiceResultAsync()` - Validates a ServiceResult and adds errors to chain
- `.ThenMatchAsync()` - Continues chain from async operations without breaking fluency
- `.WithServiceResultAsync()` - Loads data via ServiceResult and prepares for pattern matching
- `.ThenMatchDataAsync<TResult, TData>()` - Pattern matches on loaded data's Empty state

**Benefits:**
- **Performance**: Sync validations execute immediately, no unnecessary async overhead
- **Short-Circuit**: Early validation failures prevent expensive async operations
- **Readability**: Intent is crystal clear at each step
- **Composability**: Mix and match sync/async as needed
- **Type Safety**: Full compile-time checking throughout
- **Empty Pattern Integration**: Seamless branching based on Empty state without null checks

**When to Use Each Pattern:**
- **`EnsureServiceResultAsync`** - When you need to validate that an operation succeeded before continuing
- **`ThenMatchAsync`** - When validation is complete and you need to execute the main logic
- **`WithServiceResultAsync` + `ThenMatchDataAsync`** - When you need to load data and branch based on whether it's Empty
- **`AsAsync`** - When transitioning from synchronous to asynchronous validation chains
- **`MatchAsync`** ✅ PREFERRED - For executing logic after validation when T implements IEmptyDto (uses T.Empty on failure)
- **`WhenValidAsync`** ⚠️ DEPRECATED - Legacy method that uses default(T) instead of Empty pattern

##### **MatchAsync vs WhenValidAsync - Important Distinction**

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

**Migration Strategy:**
1. Replace `ServiceValidate.Build<T>()` with `ServiceValidate.For<T>()` where T implements IEmptyDto
2. Replace `.WhenValidAsync()` with `.MatchAsync()`
3. For primitive types (bool, int, etc.), wrap in appropriate DTOs that implement IEmptyDto
4. This ensures consistent Empty pattern usage throughout the codebase

**Special Cases:**
- For collections (`IEnumerable<T>`) that don't implement IEmptyDto, use `ServiceValidate.Build<IEnumerable<T>>().WhenValidAsync()`
- This is acceptable because collections have a natural empty state (empty list) that doesn't require the Empty pattern

---

#### **🔄 ServiceValidate Flow Patterns - Complete Architecture**

The ServiceValidate pattern enables a fluent, composable validation and execution pipeline that maintains consistency across all service methods. Every public service method should follow this pattern.

##### **Core Flow Scenarios**

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

##### **🚀 Advanced Flow Pattern - Multiple Chained Executions (Future Implementation)**

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

**Key Concepts for Future Implementation:**
1. **Result Accumulation**: Each execution stage adds its result to a tuple that gets passed forward
2. **Success Chain**: The chain continues only if all previous ServiceResults are successful
3. **Type Safety**: Full compile-time type checking for all accumulated results
4. **Error Aggregation**: Errors from any stage are collected and returned in the final ServiceResult
5. **Short-Circuit**: First failure stops the chain and returns immediately

**Benefits of This Pattern:**
- **Composability**: Complex operations built from simple, testable stages
- **Transparency**: Each stage's purpose is clear and isolated
- **Reusability**: Individual stages can be extracted and reused
- **Testability**: Each stage can be unit tested independently
- **Debugging**: Clear execution flow with defined checkpoints

##### **🎯 Implementation Guidelines**

**Current Capabilities:**
- ✅ Pre-validation with sync/async mixing
- ✅ Single execution stage
- ✅ Post-validation via ThenMatchDataAsync
- ✅ ServiceResult integration throughout
- ✅ Empty pattern support

**Future Capabilities (To Be Implemented When Needed):**
- ⏳ Multiple chained execution stages
- ⏳ Result accumulation across stages
- ⏳ Complex post-validation with multiple inputs
- ⏳ Parallel execution branches with merge
- ⏳ Conditional execution paths based on intermediate results

**When to Implement Advanced Features:**
Implement these advanced patterns when you encounter:
1. Complex workflows requiring multiple dependent operations
2. Need to accumulate results from multiple service calls
3. Post-processing that requires results from multiple stages
4. Conditional logic based on intermediate results

**Implementation Strategy:**
1. Start with current patterns (they cover 95% of use cases)
2. When you need multiple executions, first try composition of service methods
3. Only implement advanced chaining when composition becomes unwieldy
4. Document the pattern usage in the specific service for future reference

**CRITICAL VALIDATION PATTERN** 🚨:
- Use error message strings directly: `.EnsureNotNull(command, ErrorMessages.RequestCannotBeNull)`
- **NEVER** wrap in ServiceError.ValidationFailed(): ❌ `.EnsureNotNull(command, ServiceError.ValidationFailed(...))`
- ServiceValidate methods handle ServiceError creation internally
- **MANDATORY**: Each validation should validate ONE aspect only - no complex conditionals

##### **Atomic Validation Rule - One Validation Per Aspect**

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

**Rules:**
- ✅ Each `.Ensure*()` call validates ONE thing
- ✅ Each validation has its own specific error message
- ❌ NO `||` or `&&` operators in validation predicates
- ❌ NO complex conditionals in a single validation
- If you need multiple checks, use multiple `.Ensure*()` calls

**MANDATORY RULE**: 
- **NEVER** use `string.IsNullOrWhiteSpace()` or `string.IsNullOrEmpty()` directly with switch expressions or if statements for single values
- **ALWAYS** use `ServiceValidate.For<T>().EnsureNotWhiteSpace()` or similar methods when T implements IEmptyDto
- This ensures consistent validation behavior and error messaging across the entire API
- **EXCEPTION**: For collections (IEnumerable<T>) that don't implement IEmptyDto, use switch expressions with ServiceError.ValidationFailed() for consistency

### 2.2 **Empty Object Pattern with IEmptyDto<T>** 🚨 NEW
All DTOs must implement `IEmptyDto<T>` interface:

```csharp
// ❌ BAD - DTO without Empty pattern
public class EquipmentDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    // ... other properties
}

// Service returning null
return ServiceResult<EquipmentDto>.Failure(null, "Not found");

// ✅ GOOD - DTO implementing IEmptyDto<T>
public class EquipmentDto : IEmptyDto<EquipmentDto>
{
    public string Id { get; set; }
    public string Name { get; set; }
    // ... other properties
    
    public bool IsEmpty => string.IsNullOrEmpty(Id) || Id == "equipment-00000000-0000-0000-0000-000000000000";
    
    public static EquipmentDto Empty => new()
    {
        Id = string.Empty,
        Name = string.Empty,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = null
    };
}

// Service using Empty pattern
return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.NotFound("Equipment"));
```

**Benefits:**
- Type-safe empty values
- No null reference exceptions
- Consistent empty state across DTOs
- Better testability

### 3. **EntityResult Pattern for Entity Creation** 🚨 NEW
ALL entity creation methods must return `EntityResult<T>` - NEVER throw exceptions:

```csharp
// ❌ BAD - Throwing exceptions in entity creation
public static class Handler
{
    public static WorkoutTemplate CreateNew(string name, ...)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
            
        return new WorkoutTemplate { Name = name, ... };
    }
}

// ✅ GOOD - EntityResult pattern with Fluent Validation
public static class Handler
{
    public static EntityResult<WorkoutTemplate> CreateNew(string name, ...)
    {
        return Validate.For<WorkoutTemplate>()
            .EnsureNotEmpty(name, "Name cannot be empty")
            .EnsureLength(name, 3, 100, "Name must be between 3 and 100 characters")
            .OnSuccess(() => new WorkoutTemplate { Name = name, ... });
    }
}
```

**Rules**:
- Use `Validate.For<T>()` fluent validation
- Return `EntityResult<T>` from all Handler methods
- NO exceptions for validation failures
- Convert to ServiceResult at service layer

### 4. **Specialized ID Types**
Use specialized ID types for type safety:

```csharp
// ❌ BAD - String IDs everywhere
public async Task<ServiceResult<ExerciseDto>> GetExercise(string id) { }
public async Task<ServiceResult<bool>> AddMuscleToExercise(string exerciseId, string muscleId) { }

// ✅ GOOD - Specialized ID types
public async Task<ServiceResult<ExerciseDto>> GetExercise(ExerciseId id) { }
public async Task<ServiceResult<bool>> AddMuscleToExercise(ExerciseId exerciseId, MuscleGroupId muscleId) { }
```

ID format: `{entitytype}-{guid}` (e.g., `exercise-550e8400-e29b-41d4-a716-446655440000`)

---

## 🏗️ API Architecture Standards

### 1. **Unit of Work Pattern**
**CRITICAL**: Proper UnitOfWork usage prevents data corruption:

```csharp
// ❌ BAD - Using WritableUnitOfWork for validation
public async Task<ServiceResult<TDto>> UpdateAsync(TId id, TCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable(); // WRONG for validation!
    var repository = unitOfWork.GetRepository<TRepository>();
    
    var existing = await repository.GetByIdAsync(id); // Tracks entity!
    if (existing.IsEmpty)
        return ServiceResult<TDto>.Failure(TDto.Empty, ServiceError.NotFound());
    
    // Entity is now tracked, causing issues
}

// ✅ GOOD - Separate concerns, use existing methods
public async Task<ServiceResult<TDto>> UpdateAsync(TId id, TCommand command)
{
    // Use existing GetByIdAsync which uses ReadOnlyUnitOfWork internally
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return ServiceResult<TDto>.Failure(TDto.Empty, existingResult.Errors);
    
    // Now do the actual update with WritableUnitOfWork
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    // ... update logic
}
```

**Rules**:
- `ReadOnlyUnitOfWork`: For ALL queries (no SaveChanges capability)
- `WritableUnitOfWork`: ONLY for Create/Update/Delete
- One UnitOfWork per method
- Reuse existing query methods for validation

### 2. **Controller Standards**
Controllers are thin pass-through layers:

```csharp
// ❌ BAD - Business logic in controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto)
{
    // Validation logic in controller
    if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
        return BadRequest(new { errors = new[] { new { code = 2, message = "Invalid name" } } });
    
    if (await _service.ExistsByNameAsync(dto.Name))
        return Conflict(new { errors = new[] { new { code = 3, message = "Name exists" } } });
    
    var command = new CreateEquipmentCommand(dto.Name, dto.Description);
    var result = await _service.CreateAsync(command);
    
    return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Errors);
}

// ✅ GOOD - Single expression, pattern matching
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto) =>
    await _service.CreateAsync(dto.ToCommand()) switch
    {
        { IsSuccess: true, Data: var data } => CreatedAtAction(nameof(GetById), new { id = data.Id }, data),
        { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(new { errors = result.StructuredErrors }),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };
```

**Controller Rules**:
- Single expression bodies when possible
- NO business logic or validation
- NO ID format validation (use ParseOrEmpty)
- Pattern matching for result handling
- Consistent error response format

### Controller Error Handling - Pure Pass-Through Pattern 🚨 CRITICAL

**MANDATORY**: Controllers must NEVER interpret, translate, or modify service error messages. The service layer owns ALL error messages and business logic.

```csharp
// ❌ VIOLATION - Controller interpreting/translating errors
[HttpGet]
public async Task<IActionResult> GetWorkoutTemplates([FromQuery] int page = 1, ...)
{
    var result = await _service.SearchAsync(page, ...);
    
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { Errors: var errors } when errors.Any(e => e.Contains("Invalid page")) => 
            BadRequest(new { errors = new[] { "Invalid page number or page size" } }), // ← VIOLATION!
        { Errors: var errors } when errors.Any(e => e.Contains("not found")) =>
            NotFound(new { errors = new[] { "Resource not found" } }), // ← VIOLATION!
        { Errors: var errors } => BadRequest(new { errors })
    };
}

// ✅ CORRECT - Pure pass-through, no interpretation
[HttpGet]
public async Task<IActionResult> GetWorkoutTemplates([FromQuery] int page = 1, ...)
{
    var result = await _service.SearchAsync(page, ...);
    
    // Single exit point - no business logic, just pass through the result
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { Errors: var errors } => BadRequest(new { errors })
    };
}
```

**Why This Is Critical:**
1. **Clear Responsibility Boundaries**: Service layer owns ALL business logic and error messages
2. **Testability**: No error translation logic to test in controllers
3. **Debugging**: Clear source of truth for error messages
4. **Maintenance**: Changes to error messages happen in one place (service)

**The Only Acceptable Status Code Mapping:**
```csharp
// ✅ ACCEPTABLE - Mapping to HTTP status codes without changing messages
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
    { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(new { errors = result.Errors }),
    { Errors: var errors } => BadRequest(new { errors })
};
```

**Key Principle**: The controller can choose the HTTP status code based on error type, but MUST pass through the original error messages unchanged.

### 3. **Repository Pattern**
Repositories handle data access only:

```csharp
public interface IEquipmentRepository : IRepositoryBase<Equipment, EquipmentId>
{
    Task<Equipment> GetByNameAsync(string name);
    Task<IEnumerable<Equipment>> GetActiveAsync();
    // NO business logic methods like ValidateUniqueName, CalculateUsage, etc.
}
```

### 4. **Pattern Matching for Single Exit Point** 🚨 CRITICAL
**NEVER have multiple returns in service methods. Use pattern matching:**

```csharp
// ❌ VIOLATION - Multiple exit points in service method
public async Task<ServiceResult<WorkoutStateDto>> GetFromCacheOrLoadAsync(string cacheKey, Func<Task<WorkoutState>> loadFunc)
{
    var cacheResult = await _cacheService.GetAsync<WorkoutStateDto>(cacheKey);
    if (cacheResult.IsHit)
        return ServiceResult<WorkoutStateDto>.Success(cacheResult.Value);
    
    var entity = await loadFunc();
    if (entity.IsEmpty)
        return ServiceResult<WorkoutStateDto>.Failure(CreateEmptyDto(), ServiceError.NotFound());
    
    return ServiceResult<WorkoutStateDto>.Success(MapToDto(entity));
}

// ✅ CORRECT - Single exit with pattern matching
public async Task<ServiceResult<WorkoutStateDto>> GetFromCacheOrLoadAsync(string cacheKey, Func<Task<WorkoutState>> loadFunc)
{
    var cacheResult = await _cacheService.GetAsync<WorkoutStateDto>(cacheKey);
    
    var result = cacheResult.IsHit
        ? ServiceResult<WorkoutStateDto>.Success(cacheResult.Value)
        : await ProcessUncachedEntity(await loadFunc(), cacheKey);
        
    return result;
}

private async Task<ServiceResult<WorkoutStateDto>> ProcessUncachedEntity(WorkoutState entity, string cacheKey) =>
    entity switch
    {
        { IsEmpty: true } => ServiceResult<WorkoutStateDto>.Failure(CreateEmptyDto(), ServiceError.NotFound()),
        _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
    };
```

**Benefits:**
- Enforces single exit point automatically
- Reduces method length (often under 20 lines)
- Lowers cyclomatic complexity
- More readable and maintainable

### 5. **Primary Constructors and Dependency Injection** 🚨 NEW
**Use C# 12 primary constructors for cleaner service and repository implementations:**

```csharp
// ❌ OLD STYLE - Traditional constructor with null validation
public class BodyPartService : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<BodyPartService> _logger;
    
    public BodyPartService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<BodyPartService> logger)
    {
        // UNNECESSARY with DI - framework ensures non-null
        ArgumentNullException.ThrowIfNull(unitOfWorkProvider);
        ArgumentNullException.ThrowIfNull(logger);
        
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }
}

// ✅ MODERN - Primary constructor (C# 12+)
public class BodyPartService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<BodyPartService> logger) : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<BodyPartService> _logger = logger;
    
    // Clean, concise, no null checks needed
}

// ✅ EVEN BETTER - Direct usage without field assignment when possible
public class BodyPartReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IBodyPartService bodyPartService,
    ILogger<BodyPartReferenceService> logger) :
    PureReferenceService<BodyPart, BodyPartDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IBodyPartService _bodyPartService = bodyPartService;
    // Base class parameters passed directly, only store what we need
}
```

**Key Rules for Primary Constructors:**
1. **NO NULL VALIDATION** - Dependency Injection guarantees non-null parameters
2. **Field Assignment** - Only create fields for parameters you actually use in the class
3. **Base Class Parameters** - Pass directly to base constructor when inherited
4. **Naming Convention** - Use camelCase for parameters, _camelCase for fields

**When to Use Primary Constructors:**
- ✅ **ALWAYS** for services with dependency injection
- ✅ **ALWAYS** for repositories with dependency injection  
- ✅ **ALWAYS** for controllers with dependency injection
- ❌ **NEVER** for entities or DTOs (use traditional constructors/properties)
- ❌ **NEVER** when you need constructor validation logic beyond DI

**Benefits:**
- Reduces boilerplate code by 5-10 lines per class
- Eliminates unnecessary null checks (DI handles this)
- More readable and maintainable
- Consistent with modern C# patterns
- Compiler-optimized

**Migration Example:**
```csharp
// Step 1: Remove traditional constructor
// Step 2: Add primary constructor parameters
// Step 3: Remove null validation
// Step 4: Assign only needed fields

// BEFORE: 15 lines
public class MyService : IMyService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<MyService> _logger;
    
    public MyService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<MyService> logger)
    {
        ArgumentNullException.ThrowIfNull(unitOfWorkProvider);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(logger);
        
        _unitOfWorkProvider = unitOfWorkProvider;
        _cacheService = cacheService;
        _logger = logger;
    }
}

// AFTER: 5 lines
public class MyService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ICacheService cacheService,
    ILogger<MyService> logger) : IMyService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<MyService> _logger = logger;
}
```

---

## 🚨 CRITICAL: Service Repository Boundaries

### **The Single Repository Rule - MANDATORY**
**Each service MUST only access its own repository directly. Cross-domain data access MUST use service dependencies.**

This is a **fundamental architectural rule** that prevents tight coupling and maintains clear domain boundaries.

### ❌ **CRITICAL VIOLATION - Found in WorkoutTemplateService**
```csharp
// ❌ VIOLATION - WorkoutTemplateService accessing other repositories directly
public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    
    // VIOLATION: Accessing IExerciseRepository directly
    var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>();
    
    // VIOLATION: Accessing IWorkoutTemplateExerciseRepository directly  
    var templateExerciseRepository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
    
    // This breaks domain boundaries and creates tight coupling!
}
```

**Why This Is Critical:**
- **Domain Boundary Violation**: WorkoutTemplateService should only access `IWorkoutTemplateRepository`
- **Tight Coupling**: Creates dependencies on repository implementations outside the service's domain
- **Transaction Complexity**: Makes cross-domain transaction management difficult
- **Maintainability**: Violates separation of concerns and makes refactoring harder

### ✅ **CORRECT PATTERN - Service-to-Service Communication**
```csharp
// ✅ CORRECT - Use service dependencies for cross-domain access
public class WorkoutTemplateService : IWorkoutTemplateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseService _exerciseService; // Service dependency
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService; // Service dependency

    public WorkoutTemplateService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseService exerciseService,
        IWorkoutTemplateExerciseService workoutTemplateExerciseService)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseService = exerciseService;
        _workoutTemplateExerciseService = workoutTemplateExerciseService;
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
    {
        // Only access own repository
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await repository.GetByIdAsync(id);
        if (template.IsEmpty)
            return ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                new List<ExerciseDto>(), 
                ServiceError.NotFound());

        // Use service dependencies for cross-domain operations
        var existingExercisesResult = await _workoutTemplateExerciseService.GetByTemplateIdAsync(id);
        if (!existingExercisesResult.IsSuccess)
            return ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                new List<ExerciseDto>(), 
                existingExercisesResult.Errors);

        var suggestedExercisesResult = await _exerciseService.GetSuggestedForTemplateAsync(
            template.CategoryId, 
            existingExercisesResult.Data.Select(e => e.ExerciseId));
            
        return suggestedExercisesResult;
    }
}
```

### **Service Repository Boundary Rules**

#### ✅ **ALLOWED Repository Access:**
- `WorkoutTemplateService` → `IWorkoutTemplateRepository` ✅
- `ExerciseService` → `IExerciseRepository` ✅  
- `WorkoutTemplateExerciseService` → `IWorkoutTemplateExerciseRepository` ✅

#### ❌ **FORBIDDEN Repository Access:**
- `WorkoutTemplateService` → `IExerciseRepository` ❌
- `WorkoutTemplateService` → `IWorkoutTemplateExerciseRepository` ❌
- `ExerciseService` → `IWorkoutTemplateRepository` ❌

### **Service-to-Service Communication Patterns**

#### **Pattern 1: Validation Across Domains**
```csharp
// ✅ CORRECT - Service validates existence via other service
public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command)
{
    // Validate category exists via CategoryService
    var categoryExists = await _categoryService.ExistsAsync(command.CategoryId);
    if (!categoryExists)
        return ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            ServiceError.InvalidReference("CategoryId", command.CategoryId));

    // Only then access own repository
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    // ... continue with creation
}
```

#### **Pattern 2: Cross-Domain Queries**
```csharp
// ✅ CORRECT - Service queries other domains via service layer
public async Task<ServiceResult<WorkoutSummaryDto>> GetWorkoutSummaryAsync(WorkoutTemplateId id)
{
    // Get own domain data
    var templateResult = await GetByIdAsync(id);
    if (!templateResult.IsSuccess)
        return ServiceResult<WorkoutSummaryDto>.Failure(WorkoutSummaryDto.Empty, templateResult.Errors);

    // Get related domain data via services
    var exercisesResult = await _workoutTemplateExerciseService.GetByTemplateIdAsync(id);
    var equipmentResult = await _exerciseService.GetRequiredEquipmentAsync(
        exercisesResult.Data.Select(e => e.ExerciseId));

    // Combine and return
    return ServiceResult<WorkoutSummaryDto>.Success(new WorkoutSummaryDto
    {
        Template = templateResult.Data,
        Exercises = exercisesResult.Data,
        RequiredEquipment = equipmentResult.Data
    });
}
```

### **For Detailed Architecture Guidelines**
📖 **See**: `/memory-bank/systemPatterns.md` - Lines 118-136 for comprehensive service-to-service communication patterns

**Key Reference:**
> "Single Repository Rule: Each service only accesses its own repository directly. Service Dependencies: Services depend on other services, not repositories."

### **🔍 Violation Detection Guide**

#### **How to Detect Repository Boundary Violations**

**1. Search for Cross-Repository Access Patterns:**
```bash
# Search for services accessing multiple repositories
grep -r "GetRepository<I.*Repository>" --include="*Service.cs" Services/Implementations/
```

**2. Look for These Anti-Patterns:**
```csharp
// ❌ RED FLAGS in Service constructors
public SomeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    // NO other repository interfaces should be injected!
    IOtherRepository otherRepository  // ← VIOLATION
) 

// ❌ RED FLAGS in Service methods
using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
var otherRepository = unitOfWork.GetRepository<IOtherRepository>(); // ← VIOLATION
```

**3. Service Naming vs Repository Access Validation:**
- `WorkoutTemplateService` should ONLY access `IWorkoutTemplateRepository`
- `ExerciseService` should ONLY access `IExerciseRepository`
- Any deviation is a violation

#### **🛠️ Step-by-Step Refactoring Guide**

**Step 1: Identify the Violation**
```csharp
// ❌ CURRENT VIOLATION in WorkoutTemplateService
public async Task<ServiceResult<List<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>(); // ← VIOLATION
    var templateExerciseRepository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>(); // ← VIOLATION
}
```

**Step 2: Add Service Dependencies**
```csharp
// ✅ REFACTOR - Add service dependencies to constructor
public class WorkoutTemplateService : IWorkoutTemplateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseService _exerciseService; // ← ADD
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService; // ← ADD

    public WorkoutTemplateService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseService exerciseService, // ← ADD
        IWorkoutTemplateExerciseService workoutTemplateExerciseService) // ← ADD
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseService = exerciseService; // ← ADD
        _workoutTemplateExerciseService = workoutTemplateExerciseService; // ← ADD
    }
}
```

**Step 3: Refactor Method Implementation**
```csharp
// ✅ REFACTOR - Use service dependencies instead of repositories
public async Task<ServiceResult<List<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
{
    // Only access own repository
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    
    var template = await repository.GetByIdAsync(id);
    if (template.IsEmpty)
        return ServiceResult<List<ExerciseDto>>.Failure(
            new List<ExerciseDto>(), 
            ServiceError.NotFound());

    // Use service dependencies for cross-domain operations
    var existingExercisesResult = await _workoutTemplateExerciseService.GetByTemplateIdAsync(id);
    if (!existingExercisesResult.IsSuccess)
        return ServiceResult<List<ExerciseDto>>.Failure(
            new List<ExerciseDto>(), 
            existingExercisesResult.Errors);

    var suggestedExercisesResult = await _exerciseService.GetSuggestedForTemplateAsync(
        template.CategoryId, 
        existingExercisesResult.Data.Select(e => e.ExerciseId));
        
    return suggestedExercisesResult;
}
```

**Step 4: Update Dependency Injection Registration**
```csharp
// Ensure all service dependencies are registered in Program.cs or Startup.cs
services.AddScoped<IWorkoutTemplateService, WorkoutTemplateService>();
services.AddScoped<IExerciseService, ExerciseService>(); // ← ENSURE REGISTERED
services.AddScoped<IWorkoutTemplateExerciseService, WorkoutTemplateExerciseService>(); // ← ENSURE REGISTERED
```

**Step 5: Update Tests**
```csharp
// Update unit tests to mock the new service dependencies
public class WorkoutTemplateServiceTests
{
    private readonly Mock<IExerciseService> _mockExerciseService; // ← ADD
    private readonly Mock<IWorkoutTemplateExerciseService> _mockWorkoutTemplateExerciseService; // ← ADD

    public WorkoutTemplateServiceTests()
    {
        _mockExerciseService = new Mock<IExerciseService>(); // ← ADD
        _mockWorkoutTemplateExerciseService = new Mock<IWorkoutTemplateExerciseService>(); // ← ADD
        
        _service = new WorkoutTemplateService(
            _mockUnitOfWorkProvider.Object,
            _mockExerciseService.Object, // ← ADD
            _mockWorkoutTemplateExerciseService.Object); // ← ADD
    }
}
```

#### **🎯 Validation Checklist After Refactoring**
- [ ] Service constructor only injects IUnitOfWorkProvider and other services (no repositories)
- [ ] Service methods only call `GetRepository<IOwnRepository>()`
- [ ] Cross-domain operations use injected service dependencies
- [ ] All service dependencies are registered in DI container
- [ ] Unit tests mock all service dependencies
- [ ] Build passes with zero errors and warnings

---

## 🔒 API Security Standards

### 1. **Authorization Attributes**
All endpoints must have explicit authorization:

```csharp
// ❌ BAD - No authorization
[HttpGet]
public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

// ✅ GOOD - Explicit authorization
[HttpGet]
[Authorize(Policy = "Free-Tier")]
public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
```

Standard policies:
- `Free-Tier`: Basic access
- `PT-Tier`: Personal trainer access
- `Admin-Tier`: Administrative access

### 2. **Input Validation**
Validate at service layer, not controller:

```csharp
// Service layer validation
private ValidationResult ValidateCreateCommand(CreateEquipmentCommand command)
{
    var errors = new List<string>();
    
    if (string.IsNullOrWhiteSpace(command.Name))
        errors.Add("Name is required");
    
    if (command.Name?.Length > 100)
        errors.Add("Name cannot exceed 100 characters");
    
    return new ValidationResult(errors);
}
```

### 3. **Sensitive Data**
- Never log sensitive information
- Exclude sensitive fields from DTOs
- Use `[JsonIgnore]` for internal properties
- Audit sensitive operations

---

## 🚀 Performance Standards

### 1. **Caching Strategy**
Reference data requires caching:

```csharp
public class EquipmentService : EntityServiceBase<Equipment, EquipmentId, EquipmentDto>
{
    protected override TimeSpan CacheDuration => TimeSpan.FromHours(1);
    
    public override async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync()
    {
        return await GetCachedOrLoadAsync(
            CacheKeys.Equipment.All,
            async () => await base.GetAllAsync()
        );
    }
}
```

Standard cache durations:
- Reference data: 1 hour
- User-specific data: 5 minutes
- Transactional data: No caching

### 1.1 **CacheLoad Pattern for Consistent Cache Handling** 🚨 NEW
Use the fluent `CacheLoad` API for all cache operations:

```csharp
// ❌ BAD - Manual cache handling with multiple exit points
public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
{
    var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
    var cached = await _cacheService.GetAsync<EquipmentDto>(cacheKey);
    
    if (cached != null)
    {
        _logger.LogDebug("Cache hit for Equipment by name");
        return ServiceResult<EquipmentDto>.Success(cached);
    }
    
    var entity = await _repository.GetByNameAsync(name);
    if (entity.IsEmpty || !entity.IsActive)
        return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.NotFound("Equipment"));
    
    var dto = MapToDto(entity);
    await _cacheService.SetAsync(cacheKey, dto);
    
    return ServiceResult<EquipmentDto>.Success(dto);
}

// ✅ GOOD - CacheLoad pattern with fluent API
public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
{
    var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
    
    return await CacheLoad.For<EquipmentDto>(_cacheService, cacheKey)
        .WithLogging(_logger, "Equipment by name")
        .MatchAsync(
            onHit: cached => ServiceResult<EquipmentDto>.Success(cached),
            onMiss: async () => await LoadEquipmentByNameAsync(name, cacheKey)
        );
}

private async Task<ServiceResult<EquipmentDto>> LoadEquipmentByNameAsync(string name, string cacheKey)
{
    var entity = await _repository.GetByNameAsync(name);
    
    return entity switch
    {
        { IsEmpty: true } or { IsActive: false } => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.NotFound("Equipment")),
        _ => await CreateSuccessResultWithCachingAsync(entity, cacheKey)
    };
}
```

**Benefits:**
- Single exit point enforcement
- Consistent cache hit/miss logging
- Cleaner separation of concerns
- Better testability
- Reduced code duplication

### 2. **Entity Framework Core**
- Disable lazy loading globally
- Use `AsNoTracking()` for queries
- Include related data explicitly
- Batch operations when possible

```csharp
// ✅ GOOD - Explicit includes, no tracking
var exercises = await context.Exercises
    .AsNoTracking()
    .Include(e => e.PrimaryMuscles)
    .Include(e => e.SecondaryMuscles)
    .Where(e => e.IsActive)
    .ToListAsync();
```

---

## 📊 API-Specific Review Checklist

### ✅ REST API Standards
- [ ] Proper HTTP verbs (GET, POST, PUT, DELETE)
- [ ] RESTful routes (`/api/resource/{id}`)
- [ ] Consistent response formats
- [ ] Appropriate status codes (200, 201, 400, 404, 409, 500)
- [ ] HATEOAS links where applicable

### ✅ DTO Standards
- [ ] Separate DTOs from domain entities
- [ ] No circular references
- [ ] Proper JSON serialization attributes
- [ ] Validation attributes on DTOs
- [ ] Empty static property for Empty pattern

### ✅ Service Layer
- [ ] 🚨 **CRITICAL**: Single Repository Rule - service only accesses its own repository
- [ ] 🚨 Cross-domain access via service dependencies, NOT direct repository access
- [ ] 🚨 Single exit point per method (use pattern matching)
- [ ] All methods return ServiceResult<T>
- [ ] Proper UnitOfWork usage (ReadOnly vs Writable)
- [ ] Business logic in services, not controllers/repositories
- [ ] Proper error codes and messages
- [ ] Cache invalidation after modifications
- [ ] 🆕 Use ServiceValidate fluent API for all validation logic
- [ ] 🆕 Use CacheLoad pattern for all cache operations
- [ ] 🆕 All DTOs implement IEmptyDto<T> interface
- [ ] 🆕 Service base classes use TDto.Empty instead of CreateEmptyDto()

### ✅ Database Access
- [ ] No N+1 queries
- [ ] Proper indexing considered
- [ ] Transactions for multi-step operations
- [ ] Connection pooling configured
- [ ] Migration scripts reviewed

### ✅ API Documentation
- [ ] OpenAPI/Swagger annotations
- [ ] XML comments on public methods
- [ ] Example requests/responses
- [ ] Error response documentation
- [ ] API versioning considered

---

## 🧪 Testing Standards

### Unit Tests
- Mock all dependencies
- Test service layer thoroughly
- Use TestIds for consistent test data
- One assert per test preferred

### 🚨 CRITICAL: No Magic Strings in Tests
**NEVER test error message content - only test ServiceErrorCode:**

```csharp
// ❌ VIOLATION - Testing error message content (brittle, localization-hostile)
[Fact]
public async Task ChangeStateAsync_WithEmptyStateId_ReturnsFailure()
{
    // Act
    var result = await _service.ChangeStateAsync(_testTemplateId, WorkoutStateId.Empty);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("GUID format", result.Errors.First()); // ← VIOLATION!
}

// ✅ CORRECT - Testing error code only (stable, localization-ready)
[Fact]
public async Task ChangeStateAsync_WithEmptyStateId_ReturnsFailure()
{
    // Act
    var result = await _service.ChangeStateAsync(_testTemplateId, WorkoutStateId.Empty);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode); // ← CORRECT!
}
```

**Why This Is Critical:**
1. **Localization**: Error messages will change for different languages
2. **Maintenance**: Changing error text shouldn't break tests
3. **API Contract**: The error code IS the contract, not the message
4. **Test Stability**: Tests remain stable across message updates

**Error Message Organization:**
```csharp
// ✅ CORRECT - Centralized error messages in constants
public static class WorkoutTemplateErrorMessages
{
    public static string InvalidIdFormat => "Invalid WorkoutTemplateId format...";
    public static string NotFound => "Workout template not found";
    // ... other messages
}

// Service uses constants, not inline strings
ServiceError.NotFound(WorkoutTemplateErrorMessages.NotFound)
```

**Testing Rules:**
- ✅ Test `ServiceErrorCode` values
- ✅ Test `result.IsSuccess` boolean
- ✅ Test returned data structure
- ❌ NEVER test error message content
- ❌ NEVER use `Assert.Contains()` on error messages
- ❌ NEVER depend on specific error text

### Integration Tests
- Use BDD approach with SpecFlow
- Test full API stack
- Real database for integration tests
- Separate test data from production

---

## 🔗 Related Documents

- Universal: `CODE_QUALITY_STANDARDS.md`
- Process: `CODE_REVIEW_PROCESS.md`
- API Memory Bank: `/memory-bank/` folder with all patterns and guides

---

Remember: These standards ensure our API is robust, performant, and maintainable. When in doubt, refer to the reference implementations in the codebase.