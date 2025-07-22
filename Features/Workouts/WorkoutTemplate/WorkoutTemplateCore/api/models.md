# Workout Template Core Data Models

## Overview
This document defines all data models, DTOs, and request/response objects for the Workout Template Core feature.

## Core Entities

### WorkoutTemplate
The main workout template entity containing metadata and configuration.

```json
{
  "id": "string (guid)",
  "name": "string",
  "description": "string",
  "workoutCategoryId": "string (guid)",
  "workoutObjectiveId": "string (guid)",
  "executionProtocolId": "string (guid)",
  "estimatedDurationMinutes": "number",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "createdByUserId": "string (guid)",
  "createdAt": "datetime",
  "lastModifiedAt": "datetime",
  "version": "string",
  "isActive": "boolean",
  "tags": ["string"]
}
```

**Field Specifications**:
- `name`: Required, 1-100 characters
- `description`: Optional, max 500 characters
- `difficultyLevel`: Enum - "Beginner", "Intermediate", "Advanced"
- `estimatedDurationMinutes`: Range 10-240
- `version`: Semantic versioning format (e.g., "1.0.0")
- `tags`: Array of strings, max 10 tags, each max 30 characters

### WorkoutTemplateExercise
Links exercises to workout templates with zone and sequence information.

```json
{
  "id": "string (guid)",
  "workoutTemplateId": "string (guid)",
  "exerciseId": "string (guid)",
  "zone": "string",
  "sequenceOrder": "number",
  "exerciseNotes": "string"
}
```

**Field Specifications**:
- `zone`: Enum - "Warmup", "Main", "Cooldown"
- `sequenceOrder`: Positive integer, unique within zone
- `exerciseNotes`: Optional, max 500 characters

### SetConfiguration
Defines how sets should be performed for each exercise.

```json
{
  "id": "string (guid)",
  "workoutTemplateExerciseId": "string (guid)",
  "configurationOrder": "number",
  "executionProtocolId": "string (guid)",
  "targetSets": "number",
  "targetReps": "string",
  "targetDurationSeconds": "number",
  "intensityGuideline": "string"
}
```

**Field Specifications**:
- `configurationOrder`: Positive integer for multiple configurations
- `targetSets`: Positive integer, max 50
- `targetReps`: String format supporting ranges (e.g., "8-12") or fixed values
- `targetDurationSeconds`: Optional, for time-based exercises
- `intensityGuideline`: Optional, max 200 characters

## Data Transfer Objects (DTOs)

### WorkoutTemplateDto
Used for listing templates with summary information.

```json
{
  "id": "string (guid)",
  "name": "string",
  "description": "string",
  "workoutCategory": {
    "id": "string (guid)",
    "name": "string"
  },
  "workoutObjective": {
    "id": "string (guid)",
    "name": "string"
  },
  "executionProtocol": {
    "id": "string (guid)",
    "name": "string"
  },
  "estimatedDurationMinutes": "number",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "createdByUser": {
    "id": "string (guid)",
    "name": "string"
  },
  "createdAt": "datetime",
  "exerciseCount": "number",
  "tags": ["string"]
}
```

### WorkoutTemplateDetailDto
Full template details including all exercises and configurations.

```json
{
  "id": "string (guid)",
  "name": "string",
  "description": "string",
  "workoutCategoryId": "string (guid)",
  "workoutCategory": {
    "id": "string (guid)",
    "name": "string",
    "description": "string"
  },
  "workoutObjectiveId": "string (guid)",
  "workoutObjective": {
    "id": "string (guid)",
    "name": "string",
    "description": "string"
  },
  "executionProtocolId": "string (guid)",
  "executionProtocol": {
    "id": "string (guid)",
    "name": "string",
    "description": "string",
    "isAvailable": "boolean"
  },
  "estimatedDurationMinutes": "number",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "createdByUserId": "string (guid)",
  "createdByUser": {
    "id": "string (guid)",
    "name": "string",
    "isPT": "boolean"
  },
  "createdAt": "datetime",
  "lastModifiedAt": "datetime",
  "version": "string",
  "isActive": "boolean",
  "tags": ["string"],
  "requiredEquipment": [
    {
      "id": "string (guid)",
      "name": "string"
    }
  ],
  "exercises": [
    {
      "id": "string (guid)",
      "exerciseId": "string (guid)",
      "exercise": {
        "id": "string (guid)",
        "name": "string",
        "category": "string",
        "primaryMuscleGroups": ["string"],
        "equipment": ["string"]
      },
      "zone": "string",
      "sequenceOrder": "number",
      "exerciseNotes": "string",
      "setConfigurations": [
        {
          "id": "string (guid)",
          "configurationOrder": "number",
          "executionProtocolId": "string (guid)",
          "targetSets": "number",
          "targetReps": "string",
          "targetDurationSeconds": "number",
          "intensityGuideline": "string"
        }
      ]
    }
  ]
}
```

### WorkoutTemplateExerciseDto
Exercise within a template with full details.

```json
{
  "id": "string (guid)",
  "exerciseId": "string (guid)",
  "exercise": {
    "id": "string (guid)",
    "name": "string",
    "category": "string",
    "primaryMuscleGroups": ["string"],
    "secondaryMuscleGroups": ["string"],
    "equipment": ["string"],
    "instructions": "string",
    "hasWarmups": "boolean",
    "hasCooldowns": "boolean"
  },
  "zone": "string",
  "sequenceOrder": "number",
  "exerciseNotes": "string",
  "setConfigurations": [{
    "id": "string (guid)",
    "configurationOrder": "number",
    "executionProtocolId": "string (guid)",
    "executionProtocol": {
      "id": "string (guid)",
      "name": "string",
      "abbreviation": "string"
    },
    "targetSets": "number",
    "targetReps": "string",
    "targetDurationSeconds": "number",
    "intensityGuideline": "string"
  }]
}
```

## Request Objects

### CreateWorkoutTemplateRequest
```json
{
  "name": "string",
  "description": "string",
  "workoutCategoryId": "string (guid)",
  "workoutObjectiveId": "string (guid)",
  "executionProtocolId": "string (guid)",
  "estimatedDurationMinutes": "number",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "tags": ["string"]
}
```

### UpdateWorkoutTemplateRequest
```json
{
  "name": "string",
  "description": "string",
  "workoutCategoryId": "string (guid)",
  "workoutObjectiveId": "string (guid)",
  "executionProtocolId": "string (guid)",
  "estimatedDurationMinutes": "number",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "tags": ["string"],
  "version": "string"
}
```

### AddExerciseToTemplateRequest
```json
{
  "exerciseId": "string (guid)",
  "zone": "string",
  "sequenceOrder": "number",
  "exerciseNotes": "string",
  "setConfigurations": [
    {
      "configurationOrder": "number",
      "executionProtocolId": "string (guid)",
      "targetSets": "number",
      "targetReps": "string",
      "targetDurationSeconds": "number",
      "intensityGuideline": "string"
    }
  ]
}
```

### UpdateExerciseInTemplateRequest
```json
{
  "zone": "string",
  "sequenceOrder": "number",
  "exerciseNotes": "string"
}
```

### UpdateSetConfigurationRequest
```json
{
  "setConfigurations": [
    {
      "configurationOrder": "number",
      "executionProtocolId": "string (guid)",
      "targetSets": "number",
      "targetReps": "string",
      "targetDurationSeconds": "number",
      "intensityGuideline": "string"
    }
  ]
}
```

### ReorderExercisesRequest
```json
{
  "exercises": [
    {
      "exerciseId": "string (guid)",
      "zone": "string",
      "sequenceOrder": "number"
    }
  ]
}
```

### DuplicateTemplateRequest
```json
{
  "name": "string",
  "isPublic": "boolean"
}
```

## Response Objects

### WorkoutTemplateListResponse
```json
{
  "items": [WorkoutTemplateDto],
  "totalCount": "number",
  "page": "number",
  "pageSize": "number",
  "totalPages": "number"
}
```

### AddExerciseToTemplateResponse
```json
{
  "exercise": WorkoutTemplateExerciseDto,
  "suggestedWarmups": [
    {
      "exerciseId": "string (guid)",
      "exerciseName": "string",
      "zone": "string"
    }
  ],
  "suggestedCooldowns": [
    {
      "exerciseId": "string (guid)",
      "exerciseName": "string",
      "zone": "string"
    }
  ]
}
```

### ExerciseSuggestionsResponse
```json
{
  "suggestions": [
    {
      "exerciseId": "string (guid)",
      "exerciseName": "string",
      "category": "string",
      "primaryMuscleGroups": ["string"],
      "equipment": ["string"],
      "relevanceScore": "number",
      "reason": "string"
    }
  ]
}
```

### TemplateValidationResponse
```json
{
  "isValid": "boolean",
  "warnings": [
    {
      "type": "string",
      "message": "string",
      "affectedExerciseId": "string (guid)"
    }
  ],
  "errors": [
    {
      "type": "string",
      "message": "string",
      "field": "string"
    }
  ]
}
```

### RemoveExerciseWarningResponse
```json
{
  "hasWarnings": "boolean",
  "warnings": [
    {
      "type": "LinkedExerciseRemoval",
      "message": "string",
      "linkedExercises": [
        {
          "exerciseId": "string (guid)",
          "exerciseName": "string",
          "linkType": "string"
        }
      ]
    }
  ]
}
```

## Enumerations

### DifficultyLevel
```json
{
  "values": ["Beginner", "Intermediate", "Advanced"]
}
```

### WorkoutZone
```json
{
  "values": ["Warmup", "Main", "Cooldown"]
}
```

### ValidationWarningType
```json
{
  "values": [
    "MissingWarmup",
    "MissingCooldown",
    "LinkedExerciseRemoval",
    "IncompleteSetConfiguration",
    "NoMainExercises"
  ]
}
```

### ExerciseSuggestionReason
```json
{
  "values": [
    "CategoryMatch",
    "ComplementaryMovement",
    "MuscleGroupBalance",
    "WarmupFor",
    "CooldownFor",
    "PopularCombination"
  ]
}
```

## Reference Data Models

### WorkoutCategory (Reference)
```json
{
  "id": "string (guid)",
  "name": "string",
  "description": "string",
  "isActive": "boolean"
}
```

### WorkoutObjective (Reference)
```json
{
  "id": "string (guid)",
  "name": "string",
  "description": "string",
  "isActive": "boolean"
}
```

### ExecutionProtocol (Reference)
```json
{
  "id": "string (guid)",
  "name": "string",
  "abbreviation": "string",
  "description": "string",
  "isAvailable": "boolean",
  "isActive": "boolean"
}
```

## Validation Rules Summary

### WorkoutTemplate
- Name: Required, 1-100 characters, trimmed
- Description: Optional, max 500 characters
- EstimatedDuration: 10-240 minutes
- At least one exercise in Main zone required
- Version must follow semantic versioning

### WorkoutTemplateExercise
- Zone required and valid
- SequenceOrder unique within zone
- Exercise must exist and be active
- Exercise notes max 500 characters

### SetConfiguration
- At least one configuration required per exercise
- TargetSets: 1-50
- TargetReps: Valid format (number or range)
- Either TargetReps or TargetDuration required
- ExecutionProtocol must be active

## Equipment Aggregation
Equipment is automatically compiled from all exercises in the template:
```json
{
  "requiredEquipment": [
    {
      "id": "string (guid)",
      "name": "string",
      "fromExercises": ["string"]
    }
  ]
}
```