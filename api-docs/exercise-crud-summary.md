# Exercise CRUD API - Implementation Summary

> **⚠️ DEPRECATED**: This documentation has been migrated to the new Features structure.
> Please refer to:
> - API: `/Features/ExerciseManagement/Core/Exercise_api.md`
> - Admin: `/Features/ExerciseManagement/Core/Exercise_admin.md`
> - Clients: `/Features/ExerciseManagement/Core/Exercise_clients.md`
> This file is maintained for backward compatibility only.

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

**Response Structure**:
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
        }
      ],
      "exerciseTypes": [
        {
          "id": "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
          "value": "Warmup",
          "description": "Exercise used for warming up"
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

### 2. Get Single Exercise (GET /api/exercises/{id})

**Purpose**: Retrieve detailed information about a specific exercise

**Authentication**: Bearer token required

**Path Parameters**:
- `id` (string, required): Exercise ID in format `exercise-{guid}`

**Response Structure**: Same as items in the list response, with complete details

### 3. Create Exercise (POST /api/exercises)

**Purpose**: Create a new exercise

**Authentication**: Bearer token required

**Authorization**: Admin only (requires `PT-Tier` or `Admin-Tier` claim)

**Request Body**:
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

**Validation Rules**:
- `name`: Required, max 100 characters
- `description`: Required
- `coachNotes`: Optional array, each note requires `text` and `order`
- `exerciseTypeIds`: Optional array (Rest type cannot be combined with others)
- `isUnilateral`: Required boolean
- `difficultyId`: Required, must be valid difficulty level ID
- `imageUrl`: Optional, must be valid URL when provided
- `videoUrl`: Optional, must be valid URL when provided
- `muscleGroups`: Optional array, each entry requires `muscleGroupId` and `muscleRoleId`
- All other ID arrays are optional

**Success Response**: 
- Status: 201 Created
- Location header: `/api/exercises/{new-exercise-id}`
- Body: Complete exercise object

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

**Purpose**: Delete an exercise (soft delete by setting isActive = false)

**Authentication**: Bearer token required

**Authorization**: Admin only (requires `PT-Tier` or `Admin-Tier` claim)

**Path Parameters**:
- `id` (string, required): Exercise ID in format `exercise-{guid}`

**Business Logic**:
- Sets `isActive = false` to maintain referential integrity
- Exercise remains in database but is excluded from active listings

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
- Use `hasNextPage` and `hasPreviousPage` for navigation
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
- Muscle Roles (`/api/reference-tables/muscle-roles`)
- Equipment (`/api/reference-tables/equipment`)
- Body Parts (`/api/reference-tables/body-parts`)
- Movement Patterns (`/api/reference-tables/movement-patterns`)
- Exercise Types (`/api/reference-tables/exercise-types`)

These reference tables provide the IDs and display names needed for creating and filtering exercises.

## Key Changes from Previous Version

1. **Coach Notes**: Replaced single `instructions` field with `coachNotes` array for better structure
2. **Exercise Types**: Added support for multiple exercise types per exercise
3. **Nested Objects**: All reference data now returns as objects with id, value, and description
4. **Muscle Groups**: Now includes role relationship (Primary, Secondary, Stabilizer)
5. **Pagination**: New response structure with enhanced pagination metadata
6. **Field Naming**: Switched to camelCase (e.g., `isUnilateral` instead of `is_unilateral`)
7. **Soft Delete**: Uses `isActive` field instead of hard deletion

## Notes for Client Implementation (Mobile, Web, Desktop)

- Consider implementing offline support with local database sync
- Optimize image loading with appropriate caching strategies
- Handle video URLs appropriately for each platform
- Implement pull-to-refresh for exercise lists on mobile
- Consider platform-specific UI patterns for filters

## Notes for Admin Implementation

- Implement drag-and-drop for reordering coach notes
- Provide bulk operations for managing multiple exercises
- Include preview functionality for images and videos
- Consider implementing exercise templates for common patterns
- Add import/export functionality for exercise libraries