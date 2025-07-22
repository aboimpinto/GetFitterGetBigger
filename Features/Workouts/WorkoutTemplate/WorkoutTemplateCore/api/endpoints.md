# Workout Template Core API Endpoints

## Overview
This document provides detailed specifications for all Workout Template Core API endpoints.

## Base URL
- Development: `http://localhost:5214/api`
- Production: TBD

## Authentication
All endpoints require Bearer token authentication.

## Endpoints

### 1. List Workout Templates
Retrieves a paginated list of workout templates with filtering options.

**Endpoint**: `GET /workout-templates`

**Query Parameters**:
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 20, max: 100)
- `search` (string, optional): Search in name and description
- `categoryId` (guid, optional): Filter by workout category
- `objectiveId` (guid, optional): Filter by workout objective
- `difficultyLevel` (string, optional): Filter by difficulty (Beginner|Intermediate|Advanced)
- `isPublic` (bool, optional): Filter public/private templates
- `createdByUserId` (guid, optional): Filter by creator
- `tags` (string[], optional): Filter by tags
- `sortBy` (string, optional): Sort field (name|createdAt|lastModifiedAt|difficulty)
- `sortDirection` (string, optional): Sort direction (asc|desc)

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "workoutCategory": {
        "id": "guid",
        "name": "string"
      },
      "workoutObjective": {
        "id": "guid",
        "name": "string"
      },
      "executionProtocol": {
        "id": "guid",
        "name": "string"
      },
      "estimatedDurationMinutes": 60,
      "difficultyLevel": "Intermediate",
      "isPublic": true,
      "createdByUser": {
        "id": "guid",
        "name": "string"
      },
      "createdAt": "2025-07-22T10:00:00Z",
      "exerciseCount": 8,
      "tags": ["string"]
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

### 2. Get Workout Template Details
Retrieves complete details of a specific workout template.

**Endpoint**: `GET /workout-templates/{id}`

**Path Parameters**:
- `id` (guid, required): Template ID

**Response**: `200 OK`
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutCategory": {
    "id": "guid",
    "name": "string",
    "description": "string"
  },
  "workoutObjectiveId": "guid",
  "workoutObjective": {
    "id": "guid",
    "name": "string",
    "description": "string"
  },
  "executionProtocolId": "guid",
  "executionProtocol": {
    "id": "guid",
    "name": "string",
    "description": "string",
    "isAvailable": true
  },
  "estimatedDurationMinutes": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "createdByUserId": "guid",
  "createdByUser": {
    "id": "guid",
    "name": "string",
    "isPT": true
  },
  "createdAt": "2025-07-22T10:00:00Z",
  "lastModifiedAt": "2025-07-22T10:00:00Z",
  "version": "1.0.0",
  "isActive": true,
  "tags": ["string"],
  "requiredEquipment": [
    {
      "id": "guid",
      "name": "string"
    }
  ],
  "exercises": [
    {
      "id": "guid",
      "exerciseId": "guid",
      "exercise": {
        "id": "guid",
        "name": "string",
        "category": "string",
        "primaryMuscleGroups": ["string"],
        "equipment": ["string"]
      },
      "zone": "Main",
      "sequenceOrder": 1,
      "exerciseNotes": "string",
      "setConfigurations": [
        {
          "id": "guid",
          "configurationOrder": 1,
          "executionProtocolId": "guid",
          "targetSets": 4,
          "targetReps": "8-12",
          "targetDurationSeconds": null,
          "intensityGuideline": "string"
        }
      ]
    }
  ]
}
```

### 3. Create Workout Template
Creates a new workout template.

**Endpoint**: `POST /workout-templates`

**Authorization**: Requires PT-Tier claim

**Request Body**:
```json
{
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDurationMinutes": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "tags": ["string"]
}
```

**Response**: `201 Created`
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDurationMinutes": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "createdByUserId": "guid",
  "createdAt": "2025-07-22T10:00:00Z",
  "lastModifiedAt": "2025-07-22T10:00:00Z",
  "version": "1.0.0",
  "isActive": true,
  "tags": ["string"]
}
```

### 4. Update Workout Template
Updates an existing workout template.

**Endpoint**: `PUT /workout-templates/{id}`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID

**Request Body**:
```json
{
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDurationMinutes": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "tags": ["string"],
  "version": "string"
}
```

**Response**: `200 OK`
Returns updated template object.

### 5. Delete Workout Template
Soft deletes a workout template.

**Endpoint**: `DELETE /workout-templates/{id}`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID

**Response**: `204 No Content`

### 6. Get Template Exercises
Retrieves all exercises for a template grouped by zone.

**Endpoint**: `GET /workout-templates/{id}/exercises`

**Path Parameters**:
- `id` (guid, required): Template ID

**Response**: `200 OK`
```json
{
  "warmup": [
    {
      "id": "guid",
      "exerciseId": "guid",
      "exercise": {
        "id": "guid",
        "name": "string",
        "category": "string",
        "primaryMuscleGroups": ["string"],
        "equipment": ["string"],
        "instructions": "string"
      },
      "sequenceOrder": 1,
      "exerciseNotes": "string",
      "setConfigurations": []
    }
  ],
  "main": [],
  "cooldown": []
}
```

### 7. Add Exercise to Template
Adds a new exercise to the workout template.

**Endpoint**: `POST /workout-templates/{id}/exercises`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID

**Request Body**:
```json
{
  "exerciseId": "guid",
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "string",
  "setConfigurations": [
    {
      "configurationOrder": 1,
      "executionProtocolId": "guid",
      "targetSets": 4,
      "targetReps": "8-12",
      "targetDurationSeconds": null,
      "intensityGuideline": "string"
    }
  ]
}
```

**Response**: `201 Created`
Returns created exercise with suggestions for warmup/cooldown if applicable.

### 8. Update Exercise in Template
Updates an exercise within the template.

**Endpoint**: `PUT /workout-templates/{id}/exercises/{exerciseId}`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID
- `exerciseId` (guid, required): Template Exercise ID

**Request Body**:
```json
{
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "string"
}
```

**Response**: `200 OK`

### 9. Remove Exercise from Template
Removes an exercise from the template.

**Endpoint**: `DELETE /workout-templates/{id}/exercises/{exerciseId}`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID
- `exerciseId` (guid, required): Template Exercise ID

**Query Parameters**:
- `acknowledgeWarnings` (bool, optional): Acknowledge warmup/cooldown removal warnings

**Response**: `204 No Content` or `409 Conflict` if warnings not acknowledged

### 10. Configure Exercise Sets
Updates set configuration for a template exercise.

**Endpoint**: `PUT /workout-templates/{id}/exercises/{exerciseId}/sets`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID
- `exerciseId` (guid, required): Template Exercise ID

**Request Body**:
```json
{
  "setConfigurations": [
    {
      "configurationOrder": 1,
      "executionProtocolId": "guid",
      "targetSets": 4,
      "targetReps": "8-12",
      "targetDurationSeconds": null,
      "intensityGuideline": "string"
    }
  ]
}
```

**Response**: `200 OK`

### 11. Reorder Template Exercises
Bulk updates exercise positions within zones.

**Endpoint**: `PUT /workout-templates/{id}/exercises/reorder`

**Authorization**: Requires PT-Tier claim and ownership

**Path Parameters**:
- `id` (guid, required): Template ID

**Request Body**:
```json
{
  "exercises": [
    {
      "exerciseId": "guid",
      "zone": "Main",
      "sequenceOrder": 1
    }
  ]
}
```

**Response**: `200 OK`

### 12. Duplicate Template
Creates a copy of an existing template.

**Endpoint**: `POST /workout-templates/{id}/duplicate`

**Authorization**: Requires PT-Tier claim

**Path Parameters**:
- `id` (guid, required): Template ID to duplicate

**Request Body**:
```json
{
  "name": "string",
  "isPublic": false
}
```

**Response**: `201 Created`
Returns new template with all exercises copied.

### 13. Get Exercise Suggestions
Provides intelligent exercise suggestions based on template configuration.

**Endpoint**: `GET /workout-templates/{id}/exercise-suggestions`

**Path Parameters**:
- `id` (guid, required): Template ID

**Query Parameters**:
- `zone` (string, required): Target zone (Warmup|Main|Cooldown)
- `lastAddedExerciseId` (guid, optional): For complementary suggestions

**Response**: `200 OK`
```json
{
  "suggestions": [
    {
      "exerciseId": "guid",
      "exerciseName": "string",
      "category": "string",
      "relevanceScore": 0.95,
      "reason": "Complementary pull exercise for push movement"
    }
  ]
}
```

### 14. Validate Template
Checks if template meets all validation rules.

**Endpoint**: `POST /workout-templates/{id}/validate`

**Path Parameters**:
- `id` (guid, required): Template ID

**Response**: `200 OK`
```json
{
  "isValid": true,
  "warnings": [
    {
      "type": "MissingWarmup",
      "message": "Main exercise 'Bench Press' has no associated warmup"
    }
  ],
  "errors": []
}
```

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "fieldName": ["Error message"]
  }
}
```

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required"
}
```

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions to perform this action"
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Workout template not found"
}
```

### 409 Conflict
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Operation conflicts with current state",
  "warnings": [
    {
      "type": "LinkedExerciseRemoval",
      "message": "Removing this warmup will leave main exercise without proper preparation"
    }
  ]
}
```

## Rate Limiting
- 1000 requests per hour for authenticated users
- 100 template creations per day per user
- Burst limit: 20 requests per minute

## Pagination
All list endpoints support pagination with:
- Default page size: 20
- Maximum page size: 100
- Total count included in response
- Zero-based page numbering