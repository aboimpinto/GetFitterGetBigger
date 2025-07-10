# Exercise API Endpoints

## Overview
This document provides detailed specifications for all Exercise Management API endpoints.

## Base URL
```
/api/exercises
```

## Authentication
All endpoints require Bearer token authentication in the Authorization header.

## Endpoints

### 1. List Exercises
Retrieves a paginated list of exercises with optional filtering.

```http
GET /api/exercises
```

#### Query Parameters
| Parameter | Type | Required | Description | Default |
|-----------|------|----------|-------------|---------|
| page | number | No | Page number | 1 |
| pageSize | number | No | Items per page (max: 50) | 10 |
| name | string | No | Filter by name (partial match, case-insensitive) | - |
| difficultyId | string | No | Filter by difficulty level | - |
| exerciseTypeId | string | No | Filter by exercise type | - |
| muscleGroupId | string | No | Filter by muscle group | - |
| equipmentId | string | No | Filter by equipment | - |
| isActive | boolean | No | Filter by active status | true |

#### Response (200 OK)
```json
{
  "items": [
    {
      "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
      "name": "Barbell Back Squat",
      "description": "A compound lower body exercise...",
      "difficulty": {
        "id": "intermediate",
        "name": "Intermediate"
      },
      "primaryMuscleGroups": ["Quadriceps", "Glutes"],
      "imageUrl": "https://cdn.example.com/exercises/squat.jpg",
      "isUnilateral": false,
      "isActive": true
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

### 2. Get Exercise Details
Retrieves complete details for a single exercise.

```http
GET /api/exercises/{id}
```

#### Path Parameters
- `id` - Exercise ID (GUID format)

#### Response (200 OK)
```json
{
  "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise targeting quads, glutes, and hamstrings",
  "coachNotes": [
    {
      "id": "note-1",
      "text": "Keep chest up and core engaged",
      "order": 1
    },
    {
      "id": "note-2",
      "text": "Descend until thighs are parallel to floor",
      "order": 2
    }
  ],
  "exerciseTypes": [
    {
      "id": "workout",
      "name": "Workout"
    }
  ],
  "videoUrl": "https://cdn.example.com/videos/squat.mp4",
  "imageUrl": "https://cdn.example.com/images/squat.jpg",
  "isUnilateral": false,
  "isActive": true,
  "difficulty": {
    "id": "intermediate",
    "name": "Intermediate"
  },
  "muscleGroups": [
    {
      "muscleGroup": {
        "id": "quadriceps",
        "name": "Quadriceps"
      },
      "role": "Primary"
    },
    {
      "muscleGroup": {
        "id": "glutes",
        "name": "Glutes"
      },
      "role": "Primary"
    }
  ],
  "equipment": [
    {
      "id": "barbell",
      "name": "Barbell"
    },
    {
      "id": "squat-rack",
      "name": "Squat Rack"
    }
  ],
  "movementPatterns": [
    {
      "id": "squat",
      "name": "Squat"
    }
  ],
  "bodyParts": [
    {
      "id": "legs",
      "name": "Legs"
    }
  ],
  "kineticChain": {
    "id": "compound",
    "name": "Compound"
  },
  "exerciseWeightType": {
    "id": "weight-required",
    "name": "Weight Required"
  },
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

#### Error Responses
- **404 Not Found**: Exercise not found

### 3. Create Exercise
Creates a new exercise in the system.

```http
POST /api/exercises
```

#### Authorization
Requires `PT-Tier` or `Admin-Tier` claim.

#### Request Body
```json
{
  "name": "Barbell Back Squat",
  "description": "A compound lower body exercise targeting quads, glutes, and hamstrings",
  "coachNotes": [
    {
      "text": "Keep chest up and core engaged",
      "order": 1
    },
    {
      "text": "Descend until thighs are parallel to floor",
      "order": 2
    }
  ],
  "exerciseTypeIds": ["workout"],
  "videoUrl": "https://cdn.example.com/videos/squat.mp4",
  "imageUrl": "https://cdn.example.com/images/squat.jpg",
  "isUnilateral": false,
  "difficultyId": "intermediate",
  "kineticChainId": "compound",
  "exerciseWeightTypeId": "weight-required",
  "muscleGroups": [
    {
      "muscleGroupId": "quadriceps",
      "muscleRoleId": "primary"
    },
    {
      "muscleGroupId": "glutes",
      "muscleRoleId": "primary"
    }
  ],
  "equipmentIds": ["barbell", "squat-rack"],
  "bodyPartIds": ["legs"],
  "movementPatternIds": ["squat"]
}
```

#### Response (201 Created)
```json
{
  "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "name": "Barbell Back Squat",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### Response Headers
- `Location`: `/api/exercises/exercise-123e4567-e89b-12d3-a456-426614174000`

#### Error Responses
- **400 Bad Request**: Validation errors
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Insufficient permissions
- **409 Conflict**: Exercise name already exists

### 4. Update Exercise
Updates an existing exercise.

```http
PUT /api/exercises/{id}
```

#### Authorization
Requires `PT-Tier` or `Admin-Tier` claim.

#### Path Parameters
- `id` - Exercise ID to update

#### Request Body
Same structure as Create Exercise

#### Response (204 No Content)
No response body on success.

#### Error Responses
- **400 Bad Request**: Validation errors
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Exercise not found
- **409 Conflict**: Exercise name already exists

### 5. Delete Exercise
Soft deletes an exercise by marking it as inactive.

```http
DELETE /api/exercises/{id}
```

#### Authorization
Requires `PT-Tier` or `Admin-Tier` claim.

#### Path Parameters
- `id` - Exercise ID to delete

#### Response (204 No Content)
No response body on success.

#### Error Responses
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Exercise not found

## Common Error Response Format
All error responses follow the RFC 7807 Problem Details format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "name": ["Exercise name is required", "Exercise name must not exceed 100 characters"],
    "kineticChainId": ["Kinetic chain is required for non-REST exercises"]
  }
}
```

## Rate Limiting
- Standard rate limits apply: 1000 requests per hour per user
- Bulk operations count as multiple requests

## Caching
- List responses include ETag headers for conditional requests
- Individual exercise responses are cacheable for 1 hour
- Cache is invalidated on any update operation