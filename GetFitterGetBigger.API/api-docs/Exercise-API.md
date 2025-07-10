# Exercise API Documentation

## Overview

The Exercise API provides endpoints for managing exercises in the fitness tracking system. Exercises are the building blocks of workout plans and include detailed information about movements, muscle groups, equipment, and weight requirements.

## Data Model

### Exercise Response DTO

```json
{
  "id": "exercise-123...",
  "name": "Barbell Bench Press",
  "description": "Classic chest exercise for building upper body strength",
  "coachNotes": [
    {
      "id": "coachnote-456...",
      "text": "Keep your feet flat on the floor",
      "order": 0
    },
    {
      "id": "coachnote-789...",
      "text": "Lower the bar to your chest with control",
      "order": 1
    }
  ],
  "exerciseTypes": [
    {
      "id": "exercisetype-...",
      "value": "Strength",
      "description": "Exercises focused on building strength"
    }
  ],
  "videoUrl": "https://example.com/bench-press-demo.mp4",
  "imageUrl": "https://example.com/bench-press.jpg",
  "isUnilateral": false,
  "isActive": true,
  "difficulty": {
    "id": "difficultylevel-...",
    "value": "Intermediate",
    "description": "For users with some training experience"
  },
  "kineticChain": {
    "id": "kineticchaintype-...",
    "value": "Open Chain",
    "description": "Movement where the distal segment moves freely"
  },
  "exerciseWeightType": {
    "id": "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
    "value": "Weight Required",
    "description": "Exercises that must have external weight specified"
  },
  "muscleGroups": [
    {
      "muscleGroup": {
        "id": "musclegroup-...",
        "value": "Chest",
        "description": null
      },
      "role": {
        "id": "musclerole-...",
        "value": "Primary",
        "description": "Main muscle group targeted"
      }
    },
    {
      "muscleGroup": {
        "id": "musclegroup-...",
        "value": "Triceps",
        "description": null
      },
      "role": {
        "id": "musclerole-...",
        "value": "Secondary",
        "description": "Supporting muscle group"
      }
    }
  ],
  "equipment": [
    {
      "id": "equipment-...",
      "value": "Barbell",
      "description": null
    },
    {
      "id": "equipment-...",
      "value": "Bench",
      "description": null
    }
  ],
  "movementPatterns": [
    {
      "id": "movementpattern-...",
      "value": "Horizontal Push",
      "description": "Pushing movement in the horizontal plane"
    }
  ],
  "bodyParts": [
    {
      "id": "bodypart-...",
      "value": "Upper Body",
      "description": "Exercises targeting the upper body"
    }
  ]
}
```

## Endpoints

### GET /api/Exercises

Retrieves a paginated list of exercises with optional filtering.

**Query Parameters:**
- `page` (integer, default: 1): Page number
- `pageSize` (integer, default: 10): Items per page
- `name` (string, optional): Filter by exercise name (partial match, case-insensitive)
- `difficultyId` (string, optional): Filter by difficulty level ID
- `muscleGroupIds` (array, optional): Filter by muscle group IDs
- `equipmentIds` (array, optional): Filter by equipment IDs
- `movementPatternIds` (array, optional): Filter by movement pattern IDs
- `bodyPartIds` (array, optional): Filter by body part IDs
- `includeInactive` (boolean, default: false): Include inactive exercises

**Response:**
```json
{
  "items": [ /* Array of Exercise DTOs */ ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### GET /api/Exercises/{id}

Retrieves a specific exercise by ID.

**Parameters:**
- `id` (string, required): The exercise ID

**Response:** Exercise DTO (see data model above)

**Error Responses:**
- `404 Not Found`: Exercise not found

### POST /api/Exercises

Creates a new exercise.

**Request Body:**
```json
{
  "name": "Bulgarian Split Squat",
  "description": "Single-leg exercise for leg strength and balance",
  "coachNotes": [
    {
      "text": "Keep your torso upright",
      "order": 0
    },
    {
      "text": "Drive through your front heel",
      "order": 1
    }
  ],
  "exerciseTypeIds": ["exercisetype-..."],
  "videoUrl": "https://example.com/bulgarian-split-squat.mp4",
  "imageUrl": "https://example.com/bulgarian-split-squat.jpg",
  "isUnilateral": true,
  "difficultyId": "difficultylevel-...",
  "kineticChainId": "kineticchaintype-...",
  "exerciseWeightTypeId": "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
  "muscleGroups": [
    {
      "muscleGroupId": "musclegroup-...",
      "muscleRoleId": "musclerole-..."
    }
  ],
  "equipmentIds": ["equipment-..."],
  "movementPatternIds": ["movementpattern-..."],
  "bodyPartIds": ["bodypart-..."]
}
```

**Important Notes:**
- `exerciseWeightTypeId` is optional. If not specified, the exercise will not have weight validation rules.
- `kineticChainId` is required for non-REST exercises, must be null for REST exercises
- At least one muscle group is required for non-REST exercises

**Response:** Created Exercise DTO

**Error Responses:**
- `400 Bad Request`: Validation errors
- `409 Conflict`: Exercise with the same name already exists

### PUT /api/Exercises/{id}

Updates an existing exercise.

**Parameters:**
- `id` (string, required): The exercise ID

**Request Body:** Same as POST request, with these differences:
- `isActive` (boolean, optional): Set exercise active/inactive status
- `isUnilateral` (boolean, optional): If not provided, keeps existing value
- `exerciseWeightTypeId` (string, optional): Update the exercise's weight type. If not provided, keeps existing value
- Coach notes with IDs will update existing notes, without IDs will create new ones

**Example PUT Request:**
```json
{
  "name": "Updated Bulgarian Split Squat",
  "description": "Single-leg exercise for leg strength and balance - updated",
  "coachNotes": [
    {
      "id": "coachnote-123...",
      "text": "Keep your torso upright throughout the movement",
      "order": 0
    },
    {
      "text": "New tip: Focus on slow eccentric phase",
      "order": 1
    }
  ],
  "exerciseTypeIds": ["exercisetype-..."],
  "videoUrl": "https://example.com/bulgarian-split-squat-v2.mp4",
  "imageUrl": "https://example.com/bulgarian-split-squat-v2.jpg",
  "isUnilateral": true,
  "isActive": true,
  "difficultyId": "difficultylevel-...",
  "kineticChainId": "kineticchaintype-...",
  "exerciseWeightTypeId": "exerciseweighttype-1b3d5f7a-5b7c-4d8e-9f0a-1b2c3d4e5f6a",
  "muscleGroups": [
    {
      "muscleGroupId": "musclegroup-...",
      "muscleRoleId": "musclerole-..."
    }
  ],
  "equipmentIds": ["equipment-..."],
  "movementPatternIds": ["movementpattern-..."],
  "bodyPartIds": ["bodypart-..."]
}
```

**Response:** Updated Exercise DTO

**Error Responses:**
- `400 Bad Request`: Validation errors
- `404 Not Found`: Exercise not found
- `409 Conflict`: Another exercise with the same name already exists

### DELETE /api/Exercises/{id}

Deletes an exercise. If the exercise has references (e.g., in workout logs), it will be soft-deleted (marked as inactive). Otherwise, it will be permanently deleted.

**Parameters:**
- `id` (string, required): The exercise ID

**Response:** 
- `200 OK`: Exercise deleted successfully
- `404 Not Found`: Exercise not found

## Exercise Weight Type Integration

### Weight Type Assignment

When creating or updating an exercise, you can assign an `exerciseWeightTypeId` to define how weight should be handled:

```json
{
  "exerciseWeightTypeId": "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"
}
```

### Weight Validation Rules

The assigned weight type determines validation rules when logging workout sets:

| Weight Type | Rule | Example Exercises |
|-------------|------|-------------------|
| Bodyweight Only | Weight must be null or 0 | Pull-ups, Push-ups |
| Bodyweight Optional | Weight ≥ 0 or null | Dips, Chin-ups |
| Weight Required | Weight > 0 | Barbell Bench Press, Deadlift |
| Machine Weight | Weight > 0 | Leg Press, Cable Row |
| No Weight | Weight must be null or 0 | Plank, Stretching |

### Legacy Support

Exercises without a weight type (null `exerciseWeightTypeId`) will accept any weight value for backward compatibility.

## Validation Rules

1. **Name**: Required, max 200 characters, must be unique
2. **Description**: Required, max 1000 characters
3. **Exercise Types**: At least one required, REST type cannot be combined with others
4. **Muscle Groups**: Required for non-REST exercises
5. **Kinetic Chain**: Required for non-REST exercises, must be null for REST exercises
6. **URLs**: Must be valid URLs if provided

## Related Endpoints

- `/api/ReferenceTables/DifficultyLevels` - Get available difficulty levels
- `/api/ReferenceTables/ExerciseTypes` - Get available exercise types
- `/api/ReferenceTables/KineticChainTypes` - Get available kinetic chain types
- `/api/ReferenceTables/ExerciseWeightTypes` - Get available weight types
- `/api/MuscleGroups` - Get available muscle groups
- `/api/Equipment` - Get available equipment
- `/api/ReferenceTables/MovementPatterns` - Get available movement patterns
- `/api/ReferenceTables/BodyParts` - Get available body parts
- `/api/ReferenceTables/MuscleRoles` - Get available muscle roles

## Authorization

Currently, these endpoints do not require authentication. When authorization is implemented:
- GET endpoints will be available to all authenticated users
- POST/PUT/DELETE will require Personal Trainer (PT-Tier) or Admin claims