# Workout Template Core API Endpoints

## Overview
This document provides detailed specifications for all API endpoints related to the Workout Template Core feature.

## Base URL
```
/api/workout-templates
```

## Notes
This document describes all endpoints. Authentication and authorization will be implemented in a future feature.

## Endpoints

### 1. List Workout Templates
Retrieves a paginated list of workout templates with optional filtering.

**Endpoint**: `GET /api/workout-templates`

**Query Parameters**:
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20, max: 100)
- `search` (string, optional): Search in name and description
- `categoryId` (guid, optional): Filter by workout category
- `objectiveId` (guid, optional): Filter by workout objective
- `difficultyLevel` (string, optional): Filter by difficulty (Beginner|Intermediate|Advanced)
- `isPublic` (boolean, optional): Filter by public status
- `creatorId` (guid, optional): Filter by creator
- `stateId` (guid, optional): Filter by workout state
- `sortBy` (string, optional): Sort field (name|createdAt|lastModified|duration)
- `sortOrder` (string, optional): Sort order (asc|desc)

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
        "value": "string",
        "description": "string"
      },
      "workoutObjective": {
        "id": "guid",
        "value": "string",
        "description": "string"
      },
      "executionProtocol": {
        "id": "guid",
        "value": "string",
        "description": "string"
      },
      "estimatedDuration": "integer",
      "difficultyLevel": "string",
      "isPublic": "boolean",
      "creator": {
        "id": "guid",
        "name": "string"
      },
      "createdAt": "datetime",
      "lastModified": "datetime",
      "workoutState": {
        "id": "guid",
        "value": "string"
      },
      "exerciseCount": "integer",
      "equipmentRequired": ["string"]
    }
  ],
  "totalCount": "integer",
  "page": "integer",
  "pageSize": "integer",
  "totalPages": "integer"
}
```

### 2. Get Workout Template by ID
Retrieves a complete workout template including all exercises and configurations.

**Endpoint**: `GET /api/workout-templates/{id}`

**Path Parameters**:
- `id` (guid, required): Workout template ID

**Response**: `200 OK`
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDuration": "integer",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "creatorId": "guid",
  "createdAt": "datetime",
  "lastModified": "datetime",
  "version": "string",
  "isActive": "boolean",
  "tags": ["string"],
  "workoutStateId": "guid",
  "exercises": [
    {
      "id": "guid",
      "exerciseId": "guid",
      "exercise": {
        "id": "guid",
        "name": "string",
        "category": "string",
        "equipment": ["string"],
        "primaryMuscles": ["string"],
        "secondaryMuscles": ["string"]
      },
      "zone": "string",
      "sequenceOrder": "integer",
      "exerciseNotes": "string",
      "setConfigurations": [
        {
          "id": "guid",
          "configurationOrder": "integer",
          "executionProtocolId": "guid",
          "targetSets": "integer",
          "targetReps": "string",
          "targetDuration": "integer",
          "intensityGuideline": "string"
        }
      ]
    }
  ],
  "equipmentRequired": ["string"]
}
```

**Error Responses**:
- `404 Not Found`: Template not found
- `403 Forbidden`: Private template not owned by user

### 3. Create Workout Template
Creates a new workout template in DRAFT state.

**Endpoint**: `POST /api/workout-templates`

**Request Body**:
```json
{
  "name": "string (required)",
  "description": "string",
  "workoutCategoryId": "guid (required)",
  "workoutObjectiveId": "guid (required)",
  "executionProtocolId": "guid (required)",
  "estimatedDuration": "integer (required)",
  "difficultyLevel": "string (required)",
  "isPublic": "boolean",
  "tags": ["string"]
}
```

**Response**: `201 Created`
Returns the created workout template object with Location header.

**Error Responses**:
- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Not a Personal Trainer

### 4. Update Workout Template
Updates an existing workout template.

**Endpoint**: `PUT /api/workout-templates/{id}`

**Path Parameters**:
- `id` (guid, required): Workout template ID

**Request Body**:
```json
{
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDuration": "integer",
  "difficultyLevel": "string",
  "isPublic": "boolean",
  "tags": ["string"],
  "isActive": "boolean"
}
```

**Response**: `200 OK`
Returns the updated workout template object.

**Error Responses**:
- `400 Bad Request`: Validation errors
- `403 Forbidden`: Not the owner or template not in DRAFT state
- `404 Not Found`: Template not found

### 5. Delete Workout Template
Deletes a workout template. Only allowed for templates with no execution logs.

**Endpoint**: `DELETE /api/workout-templates/{id}`

**Path Parameters**:
- `id` (guid, required): Workout template ID

**Response**: `204 No Content`

**Error Responses**:
- `403 Forbidden`: Not the owner or template has execution logs
- `404 Not Found`: Template not found

### 6. Add Exercise to Template
Adds an exercise to a workout template.

**Endpoint**: `POST /api/workout-templates/{id}/exercises`

**Path Parameters**:
- `id` (guid, required): Workout template ID

**Request Body**:
```json
{
  "exerciseId": "guid (required)",
  "zone": "string (required)",
  "sequenceOrder": "integer (required)",
  "exerciseNotes": "string"
}
```

**Response**: `201 Created`
Returns the created workout template exercise object.

**Error Responses**:
- `400 Bad Request`: Validation errors or duplicate sequence order
- `403 Forbidden`: Not the owner or template not in DRAFT state
- `404 Not Found`: Template or exercise not found

### 7. Update Exercise in Template
Updates an exercise within a workout template.

**Endpoint**: `PUT /api/workout-templates/{id}/exercises/{exerciseId}`

**Path Parameters**:
- `id` (guid, required): Workout template ID
- `exerciseId` (guid, required): Workout template exercise ID

**Request Body**:
```json
{
  "zone": "string",
  "sequenceOrder": "integer",
  "exerciseNotes": "string"
}
```

**Response**: `200 OK`
Returns the updated workout template exercise object.

### 8. Remove Exercise from Template
Removes an exercise from a workout template.

**Endpoint**: `DELETE /api/workout-templates/{id}/exercises/{exerciseId}`

**Path Parameters**:
- `id` (guid, required): Workout template ID
- `exerciseId` (guid, required): Workout template exercise ID

**Response**: `204 No Content`

### 9. Add Set Configuration
Adds a set configuration to an exercise.

**Endpoint**: `POST /api/workout-templates/{id}/exercises/{exerciseId}/configurations`

**Path Parameters**:
- `id` (guid, required): Workout template ID
- `exerciseId` (guid, required): Workout template exercise ID

**Request Body**:
```json
{
  "configurationOrder": "integer (required)",
  "executionProtocolId": "guid (required)",
  "targetSets": "integer (required)",
  "targetReps": "string",
  "targetDuration": "integer",
  "intensityGuideline": "string"
}
```

**Response**: `201 Created`
Returns the created set configuration object.

### 10. Update Set Configuration
Updates a set configuration.

**Endpoint**: `PUT /api/workout-templates/{id}/exercises/{exerciseId}/configurations/{configId}`

**Path Parameters**:
- `id` (guid, required): Workout template ID
- `exerciseId` (guid, required): Workout template exercise ID
- `configId` (guid, required): Set configuration ID

**Request Body**:
Same as Add Set Configuration

**Response**: `200 OK`
Returns the updated set configuration object.

### 11. Delete Set Configuration
Removes a set configuration.

**Endpoint**: `DELETE /api/workout-templates/{id}/exercises/{exerciseId}/configurations/{configId}`

**Response**: `204 No Content`

### 12. Change Workout State
Changes the state of a workout template.

**Endpoint**: `PUT /api/workout-templates/{id}/state`

**Path Parameters**:
- `id` (guid, required): Workout template ID

**Request Body**:
```json
{
  "workoutStateId": "guid (required)"
}
```

**Response**: `200 OK`
Returns the updated workout template with new state.

**Error Responses**:
- `400 Bad Request`: Invalid state transition
- `403 Forbidden`: Not the owner
- `409 Conflict`: State transition blocked (e.g., execution logs exist)

### 13. Get Exercise Suggestions
Gets intelligent exercise suggestions based on workout category and existing exercises.

**Endpoint**: `GET /api/workout-templates/{id}/exercise-suggestions`

**Path Parameters**:
- `id` (guid, required): Workout template ID

**Query Parameters**:
- `zone` (string, optional): Target zone (Warmup|Main|Cooldown)
- `limit` (integer, optional): Number of suggestions (default: 10)

**Response**: `200 OK`
```json
{
  "suggestions": [
    {
      "exerciseId": "guid",
      "name": "string",
      "category": "string",
      "reason": "string",
      "equipment": ["string"],
      "associatedWarmups": ["guid"],
      "associatedCooldowns": ["guid"]
    }
  ]
}
```

### 14. Duplicate Workout Template
Creates a copy of an existing workout template.

**Endpoint**: `POST /api/workout-templates/{id}/duplicate`

**Path Parameters**:
- `id` (guid, required): Source workout template ID

**Request Body**:
```json
{
  "name": "string (required)",
  "description": "string"
}
```

**Response**: `201 Created`
Returns the duplicated workout template in DRAFT state.

## Reference Data Endpoints

### Workout States Endpoints

#### GET /reference-tables/workout-states
Retrieves all available workout states.

**Endpoint**: `GET /api/reference-tables/workout-states`

**Query Parameters**:
- `includeInactive` (boolean, optional): Include inactive states. Default: false

**Response**: `200 OK`
```json
[
  {
    "workoutStateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "value": "DRAFT",
    "description": "Template under development. Personal Trainer can freely modify all aspects of the template. All execution logs are automatically deleted when changes are made.",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "workoutStateId": "4fb96f75-6828-5673-c4fd-3d074f77afb7",
    "value": "PRODUCTION",
    "description": "Active template ready for use. The workout is finalized and available for public use. Cannot return to DRAFT if execution logs exist.",
    "displayOrder": 2,
    "isActive": true
  },
  {
    "workoutStateId": "5fc07f86-7939-6784-d5fe-4e185f88afc8",
    "value": "ARCHIVED",
    "description": "Retired template (view only). No user can execute an archived workout, but historical data is preserved for performance tracking.",
    "displayOrder": 3,
    "isActive": true
  }
]
```

**Caching Headers**:
```
Cache-Control: public, max-age=31536000
ETag: "workout-states-v1"
```

#### GET /reference-tables/workout-states/{id}
Retrieves a specific workout state by ID.

**Endpoint**: `GET /api/reference-tables/workout-states/{id}`

**Path Parameters**:
- `id` (string, required): WorkoutState GUID

**Response**: `200 OK`
```json
{
  "workoutStateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "value": "DRAFT",
  "description": "Template under development. Personal Trainer can freely modify all aspects of the template. All execution logs are automatically deleted when changes are made.",
  "displayOrder": 1,
  "isActive": true
}
```

**Error Response (404 Not Found)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Workout state not found",
  "status": 404,
  "detail": "No workout state exists with the specified ID"
}
```

#### GET /reference-tables/workout-states/value/{value}
Retrieves a specific workout state by its value.

**Endpoint**: `GET /api/reference-tables/workout-states/value/{value}`

**Path Parameters**:
- `value` (string, required): WorkoutState value (e.g., "DRAFT", "PRODUCTION", "ARCHIVED")

**Response**: `200 OK`
```json
{
  "workoutStateId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "value": "DRAFT",
  "description": "Template under development. Personal Trainer can freely modify all aspects of the template. All execution logs are automatically deleted when changes are made.",
  "displayOrder": 1,
  "isActive": true
}
```

### Reference Data Caching Strategy
All reference data endpoints implement eternal caching (365 days):
- **Server-side**: Redis cache with 365-day TTL
- **Client-side**: HTTP cache headers allowing 365-day browser caching
- **CDN**: Edge caching for 365 days
- **Invalidation**: Manual cache clearing only when reference data updates

## Common Error Responses

All endpoints may return these common errors:

- `403 Forbidden`: Insufficient permissions
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error
- `503 Service Unavailable`: Service temporarily unavailable

## Rate Limiting
- 1000 requests per hour per user
- 100 requests per minute per user

## Pagination
All list endpoints support pagination with these standard parameters:
- `page`: Page number (1-based)
- `pageSize`: Items per page (max 100)

## Filtering and Sorting
List endpoints support filtering via query parameters and sorting via `sortBy` and `sortOrder` parameters.