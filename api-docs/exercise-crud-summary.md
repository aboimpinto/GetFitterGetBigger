# Exercise CRUD API - Implementation Summary

This document provides essential information for client applications (Admin and Clients) to implement Exercise management functionality.

## Overview

The Exercise CRUD API enables the management of exercises, which are the fundamental building blocks of workouts. This API provides full Create, Read, Update, and Delete operations with advanced filtering and pagination capabilities.

## Base Configuration

- **Base URL**: 
  - Development: `http://localhost:5214/`
  - Production: Not yet assigned
  - Swagger documentation: `http://localhost:5214/swagger`
- **Authentication**: Bearer token required for all endpoints
- **Authorization**: Admin-only access for Create, Update, and Delete operations (requires `PT-Tier` or `Admin-Tier` claim)
- **Content-Type**: `application/json`

## Endpoints

### 1. List Exercises (GET /api/exercises)

**Purpose**: Retrieve a paginated and filtered list of exercises

**Authentication**: Bearer token required

**Query Parameters**:
- `page` (int, optional): Page number, default is 1
- `pageSize` (int, optional): Items per page, default is 10, max is 50
- `name` (string, optional): Filter by exercise name (case-insensitive, partial match)
- `difficultyId` (string, optional): Filter by difficulty level ID
- `muscleGroupIds` (string[], optional): Filter by muscle group IDs
- `equipmentIds` (string[], optional): Filter by equipment IDs
- `movementPatternIds` (string[], optional): Filter by movement pattern IDs
- `isActive` (bool, optional): Include inactive exercises, default is true

**Response Structure**:
```json
{
  "pagination": {
    "total": 150,
    "pages": 15,
    "currentPage": 1,
    "limit": 10
  },
  "exercises": [
    {
      "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "name": "Barbell Back Squat",
      "description": "A fundamental compound exercise for lower body strength.",
      "difficulty": {
        "id": "difficultylevel-xxx",
        "name": "Intermediate"
      },
      "isUnilateral": false,
      "imageUrl": "https://api.getfitterbigger.com/images/squat.jpeg",
      "videoUrl": "https://api.getfitterbigger.com/videos/squat.mp4",
      "muscleGroups": [
        {
          "id": "musclegroup-xxx",
          "name": "Quadriceps",
          "role": "Primary"
        }
      ],
      "equipment": [
        {
          "id": "equipment-xxx",
          "name": "Barbell"
        }
      ],
      "bodyParts": [],
      "movementPatterns": []
    }
  ]
}
```

### 2. Get Single Exercise (GET /api/exercises/{id})

**Purpose**: Retrieve detailed information about a specific exercise

**Authentication**: Bearer token required

**Path Parameters**:
- `id` (string, required): Exercise ID in format `exercise-{guid}`

**Response Structure**:
```json
{
  "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "name": "Barbell Back Squat",
  "description": "A fundamental compound exercise for lower body strength.",
  "instructions": "1. Stand with your feet shoulder-width apart...\n2. Place the barbell on your upper back...\n3. Lower your body until your thighs are parallel to the floor...",
  "difficulty": {
    "id": "difficultylevel-xxx",
    "name": "Intermediate"
  },
  "isUnilateral": false,
  "imageUrl": "https://api.getfitterbigger.com/images/squat.jpeg",
  "videoUrl": "https://api.getfitterbigger.com/videos/squat.mp4",
  "muscleGroups": [
    {
      "id": "musclegroup-xxx",
      "name": "Quadriceps",
      "role": "Primary"
    },
    {
      "id": "musclegroup-yyy",
      "name": "Glutes",
      "role": "Primary"
    }
  ],
  "equipment": [
    {
      "id": "equipment-xxx",
      "name": "Barbell"
    }
  ],
  "bodyParts": [],
  "movementPatterns": [
    {
      "id": "movementpattern-xxx",
      "name": "Squat"
    }
  ]
}
```

### 3. Create Exercise (POST /api/exercises)

**Purpose**: Create a new exercise

**Authentication**: Bearer token required

**Authorization**: Admin only (requires `PT-Tier` or `Admin-Tier` claim)

**Request Body**:
```json
{
  "name": "Push-up",
  "description": "A classic bodyweight exercise for upper body strength.",
  "instructions": "1. Get into a high plank position...\n2. Lower your body until your chest nearly touches the floor...",
  "difficultyId": "difficultylevel-b1c2d3e4-f5a6-b7c8-d9e0-f1a2b3c4d5e6",
  "isUnilateral": false,
  "imageUrl": "https://api.getfitterbigger.com/images/pushup.jpeg",
  "videoUrl": "https://api.getfitterbigger.com/videos/pushup.mp4",
  "muscleGroupsWithRoles": [
    {
      "muscleGroupId": "musclegroup-xxx",
      "role": "Primary"
    },
    {
      "muscleGroupId": "musclegroup-yyy",
      "role": "Secondary"
    }
  ],
  "equipmentIds": [],
  "bodyPartIds": ["bodypart-xxx"],
  "movementPatternIds": ["movementpattern-xxx"]
}
```

**Validation Rules**:
- `name`: Required, max 200 characters, must be unique (case-insensitive)
- `description`: Required, max 1000 characters
- `instructions`: Required, max 5000 characters
- `difficultyId`: Required, must be valid difficulty level ID
- `isUnilateral`: Required boolean
- `imageUrl`: Optional, must be valid URL when provided
- `videoUrl`: Optional, must be valid URL when provided
- `muscleGroupsWithRoles`: Required, at least one muscle group must be specified
  - `role` must be one of: "Primary", "Secondary", "Stabilizer"
- All ID arrays accept empty arrays except muscleGroupsWithRoles

**Success Response**: 
- Status: 201 Created
- Location header: `/api/exercises/{new-exercise-id}`
- Body: Same as GET single exercise response

### 4. Update Exercise (PUT /api/exercises/{id})

**Purpose**: Update an existing exercise

**Authentication**: Bearer token required

**Authorization**: Admin only (requires `PT-Tier` or `Admin-Tier` claim)

**Path Parameters**:
- `id` (string, required): Exercise ID in format `exercise-{guid}`

**Request Body**: Same structure as Create Exercise request

**Success Response**: 
- Status: 204 No Content

### 5. Delete Exercise (DELETE /api/exercises/{id})

**Purpose**: Delete an exercise (soft delete if referenced, hard delete if not)

**Authentication**: Bearer token required

**Authorization**: Admin only (requires `PT-Tier` or `Admin-Tier` claim)

**Path Parameters**:
- `id` (string, required): Exercise ID in format `exercise-{guid}`

**Business Logic**:
- If the exercise is referenced in any workouts: Soft delete (sets `isActive = false`)
- If not referenced: Hard delete (permanently removes from database)

**Success Response**: 
- Status: 204 No Content

## Error Responses

All endpoints return standard error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "name": ["Exercise name already exists"]
  }
}
```

Common Status Codes:
- `400 Bad Request`: Validation errors or business rule violations
- `401 Unauthorized`: Missing or invalid authentication token
- `403 Forbidden`: User lacks required permissions (Admin claim)
- `404 Not Found`: Exercise not found
- `409 Conflict`: Name already exists (on create/update)
- `500 Internal Server Error`: Server-side error

## Implementation Guidelines

### Authentication Flow
1. Obtain a bearer token through the authentication endpoint
2. Include the token in the Authorization header: `Authorization: Bearer {token}`
3. Handle 401 responses by refreshing the token or re-authenticating

### Pagination Best Practices
- Default page size is 10, adjust based on UI requirements
- Maximum page size is 50 to prevent performance issues
- Always check `pagination.pages` to determine if more pages exist
- Implement infinite scroll or traditional pagination based on UX needs

### Filtering Recommendations
- Combine multiple filters for advanced search functionality
- Name filter supports partial matching for user-friendly search
- Use reference table IDs obtained from the reference tables endpoint
- Consider implementing filter presets for common exercise categories

### Error Handling
- Parse validation errors from the `errors` object in 400 responses
- Display user-friendly messages for business rule violations
- Implement retry logic for network failures
- Log errors for debugging but avoid exposing sensitive information

### Performance Considerations
- Cache exercise lists with appropriate TTL
- Implement optimistic updates for better UX
- Use pagination to limit data transfer
- Consider implementing local search on cached data

## Reference Data Requirements

Before implementing exercise CRUD, ensure you have loaded the following reference tables:
- Difficulty Levels (`/api/reference-tables/difficulty-levels`)
- Muscle Groups (`/api/reference-tables/muscle-groups`)
- Equipment (`/api/reference-tables/equipment`)
- Body Parts (`/api/reference-tables/body-parts`)
- Movement Patterns (`/api/reference-tables/movement-patterns`)

These reference tables provide the IDs and display names needed for creating and filtering exercises.

## Notes for Client Implementation (Mobile, Web, Desktop)

- Consider implementing offline support with local database sync
- Optimize image loading with appropriate caching strategies
- Handle video URLs appropriately for each platform
- Implement pull-to-refresh for exercise lists on mobile
- Consider platform-specific UI patterns for filters

## Notes for Admin Implementation

- Implement rich text editor for exercise instructions
- Provide bulk operations for managing multiple exercises
- Include preview functionality for images and videos
- Consider implementing exercise templates for common patterns
- Add import/export functionality for exercise libraries