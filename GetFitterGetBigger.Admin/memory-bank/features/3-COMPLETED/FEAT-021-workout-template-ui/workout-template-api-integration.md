# Workout Template API Integration Guide

## Overview
This document provides the complete API integration guide for workout template management in the Admin application. 

**IMPORTANT NOTES**:
1. **No User Context Required**: The API does not use CreatedBy field. All trainers can create/manage templates without user-specific restrictions.
2. **Exercise Management**: This API provides read-only access to exercises. Exercise management (add/remove/update) will be implemented in FEAT-028.
3. **State Management**: Templates follow a strict state workflow: DRAFT → PRODUCTION → ARCHIVED

## Base Configuration
- **Base URL**: `http://localhost:5214/api`
- **Content-Type**: `application/json`
- **Authentication**: Bearer token (when implemented)

## Core Endpoints

### 1. Create Workout Template
Creates a new workout template in DRAFT state.

**Endpoint**: `POST /workout-templates`

**Request Body**:
```json
{
  "name": "Upper Body Strength",
  "description": "Focus on compound movements",
  "categoryId": "workoutcategory-20000002-2000-4000-8000-200000000001",
  "difficultyId": "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
  "estimatedDurationMinutes": 60,
  "isPublic": true,
  "tags": ["strength", "upper-body"]
}
```

**Response** (201 Created):
```json
{
  "id": "workouttemplate-03000001-0000-0000-0000-000000000001",
  "name": "Upper Body Strength",
  "description": "Focus on compound movements",
  "category": {
    "id": "workoutcategory-20000002-2000-4000-8000-200000000001",
    "value": "Upper Body - Push",
    "description": "Push exercises targeting chest, shoulders, triceps"
  },
  "difficulty": {
    "id": "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
    "value": "Intermediate",
    "description": "Suitable for those with some fitness experience"
  },
  "workoutState": {
    "id": "workoutstate-02000001-0000-0000-0000-000000000001",
    "value": "DRAFT",
    "description": "Template under construction"
  },
  "estimatedDurationMinutes": 60,
  "isPublic": true,
  "tags": ["strength", "upper-body"],
  "exercises": [],
  "objectives": [],
  "createdAt": "2025-01-23T10:00:00Z",
  "updatedAt": "2025-01-23T10:00:00Z"
}
```

### 2. Get Workout Template by ID
Retrieves a single workout template with all details.

**Endpoint**: `GET /workout-templates/{id}`

**Response** (200 OK): Same structure as create response

### 3. Update Workout Template
Updates an existing workout template. Only allowed in DRAFT state.

**Endpoint**: `PUT /workout-templates/{id}`

**Request Body**: Same as create (all fields required)

**Response** (200 OK): Updated template

### 4. Delete Workout Template
Soft deletes a workout template. Only allowed in DRAFT or ARCHIVED state.

**Endpoint**: `DELETE /workout-templates/{id}`

**Response** (204 No Content)

### 5. Get Paged Workout Templates
Retrieves templates with pagination support.

**Endpoint**: `GET /workout-templates?page=1&pageSize=10`

**Query Parameters**:
- `page` (default: 1)
- `pageSize` (default: 10, max: 100)

**Response** (200 OK):
```json
{
  "items": [...],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 6. Search Templates by Name
Search templates using a name pattern.

**Endpoint**: `GET /workout-templates/search?namePattern=upper`

**Response** (200 OK): Array of matching templates

### 7. Filter by Category
Get templates for a specific category.

**Endpoint**: `GET /workout-templates/filter/category/{categoryId}`

**Response** (200 OK): Array of templates

### 8. Filter by Difficulty
Get templates for a specific difficulty level.

**Endpoint**: `GET /workout-templates/filter/difficulty/{difficultyId}`

**Response** (200 OK): Array of templates

### 9. Change Template State
Transitions a template to a new state.

**Endpoint**: `PUT /workout-templates/{id}/state`

**Request Body**:
```json
{
  "newStateId": "workoutstate-02000002-0000-0000-0000-000000000002"
}
```

**Response** (200 OK): Updated template

**State Transition Rules**:
- DRAFT → PRODUCTION (allowed)
- PRODUCTION → ARCHIVED (allowed)
- ARCHIVED → PRODUCTION (allowed)
- Any → DRAFT (not allowed if execution logs exist)

### 10. Duplicate Template
Creates a copy of an existing template in DRAFT state.

**Endpoint**: `POST /workout-templates/{id}/duplicate`

**Request Body**:
```json
{
  "newName": "Copy of Original Template"
}
```

**Response** (201 Created): New template

### 11. Get Template Exercises
Retrieves exercises associated with a template (read-only).

**Endpoint**: `GET /workout-templates/{id}/exercises`

**Response** (200 OK):
```json
[
  {
    "exerciseId": "exercise-01000001-0000-0000-0000-000000000001",
    "exerciseName": "Bench Press",
    "orderIndex": 1,
    "sets": 3,
    "targetReps": "8-10",
    "restSeconds": 90,
    "notes": "Focus on form"
  }
]
```

## Reference Data Endpoints

### Get Workout Categories
**Endpoint**: `GET /workout-templates/categories`

### Get Difficulty Levels  
**Endpoint**: `GET /workout-templates/difficulties`

### Get Workout States
**Endpoint**: `GET /workout-templates/states`

### Get Workout Objectives
**Endpoint**: `GET /workout-templates/objectives`

## Validation Rules

### Template Name
- Required
- Length: 3-100 characters
- Must be unique across all templates

### Description
- Optional
- Max length: 500 characters

### Estimated Duration
- Required
- Range: 5-300 minutes

### Category & Difficulty
- Required
- Must be valid IDs from reference data

## Error Responses

### 400 Bad Request
```json
{
  "error": "Validation failed",
  "details": {
    "name": ["Name must be between 3 and 100 characters"],
    "estimatedDurationMinutes": ["Duration must be between 5 and 300 minutes"]
  }
}
```

### 404 Not Found
```json
{
  "error": "Workout template with ID 'xxx' not found"
}
```

### 409 Conflict
```json
{
  "error": "Workout template with name 'Upper Body Strength' already exists"
}
```

## Common Reference IDs

### Workout States
- DRAFT: `workoutstate-02000001-0000-0000-0000-000000000001`
- PRODUCTION: `workoutstate-02000002-0000-0000-0000-000000000002`
- ARCHIVED: `workoutstate-02000003-0000-0000-0000-000000000003`

### Categories
- Upper Body - Push: `workoutcategory-20000002-2000-4000-8000-200000000001`
- Upper Body - Pull: `workoutcategory-20000002-2000-4000-8000-200000000002`
- Lower Body: `workoutcategory-20000002-2000-4000-8000-200000000003`
- Core: `workoutcategory-20000002-2000-4000-8000-200000000004`
- Full Body: `workoutcategory-20000002-2000-4000-8000-200000000005`

### Difficulty Levels
- Beginner: `difficultylevel-a51b5e6f-e29b-41a7-9e65-4a3cb1e8ff1a`
- Intermediate: `difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a`
- Advanced: `difficultylevel-48a0c7e7-0fbd-4e85-9e3f-2b8e5f3d6a9c`

## UI Implementation Guidelines

### State Management
1. Always check current state before showing action buttons
2. Disable edit/delete for PRODUCTION templates
3. Show appropriate state transition options

### Form Validation
1. Validate on client side before API call
2. Show inline validation errors
3. Prevent duplicate names by checking existence first

### Performance
1. Use pagination for template lists
2. Cache reference data (categories, difficulties)
3. Implement debounced search

### User Experience
1. Show loading states during API calls
2. Display success/error notifications
3. Confirm destructive actions (delete, state changes)

## Future Enhancements (Not Yet Implemented)
1. **Authorization**: Template access control by trainer
2. **Exercise Management**: Add/remove/reorder exercises (FEAT-028)
3. **Objectives**: Link templates to workout objectives
4. **Versioning**: Template version history
5. **Bulk Operations**: Multiple template operations

## Testing Endpoints

### Check if Name Exists
**Endpoint**: `GET /workout-templates/exists/name?name=Template%20Name`

**Response**: `true` or `false`

### Get Template Count
**Endpoint**: `GET /workout-templates/count`

**Response**: `{ "count": 42 }`

### Get Templates by State
**Endpoint**: `GET /workout-templates/by-state/{stateId}`

### Get Public Templates Only
**Endpoint**: `GET /workout-templates/public`

---

**Note**: This API is fully implemented and tested. All endpoints follow RESTful conventions and return consistent response formats.