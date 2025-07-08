# Service Layer Patterns

## Overview
This document defines the patterns and best practices for the service layer in the GetFitterGetBigger API. These patterns were established as part of FEAT-020 to ensure consistent and maintainable service implementations.

## Core Principles

### 1. Single Repository Rule
Each service is responsible for managing one and only one entity type, and therefore should only directly access its corresponding repository.

**Example:**
```csharp
public class MuscleGroupService : IMuscleGroupService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IBodyPartService _bodyPartService; // Service dependency, not repository
    
    // Constructor only receives service dependencies, not repositories
}
```

### 2. Service-to-Service Communication
When a service needs to access data from another entity, it must use the corresponding service rather than accessing the repository directly.

**Before (Violation):**
```csharp
// In MuscleGroupService - DON'T DO THIS
var bodyPartRepository = unitOfWork.GetRepository<IBodyPartRepository>();
var bodyPart = await bodyPartRepository.GetByIdAsync(bodyPartId);
```

**After (Correct):**
```csharp
// In MuscleGroupService - DO THIS
if (!await _bodyPartService.ExistsAsync(bodyPartId))
{
    throw new InvalidOperationException($"Body part with ID {bodyPartId} not found");
}
```

## Service Patterns

### 1. Validation Services
Services that provide validation capabilities for other services.

**Interface Pattern:**
```csharp
public interface IBodyPartService
{
    Task<bool> ExistsAsync(BodyPartId id);
}

public interface IExerciseTypeService
{
    Task<bool> ExistsAsync(ExerciseTypeId id);
    Task<bool> AllExistAsync(IEnumerable<string> ids);
    Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids);
}
```

**Implementation Pattern:**
```csharp
public class BodyPartService : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;

    public async Task<bool> ExistsAsync(BodyPartId id)
    {
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IBodyPartRepository>();
        var bodyPart = await repository.GetByIdAsync(id);
        return bodyPart != null && bodyPart.IsActive;
    }
}
```

### 2. Transactional Services
Services that participate in transactions initiated by other services.

**Interface Pattern:**
```csharp
public interface IClaimService
{
    // Note: Accepts IWritableUnitOfWork as parameter for transactional consistency
    Task<ClaimId> CreateUserClaimAsync(UserId userId, string claimType, IWritableUnitOfWork<FitnessDbContext> unitOfWork);
}
```

**Implementation Pattern:**
```csharp
public class ClaimService : IClaimService
{
    public async Task<ClaimId> CreateUserClaimAsync(UserId userId, string claimType, IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        // Use the provided unitOfWork, don't create a new one
        var repository = unitOfWork.GetRepository<IClaimRepository>();
        
        var claim = new Claim
        {
            Id = ClaimId.New(),
            UserId = userId,
            ClaimType = claimType,
            ExpirationDate = null,
            Resource = null
        };
        
        await repository.AddClaimAsync(claim);
        // Note: Don't commit here - let the calling service handle the transaction
        
        return claim.Id;
    }
}
```

**Usage in Calling Service:**
```csharp
public class AuthService : IAuthService
{
    public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var userRepository = unitOfWork.GetRepository<IUserRepository>();

        var user = await userRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            user = new User { Id = UserId.New(), Email = request.Email };
            await userRepository.AddUserAsync(user);
            
            // Pass the same unitOfWork for transactional consistency
            await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
        }

        // Commit once at the end
        await unitOfWork.CommitAsync();
        return new AuthenticationResponse(token, claims);
    }
}
```

### 3. CRUD Services
Services that provide full CRUD operations for their entities.

**Pattern:**
```csharp
public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseTypeService _exerciseTypeService;

    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
    {
        // Use own repository for primary operations
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IExerciseRepository>();
        
        // Use service dependencies for validation
        foreach (var typeId in request.ExerciseTypeIds)
        {
            if (await _exerciseTypeService.ExistsAsync(typeId))
            {
                // Add to exercise
            }
        }
        
        var exercise = await repository.AddAsync(newExercise);
        await unitOfWork.CommitAsync();
        
        return MapToDto(exercise);
    }
}
```

## Common Patterns

### 1. Repository Access Pattern
Always get repositories from UnitOfWork, never inject them directly:

```csharp
// DON'T DO THIS
private readonly IBodyPartRepository _repository; // ❌ Direct injection

// DO THIS
using var uow = _unitOfWorkProvider.CreateReadOnly();
var repository = uow.GetRepository<IBodyPartRepository>(); // ✅ From UnitOfWork
```

### 2. Read vs Write Operations
Choose the appropriate UnitOfWork type:

```csharp
// For read operations
using var uow = _unitOfWorkProvider.CreateReadOnly();

// For write operations
using var uow = _unitOfWorkProvider.CreateWritable();
// ... make changes ...
await uow.CommitAsync();
```

### 3. Service Dependency Injection
Services should be injected as dependencies, not repositories:

```csharp
public class MuscleGroupService : IMuscleGroupService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IBodyPartService _bodyPartService; // ✅ Service dependency
    // NOT: private readonly IBodyPartRepository _bodyPartRepository; ❌
    
    public MuscleGroupService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IBodyPartService bodyPartService)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _bodyPartService = bodyPartService;
    }
}
```

## Testing Patterns

### 1. Mocking Services
When testing services that depend on other services:

```csharp
[Fact]
public async Task CreateAsync_ValidatesBodyPartExists()
{
    // Arrange
    var mockBodyPartService = new Mock<IBodyPartService>();
    mockBodyPartService
        .Setup(s => s.ExistsAsync(It.IsAny<BodyPartId>()))
        .ReturnsAsync(true);
    
    var service = new MuscleGroupService(_mockUnitOfWorkProvider.Object, mockBodyPartService.Object);
    
    // Act & Assert
}
```

### 2. Transactional Testing
For services that accept UnitOfWork parameters:

```csharp
[Fact]
public async Task CreateUserClaimAsync_UsesProvidedUnitOfWork()
{
    // Arrange
    var mockUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
    var mockRepository = new Mock<IClaimRepository>();
    
    mockUnitOfWork
        .Setup(uow => uow.GetRepository<IClaimRepository>())
        .Returns(mockRepository.Object);
    
    // Act
    await _claimService.CreateUserClaimAsync(userId, "Free-Tier", mockUnitOfWork.Object);
    
    // Assert
    mockRepository.Verify(r => r.AddClaimAsync(It.IsAny<Claim>()), Times.Once);
    // Note: Don't verify CommitAsync - that's the caller's responsibility
}
```

## Benefits

1. **Separation of Concerns**: Each service is responsible for one entity type
2. **Testability**: Services can be tested in isolation with mocked dependencies
3. **Flexibility**: Implementation details can change without affecting consumers
4. **Consistency**: All services follow the same patterns
5. **Transaction Management**: Clear patterns for transactional operations
6. **Maintainability**: Easy to understand and modify individual services

## Migration Guide

When refactoring existing services to follow these patterns:

1. **Identify Repository Dependencies**: Find all repositories used by the service
2. **Create Service Interfaces**: For each external repository, identify or create the corresponding service
3. **Replace Repository Calls**: Change direct repository access to service calls
4. **Update Constructor**: Remove repository dependencies, add service dependencies
5. **Update Tests**: Mock services instead of repositories
6. **Verify Behavior**: Ensure all tests still pass

## See Also

- [Architecture Refactoring Initiative](./ARCHITECTURE-REFACTORING-INITIATIVE.md)
- [System Patterns](./systemPatterns.md)
- [Unit of Work Pattern](./unitOfWorkPattern.md)