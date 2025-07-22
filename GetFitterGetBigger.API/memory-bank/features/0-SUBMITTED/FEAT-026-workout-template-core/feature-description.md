# Feature: Workout Template Core

## Feature ID: FEAT-026
## Created: 2025-07-22
## Status: SUBMITTED
## Source: Features/Workouts/WorkoutTemplate/WorkoutTemplateCore/
## Target PI: PI-2025-Q1

## Summary
Implement comprehensive workout template management system in the API, enabling Personal Trainers to create, manage, and share reusable workout blueprints. The system will support exercise organization in zones (warmup, main, cooldown), set configurations, and intelligent exercise suggestions.

## Business Context
This feature addresses the need for structured workout template management as defined in the source RAW file at Features/Workouts/WorkoutTemplate/WorkoutTemplateCore/WorkoutTemplateCore_RAW.md. Personal trainers need efficient tools to create reusable workout programs that ensure consistency, enable progressive overload tracking, and provide clear guidance for clients.

## Data Model Requirements

### New Entities
- **WorkoutTemplate**: Master blueprint containing workout metadata and configuration
  - Key relationships: WorkoutCategory, WorkoutObjective, ExecutionProtocol, User (creator)
  - Business rules: Must have at least one exercise in Main zone, zone sequence validation
  
- **WorkoutTemplateExercise**: Links exercises to templates with zone and sequence information
  - Key relationships: WorkoutTemplate, Exercise
  - Business rules: Sequence unique within zone, zone validation, automatic warmup/cooldown suggestions
  
- **SetConfiguration**: Defines performance parameters for each exercise
  - Key relationships: WorkoutTemplateExercise, ExecutionProtocol
  - Business rules: Protocol-based interpretation, positive set count, valid rep format

### Entity Relationships
```
WorkoutTemplate ←→ WorkoutTemplateExercise (one-to-many)
WorkoutTemplateExercise ←→ SetConfiguration (one-to-many)
WorkoutTemplate ←→ WorkoutCategory (many-to-one)
WorkoutTemplate ←→ WorkoutObjective (many-to-one)
WorkoutTemplate ←→ ExecutionProtocol (many-to-one)
WorkoutTemplate ←→ User (many-to-one)
WorkoutTemplateExercise ←→ Exercise (many-to-one)
SetConfiguration ←→ ExecutionProtocol (many-to-one)
```

### Database Schema Changes
- [ ] New tables: WorkoutTemplates, WorkoutTemplateExercises, SetConfigurations
- [ ] Modified tables: None
- [ ] New indexes: 
  - IX_WorkoutTemplates_CreatedByUserId
  - IX_WorkoutTemplates_IsPublic_IsActive
  - IX_WorkoutTemplateExercises_WorkoutTemplateId_Zone_SequenceOrder
- [ ] Migration scripts: Initial table creation with foreign keys

## API Endpoints

### WorkoutTemplate Management
```
GET    /api/workout-templates              # List with pagination and filtering
POST   /api/workout-templates              # Create new template
GET    /api/workout-templates/{id}         # Get template details
PUT    /api/workout-templates/{id}         # Update template
DELETE /api/workout-templates/{id}         # Soft delete template
```

### Exercise Management
```
GET    /api/workout-templates/{id}/exercises                          # Get exercises by zone
POST   /api/workout-templates/{id}/exercises                          # Add exercise
PUT    /api/workout-templates/{id}/exercises/{exerciseId}            # Update exercise
DELETE /api/workout-templates/{id}/exercises/{exerciseId}            # Remove exercise
PUT    /api/workout-templates/{id}/exercises/{exerciseId}/sets       # Configure sets
PUT    /api/workout-templates/{id}/exercises/reorder                 # Bulk reorder
```

### Business Operations
```
POST   /api/workout-templates/{id}/duplicate                         # Duplicate template
GET    /api/workout-templates/{id}/exercise-suggestions              # Smart suggestions
POST   /api/workout-templates/{id}/validate                          # Validate template
```

## Business Rules & Validation

### Core Business Rules
1. **Zone Sequence Rule**: Warmup exercises must precede Main exercises, Cooldown must follow Main
2. **Main Zone Requirement**: Template must have at least one exercise in Main zone to be valid
3. **Exercise Suggestions**: When adding Main exercise with warmup/cooldown associations, suggest related exercises
4. **Equipment Aggregation**: Automatically compile equipment list from selected exercises
5. **Rest as Exercise**: Rest periods implemented as special exercises in the workflow
6. **Sequence Uniqueness**: Exercise sequence must be unique within each zone
7. **Template Ownership**: Only creator can modify private templates
8. **Public Template Access**: All authenticated users can view public templates

### Validation Requirements
| Field | Type | Required | Validation Rules | Error Message |
|-------|------|----------|------------------|---------------|
| name | string | Yes | Max 100 chars, not empty | "Name is required and must not exceed 100 characters" |
| description | string | No | Max 500 chars | "Description must not exceed 500 characters" |
| workoutCategoryId | guid | Yes | Must exist and be active | "Invalid workout category" |
| workoutObjectiveId | guid | Yes | Must exist and be active | "Invalid workout objective" |
| executionProtocolId | guid | Yes | Must exist and be active | "Invalid execution protocol" |
| estimatedDurationMinutes | int | Yes | 10-240 | "Duration must be between 10 and 240 minutes" |
| difficultyLevel | string | Yes | Beginner/Intermediate/Advanced | "Invalid difficulty level" |
| zone | string | Yes | Warmup/Main/Cooldown | "Invalid exercise zone" |
| sequenceOrder | int | Yes | Positive, unique in zone | "Invalid sequence order" |
| targetSets | int | Yes | 1-50 | "Sets must be between 1 and 50" |
| targetReps | string | No | Valid format (e.g., "8-12") | "Invalid rep format" |
| tags | string[] | No | Max 10 tags, each max 30 chars | "Too many tags or tag too long" |

## Service Layer Requirements

### IWorkoutTemplateService Interface
```csharp
public interface IWorkoutTemplateService
{
    Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateRequest request);
    Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id);
    Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> GetAllAsync(WorkoutTemplateFilterParams filters, PaginationParams pagination);
    Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(UpdateWorkoutTemplateRequest request);
    Task<ServiceResult> DeleteAsync(WorkoutTemplateId id);
    Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, DuplicateTemplateRequest request);
}
```

### IWorkoutTemplateExerciseService Interface
```csharp
public interface IWorkoutTemplateExerciseService
{
    Task<ServiceResult<AddExerciseResponse>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseRequest request);
    Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, UpdateExerciseRequest request);
    Task<ServiceResult> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, bool acknowledgeWarnings);
    Task<ServiceResult> ReorderExercisesAsync(WorkoutTemplateId templateId, ReorderExercisesRequest request);
    Task<ServiceResult<ExerciseSuggestionsResponse>> GetExerciseSuggestionsAsync(WorkoutTemplateId templateId, string zone, ExerciseId? lastAddedExerciseId);
}
```

### Repository Requirements
- **ReadOnlyUnitOfWork**: For all query operations, validation, and existence checks
- **WritableUnitOfWork**: Only for Create, Update, Delete operations
- **Includes**: Navigation properties for reference data (Category, Objective, Protocol, Exercises)
- **Caching**: Cache reference data lookups, public template lists

## Authentication & Authorization

### Required Claims
- **Create/Update/Delete**: PT-Tier or Admin-Tier
- **Read Operations**: Free-Tier, PT-Tier, Admin-Tier
- **Duplicate Template**: PT-Tier or Admin-Tier
- **Exercise Management**: PT-Tier or Admin-Tier (owner only for private templates)

### Authorization Rules
- Users can only modify their own templates (unless Admin-Tier)
- Private templates only visible to creator (unless Admin-Tier)
- Public templates readable by all authenticated users
- Admin-Tier users have full access to all templates

## BDD Test Scenarios (MANDATORY)

### Scenario 1: Create Workout Template - Happy Path
```gherkin
Given I am authenticated as "PT-Tier"
And I have valid workout template data
  | name | Upper Body Strength |
  | workoutCategoryId | valid-category-id |
  | workoutObjectiveId | valid-objective-id |
  | executionProtocolId | valid-protocol-id |
  | estimatedDurationMinutes | 60 |
  | difficultyLevel | Intermediate |
  | isPublic | true |
When I send a POST request to "/api/workout-templates"
Then the response status should be 201
And the response should contain the created workout template
And the workout template should be persisted in the database
```

### Scenario 2: Create Workout Template - Validation Error
```gherkin
Given I am authenticated as "PT-Tier"
And I have invalid workout template data with missing name
When I send a POST request to "/api/workout-templates"
Then the response status should be 400
And the response should contain validation errors for "name"
```

### Scenario 3: Add Exercise to Template - With Warmup Suggestions
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template exists that I own
And an exercise "Bench Press" exists with warmup associations
When I send a POST request to "/api/workout-templates/{id}/exercises" with
  | exerciseId | bench-press-id |
  | zone | Main |
  | sequenceOrder | 1 |
Then the response status should be 201
And the response should contain warmup suggestions
And the exercise should be added to the template
```

### Scenario 4: Remove Exercise - With Warning
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template exists with a warmup exercise linked to a main exercise
When I send a DELETE request to "/api/workout-templates/{id}/exercises/{warmupId}"
Then the response status should be 409
And the response should contain a warning about linked exercise
When I send a DELETE request with "acknowledgeWarnings=true"
Then the response status should be 204
And the exercise should be removed from the template
```

### Scenario 5: Reorder Exercises Within Zone
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template exists with multiple exercises in Main zone
When I send a PUT request to "/api/workout-templates/{id}/exercises/reorder" with new order
Then the response status should be 200
And the exercises should be reordered in the database
```

### Scenario 6: Get Public Templates - Authorization
```gherkin
Given I am authenticated as "Free-Tier"
And public workout templates exist
When I send a GET request to "/api/workout-templates?isPublic=true"
Then the response status should be 200
And the response should contain only public templates
```

### Scenario 7: Update Private Template - Authorization Failure
```gherkin
Given I am authenticated as "PT-Tier"
And a private workout template exists owned by another user
When I send a PUT request to "/api/workout-templates/{id}"
Then the response status should be 403
And the response should contain authorization error
```

### Scenario 8: Duplicate Template - Success
```gherkin
Given I am authenticated as "PT-Tier"
And a public workout template exists
When I send a POST request to "/api/workout-templates/{id}/duplicate" with
  | name | My Copy of Template |
  | isPublic | false |
Then the response status should be 201
And a new template should be created with all exercises copied
And I should be the owner of the new template
```

### Edge Cases & Error Scenarios
- [ ] Duplicate sequence order within zone
- [ ] Adding exercise to non-existent template
- [ ] Invalid zone value
- [ ] Concurrent template modifications
- [ ] Template with 50+ exercises performance
- [ ] Invalid foreign key references (category, objective, protocol)
- [ ] Tag limit exceeded

## Performance Considerations

### Caching Strategy
- **Cache Keys**: 
  - `workout-templates:public:page:{page}:{pageSize}:{filters}`
  - `workout-template:{id}`
  - `workout-template-exercises:{templateId}`
- **Cache Duration**: 
  - Public template lists: 5 minutes
  - Individual templates: 10 minutes
  - Reference data: 30 minutes
- **Cache Invalidation**: On any template modification, invalidate related caches

### Query Optimization
- **Indexes**: 
  - CreatedByUserId for user's templates
  - IsPublic + IsActive for public template queries
  - WorkoutTemplateId + Zone + SequenceOrder for exercise ordering
- **Pagination**: Default 20, max 100 items per page
- **Filtering**: Support multiple filter combinations efficiently
- **Sorting**: Indexed fields for common sort operations

## Technical Dependencies

### Internal Dependencies
- **Entities**: WorkoutTemplate, WorkoutTemplateExercise, SetConfiguration
- **Services**: IExerciseService (for exercise validation and suggestions)
- **Repositories**: IWorkoutTemplateRepository, IWorkoutTemplateExerciseRepository
- **Reference Data**: WorkoutCategory, WorkoutObjective, ExecutionProtocol, DifficultyLevel

### External Dependencies
- **Database**: SQL Server with EF Core migrations
- **Authentication**: JWT Bearer tokens
- **Caching**: IMemoryCache for performance
- **Logging**: ILogger for diagnostics

## Migration Strategy

### Database Migrations
- [ ] Create WorkoutTemplates table with all columns and constraints
- [ ] Create WorkoutTemplateExercises table with composite indexes
- [ ] Create SetConfigurations table
- [ ] Add foreign key constraints to reference tables
- [ ] Create performance indexes
- [ ] No seed data required (templates created by users)

### Backward Compatibility
- [ ] New feature - no breaking changes
- [ ] All endpoints follow existing API patterns
- [ ] Consistent error response format

## Acceptance Criteria

- [ ] All API endpoints respond correctly with proper status codes
- [ ] Database schema matches entity model requirements
- [ ] Business rules enforced (zone sequence, main zone requirement)
- [ ] Validation works as specified with clear error messages
- [ ] Authorization rules implemented (ownership, public/private)
- [ ] BDD scenarios pass completely
- [ ] Equipment aggregation works automatically
- [ ] Warmup/cooldown suggestions functional
- [ ] Performance meets requirements (sub-200ms for lists)
- [ ] Caching implemented for performance
- [ ] Documentation complete in Swagger

## Dependencies

- **API Features**: 
  - FEAT-001: Exercise Management (for exercise validation and associations)
  - FEAT-002: User Management (for authentication and ownership)
- **Reference Data**: 
  - WorkoutCategory table (must be populated)
  - WorkoutObjective table (must be populated)
  - ExecutionProtocol table (must be populated)
- **Other Systems**: None

## Implementation Notes

### Repository Pattern Requirements
- Use `ReadOnlyUnitOfWork` for:
  - All GET operations
  - Validation checks (existence, uniqueness)
  - Loading reference data
  - Checking permissions
- Use `WritableUnitOfWork` ONLY for:
  - CREATE operations (SaveChangesAsync)
  - UPDATE operations (SaveChangesAsync)
  - DELETE operations (SaveChangesAsync)
- Implement proper Include() statements for navigation properties
- Follow existing repository patterns for consistency

### Service Layer Guidelines
- Implement all business logic in service layer, not controllers
- Use ServiceResult<T> pattern for all returns
- Validate all inputs before processing
- Check authorization at service level before operations
- Use transactions for multi-step operations
- Implement caching for frequently accessed data
- Handle equipment aggregation automatically

### Controller Implementation
- Keep controllers thin - delegate all logic to services
- Use proper HTTP status codes (201 for create, 204 for delete)
- Implement consistent error response format
- Add comprehensive OpenAPI documentation attributes
- Follow RESTful naming conventions
- Use action filters for common concerns

### Testing Requirements
- Unit tests: Mock ALL dependencies, test business logic thoroughly
- Integration tests: Use BDD format with real database
- Test all BDD scenarios defined above
- Test edge cases and error conditions
- Achieve minimum 80% code coverage
- Performance tests for templates with many exercises