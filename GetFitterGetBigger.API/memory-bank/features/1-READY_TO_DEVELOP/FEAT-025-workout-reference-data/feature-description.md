# Feature: Workout Reference Data

## Feature ID: FEAT-025
## Created: 2025-07-13
## Status: SUBMITTED
## Source: Features/Workouts/WorkoutTemplate/WorkoutReferenceData/
## Target PI: PI-2025-Q3

## Summary
Implementation of foundational reference tables for workout organization and discovery within the GetFitterGetBigger API. This feature provides read-only reference data for workout objectives, categories, and execution protocols that support both casual fitness enthusiasts and professional trainers.

## Business Context
This feature addresses the need for standardized workout classification, objective-based training guidance, and execution protocol definitions as defined in the source RAW file at Features/Workouts/WorkoutTemplate/WorkoutReferenceData/README.md.

The reference tables create the metadata infrastructure necessary for effective workout template management, discovery, and execution across all client platforms while ensuring consistency in training terminology and methodology.

## Data Model Requirements

### New Entities
- **WorkoutObjective**: Training goals and objectives with scientific programming guidance
  - Key relationships: Referenced by WorkoutTemplate (future)
  - Business rules: Unique values, exercise science-based descriptions, sequential display order

- **WorkoutCategory**: Workout organization by primary focus area and muscle groups
  - Key relationships: Referenced by WorkoutTemplate (future)
  - Business rules: Unique icons and colors, muscle group specifications, visual consistency

- **ExecutionProtocol**: Standardized set execution methods and training protocols
  - Key relationships: Referenced by WorkoutTemplate and Exercise entities
  - Business rules: Unique codes (uppercase underscore format), time/rep base indicators, intensity classifications

- **WorkoutMuscles**: Relationship table linking workout templates to muscles with engagement levels
  - Key relationships: WorkoutTemplate ←→ Muscle (many-to-many through this table)
  - Business rules: Engagement level validation, load estimation range 1-10

### Entity Relationships
```
WorkoutObjective ←→ WorkoutTemplate (one-to-many)
WorkoutCategory ←→ WorkoutTemplate (one-to-many) 
ExecutionProtocol ←→ WorkoutTemplate (one-to-many)
ExecutionProtocol ←→ Exercise (one-to-many)
WorkoutTemplate ←→ Muscle (many-to-many via WorkoutMuscles)
```

### Database Schema Changes
- [ ] New tables: WorkoutObjective, WorkoutCategory, ExecutionProtocol, WorkoutMuscles
- [ ] Modified tables: None (pure addition)
- [ ] New indexes: Unique constraints on value/code fields, performance indexes on isActive
- [ ] Migration scripts: Comprehensive seed data for all reference tables

## API Endpoints

### Workout Objectives Management
```
GET    /api/workout-objectives           # List all with optional includeInactive parameter
GET    /api/workout-objectives/{id}      # Get by ID with caching
```

### Workout Categories Management
```
GET    /api/workout-categories           # List all with optional includeInactive parameter
GET    /api/workout-categories/{id}      # Get by ID with caching
```

### Execution Protocols Management
```
GET    /api/execution-protocols          # List all with optional includeInactive parameter
GET    /api/execution-protocols/{id}     # Get by ID with caching
GET    /api/execution-protocols/by-code/{code}  # Get by programmatic code
```

## Business Rules & Validation

### Core Business Rules
1. **Read-Only Reference Data**: All endpoints provide read-only access to predefined data
2. **Exercise Science Alignment**: WorkoutObjective values must align with established exercise science principles
3. **Visual Consistency**: WorkoutCategory icons and colors must be unique across all categories
4. **Code Standards**: ExecutionProtocol codes must be unique and follow UPPERCASE_UNDERSCORE format
5. **Sequential Ordering**: DisplayOrder values must be sequential and unique within each table
6. **Soft Delete Support**: IsActive flag controls visibility while maintaining referential integrity

### Validation Requirements
| Field | Type | Required | Validation Rules | Error Message |
|-------|------|----------|------------------|---------------|
| value | string | Yes | Max 100 chars, unique within table | "Value is required and must be unique" |
| description | string | Yes | Max 500 chars | "Description is required and cannot exceed 500 characters" |
| displayOrder | integer | Yes | Positive, unique within table | "Display order must be positive and unique" |
| code | string | Yes (ExecutionProtocol) | Max 50 chars, uppercase with underscores | "Code must be uppercase with underscores only" |
| icon | string | Yes (WorkoutCategory) | Max 50 chars, unique | "Icon is required and must be unique" |
| color | string | Yes (WorkoutCategory) | Valid hex color code | "Color must be a valid hex color code" |
| isActive | boolean | Yes | Default: true | N/A |

## Service Layer Requirements

### IWorkoutObjectiveService Interface
```csharp
public interface IWorkoutObjectiveService
{
    Task<ServiceResult<List<WorkoutObjectiveDto>>> GetAllAsync(bool includeInactive = false);
    Task<ServiceResult<WorkoutObjectiveDto>> GetByIdAsync(WorkoutObjectiveId id);
}
```

### IWorkoutCategoryService Interface
```csharp
public interface IWorkoutCategoryService
{
    Task<ServiceResult<List<WorkoutCategoryDto>>> GetAllAsync(bool includeInactive = false);
    Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id);
}
```

### IExecutionProtocolService Interface
```csharp
public interface IExecutionProtocolService
{
    Task<ServiceResult<List<ExecutionProtocolDto>>> GetAllAsync(bool includeInactive = false);
    Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id);
    Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code);
}
```

### Repository Requirements
- **ReadOnlyUnitOfWork**: For all query operations (100% read-only feature)
- **WritableUnitOfWork**: Not needed (read-only reference data)
- **Includes**: No navigation properties to include initially
- **Caching**: All operations should be cached with 1-hour TTL

## Authentication & Authorization

### Required Claims
- **All Operations**: Free-Tier, PT-Tier, or Admin-Tier (minimum Free-Tier access)

### Authorization Rules
- Read access available to all authenticated users
- No create/update/delete operations (reference data managed via seeding)
- No user-specific filtering (global reference data)

## BDD Test Scenarios (MANDATORY)

### Scenario 1: Get All Workout Objectives - Success
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/workout-objectives"
Then the response status should be 200
And the response should contain a list of workout objectives
And each objective should have required fields: workoutObjectiveId, value, description, displayOrder, isActive
And the objectives should be ordered by displayOrder ascending
```

### Scenario 2: Get Workout Objective by ID - Success
```gherkin
Given I am authenticated as "Free-Tier"
And a workout objective with id "3fa85f64-5717-4562-b3fc-2c963f66afa6" exists
When I send a GET request to "/api/workout-objectives/3fa85f64-5717-4562-b3fc-2c963f66afa6"
Then the response status should be 200
And the response should contain the workout objective details
And the objective should have value "Muscular Strength"
```

### Scenario 3: Get Workout Objective by ID - Not Found
```gherkin
Given I am authenticated as "Free-Tier"
And no workout objective with id "00000000-0000-0000-0000-000000000000" exists
When I send a GET request to "/api/workout-objectives/00000000-0000-0000-0000-000000000000"
Then the response status should be 404
And the response should contain a not found error
```

### Scenario 4: Get All Workout Categories - Success
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/workout-categories"
Then the response status should be 200
And the response should contain a list of workout categories
And each category should have fields: workoutCategoryId, value, description, icon, color, primaryMuscleGroups, displayOrder, isActive
And the categories should be ordered by displayOrder ascending
```

### Scenario 5: Get All Execution Protocols - Success
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/execution-protocols"
Then the response status should be 200
And the response should contain a list of execution protocols
And each protocol should have fields: executionProtocolId, code, value, description, timeBase, repBase, restPattern, intensityLevel, displayOrder, isActive
And the protocols should be ordered by displayOrder ascending
```

### Scenario 6: Get Execution Protocol by Code - Success
```gherkin
Given I am authenticated as "Free-Tier"
And an execution protocol with code "STANDARD" exists
When I send a GET request to "/api/execution-protocols/by-code/STANDARD"
Then the response status should be 200
And the response should contain the execution protocol details
And the protocol should have code "STANDARD" and value "Standard"
```

### Scenario 7: Unauthorized Access
```gherkin
Given I am not authenticated
When I send a GET request to "/api/workout-objectives"
Then the response status should be 401
And the response should contain authentication error
```

### Scenario 8: Include Inactive Parameters
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/workout-objectives?includeInactive=true"
Then the response status should be 200
And the response should contain both active and inactive objectives
```

### Edge Cases & Error Scenarios
- [ ] Invalid GUID format handling
- [ ] Case sensitivity for execution protocol codes
- [ ] Large payload response optimization
- [ ] Concurrent access performance
- [ ] Cache invalidation scenarios

## Performance Considerations

### Caching Strategy
- **Cache Keys**: "workout-objectives", "workout-categories", "execution-protocols"
- **Cache Duration**: 1 hour TTL for all reference data
- **Cache Invalidation**: Manual invalidation on reference data updates (rare)
- **HTTP Headers**: Cache-Control: public, max-age=3600

### Query Optimization
- **Indexes**: Unique indexes on value and displayOrder, regular index on isActive
- **Pagination**: Not needed (small reference datasets)
- **Filtering**: Only includeInactive parameter supported
- **Sorting**: Always by displayOrder ascending

## Technical Dependencies

### Internal Dependencies
- **Entities**: WorkoutObjective, WorkoutCategory, ExecutionProtocol, WorkoutMuscles
- **Services**: IWorkoutObjectiveService, IWorkoutCategoryService, IExecutionProtocolService
- **Repositories**: IWorkoutObjectiveRepository, IWorkoutCategoryRepository, IExecutionProtocolRepository
- **Reference Data**: Integration with existing Muscle table for WorkoutMuscles relationship

### External Dependencies
- **Database**: EF Core migrations for new reference tables
- **Authentication**: JWT token validation system
- **Caching**: IMemoryCache for reference data caching

## Migration Strategy

### Database Migrations
- [ ] Create WorkoutObjective table with ReferenceDataBase pattern
- [ ] Create WorkoutCategory table with additional visual fields (icon, color, primaryMuscleGroups)
- [ ] Create ExecutionProtocol table with protocol-specific fields (timeBase, repBase, restPattern, intensityLevel)
- [ ] Create WorkoutMuscles relationship table
- [ ] Add unique constraints and performance indexes
- [ ] Seed comprehensive reference data for all tables

### Backward Compatibility
- [ ] No breaking changes (pure addition)
- [ ] No impact on existing endpoints
- [ ] Future WorkoutTemplate integration prepared

## Acceptance Criteria

- [ ] All 6 API endpoints respond correctly with proper status codes
- [ ] Database schema created with proper indexes and constraints
- [ ] Comprehensive seed data populated for all reference tables
- [ ] Validation rules enforced for all fields
- [ ] Authentication and authorization implemented correctly
- [ ] All BDD scenarios pass completely
- [ ] Caching implemented with 1-hour TTL
- [ ] Performance meets <50ms cached, <200ms uncached targets
- [ ] OpenAPI documentation complete and accurate

## Dependencies

- **API Features**: None (foundational feature)
- **Reference Data**: Existing Muscle table for WorkoutMuscles relationships
- **Other Systems**: Authentication system for JWT validation

## Implementation Notes

### Repository Pattern Requirements
- Use `ReadOnlyUnitOfWork` for ALL operations (read-only feature)
- No need for `WritableUnitOfWork` (no create/update/delete operations)
- Implement proper ordering by displayOrder for consistency
- Follow existing repository patterns for read-only reference data

### Service Layer Guidelines
- Implement business logic for includeInactive parameter filtering
- Use ServiceResult pattern for consistent error handling
- Validate all GUID inputs before processing
- Check authorization at service level (minimum Free-Tier)
- Implement aggressive caching for all operations

### Controller Implementation
- Keep controllers thin - delegate to services
- Use proper HTTP status codes (200, 404, 401, 500)
- Implement consistent error response format using ProblemDetails
- Add comprehensive OpenAPI documentation attributes
- Follow existing reference data endpoint patterns

### Testing Requirements
- Unit tests: Mock ALL dependencies, test business logic and caching
- Integration tests: Use BDD format with real database and seed data
- Test all BDD scenarios defined above with proper test data
- Achieve high test coverage (>80% target)
- Performance tests for caching effectiveness

## Seed Data Requirements

### WorkoutObjective Seed Data (7 entries)
1. Muscular Strength, Hypertrophy, Power, Muscular Endurance
2. Cardiovascular Conditioning, Flexibility & Mobility, General Fitness

### WorkoutCategory Seed Data (8 entries)
1. HIIT, Arms, Legs, Abs & Core
2. Shoulders, Back, Chest, Full Body

### ExecutionProtocol Seed Data (8 entries)
1. STANDARD, AMRAP, EMOM, FOR_TIME
2. TABATA, CLUSTER, DROP_SET, REST_PAUSE

All seed data includes comprehensive descriptions, proper display ordering, and appropriate metadata fields as specified in the existing documentation.