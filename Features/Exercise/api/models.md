# Exercise Data Models

## Overview
This document defines all data models and structures used in the Exercise Management feature.

## Core Models

### Exercise
The main exercise entity containing all exercise information.

```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "coachNotes": "array<CoachNote>",
  "exerciseTypes": "array<ExerciseType>",
  "videoUrl": "string | null",
  "imageUrl": "string | null",
  "isUnilateral": "boolean",
  "isActive": "boolean",
  "difficulty": "DifficultyLevel",
  "muscleGroups": "array<MuscleGroupAssignment>",
  "equipment": "array<Equipment>",
  "movementPatterns": "array<MovementPattern>",
  "bodyParts": "array<BodyPart>",
  "kineticChain": "KineticChainType | null",
  "exerciseWeightType": "ExerciseWeightType",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### CoachNote
Individual coaching instruction for an exercise.

```json
{
  "id": "string",
  "text": "string",
  "order": "number"
}
```

### MuscleGroupAssignment
Defines how a muscle group is engaged in an exercise.

```json
{
  "muscleGroup": "MuscleGroup",
  "role": "string"
}
```

## Reference Models

### DifficultyLevel
```json
{
  "id": "string",
  "name": "string",
  "sortOrder": "number"
}
```

### ExerciseType
```json
{
  "id": "string",
  "name": "string",
  "description": "string"
}
```

### MuscleGroup
```json
{
  "id": "string",
  "name": "string",
  "category": "string"
}
```

### Equipment
```json
{
  "id": "string",
  "name": "string",
  "category": "string"
}
```

### MovementPattern
```json
{
  "id": "string",
  "name": "string",
  "description": "string"
}
```

### BodyPart
```json
{
  "id": "string",
  "name": "string"
}
```

### KineticChainType
```json
{
  "id": "string",
  "name": "string",
  "description": "string"
}
```

### ExerciseWeightType
```json
{
  "id": "string",
  "code": "string",
  "name": "string",
  "description": "string",
  "uiBehavior": "string",
  "validationRules": "string"
}
```

## Enumerations

### MuscleRole
Valid values for muscle engagement roles:
- `"Primary"` - Main muscle targeted
- `"Secondary"` - Supporting muscle
- `"Stabilizer"` - Stabilizing muscle

### ExerciseWeightTypeCode
Valid codes for exercise weight types:
- `"BODYWEIGHT_ONLY"` - No external weight allowed
- `"BODYWEIGHT_OPTIONAL"` - Can add weight to bodyweight
- `"WEIGHT_REQUIRED"` - Must specify external weight
- `"MACHINE_WEIGHT"` - Machine-based weight
- `"NO_WEIGHT"` - Weight not applicable (e.g., stretches)

## Request/Response DTOs

### CreateExerciseRequest
```json
{
  "name": "string",
  "description": "string",
  "coachNotes": [
    {
      "text": "string",
      "order": "number"
    }
  ],
  "exerciseTypeIds": ["string"],
  "videoUrl": "string | null",
  "imageUrl": "string | null",
  "isUnilateral": "boolean",
  "difficultyId": "string",
  "kineticChainId": "string | null",
  "exerciseWeightTypeId": "string",
  "muscleGroups": [
    {
      "muscleGroupId": "string",
      "muscleRoleId": "string"
    }
  ],
  "equipmentIds": ["string"],
  "bodyPartIds": ["string"],
  "movementPatternIds": ["string"]
}
```

### UpdateExerciseRequest
Same structure as CreateExerciseRequest.

### ExerciseListResponse
```json
{
  "items": ["Exercise"],
  "currentPage": "number",
  "pageSize": "number",
  "totalCount": "number",
  "totalPages": "number",
  "hasPreviousPage": "boolean",
  "hasNextPage": "boolean"
}
```

### ExerciseListItem
Simplified exercise model for list views.

```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "difficulty": {
    "id": "string",
    "name": "string"
  },
  "primaryMuscleGroups": ["string"],
  "imageUrl": "string | null",
  "isUnilateral": "boolean",
  "isActive": "boolean"
}
```

## Validation Rules

### Exercise Name
- Required
- Length: 1-100 characters
- Must be unique among active exercises
- Allowed characters: letters, numbers, spaces, hyphens, parentheses

### Description
- Required
- Minimum length: 10 characters
- Maximum length: 1000 characters

### Coach Notes
- Text: Required, 1-500 characters per note
- Order: Must be sequential starting from 1
- Maximum notes per exercise: 10

### URLs
- Must be valid HTTP/HTTPS URLs
- Maximum length: 500 characters
- Must point to allowed domains (configured in settings)

### Reference IDs
- All reference IDs must exist in their respective tables
- All referenced entities must be active

## Database Schema

### Exercises Table
```json
{
  "columns": {
    "Id": "GUID PRIMARY KEY",
    "Name": "NVARCHAR(100) NOT NULL",
    "Description": "NVARCHAR(1000) NOT NULL",
    "VideoUrl": "NVARCHAR(500) NULL",
    "ImageUrl": "NVARCHAR(500) NULL",
    "IsUnilateral": "BIT NOT NULL",
    "IsActive": "BIT NOT NULL DEFAULT 1",
    "DifficultyId": "INT NOT NULL FK",
    "KineticChainId": "INT NULL FK",
    "ExerciseWeightTypeId": "INT NOT NULL FK",
    "CreatedAt": "DATETIME2 NOT NULL",
    "UpdatedAt": "DATETIME2 NOT NULL",
    "CreatedBy": "NVARCHAR(450) NOT NULL",
    "UpdatedBy": "NVARCHAR(450) NOT NULL"
  },
  "indexes": [
    "IX_Exercises_Name",
    "IX_Exercises_IsActive",
    "IX_Exercises_DifficultyId"
  ]
}
```

### ExerciseCoachNotes Table
```json
{
  "columns": {
    "Id": "GUID PRIMARY KEY",
    "ExerciseId": "GUID NOT NULL FK",
    "Text": "NVARCHAR(500) NOT NULL",
    "Order": "INT NOT NULL"
  },
  "indexes": [
    "IX_ExerciseCoachNotes_ExerciseId_Order"
  ]
}
```

### ExerciseMuscleGroups Table
```json
{
  "columns": {
    "ExerciseId": "GUID NOT NULL FK",
    "MuscleGroupId": "INT NOT NULL FK",
    "MuscleRoleId": "INT NOT NULL FK"
  },
  "primaryKey": ["ExerciseId", "MuscleGroupId"],
  "indexes": [
    "IX_ExerciseMuscleGroups_MuscleGroupId"
  ]
}
```