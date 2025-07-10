# Exercise Weight Type

## Metadata
- **Feature ID**: FEAT-023
- **Status**: IN_PROGRESS
- **Created**: 2025-07-10
- **Last Updated**: 2025-07-10
- **Version**: 1.0.0
- **Owner**: Backend Team
- **Projects Affected**: API | Admin

## Overview

### Business Purpose
The Exercise Weight Type feature provides a standardized classification system for how exercises handle weight assignments in workout creation and planning. This reference data ensures consistency across the platform by defining clear rules for whether an exercise requires weight, allows optional weight, or doesn't use weight at all.

This feature solves the problem of inconsistent weight handling across different exercise types, preventing invalid workout configurations where bodyweight exercises might be assigned external weights or weighted exercises might be created without proper weight specifications. It enables personal trainers to create accurate workout plans with appropriate weight assignments for each exercise type.

### Target Users
- **Primary**: Personal Trainers creating and managing workout templates
- **Secondary**: System administrators maintaining exercise catalog integrity

### Success Metrics
- 100% of exercises have appropriate weight type classification
- Zero invalid weight assignments in workout templates
- Reduction in workout creation errors by 80%

## Technical Specification

### Data Model
```json
{
  "ExerciseWeightType": {
    "id": "string (guid)",
    "code": "string (unique, immutable)",
    "name": "string",
    "description": "string",
    "isActive": "boolean",
    "displayOrder": "integer",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  },
  "ReferenceDataValues": [
    {
      "code": "BODYWEIGHT_ONLY",
      "name": "Bodyweight Only",
      "description": "Exercises that cannot have external weight added",
      "displayOrder": 1
    },
    {
      "code": "BODYWEIGHT_OPTIONAL",
      "name": "Bodyweight Optional",
      "description": "Exercises that can be performed with or without additional weight",
      "displayOrder": 2
    },
    {
      "code": "WEIGHT_REQUIRED",
      "name": "Weight Required",
      "description": "Exercises that must have external weight specified",
      "displayOrder": 3
    },
    {
      "code": "MACHINE_WEIGHT",
      "name": "Machine Weight",
      "description": "Exercises performed on machines with weight stacks",
      "displayOrder": 4
    },
    {
      "code": "NO_WEIGHT",
      "name": "No Weight",
      "description": "Exercises that do not use weight as a metric",
      "displayOrder": 5
    }
  ]
}
```

### API Endpoints
| Method | Endpoint | Purpose | Auth Required | Claims |
|--------|----------|---------|---------------|--------|
| GET    | /api/exercise-weight-types | List all weight types | No | None |
| GET    | /api/exercise-weight-types/{id} | Get specific weight type | No | None |

### Business Rules
1. Exercise Weight Types are system-defined reference data and cannot be created, updated, or deleted by users
2. Every exercise must have exactly one Exercise Weight Type assigned
3. Weight validation rules are enforced based on the exercise's weight type code
4. The `code` field is immutable once created and used for type-safe validation across all applications

### Validation Rules
- **BODYWEIGHT_ONLY**: Weight must be null or 0
- **NO_WEIGHT**: Weight must be null or 0
- **BODYWEIGHT_OPTIONAL**: Weight can be null, 0, or any positive number
- **WEIGHT_REQUIRED**: Weight must be a positive number (> 0)
- **MACHINE_WEIGHT**: Weight must be a positive number (> 0)

## Implementation Details

### API Project
- **Endpoints Implemented**: GET /api/exercise-weight-types (list), GET /api/exercise-weight-types/{id} (detail)
- **Data Models**: ExerciseWeightType entity, ExerciseWeightTypeDto
- **Database Changes**: 
  ```json
  {
    "table": "ExerciseWeightTypes",
    "columns": {
      "Id": "uniqueidentifier PRIMARY KEY",
      "Code": "nvarchar(50) UNIQUE NOT NULL",
      "Name": "nvarchar(100) NOT NULL",
      "Description": "nvarchar(500)",
      "IsActive": "bit NOT NULL DEFAULT 1",
      "DisplayOrder": "int NOT NULL",
      "CreatedAt": "datetime2 NOT NULL",
      "UpdatedAt": "datetime2 NOT NULL"
    }
  }
  ```
- **Business Logic**: Weight validation service that enforces rules based on exercise weight type

### Admin Project
- **UI Components**: Weight type selector dropdown, weight input field with dynamic behavior, weight type badge display
- **Routes**: Integrated into exercise management routes (/admin/exercises)
- **User Workflows**: Exercise creation with weight type selection, bulk exercise weight type updates
- **UI Requirements**: Dynamic form validation based on selected weight type, clear visual indicators for weight requirements

### Clients Project
- **Platforms**: Not applicable - This feature is used only for workout creation and management, not for performing workouts
- **Implementation Status**: N/A
- **Platform-Specific Considerations**: N/A

## Request/Response Examples

### Example 1: List All Exercise Weight Types
**Request**:
```http
GET /api/exercise-weight-types
Content-Type: application/json
```

**Success Response (200 OK)**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "code": "BODYWEIGHT_ONLY",
    "name": "Bodyweight Only",
    "description": "Exercises that cannot have external weight added",
    "isActive": true,
    "displayOrder": 1
  },
  {
    "id": "223e4567-e89b-12d3-a456-426614174001",
    "code": "BODYWEIGHT_OPTIONAL",
    "name": "Bodyweight Optional",
    "description": "Exercises that can be performed with or without additional weight",
    "isActive": true,
    "displayOrder": 2
  }
]
```

### Example 2: Get Single Exercise Weight Type
**Request**:
```http
GET /api/exercise-weight-types/123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json
```

**Success Response (200 OK)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "code": "BODYWEIGHT_ONLY",
  "name": "Bodyweight Only",
  "description": "Exercises that cannot have external weight added",
  "isActive": true,
  "displayOrder": 1
}
```

**Error Response (404 Not Found)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Exercise weight type not found"
}
```

## Error Handling

### Error Codes
| Code | Meaning | User Message |
|------|---------|--------------|
| 404  | Weight type not found | The requested exercise weight type does not exist |
| 400  | Invalid weight for exercise type | Weight assignment violates exercise type rules |

## Security Considerations
- **Authentication**: Not required for reading weight types (public reference data)
- **Authorization**: Only system administrators can modify reference data through database migrations
- **Data Privacy**: No sensitive data involved
- **Audit Trail**: Changes tracked through standard audit fields

## Dependencies

### External Dependencies
- None

### Internal Dependencies
- Exercise Management feature (exercises reference weight types)
- Workout Template feature (uses weight types for validation)

### Reference Data
- Fixed set of 5 weight type codes maintained as system reference data

## Migration Plan
1. Create ExerciseWeightTypes table with initial reference data
2. Add ExerciseWeightTypeId column to Exercises table
3. Map existing exercises to appropriate weight types based on exercise characteristics
4. Add foreign key constraint from Exercises to ExerciseWeightTypes
5. Update exercise management UI to include weight type selection

## Testing Requirements

### Unit Tests
- Weight validation logic for each weight type code
- Reference data retrieval operations
- Exercise weight type assignment validation

### Integration Tests
- API endpoint responses for list and detail views
- Exercise creation with different weight types
- Weight validation during workout template creation

### E2E Tests
- Create exercise with weight type and verify weight input behavior
- Bulk update exercises with new weight types
- Create workout with exercises of different weight types

## Documentation

### User Documentation
- Exercise management guide with weight type explanations
- Workout creation guide with weight assignment rules

### Developer Documentation
- API endpoint documentation in OpenAPI/Swagger
- Weight validation service implementation guide

## Future Enhancements
- **Phase 2**: Custom weight type creation for specialized training methods
- **Phase 3**: Weight progression rules based on exercise weight type

## Related Features
- Exercise Management - Exercises are classified by weight type
- Workout Template - Uses weight types for exercise weight validation
- Training Programs - Applies weight progression based on exercise weight types

## Changelog
| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0   | 2025-07-10 | Initial release | Backend Team |

## Notes
This is a reference data feature that provides the foundation for proper weight handling across the workout creation and management system. The feature is intentionally limited to API and Admin projects as it deals with workout planning, not workout execution.

### Technology Stack Reference
While this feature documentation remains technology-agnostic, the current implementation uses:
- API: C# Minimal API
- Admin: C# Blazor  
- Clients: Not applicable for this feature

Note: Feature documentation focuses on business requirements and uses JSON for data models to maintain technology independence.