# Workout Template Core API Endpoints Specification

## Base URL
```
/api/workout-templates
```

## Endpoints Overview

### Template Management
1. `GET /api/workout-templates` - List all workout templates with pagination and filtering
2. `GET /api/workout-templates/{id}` - Get specific workout template with full details
3. `POST /api/workout-templates` - Create new workout template
4. `PUT /api/workout-templates/{id}` - Update workout template
5. `DELETE /api/workout-templates/{id}` - Delete workout template

### Exercise Management
6. `POST /api/workout-templates/{id}/exercises` - Add exercise to template
7. `PUT /api/workout-templates/{id}/exercises/{exerciseId}` - Update exercise in template
8. `DELETE /api/workout-templates/{id}/exercises/{exerciseId}` - Remove exercise from template

### Set Configuration
9. `POST /api/workout-templates/{id}/exercises/{exerciseId}/configurations` - Add set configuration
10. `PUT /api/workout-templates/{id}/exercises/{exerciseId}/configurations/{configId}` - Update set configuration
11. `DELETE /api/workout-templates/{id}/exercises/{exerciseId}/configurations/{configId}` - Delete set configuration

### State Management
12. `PUT /api/workout-templates/{id}/state` - Change workout state

### Support Endpoints
13. `GET /api/workout-templates/{id}/exercise-suggestions` - Get intelligent exercise suggestions
14. `POST /api/workout-templates/{id}/duplicate` - Duplicate workout template

### Reference Data
15. `GET /api/reference-tables/workout-states` - Get all workout states
16. `GET /api/reference-tables/workout-states/{id}` - Get specific workout state by ID
17. `GET /api/reference-tables/workout-states/value/{value}` - Get workout state by value

## Detailed Endpoint Specifications

### 1. List Workout Templates
**Endpoint**: `GET /api/workout-templates`

**Query Parameters**:
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)
- `search` (string): Search in name and description
- `categoryId` (guid): Filter by workout category
- `objectiveId` (guid): Filter by workout objective
- `difficultyLevel` (string): Filter by difficulty
- `isPublic` (bool): Filter by public status
- `creatorId` (guid): Filter by creator
- `stateId` (guid): Filter by workout state
- `sortBy` (string): Sort field (name|createdAt|lastModified|duration)
- `sortOrder` (string): Sort order (asc|desc)

**Response**: `200 OK`
```json
{
  "items": [WorkoutTemplateDto],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

### 2. Get Workout Template by ID
**Endpoint**: `GET /api/workout-templates/{id}`

**Response**: `200 OK`
Returns `WorkoutTemplateDetailDto` with complete template structure including exercises and configurations.

**Error Responses**:
- `404 Not Found`: Template not found
- `403 Forbidden`: Private template not owned by user

### 3. Create Workout Template
**Endpoint**: `POST /api/workout-templates`

**Request Body**: `CreateWorkoutTemplateDto`
```json
{
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDuration": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "tags": ["strength", "upper-body"]
}
```

**Response**: `201 Created`
Returns created `WorkoutTemplateDto` with Location header.

### 4. Update Workout Template
**Endpoint**: `PUT /api/workout-templates/{id}`

**Request Body**: `UpdateWorkoutTemplateDto`
All fields are optional - only provided fields will be updated.

**Response**: `200 OK`
Returns updated `WorkoutTemplateDto`.

### 5. Delete Workout Template
**Endpoint**: `DELETE /api/workout-templates/{id}`

**Response**: `204 No Content`

**Error Responses**:
- `403 Forbidden`: Not the owner or template has execution logs
- `404 Not Found`: Template not found

### 6. Add Exercise to Template
**Endpoint**: `POST /api/workout-templates/{id}/exercises`

**Request Body**: `AddExerciseDto`
```json
{
  "exerciseId": "guid",
  "zone": "Main",
  "sequenceOrder": 1,
  "exerciseNotes": "Focus on form"
}
```

**Response**: `201 Created`
Returns created `WorkoutTemplateExerciseDto`.

### 7. Change Workout State
**Endpoint**: `PUT /api/workout-templates/{id}/state`

**Request Body**: `ChangeWorkoutStateDto`
```json
{
  "workoutStateId": "guid"
}
```

**Response**: `200 OK`
Returns updated template with new state.

**Error Responses**:
- `400 Bad Request`: Invalid state transition
- `409 Conflict`: State transition blocked (e.g., execution logs exist)

### 8. Get Exercise Suggestions
**Endpoint**: `GET /api/workout-templates/{id}/exercise-suggestions`

**Query Parameters**:
- `zone` (string): Target zone (Warmup|Main|Cooldown)
- `limit` (int): Number of suggestions (default: 10)

**Response**: `200 OK`
```json
{
  "suggestions": [ExerciseSuggestionDto]
}
```

## Data Transfer Objects

### WorkoutTemplateDto
Used for list operations:
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "workoutCategory": { "id": "guid", "value": "string" },
  "workoutObjective": { "id": "guid", "value": "string" },
  "executionProtocol": { "id": "guid", "value": "string" },
  "estimatedDuration": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "creator": { "id": "guid", "name": "string" },
  "createdAt": "2025-07-22T10:00:00Z",
  "lastModified": "2025-07-22T10:00:00Z",
  "workoutState": { "id": "guid", "value": "DRAFT" },
  "exerciseCount": 6,
  "equipmentRequired": ["Barbell", "Bench"],
  "tags": ["strength"]
}
```

### WorkoutTemplateDetailDto
Used for single template retrieval with full details:
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "workoutCategoryId": "guid",
  "workoutObjectiveId": "guid",
  "executionProtocolId": "guid",
  "estimatedDuration": 60,
  "difficultyLevel": "Intermediate",
  "isPublic": true,
  "creatorId": "guid",
  "createdAt": "2025-07-22T10:00:00Z",
  "lastModified": "2025-07-22T10:00:00Z",
  "version": "1.0.0",
  "isActive": true,
  "tags": ["strength"],
  "workoutStateId": "guid",
  "exercises": [
    {
      "id": "guid",
      "exerciseId": "guid",
      "exercise": {
        "id": "guid",
        "name": "Bench Press",
        "category": "Push",
        "equipment": ["Barbell", "Bench"],
        "primaryMuscles": ["Chest"],
        "secondaryMuscles": ["Triceps", "Shoulders"]
      },
      "zone": "Main",
      "sequenceOrder": 1,
      "exerciseNotes": "Control the descent",
      "setConfigurations": [
        {
          "id": "guid",
          "configurationOrder": 1,
          "executionProtocolId": "guid",
          "executionProtocol": { "id": "guid", "value": "Standard" },
          "targetSets": 3,
          "targetReps": "8-12",
          "targetDuration": null,
          "intensityGuideline": "RPE 7-8"
        }
      ]
    }
  ],
  "equipmentRequired": ["Barbell", "Bench"]
}
```

## Common Error Responses
All endpoints may return:
- `403 Forbidden`: Insufficient permissions
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error
- `503 Service Unavailable`: Service temporarily unavailable

## Rate Limiting
- 1000 requests per hour per user
- 100 requests per minute per user

## Caching Headers
Reference data endpoints include:
```
Cache-Control: public, max-age=31536000
ETag: "workout-states-v1"
```