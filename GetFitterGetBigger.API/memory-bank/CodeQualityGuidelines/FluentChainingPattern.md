# Fluent Chaining Pattern for Single Exit Point

## Core Principle
**Always strive for a single fluent chain** that reads like a story from top to bottom. This is the gold standard for refactoring methods with multiple exit points.

## The Goal: Beautiful, Readable Code

```csharp
// THIS is what we're aiming for - a single, unbroken chain:
return await ServiceValidate.Build<TResult>()
    .EnsureNotEmpty(source, error)
    .ThenCreateDuplicate(src => ...)
    .ThenAddAsync(async d => ...)
    .ThenReloadAsync(async d => ...)
    .MatchAsync(
        whenValid: data => Success(data),
        whenInvalid: error => Failure(error));
```

## When to Create Extension Methods

### 1. Create a `Then` Extension Method When:

- **You have a common operation** that appears in multiple service methods
- **You need to transform data** from one type to another
- **You need to perform an operation** (add, update, delete)
- **You want to make intent clearer** (ThenCreateDuplicate vs ThenCreate)

### 2. Types of Extension Methods to Create:

#### Validation Extensions
```csharp
.EnsureNotEmpty(entity, error)
.EnsureIsValid(predicate, error)
.EnsureExists(async () => await CheckExists(), error)
```

#### Transformation Extensions
```csharp
.ThenCreate(data => EntityResult<T>)
.ThenTransform(data => newData)
.ThenMap(data => dto)
```

#### Operation Extensions
```csharp
.ThenAddAsync(async entity => await repository.AddAsync(entity))
.ThenUpdateAsync(async entity => await repository.UpdateAsync(entity))
.ThenDeleteAsync(async entity => await repository.DeleteAsync(entity))
```

#### Loading Extensions
```csharp
.ThenLoadAsync(async id => await repository.GetByIdAsync(id))
.ThenReloadAsync(async entity => await repository.GetByIdWithDetailsAsync(entity.Id))
```

## The Key Insight: Deferred Execution

**THERE IS NOTHING WRONG WITH CONVERTING ASYNC TO SYNC** for the sake of fluent chaining!

### The Pattern:
1. **Async operations should return immediately** with a ChainedServiceValidationBuilder
2. **Store the async operation as a function** to execute later
3. **Execute the entire pipeline** when MatchAsync is called

```csharp
public ChainedServiceValidationBuilder<TData, TResult> ThenPerformAsync(
    Func<TData, Task> operation,
    string operationDescription)
{
    // Store the async operation as a deferred function
    async Task<(object data, bool isValid, ServiceError? error)> NewPipeline()
    {
        var (prevData, prevValid, prevError) = await _pipeline();
        
        if (!prevValid)
            return (prevData, false, prevError);
        
        await operation((TData)prevData);  // Execute when pipeline runs
        return (prevData, true, null);
    }
    
    // Return immediately - maintain fluent interface
    return new ChainedServiceValidationBuilder<TData, TResult>(_innerBuilder, NewPipeline);
}
```

## Why This Pattern is Superior

### 1. **Single Exit Point**
- Only ONE return statement in the entire method
- No conditional returns scattered throughout

### 2. **Readable Like a Story**
```csharp
// You can read this top to bottom and understand the flow:
1. Ensure the source is not empty
2. Then create a duplicate
3. Then add it to the repository
4. Then reload it with full details
5. Return success or failure
```

### 3. **Composable and Reusable**
- Each extension method is a building block
- Can mix and match for different scenarios
- DRY principle - write once, use everywhere

### 4. **Testable**
- Each extension can be tested independently
- Pipeline execution is predictable
- Easy to mock dependencies

## Common Patterns to Implement

### Pattern 1: Load → Validate → Transform → Save → Return
```csharp
return await ServiceValidate.Build<TDto>()
    .ThenLoadAsync(async () => await repository.GetByIdAsync(id))
    .EnsureNotEmpty(entity, ErrorMessages.NotFound)
    .ThenTransform(entity => entity.Update(newData))
    .ThenSaveAsync(async updated => await repository.UpdateAsync(updated))
    .ThenMap(saved => saved.ToDto())
    .MatchAsync(
        whenValid: dto => Success(dto),
        whenInvalid: error => Failure(Empty, error));
```

### Pattern 2: Validate → Create → Add → Return
```csharp
return await ServiceValidate.Build<TDto>()
    .EnsureNotWhiteSpace(name, ErrorMessages.NameRequired)
    .ThenCreate(() => Entity.Handler.CreateNew(name, ...))
    .ThenAddAsync(async entity => await repository.AddAsync(entity))
    .ThenMap(entity => entity.ToDto())
    .MatchAsync(
        whenValid: dto => Success(dto),
        whenInvalid: error => Failure(Empty, error));
```

### Pattern 3: Load → Check → Delete → Return
```csharp
return await ServiceValidate.Build<BooleanResultDto>()
    .ThenLoadAsync(async () => await repository.GetByIdAsync(id))
    .EnsureNotEmpty(entity, ErrorMessages.NotFound)
    .EnsureCanDelete(entity, ErrorMessages.CannotDelete)
    .ThenDeleteAsync(async entity => await repository.DeleteAsync(entity.Id))
    .ThenReturn(() => BooleanResultDto.Create(true))
    .MatchAsync(
        whenValid: result => Success(result),
        whenInvalid: error => Failure(BooleanResultDto.Create(false), error));
```

## Guidelines for Creating Extension Methods

### 1. **Name Them Clearly**
- Start with `Then` for operations: ThenCreate, ThenAdd, ThenUpdate
- Start with `Ensure` for validations: EnsureNotEmpty, EnsureValid
- Make the intent obvious: ThenCreateDuplicate vs generic ThenCreate

### 2. **Keep Them Focused**
- Each extension should do ONE thing
- Don't combine multiple operations in one extension
- Let the chain tell the story

### 3. **Handle Errors Gracefully**
- Wrap operations in try-catch when appropriate
- Convert exceptions to ServiceErrors
- Pass errors through the pipeline

### 4. **Make Them Reusable**
- Generic where possible
- Specific where needed for clarity
- Think about other places the extension could be used

## The Philosophy

**"If you need to break the chain into variables, you haven't created enough extension methods."**

Every time you see code like this:
```csharp
// BAD - Breaking the chain
var chain = ServiceValidate.Build<T>().EnsureNotEmpty(source, error);
var chainAfterAdd = await chain.ThenAddAsync(...);
var chainAfterReload = await chainAfterAdd.ThenReloadAsync(...);
return await chainAfterReload.MatchAsync(...);
```

Ask yourself: **"What extension method am I missing?"**

The answer is usually: You need to defer the async execution!

## Real-World Example

### Before (Multiple Exit Points):
```csharp
private async Task<ServiceResult<WorkoutTemplateDto>> DuplicateWithScopeAsync(...)
{
    var source = await repository.GetByIdWithDetailsAsync(sourceId);
    if (source.IsEmpty)
        return ServiceResult<WorkoutTemplateDto>.Success(WorkoutTemplateDto.Empty);
    
    var duplicateResult = WorkoutTemplateEntity.Handler.CreateNew(...);
    if (!duplicateResult.IsSuccess)
        return ServiceResult<WorkoutTemplateDto>.Failure(WorkoutTemplateDto.Empty, ...);
    
    await repository.AddAsync(duplicate);
    var created = await repository.GetByIdWithDetailsAsync(duplicate.Id);
    return ServiceResult<WorkoutTemplateDto>.Success(created.ToDto());
}
```

### After (Single Fluent Chain):
```csharp
private async Task<ServiceResult<WorkoutTemplateDto>> DuplicateWithScopeAsync(...)
{
    var source = await repository.GetByIdWithDetailsAsync(sourceId);
    
    return await ServiceValidate.Build<WorkoutTemplateDto>()
        .EnsureNotEmpty(source, ServiceError.NotFound("WorkoutTemplate", sourceId))
        .ThenCreateDuplicate(src => WorkoutTemplateEntity.Handler.CreateNew(...))
        .ThenAddAsync(async duplicate => await repository.AddAsync(duplicate))
        .ThenReloadAsync(async duplicate => await repository.GetByIdWithDetailsAsync(duplicate.Id))
        .MatchAsync(
            whenValid: template => ServiceResult<WorkoutTemplateDto>.Success(template.ToDto()),
            whenInvalid: error => ServiceResult<WorkoutTemplateDto>.Failure(WorkoutTemplateDto.Empty, error));
}
```

## Conclusion

This pattern represents the pinnacle of clean, maintainable code:
- **Single responsibility** - Each extension does one thing
- **Single exit point** - One return statement
- **Readable** - Flows like natural language
- **Testable** - Each step can be verified
- **Maintainable** - Easy to add, remove, or reorder steps

**Always strive for the single fluent chain. It's not just about following rules - it's about creating code that's a joy to read and maintain.**