# Positive Validation Pattern - Clean Assertions

**🎯 PURPOSE**: Establish validation patterns that use positive assertions for better readability and maintainability

## Core Principle

```
┌─────────────────────────────────────────────────────────────────┐
│ 🚀 POSITIVE ASSERTIONS - VALIDATE WHAT THINGS ARE, NOT AREN'T  │
├─────────────────────────────────────────────────────────────────┤
│ ✅ EnsureNameIsUnique       ❌ Ensure(!CheckDuplicate)         │
│ ✅ EnsureHasValid           ❌ Ensure(!Invalid)                │
│ ✅ EnsureIsActive           ❌ Ensure(!IsInactive)             │
│ ✅ EnsureExists             ❌ Ensure(!NotExists)              │
└─────────────────────────────────────────────────────────────────┘
```

## The Problem with Negative Assertions

```csharp
// ❌ BAD - Double negation is hard to read
.EnsureAsync(
    async () => !await CheckDuplicateNameAsync(command.Name, null),
    ServiceError.AlreadyExists("Exercise", command.Name))

// Reading this in plain English:
// "Ensure that NOT check duplicate name" - WTF?!
```

## The Solution: Positive Assertions with Extension Methods

### Option 1: Create Private Extension Methods (PREFERRED)
```csharp
// ✅ BEST - Extension method with positive naming
.EnsureNameIsUniqueAsync(
    async () => await _queryDataService.IsExerciseNameUniqueAsync(command.Name),
    "Exercise",
    command.Name)

// Extension method to create:
public static class ExerciseDataServiceExtensions
{
    /// <summary>
    /// Checks if an exercise name is unique (doesn't exist).
    /// </summary>
    /// <returns>True if the name is unique, false if it already exists</returns>
    public static async Task<bool> IsExerciseNameUniqueAsync(
        this IExerciseQueryDataService dataService,
        string name,
        ExerciseId? excludeId = null)
    {
        var existsResult = await dataService.ExistsByNameAsync(name, excludeId);
        return existsResult.IsSuccess && !existsResult.Data.Value;
    }
}
```

### Option 2: Private Helper Method in Service
```csharp
// ✅ GOOD - Private helper with positive naming
.EnsureNameIsUniqueAsync(
    async () => await IsNameUniqueAsync(command.Name, null),
    "Exercise",
    command.Name)

// Private helper method:
private async Task<bool> IsNameUniqueAsync(string name, ExerciseId? excludeId)
{
    var existsResult = await _queryDataService.ExistsByNameAsync(name, excludeId);
    return existsResult.IsSuccess && !existsResult.Data.Value;
}
```

### Extension Method Naming Pattern
When creating extension methods for validation predicates, follow this pattern:

```
Is<What><Result>Async
```

Examples:
- `IsExerciseNameUniqueAsync()` - Checks if exercise name is unique
- `IsWorkoutTemplateNameUniqueAsync()` - Checks if workout template name is unique
- `IsEquipmentNameUniqueAsync()` - Checks if equipment name is unique
- `AreExerciseTypesValidAsync()` - Checks if exercise types are valid
- `IsKineticChainValidAsync()` - Checks if kinetic chain is valid
- `CanDeleteExerciseAsync()` - Checks if exercise can be deleted
- `HasValidConfigurationAsync()` - Checks if configuration is valid

## Standard Validation Extensions

### 1. Commands Never Null
```csharp
// Commands are NEVER null - no validation needed
// Trust the architecture!
```

### 2. String Validations
```csharp
// Required string
.EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)

// Maximum length
.EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
```

### 3. ID Validations
```csharp
// ID not empty
.EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)

// Note: Returns ValidationFailed, not InvalidFormat
```

### 4. Uniqueness Validation (Async)
```csharp
// Name uniqueness with positive assertion
.EnsureNameIsUniqueAsync(
    async () => await IsNameUniqueAsync(command.Name, excludeId),
    "EntityName",
    command.Name)

// Helper method returns true when name IS unique
private async Task<bool> IsNameUniqueAsync(string name, ExerciseId? excludeId)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IExerciseRepository>();
    var exists = await repository.ExistsAsync(name, excludeId);
    return !exists; // Return true when name IS unique
}
```

### 5. Configuration Validation (Async)
```csharp
// Has valid configuration
.EnsureHasValidAsync(
    async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
    "Invalid exercise type configuration")

// Helper method returns true when configuration IS valid
private async Task<bool> HasValidExerciseTypesAsync(List<ExerciseTypeId> ids)
{
    // Return true when types ARE valid
    // ...
}
```

## Complete Example

```csharp
public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
{
    return await ServiceValidate.Build<ExerciseDto>()
        // String validations
        .EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
        .EnsureNotWhiteSpace(command.Description, ExerciseErrorMessages.ExerciseDescriptionRequired)
        
        // ID validation
        .EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
        
        // Length validation
        .EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
        
        // Uniqueness validation (positive)
        .EnsureNameIsUniqueAsync(
            async () => await IsNameUniqueAsync(command.Name, null),
            "Exercise",
            command.Name)
        
        // Configuration validation (positive)
        .EnsureHasValidAsync(
            async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
            "Invalid exercise type configuration")
        
        // Execute when all validations pass
        .MatchAsync(
            whenValid: async () => await CreateExerciseInternalAsync(command)
        );
}
```

## Extension Methods Added

### ServiceValidationBuilder<T> Extensions

```csharp
/// <summary>
/// Validates that a string does not exceed a maximum length.
/// </summary>
public ServiceValidationBuilder<T> EnsureMaxLength(
    string value, 
    int maxLength, 
    string errorMessage)

/// <summary>
/// Validates that a name IS unique (positive assertion).
/// </summary>
public ServiceValidationBuilder<T> EnsureNameIsUniqueAsync(
    Func<Task<bool>> isUniqueCheck,
    string entityName,
    string nameValue)

/// <summary>
/// Validates that something HAS a valid configuration (positive assertion).
/// </summary>
public ServiceValidationBuilder<T> EnsureHasValidAsync(
    Func<Task<bool>> hasValidCheck,
    string errorMessage)
```

## Naming Conventions for Helper Methods

### ✅ Use Positive Names
- `IsNameUniqueAsync()` - Returns true when unique
- `HasValidExerciseTypesAsync()` - Returns true when valid
- `IsActiveAsync()` - Returns true when active
- `ExistsAsync()` - Returns true when exists

### ❌ Avoid Negative Names
- `CheckDuplicateNameAsync()` - Ambiguous return meaning
- `ValidateInvalidTypesAsync()` - Double negative confusion
- `NotExistsAsync()` - Negative naming

## Key Principles

1. **Name methods for what they confirm**, not what they deny
2. **Return true for positive state**, false for negative
3. **Use "Is", "Has", "Can" prefixes** for boolean methods
4. **Avoid "Check", "Validate" prefixes** - they're ambiguous
5. **Extension methods should read like English** when chained

## Benefits

1. **Readability**: Code reads like natural language
2. **Maintainability**: Clear intent reduces bugs
3. **Consistency**: Standard patterns across services
4. **Discoverability**: IntelliSense helps find the right method
5. **Less Cognitive Load**: No mental negation gymnastics

## Related Documentation
- [ServiceValidate Pattern](./ServiceValidatePattern.md)
- [Clean Validation Pattern](./CleanValidationPattern.md)
- [No Null Command Pattern](./NoNullCommandPattern.md)
- [ID Validation Pattern](./IdValidationPattern.md)

## Remember

> "Code should read like well-written prose. If you need to mentally parse double negatives, the code is wrong."

---

*Always prefer positive assertions. They make code self-documenting and reduce the chance of logic errors.*