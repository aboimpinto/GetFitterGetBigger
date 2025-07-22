# Workout Template Core Data Models

## Overview
This document defines all data models and DTOs used in the Workout Template Core feature. All models are presented in a technology-agnostic JSON format.

## Core Entities

### WorkoutTemplate
The main entity representing a reusable workout blueprint.

```json
{
  "WorkoutTemplate": {
    "id": "string (guid)",
    "name": "string",
    "description": "string | null",
    "workoutCategoryId": "string (guid)",
    "workoutObjectiveId": "string (guid)",
    "executionProtocolId": "string (guid)",
    "estimatedDuration": "integer",
    "difficultyLevel": "string",
    "isPublic": "boolean",
    "creatorId": "string (guid)",
    "createdAt": "datetime",
    "lastModified": "datetime",
    "version": "string",
    "isActive": "boolean",
    "tags": ["string"],
    "workoutStateId": "string (guid)"
  }
}
```

**Field Specifications**:
- `id`: Unique identifier, system-generated
- `name`: Display name, 3-100 characters
- `description`: Optional detailed description, max 1000 characters
- `workoutCategoryId`: Foreign key to WorkoutCategory reference table
- `workoutObjectiveId`: Foreign key to WorkoutObjective reference table
- `executionProtocolId`: Foreign key to ExecutionProtocol reference table
- `estimatedDuration`: Time in minutes (5-300)
- `difficultyLevel`: Enum - "Beginner" | "Intermediate" | "Advanced"
- `isPublic`: Visibility flag
- `creatorId`: Foreign key to User entity
- `createdAt`: UTC timestamp of creation
- `lastModified`: UTC timestamp of last update
- `version`: Semantic version (e.g., "1.0.0")
- `isActive`: Soft delete flag
- `tags`: Array of searchable tags
- `workoutStateId`: Foreign key to WorkoutState reference table

### WorkoutTemplateExercise
Represents an exercise within a workout template.

```json
{
  "WorkoutTemplateExercise": {
    "id": "string (guid)",
    "workoutTemplateId": "string (guid)",
    "exerciseId": "string (guid)",
    "zone": "string",
    "sequenceOrder": "integer",
    "exerciseNotes": "string | null"
  }
}
```

**Field Specifications**:
- `id`: Unique identifier, system-generated
- `workoutTemplateId`: Foreign key to WorkoutTemplate
- `exerciseId`: Foreign key to Exercise entity
- `zone`: Enum - "Warmup" | "Main" | "Cooldown"
- `sequenceOrder`: Order within zone (1-based)
- `exerciseNotes`: Optional context-specific notes, max 500 characters

### SetConfiguration
Defines how an exercise should be performed.

```json
{
  "SetConfiguration": {
    "id": "string (guid)",
    "workoutTemplateExerciseId": "string (guid)",
    "configurationOrder": "integer",
    "executionProtocolId": "string (guid)",
    "targetSets": "integer",
    "targetReps": "string | null",
    "targetDuration": "integer | null",
    "intensityGuideline": "string | null"
  }
}
```

**Field Specifications**:
- `id`: Unique identifier, system-generated
- `workoutTemplateExerciseId`: Foreign key to WorkoutTemplateExercise
- `configurationOrder`: Order when multiple configurations exist
- `executionProtocolId`: Foreign key to ExecutionProtocol
- `targetSets`: Number of sets (1-100)
- `targetReps`: Rep target (e.g., "8-12", "15", "AMRAP")
- `targetDuration`: Time in seconds for timed exercises
- `intensityGuideline`: Instructions (e.g., "RPE 7", "2 RIR")

## Reference Data Entities

### WorkoutState
Pure reference entity for workout lifecycle states.

```json
{
  "WorkoutState": {
    "workoutStateId": "string (guid)",
    "value": "string",
    "description": "string",
    "displayOrder": "integer",
    "isActive": "boolean"
  }
}
```

**Field Specifications**:
- `workoutStateId`: Unique identifier, system-generated
- `value`: State identifier (e.g., "DRAFT", "PRODUCTION", "ARCHIVED")
- `description`: Human-readable description of the state
- `displayOrder`: Order for UI display
- `isActive`: Whether this state is currently available

**Predefined Values**:
| Value | Description | Display Order |
|-------|-------------|---------------|
| DRAFT | Template under development. Personal Trainer can freely modify all aspects of the template. All execution logs are automatically deleted when changes are made. | 1 |
| PRODUCTION | Active template ready for use. The workout is finalized and available for public use. Cannot return to DRAFT if execution logs exist. | 2 |
| ARCHIVED | Retired template (view only). No user can execute an archived workout, but historical data is preserved for performance tracking. | 3 |

### WorkoutStateDto
Data transfer object for WorkoutState reference data.

```json
{
  "WorkoutStateDto": {
    "workoutStateId": "string (guid)",
    "value": "string",
    "description": "string",
    "displayOrder": "integer",
    "isActive": "boolean"
  }
}
```

**Usage**:
- Returned from GET endpoints for workout states
- Used in WorkoutTemplateDto for state information
- Cached eternally (365 days) as reference data

## Data Transfer Objects (DTOs)

### WorkoutTemplateDto
Used for API responses when listing templates.

```json
{
  "WorkoutTemplateDto": {
    "id": "string (guid)",
    "name": "string",
    "description": "string | null",
    "workoutCategory": {
      "id": "string (guid)",
      "value": "string",
      "description": "string"
    },
    "workoutObjective": {
      "id": "string (guid)",
      "value": "string",
      "description": "string"
    },
    "executionProtocol": {
      "id": "string (guid)",
      "value": "string",
      "description": "string"
    },
    "estimatedDuration": "integer",
    "difficultyLevel": "string",
    "isPublic": "boolean",
    "creator": {
      "id": "string (guid)",
      "name": "string",
      "email": "string"
    },
    "createdAt": "datetime",
    "lastModified": "datetime",
    "workoutState": {
      "id": "string (guid)",
      "value": "string",
      "description": "string"
    },
    "exerciseCount": "integer",
    "equipmentRequired": ["string"],
    "tags": ["string"]
  }
}
```

### WorkoutTemplateDetailDto
Used for API responses when retrieving a single template with full details.

```json
{
  "WorkoutTemplateDetailDto": {
    "id": "string (guid)",
    "name": "string",
    "description": "string | null",
    "workoutCategoryId": "string (guid)",
    "workoutObjectiveId": "string (guid)",
    "executionProtocolId": "string (guid)",
    "estimatedDuration": "integer",
    "difficultyLevel": "string",
    "isPublic": "boolean",
    "creatorId": "string (guid)",
    "createdAt": "datetime",
    "lastModified": "datetime",
    "version": "string",
    "isActive": "boolean",
    "tags": ["string"],
    "workoutStateId": "string (guid)",
    "exercises": [{
      "id": "string (guid)",
      "exerciseId": "string (guid)",
      "exercise": {
        "id": "string (guid)",
        "name": "string",
        "category": "string",
        "equipment": ["string"],
        "primaryMuscles": ["string"],
        "secondaryMuscles": ["string"],
        "difficulty": "string",
        "instructions": "string"
      },
      "zone": "string",
      "sequenceOrder": "integer",
      "exerciseNotes": "string | null",
      "setConfigurations": [{
        "id": "string (guid)",
        "configurationOrder": "integer",
        "executionProtocolId": "string (guid)",
        "executionProtocol": {
          "id": "string (guid)",
          "value": "string",
          "description": "string"
        },
        "targetSets": "integer",
        "targetReps": "string | null",
        "targetDuration": "integer | null",
        "intensityGuideline": "string | null"
      }]
    }],
    "equipmentRequired": ["string"]
  }
}
```

### CreateWorkoutTemplateDto
Used for creating new workout templates.

```json
{
  "CreateWorkoutTemplateDto": {
    "name": "string",
    "description": "string | null",
    "workoutCategoryId": "string (guid)",
    "workoutObjectiveId": "string (guid)",
    "executionProtocolId": "string (guid)",
    "estimatedDuration": "integer",
    "difficultyLevel": "string",
    "isPublic": "boolean",
    "tags": ["string"]
  }
}
```

### UpdateWorkoutTemplateDto
Used for updating existing workout templates.

```json
{
  "UpdateWorkoutTemplateDto": {
    "name": "string | null",
    "description": "string | null",
    "workoutCategoryId": "string (guid) | null",
    "workoutObjectiveId": "string (guid) | null",
    "executionProtocolId": "string (guid) | null",
    "estimatedDuration": "integer | null",
    "difficultyLevel": "string | null",
    "isPublic": "boolean | null",
    "tags": ["string"] | null,
    "isActive": "boolean | null"
  }
}
```

### AddExerciseDto
Used for adding exercises to templates.

```json
{
  "AddExerciseDto": {
    "exerciseId": "string (guid)",
    "zone": "string",
    "sequenceOrder": "integer",
    "exerciseNotes": "string | null"
  }
}
```

### CreateSetConfigurationDto
Used for creating set configurations.

```json
{
  "CreateSetConfigurationDto": {
    "configurationOrder": "integer",
    "executionProtocolId": "string (guid)",
    "targetSets": "integer",
    "targetReps": "string | null",
    "targetDuration": "integer | null",
    "intensityGuideline": "string | null"
  }
}
```

### ChangeWorkoutStateDto
Used for changing workout state.

```json
{
  "ChangeWorkoutStateDto": {
    "workoutStateId": "string (guid)"
  }
}
```

### ExerciseSuggestionDto
Used for exercise suggestions response.

```json
{
  "ExerciseSuggestionDto": {
    "exerciseId": "string (guid)",
    "name": "string",
    "category": "string",
    "reason": "string",
    "equipment": ["string"],
    "associatedWarmups": ["string (guid)"],
    "associatedCooldowns": ["string (guid)"],
    "relevanceScore": "number"
  }
}
```

## Validation Rules

### WorkoutTemplate Validation
- `name`: Required, 3-100 characters, no special characters except spaces and hyphens
- `description`: Optional, max 1000 characters
- `estimatedDuration`: Required, 5-300 minutes
- `difficultyLevel`: Required, must be one of: "Beginner", "Intermediate", "Advanced"
- `tags`: Optional, max 10 tags, each tag 2-30 characters

### WorkoutTemplateExercise Validation
- `zone`: Required, must be one of: "Warmup", "Main", "Cooldown"
- `sequenceOrder`: Required, positive integer, unique within zone
- `exerciseNotes`: Optional, max 500 characters

### SetConfiguration Validation
- `targetSets`: Required, 1-100
- `targetReps`: Required for rep-based exercises, format: number or range (e.g., "12" or "8-12")
- `targetDuration`: Required for time-based exercises, 1-3600 seconds
- `intensityGuideline`: Optional, max 100 characters

## Business Rules in Data Model

1. **State Transitions**:
   - New templates start in DRAFT state
   - DRAFT → PRODUCTION requires no validation errors
   - PRODUCTION → DRAFT only if no execution logs exist
   - Any state → ARCHIVED is always allowed

2. **Exercise Zones**:
   - Exercises must be organized by zone
   - Sequence order must be unique within each zone
   - Warmup exercises logically precede Main exercises
   - Cooldown exercises logically follow Main exercises

3. **Equipment Aggregation**:
   - Equipment list is automatically compiled from all exercises
   - No manual equipment entry at template level
   - Equipment requirements are read-only at template level

4. **Version Management**:
   - Version increments automatically on significant changes
   - Version format follows semantic versioning (MAJOR.MINOR.PATCH)

## Data Relationships

```
WorkoutTemplate
  ├── WorkoutCategory (Reference)
  ├── WorkoutObjective (Reference)
  ├── ExecutionProtocol (Reference)
  ├── WorkoutState (Reference)
  ├── User (Creator)
  └── WorkoutTemplateExercise[] (1:many)
       ├── Exercise (Reference)
       └── SetConfiguration[] (1:many)
            └── ExecutionProtocol (Reference)
```

## Notes

- All datetime values are stored and transmitted in UTC format
- GUIDs are used for all entity identifiers
- Soft deletes are implemented via `isActive` flag
- Reference data entities use eternal caching (365 days)
- Equipment lists are computed properties, not stored directly