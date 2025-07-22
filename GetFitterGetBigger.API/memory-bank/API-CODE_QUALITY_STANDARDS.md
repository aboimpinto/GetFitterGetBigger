# API-Specific Code Quality Standards - GetFitterGetBigger

**🎯 PURPOSE**: API-specific code quality standards that extend the universal standards for the GetFitterGetBigger API project. These standards are mandatory for all API implementations.

## 📋 Prerequisites

This document extends the universal `CODE_QUALITY_STANDARDS.md`. Read that first, then apply these API-specific standards.

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

// ✅ GOOD - ServiceResult pattern
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    var validationResult = ValidateCommand(command);
    if (!validationResult.IsValid)
        return ServiceResult<EquipmentDto>.Failure(
            EquipmentDto.Empty, 
            ServiceError.ValidationFailed(validationResult.Errors));
    
    var entity = await _repository.CreateAsync(command.ToEntity());
    return ServiceResult<EquipmentDto>.Success(entity.ToDto());
}
```

### 3. **Specialized ID Types**
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
- [ ] 🚨 Single exit point per method (use pattern matching)
- [ ] All methods return ServiceResult<T>
- [ ] Proper UnitOfWork usage (ReadOnly vs Writable)
- [ ] Business logic in services, not controllers/repositories
- [ ] Proper error codes and messages
- [ ] Cache invalidation after modifications

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