# Exercise API Documentation - KineticChain Field Update

## Overview
This document describes the API changes for the KineticChain field addition to Exercise entities.

## Exercise DTO Structure

The `ExerciseDto` now includes the `kineticChain` field:

```json
{
  "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise",
  "videoUrl": "https://example.com/squat-video.mp4",
  "imageUrl": "https://example.com/squat-image.jpg",
  "isUnilateral": false,
  "difficulty": {
    "id": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
    "name": "Beginner",
    "description": "Suitable for those new to fitness",
    "order": 1,
    "isActive": true
  },
  "kineticChain": {
    "id": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
    "name": "Compound",
    "description": "Exercises that work multiple muscle groups",
    "order": 1,
    "isActive": true
  },
  "exerciseTypes": [...],
  "muscleGroups": [...],
  "equipment": [...],
  "bodyParts": [...],
  "movementPatterns": [...],
  "coachNotes": [...]
}
```

## API Endpoints

### POST /api/exercises - Create Exercise

**Authorization**: Required (`PT-Tier` or `Admin-Tier`)

#### Request Body Examples

##### Non-REST Exercise (KineticChainId Required)
```json
{
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise",
  "coachNotes": [
    { "text": "Stand with feet shoulder-width apart", "order": 0 },
    { "text": "Lower body by bending knees and hips", "order": 1 },
    { "text": "Push through heels to return to starting position", "order": 2 }
  ],
  "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
  "videoUrl": "https://example.com/squat-video.mp4",
  "imageUrl": "https://example.com/squat-image.jpg",
  "isUnilateral": false,
  "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
  "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
  "muscleGroups": [
    {
      "muscleGroupId": "musclegroup-eeff0011-2233-4455-6677-889900112233",
      "muscleRoleId": "musclerole-5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b"
    }
  ],
  "equipmentIds": ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
  "bodyPartIds": ["bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5"],
  "movementPatternIds": ["movementpattern-bbccddee-ff00-1122-3344-556677889900"]
}
```

##### REST Exercise (KineticChainId Must Be Null)
```json
{
  "name": "Rest Period",
  "description": "Rest between sets",
  "coachNotes": [
    { "text": "Take a 90-second rest", "order": 0 }
  ],
  "exerciseTypeIds": ["exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"],
  "isUnilateral": false,
  "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
  "kineticChainId": null,
  "muscleGroups": [],
  "equipmentIds": [],
  "bodyPartIds": [],
  "movementPatternIds": []
}
```

#### Response
- **201 Created**: Exercise created successfully
- **400 Bad Request**: Validation error (missing required fields, invalid KineticChainId)
- **401 Unauthorized**: No authentication
- **403 Forbidden**: User doesn't have PT-Tier or Admin-Tier
- **409 Conflict**: Exercise with same name already exists

### PUT /api/exercises/{id} - Update Exercise

**Authorization**: Required (`PT-Tier` or `Admin-Tier`)

#### Request Body Example
```json
{
  "name": "Bulgarian Split Squat",
  "description": "Unilateral leg exercise focusing on single leg strength",
  "coachNotes": [
    { "text": "Place rear foot on bench behind you", "order": 0 },
    { "text": "Lower into lunge position", "order": 1 },
    { "text": "Drive through front heel to return", "order": 2 }
  ],
  "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
  "videoUrl": "https://example.com/bulgarian-split-squat.mp4",
  "imageUrl": "https://example.com/bulgarian-split-squat.jpg",
  "isUnilateral": true,
  "difficultyId": "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
  "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
  "muscleGroups": [
    {
      "muscleGroupId": "musclegroup-eeff0011-2233-4455-6677-889900112233",
      "muscleRoleId": "musclerole-5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b"
    }
  ],
  "equipmentIds": [],
  "bodyPartIds": ["bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5"],
  "movementPatternIds": ["movementpattern-bbccddee-ff00-1122-3344-556677889900"]
}
```

#### Response
- **200 OK**: Exercise updated successfully
- **400 Bad Request**: Validation error
- **401 Unauthorized**: No authentication
- **403 Forbidden**: User doesn't have PT-Tier or Admin-Tier
- **404 Not Found**: Exercise not found
- **409 Conflict**: Another exercise with same name exists

### GET /api/exercises - List Exercises

**Authorization**: Not required

#### Query Parameters
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `name` (string): Filter by exercise name (partial match)
- `muscleGroupIds` (string[]): Filter by muscle group IDs
- `equipmentIds` (string[]): Filter by equipment IDs
- `difficultyIds` (string[]): Filter by difficulty level IDs
- `exerciseTypeIds` (string[]): Filter by exercise type IDs
- `movementPatternIds` (string[]): Filter by movement pattern IDs
- `bodyPartIds` (string[]): Filter by body part IDs

#### Response Example
```json
{
  "items": [
    {
      "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
      "name": "Barbell Back Squat",
      "description": "A compound lower body exercise",
      "videoUrl": "https://example.com/squat-video.mp4",
      "imageUrl": "https://example.com/squat-image.jpg",
      "isUnilateral": false,
      "difficulty": {
        "id": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "name": "Beginner",
        "description": "Suitable for those new to fitness",
        "order": 1,
        "isActive": true
      },
      "kineticChain": {
        "id": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
        "name": "Compound",
        "description": "Exercises that work multiple muscle groups",
        "order": 1,
        "isActive": true
      },
      "exerciseTypes": [
        {
          "id": "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
          "name": "Workout",
          "description": "Main exercises that form the core of the training session",
          "order": 2,
          "isActive": true
        }
      ],
      "muscleGroups": [
        {
          "muscleGroup": {
            "id": "musclegroup-eeff0011-2233-4455-6677-889900112233",
            "name": "Quadriceps",
            "bodyPart": {
              "id": "bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5",
              "name": "Legs"
            }
          },
          "muscleRole": {
            "id": "musclerole-5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b",
            "name": "Primary"
          }
        }
      ],
      "equipment": [
        {
          "id": "equipment-33445566-7788-99aa-bbcc-ddeeff001122",
          "name": "Barbell"
        }
      ],
      "bodyParts": [
        {
          "id": "bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5",
          "name": "Legs",
          "description": "Leg muscles including quadriceps and hamstrings",
          "order": 3,
          "isActive": true
        }
      ],
      "movementPatterns": [
        {
          "id": "movementpattern-bbccddee-ff00-1122-3344-556677889900",
          "name": "Squat",
          "description": "Bending at the knees and hips"
        }
      ],
      "coachNotes": [
        {
          "id": "coachnote-abc123",
          "text": "Stand with feet shoulder-width apart",
          "order": 0
        },
        {
          "id": "coachnote-def456",
          "text": "Lower body by bending knees and hips",
          "order": 1
        },
        {
          "id": "coachnote-ghi789",
          "text": "Push through heels to return to starting position",
          "order": 2
        }
      ]
    }
  ],
  "totalCount": 150,
  "pageSize": 10,
  "currentPage": 1,
  "totalPages": 15
}
```

### GET /api/exercises/{id} - Get Single Exercise

**Authorization**: Not required

#### Response
Same structure as individual items in the list response.

## Validation Rules

### KineticChainId Validation

1. **Non-REST Exercises** (any exercise type except REST):
   - `kineticChainId` is **REQUIRED**
   - Must reference an existing KineticChainType
   - Error message: "Kinetic chain is required for non-rest exercises"

2. **REST Exercises** (exercise type = REST):
   - `kineticChainId` **MUST be null**
   - Error message: "Kinetic chain must be null for rest exercises"

### Available KineticChainType IDs

| ID | Name | Description |
|----|------|-------------|
| `kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4` | Compound | Exercises that work multiple muscle groups |
| `kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b` | Isolation | Exercises that work a single muscle group |

### Available ExerciseType IDs

| ID | Name | Description |
|----|------|-------------|
| `exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d` | Warmup | Exercises performed to prepare the body |
| `exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e` | Workout | Main exercises that form the core of the training |
| `exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f` | Cooldown | Exercises performed to help recovery |
| `exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a` | Rest | Periods of rest between exercises |

## Error Response Format

All validation errors return a 400 Bad Request with the following format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-abc123...",
  "errors": {
    "KineticChainId": [
      "Kinetic chain is required for non-rest exercises"
    ]
  }
}
```

## Migration Notes

When migrating existing exercises:
1. All non-REST exercises must have a KineticChainId assigned
2. All REST exercises must have KineticChainId set to null
3. Use the validation endpoint to verify exercises before saving

## Examples by Exercise Type

### Compound Exercise Example
```json
{
  "name": "Deadlift",
  "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
  "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
  // ... other required fields
}
```

### Isolation Exercise Example
```json
{
  "name": "Bicep Curl",
  "kineticChainId": "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b",
  "exerciseTypeIds": ["exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"],
  // ... other required fields
}
```

### Rest Period Example
```json
{
  "name": "90 Second Rest",
  "kineticChainId": null,
  "exerciseTypeIds": ["exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"],
  // ... other required fields
}
```