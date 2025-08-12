# Unit Testing with AutoMocker and FluentAssertions

## Overview
This guide documents our modern approach to unit testing using AutoMocker and FluentAssertions, focusing on isolation, clarity, and maintainability.

## Why the Change?

### Problems with Traditional Testing
```csharp
// ❌ OLD WAY - Shared state, manual mocking, verbose setup
public class ServiceTests
{
    private readonly Mock<IRepository> _mockRepo;
    private readonly Mock<ICache> _mockCache;
    private readonly Service _service;
    
    public ServiceTests()
    {
        _mockRepo = new Mock<IRepository>();
        _mockCache = new Mock<ICache>();
        _mockLogger = new Mock<ILogger>();
        // ... 10 more mocks
        _service = new Service(_mockRepo.Object, _mockCache.Object, ...);
    }
}
```

**Issues:**
- **Shared State** - Tests can affect each other, causing flaky tests
- **Manual Dependency Management** - Adding a dependency breaks all tests
- **Verbose Setup** - Lots of boilerplate for each mock
- **Poor Readability** - Hard to see what's being tested
- **Maintenance Nightmare** - Changes cascade through all tests

### The AutoMocker Solution
```csharp
// ✅ NEW WAY - Isolated, automatic mocking, minimal setup
[Fact]
public async Task TestMethod()
{
    // Arrange
    var automocker = new AutoMocker();
    var testee = automocker.CreateInstance<Service>();
    
    // Setup only what you need
    automocker.SetupRepository().SetupCacheMiss();
    
    // Act & Assert...
}
```

## Core Principles

### 1. ISOLATION Above All Else
**Every test gets its own AutoMocker instance**
```csharp
[Fact]
public async Task Test1()
{
    var automocker = new AutoMocker(); // Isolated instance
    var testee = automocker.CreateInstance<Service>();
}

[Fact]
public async Task Test2()
{
    var automocker = new AutoMocker(); // Different isolated instance
    var testee = automocker.CreateInstance<Service>();
}
```

**Why?** Prevents test pollution, makes tests independent, enables parallel execution

### 2. Minimal Setup - Only What You Need
```csharp
// ❌ BAD - Setting up everything
var bodyPart = new BodyPartBuilder()
    .WithBodyPartId(id)
    .WithValue("Chest")
    .WithDescription("Chest muscles")
    .WithOrder(1)
    .WithIsActive(true)
    .Build();

// ✅ GOOD - Only what's needed for the test
var bodyPart = new BodyPartBuilder()
    .WithBodyPartId(id)  // Only what we're testing
    .Build();

// ✅ GOOD - Set value only if asserting on it
var bodyPart = new BodyPartBuilder()
    .WithBodyPartId(id)
    .WithValue("Chest")  // Need this for assertion
    .Build();
```

### 3. Builder Pattern with Good Defaults
```csharp
public class BodyPartBuilder
{
    // Good defaults that create valid objects
    private BodyPartId _bodyPartId = BodyPartId.Empty;
    private string _value = "Test value";
    private string _description = "Test description";
    private bool _active = true;
    private int _order = 1;
    
    // Only set what you need
    public BodyPartBuilder WithBodyPartId(BodyPartId id) { ... }
    public BodyPartBuilder WithInactiveFlag() { ... }
    
    public BodyPart Build() { ... }
}
```

### 4. Fluent Extension Methods for Setup
```csharp
// Chain setup methods for clarity
automocker
    .SetupBodyPartUnitOfWork()
    .SetupCacheMiss<BodyPartDto>()
    .SetupBodyPartGetById(bodyPart);
```

### 5. Explicit Verification
```csharp
// ❌ BAD - Verify with Any() loses precision
automocker.Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Once);

// ✅ GOOD - Verify exact values
automocker
    .VerifyReadOnlyUnitOfWorkCreatedOnce()
    .VerifyBodyPartGetByIdOnce(providedBodyPartId);  // Exact ID
```

## Service-Specific Extension Pattern

### ⚠️ CRITICAL: One Extension Class Per Service

**PROBLEM**: As we add more services and tests, having all AutoMocker extensions in a single file becomes unmanageable. A single `AutoMockerExtensions.cs` file with extensions for 20+ services would be thousands of lines long.

**SOLUTION**: Create service-specific extension classes following this pattern:

```
GetFitterGetBigger.API.Tests/
├── Services/
│   ├── Extensions/
│   │   ├── AutoMockerExtensions.cs              # Shared/common extensions only
│   │   ├── AutoMockerExerciseServiceExtensions.cs    # Exercise-specific
│   │   ├── AutoMockerBodyPartServiceExtensions.cs    # BodyPart-specific
│   │   ├── AutoMockerEquipmentServiceExtensions.cs   # Equipment-specific
│   │   └── AutoMockerWorkoutTemplateExtensions.cs    # WorkoutTemplate-specific
│   └── ExerciseServiceTests.cs
```

### Naming Convention

**Pattern**: `AutoMocker{ServiceName}Extensions.cs`

Examples:
- `AutoMockerExerciseServiceExtensions.cs`
- `AutoMockerUserServiceExtensions.cs`
- `AutoMockerWorkoutTemplateServiceExtensions.cs`

### Structure of Service-Specific Extensions

```csharp
namespace GetFitterGetBigger.API.Tests.Services.Extensions;

public static class AutoMockerExerciseServiceExtensions
{
    // Setup methods specific to ExerciseService testing
    public static AutoMocker SetupExerciseUnitOfWork(this AutoMocker mocker)
    {
        // Setup both read-only and writable unit of work for exercises
    }
    
    public static AutoMocker SetupExerciseGetById(this AutoMocker mocker, Exercise returnValue)
    {
        // Setup repository to return specific exercise
    }
    
    public static AutoMocker SetupExerciseTypeServiceIsRestType(this AutoMocker mocker, bool isRestType)
    {
        // Setup exercise type service behavior
    }
    
    // Verification methods - Positive cases
    public static AutoMocker VerifyExerciseAddOnce(this AutoMocker mocker)
    {
        // Verify exercise was added exactly once
    }
    
    // Verification methods - Negative cases (explicit naming!)
    public static AutoMocker VerifyExerciseRepositoryNotAccessedForValidationFailure(this AutoMocker mocker)
    {
        // Clear intent - validation failed so repository shouldn't be touched
    }
}
```

### What Goes in Each Extension File

#### Service-Specific Extension (e.g., AutoMockerExerciseServiceExtensions)
- Setup methods for that service's repository
- Setup methods for that service's dependencies
- Verification methods specific to that service
- Helper methods for common test scenarios

#### Shared Extensions (AutoMockerExtensions.cs)
- Cross-cutting concerns (cache, logging)
- Common patterns used by multiple services
- Generic helper methods
- Base infrastructure setup

### Benefits of This Pattern

1. **Scalability**: Each service's test helpers are isolated
2. **Discoverability**: Easy to find extensions for a specific service
3. **Maintainability**: Changes to one service don't affect others
4. **Team Work**: Multiple developers can work on different services without conflicts
5. **IntelliSense**: Better IDE support with smaller, focused files

### Example Usage in Tests

```csharp
using GetFitterGetBigger.API.Tests.Services.Extensions; // Import extensions

public class ExerciseServiceTests
{
    [Fact]
    public async Task CreateAsync_WithNewExerciseName_CreatesAndReturnsExercise()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<ExerciseService>();
        
        const string exerciseName = "Squat";
        
        // Using Exercise-specific extensions
        automocker
            .SetupExerciseUnitOfWork()           // From AutoMockerExerciseServiceExtensions
            .SetupExerciseExists(exerciseName, false)  // From AutoMockerExerciseServiceExtensions
            .SetupCacheMiss<ExerciseDto>()       // From shared AutoMockerExtensions
            .SetupExerciseAdd(createdExercise);  // From AutoMockerExerciseServiceExtensions
        
        // Act
        var result = await testee.CreateAsync(request.ToCommand());
        
        // Assert & Verify using specific extensions
        automocker
            .VerifyExerciseAddOnce()             // From AutoMockerExerciseServiceExtensions
            .VerifyWritableUnitOfWorkCommitOnce(); // From AutoMockerExerciseServiceExtensions
    }
}
```

### Migration Strategy for Existing Tests

When you have a large `AutoMockerExtensions.cs`:

1. **Identify service-specific methods** by looking for method names containing service names
2. **Create new extension class** for each service
3. **Move methods** to appropriate service-specific class
4. **Update using statements** in test files
5. **Keep only shared methods** in the original file

### Checklist for Creating New Service Tests

- [ ] Create `AutoMocker{ServiceName}Extensions.cs` file
- [ ] Add setup methods for the service's repository
- [ ] Add setup methods for the service's dependencies
- [ ] Add positive verification methods (Once, CalledWith)
- [ ] Add negative verification methods (NeverCalled, NotAccessed)
- [ ] Use explicit naming for "Never" methods to show intent
- [ ] Import the extension namespace in test files

## Implementation Pattern

### 1. Test Structure
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var automocker = new AutoMocker();
    var testee = automocker.CreateInstance<ServiceClass>();
    
    var inputData = new DataBuilder()
        .WithRequiredProperty(value)  // Only what's needed
        .Build();
    
    automocker
        .SetupDependency1()
        .SetupDependency2();
    
    // Act
    var result = await testee.MethodUnderTest(inputData);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
    result.Data.Property.Should().Be(expectedValue);
    
    automocker
        .VerifyDependency1Called()
        .VerifyDependency2CalledWith(specificValue);
}
```

### 2. Extension Methods Structure
```csharp
public static class AutoMockerExtensions
{
    // Setup methods
    public static AutoMocker SetupBodyPartUnitOfWork(this AutoMocker mocker)
    {
        var repositoryMock = mocker.GetMock<IBodyPartRepository>();
        // ... setup chain
        return mocker;  // Enable chaining
    }
    
    // Verification methods - Positive cases
    public static AutoMocker VerifyBodyPartGetByIdOnce(
        this AutoMocker mocker, 
        BodyPartId bodyPartId)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetByIdAsync(bodyPartId), Times.Once());
        return mocker;  // Enable chaining
    }
    
    // Verification methods - "Never" cases (explicit intent!)
    public static AutoMocker VerifyBodyPartGetByIdNeverCalled(this AutoMocker mocker)
    {
        mocker.GetMock<IBodyPartRepository>()
            .Verify(x => x.GetByIdAsync(It.IsAny<BodyPartId>()), Times.Never);
        return mocker;
    }
    
    public static AutoMocker VerifyReadOnlyUnitOfWorkNeverCreated(this AutoMocker mocker)
    {
        mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Verify(x => x.CreateReadOnly(), Times.Never);
        return mocker;
    }
}
```

### 3. Explicit Verification Names
**ALWAYS create explicit verification methods for "Never" scenarios:**

```csharp
// ❌ BAD - Not clear what we're verifying
automocker.GetMock<IRepository>()
    .Verify(x => x.GetByIdAsync(It.IsAny<EntityId>()), Times.Never);

// ✅ GOOD - Intent is crystal clear
automocker.VerifyRepositoryGetByIdNeverCalled();

// ✅ GOOD - Even clearer for specific scenarios
automocker.VerifyRepositoryNotAccessedForValidationFailure();
```

**Why?** 
- The method name explains WHY something shouldn't be called
- Makes tests self-documenting
- Easier to understand test intent when reading

## FluentAssertions - Why and How

### Why FluentAssertions?

#### Traditional Assertions (xUnit/NUnit)
```csharp
// ❌ OLD WAY - Less readable, inconsistent API
Assert.True(result.IsSuccess);
Assert.NotNull(result.Data);
Assert.Equal(expected, result.Data.Value);
Assert.Contains("error", result.Errors[0]);
Assert.Throws<ArgumentException>(() => service.Method());
```

#### FluentAssertions
```csharp
// ✅ NEW WAY - Natural language, consistent API
result.IsSuccess.Should().BeTrue();
result.Data.Should().NotBeNull();
result.Data.Value.Should().Be(expected);
result.Errors[0].Should().Contain("error");
service.Invoking(s => s.Method()).Should().Throw<ArgumentException>();
```

### Key Benefits

1. **Readable Like English**
   ```csharp
   // Reads like a sentence
   result.Items.Should().NotBeEmpty()
       .And.HaveCount(3)
       .And.ContainSingle(x => x.Name == "Test");
   ```

2. **Better Failure Messages**
   ```csharp
   // xUnit: "Assert.Equal() Failure\nExpected: 5\nActual: 3"
   // FluentAssertions: "Expected result.Count to be 5, but found 3"
   ```

3. **Chainable Assertions**
   ```csharp
   result.Should()
       .NotBeNull()
       .And.BeOfType<SuccessResult>()
       .Which.Data.Should().Be(expected);
   ```

4. **Rich API for Collections**
   ```csharp
   collection.Should()
       .HaveCount(3)
       .And.ContainInOrder(first, second, third)
       .And.OnlyHaveUniqueItems()
       .And.AllSatisfy(x => x.IsValid.Should().BeTrue());
   ```

### Common FluentAssertions Patterns

#### Basic Assertions
```csharp
// Booleans
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();

// Nullability
result.Should().NotBeNull();
result.Should().BeNull();

// Equality
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Type checking
result.Should().BeOfType<ServiceResult>();
result.Should().BeAssignableTo<IResult>();
```

#### String Assertions
```csharp
message.Should().Be("exact match");
message.Should().BeEmpty();
message.Should().NotBeNullOrWhiteSpace();
message.Should().Contain("substring");
message.Should().StartWith("Error:");
message.Should().EndWith(".");
message.Should().Match("regex.*pattern");
message.Should().BeEquivalentTo("CASE insensitive");
```

#### Numeric Assertions
```csharp
value.Should().Be(42);
value.Should().BeGreaterThan(0);
value.Should().BeLessThanOrEqualTo(100);
value.Should().BeInRange(1, 10);
value.Should().BeCloseTo(3.14f, 0.01f);
```

#### Collection Assertions
```csharp
// Count
items.Should().BeEmpty();
items.Should().NotBeEmpty();
items.Should().HaveCount(5);
items.Should().HaveCountGreaterThan(0);

// Content
items.Should().Contain(expectedItem);
items.Should().ContainSingle();
items.Should().ContainSingle(x => x.Id == id);
items.Should().OnlyContain(x => x.IsValid);

// Order
items.Should().BeInAscendingOrder(x => x.Name);
items.Should().ContainInOrder(first, second, third);

// Equality
items.Should().BeEquivalentTo(expectedItems);
items.Should().Equal(expectedItems);
```

#### Exception Assertions
```csharp
// Simple exception
action.Should().Throw<ArgumentException>();
action.Should().NotThrow();

// With message validation
action.Should()
    .Throw<ArgumentException>()
    .WithMessage("*invalid*");  // Wildcard matching

// Async exceptions
await asyncAction.Should().ThrowAsync<InvalidOperationException>();

// Using Invoking for inline actions
service.Invoking(s => s.Method(null))
    .Should().Throw<ArgumentNullException>()
    .And.ParamName.Should().Be("parameter");
```

#### Async Assertions
```csharp
// Async methods
await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
await func.Should().NotThrowAsync();

// Task results
var result = await GetResultAsync();
result.Should().Be(expected);
```

#### Custom Assertions for Our Domain
```csharp
// ServiceResult assertions
result.IsSuccess.Should().BeTrue("because the operation should succeed");
result.Errors.Should().BeEmpty();
result.Data.Should().NotBeNull();
result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);

// Entity assertions
entity.Id.Should().NotBe(EntityId.Empty);
entity.IsEmpty.Should().BeFalse();
entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

// DTO assertions
dto.Should().BeEquivalentTo(expectedDto, options => 
    options.Excluding(x => x.UpdatedAt));
```

### Best Practices with FluentAssertions

1. **Use Descriptive Reasons**
   ```csharp
   result.IsSuccess.Should().BeTrue(
       because: "valid input should succeed");
   ```

2. **Chain Related Assertions**
   ```csharp
   result.Should()
       .NotBeNull()
       .And.Match<ServiceResult>(r => r.IsSuccess)
       .And.Match(r => r.Errors.Count == 0);
   ```

3. **Use BeEquivalentTo for Object Comparison**
   ```csharp
   // Compares properties, not references
   actualDto.Should().BeEquivalentTo(expectedDto);
   
   // With exclusions
   actualDto.Should().BeEquivalentTo(expectedDto, 
       options => options
           .Excluding(x => x.Id)
           .Excluding(x => x.CreatedAt));
   ```

4. **Leverage Which for Nested Assertions**
   ```csharp
   result.Should().BeOfType<ServiceResult<UserDto>>()
       .Which.Data.Should().NotBeNull()
       .And.Subject.Name.Should().Be("John");
   ```

## Benefits

### Marginal Gains (Day-to-Day)
1. **Faster Test Writing** - AutoMocker handles dependency injection
2. **Less Boilerplate** - Extension methods encapsulate common setups
3. **Better Readability** - Tests show only what matters
4. **Easier Debugging** - Isolated tests = clearer failure causes

### Real Gains (Long-Term)
1. **No More Flaky Tests** - True isolation eliminates shared state bugs
2. **Resilient to Changes** - Adding dependencies doesn't break existing tests
3. **Parallel Execution** - Isolated tests can run simultaneously
4. **Knowledge Transfer** - Consistent patterns make tests self-documenting
5. **Reduced Maintenance** - Changes are localized, not cascading

## Migration Strategy

### No Big Bang Refactoring!
We're NOT doing a massive migration project. Instead:

### The "Touch and Convert" Rule
**When you need to modify a test class:**

1. **First** - Convert to AutoMocker pattern
2. **Verify** - All existing tests still pass
3. **Then** - Add your new tests

### Migration Checklist
```markdown
- [ ] Remove class-level fields
- [ ] Remove constructor
- [ ] Each test gets its own AutoMocker instance
- [ ] Create/update Builder classes with good defaults
- [ ] Create extension methods for common setups
- [ ] Update to FluentAssertions
- [ ] Verify all tests pass
- [ ] Add new tests in the new pattern
```

### Example Migration
```csharp
// BEFORE
public class ServiceTests
{
    private Mock<IRepo> _mockRepo;
    private Service _service;
    
    public ServiceTests()
    {
        _mockRepo = new Mock<IRepo>();
        _service = new Service(_mockRepo.Object);
    }
    
    [Fact]
    public void Test()
    {
        _mockRepo.Setup(x => x.Get()).Returns(data);
        var result = _service.Method();
        Assert.True(result.IsSuccess);
    }
}

// AFTER
public class ServiceTests
{
    [Fact]
    public void Test()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<Service>();
        
        automocker.GetMock<IRepo>()
            .Setup(x => x.Get())
            .Returns(data);
        
        // Act
        var result = testee.Method();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

## Common Patterns

### Pattern 1: Testing Success Cases
```csharp
[Fact]
public async Task GetById_WithValidId_ReturnsSuccess()
{
    // Arrange
    var automocker = new AutoMocker();
    var testee = automocker.CreateInstance<Service>();
    
    var id = EntityId.New();
    var entity = new EntityBuilder()
        .WithId(id)
        .Build();
    
    automocker
        .SetupUnitOfWork()
        .SetupRepositoryGetById(entity);
    
    // Act
    var result = await testee.GetByIdAsync(id);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Id.Should().Be(id.ToString());
}
```

### Pattern 2: Testing Validation Failures
```csharp
[Fact]
public async Task GetById_WithEmptyId_ReturnsValidationFailure()
{
    // Arrange
    var automocker = new AutoMocker();
    var testee = automocker.CreateInstance<Service>();
    
    // Act
    var result = await testee.GetByIdAsync(EntityId.Empty);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
    
    // Verify repository was never called (validation rejects before DB access)
    automocker.VerifyRepositoryGetByIdNeverCalled();
}
```

### Pattern 3: Testing Cache Behavior
```csharp
[Fact]
public async Task GetById_WhenCacheMiss_QueriesDatabase()
{
    // Arrange
    var automocker = new AutoMocker();
    var testee = automocker.CreateInstance<Service>();
    
    var entity = new EntityBuilder().Build();
    
    automocker
        .SetupCacheMiss<EntityDto>()
        .SetupRepositoryGetById(entity);
    
    // Act
    var result = await testee.GetByIdAsync(entity.Id);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    
    automocker
        .VerifyRepositoryCalledOnce()
        .VerifyCacheSetOnce<EntityDto>();
}
```

## Quick Reference

### AutoMocker Basics
```csharp
// Create instance with auto-mocked dependencies
var automocker = new AutoMocker();
var testee = automocker.CreateInstance<ServiceClass>();

// Get a mock to setup/verify
var mock = automocker.GetMock<IDependency>();

// Setup
mock.Setup(x => x.Method()).Returns(value);

// Verify - Positive cases
mock.Verify(x => x.Method(), Times.Once);
mock.Verify(x => x.Method(), Times.Exactly(2));

// Verify - Negative cases (use extension methods!)
automocker.VerifyMethodNeverCalled();  // Explicit intent
automocker.VerifyRepositoryNotAccessed();  // Clear meaning
```

### Extension Method Naming Conventions
```csharp
// Setup methods
SetupUnitOfWork()           // Creates UoW chain
SetupCacheMiss<T>()         // Cache returns miss
SetupRepositoryGetById()    // Repository returns entity

// Positive verifications
VerifyGetByIdOnce()         // Called exactly once
VerifyGetByIdCalledWith()   // Called with specific value

// Negative verifications (EXPLICIT NAMES!)
VerifyGetByIdNeverCalled()              // Never called at all
VerifyRepositoryNotAccessedForEmpty()   // Explains WHY not called
VerifyUnitOfWorkNeverCreated()          // Clear intent
```

### FluentAssertions Basics
```csharp
// Boolean assertions
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();

// Null checks
result.Data.Should().NotBeNull();
result.Data.Should().BeNull();

// Equality
result.Value.Should().Be(expected);
result.Count.Should().Be(5);

// Collections
result.Items.Should().HaveCount(3);
result.Items.Should().Contain(item);
result.Items.Should().BeEmpty();
result.Items.Should().NotBeEmpty();

// Strings
result.Message.Should().Contain("error");
result.Message.Should().StartWith("Error:");

// Exceptions
action.Should().Throw<ArgumentException>();
action.Should().NotThrow();
```

## Remember

> "Isolation, Isolation, Isolation" - Every test must be completely independent

> "Setup only what you need" - If you're not testing it or asserting on it, don't set it

> "Touch and Convert" - When modifying a test class, convert it first, then add new tests

> "Explicit is better than implicit" - Verify exact values, not Any()

> "Good defaults save time" - Builders should create valid objects by default