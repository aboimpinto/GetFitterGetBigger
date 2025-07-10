# Feature Request: Exercise Weight Type Reference Table

## Feature ID: FEAT-023
## Status: SUBMITTED

## Summary
Implement a new reference table to define how exercises handle weight assignments. This feature introduces the ExerciseWeightType entity to categorize exercises based on their weight requirements, enabling proper validation and improved user experience when creating workouts.

## Business Value
- **Improved Safety**: Prevents inappropriate weight assignments (e.g., adding weight to bodyweight-only exercises)
- **Better UX**: Dynamic UI can show/hide weight fields based on exercise type
- **Data Integrity**: Enforces consistent weight handling rules across all applications
- **Trainer Efficiency**: Clear indication of which exercises require weights
- **Client Clarity**: Users understand weight requirements for each exercise

## Current State
- Exercises have no formal weight type classification
- No validation for weight assignments
- UI must guess whether to show weight input fields
- Inconsistent handling of bodyweight vs weighted exercises

## Desired State
- Every exercise has an assigned weight type
- Weight validation enforced based on exercise type
- Clear API contract for weight handling rules
- Consistent behavior across all client applications

## Conceptual Model

### ExerciseWeightType Entity
```
ExerciseWeightType:
  - Identifier: Unique identifier (format: "exerciseweighttype-{guid}")
  - Code: Immutable string for programmatic use
  - Name: Human-readable display name
  - Description: Detailed explanation of the weight type
  - DisplayOrder: Integer for consistent ordering
  - IsActive: Boolean flag for soft delete capability
  - CreatedAt: Timestamp of creation
  - UpdatedAt: Timestamp of last modification
```

### Reference Data Values

#### BODYWEIGHT_ONLY
- **Code**: BODYWEIGHT_ONLY
- **Name**: Bodyweight Only
- **Description**: Exercises that cannot have external weight added
- **Display Order**: 1
- **Examples**: Push-ups, Pull-ups, Bodyweight Squats

#### BODYWEIGHT_OPTIONAL
- **Code**: BODYWEIGHT_OPTIONAL
- **Name**: Bodyweight Optional
- **Description**: Exercises that can be performed with or without additional weight
- **Display Order**: 2
- **Examples**: Dips (can add weight belt), Lunges (can hold dumbbells)

#### WEIGHT_REQUIRED
- **Code**: WEIGHT_REQUIRED
- **Name**: Weight Required
- **Description**: Exercises that must have external weight specified
- **Display Order**: 3
- **Examples**: Barbell Bench Press, Dumbbell Curls

#### MACHINE_WEIGHT
- **Code**: MACHINE_WEIGHT
- **Name**: Machine Weight
- **Description**: Exercises performed on machines with weight stacks
- **Display Order**: 4
- **Examples**: Leg Press, Cable Rows

#### NO_WEIGHT
- **Code**: NO_WEIGHT
- **Name**: No Weight
- **Description**: Exercises that do not use weight as a metric
- **Display Order**: 5
- **Examples**: Plank (time-based), Stretches, Mobility work

## API Specification

### Endpoints

#### 1. Get All Exercise Weight Types
- **Method**: GET
- **Path**: `/api/ReferenceTables/ExerciseWeightTypes`
- **Authentication**: Required
- **Authorization**: Any authenticated user
- **Response**: Array of ExerciseWeightType DTOs

#### 2. Get Exercise Weight Type by ID
- **Method**: GET
- **Path**: `/api/ReferenceTables/ExerciseWeightTypes/{id}`
- **Authentication**: Required
- **Authorization**: Any authenticated user
- **Parameters**:
  - `id`: The structured ID (e.g., "exerciseweighttype-123e4567-e89b-12d3-a456-426614174000")
- **Response**: Single ExerciseWeightType DTO or 404

#### 3. Get Exercise Weight Type by Value
- **Method**: GET
- **Path**: `/api/ReferenceTables/ExerciseWeightTypes/ByValue/{value}`
- **Authentication**: Required
- **Authorization**: Any authenticated user
- **Parameters**:
  - `value`: The name of the weight type (case-insensitive)
- **Response**: Single ExerciseWeightType DTO or 404

### Response Format

All endpoints return data in the standard ReferenceDataDto format:

```json
{
  "id": "exerciseweighttype-a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "value": "Bodyweight Only",
  "description": "Exercises that cannot have external weight added"
}
```

### Example Responses

#### GET /api/ReferenceTables/ExerciseWeightTypes
```json
[
  {
    "id": "exerciseweighttype-11111111-1111-1111-1111-111111111111",
    "value": "Bodyweight Only",
    "description": "Exercises that cannot have external weight added"
  },
  {
    "id": "exerciseweighttype-22222222-2222-2222-2222-222222222222",
    "value": "Bodyweight Optional",
    "description": "Exercises that can be performed with or without additional weight"
  },
  {
    "id": "exerciseweighttype-33333333-3333-3333-3333-333333333333",
    "value": "Weight Required",
    "description": "Exercises that must have external weight specified"
  },
  {
    "id": "exerciseweighttype-44444444-4444-4444-4444-444444444444",
    "value": "Machine Weight",
    "description": "Exercises performed on machines with weight stacks"
  },
  {
    "id": "exerciseweighttype-55555555-5555-5555-5555-555555555555",
    "value": "No Weight",
    "description": "Exercises that do not use weight as a metric"
  }
]
```

## Exercise Entity Integration

The Exercise entity must be updated to include a reference to ExerciseWeightType:

### Exercise Entity Update
```
Exercise:
  - (existing properties...)
  - ExerciseWeightTypeId: Reference to ExerciseWeightType (required)
```

### Exercise API Response Update
The exercise endpoints should include the weight type information:

```json
{
  "id": "exercise-abc123...",
  "name": "Barbell Bench Press",
  "description": "...",
  "exerciseWeightType": {
    "id": "exerciseweighttype-33333333-3333-3333-3333-333333333333",
    "value": "Weight Required",
    "description": "Exercises that must have external weight specified"
  },
  // ... other exercise properties
}
```

## Validation Rules

The following validation rules must be implemented in application code (not database constraints):

### Weight Assignment Validation
When creating or updating workout exercises:

1. **BODYWEIGHT_ONLY** or **NO_WEIGHT**:
   - Target weight must be null or 0
   - Validation error if positive weight provided

2. **BODYWEIGHT_OPTIONAL**:
   - Target weight can be null, 0, or any positive number
   - Validation error if negative weight provided

3. **WEIGHT_REQUIRED** or **MACHINE_WEIGHT**:
   - Target weight must be a positive number (> 0)
   - Validation error if null, 0, or negative

### UI Behavior Guidelines
Client applications should implement these behaviors based on weight type:

| Weight Type Code | UI Behavior |
|-----------------|-------------|
| BODYWEIGHT_ONLY | Hide weight input field |
| NO_WEIGHT | Hide weight input field |
| BODYWEIGHT_OPTIONAL | Show optional weight field with "+" prefix for additional weight |
| WEIGHT_REQUIRED | Show required weight field with validation |
| MACHINE_WEIGHT | Show weight field with machine-specific context |

## Migration Strategy

### Initial Data Migration
1. Insert the 5 weight type records with predefined IDs
2. Update existing exercises with appropriate weight types:
   - Exercises containing "Barbell", "Dumbbell", "Cable" → WEIGHT_REQUIRED
   - Exercises with "Machine" in equipment → MACHINE_WEIGHT
   - Common bodyweight exercises → BODYWEIGHT_ONLY
   - Exercises with "Weighted" prefix → BODYWEIGHT_OPTIONAL
   - Stretches, mobility exercises → NO_WEIGHT
   - Default for uncertain cases → BODYWEIGHT_OPTIONAL

### Data Integrity
- All exercises must have a valid ExerciseWeightTypeId after migration
- Foreign key constraint ensures referential integrity
- Soft delete on weight types preserves historical data

## Acceptance Criteria

1. **Reference Table Implementation**
   - [ ] All 5 weight types are seeded in the database
   - [ ] GET endpoints return weight types correctly
   - [ ] Standard reference table patterns are followed

2. **Exercise Integration**
   - [ ] Exercise entity includes ExerciseWeightTypeId
   - [ ] Exercise API responses include weight type data
   - [ ] Exercise creation/update requires valid weight type

3. **Validation**
   - [ ] Weight validation rules enforce correct values by type
   - [ ] Clear error messages for validation failures
   - [ ] Validation occurs at service layer

4. **Migration**
   - [ ] All existing exercises have weight types assigned
   - [ ] No data loss during migration
   - [ ] Rollback strategy documented

5. **Performance**
   - [ ] Weight types are cached appropriately
   - [ ] No significant performance degradation
   - [ ] Efficient querying of exercises by weight type

## Technical Considerations

### Caching
- Cache duration: 24 hours (static reference table)
- Cache key pattern: `reference-table:exerciseweighttypes`
- Invalidation: Only on application restart or manual refresh

### Error Responses
Standard HTTP status codes and error formats:
- 400 Bad Request: Invalid weight for exercise type
- 404 Not Found: Weight type not found
- 422 Unprocessable Entity: Business rule violations

### Backwards Compatibility
- Existing exercise endpoints continue to function
- Weight type is required for new exercises
- Grace period for updating existing exercises

## Dependencies
- Exercise entity must be updated
- Reference table base infrastructure must exist
- Database migration framework must be configured

## Priority
High - This feature improves data integrity and user experience across all applications

## Estimated Effort
- API Implementation: 2-3 days
- Testing: 1 day
- Migration and deployment: 1 day
- Total: 4-5 days