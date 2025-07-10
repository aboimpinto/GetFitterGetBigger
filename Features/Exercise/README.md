# Exercise Management

## Metadata
- **Feature ID**: FEAT-001
- **Status**: COMPLETED
- **Created**: 2024-01-15
- **Last Updated**: 2025-01-10
- **Version**: 1.2.0
- **Owner**: Core Team
- **Projects Affected**: API | Admin | Clients

## Overview

### Business Purpose
The Exercise Management feature provides the foundational system for creating, organizing, and managing exercises within the GetFitterGetBigger platform. Exercises are the atomic units of any workout, and this feature ensures that Personal Trainers can build a comprehensive exercise library with rich metadata including muscle groups, equipment requirements, difficulty levels, and proper form instructions.

This feature addresses the need for a standardized exercise database that can be used across all workouts while maintaining consistency in naming, form instructions, and categorization. It enables Personal Trainers to create custom exercises specific to their training methodology while leveraging a shared library of common exercises.

### Target Users
- **Primary**: Personal Trainers who need to create and manage exercise libraries for their clients
- **Secondary**: End users (clients) who need to understand and perform exercises correctly during workouts

### Success Metrics
- Exercise library growth rate (target: 50+ exercises per PT within first month)
- Exercise search/filter usage (target: 80% of workout creations use search)
- Form video view completion rate (target: 70% completion rate)
- Exercise categorization accuracy (target: 95% properly categorized)

## Technical Specification

### Data Model
```json
{
  "Exercise": {
    "id": "string (exercise-{guid})",
    "name": "string (max 100 chars)",
    "description": "string",
    "coachNotes": [
      {
        "id": "string",
        "text": "string",
        "order": "number"
      }
    ],
    "exerciseTypes": ["string (reference)"],
    "videoUrl": "string (optional)",
    "imageUrl": "string (optional)",
    "isUnilateral": "boolean",
    "isActive": "boolean",
    "difficulty": "string (reference)",
    "muscleGroups": [
      {
        "muscleGroup": "string (reference)",
        "role": "string (Primary|Secondary|Stabilizer)"
      }
    ],
    "equipment": ["string (reference)"],
    "movementPatterns": ["string (reference)"],
    "bodyParts": ["string (reference)"],
    "kineticChain": "string (optional, reference)",
    "exerciseWeightType": "string (reference)"
  }
}
```

### API Endpoints
| Method | Endpoint | Purpose | Auth Required | Claims |
|--------|----------|---------|---------------|--------|
| GET    | /api/exercises | List exercises with pagination and filters | Yes | Any |
| GET    | /api/exercises/{id} | Get single exercise details | Yes | Any |
| POST   | /api/exercises | Create new exercise | Yes | PT-Tier, Admin-Tier |
| PUT    | /api/exercises/{id} | Update exercise | Yes | PT-Tier, Admin-Tier |
| DELETE | /api/exercises/{id} | Soft delete exercise | Yes | PT-Tier, Admin-Tier |

### Business Rules
1. Exercise names must be unique among active exercises
2. REST exercise type cannot be combined with other exercise types
3. Kinetic chain is required for all non-REST exercises
4. Exercises are never hard deleted, only marked as inactive
5. Exercise weight type determines valid weight assignments in workouts
6. At least one muscle group must be assigned as Primary

### Validation Rules
- **Name**: Required, 1-100 characters, unique among active exercises
- **Description**: Required, minimum 10 characters
- **Exercise Types**: Optional array, REST type is exclusive
- **Difficulty**: Required, must reference existing difficulty level
- **Video/Image URLs**: Must be valid URLs when provided
- **Kinetic Chain**: Required for non-REST exercises, null for REST
- **Exercise Weight Type**: Required, must be valid reference

## Implementation Details

### API Project
- **Endpoints Implemented**: Full CRUD operations for exercises
- **Data Models**: Exercise entity with related reference tables
- **Database Changes**: 
  ```json
  {
    "tables": ["Exercises", "ExerciseCoachNotes", "ExerciseMuscleGroups"],
    "indexes": ["IX_Exercises_Name", "IX_Exercises_IsActive", "IX_Exercises_Difficulty"],
    "constraints": ["FK_Exercises_Difficulty", "FK_Exercises_KineticChain", "FK_Exercises_ExerciseWeightType"]
  }
  ```
- **Business Logic**: Soft delete implementation, unique name validation, exercise type exclusivity rules

### Admin Project
- **UI Components**: Exercise list view, detail view, create/edit form, bulk operations toolbar
- **Routes**: /exercises, /exercises/new, /exercises/{id}, /exercises/{id}/edit
- **User Workflows**: Browse exercises, create new exercise, edit existing, manage coach notes, link exercises
- **UI Requirements**: Responsive design, drag-drop for coach notes ordering, real-time search, image/video preview

### Clients Project
- **Platforms**: Web | Mobile (Android/iOS) | Desktop
- **Implementation Status**:
  - Web: Completed
  - Android: Completed
  - iOS: Completed
  - Desktop: Completed
- **Platform-Specific Considerations**: 
  - Mobile: Optimized video loading, offline exercise cache
  - Desktop: Multi-panel view support
  - All: Adaptive UI based on screen size

## Request/Response Examples

### Example 1: Create Exercise
**Request**:
```http
POST /api/exercises
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise targeting quads, glutes, and hamstrings",
  "coachNotes": [
    {"text": "Keep chest up and core engaged", "order": 1},
    {"text": "Descend until thighs are parallel to floor", "order": 2}
  ],
  "exerciseTypeIds": ["workout"],
  "isUnilateral": false,
  "difficultyId": "intermediate",
  "kineticChainId": "compound",
  "exerciseWeightTypeId": "weight-required",
  "muscleGroups": [
    {"muscleGroupId": "quadriceps", "muscleRoleId": "primary"},
    {"muscleGroupId": "glutes", "muscleRoleId": "primary"}
  ],
  "equipmentIds": ["barbell", "squat-rack"]
}
```

**Success Response (201 Created)**:
```json
{
  "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise targeting quads, glutes, and hamstrings",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Error Response (400 Bad Request)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "name": ["An exercise with this name already exists"],
    "kineticChainId": ["Kinetic chain is required for non-REST exercises"]
  }
}
```

## Error Handling

### Error Codes
| Code | Meaning | User Message |
|------|---------|--------------|
| 400  | Validation error | Please check the highlighted fields and try again |
| 401  | Not authenticated | Please log in to continue |
| 403  | Not authorized | You don't have permission to manage exercises |
| 404  | Exercise not found | The requested exercise could not be found |
| 409  | Duplicate name | An exercise with this name already exists |

## Security Considerations
- **Authentication**: Bearer token required for all endpoints
- **Authorization**: Write operations require PT-Tier or Admin-Tier claims
- **Data Privacy**: Exercise data is organization-scoped
- **Audit Trail**: All modifications are logged with user ID and timestamp

## Dependencies

### External Dependencies
- CDN service for image/video storage
- Video streaming service for exercise demonstrations

### Internal Dependencies
- Reference Data feature (for difficulty levels, muscle groups, etc.)
- Authentication/Authorization system
- Media Upload feature

### Reference Data
- Difficulty Levels
- Muscle Groups and Roles
- Equipment Types
- Body Parts
- Movement Patterns
- Exercise Types
- Kinetic Chain Types
- Exercise Weight Types

## Migration Plan
1. Create exercise tables and indexes
2. Populate reference data tables
3. Migrate existing exercise data (if any)
4. Update workout templates to reference new exercise IDs
5. Enable new exercise management UI
6. Deprecate old exercise system (if applicable)

## Testing Requirements

### Unit Tests
- Exercise name uniqueness validation
- Exercise type exclusivity rules
- Kinetic chain requirement logic
- Weight type validation rules

### Integration Tests
- Full CRUD operations with database
- Reference data integrity checks
- Soft delete functionality
- Search and filter operations

### E2E Tests
- Create exercise workflow in admin
- Browse and filter exercises
- Edit exercise with coach notes reordering
- Exercise usage in workout creation

## Documentation

### User Documentation
- Exercise creation guide for Personal Trainers
- Exercise categorization best practices
- Video upload guidelines

### Developer Documentation
- API endpoint reference
- Data model relationships
- Integration guide for workout features

## Future Enhancements
- **Phase 2**: AI-powered exercise form analysis
- **Phase 3**: Exercise recommendation engine based on client goals
- **Phase 4**: Community-contributed exercise library with approval workflow

## Related Features
- **Exercise Linking** - Defines relationships between exercises for progressions
- **Workout Builder** - Uses exercises to create structured workouts
- **Reference Data** - Provides lookup values for exercise metadata

## Changelog
| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0   | 2024-01-15 | Initial release | Core Team |
| 1.1.0   | 2024-03-01 | Added kinetic chain support | Core Team |
| 1.2.0   | 2025-01-10 | Added exercise weight type | Core Team |

## Notes
The Exercise Management feature serves as the foundation for all workout-related functionality in the platform. Maintaining data quality and comprehensive categorization is critical for downstream features.

### Technology Stack Reference
While this feature documentation remains technology-agnostic, the current implementation uses:
- API: C# Minimal API
- Admin: C# Blazor  
- Clients: C# Avalonia (Android, iOS, Desktop, Web)

Note: Feature documentation focuses on business requirements and uses JSON for data models to maintain technology independence.