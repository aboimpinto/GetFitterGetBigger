# Exercises API

This document outlines the API endpoints for managing exercises. Personal Trainers use these endpoints to create, update, and organize the exercises that will be used in workouts.

---
used_by:
  - admin
  - client
  - shared
---

## 1. Get All Exercises

Retrieves a paginated and filterable list of all exercises.

- **Endpoint:** `GET /api/exercises`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim.

### Query Parameters

| Parameter      | Type   | Required | Description                                               |
| :------------- | :----- | :------- | :-------------------------------------------------------- |
| `page`         | number | No       | Page number for pagination (default: 1).                  |
| `pageSize`     | number | No       | Number of items per page (default: 10, max: 50).          |
| `name`         | string | No       | Search term to filter exercises by name (case-insensitive). |
| `difficultyId` | string | No       | Filter exercises by a specific difficulty level ID.       |

### Success Response (200 OK)

```json
{
  "items": [
    {
      "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "name": "Barbell Back Squat",
      "description": "A compound lower body exercise targeting quads and glutes",
      "coachNotes": [
        {
          "id": "note-1234",
          "text": "Keep your chest up and core engaged",
          "order": 1
        },
        {
          "id": "note-5678",
          "text": "Drive through the heels, not the toes",
          "order": 2
        }
      ],
      "exerciseTypes": [
        {
          "id": "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
          "value": "Warmup",
          "description": "Exercise used for warming up"
        },
        {
          "id": "exercisetype-22334455-6677-8899-aabb-ccddeeff0011",
          "value": "Workout",
          "description": "Main workout exercise"
        }
      ],
      "videoUrl": "https://example.com/squat-video.mp4",
      "imageUrl": "https://example.com/squat-image.jpg",
      "isUnilateral": false,
      "isActive": true,
      "difficulty": {
        "id": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
        "value": "Intermediate",
        "description": "Suitable for those with some training experience"
      },
      "muscleGroups": [
        {
          "muscleGroup": {
            "id": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
            "value": "Quadriceps",
            "description": "Front thigh muscles"
          },
          "role": {
            "id": "musclerole-abcdef12-3456-7890-abcd-ef1234567890",
            "value": "Primary",
            "description": "Main muscle targeted"
          }
        }
      ],
      "equipment": [
        {
          "id": "equipment-33445566-7788-99aa-bbcc-ddeeff001122",
          "value": "Barbell",
          "description": "Standard olympic barbell"
        }
      ],
      "movementPatterns": [
        {
          "id": "movementpattern-99aabbcc-ddee-ff00-1122-334455667788",
          "value": "Squat",
          "description": "Knee-dominant lower body movement"
        }
      ],
      "bodyParts": [
        {
          "id": "bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c",
          "value": "Legs",
          "description": "Lower body"
        }
      ]
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 2. Get Exercise by ID

Retrieves a single exercise by its unique identifier.

- **Endpoint:** `GET /api/exercises/{id}`
- **Authentication:** Bearer token required.

### Success Response (200 OK)

```json
{
  "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise targeting quads and glutes",
  "coachNotes": [
    {
      "id": "note-1234",
      "text": "Keep your chest up and core engaged",
      "order": 1
    },
    {
      "id": "note-5678",
      "text": "Drive through the heels, not the toes",
      "order": 2
    },
    {
      "id": "note-9012",
      "text": "Maintain a neutral spine throughout the movement",
      "order": 3
    }
  ],
  "exerciseTypes": [
    {
      "id": "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
      "value": "Warmup",
      "description": "Exercise used for warming up"
    },
    {
      "id": "exercisetype-22334455-6677-8899-aabb-ccddeeff0011",
      "value": "Workout",
      "description": "Main workout exercise"
    }
  ],
  "videoUrl": "https://example.com/squat-video.mp4",
  "imageUrl": "https://example.com/squat-image.jpg",
  "isUnilateral": false,
  "isActive": true,
  "difficulty": {
    "id": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
    "value": "Intermediate",
    "description": "Suitable for those with some training experience"
  },
  "muscleGroups": [
    {
      "muscleGroup": {
        "id": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
        "value": "Quadriceps",
        "description": "Front thigh muscles"
      },
      "role": {
        "id": "musclerole-abcdef12-3456-7890-abcd-ef1234567890",
        "value": "Primary",
        "description": "Main muscle targeted"
      }
    },
    {
      "muscleGroup": {
        "id": "musclegroup-ddeeff00-1122-3344-5566-778899aabbcc",
        "value": "Glutes",
        "description": "Buttock muscles"
      },
      "role": {
        "id": "musclerole-bcdef123-4567-890a-bcde-f12345678901",
        "value": "Secondary",
        "description": "Supporting muscle"
      }
    }
  ],
  "equipment": [
    {
      "id": "equipment-33445566-7788-99aa-bbcc-ddeeff001122",
      "value": "Barbell",
      "description": "Standard olympic barbell"
    }
  ],
  "movementPatterns": [
    {
      "id": "movementpattern-99aabbcc-ddee-ff00-1122-334455667788",
      "value": "Squat",
      "description": "Knee-dominant lower body movement"
    }
  ],
  "bodyParts": [
    {
      "id": "bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c",
      "value": "Legs",
      "description": "Lower body"
    }
  ]
}
```

---

## 3. Create Exercise

Creates a new exercise.

- **Endpoint:** `POST /api/exercises`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim.

### Request Body

```json
{
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise targeting quads and glutes",
  "coachNotes": [
    {
      "text": "Keep your chest up and core engaged",
      "order": 1
    },
    {
      "text": "Drive through the heels, not the toes",
      "order": 2
    }
  ],
  "exerciseTypeIds": [
    "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
    "exercisetype-22334455-6677-8899-aabb-ccddeeff0011"
  ],
  "videoUrl": "https://example.com/squat-video.mp4",
  "imageUrl": "https://example.com/squat-image.jpg",
  "isUnilateral": false,
  "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
  "muscleGroups": [
    {
      "muscleGroupId": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
      "muscleRoleId": "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    }
  ],
  "equipmentIds": ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
  "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
  "movementPatternIds": ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"]
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| name | string | Yes | Exercise name (max 100 characters) |
| description | string | Yes | Brief description of the exercise |
| coachNotes | array | No | Array of coaching instructions with text and order |
| exerciseTypeIds | array | No | Array of exercise type IDs (Warmup, Workout, Cooldown, Rest) |
| videoUrl | string | No | URL to instructional video |
| imageUrl | string | No | URL to exercise image |
| isUnilateral | boolean | Yes | Whether exercise works one side at a time |
| difficultyId | string | Yes | Difficulty level ID |
| muscleGroups | array | No | Array of muscle groups with their roles |
| equipmentIds | array | No | Array of equipment IDs needed |
| bodyPartIds | array | No | Array of body part IDs targeted |
| movementPatternIds | array | No | Array of movement pattern IDs |

### Success Response (201 Created)

The response will include a `Location` header with the URL of the newly created resource (e.g., `/api/exercises/exercise-c4a5b6d7-e8f9-a0b1-c2d3-e4f5a6b7c8d9`) and return the created exercise object.

---

## 4. Update Exercise

Updates an existing exercise.

- **Endpoint:** `PUT /api/exercises/{id}`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim.

### Request Body

The request body is the same as the `POST /api/exercises` endpoint. All fields that were required for creation remain required for updates.

### Success Response (204 No Content)

A successful update returns a `204 No Content` status code with an empty body.

---

## 5. Deactivate/Delete Exercise

Marks an exercise as inactive. If the exercise is not associated with any workouts, it may be permanently deleted.

- **Endpoint:** `DELETE /api/exercises/{id}`
- **Authentication:** Bearer token required.
- **Authorization:** Required `PT-Tier` or `Admin-Tier` claim.

### Success Response (204 No Content)

A successful deactivation/deletion returns a `204 No Content` status code with an empty body.

---

## Notes

- The `isActive` field is managed internally and indicates whether an exercise is available for use
- Exercise Types: An exercise can have multiple types, but "Rest" type cannot be combined with others
- Coach Notes are ordered instructions that replace the old single "instructions" field
- All reference data (difficulty, muscle groups, equipment, etc.) must exist in the system before being referenced