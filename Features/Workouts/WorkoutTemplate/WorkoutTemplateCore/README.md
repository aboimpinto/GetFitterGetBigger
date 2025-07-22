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
The Workout Template Core feature provides the comprehensive infrastructure for creating, managing, and executing structured workout blueprints that serve as the foundation for actual workout logging and client training programs. This feature enables personal trainers to encapsulate their expertise and programming knowledge into reusable formats that ensure consistency across training sessions, enable progressive overload tracking, and provide clear guidance for both trainers and clients.

A workout template is a reusable blueprint that defines exercises organized to achieve specific fitness goals. The template serves as a container that brings exercises together based on a chosen workout category (such as Upper Body or Lower Body), targeting specific objectives (like Muscular Strength or Endurance) through a defined execution protocol (Standard, AMRAP, EMOM, etc.).

### Target Users
- **Primary**: Personal Trainers - Need to create structured, reusable workout programs for multiple clients efficiently
- **Secondary**: Fitness Enthusiasts - Want to access and execute professionally designed workout templates

### Success Metrics
- Template creation time reduced by 50% compared to manual programming
- 80% of personal trainers creating at least 5 templates per month
- 90% client satisfaction with workout template structure and execution flow

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
    "estimatedDurationMinutes": "number",
    "difficultyLevel": "string (Beginner|Intermediate|Advanced)",
    "isPublic": "boolean",
    "createdByUserId": "string (guid)",
    "createdAt": "datetime",
    "lastModifiedAt": "datetime",
    "version": "string",
    "isActive": "boolean",
    "tags": ["string"],
    "relationships": {
      "workoutCategory": "WorkoutCategory",
      "workoutObjective": "WorkoutObjective",
      "executionProtocol": "ExecutionProtocol",
      "exercises": ["WorkoutTemplateExercise"]
    }
  },
  "WorkoutTemplateExercise": {
    "id": "string (guid)",
    "workoutTemplateId": "string (guid)",
    "exerciseId": "string (guid)",
    "zone": "string (Warmup|Main|Cooldown)",
    "sequenceOrder": "number",
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
    "configurationOrder": "number",
    "executionProtocolId": "string (guid)",
    "targetSets": "number",
    "targetReps": "string",
    "targetDurationSeconds": "number",
    "intensityGuideline": "string",
    "relationships": {
      "workoutTemplateExercise": "WorkoutTemplateExercise",
      "executionProtocol": "ExecutionProtocol"
    }
  }
}
```

### API Endpoints
| Method | Endpoint | Purpose | Auth Required | Claims |
|--------|----------|---------|---------------|--------|
| GET    | /api/workout-templates | List all workout templates | Yes | Free-Tier |
| GET    | /api/workout-templates/{id} | Get specific template details | Yes | Free-Tier |
| POST   | /api/workout-templates | Create new workout template | Yes | PT-Tier |
| PUT    | /api/workout-templates/{id} | Update workout template | Yes | PT-Tier |
| DELETE | /api/workout-templates/{id} | Delete workout template | Yes | PT-Tier |
| GET    | /api/workout-templates/{id}/exercises | Get template exercises | Yes | Free-Tier |
| POST   | /api/workout-templates/{id}/exercises | Add exercise to template | Yes | PT-Tier |
| PUT    | /api/workout-templates/{id}/exercises/{exerciseId} | Update exercise in template | Yes | PT-Tier |
| DELETE | /api/workout-templates/{id}/exercises/{exerciseId} | Remove exercise from template | Yes | PT-Tier |
| POST   | /api/workout-templates/{id}/exercises/{exerciseId}/sets | Configure sets for exercise | Yes | PT-Tier |

### Business Rules
1. Warmup exercises must precede Main exercises in sequence
2. Cooldown exercises must follow Main exercises in sequence
3. When an exercise with warmup/cooldown associations is added to Main zone, associated exercises are automatically suggested
4. Equipment requirements are automatically aggregated from selected exercises
5. Rest periods are implemented as special exercises in the workflow
6. Template must have at least one exercise in the Main zone to be valid
7. Execution protocol selection determines how set configurations are interpreted
8. Templates can be public (available to all) or private (creator only)

### Validation Rules
- **Name**: Required, max 100 characters
- **Description**: Optional, max 500 characters
- **Workout Category**: Required, must exist in reference data
- **Workout Objective**: Required, must exist in reference data
- **Execution Protocol**: Required, must exist in reference data
- **Estimated Duration**: Required, must be between 10-240 minutes
- **Difficulty Level**: Required, must be one of: Beginner, Intermediate, Advanced
- **Exercise Zone**: Required for each exercise, must be one of: Warmup, Main, Cooldown
- **Sequence Order**: Required, must be unique within zone
- **Target Sets**: Required for set configuration, must be positive integer
- **Target Reps**: Optional, format can be range (e.g., "8-12") or fixed number

## Implementation Details

### API Project
- **Endpoints Implemented**: Full CRUD for workout templates, exercises, and set configurations
- **Data Models**: WorkoutTemplateDto, WorkoutTemplateExerciseDto, SetConfigurationDto
- **Database Changes**: 
  ```json
  {
    "tables": [
      "WorkoutTemplates",
      "WorkoutTemplateExercises", 
      "SetConfigurations"
    ],
    "indexes": [
      "IX_WorkoutTemplates_CreatedByUserId",
      "IX_WorkoutTemplates_IsPublic_IsActive",
      "IX_WorkoutTemplateExercises_WorkoutTemplateId_Zone_SequenceOrder"
    ]
  }
  ```
- **Business Logic**: Automatic warmup/cooldown suggestion, equipment aggregation, zone sequence validation

### Admin Project
- **UI Components**: 
  - WorkoutTemplateList (searchable, filterable grid)
  - WorkoutTemplateEditor (multi-step form)
  - ExerciseSelector (category-based with intelligent suggestions)
  - SetConfigurationEditor (protocol-aware interface)
  - ZoneManager (drag-drop exercise organization)
- **Routes**: 
  - /admin/workout-templates (list view)
  - /admin/workout-templates/new (creation)
  - /admin/workout-templates/{id}/edit (editing)
- **User Workflows**: Template creation wizard, exercise selection with suggestions, zone-based organization
- **UI Requirements**: Responsive design, drag-drop support for exercise ordering, real-time validation

### Clients Project
- **Platforms**: Web | Mobile (Android/iOS) | Desktop
- **Implementation Status**:
  - Web: Planned
  - Android: Planned
  - iOS: Planned
  - Desktop: Planned
- **Platform-Specific Considerations**: 
  - Mobile: Optimized exercise selection for touch
  - Desktop: Keyboard shortcuts for rapid template creation
  - All platforms: Offline template caching for execution

## Request/Response Examples

### Example 1: Create Workout Template
**Request**:
```http
POST /api/workout-templates
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Upper Body Strength Day",
  "description": "Focus on compound movements for upper body strength development",
  "workoutCategoryId": "123e4567-e89b-12d3-a456-426614174000",
  "workoutObjectiveId": "234e5678-e89b-12d3-a456-426614174000",
  "executionProtocolId": "345e6789-e89b-12d3-a456-426614174000",
  "estimatedDurationMinutes": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "tags": ["strength", "upper-body", "compound"]
}
```

**Success Response (201 Created)**:
```json
{
  "id": "456e789a-e89b-12d3-a456-426614174000",
  "name": "Upper Body Strength Day",
  "description": "Focus on compound movements for upper body strength development",
  "workoutCategoryId": "123e4567-e89b-12d3-a456-426614174000",
  "workoutObjectiveId": "234e5678-e89b-12d3-a456-426614174000",
  "executionProtocolId": "345e6789-e89b-12d3-a456-426614174000",
  "estimatedDurationMinutes": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "createdByUserId": "567e89ab-e89b-12d3-a456-426614174000",
  "createdAt": "2025-07-22T10:00:00Z",
  "lastModifiedAt": "2025-07-22T10:00:00Z",
  "version": "1.0.0",
  "isActive": true,
  "tags": ["strength", "upper-body", "compound"]
}
```

**Error Response (400 Bad Request)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "name": ["The name field is required"],
    "workoutCategoryId": ["Invalid workout category"]
  }
}
```

### Example 2: Add Exercise to Template
**Request**:
```http
POST /api/workout-templates/456e789a-e89b-12d3-a456-426614174000/exercises
Authorization: Bearer [token]
Content-Type: application/json

{
  "exerciseId": "678e9abc-e89b-12d3-a456-426614174000",
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "Focus on controlled eccentric phase",
  "setConfigurations": [
    {
      "configurationOrder": 1,
      "executionProtocolId": "345e6789-e89b-12d3-a456-426614174000",
      "targetSets": 4,
      "targetReps": "6-8",
      "intensityGuideline": "Leave 1-2 reps in reserve"
    }
  ]
}
```

**Success Response (201 Created)**:
```json
{
  "id": "789eabcd-e89b-12d3-a456-426614174000",
  "workoutTemplateId": "456e789a-e89b-12d3-a456-426614174000",
  "exerciseId": "678e9abc-e89b-12d3-a456-426614174000",
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "Focus on controlled eccentric phase",
  "exercise": {
    "id": "678e9abc-e89b-12d3-a456-426614174000",
    "name": "Bench Press",
    "category": "Upper Body",
    "primaryMuscleGroups": ["Chest", "Triceps"],
    "equipment": ["Barbell", "Bench"]
  },
  "setConfigurations": [
    {
      "id": "89abcdef-e89b-12d3-a456-426614174000",
      "configurationOrder": 1,
      "executionProtocolId": "345e6789-e89b-12d3-a456-426614174000",
      "targetSets": 4,
      "targetReps": "6-8",
      "intensityGuideline": "Leave 1-2 reps in reserve"
    }
  ],
  "suggestedWarmups": [
    {
      "exerciseId": "9abcdef0-e89b-12d3-a456-426614174000",
      "exerciseName": "Bench Press - Light Weight",
      "zone": "Warmup"
    }
  ]
}
```

## Error Handling

### Error Codes
| Code | Meaning | User Message |
|------|---------|--------------|
| 400  | Invalid request data | "Please check your input and try again" |
| 401  | Not authenticated | "Please log in to continue" |
| 403  | Insufficient permissions | "You need Personal Trainer access to perform this action" |
| 404  | Template not found | "The requested workout template was not found" |
| 409  | Conflict (e.g., duplicate sequence) | "An exercise already exists at this position" |

## Security Considerations
- **Authentication**: Required for all endpoints
- **Authorization**: 
  - PT-Tier claims required for create/update/delete operations
  - Free-Tier can view public templates
  - Users can only modify their own templates
- **Data Privacy**: Private templates only visible to creator
- **Audit Trail**: All template modifications logged with user and timestamp

## Dependencies

### External Dependencies
- None

### Internal Dependencies
- Exercise Management feature (for exercise library)
- User Management feature (for creator tracking)
- Authentication feature (for access control)

### Reference Data
- WorkoutCategory table
- WorkoutObjective table
- ExecutionProtocol table
- DifficultyLevel enumeration

## Migration Plan
1. Create database tables for WorkoutTemplates, WorkoutTemplateExercises, SetConfigurations
2. Establish foreign key relationships with Exercise and Reference tables
3. Create indexes for performance optimization
4. Seed sample templates for testing
5. Implement API endpoints with validation
6. Deploy Admin UI for template management
7. Release client applications with template execution

## Testing Requirements

### Unit Tests
- Template creation with valid/invalid data
- Exercise addition with zone validation
- Set configuration with protocol validation
- Equipment aggregation logic
- Warmup/cooldown suggestion algorithm

### Integration Tests
- Complete template creation workflow
- Exercise reordering within zones
- Template duplication functionality
- Public/private template filtering
- Authorization enforcement

### E2E Tests
- Personal Trainer creates new template
- Client browses and selects template for workout
- Template modification and versioning
- Exercise substitution based on equipment

## Documentation

### User Documentation
- Personal Trainer guide for template creation
- Client guide for template selection and execution

### Developer Documentation
- API endpoint documentation with examples
- Data model relationships diagram
- Business logic flow charts

## Future Enhancements
- **Phase 2**: Template sharing marketplace
- **Phase 3**: AI-powered template generation based on goals
- **Phase 4**: Template performance analytics

## Related Features
- Exercise Management - Provides exercise library for template construction
- Workout Logging - Uses templates as basis for actual workout execution
- Progress Tracking - Analyzes performance against template targets

## Changelog
| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0   | 2025-07-22 | Initial release | GetFitterGetBigger Team |

## Notes
The Workout Template Core feature implements intelligent exercise selection through category-based suggestions and automatic warmup/cooldown management. Rest periods are treated as special exercises for maximum flexibility. Equipment requirements are automatically compiled from selected exercises.

### Technology Stack Reference
While this feature documentation remains technology-agnostic, the current implementation uses:
- API: C# Minimal API
- Admin: C# Blazor  
- Clients: C# Avalonia (Android, iOS, Desktop, Web)

Note: Feature documentation focuses on business requirements and uses JSON for data models to maintain technology independence.