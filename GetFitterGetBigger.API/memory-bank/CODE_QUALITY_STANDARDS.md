# Code Quality Standards - GetFitterGetBigger

**🎯 PURPOSE**: Mandatory code quality standards that MUST be followed for ALL implementations. These standards were derived from the CacheService refactoring and represent our textbook implementation approach.

## 🚨 MANDATORY: Read This Before Starting ANY Implementation

This document defines the non-negotiable code quality standards for all classes, methods, and implementations in the GetFitterGetBigger ecosystem.

---

## 📋 Core Principles

### 1. **Pattern Matching Over If-Else**
Always prefer pattern matching when dealing with conditional logic:

```csharp
// ❌ BAD - Traditional if-else
if (result.IsHit)
    return result.Value;
else
    return T.Empty;

// ✅ GOOD - Pattern matching
return result switch
{
    { IsHit: true } => result.Value,
    _ => T.Empty
};
```

### 2. **Empty/Null Object Pattern**
**NEVER** return or handle nulls directly. Always use the Empty pattern:

```csharp
// ❌ BAD - Returning null
if (entity == null)
    return null;

// ✅ GOOD - Returning Empty
if (entity.IsEmpty)
    return EntityDto.Empty;
```

### 3. **No Defensive Programming Without Justification**
Trust the framework and validate at appropriate levels:

```csharp
// ❌ BAD - Over-defensive
public void SetValue(string key, object value)
{
    if (key == null) throw new ArgumentNullException(nameof(key));
    if (value == null) throw new ArgumentNullException(nameof(value));
    if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty");
    // ... actual logic
}

// ✅ GOOD - Trust the framework
public void SetValue(string key, object value)
{
    _cache.Set(key, value); // Let MemoryCache handle null validation
}
```

### 4. **Method Length and Readability**
- Methods should be **short and focused** (ideally < 20 lines)
- Extract complex logic into helper methods
- Use descriptive names that explain the "what" not the "how"

---

## 🛠️ Implementation Standards

### 1. **Async/Await Guidelines**

#### No Fake Async
```csharp
// ❌ BAD - Fake async
public async Task DoSomething()
{
    DoWork();
    await Task.CompletedTask;
}

// ✅ GOOD - Return Task directly
public Task DoSomething()
{
    DoWork();
    return Task.CompletedTask;
}
```

#### Document Synchronous Task Returns
```csharp
/// <remarks>
/// This method returns a Task for interface compatibility, but the operation is synchronous
/// since we're using in-memory cache (IMemoryCache). If we migrate to a distributed cache
/// (Redis, SQL, etc.) in the future, the async signature will be necessary.
/// </remarks>
public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
```

### 2. **Exception Handling**

#### Only Catch What You Can Handle
```csharp
// ❌ BAD - Catching everything
try
{
    return _cache.Get<T>(key);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error getting from cache");
    return null;
}

// ✅ GOOD - Let it fail naturally
return _cache.Get<T>(key);
```

#### Minimal Try-Catch Blocks
Keep try-catch blocks focused on the specific operation that can fail:

```csharp
// ❌ BAD - Large try-catch block encompassing entire method
public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
{
    try
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var existingEntity = await repository.GetByIdAsync(id);
        if (existingEntity == null)
            return ServiceResult<bool>.Failure(false, ServiceError.NotFound("Equipment"));
        
        await repository.DeleteAsync(existingEntity);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Equipment deleted");
        InvalidateCache();
        
        return ServiceResult<bool>.Success(true);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting equipment");
        return ServiceResult<bool>.Failure(false, "Failed to delete equipment");
    }
}

// ✅ GOOD - Validate preconditions, no unnecessary try-catch
public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
{
    // Validate existence first using existing method
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return ServiceResult<bool>.Failure(false, existingResult.StructuredErrors.First());
    
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    await repository.DeleteAsync(id);
    await unitOfWork.CommitAsync();
    
    _logger.LogInformation("Equipment {EquipmentId} deleted", id);
    InvalidateCache();
    
    return ServiceResult<bool>.Success(true);
}
```

#### Precondition Validation Over Exception Handling
Always validate preconditions instead of relying on database exceptions:

```csharp
// ❌ BAD - Waiting for database constraint violation
try
{
    await repository.CreateAsync(entity);
    await unitOfWork.CommitAsync();
}
catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") ?? false)
{
    return ServiceResult<T>.Failure(ServiceErrorCode.Conflict, "Name already exists");
}

// ✅ GOOD - Check preconditions first
var existingEntity = await GetByNameAsync(entity.Name);
if (!existingEntity.IsEmpty)
    return ServiceResult<T>.Failure(ServiceErrorCode.Conflict, "Name already exists");

await repository.CreateAsync(entity);
await unitOfWork.CommitAsync();
```

#### Database Connectivity
If the database is offline, that's a critical infrastructure issue - let it fail:
- Don't wrap database operations in try-catch for connectivity issues
- Infrastructure problems should bubble up and trigger alerts
- The app should not continue operating with a dead database

#### Resource Access Requires Try-Catch
Only use try-catch for operations that access external resources AND can be recovered from:
- File System operations (file might not exist)
- Network/HTTP calls (service might be temporarily down)
- Third-party API calls (external service issues)
- NOT for database operations (precondition validation instead)
- NOT for in-memory operations

### 3. **Null Handling During Migration**

#### Mark Nullable Methods as Obsolete
```csharp
[Obsolete("This method returns null and will be removed after Empty pattern migration. " +
          "Use GetAsync<T>(key) for CacheResult<T>, or GetOrEmptyAsync<T>(key) for types implementing IEmptyEntity<T>.")]
Task<T?> GetAsync<T>(string key) where T : class;
```

#### Create Parallel Interfaces for Migration
```csharp
// Temporary interface for gradual migration
public interface IEmptyEnabledCacheService : ICacheService
{
    new Task<CacheResult<T>> GetAsync<T>(string key) where T : class;
    Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>;
}
```

---

## 🏗️ Architecture Standards

### 1. **Domain-Driven Design (DDD) Principles**
- **Clean Architecture**: Strict layer separation (Controllers → Services → Repositories → Database)
- **Dependency Rules**: Dependencies flow inward only (outer layers depend on inner)
- **No Cross-Layer Violations**: Controllers never access repositories directly
- **Business Logic Location**: All business logic in service layer, NOT in controllers or repositories

### 2. **Service Layer Standards**
```csharp
// ✅ GOOD - ServiceResult pattern for all service methods
public async Task<ServiceResult<EntityDto>> GetByIdAsync(string id)
{
    return id.IsEmpty 
        ? ServiceResult<EntityDto>.Failure(ServiceErrorCode.ValidationFailed, "Invalid ID")
        : await base.GetByIdAsync(id);
}

// ❌ BAD - Throwing exceptions for control flow
public async Task<EntityDto> GetByIdAsync(string id)
{
    if (string.IsNullOrEmpty(id))
        throw new ArgumentException("Invalid ID");
    return await base.GetByIdAsync(id);
}
```

### 3. **Repository Pattern Standards**
- **ReadOnlyUnitOfWork**: For ALL query operations (no SaveChanges)
- **WritableUnitOfWork**: ONLY for Create/Update/Delete operations
- **No Business Logic**: Repositories are data access only
- **Pattern Consistency**: All repositories follow same base patterns

#### Single UnitOfWork Per Method Principle
Each method MUST have one and only one UnitOfWork. This ensures:
- Clear separation of read vs write operations
- Single responsibility per method
- Proper transaction boundaries
- Avoiding unnecessary database locks

```csharp
// ❌ BAD - Multiple responsibilities, mixing read and write in same UnitOfWork
public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable(); // Wrong! Used for validation
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    // Validation query using WritableUnitOfWork
    var existingEntity = await repository.GetByIdAsync(id);
    if (existingEntity == null)
        return ServiceResult<bool>.Failure(false, ServiceError.NotFound("Equipment"));
    
    // Delete operation
    await repository.DeleteAsync(existingEntity);
    await unitOfWork.CommitAsync();
    return ServiceResult<bool>.Success(true);
}

// ✅ GOOD - Separated concerns, method orchestration
public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
{
    // Use existing GetByIdAsync method for validation
    var existingResult = await GetByIdAsync(id);
    if (!existingResult.IsSuccess)
        return ServiceResult<bool>.Failure(false, existingResult.StructuredErrors.First());
    
    // Now perform the actual deletion
    return await PerformDeleteAsync(id);
}

private async Task<ServiceResult<bool>> PerformDeleteAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    
    await repository.DeleteAsync(id);
    await unitOfWork.CommitAsync();
    
    _logger.LogInformation("Equipment {EquipmentId} deleted successfully", id);
    return ServiceResult<bool>.Success(true);
}
```

Key benefits:
- Reuses existing validated methods (GetByIdAsync already returns Empty for non-existent entities)
- Each method has single focus
- ReadOnly operations remain separate from Write operations
- Better testability and maintainability

### 4. **Controller Standards**
- **Clean Pass-Through**: Controllers only orchestrate, no business logic
- **Pattern Matching**: Use pattern matching for ServiceResult handling
- **HTTP Status Codes**: Consistent mapping (200/201/400/404/500)
- **No Direct Database Access**: Never inject repositories or UnitOfWork
- **No ID Format Validation**: Controllers should NOT validate ID formats - just ParseOrEmpty and let service handle validation
- **Single Expression Methods**: Controller actions should be simple expression-bodied methods when possible
- **Low Cyclomatic Complexity**: Each action should have a single return statement using pattern matching

```csharp
// ❌ BAD - Multiple returns, ID validation in controller
public async Task<IActionResult> GetById(string id)
{
    if (string.IsNullOrWhiteSpace(id) || !id.StartsWith("equipment-"))
        return BadRequest(new { errors = new[] { new { code = 2, message = "Invalid ID" } } });
        
    var equipmentId = EquipmentId.ParseOrEmpty(id);
    if (equipmentId.IsEmpty)
        return BadRequest(new { errors = new[] { new { code = 2, message = "Invalid ID" } } });
        
    return await _service.GetByIdAsync(equipmentId) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };
}

// ✅ GOOD - Single expression, no validation in controller
public async Task<IActionResult> GetById(string id) =>
    await _service.GetByIdAsync(EquipmentId.ParseOrEmpty(id)) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };
```

---

## 📊 Comprehensive Code Review Checklist

### ✅ Architecture & Design Patterns
- [ ] **Layer Separation**: No cross-layer dependencies
- [ ] **DDD Compliance**: Domain logic properly encapsulated
- [ ] **SOLID Principles**: Single responsibility, dependency inversion
- [ ] **Repository Pattern**: Correct UnitOfWork usage (ReadOnly vs Writable)
- [ ] **Single UnitOfWork Per Method**: Each method has one and only one UnitOfWork
- [ ] **Method Orchestration**: Complex operations use method composition, not mixed concerns
- [ ] **Service Pattern**: All methods return ServiceResult<T>
- [ ] **Controller Pattern**: Clean pass-through, no business logic

### ✅ Pattern Matching & Modern C#
- [ ] All conditional returns use switch expressions where applicable
- [ ] No if-else chains that could be pattern matches
- [ ] Tuple deconstruction used where appropriate
- [ ] Target-typed new expressions used
- [ ] Record types for DTOs where applicable

### ✅ Empty/Null Object Pattern
- [ ] No methods return null (except legacy/obsolete)
- [ ] All entities implement IEmptyEntity<T>
- [ ] IsEmpty checks instead of null checks
- [ ] Empty static property on all entities
- [ ] No null propagation operators (?.) except in DTOs

### ✅ Method Quality & Complexity
- [ ] Methods are < 20 lines
- [ ] Single responsibility per method
- [ ] Clear, descriptive names
- [ ] No fake async
- [ ] Cyclomatic complexity < 10
- [ ] No deeply nested code (max 3 levels)

### ✅ Error Handling & Exceptions
- [ ] No unnecessary try-catch blocks
- [ ] Minimal try-catch scope (only wrap specific failing operations)
- [ ] Precondition validation instead of catching database exceptions
- [ ] Only catch exceptions for recoverable external resource failures
- [ ] Let framework handle validation
- [ ] Let database connectivity issues fail (infrastructure problem)
- [ ] No exceptions for control flow
- [ ] ServiceResult pattern for error propagation
- [ ] Proper error codes (ServiceErrorCode enum)

### ✅ Performance & Efficiency
- [ ] **Caching Strategy**: Proper cache implementation for reference data
- [ ] **Async Patterns**: No blocking async calls (.Result, .Wait())
- [ ] **Database Queries**: Efficient queries, no N+1 problems
- [ ] **Memory Usage**: No unnecessary object allocations
- [ ] **Lazy Loading**: Disabled to prevent performance issues

### ✅ Security Standards
- [ ] **Input Validation**: All inputs validated at service layer
- [ ] **SQL Injection**: No raw SQL, use EF Core properly
- [ ] **Authorization**: Proper claim checks in controllers
- [ ] **Sensitive Data**: No logging of sensitive information
- [ ] **CORS**: Properly configured if applicable

### ✅ Testing Standards
- [ ] **Unit Tests**: Everything mocked, test single units
- [ ] **Integration Tests**: BDD scenarios, real database
- [ ] **Test Separation**: Unit in API.Tests, Integration in API.IntegrationTests
- [ ] **No Magic Strings**: Use constants or builders in tests
- [ ] **Test Coverage**: Critical paths covered
- [ ] **Test Naming**: Clear Given_When_Then pattern

### ✅ Documentation & Maintainability
- [ ] XML comments on all public methods
- [ ] Remarks for non-obvious design decisions
- [ ] Obsolete attributes with migration guidance
- [ ] Clear variable and method names
- [ ] No commented-out code
- [ ] README updates if needed

### ✅ Code Consistency
- [ ] **Naming Conventions**: PascalCase, camelCase properly used
- [ ] **File Organization**: One class per file
- [ ] **Namespace Structure**: Matches folder structure
- [ ] **Code Formatting**: Consistent indentation and spacing
- [ ] **Import Organization**: System imports first, then project

### ✅ Dependency Injection
- [ ] **Service Registration**: Proper lifetime (Scoped/Singleton/Transient)
- [ ] **Interface Segregation**: Small, focused interfaces
- [ ] **No Service Locator**: Constructor injection only
- [ ] **Circular Dependencies**: None present

### ✅ Database & EF Core
- [ ] **Migrations**: Clean, reversible migrations
- [ ] **Seed Data**: Proper seed data for reference tables
- [ ] **Entity Configuration**: Fluent API over attributes
- [ ] **Navigation Properties**: Properly configured
- [ ] **Indexes**: Added for frequently queried fields

### ✅ API Design
- [ ] **RESTful Conventions**: Proper HTTP verbs and routes
- [ ] **API Versioning**: Considered if breaking changes
- [ ] **OpenAPI/Swagger**: Properly documented endpoints
- [ ] **Request/Response DTOs**: Separate from domain models
- [ ] **Consistent Error Responses**: Standard error format

---

## 🚀 Migration Strategy

When refactoring existing code:

1. **Create Parallel Structure**
   - Don't break existing code
   - Add new Empty-aware methods alongside old ones

2. **Mark Old Methods Obsolete**
   - Clear migration messages
   - Point to new methods

3. **Update Incrementally**
   - Fix warnings as you work on each area
   - Don't do massive refactors

4. **Document the Journey**
   - Update EMPTY-PATTERN-MIGRATION-GUIDE.md
   - Add to LESSONS-LEARNED.md

---

## 📚 Reference Implementation

The `CacheService` class serves as our reference implementation. Study these methods:

```csharp
// Pattern matching example
public async Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>
{
    var result = await GetAsync<T>(key);
    return result switch
    {
        { IsHit: true } => result.Value,
        _ => T.Empty
    };
}

// Clean, simple implementation
public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
{
    var cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(expiration)
        .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
        {
            _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
        });

    _memoryCache.Set(key, value, cacheEntryOptions);
    _cacheKeys.TryAdd(key, true);
    
    return Task.CompletedTask;
}
```

---

## 🔗 Related Documents

- `EMPTY-PATTERN-MIGRATION-GUIDE.md` - How to migrate to Empty pattern
- `DEVELOPMENT_PROCESS.md` - Overall development workflow
- `TESTING-QUICK-REFERENCE.md` - Testing patterns and standards

---

## 💡 Remember

> "The best code is code that doesn't need to exist. The second best is code that's so clear it doesn't need comments."

Follow these standards to create maintainable, readable, and robust code that serves as an example for others.