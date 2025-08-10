# Service Repository Boundaries - Single Repository Rule

**🎯 PURPOSE**: This document defines the **MANDATORY** architectural rule that maintains clear domain boundaries and prevents tight coupling between services.

## 🚨 CRITICAL: The Single Repository Rule

**Each service MUST only access its own repository directly. Cross-domain data access MUST use service dependencies.**

This is a **fundamental architectural rule** that prevents tight coupling and maintains clear domain boundaries.

## The Problem - Repository Boundary Violations

### ❌ CRITICAL VIOLATION Example

```csharp
// ❌ VIOLATION - WorkoutTemplateService accessing other repositories directly
public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    
    // VIOLATION: Accessing IExerciseRepository directly
    var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>();
    
    // VIOLATION: Accessing IWorkoutTemplateExerciseRepository directly  
    var templateExerciseRepository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
    
    // This breaks domain boundaries and creates tight coupling!
}
```

**Why This Is Critical:**
- **Domain Boundary Violation**: WorkoutTemplateService should only access `IWorkoutTemplateRepository`
- **Tight Coupling**: Creates dependencies on repository implementations outside the service's domain
- **Transaction Complexity**: Makes cross-domain transaction management difficult
- **Maintainability**: Violates separation of concerns and makes refactoring harder

## The Solution - Service-to-Service Communication

### ✅ CORRECT PATTERN

```csharp
// ✅ CORRECT - Use service dependencies for cross-domain access
public class WorkoutTemplateService : IWorkoutTemplateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseService _exerciseService; // Service dependency
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService; // Service dependency

    public WorkoutTemplateService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseService exerciseService,
        IWorkoutTemplateExerciseService workoutTemplateExerciseService)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseService = exerciseService;
        _workoutTemplateExerciseService = workoutTemplateExerciseService;
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
    {
        // Only access own repository
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await repository.GetByIdAsync(id);
        if (template.IsEmpty)
            return ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                new List<ExerciseDto>(), 
                ServiceError.NotFound());

        // Use service dependencies for cross-domain operations
        var existingExercisesResult = await _workoutTemplateExerciseService.GetByTemplateIdAsync(id);
        if (!existingExercisesResult.IsSuccess)
            return ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                new List<ExerciseDto>(), 
                existingExercisesResult.Errors);

        var suggestedExercisesResult = await _exerciseService.GetSuggestedForTemplateAsync(
            template.CategoryId, 
            existingExercisesResult.Data.Select(e => e.ExerciseId));
            
        return suggestedExercisesResult;
    }
}
```

## Service Repository Boundary Rules

### ✅ ALLOWED Repository Access

- `WorkoutTemplateService` → `IWorkoutTemplateRepository` ✅
- `ExerciseService` → `IExerciseRepository` ✅  
- `WorkoutTemplateExerciseService` → `IWorkoutTemplateExerciseRepository` ✅
- `MuscleGroupService` → `IMuscleGroupRepository` ✅
- `EquipmentService` → `IEquipmentRepository` ✅

### ❌ FORBIDDEN Repository Access

- `WorkoutTemplateService` → `IExerciseRepository` ❌
- `WorkoutTemplateService` → `IWorkoutTemplateExerciseRepository` ❌
- `ExerciseService` → `IWorkoutTemplateRepository` ❌
- `MuscleGroupService` → `IBodyPartRepository` ❌
- Any service → Another service's repository ❌

## Service-to-Service Communication Patterns

### Pattern 1: Validation Across Domains

```csharp
// ✅ CORRECT - Service validates existence via other service
public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command)
{
    // Validate category exists via CategoryService
    var categoryExists = await _categoryService.ExistsAsync(command.CategoryId);
    if (!categoryExists)
        return ServiceResult<WorkoutTemplateDto>.Failure(
            WorkoutTemplateDto.Empty,
            ServiceError.InvalidReference("CategoryId", command.CategoryId));

    // Only then access own repository
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    // ... continue with creation
}
```

### Pattern 2: Cross-Domain Queries

```csharp
// ✅ CORRECT - Service queries other domains via service layer
public async Task<ServiceResult<WorkoutSummaryDto>> GetWorkoutSummaryAsync(WorkoutTemplateId id)
{
    // Get own domain data
    var templateResult = await GetByIdAsync(id);
    if (!templateResult.IsSuccess)
        return ServiceResult<WorkoutSummaryDto>.Failure(WorkoutSummaryDto.Empty, templateResult.Errors);

    // Get related domain data via services
    var exercisesResult = await _workoutTemplateExerciseService.GetByTemplateIdAsync(id);
    var equipmentResult = await _exerciseService.GetRequiredEquipmentAsync(
        exercisesResult.Data.Select(e => e.ExerciseId));

    // Combine and return
    return ServiceResult<WorkoutSummaryDto>.Success(new WorkoutSummaryDto
    {
        Template = templateResult.Data,
        Exercises = exercisesResult.Data,
        RequiredEquipment = equipmentResult.Data
    });
}
```

### Pattern 3: Aggregate Operations

```csharp
// ✅ CORRECT - Orchestrating multiple services for complex operations
public async Task<ServiceResult<WorkoutPlanDto>> CreateCompleteWorkoutPlanAsync(CreateWorkoutPlanCommand command)
{
    // Validate all references via their respective services
    var templateResult = await _workoutTemplateService.GetByIdAsync(command.TemplateId);
    if (!templateResult.IsSuccess)
        return ServiceResult<WorkoutPlanDto>.Failure(WorkoutPlanDto.Empty, templateResult.Errors);

    var userResult = await _userService.GetByIdAsync(command.UserId);
    if (!userResult.IsSuccess)
        return ServiceResult<WorkoutPlanDto>.Failure(WorkoutPlanDto.Empty, userResult.Errors);

    // Create in own domain
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutPlanRepository>();
    // ... create workout plan
}
```

## Violation Detection Guide

### How to Detect Repository Boundary Violations

**1. Search for Cross-Repository Access Patterns:**
```bash
# Search for services accessing multiple repositories
grep -r "GetRepository<I.*Repository>" --include="*Service.cs" Services/Implementations/
```

**2. Look for These Anti-Patterns:**
```csharp
// ❌ RED FLAGS in Service constructors
public SomeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    // NO other repository interfaces should be injected!
    IOtherRepository otherRepository  // ← VIOLATION
) 

// ❌ RED FLAGS in Service methods
using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
var otherRepository = unitOfWork.GetRepository<IOtherRepository>(); // ← VIOLATION
```

**3. Service Naming vs Repository Access Validation:**
- `WorkoutTemplateService` should ONLY access `IWorkoutTemplateRepository`
- `ExerciseService` should ONLY access `IExerciseRepository`
- Any deviation is a violation

## Step-by-Step Refactoring Guide

### Step 1: Identify the Violation

```csharp
// ❌ CURRENT VIOLATION in WorkoutTemplateService
public async Task<ServiceResult<List<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var exerciseRepository = unitOfWork.GetRepository<IExerciseRepository>(); // ← VIOLATION
    var templateExerciseRepository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>(); // ← VIOLATION
}
```

### Step 2: Add Service Dependencies

```csharp
// ✅ REFACTOR - Add service dependencies to constructor
public class WorkoutTemplateService : IWorkoutTemplateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseService _exerciseService; // ← ADD
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService; // ← ADD

    public WorkoutTemplateService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IExerciseService exerciseService, // ← ADD
        IWorkoutTemplateExerciseService workoutTemplateExerciseService) // ← ADD
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _exerciseService = exerciseService; // ← ADD
        _workoutTemplateExerciseService = workoutTemplateExerciseService; // ← ADD
    }
}
```

### Step 3: Refactor Method Implementation

```csharp
// ✅ REFACTOR - Use service dependencies instead of repositories
public async Task<ServiceResult<List<ExerciseDto>>> GetSuggestedExercisesAsync(WorkoutTemplateId id)
{
    // Only access own repository
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    
    var template = await repository.GetByIdAsync(id);
    if (template.IsEmpty)
        return ServiceResult<List<ExerciseDto>>.Failure(
            new List<ExerciseDto>(), 
            ServiceError.NotFound());

    // Use service dependencies for cross-domain operations
    var existingExercisesResult = await _workoutTemplateExerciseService.GetByTemplateIdAsync(id);
    if (!existingExercisesResult.IsSuccess)
        return ServiceResult<List<ExerciseDto>>.Failure(
            new List<ExerciseDto>(), 
            existingExercisesResult.Errors);

    var suggestedExercisesResult = await _exerciseService.GetSuggestedForTemplateAsync(
        template.CategoryId, 
        existingExercisesResult.Data.Select(e => e.ExerciseId));
        
    return suggestedExercisesResult;
}
```

### Step 4: Update Dependency Injection Registration

```csharp
// Ensure all service dependencies are registered in Program.cs or Startup.cs
services.AddScoped<IWorkoutTemplateService, WorkoutTemplateService>();
services.AddScoped<IExerciseService, ExerciseService>(); // ← ENSURE REGISTERED
services.AddScoped<IWorkoutTemplateExerciseService, WorkoutTemplateExerciseService>(); // ← ENSURE REGISTERED
```

### Step 5: Update Tests

```csharp
// Update unit tests to mock the new service dependencies
public class WorkoutTemplateServiceTests
{
    private readonly Mock<IExerciseService> _mockExerciseService; // ← ADD
    private readonly Mock<IWorkoutTemplateExerciseService> _mockWorkoutTemplateExerciseService; // ← ADD

    public WorkoutTemplateServiceTests()
    {
        _mockExerciseService = new Mock<IExerciseService>(); // ← ADD
        _mockWorkoutTemplateExerciseService = new Mock<IWorkoutTemplateExerciseService>(); // ← ADD
        
        _service = new WorkoutTemplateService(
            _mockUnitOfWorkProvider.Object,
            _mockExerciseService.Object, // ← ADD
            _mockWorkoutTemplateExerciseService.Object); // ← ADD
    }
}
```

## Validation Checklist After Refactoring

- [ ] Service constructor only injects IUnitOfWorkProvider and other services (no repositories)
- [ ] Service methods only call `GetRepository<IOwnRepository>()`
- [ ] Cross-domain operations use injected service dependencies
- [ ] All service dependencies are registered in DI container
- [ ] Unit tests mock all service dependencies
- [ ] Build passes with zero errors and warnings

## Benefits of This Pattern

1. **Clear Domain Boundaries**: Each service owns its domain completely
2. **Better Testability**: Services can be tested in isolation with mocked dependencies
3. **Easier Refactoring**: Changes to one repository don't affect other services
4. **Transaction Management**: Each service manages its own transactions
5. **Reduced Coupling**: Services depend on interfaces, not implementations
6. **Better Scalability**: Services can be moved to microservices if needed

## Key Principle

> "Single Repository Rule: Each service only accesses its own repository directly. Service Dependencies: Services depend on other services, not repositories."

## Related Documentation

- `/memory-bank/systemPatterns.md` - Lines 118-136 for comprehensive service-to-service communication patterns
- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/unitOfWorkPattern.md` - Unit of Work pattern details