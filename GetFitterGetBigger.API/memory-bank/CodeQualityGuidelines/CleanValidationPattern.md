# Clean Validation Pattern

## Overview

This pattern establishes a clean, readable approach to service validation that emphasizes positive assertions, trust boundaries, and minimal defensive programming noise.

## Core Principles

### 1. Trust Your Boundaries
- **Controllers** are responsible for validating and transforming inputs into valid commands
- **Services** trust they receive valid commands from controllers
- **Minimal defensive checks** only where absolutely necessary (e.g., public API boundaries)
- **Fail fast** on programming errors rather than hiding them with excessive validation

### 2. Positive Assertions
Validation methods should read naturally with positive phrasing:
- ✅ `EnsureIsUniqueAsync` - "Ensure item IS unique"
- ✅ `EnsureExistsAsync` - "Ensure item EXISTS"
- ✅ `EnsureValidId` - "Ensure ID IS valid"
- ❌ `Ensure(() => !exists)` - Avoid double negatives
- ❌ `EnsureAsync(async () => !await IsDuplicate())` - Mental gymnastics required

### 3. Minimal Noise
- Avoid proliferation of `?`, `??`, `!` operators
- No complex boolean expressions with `||` and `&&` in validation predicates
- Keep validation logic simple and readable
- KISS principle: Keep It Simple, Stupid

## Implementation Pattern

### Service Method Structure

```csharp
public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
{
    // Minimal defensive check only if necessary
    if (command == null)
        return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
            ServiceError.ValidationFailed(EquipmentErrorMessages.Validation.RequestCannotBeNull));
    
    // Clean validation chain with positive assertions
    return await ServiceValidate.Build<EquipmentDto>()
        .EnsureNotWhiteSpace(command.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
        .EnsureIsUniqueAsync(
            async () => await IsEquipmentNameUniqueAsync(command.Name),
            ServiceError.AlreadyExists("Equipment", command.Name))
        .MatchAsync(
            whenValid: async () => await PerformCreateAsync(command),
            whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, 
                errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
        );
}
```

### Positive Helper Methods

Helper methods should have names that clearly express what they check:

```csharp
// ✅ GOOD - Clear positive intent
private async Task<bool> IsEquipmentNameUniqueAsync(string name)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    return !await repository.ExistsAsync(name.Trim());
}

private async Task<bool> CanDeleteEquipmentAsync(EquipmentId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IEquipmentRepository>();
    return !await repository.IsInUseAsync(id);
}

// ❌ BAD - Negative naming
private async Task<bool> CheckDuplicateNameAsync(string name) { ... }
private async Task<bool> IsInUseAsync(EquipmentId id) { ... }
```

## Available Validation Extensions

### Core Extensions

#### EnsureValidId
Validates that a specialized ID is valid (not null and not empty).
```csharp
.EnsureValidId(id, "Invalid equipment ID")
```

#### EnsureIsUniqueAsync
Validates that an item is unique. The predicate should return true when unique.

**Full Control (with ServiceError):**
```csharp
.EnsureIsUniqueAsync(
    async () => await IsNameUniqueAsync(name),
    ServiceError.AlreadyExists("Equipment", name))
```

**Simplified (auto-creates ServiceError):**
```csharp
.EnsureIsUniqueAsync(
    async () => await IsNameUniqueAsync(name),
    "Equipment",     // Entity name
    name)            // Item value that would be duplicate
// Internally creates: ServiceError.AlreadyExists("Equipment", name)
```

#### EnsureExistsAsync
Validates that an item exists. The predicate should return true when it exists.

**Full Control (with ServiceError):**
```csharp
.EnsureExistsAsync(
    async () => await ItemExistsAsync(id),
    ServiceError.NotFound("Equipment"))
```

**Simplified (auto-creates ServiceError):**
```csharp
.EnsureExistsAsync(
    async () => await ItemExistsAsync(id),
    "Equipment")     // Entity name
// Internally creates: ServiceError.NotFound("Equipment")
```

#### EnsureNotExistsAsync
Validates that an item does not exist. The predicate should return true when it doesn't exist.
```csharp
.EnsureNotExistsAsync(
    async () => await IsNewItemAsync(name),
    ServiceError.AlreadyExists("Equipment", name))
```

### Standard Extensions
- `EnsureNotWhiteSpace` - For string validation
- `EnsureNotNull` - For object validation (use sparingly)
- `EnsureAsync` - For custom async validations

## When to Use Defensive Validation

Only add defensive null checks when:
1. **Public API boundaries** - Methods that could be called from external sources
2. **Integration points** - Where data comes from external systems
3. **Critical operations** - Where a null could cause significant damage

Example of minimal defensive check:
```csharp
public async Task<ServiceResult<T>> ProcessCommand(Command command)
{
    // Single defensive check at entry point
    if (command == null)
        return ServiceResult<T>.Failure(T.Empty, 
            ServiceError.ValidationFailed("Command cannot be null"));
    
    // Trust command properties from here on
    // No need for command?.Property or command.Property ?? defaultValue
}
```

## Anti-Patterns to Avoid

### 1. Excessive Null Checking
```csharp
// ❌ BAD - Too defensive
.Ensure(() => command != null, "Command is null")
.EnsureNotWhiteSpace(command?.Name, "Name is empty")
.EnsureAsync(async () => command == null || !await Exists(command?.Id ?? 0))

// ✅ GOOD - Trust after initial check
if (command == null) return Failure(...);
.EnsureNotWhiteSpace(command.Name, "Name is empty")
.EnsureAsync(async () => await IsUnique(command.Id))
```

### 2. Complex Boolean Logic
```csharp
// ❌ BAD - Hard to read
.EnsureAsync(
    async () => command == null || !await CheckDuplicate(command.Name) && command.IsValid,
    "Complex error")

// ✅ GOOD - Simple and clear
.EnsureIsUniqueAsync(
    async () => await IsNameUnique(command.Name),
    ServiceError.AlreadyExists("Item", command.Name))
```

### 3. Negative Helper Methods
```csharp
// ❌ BAD - Double negative in usage
if (!await IsNotValid(item)) { ... }

// ✅ GOOD - Positive assertion
if (await IsValid(item)) { ... }
```

## Migration Guide

When refactoring existing services:

1. **Identify trust boundaries** - Where does validated data enter the service?
2. **Remove redundant null checks** - Keep only essential defensive checks
3. **Create positive helper methods** - Rename or create new helpers with positive names
4. **Use new validation extensions** - Replace complex Ensure calls with specific extensions
5. **Test thoroughly** - Ensure behavior remains consistent

## Benefits

1. **Improved Readability** - Code reads like natural language
2. **Reduced Cognitive Load** - No mental gymnastics with negations
3. **Fewer Bugs** - Simple logic has fewer edge cases
4. **Better Maintainability** - Clear intent makes changes easier
5. **Performance** - Less defensive code means fewer unnecessary checks

## Related Patterns

- [ServiceValidatePattern.md](ServiceValidatePattern.md) - Core validation pattern
- [SingleExitPointPattern.md](SingleExitPointPattern.md) - Pattern matching for clean flow
- [EmptyPattern.md](EmptyPattern.md) - Handling empty states consistently