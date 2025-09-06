# Trust the Validation Chain - Stop Repeating Yourself!

## 🎯 Core Principle

**"Validate Once, Trust Forever"** - Each layer should validate what it's responsible for, then trust that validation throughout the execution chain.

## ❌ The Problem: Paranoid Validation

Too often, we see code that repeatedly validates the same conditions across multiple layers and methods:

```csharp
// ❌ ANTI-PATTERN: Same validation repeated 4 times!

// Layer 1: Controller
public async Task<IActionResult> UpdateLink(string linkId, UpdateRequest request)
{
    if (string.IsNullOrEmpty(linkId))  // Validation #1
        return BadRequest("Invalid ID");
    
    var command = request.ToCommand(linkId);
    return await _service.UpdateAsync(command);
}

// Layer 2: Service Public Method
public async Task<ServiceResult<T>> UpdateAsync(UpdateCommand command)
{
    if (command.LinkId.IsEmpty)  // Validation #2 (redundant!)
        return ServiceResult<T>.Failure(ValidationFailed());
        
    return await ServiceValidate.Build<T>()
        .EnsureNotEmpty(command.LinkId, "Invalid ID")  // Validation #3 (redundant!)
        .EnsureAsync(async () => await ExistsAsync(command.LinkId), NotFound())
        .MatchAsync(whenValid: async () => await UpdateInternalAsync(command));
}

// Layer 3: Service Internal Method
private async Task<ServiceResult<T>> UpdateInternalAsync(UpdateCommand command)
{
    if (command.LinkId.IsEmpty)  // Validation #4 (paranoid!)
        return ServiceResult<T>.Failure(ValidationFailed());
        
    var entity = await GetByIdAsync(command.LinkId);
    if (entity.IsEmpty)  // Validation #5 (we already checked existence!)
        return ServiceResult<T>.Failure(NotFound());
        
    // ... actual update logic
}

// Layer 4: DataService
public async Task<ServiceResult<T>> UpdateAsync(T dto)
{
    var id = ParseId(dto.Id);
    if (id.IsEmpty)  // Validation #6 (seriously?!)
        return ServiceResult<T>.Failure(ValidationFailed());
        
    var entity = await repository.GetByIdAsync(id);
    if (entity.IsEmpty)  // Validation #7 (STOP IT!)
        return ServiceResult<T>.Failure(NotFound());
        
    // ... update logic
}
```

## ✅ The Solution: Trust the Chain

Each layer validates ONCE, then trusts that validation:

```csharp
// ✅ CORRECT: Each validation happens exactly once

// Layer 1: Controller - Validates request format
public async Task<IActionResult> UpdateLink(string linkId, UpdateRequest request)
{
    // Controller's job: Parse and convert
    var parsedId = ExerciseLinkId.ParseOrEmpty(linkId);
    if (parsedId.IsEmpty)
        return BadRequest("Invalid ID format");
    
    var command = request.ToCommand(parsedId);  // Pass typed ID
    return await _service.UpdateAsync(command);
}

// Layer 2: Service - Validates business rules ONCE
public async Task<ServiceResult<T>> UpdateAsync(UpdateCommand command)
{
    // All validation in ONE place
    return await ServiceValidate.Build<T>()
        .EnsureNotEmpty(command.LinkId, "Invalid ID")
        .EnsureAsync(async () => await ExistsAsync(command.LinkId), NotFound())
        .EnsureAsync(async () => await CanUserUpdate(command.LinkId), Forbidden())
        .MatchAsync(whenValid: async () => await UpdateInternalAsync(command));
}

// Layer 3: Service Internal - TRUSTS validation
private async Task<ServiceResult<T>> UpdateInternalAsync(UpdateCommand command)
{
    // NO VALIDATION! We trust the chain
    // Just execute with confidence
    return await _commandDataService.UpdateAsync(
        command.LinkId,
        entity => /* transformation logic */
    );
}

// Layer 4: DataService - TRUSTS service validated everything
public async Task<ServiceResult<T>> UpdateAsync(TId id, Func<T, T> updateAction)
{
    // NO VALIDATION! Just execute
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IRepository>();
    
    var entity = await repository.GetByIdAsync(id);  // Trust it exists
    var updated = updateAction(entity);              // Trust the transformation
    await repository.UpdateAsync(updated);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<T>.Success(updated.ToDto());
}
```

## 📊 Validation Responsibility Matrix

| Layer | What It Validates | What It Trusts |
|-------|------------------|----------------|
| **Controller** | • Request format<br>• ID parsing<br>• Required fields | • Nothing (entry point) |
| **Service (Public)** | • Business rules<br>• Entity existence<br>• Permissions<br>• State transitions | • Controller provided valid command |
| **Service (Internal)** | • Nothing! | • All validation from public method |
| **DataService** | • Nothing! | • Service validated everything |
| **Repository** | • Nothing! | • DataService knows what it's doing |

## 🔍 How to Identify Where You're Coming From

Before writing any validation, ask yourself:

### 1. What's My Entry Point?
```csharp
// If you're in UpdateInternalAsync, you came from UpdateAsync
UpdateAsync() --> UpdateInternalAsync()
     ↓                    ↓
  Validates           TRUSTS
```

### 2. What Was Already Validated?
```csharp
// Look at the calling method's validation chain
public async Task<ServiceResult<T>> UpdateAsync(UpdateCommand command)
{
    return await ServiceValidate.Build<T>()
        .EnsureNotEmpty(command.LinkId, "...")        // ✓ ID validated
        .EnsureAsync(async () => await ExistsAsync()) // ✓ Existence validated
        .EnsureAsync(async () => await CanUpdate())   // ✓ Permission validated
        .MatchAsync(whenValid: async () => await UpdateInternalAsync(command));
}

// So UpdateInternalAsync knows:
// - LinkId is not empty ✓
// - Entity exists ✓
// - User has permission ✓
// NO NEED TO CHECK AGAIN!
```

### 3. What's My Responsibility?
```csharp
// Each method has ONE job
UpdateAsync()          → Validate and orchestrate
UpdateInternalAsync()  → Execute the update
DataService.Update()   → Handle database operation
Repository.Update()    → Execute SQL
```

## 🚨 Code Smells That Indicate Redundant Validation

### 1. The "Just In Case" Check
```csharp
private async Task<Result> DoSomethingInternal(Command command)
{
    // 🚨 SMELL: "Just in case" the caller didn't validate
    if (command == null || command.Id.IsEmpty)
        return Failure();
}
```

### 2. The "Paranoid Null Check"
```csharp
public async Task<Result> UpdateAsync(UpdateDto dto)
{
    // 🚨 SMELL: Service layer would never send null
    if (dto == null)
        return Failure();
}
```

### 3. The "Existence Check Cascade"
```csharp
// 🚨 SMELL: Checking existence at every level
Service:     if (!await ExistsAsync(id)) return NotFound();
Internal:    if (!await ExistsAsync(id)) return NotFound();  // Again?!
DataService: if (!await ExistsAsync(id)) return NotFound();  // STOP!
```

### 4. The "Defense in Depth" Fallacy
```csharp
// 🚨 SMELL: Multiple layers of the same check
if (id.IsEmpty) return Error();
var entity = await GetByIdAsync(id);
if (entity == null) return Error();      // Can't happen with Empty pattern
if (entity.IsEmpty) return Error();      // Redundant with null check
if (entity.Id != id) return Error();     // Paranoid!
```

## ✅ Best Practices

### 1. Document Your Trust Boundaries
```csharp
private async Task<ServiceResult<T>> UpdateInternalAsync(UpdateCommand command)
{
    // TRUST: UpdateAsync has validated:
    // - command.LinkId is not empty
    // - Link exists in database
    // - User has permission to update
    
    // Just execute with confidence
    return await _commandDataService.UpdateAsync(command.LinkId, ...);
}
```

### 2. Use Method Names That Indicate Trust
```csharp
// Names that indicate validation is expected
public async Task<Result> UpdateAsync()           // Public API - validates
public async Task<Result> ValidateAndUpdate()     // Explicit about validation

// Names that indicate trust
private async Task<Result> UpdateInternalAsync()  // Internal - trusts
private async Task<Result> ExecuteUpdateAsync()   // Execute - no validation
private async Task<Result> PerformUpdateAsync()   // Perform - just does it
```

### 3. Single Validation Point Pattern
```csharp
public async Task<ServiceResult<T>> UpdateAsync(UpdateCommand command)
{
    // ONE validation chain for ALL paths
    return await ServiceValidate.Build<T>()
        .EnsureNotEmpty(command.Id, InvalidId)
        .EnsureAsync(async () => await ExistsAsync(command.Id), NotFound())
        .EnsureAsync(async () => await IsNotInUse(command.Id), InUse())
        .MatchAsync(
            whenValid: async () => command.UseOptimizedPath 
                ? await FastUpdateAsync(command)    // Both paths trust validation
                : await StandardUpdateAsync(command)
        );
}
```

### 4. Trust Comments
```csharp
public async Task<ServiceResult<T>> UpdateAsync(TId id, Func<T, T> updateAction)
{
    // TRUST THE INFRASTRUCTURE!
    // Service layer has already validated:
    // - ID is not empty
    // - Entity exists
    // - User has permission
    // We just execute the update
    
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IRepository>();
    
    var entity = await repository.GetByIdAsync(id);
    var updated = updateAction(entity);
    await repository.UpdateAsync(updated);
    await unitOfWork.CommitAsync();
    
    return ServiceResult<T>.Success(updated.ToDto());
}
```

## 📈 Benefits of Trusting the Chain

### 1. Performance
- **50-70% fewer database calls** (no redundant existence checks)
- **Faster response times** (less validation overhead)
- **Reduced database load** (one check instead of five)

### 2. Maintainability
- **Single source of truth** for validation logic
- **Easier to update** validation rules (one place)
- **Clearer code flow** (validation separate from execution)

### 3. Readability
- **Less clutter** in internal methods
- **Clear responsibilities** for each method
- **Explicit trust boundaries** documented

### 4. Testing
- **Easier to test** (mock less, trust more)
- **Focused tests** (each layer tests its responsibility)
- **No redundant test cases** for the same validation

## 🎯 The Golden Rule

> **"If you're validating the same thing twice, you're doing it wrong!"**

Every validation should happen exactly once, at the highest appropriate level, and then be trusted throughout the execution chain.

## 🔗 Related Documentation
- [ServiceValidatePattern.md](./ServiceValidatePattern.md) - How to structure validation chains
- [ValidationAntiPatterns.md](./ValidationAntiPatterns.md) - Common validation mistakes
- [LayeredArchitectureRules.md](./ArchitecturalPatterns/LayeredArchitectureRules.md) - Layer responsibilities

---

**Remember**: Defensive programming is not about checking everything everywhere. It's about checking the right things in the right places, then trusting your architecture to maintain those invariants.