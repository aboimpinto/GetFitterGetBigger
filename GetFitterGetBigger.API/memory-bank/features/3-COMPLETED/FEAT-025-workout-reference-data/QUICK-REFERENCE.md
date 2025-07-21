# Quick Reference: Workout Reference Data API

## API Endpoints

### Workout Objectives
```bash
# Get all workout objectives
GET /api/workout-objectives
GET /api/workout-objectives?includeInactive=true

# Get specific workout objective
GET /api/workout-objectives/{id}

# Example Response:
{
  "workoutObjectiveId": "10000001-0001-0001-0001-000000000001",
  "value": "Muscular Strength",
  "description": "Build maximum strength through progressive overload",
  "displayOrder": 1,
  "isActive": true
}
```

### Workout Categories
```bash
# Get all workout categories
GET /api/workout-categories
GET /api/workout-categories?includeInactive=true

# Get specific workout category
GET /api/workout-categories/{id}

# Example Response:
{
  "workoutCategoryId": "20000002-0001-0001-0001-000000000001",
  "value": "HIIT",
  "description": "High-Intensity Interval Training workouts",
  "icon": "⚡",
  "color": "#FF6B6B",
  "primaryMuscleGroups": ["Cardio", "Full Body"],
  "displayOrder": 1,
  "isActive": true
}
```

### Execution Protocols
```bash
# Get all execution protocols
GET /api/execution-protocols
GET /api/execution-protocols?includeInactive=true

# Get specific execution protocol
GET /api/execution-protocols/{id}

# Get by code (case-insensitive)
GET /api/execution-protocols/by-code/STANDARD
GET /api/execution-protocols/by-code/amrap

# Example Response:
{
  "executionProtocolId": "30000003-0001-0001-0001-000000000001",
  "code": "STANDARD",
  "value": "Standard",
  "description": "Traditional set and rep execution",
  "timeBase": false,
  "repBase": true,
  "restPattern": "Fixed rest between sets",
  "intensityLevel": "Variable",
  "displayOrder": 1,
  "isActive": true
}
```

## Available Reference Data

### Workout Objectives (7 total)
1. Muscular Strength
2. Hypertrophy
3. Power
4. Muscular Endurance
5. Cardiovascular Conditioning
6. Flexibility & Mobility
7. General Fitness

### Workout Categories (8 total)
1. HIIT (⚡)
2. Arms (💪)
3. Legs (🦵)
4. Abs & Core (🎯)
5. Shoulders (🏋️)
6. Back (🔙)
7. Chest (💎)
8. Full Body (🏃)

### Execution Protocols (8 total)
1. STANDARD - Traditional sets and reps
2. AMRAP - As Many Reps As Possible
3. EMOM - Every Minute On the Minute
4. FOR_TIME - Complete as fast as possible
5. TABATA - 20 seconds on, 10 seconds off
6. CLUSTER - Mini-sets with short rest
7. DROP_SET - Reduce weight, continue reps
8. REST_PAUSE - Brief pause, then continue

## Service Layer Usage

### Dependency Injection
```csharp
public class MyService
{
    private readonly IWorkoutObjectiveService _objectiveService;
    private readonly IWorkoutCategoryService _categoryService;
    private readonly IExecutionProtocolService _protocolService;

    public MyService(
        IWorkoutObjectiveService objectiveService,
        IWorkoutCategoryService categoryService,
        IExecutionProtocolService protocolService)
    {
        _objectiveService = objectiveService;
        _categoryService = categoryService;
        _protocolService = protocolService;
    }
}
```

### Service Methods
```csharp
// Get all objectives
var result = await _objectiveService.GetAllAsync(includeInactive: false);
if (result.IsSuccess)
{
    var objectives = result.Data;
}

// Get specific category
var categoryResult = await _categoryService.GetByIdAsync("20000002-0001-0001-0001-000000000001");

// Get protocol by code
var protocolResult = await _protocolService.GetByCodeAsync("AMRAP");
```

## Empty Pattern Handling

### Parsing IDs Safely
```csharp
// Safe parsing with Empty fallback
var objectiveId = WorkoutObjectiveId.ParseOrEmpty(userInput);
if (objectiveId.IsEmpty)
{
    // Handle invalid ID
}

// Direct parsing (throws on invalid)
var categoryId = WorkoutCategoryId.From("20000002-0001-0001-0001-000000000001");
```

### Empty Entities
```csharp
// All entities have Empty singleton
var emptyObjective = WorkoutObjective.Empty;
var emptyCategory = WorkoutCategory.Empty;
var emptyProtocol = ExecutionProtocol.Empty;

// Services return Empty instead of null
var result = await service.GetByIdAsync("invalid-id");
// result.Data will be Empty DTO, not null
```

## Testing Patterns

### Using Test Builders
```csharp
var objective = new WorkoutObjectiveTestBuilder()
    .WithValue("Test Objective")
    .WithDescription("Test Description")
    .Build();

var category = new WorkoutCategoryTestBuilder()
    .WithDefaults()
    .WithIcon("🎯")
    .Build();
```

### Using TestIds
```csharp
// Predefined test IDs
var strengthId = TestIds.WorkoutObjectiveIds.MuscularStrength;
var hiitId = TestIds.WorkoutCategoryIds.HIIT;
var standardId = TestIds.ExecutionProtocolIds.Standard;
```

## Cache Keys

Reference data is cached with these keys:
- `"workout-objectives"` - All objectives
- `"workout-objectives:{id}"` - Specific objective
- `"workout-categories"` - All categories
- `"workout-categories:{id}"` - Specific category
- `"execution-protocols"` - All protocols
- `"execution-protocols:{id}"` - Specific protocol
- `"execution-protocols:code:{code}"` - Protocol by code

Cache duration: 365 days (effectively eternal for reference data)

## Common Patterns

### Controller Error Handling
```csharp
var result = await _service.GetByIdAsync(id);
return result.Match(
    success: data => Ok(data),
    failure: error => error.Code switch
    {
        ServiceErrorCode.NotFound => NotFound(error.Message),
        ServiceErrorCode.ValidationError => BadRequest(error.Message),
        _ => StatusCode(500, "An error occurred")
    }
);
```

### Service Result Pattern
```csharp
// Success case
return ServiceResult<WorkoutObjectiveDto>.Success(dto);

// Failure case
return ServiceResult<WorkoutObjectiveDto>.Failure(
    ServiceErrorCode.NotFound,
    $"Workout objective with ID '{id}' not found"
);
```

## Migration Support

### Database Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Seed Data Location
Seed data is defined in:
- `FitnessDbContext.OnModelCreating()`
- `SeedDataBuilder` class for test data

## Troubleshooting

### Common Issues

1. **404 Not Found**: Check ID format matches pattern (e.g., "10000001-xxxx")
2. **401 Unauthorized**: Ensure JWT token includes Free-Tier claim minimum
3. **Empty Results**: Check includeInactive parameter if expecting inactive items
4. **Case Sensitivity**: Execution protocol codes are case-insensitive

### Debug Helpers
```csharp
// Log cache operations
services.AddSingleton<ICacheService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<CacheService>>();
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    return new CacheService(memoryCache, logger);
});
```

## Quick Command Reference

### Test the API
```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=WorkoutReferenceData"

# Check API health
curl http://localhost:5214/health
```

### View in Swagger
Navigate to: http://localhost:5214/swagger

Look for:
- Workout Objectives section
- Workout Categories section
- Execution Protocols section