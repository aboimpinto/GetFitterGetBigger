# Modern C# Patterns (C# 12+)

**🎯 PURPOSE**: This document defines **MANDATORY** modern C# patterns that reduce boilerplate, improve readability, and leverage compiler optimizations in the GetFitterGetBigger API.

## Overview

These patterns are **REQUIRED** for all new code and should be adopted when refactoring existing code. They leverage C# 12+ features for cleaner, more maintainable code.

## Collection Expressions - Mandatory for Empty Collections

### ❌ OLD STYLE - Verbose Collection Initialization

```csharp
// Verbose and unnecessary
var claims = userDto.Claims ?? new List<ClaimInfo>();
var errors = command?.Errors ?? new List<string>();
var items = response?.Items ?? new Dictionary<string, object>();
var emptyList = new List<Equipment>();
var emptyArray = new Equipment[0];
```

### ✅ MODERN - Collection Expressions (C# 12+)

```csharp
// Clean and concise
var claims = userDto.Claims ?? [];
var errors = command?.Errors ?? [];
var items = response?.Items ?? [];
var emptyList = [];
var emptyArray = [];

// Also works for initialization
List<string> tags = ["fitness", "workout", "health"];
int[] numbers = [1, 2, 3, 4, 5];
```

**Benefits:**
- **Concise**: Reduces boilerplate code
- **Readable**: Intent is immediately clear
- **Consistent**: Same syntax for all collection types
- **Performance**: Compiler-optimized collection creation

## Primary Constructors for Dependency Injection

### Key Principle: No Guard Clauses for DI Parameters

When using Dependency Injection (DI) in ASP.NET Core, **guard clauses (null checks) in constructors are unnecessary and should be avoided**.

**Why?**
1. **DI Container Guarantees**: The DI container will never pass null dependencies
2. **Fail Fast at Startup**: Missing dependencies are caught during application startup
3. **Cleaner Code**: Removing unnecessary guard clauses reduces boilerplate

### ❌ OLD STYLE - Traditional Constructor with Null Validation

```csharp
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
```

### ✅ MODERN - Primary Constructor (C# 12+)

```csharp
public class BodyPartService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<BodyPartService> logger) : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<BodyPartService> _logger = logger;
    
    // Clean, concise, no null checks needed
}
```

### ✅ EVEN BETTER - Direct Usage Without Field Assignment

```csharp
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

## Key Rules for Primary Constructors

1. **NO NULL VALIDATION** - Dependency Injection guarantees non-null parameters
2. **Field Assignment** - Only create fields for parameters you actually use in the class
3. **Base Class Parameters** - Pass directly to base constructor when inherited
4. **Naming Convention** - Use camelCase for parameters, _camelCase for fields

## When to Use Primary Constructors

### ✅ ALWAYS Use For:
- Services with dependency injection
- Repositories with dependency injection  
- Controllers with dependency injection
- Any class using constructor dependency injection

### ❌ NEVER Use For:
- Entities or DTOs (use traditional constructors/properties)
- When you need constructor validation logic beyond DI
- Classes with complex initialization logic

## Real-World Migration Example

### BEFORE: 15 Lines

```csharp
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
```

### AFTER: 5 Lines

```csharp
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

## Pattern Matching for Single Exit Points

### ❌ VIOLATION - Multiple Exit Points

```csharp
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
```

### ✅ CORRECT - Single Exit with Pattern Matching

```csharp
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

## Target-Typed New Expressions

### ❌ VERBOSE - Repeating Type Names

```csharp
Dictionary<string, string> headers = new Dictionary<string, string>();
List<EquipmentDto> equipment = new List<EquipmentDto>();
ServiceResult<bool> result = new ServiceResult<bool>(true);
```

### ✅ CONCISE - Target-Typed New

```csharp
Dictionary<string, string> headers = new();
List<EquipmentDto> equipment = new();
ServiceResult<bool> result = new(true);
```

## Switch Expressions for Clean Logic

### ❌ OLD STYLE - Verbose Switch Statements

```csharp
public string GetStatusMessage(WorkoutState state)
{
    switch (state)
    {
        case WorkoutState.Draft:
            return "Workout is in draft state";
        case WorkoutState.Published:
            return "Workout is published";
        case WorkoutState.Archived:
            return "Workout is archived";
        default:
            return "Unknown state";
    }
}
```

### ✅ MODERN - Switch Expressions

```csharp
public string GetStatusMessage(WorkoutState state) => state switch
{
    WorkoutState.Draft => "Workout is in draft state",
    WorkoutState.Published => "Workout is published",
    WorkoutState.Archived => "Workout is archived",
    _ => "Unknown state"
};
```

## Record Types for Immutable Data

### ❌ OLD STYLE - Classes with Boilerplate

```csharp
public class EquipmentDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public override bool Equals(object obj) { /* ... */ }
    public override int GetHashCode() { /* ... */ }
    // ... more boilerplate
}
```

### ✅ MODERN - Records

```csharp
public record EquipmentDto
{
    public string Id { get; init; }
    public string Name { get; init; }
    // Equals, GetHashCode, ToString auto-generated
}

// Or even simpler for pure data
public record EquipmentDto(string Id, string Name);
```

## File-Scoped Namespaces

### ❌ OLD STYLE - Nested Namespace

```csharp
namespace GetFitterGetBigger.API.Services
{
    public class EquipmentService
    {
        // ... entire class indented
    }
}
```

### ✅ MODERN - File-Scoped Namespace

```csharp
namespace GetFitterGetBigger.API.Services;

public class EquipmentService
{
    // No extra indentation needed
}
```

## Global Using Directives

Create a `GlobalUsings.cs` file:

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using GetFitterGetBigger.API.Services.Results;
// ... other common usings
```

Benefits:
- Reduces repetitive using statements
- Cleaner file headers
- Centralized namespace management

## Benefits Summary

**Code Reduction:**
- Primary constructors: Save 5-10 lines per class
- Collection expressions: Save 50-70% on collection initialization
- File-scoped namespaces: Save 1 indentation level
- Pattern matching: Reduce method complexity by 30-50%

**Performance:**
- Compiler optimizations for collection expressions
- Better JIT optimization for pattern matching
- Reduced allocations with target-typed new

**Maintainability:**
- Less boilerplate = fewer bugs
- Clearer intent with modern syntax
- Better tooling support
- Easier code reviews

## Migration Strategy

1. **New Code**: Always use modern patterns
2. **Refactoring**: Update when touching existing code
3. **Bulk Updates**: Use IDE refactoring tools for:
   - Converting to primary constructors
   - Converting to file-scoped namespaces
   - Converting to collection expressions
4. **Code Reviews**: Enforce modern patterns in reviews

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/DI-CONSTRUCTOR-PATTERN.md` - Dependency injection patterns
- C# 12 Documentation: https://docs.microsoft.com/dotnet/csharp/