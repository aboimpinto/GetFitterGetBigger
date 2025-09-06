# Code Quality Standards - GetFitterGetBigger C# Projects

**🎯 PURPOSE**: Code quality standards for C# projects in the GetFitterGetBigger ecosystem (API and Admin). These are the non-negotiable quality standards for maintainable, readable, and robust C# code.

## 🚨 MANDATORY: Read This Before Starting ANY Implementation

This document defines the core quality principles for C# development in GetFitterGetBigger. All code examples use C# to maintain consistency.

---

## 📋 Core C# Development Principles

### 1. **Pattern Matching Over If-Else**
C# supports pattern matching - use it for cleaner, more readable code:

```csharp
// ❌ BAD - Traditional if-else chains
if (result.IsSuccess)
    return result.Value;
else if (result.IsNotFound)
    return NotFound();
else
    return BadRequest(result.Error);

// ✅ GOOD - Pattern matching (C# example)
return result switch
{
    { IsSuccess: true } => result.Value,
    { IsNotFound: true } => NotFound(),
    _ => BadRequest(result.Error)
};
```

#### Result Pattern with Extension Methods
When handling service results with specific error conditions, combine pattern matching with extension methods:

```csharp
// ❌ BAD - Multiple returns with if-else chains
var dataResult = await _dataProvider.CreateWorkoutTemplateAsync(validTemplate);

if (!dataResult.IsSuccess)
{
    var error = dataResult.Errors.FirstOrDefault();
    if (error?.Code == DataErrorCode.Conflict)
    {
        return ServiceResult<WorkoutTemplateDto>.Failure(
            ServiceError.DuplicateName(validTemplate.Name));
    }
    
    return dataResult.ToServiceResult();
}

return ServiceResult<WorkoutTemplateDto>.Success(dataResult.Data!);

// ✅ GOOD - Pattern matching with extension methods and single exit point
var dataResult = await _dataProvider.CreateWorkoutTemplateAsync(validTemplate);

return dataResult switch
{
    { IsSuccess: true } => ServiceResult<WorkoutTemplateDto>.Success(dataResult.Data!),
    { IsSuccess: false } when dataResult.HasError(DataErrorCode.Conflict) 
        => ServiceResult<WorkoutTemplateDto>.Failure(ServiceError.DuplicateName(validTemplate.Name)),
    _ => dataResult.ToServiceResult()
};
```

**Benefits**:
- Single exit point principle maintained
- Clear, declarative intent
- Extension methods (`HasError`) make conditions readable
- All possible outcomes visible in one place
- No scattered returns throughout the method

### 2. **Null Safety First**
Minimize null usage through C# features and patterns:
- Enable nullable reference types in project settings
- Use null-conditional operators (?. and ??)
- Validate at boundaries, not throughout the code
- Document when null is intentionally allowed

```csharp
// ❌ BAD - Null checks everywhere
public void ProcessUser(User? user)
{
    if (user == null) return;
    if (user.Profile == null) return;
    if (user.Profile.Settings == null) return;
    // actual logic
}

// ✅ GOOD - Validate at boundary
public void ProcessUser(User user)
{
    // User is guaranteed to be valid with all required properties
    // Validation happened at API boundary
}
```

### 3. **Method Length and Complexity**
- **Target**: Methods < 20 lines
- **Maximum cyclomatic complexity**: 10
- Extract complex logic into well-named helper methods
- One level of abstraction per method

### 5. **Single Exit Point Principle**
**CRITICAL**: Every method should have ONE exit point at the end:

```csharp
// ❌ BAD - Multiple returns scattered throughout
public decimal CalculateDiscount(Customer customer, decimal amount)
{
    if (!customer.IsActive) return 0;
    
    if (customer.Tier == "gold")
    {
        if (amount > 100) return amount * 0.2m;
        return amount * 0.1m;
    }
    
    if (customer.Tier == "silver") return amount * 0.05m;
    
    return 0;
}

// ✅ GOOD - Single exit point
public decimal CalculateDiscount(Customer customer, decimal amount)
{
    decimal discount = 0;
    
    if (customer.IsActive)
    {
        discount = customer.Tier switch
        {
            "gold" => amount > 100 ? amount * 0.2m : amount * 0.1m,
            "silver" => amount * 0.05m,
            _ => 0
        };
    }
    
    return discount;
}
```

### 6. **Defensive Programming Balance**
- Validate at system boundaries (API inputs, user inputs)
- Trust internal components and frameworks
- Don't duplicate validation at every layer
- Let infrastructure failures fail fast

---

## 🛠️ C# Implementation Standards

### 1. **Async/Await Best Practices**

#### Avoid Fake Async
Don't create async methods that don't actually perform async operations:

```csharp
// ❌ BAD - Fake async
public async Task<string> GetNameAsync()
{
    return "John"; // No actual async operation
}

// ✅ GOOD - Synchronous when appropriate
public string GetName()
{
    return "John";
}
```

#### Document Sync-in-Async
When interface requires async but implementation is sync:

```csharp
/// <summary>
/// Returns user preference from in-memory cache.
/// </summary>
/// <remarks>
/// Method is synchronous but returns Task for interface compatibility.
/// Will become truly async if we switch to distributed cache.
/// </remarks>
public async Task<string> GetPreferenceAsync(string key)
{
    return await Task.FromResult(_cache.Get(key));
}
```

### 2. **Error Handling Philosophy**

#### Only Catch What You Can Handle
```csharp
// ❌ BAD - Catching and hiding errors
try
{
    var data = await FetchUserDataAsync(userId);
    return ProcessData(data);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error fetching user data");
    return null; // Hiding the actual problem
}

// ✅ GOOD - Let unhandleable errors propagate
var data = await FetchUserDataAsync(userId); // Let network errors bubble up
return ProcessData(data);
```

#### Validate Preconditions
Check conditions before operations rather than catching failures:

```csharp
// ❌ BAD - Waiting for exception
try
{
    var user = users[userId];
    user.Update(data);
}
catch (KeyNotFoundException)
{
    return new Error("User not found");
}

// ✅ GOOD - Check precondition
if (!users.ContainsKey(userId))
{
    return new Error("User not found");
}

var user = users[userId];
user.Update(data);
```

### 3. **C# Naming Conventions**

#### Standard C# Naming Rules
- **Classes/Types**: PascalCase (`UserAccount`, `OrderService`)
- **Methods/Properties**: PascalCase (`GetUserById`, `CalculateTotal`)
- **Parameters/Variables**: camelCase (`userId`, `totalAmount`)
- **Private fields**: Leading underscore + camelCase (`_internalCache`, `_processQueue`)
- **Constants**: PascalCase (`MaxRetryCount`, `DefaultTimeout`)

#### Descriptive Names
- Name should convey intent, not implementation
- Avoid abbreviations except well-known ones
- Boolean names should be questions (`isActive`, `hasPermission`)
- Async methods should end with Async suffix (`GetUserAsync`, `LoadDataAsync`)

### 4. **Namespace Usage and DI Registration**

#### Use Using Statements Over Fully Qualified Names
Always prefer using statements at the top of files rather than fully qualified type names:

```csharp
// ❌ BAD - Fully qualified names make code verbose and hard to read
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.ITableComponentRegistry, 
    GetFitterGetBigger.Admin.Services.TableComponentRegistry>();
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.TableComponentStrategies.ITableComponentStrategy, 
    GetFitterGetBigger.Admin.Services.TableComponentStrategies.EquipmentTableStrategy>();

// ✅ GOOD - Clean and readable with using statements
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.TableComponentStrategies;

builder.Services.AddScoped<ITableComponentRegistry, TableComponentRegistry>();
builder.Services.AddScoped<ITableComponentStrategy, EquipmentTableStrategy>();
```

**Benefits**:
- **Improved readability** - Focus on what's being registered, not namespaces
- **Easier maintenance** - Namespace changes only require updating using statements
- **Consistency** - All registrations follow the same clean pattern
- **Reduced verbosity** - Less horizontal scrolling and visual noise

**Apply this to**:
- Dependency injection registrations
- Type declarations in code
- Generic type parameters
- Any place where types are referenced

---

## 🏗️ Architecture Standards

### 1. **Layered Architecture Pattern**
- **See**: `systemPatterns.md` - "Layered Architecture Pattern" section for complete details
- **Summary**: Strict 4-layer architecture (UI → Business → Data → Transport)
- **Key Rule**: Dependencies flow downward only, never upward
- **Implementation**: All features must follow this pattern

### 2. **Separation of Concerns**
- **Presentation**: UI logic only, no business rules
- **Business Logic**: Core domain logic, framework-agnostic
- **Data Access**: Database/API interactions isolated
- **Cross-Cutting**: Logging, caching, security in separate modules

### 3. **Dependency Direction**
- Dependencies flow inward (outer layers depend on inner)
- Business logic never depends on UI or infrastructure
- Use interfaces/abstractions at boundaries
- Inject dependencies, don't create them

### 4. **Component Size**
- **Single Responsibility**: Each class/module has one reason to change
- **High Cohesion**: Related functionality stays together
- **Low Coupling**: Minimize dependencies between components
- **File Length**: Prefer multiple small files over large ones

### 5. **Extensibility Through Patterns**
Use design patterns to create extensible systems without modifying core code:

```csharp
// ✅ GOOD - Strategy Pattern for Reference Tables
// Adding new reference tables requires NO changes to service code
public class ReferenceDataService
{
    public async Task<IEnumerable<ReferenceDataDto>> GetReferenceDataAsync<T>() 
        where T : IReferenceTableEntity
    {
        // Single generic method handles ALL reference tables
        // New tables added via new strategy classes only
    }
}

// ❌ BAD - Method per table (not scalable)
public class OldReferenceDataService
{
    public async Task<IEnumerable<ReferenceDataDto>> GetBodyParts() { }
    public async Task<IEnumerable<ReferenceDataDto>> GetEquipment() { }
    // Would need 100+ methods for 100+ tables!
}
```

**Pattern Benefits**:
- Open/Closed Principle: Open for extension, closed for modification
- Single exit points maintained in all methods
- Type safety through marker interfaces
- Automatic registration via assembly scanning

### 6. **Avoid God Classes**
God Classes violate Single Responsibility and become maintenance nightmares:

```csharp
// ❌ BAD - God Class with hard-coded mappings
public class TableComponentRegistry
{
    private readonly Dictionary<string, Type> _componentMappings = new()
    {
        ["Equipment"] = typeof(EquipmentTable),
        ["MuscleGroups"] = typeof(MuscleGroupsTable),
        ["BodyParts"] = typeof(GenericTable),
        // ... 100+ hard-coded entries
    };
    
    private readonly Dictionary<string, string> _displayNames = new()
    {
        ["Equipment"] = "Equipment",
        ["MuscleGroups"] = "Muscle Groups",
        // ... 100+ more entries
    };
}

// ✅ GOOD - Strategy Pattern with extensibility
public class TableComponentRegistry
{
    private readonly Dictionary<string, ITableComponentStrategy> _strategies;
    
    public TableComponentRegistry(IEnumerable<ITableComponentStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.TableName);
        // New tables added via new strategy classes, no modification needed
    }
}
```

**God Class Warning Signs**:
- Class grows with each new feature
- Multiple responsibilities in one class
- Hard-coded mappings or configurations
- Switch statements or if-else chains that grow
- Violates Open/Closed Principle

**Refactoring Approach**:
1. Identify the varying behavior (what changes with each new entry)
2. Extract interface for that behavior
3. Create strategy/handler classes for each variation
4. Use dependency injection to provide all strategies
5. Registry/coordinator class just orchestrates, doesn't contain logic

---

## 📊 Code Review Checklist

### ✅ C# Code Review Checks

#### Code Quality
- [ ] Methods are focused and < 20 lines
- [ ] Single exit point per method (with rare justified exceptions)
- [ ] Pattern matching used instead of if-else chains
- [ ] Extension methods used for repeated patterns
- [ ] No code duplication (DRY principle)
- [ ] Clear, descriptive naming
- [ ] Appropriate comments for complex logic

#### Error Handling
- [ ] Only catching handleable exceptions
- [ ] Preconditions validated
- [ ] Errors properly propagated
- [ ] No empty catch blocks
- [ ] No suppressed errors

#### Architecture
- [ ] Follows project architecture patterns
- [ ] No cross-layer violations
- [ ] Dependencies injected properly
- [ ] Single responsibility maintained
- [ ] Proper abstraction levels

#### Testing
- [ ] New code has xUnit/bUnit tests
- [ ] Tests use proper async/await patterns
- [ ] Mocks created with Moq follow conventions
- [ ] Edge cases covered
- [ ] No magic values - use constants or builders
- [ ] Tests follow AAA pattern (Arrange, Act, Assert)

#### Performance
- [ ] No obvious performance issues (avoid LINQ in hot paths)
- [ ] Appropriate algorithm complexity
- [ ] IDisposable resources properly disposed (using statements)
- [ ] No memory leaks (weak references where appropriate)
- [ ] Async/await used properly (ConfigureAwait where needed)
- [ ] String operations use StringBuilder for concatenation in loops

---

## 📚 C# Best Practices

### 1. **Immutability Where Possible**
- Use `readonly` fields and properties
- Prefer immutable collections (ImmutableList, ImmutableDictionary)
- Use `init` accessors for properties (C# 9+)
- Return new objects rather than modifying
- Document when mutation is intentional

### 2. **Composition Over Inheritance**
- Favor composition for code reuse
- Use inheritance for true "is-a" relationships
- Keep inheritance hierarchies shallow
- Prefer interfaces over abstract base classes

### 3. **Fail Fast**
- Validate inputs early
- Throw meaningful errors immediately
- Don't continue with invalid state
- Make illegal states unrepresentable

### 4. **Configuration Management**
- Use `IConfiguration` and Options pattern
- Strongly-typed configuration classes
- Validate configuration at startup with `IValidateOptions<T>`
- Use `appsettings.{Environment}.json` for environment-specific settings
- Document all configuration options

### 5. **C#-Specific Patterns**

#### Use Extension Methods for Cleaner Code
Create extension methods to make complex conditions more readable:

```csharp
// ❌ BAD - Complex lambda expressions in pattern matching
return dataResult switch
{
    { IsSuccess: false } when dataResult.Errors.Any(e => e.Code == DataErrorCode.Conflict) 
        => ServiceResult<T>.Failure(ServiceError.DuplicateName(name)),
    _ => dataResult.ToServiceResult()
};

// ✅ GOOD - Extension method for cleaner syntax
// Extension method definition:
public static bool HasError<T>(this DataServiceResult<T> result, DataErrorCode errorCode)
{
    return result.Errors.Any(e => e.Code.Equals(errorCode));
}

// Usage:
return dataResult switch
{
    { IsSuccess: false } when dataResult.HasError(DataErrorCode.Conflict) 
        => ServiceResult<T>.Failure(ServiceError.DuplicateName(name)),
    _ => dataResult.ToServiceResult()
};
```

**Guidelines for Extension Methods**:
- Create them for commonly repeated patterns
- Name them to express intent clearly (`HasError`, `IsEmpty`, `ContainsAny`)
- Keep them focused on a single responsibility
- Place them in a logical namespace (e.g., `Extensions` folder)
- Document their purpose with XML comments

#### Use Records for DTOs
```csharp
// ✅ GOOD - Using records for immutable DTOs
public record UserDto(string Id, string Name, string Email);

// ❌ BAD - Mutable class for DTO
public class UserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

#### Leverage using Statements
```csharp
// ✅ GOOD - Using declaration (C# 8+)
using var connection = new SqlConnection(connectionString);
var result = await connection.QueryAsync<User>(query);

// ❌ BAD - Manual disposal
SqlConnection connection = null;
try
{
    connection = new SqlConnection(connectionString);
    var result = await connection.QueryAsync<User>(query);
}
finally
{
    connection?.Dispose();
}
```

#### Null-Forgiving Operator Usage
```csharp
// ✅ GOOD - Only when you're certain value is not null
public void ProcessVerifiedUser(string? userId)
{
    // After validation, we know it's not null
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentException("User ID is required");
    
    var user = GetUser(userId!); // Safe to use ! here
}

// ❌ BAD - Using ! to silence warnings without validation
public void ProcessUser(string? userId)
{
    var user = GetUser(userId!); // Dangerous - could be null!
}
```

---

## 🔗 Related Documents

- `CODE_REVIEW_PROCESS.md` - How to conduct code reviews
- `UNIFIED_DEVELOPMENT_PROCESS.md` - Overall development workflow
- Project-specific standards:
  - `API-CODE_QUALITY_STANDARDS.md` - API-specific standards
  - `ADMIN-CODE_QUALITY_STANDARDS.md` - Admin Blazor-specific standards
  - `patterns/BLAZOR-NAVIGATION-BEST-PRACTICES.md` - Blazor navigation patterns and best practices
  - `patterns/blazor-shouldrender-optimization-pattern.md` - ShouldRender optimization techniques for Blazor components
  - `patterns/comprehensive-blazor-testing-patterns.md` - Comprehensive testing patterns for Blazor applications
  - `guides/accessibility-automation-guide.md` - Automated accessibility testing implementation guide

---

## 💡 Remember

> "Any fool can write code that a computer can understand. Good programmers write code that humans can understand." - Martin Fowler

These standards exist to create maintainable, understandable code that serves as a positive example for the entire team.