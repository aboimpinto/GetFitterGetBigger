# Validation Extensions Catalog

**🎯 PURPOSE**: Complete catalog of all ServiceValidation extensions available for use in service methods

## Core Extensions Summary

```
┌─────────────────────────────────────────────────────────────────┐
│                   VALIDATION EXTENSIONS CATALOG                 │
├─────────────────────────────────────────────────────────────────┤
│ Synchronous Validations (use with Build or For)                │
│ • EnsureNotWhiteSpace - Required string validation             │
│ • EnsureNotEmpty - ID validation                               │
│ • EnsureMaxLength - String length validation                   │
│                                                                 │
│ Async Validations (use with Build only)                        │
│ • EnsureNameIsUniqueAsync - Positive uniqueness check          │
│ • EnsureHasValidAsync - Positive configuration validation      │
│ • EnsureAsync - General async validation                       │
└─────────────────────────────────────────────────────────────────┘
```

## Complete Extension Reference

### 1. EnsureNotWhiteSpace
**Purpose**: Validates that a string is not null, empty, or whitespace
**When to use**: Required text fields (names, descriptions, etc.)
**Pattern**: Synchronous validation

```csharp
.EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
.EnsureNotWhiteSpace(command.Description, ExerciseErrorMessages.ExerciseDescriptionRequired)
```

**Key Points**:
- Returns `ValidationFailed` error code
- Works with both `ServiceValidate.For<T>()` and `ServiceValidate.Build<T>()`
- No null checks needed - trust the command

---

### 2. EnsureNotEmpty
**Purpose**: Validates that a specialized ID is not empty
**When to use**: Validating entity IDs
**Pattern**: Synchronous validation

```csharp
.EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)
.EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
```

**Key Points**:
- Works with any `ISpecializedIdBase` implementation
- Returns `ValidationFailed` error code (not `InvalidFormat`)
- Replaces lambda expressions like `Ensure(() => !id.IsEmpty, ...)`

---

### 3. EnsureMaxLength
**Purpose**: Validates that a string does not exceed a maximum length
**When to use**: String fields with database length constraints
**Pattern**: Synchronous validation
**Added to**: `ServiceValidationBuilder<T>`

```csharp
.EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
```

**Implementation**:
```csharp
public ServiceValidationBuilder<T> EnsureMaxLength(string value, int maxLength, string errorMessage)
{
    _validation.Ensure(() => value.Length <= maxLength, ServiceError.ValidationFailed(errorMessage));
    return this;
}
```

**Key Points**:
- Cleaner than inline lambda `Ensure(() => command.Name.Length <= 255, ...)`
- Error message should be in constants file
- Works with `ServiceValidate.Build<T>()` only

---

### 4. EnsureNameIsUniqueAsync
**Purpose**: Validates that a name IS unique (positive assertion)
**When to use**: Checking for duplicate names in database
**Pattern**: Async validation with positive assertion
**Added to**: `ServiceValidationBuilder<T>`

```csharp
.EnsureNameIsUniqueAsync(
    async () => await IsNameUniqueAsync(command.Name, excludeId),
    "Exercise",
    command.Name)
```

**Implementation**:
```csharp
public ServiceValidationBuilder<T> EnsureNameIsUniqueAsync(
    Func<Task<bool>> isUniqueCheck,
    string entityName,
    string nameValue)
{
    _asyncServiceErrorValidations.Add(async () =>
    {
        var isUnique = await isUniqueCheck();
        return (isUnique, isUnique ? null : ServiceError.AlreadyExists(entityName, nameValue));
    });
    return this;
}
```

**Helper Method Pattern**:
```csharp
private async Task<bool> IsNameUniqueAsync(string name, ExerciseId? excludeId)
{
    using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
    var repository = readOnlyUow.GetRepository<IExerciseRepository>();
    var exists = await repository.ExistsAsync(name, excludeId);
    return !exists; // Return true when name IS unique
}
```

**Key Points**:
- Uses positive assertion - "IS unique" not "NOT duplicate"
- Helper method returns `true` for positive state
- Automatically creates `AlreadyExists` error
- Requires `ServiceValidate.Build<T>()` (async)

---

### 5. EnsureHasValidAsync
**Purpose**: Validates that configuration HAS valid state (positive assertion)
**When to use**: Complex business rule validation requiring database checks
**Pattern**: Async validation with positive assertion
**Added to**: `ServiceValidationBuilder<T>`

```csharp
.EnsureHasValidAsync(
    async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
    ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
```

**Implementation**:
```csharp
public ServiceValidationBuilder<T> EnsureHasValidAsync(
    Func<Task<bool>> hasValidCheck,
    string errorMessage)
{
    _asyncServiceErrorValidations.Add(async () =>
    {
        var isValid = await hasValidCheck();
        return (isValid, isValid ? null : ServiceError.ValidationFailed(errorMessage));
    });
    return this;
}
```

**Helper Method Pattern**:
```csharp
private async Task<bool> HasValidExerciseTypesAsync(List<ExerciseTypeId> exerciseTypeIds)
{
    // Complex validation logic
    // Returns true when configuration IS valid
    return hasNoTypes || (!allIdsAreEmpty && !restExerciseHasMultipleTypes);
}
```

**Key Points**:
- Uses positive assertion - "HAS valid" not "NOT invalid"
- For complex business rules that need async checks
- Error message should be in constants
- Requires `ServiceValidate.Build<T>()` (async)

---

### 6. EnsureAsync (Built-in)
**Purpose**: General async validation when custom extensions don't fit
**When to use**: One-off async validations
**Pattern**: Async validation

```csharp
.EnsureAsync(
    async () => await ExerciseExistsAsync(id),
    ServiceError.NotFound("Exercise", id.ToString()))
```

**Key Points**:
- Built into the framework
- Use when no specific extension exists
- Try to create positive assertions when possible
- Requires `ServiceValidate.Build<T>()` (async)

---

## Usage Patterns

### Pattern 1: Sync-Only Validations (Use `For<T>()`)
When all validations are synchronous:
```csharp
return ServiceValidate.For<BodyPartDto>()
    .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
    .Match(
        whenValid: () => LoadFromDatabase(id),
        whenInvalid: errors => ServiceResult<BodyPartDto>.Failure(...)
    );
```

### Pattern 2: Mixed Sync/Async Validations (Use `Build<T>()`)
When you have async validations:
```csharp
return await ServiceValidate.Build<ExerciseDto>()
    .EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
    .EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
    .EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
    .EnsureNameIsUniqueAsync(
        async () => await IsNameUniqueAsync(command.Name, null),
        "Exercise",
        command.Name)
    .EnsureHasValidAsync(
        async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
        ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
    .MatchAsync(
        whenValid: async () => await CreateExerciseInternalAsync(command)
    );
```

## Best Practices

### ✅ DO:
1. **Use positive assertions** - `IsUnique`, `HasValid`, `Exists`
2. **Keep error messages in constants** - `ExerciseErrorMessages.NameRequired`
3. **Trust commands are not null** - No null checks on command parameter
4. **Use specific extensions** - `EnsureMaxLength` vs generic `Ensure`
5. **Return true for positive state** in helper methods

### ❌ DON'T:
1. **Use double negatives** - `!CheckDuplicate`, `!Invalid`
2. **Check for null commands** - Commands are never null
3. **Use magic strings** - Always use constants
4. **Mix sync/async without Build** - Use `Build<T>()` for async
5. **Return ambiguous booleans** - Be clear what true means

## Complete Example

```csharp
public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
{
    return await ServiceValidate.Build<ExerciseDto>()
        // Synchronous validations
        .EnsureNotWhiteSpace(command.Name, ExerciseErrorMessages.ExerciseNameRequired)
        .EnsureNotWhiteSpace(command.Description, ExerciseErrorMessages.ExerciseDescriptionRequired)
        .EnsureNotEmpty(command.DifficultyId, ExerciseErrorMessages.DifficultyLevelRequired)
        .EnsureMaxLength(command.Name, 255, ExerciseErrorMessages.ExerciseNameMaxLength)
        
        // Async validations with positive assertions
        .EnsureNameIsUniqueAsync(
            async () => await IsNameUniqueAsync(command.Name, null),
            "Exercise",
            command.Name)
        .EnsureHasValidAsync(
            async () => await HasValidExerciseTypesAsync(command.ExerciseTypeIds),
            ExerciseErrorMessages.InvalidExerciseTypeConfiguration)
        
        // Execute when all validations pass
        .MatchAsync(
            whenValid: async () => await CreateExerciseInternalAsync(command)
        );
}
```

## Related Documentation
- [ServiceValidate Pattern](./ServiceValidatePattern.md)
- [Positive Validation Pattern](./PositiveValidationPattern.md)
- [No Null Command Pattern](./NoNullCommandPattern.md)
- [ID Validation Pattern](./IdValidationPattern.md)
- [Clean Validation Pattern](./CleanValidationPattern.md)

## Quick Reference Table

| Extension | Purpose | Sync/Async | Use With | Returns |
|-----------|---------|------------|----------|---------|
| `EnsureNotWhiteSpace` | Required string | Sync | For/Build | ValidationFailed |
| `EnsureNotEmpty` | ID not empty | Sync | For/Build | ValidationFailed |
| `EnsureMaxLength` | String max length | Sync | Build | ValidationFailed |
| `EnsureNameIsUniqueAsync` | Name uniqueness | Async | Build | AlreadyExists |
| `EnsureHasValidAsync` | Config validation | Async | Build | ValidationFailed |
| `EnsureAsync` | General async | Async | Build | Custom |

---

*This catalog documents all validation extensions created during the ExerciseService refactoring. Use these patterns consistently across all services.*