# Quick Reference - FEAT-026 Workout Template Core

## 🚀 API Endpoints

### Basic CRUD
```bash
# Create template
POST /api/workout-templates
{
  "name": "Upper Body Strength",
  "description": "Focus on compound movements",
  "categoryId": "workoutcategory-20000002-2000-4000-8000-200000000001",
  "difficultyId": "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
  "estimatedDurationMinutes": 60,
  "isPublic": true
}

# Get by ID
GET /api/workout-templates/{id}

# Update template
PUT /api/workout-templates/{id}

# Delete template (soft delete)
DELETE /api/workout-templates/{id}
```

### Query Operations
```bash
# Get paged templates
GET /api/workout-templates?page=1&pageSize=10

# Search by name
GET /api/workout-templates/search?namePattern=upper

# Filter by category
GET /api/workout-templates/filter/category/{categoryId}

# Filter by difficulty  
GET /api/workout-templates/filter/difficulty/{difficultyId}

# Get public templates
GET /api/workout-templates/public
```

### State Management
```bash
# Change state
PUT /api/workout-templates/{id}/state
{
  "newStateId": "workoutstate-02000002-0000-0000-0000-000000000002"
}

# Get all states
GET /api/workout-templates/states
```

### Template Operations
```bash
# Duplicate template
POST /api/workout-templates/{id}/duplicate
{
  "newName": "Copy of Original Template"
}

# Get template exercises
GET /api/workout-templates/{id}/exercises

# Check name exists
GET /api/workout-templates/exists/name?name=Template%20Name
```

## 🔑 Key IDs

### Workout States
```
DRAFT:      workoutstate-02000001-0000-0000-0000-000000000001
PRODUCTION: workoutstate-02000002-0000-0000-0000-000000000002  
ARCHIVED:   workoutstate-02000003-0000-0000-0000-000000000003
```

### Common Categories
```
Upper Body - Push: workoutcategory-20000002-2000-4000-8000-200000000001
Upper Body - Pull: workoutcategory-20000002-2000-4000-8000-200000000002
Lower Body:        workoutcategory-20000002-2000-4000-8000-200000000003
Core:              workoutcategory-20000002-2000-4000-8000-200000000004
Full Body:         workoutcategory-20000002-2000-4000-8000-200000000005
```

### Difficulty Levels
```
Beginner:     difficultylevel-a51b5e6f-e29b-41a7-9e65-4a3cb1e8ff1a
Intermediate: difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a
Advanced:     difficultylevel-48a0c7e7-0fbd-4e85-9e3f-2b8e5f3d6a9c
```

## 📋 Service Interface

```csharp
public interface IWorkoutTemplateService
{
    // Create
    Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateDto dto);
    
    // Read
    Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id);
    Task<ServiceResult<PagedResult<WorkoutTemplateDto>>> GetPagedAsync(int page, int pageSize);
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByNamePatternAsync(string pattern);
    
    // Update
    Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateDto dto);
    Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId);
    
    // Delete
    Task<ServiceResult<bool>> DeleteAsync(WorkoutTemplateId id);
    
    // Special Operations
    Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName);
}
```

## 🛠️ Common Patterns

### Controller Pattern Matching
```csharp
var result = await _service.GetByIdAsync(id);
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { Error: ServiceErrorCode.NotFound } => NotFound(result.ErrorMessage),
    _ => BadRequest(new { error = result.ErrorMessage })
};
```

### Service Implementation Pattern
```csharp
return await _readOnlyUnitOfWork.ExecuteAsync(async uow =>
{
    var repository = uow.GetRepository<IWorkoutTemplateRepository>();
    var entity = await repository.GetByIdAsync(id);
    
    return entity.IsEmpty
        ? ServiceResult<WorkoutTemplateDto>.NotFound($"Template {id} not found")
        : ServiceResult<WorkoutTemplateDto>.Success(entity.ToDto());
});
```

### Empty Pattern Usage
```csharp
// Check for empty
if (template.IsEmpty) { /* handle not found */ }

// Parse ID safely
var templateId = WorkoutTemplateId.ParseOrEmpty(idString);

// Return empty instead of null
return template ?? WorkoutTemplate.Empty;
```

## ⚙️ Validation Rules

### Name
- Length: 3-100 characters
- Must be unique globally

### Description  
- Optional
- Max 500 characters

### Duration
- Range: 5-300 minutes

### State Transitions
- DRAFT → PRODUCTION → ARCHIVED only
- Cannot go back to DRAFT with execution logs
- Cannot delete if in PRODUCTION state

## 🐛 Common Issues & Solutions

### Issue: Test fails with "Expected 2 but found 3"
**Cause**: Test data naming conflicts  
**Solution**: Use unique names that don't overlap with search patterns

### Issue: Navigation properties null
**Cause**: Missing Include() in query  
**Solution**: Add proper includes to repository methods

### Issue: State change rejected
**Cause**: Invalid state transition  
**Solution**: Check current state and allowed transitions

### Issue: Duplicate name error
**Cause**: Name already exists  
**Solution**: Check with exists endpoint first

## 📊 DTOs

### CreateWorkoutTemplateDto
```csharp
{
    string Name
    string? Description
    WorkoutCategoryId CategoryId
    DifficultyLevelId DifficultyId
    int EstimatedDurationMinutes
    bool IsPublic
    List<string>? Tags
}
```

### UpdateWorkoutTemplateDto
```csharp
{
    string Name
    string? Description
    WorkoutCategoryId CategoryId
    DifficultyLevelId DifficultyId
    int EstimatedDurationMinutes
    bool IsPublic
    List<string>? Tags
}
```

### WorkoutTemplateDto (Response)
```csharp
{
    WorkoutTemplateId Id
    string Name
    string? Description
    ReferenceDataDto Category
    ReferenceDataDto Difficulty
    ReferenceDataDto WorkoutState
    int EstimatedDurationMinutes
    bool IsPublic
    List<string> Tags
    List<WorkoutTemplateExerciseDto> Exercises
    List<ReferenceDataDto> Objectives
    DateTime CreatedAt
    DateTime UpdatedAt
}
```

## 🔍 Testing

### Unit Test Example
```csharp
// Arrange
var template = new WorkoutTemplateBuilder()
    .WithName("Test Template")
    .InDraftState()
    .Build();

// Act
var result = await service.CreateAsync(createDto);

// Assert
result.Should().BeSuccess();
result.Data.Name.Should().Be("Test Template");
```

### Integration Test Example
```gherkin
Scenario: Create a new workout template
    Given I am a Personal Trainer
    When I create a new workout template with:
        | Field       | Value        |
        | Name        | My Template  |
        | CategoryId  | Full Body    |
    Then the workout template should be created successfully
```

## 📌 Important Notes

1. **No User Context**: Templates are not tied to specific users (CreatedBy removed in Phase 7)
2. **Exercise Management**: Moved to FEAT-028 - this feature only reads exercises
3. **Authorization**: Framework ready but not activated
4. **Caching**: Structure prepared but not implemented
5. **Objectives**: Entity relationship ready but linking not implemented

## 🚦 Status Codes

- **200 OK**: Success
- **201 Created**: Resource created
- **400 Bad Request**: Validation error
- **404 Not Found**: Resource not found
- **409 Conflict**: Business rule violation (e.g., duplicate name)

---

**Quick Tip**: All service methods return `ServiceResult<T>` - always check `IsSuccess` before using `Data`!