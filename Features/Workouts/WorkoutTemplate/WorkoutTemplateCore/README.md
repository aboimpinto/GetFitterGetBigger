# Workout Template Core

## Metadata
- **Feature ID**: FEAT-WTC-001
- **Status**: IN_PROGRESS
- **Created**: 2025-07-22
- **Last Updated**: 2025-07-22
- **Version**: 1.0.0
- **Owner**: GetFitterGetBigger Team
- **Projects Affected**: API | Admin | Clients

## Overview

### Business Purpose
The Workout Template Core feature represents the heart of the workout management system within the GetFitterGetBigger ecosystem. This feature provides the comprehensive infrastructure for creating, managing, and executing structured workout blueprints that serve as the foundation for actual workout logging and client training programs.

A workout template is a reusable blueprint that defines exercises organized to achieve specific fitness goals. The template serves as a container that brings exercises together based on a chosen workout category (such as Upper Body or Lower Body), targeting specific objectives (like Muscular Strength or Endurance) through a defined execution protocol (Standard, AMRAP, EMOM, etc.).

This feature enables personal trainers to encapsulate their expertise and programming knowledge into reusable formats, ensuring consistency across training sessions, enabling progressive overload tracking, and providing clear guidance for both trainers and clients.

### Target Users
- **Primary**: Personal Trainers who need to create, manage, and assign structured workout programs to their clients
- **Secondary**: Clients who execute workout templates and track their performance against prescribed targets

### Success Metrics
- Number of workout templates created per trainer
- Template reuse rate across multiple clients
- Workout completion rates for assigned templates
- Time saved in workout programming (measured in hours per week)

## Technical Specification

### Data Model
```json
{
  "WorkoutTemplate": {
    "id": "string (guid)",
    "name": "string",
    "description": "string",
    "workoutCategoryId": "string (guid)",
    "workoutObjectiveId": "string (guid)",
    "executionProtocolId": "string (guid)",
    "estimatedDuration": "integer (minutes)",
    "difficultyLevel": "string (Beginner|Intermediate|Advanced)",
    "isPublic": "boolean",
    "creatorId": "string (guid)",
    "createdAt": "datetime",
    "lastModified": "datetime",
    "version": "string",
    "isActive": "boolean",
    "tags": ["string"],
    "workoutStateId": "string (guid)",
    "relationships": {
      "workoutCategory": "WorkoutCategory",
      "workoutObjective": "WorkoutObjective",
      "executionProtocol": "ExecutionProtocol",
      "workoutState": "WorkoutState",
      "creator": "User",
      "exercises": ["WorkoutTemplateExercise"]
    }
  },
  "WorkoutTemplateExercise": {
    "id": "string (guid)",
    "workoutTemplateId": "string (guid)",
    "exerciseId": "string (guid)",
    "zone": "string (Warmup|Main|Cooldown)",
    "sequenceOrder": "integer",
    "exerciseNotes": "string",
    "relationships": {
      "workoutTemplate": "WorkoutTemplate",
      "exercise": "Exercise",
      "setConfigurations": ["SetConfiguration"]
    }
  },
  "SetConfiguration": {
    "id": "string (guid)",
    "workoutTemplateExerciseId": "string (guid)",
    "configurationOrder": "integer",
    "executionProtocolId": "string (guid)",
    "targetSets": "integer",
    "targetReps": "string",
    "targetDuration": "integer (seconds)",
    "intensityGuideline": "string",
    "relationships": {
      "workoutTemplateExercise": "WorkoutTemplateExercise",
      "executionProtocol": "ExecutionProtocol"
    }
  },
  "WorkoutState": {
    "id": "string (guid)",
    "value": "string",
    "description": "string",
    "displayOrder": "integer",
    "isActive": "boolean"
  }
}
```

### API Endpoints
| Method | Endpoint | Purpose | Claims |
|--------|----------|---------|--------|
| GET    | /api/workout-templates | List all workout templates | Any user |
| GET    | /api/workout-templates/{id} | Get specific workout template | Any user |
| POST   | /api/workout-templates | Create new workout template | Personal Trainer role |
| PUT    | /api/workout-templates/{id} | Update workout template | Personal Trainer role + owner |
| DELETE | /api/workout-templates/{id} | Delete workout template | Personal Trainer role + owner |
| POST   | /api/workout-templates/{id}/exercises | Add exercise to template | Personal Trainer role + owner |
| PUT    | /api/workout-templates/{id}/exercises/{exerciseId} | Update exercise in template | Personal Trainer role + owner |
| DELETE | /api/workout-templates/{id}/exercises/{exerciseId} | Remove exercise from template | Personal Trainer role + owner |
| POST   | /api/workout-templates/{id}/exercises/{exerciseId}/configurations | Add set configuration | Personal Trainer role + owner |
| PUT    | /api/workout-templates/{id}/state | Change workout state | Personal Trainer role + owner |
| GET    | /api/reference-tables/workout-states | Get all workout states | Any user |

### Business Rules
1. A workout template must have a name, category, objective, and execution protocol
2. Exercises in the workout flow must follow the zone order: Warmup → Main → Cooldown
3. Each exercise within a zone must have a unique sequence order
4. When an exercise with warmup/cooldown associations is added to the Main zone, the system suggests adding those associations
5. Equipment requirements are automatically aggregated from all selected exercises
6. Rest periods are implemented as special exercise entries
7. Workout state transitions follow specific rules:
   - DRAFT → PRODUCTION: Deletes all associated execution logs
   - PRODUCTION → DRAFT: Only allowed if no execution logs exist
   - Any state → ARCHIVED: Preserves all execution logs for historical reference
8. Only workouts in PRODUCTION state can be executed by regular users
9. Archived workouts cannot be executed but historical data remains viewable
10. Exercise suggestions are based on the selected workout category and push/pull complementarity

### Validation Rules
- **Name**: Required, 3-100 characters
- **Description**: Optional, max 1000 characters
- **Workout Category**: Required, must be valid reference
- **Workout Objective**: Required, must be valid reference
- **Execution Protocol**: Required, must be valid reference
- **Estimated Duration**: Required, 5-300 minutes
- **Difficulty Level**: Required, must be one of: Beginner, Intermediate, Advanced
- **Zone**: Required for exercises, must be one of: Warmup, Main, Cooldown
- **Sequence Order**: Required, must be unique within zone
- **Target Sets**: Required for set configuration, 1-100
- **Target Reps**: Required for rep-based exercises, can be range (e.g., "8-12")
- **Target Duration**: Required for time-based exercises, 1-3600 seconds

## Implementation Details

### API Project
- **Endpoints Implemented**: Full CRUD operations for workout templates, exercises, and configurations
- **Data Models**: WorkoutTemplateDto, WorkoutTemplateExerciseDto, SetConfigurationDto, WorkoutStateDto
- **Database Changes**: 
  ```json
  {
    "tables": [
      "WorkoutTemplates",
      "WorkoutTemplateExercises",
      "SetConfigurations",
      "WorkoutStates"
    ],
    "relationships": [
      "WorkoutTemplates → WorkoutCategories",
      "WorkoutTemplates → WorkoutObjectives",
      "WorkoutTemplates → ExecutionProtocols",
      "WorkoutTemplates → WorkoutStates",
      "WorkoutTemplateExercises → Exercises",
      "SetConfigurations → ExecutionProtocols"
    ]
  }
  ```
- **Business Logic**: State transition validation, exercise suggestion algorithm, equipment aggregation

### Admin Project
- **UI Components**: 
  - Workout template list/grid view
  - Template creation/edit wizard
  - Exercise selection modal with intelligent suggestions
  - Set configuration editor
  - State transition controls
- **Routes**: 
  - /workout-templates - List view
  - /workout-templates/create - Creation wizard
  - /workout-templates/{id}/edit - Edit existing template
  - /workout-templates/{id}/preview - Preview mode
- **User Workflows**: 
  - Template creation with step-by-step wizard
  - Drag-and-drop exercise ordering
  - Bulk exercise operations
  - Template duplication
- **UI Requirements**: 
  - Responsive design for tablet and desktop
  - Accessible form controls
  - Real-time validation feedback
  - Auto-save functionality

### Clients Project
- **Platforms**: Web | Mobile (Android/iOS) | Desktop
- **Implementation Status**:
  - Web: Planned
  - Android: Planned
  - iOS: Planned
  - Desktop: Planned
- **Platform-Specific Considerations**: 
  - Mobile: Optimized exercise selection for touch interfaces
  - Desktop: Keyboard shortcuts for quick navigation
  - All platforms: Offline template viewing capability

## Request/Response Examples

### Example 1: Create Workout Template
**Request**:
```http
POST /api/workout-templates
Content-Type: application/json

{
  "name": "Upper Body Strength Day",
  "description": "Focus on compound movements for upper body strength development",
  "workoutCategoryId": "123e4567-e89b-12d3-a456-426614174001",
  "workoutObjectiveId": "123e4567-e89b-12d3-a456-426614174002",
  "executionProtocolId": "123e4567-e89b-12d3-a456-426614174003",
  "estimatedDuration": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "tags": ["strength", "upper-body", "compound"]
}
```

**Success Response (201 Created)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Upper Body Strength Day",
  "description": "Focus on compound movements for upper body strength development",
  "workoutCategoryId": "123e4567-e89b-12d3-a456-426614174001",
  "workoutObjectiveId": "123e4567-e89b-12d3-a456-426614174002",
  "executionProtocolId": "123e4567-e89b-12d3-a456-426614174003",
  "estimatedDuration": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "creatorId": "123e4567-e89b-12d3-a456-426614174004",
  "createdAt": "2025-07-22T10:00:00Z",
  "lastModified": "2025-07-22T10:00:00Z",
  "version": "1.0.0",
  "isActive": true,
  "tags": ["strength", "upper-body", "compound"],
  "workoutStateId": "123e4567-e89b-12d3-a456-426614174005"
}
```

**Error Response (400 Bad Request)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "name": ["Name must be between 3 and 100 characters"],
    "workoutCategoryId": ["Invalid workout category"]
  }
}
```

### Example 2: Add Exercise to Template
**Request**:
```http
POST /api/workout-templates/123e4567-e89b-12d3-a456-426614174000/exercises
Content-Type: application/json

{
  "exerciseId": "123e4567-e89b-12d3-a456-426614174006",
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "Focus on controlled movement, 2-1-2 tempo"
}
```

**Success Response (201 Created)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174007",
  "workoutTemplateId": "123e4567-e89b-12d3-a456-426614174000",
  "exerciseId": "123e4567-e89b-12d3-a456-426614174006",
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "Focus on controlled movement, 2-1-2 tempo",
  "exercise": {
    "id": "123e4567-e89b-12d3-a456-426614174006",
    "name": "Bench Press",
    "equipment": ["Barbell", "Bench"]
  }
}
```

## Error Handling

### Error Codes
| Code | Meaning | User Message |
|------|---------|--------------|
| 400  | Invalid request data | Please check your input and try again |
| 403  | Insufficient permissions | You don't have permission to perform this action |
| 404  | Template not found | The requested workout template was not found |
| 409  | State transition conflict | Cannot change state due to existing execution logs |

## Security Considerations
- **Data Privacy**: Templates marked as private are only visible to the creator
- **Audit Trail**: All template modifications are logged with timestamp and user ID
- **Role-Based Access**: Personal Trainer role required for create/update/delete operations

## Dependencies

### External Dependencies
- None

### Internal Dependencies
- Exercise Management feature (for exercise library)
- User Management feature (for creator/trainer information)

### Reference Data
- WorkoutCategory (Upper Body, Lower Body, Full Body, etc.)
- WorkoutObjective (Muscular Strength, Endurance, Power, etc.)
- ExecutionProtocol (Standard, AMRAP, EMOM, etc.)
- WorkoutState (DRAFT, PRODUCTION, ARCHIVED)

## Migration Plan
1. Create database tables for WorkoutTemplates, WorkoutTemplateExercises, SetConfigurations
2. Seed WorkoutStates reference data
3. Migrate existing workout data if applicable
4. Update user permissions to include Personal Trainer role
5. Deploy API endpoints
6. Release Admin UI for template management
7. Update client applications to support template execution

## Testing Requirements

### Unit Tests
- WorkoutTemplate entity validation
- State transition logic
- Exercise suggestion algorithm
- Equipment aggregation logic
- Business rule enforcement

### Integration Tests
- Full template creation workflow
- Exercise addition with warmup/cooldown associations
- State transitions with execution log validation
- Permission checks for different user roles

### E2E Tests
- Complete template creation from Admin UI
- Template assignment to client
- Client workout execution from template
- Historical data preservation after archiving

## Documentation

### User Documentation
- Personal Trainer guide for template creation
- Client guide for executing workout templates

### Developer Documentation
- API endpoint documentation with examples
- State machine diagram for workout states
- Exercise suggestion algorithm documentation

## Future Enhancements

### Authentication and Authorization
The workout template system will include comprehensive authentication and authorization features in future releases:
- Role-based access control for different user tiers (admin, workout_tester, etc.)
- Claims-based authorization for workout execution
- Restricted access to DRAFT workouts for testing purposes
- Tiered access control for premium features

Note: These authentication features are planned for future implementation and are not part of the current core feature set. The current implementation focuses on the fundamental workout template structure and management capabilities.

### Phase 2 Enhancements
- Template sharing marketplace
- AI-powered exercise suggestions
- Template performance analytics
- Collaborative template editing

### Phase 3 Enhancements
- Template versioning with diff view
- Template scheduling and periodization
- Integration with wearable devices
- Automated progression algorithms

## Related Features
- **Exercise Management** - Provides the exercise library used in templates
- **Workout Execution** - Uses templates as blueprints for actual workouts
- **Progress Tracking** - Analyzes performance against template targets
- **Client Management** - Assigns templates to specific clients

## Changelog
| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0   | 2025-07-22 | Initial release | GetFitterGetBigger Team |

## Notes
The Workout Template Core feature provides a flexible yet structured approach to workout creation. By treating workouts as goal-oriented containers for exercises, supporting intelligent exercise selection, and automatically managing equipment and warmup/cooldown relationships, the system enables trainers to create effective, safe workout programs.

The integration with execution protocols provides future extensibility while the initial Standard protocol implementation ensures immediate usability. The approach of treating rest as exercises and maintaining contextual notes ensures maximum flexibility while keeping the data model clean and intuitive.

### Technology Stack Reference
While this feature documentation remains technology-agnostic, the current implementation uses:
- API: C# Minimal API
- Admin: C# Blazor  
- Clients: C# Avalonia (Android, iOS, Desktop, Web)

Note: Feature documentation focuses on business requirements and uses JSON for data models to maintain technology independence.