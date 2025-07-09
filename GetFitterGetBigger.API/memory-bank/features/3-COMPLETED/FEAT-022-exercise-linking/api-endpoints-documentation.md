# Exercise Links API Endpoints

This document describes the exercise linking endpoints available in the GetFitterGetBigger API.

## Overview

Exercise links allow connecting exercises together for warmup and cooldown purposes. For example, a "Barbell Squat" workout exercise can have "Air Squats" linked as a warmup exercise.

### Business Rules
- Only exercises with type "Workout" can have links (source exercises)
- Target exercises must have the appropriate type (Warmup or Cooldown)
- REST exercises cannot be linked
- Maximum 10 links per type (10 warmups, 10 cooldowns) per exercise
- No circular references allowed
- Hard delete is used (links are permanently removed from the database)

## Endpoints

### 1. Create Exercise Link

Creates a new link between a source exercise and a target exercise.

**Endpoint:** `POST /api/exercises/{exerciseId}/links`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID (must be a Workout type exercise)

**Request Body:**
```json
{
  "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "linkType": "Warmup",
  "displayOrder": 1
}
```

**Request Body Schema:**
- `targetExerciseId` (string, required): The ID of the exercise to link to
- `linkType` (string, required): Type of link - must be either "Warmup" or "Cooldown"
- `displayOrder` (integer, required): Order in which to display this link (1-based)

**Response:** `201 Created`
```json
{
  "id": "exerciselink-550e8400-e29b-41d4-a716-446655440000",
  "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
  "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "targetExerciseName": "Air Squat",
  "linkType": "Warmup",
  "displayOrder": 1,
  "isActive": true,
  "createdAt": "2025-07-09T10:30:00Z",
  "updatedAt": "2025-07-09T10:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid input or validation failure
  ```json
  {
    "error": "Source exercise must be of type 'Workout'"
  }
  ```
  ```json
  {
    "error": "Target exercise must be of type 'Warmup'"
  }
  ```
  ```json
  {
    "error": "REST exercises cannot be linked"
  }
  ```
  ```json
  {
    "error": "A Warmup link already exists between these exercises"
  }
  ```
  ```json
  {
    "error": "This link would create a circular reference"
  }
  ```
  ```json
  {
    "error": "Maximum number of Warmup links (10) has been reached"
  }
  ```

### 2. Get Exercise Links

Retrieves all links for a specific exercise, optionally filtered by link type.

**Endpoint:** `GET /api/exercises/{exerciseId}/links`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID

**Query Parameters:**
- `linkType` (string, optional): Filter by link type ("Warmup" or "Cooldown")
- `includeExerciseDetails` (boolean, optional): Include full exercise details (default: false)

**Response:** `200 OK`
```json
{
  "exerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
  "exerciseName": "Barbell Squat",
  "links": [
    {
      "id": "exerciselink-550e8400-e29b-41d4-a716-446655440000",
      "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
      "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
      "targetExerciseName": "Air Squat",
      "linkType": "Warmup",
      "displayOrder": 1,
      "isActive": true,
      "createdAt": "2025-07-09T10:30:00Z",
      "updatedAt": "2025-07-09T10:30:00Z"
    },
    {
      "id": "exerciselink-660e8400-e29b-41d4-a716-446655440001",
      "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
      "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174002",
      "targetExerciseName": "Leg Swings",
      "linkType": "Warmup",
      "displayOrder": 2,
      "isActive": true,
      "createdAt": "2025-07-09T10:31:00Z",
      "updatedAt": "2025-07-09T10:31:00Z"
    }
  ],
  "totalCount": 2
}
```

**With includeExerciseDetails=true:**
```json
{
  "exerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
  "exerciseName": "Barbell Squat",
  "links": [
    {
      "id": "exerciselink-550e8400-e29b-41d4-a716-446655440000",
      "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
      "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
      "targetExerciseName": "Air Squat",
      "linkType": "Warmup",
      "displayOrder": 1,
      "isActive": true,
      "createdAt": "2025-07-09T10:30:00Z",
      "updatedAt": "2025-07-09T10:30:00Z",
      "targetExercise": {
        "id": "exercise-123e4567-e89b-12d3-a456-426614174000",
        "name": "Air Squat",
        "description": "Bodyweight squat exercise",
        "videoUrl": "https://example.com/air-squat.mp4",
        "imageUrl": "https://example.com/air-squat.jpg",
        "difficulty": {
          "id": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
          "name": "Beginner"
        },
        "exerciseTypes": ["Workout", "Warmup"],
        "muscleGroups": [
          {
            "muscleGroup": {
              "id": "musclegroup-eeff0011-2233-4455-6677-889900112233",
              "name": "Quadriceps"
            },
            "muscleRole": {
              "id": "musclerole-abcdef12-3456-7890-abcd-ef1234567890",
              "name": "Primary"
            }
          }
        ]
      }
    }
  ],
  "totalCount": 1
}
```

**Error Responses:**
- `400 Bad Request`: Invalid exercise ID format

### 3. Get Suggested Links

Retrieves suggested exercises that could be linked based on common usage patterns.

**Endpoint:** `GET /api/exercises/{exerciseId}/links/suggested`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID

**Query Parameters:**
- `count` (integer, optional): Number of suggestions to return (default: 5)

**Response:** `200 OK`
```json
[
  {
    "id": "exerciselink-suggested-1",
    "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
    "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
    "targetExerciseName": "Air Squat",
    "linkType": "Warmup",
    "displayOrder": 1,
    "isActive": true,
    "createdAt": "2025-07-09T10:30:00Z",
    "updatedAt": "2025-07-09T10:30:00Z"
  },
  {
    "id": "exerciselink-suggested-2",
    "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
    "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174003",
    "targetExerciseName": "Foam Rolling",
    "linkType": "Cooldown",
    "displayOrder": 1,
    "isActive": true,
    "createdAt": "2025-07-09T10:30:00Z",
    "updatedAt": "2025-07-09T10:30:00Z"
  }
]
```

### 4. Update Exercise Link

Updates an existing exercise link (display order and active status).

**Endpoint:** `PUT /api/exercises/{exerciseId}/links/{linkId}`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID
- `linkId` (string): The link ID to update

**Request Body:**
```json
{
  "displayOrder": 3,
  "isActive": true
}
```

**Request Body Schema:**
- `displayOrder` (integer, required): New display order
- `isActive` (boolean, required): Whether the link is active

**Response:** `200 OK`
```json
{
  "id": "exerciselink-550e8400-e29b-41d4-a716-446655440000",
  "sourceExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174001",
  "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "targetExerciseName": "Air Squat",
  "linkType": "Warmup",
  "displayOrder": 3,
  "isActive": true,
  "createdAt": "2025-07-09T10:30:00Z",
  "updatedAt": "2025-07-09T11:45:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid input
- `404 Not Found`: Link not found or doesn't belong to the specified exercise

### 5. Delete Exercise Link

Permanently deletes an exercise link from the database.

**Endpoint:** `DELETE /api/exercises/{exerciseId}/links/{linkId}`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID
- `linkId` (string): The link ID to delete

**Response:** `204 No Content`

**Error Responses:**
- `400 Bad Request`: Invalid ID format
- `404 Not Found`: Link not found (subsequent deletes of the same link will return 404)

## Data Models

### ExerciseLinkDto
```typescript
interface ExerciseLinkDto {
  id: string;                    // Format: "exerciselink-{guid}"
  sourceExerciseId: string;      // Format: "exercise-{guid}"
  targetExerciseId: string;      // Format: "exercise-{guid}"
  targetExerciseName: string;
  linkType: "Warmup" | "Cooldown";
  displayOrder: number;
  isActive: boolean;
  createdAt: string;             // ISO 8601 date
  updatedAt: string;             // ISO 8601 date
  targetExercise?: ExerciseDto;  // Optional, when includeExerciseDetails=true
}
```

### ExerciseLinksResponseDto
```typescript
interface ExerciseLinksResponseDto {
  exerciseId: string;
  exerciseName: string;
  links: ExerciseLinkDto[];
  totalCount: number;
}
```

### CreateExerciseLinkDto
```typescript
interface CreateExerciseLinkDto {
  targetExerciseId: string;      // Required
  linkType: "Warmup" | "Cooldown"; // Required
  displayOrder: number;          // Required, min: 1
}
```

### UpdateExerciseLinkDto
```typescript
interface UpdateExerciseLinkDto {
  displayOrder: number;          // Required, min: 1
  isActive: boolean;             // Required
}
```

## Usage Examples

### Admin UI Workflow

1. **Adding a warmup exercise:**
```bash
POST /api/exercises/exercise-123e4567-e89b-12d3-a456-426614174001/links
{
  "targetExerciseId": "exercise-123e4567-e89b-12d3-a456-426614174000",
  "linkType": "Warmup",
  "displayOrder": 1
}
```

2. **Reordering links:**
```bash
PUT /api/exercises/exercise-123e4567-e89b-12d3-a456-426614174001/links/exerciselink-550e8400-e29b-41d4-a716-446655440000
{
  "displayOrder": 2,
  "isActive": true
}
```

3. **Removing a link (permanent deletion):**
```bash
DELETE /api/exercises/exercise-123e4567-e89b-12d3-a456-426614174001/links/exerciselink-550e8400-e29b-41d4-a716-446655440000
```

### Client App Workflow

1. **Get all warmup exercises for a workout:**
```bash
GET /api/exercises/exercise-123e4567-e89b-12d3-a456-426614174001/links?linkType=Warmup&includeExerciseDetails=true
```

2. **Get suggested exercises:**
```bash
GET /api/exercises/exercise-123e4567-e89b-12d3-a456-426614174001/links/suggested?count=3
```

## Notes for Implementation

- All endpoints currently have no authorization (to be added later)
- Hard delete is used - once deleted, links cannot be recovered and subsequent deletes return 404
- The suggested links endpoint currently returns the most commonly used links across all exercises
- Display order is used for sorting within each link type
- Exercise type validation is strict - ensure exercises have the correct types before linking