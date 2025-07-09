# Exercise Linking API Integration Guide

## Overview

This document provides complete API integration details for implementing the Exercise Linking feature in the Admin application. This is based on the completed API implementation in FEAT-022.

## API Base URL

Development: `http://localhost:5214/api`

## Authentication

Currently, no authentication is required for these endpoints. Authorization will be added in a future iteration.

## API Endpoints

### 1. Create Exercise Link

Creates a new link between a source exercise (Workout type) and a target exercise (Warmup or Cooldown type).

**Endpoint:** `POST /api/exercises/{exerciseId}/links`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID (must be a Workout type exercise)
  - Format: `exercise-{guid}`
  - Example: `exercise-123e4567-e89b-12d3-a456-426614174001`

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
- `displayOrder` (integer, required): Order in which to display this link (1-based, min: 1)

**Success Response:** `201 Created`
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
- `400 Bad Request`: Various validation failures
  - `"Source exercise must be of type 'Workout'"`
  - `"Target exercise must be of type 'Warmup'"` (or Cooldown)
  - `"REST exercises cannot be linked"`
  - `"A Warmup link already exists between these exercises"`
  - `"This link would create a circular reference"`
  - `"Maximum number of Warmup links (10) has been reached"`

### 2. Get Exercise Links

Retrieves all links for a specific exercise, optionally filtered by link type.

**Endpoint:** `GET /api/exercises/{exerciseId}/links`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID

**Query Parameters:**
- `linkType` (string, optional): Filter by link type ("Warmup" or "Cooldown")
- `includeExerciseDetails` (boolean, optional): Include full exercise data (default: false)

**Success Response:** `200 OK`

Basic response (includeExerciseDetails=false):
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
    }
  ],
  "totalCount": 1
}
```

Detailed response (includeExerciseDetails=true):
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

### 3. Get Suggested Links

Retrieves suggested exercises that could be linked based on common usage patterns.

**Endpoint:** `GET /api/exercises/{exerciseId}/links/suggested`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID

**Query Parameters:**
- `count` (integer, optional): Number of suggestions to return (default: 5, max: 20)

**Success Response:** `200 OK`
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

Updates an existing exercise link's display order and active status.

**Endpoint:** `PUT /api/exercises/{exerciseId}/links/{linkId}`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID
- `linkId` (string): The link ID to update
  - Format: `exerciselink-{guid}`

**Request Body:**
```json
{
  "displayOrder": 3,
  "isActive": true
}
```

**Request Body Schema:**
- `displayOrder` (integer, required): New display order (min: 1)
- `isActive` (boolean, required): Whether the link is active

**Success Response:** `200 OK`
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

Soft deletes an exercise link (marks it as inactive).

**Endpoint:** `DELETE /api/exercises/{exerciseId}/links/{linkId}`

**Path Parameters:**
- `exerciseId` (string): The source exercise ID
- `linkId` (string): The link ID to delete

**Success Response:** `204 No Content`

**Error Responses:**
- `400 Bad Request`: Invalid ID format
- `404 Not Found`: Link not found (Note: Due to soft delete, subsequent deletes return 204)

## TypeScript Interfaces

```typescript
// DTOs for API communication
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

interface ExerciseLinksResponseDto {
  exerciseId: string;
  exerciseName: string;
  links: ExerciseLinkDto[];
  totalCount: number;
}

interface CreateExerciseLinkDto {
  targetExerciseId: string;      // Required
  linkType: "Warmup" | "Cooldown"; // Required
  displayOrder: number;          // Required, min: 1
}

interface UpdateExerciseLinkDto {
  displayOrder: number;          // Required, min: 1
  isActive: boolean;             // Required
}

// Service interface for Admin implementation
interface IExerciseLinkService {
  createLink(exerciseId: string, data: CreateExerciseLinkDto): Promise<ExerciseLinkDto>;
  getLinks(exerciseId: string, linkType?: string, includeDetails?: boolean): Promise<ExerciseLinksResponseDto>;
  getSuggestedLinks(exerciseId: string, count?: number): Promise<ExerciseLinkDto[]>;
  updateLink(exerciseId: string, linkId: string, data: UpdateExerciseLinkDto): Promise<ExerciseLinkDto>;
  deleteLink(exerciseId: string, linkId: string): Promise<void>;
}
```

## Business Rules Summary

1. **Source Exercise Requirements:**
   - Must be of type "Workout"
   - REST exercises cannot have links

2. **Target Exercise Requirements:**
   - For Warmup links: target must have "Warmup" type
   - For Cooldown links: target must have "Cooldown" type
   - REST exercises cannot be linked as targets

3. **Link Constraints:**
   - Maximum 10 warmup links per exercise
   - Maximum 10 cooldown links per exercise
   - No duplicate links (same source, target, and type)
   - No circular references (if A links to B, B cannot link back to A)

4. **Display Order:**
   - Must be unique within the same source exercise and link type
   - 1-based numbering
   - Used for sorting linked exercises

5. **Soft Delete:**
   - Links are marked as inactive rather than deleted
   - Deleting an already deleted link returns success (idempotent)

## Implementation Recommendations

### Error Handling
```typescript
// Example error handling for the Admin service
async createLink(exerciseId: string, data: CreateExerciseLinkDto): Promise<ExerciseLinkDto> {
  try {
    const response = await fetch(`${API_BASE_URL}/exercises/${exerciseId}/links`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new ExerciseLinkError(error.error || 'Failed to create link');
    }
    
    return await response.json();
  } catch (error) {
    // Handle specific error cases
    if (error.message.includes('Maximum number')) {
      throw new MaxLinksExceededError(error.message);
    }
    if (error.message.includes('circular reference')) {
      throw new CircularReferenceError(error.message);
    }
    throw error;
  }
}
```

### State Management
```typescript
// Suggested state structure for exercise links
interface ExerciseLinksState {
  links: {
    [exerciseId: string]: {
      warmups: ExerciseLinkDto[];
      cooldowns: ExerciseLinkDto[];
      lastFetched: number;
      isLoading: boolean;
      error: string | null;
    };
  };
  suggestions: {
    [exerciseId: string]: ExerciseLinkDto[];
  };
}
```

### UI Implementation Tips

1. **Loading States:**
   - Show skeleton cards while loading links
   - Disable interaction during API calls
   - Show progress indicators for bulk operations

2. **Optimistic Updates:**
   - Update UI immediately on user action
   - Revert on API failure
   - Show temporary "saving" indicators

3. **Drag and Drop:**
   - Use displayOrder for positioning
   - Update all affected items' displayOrder
   - Batch API calls for efficiency

4. **Validation:**
   - Check exercise types before allowing selection
   - Count existing links before adding new ones
   - Validate circular references client-side when possible

## Example Usage Flows

### Adding a Warmup Exercise
```typescript
// 1. User selects exercise to link
const targetExerciseId = "exercise-123...";

// 2. Check if exercise can be linked
const sourceExercise = await getExercise(currentExerciseId);
if (!sourceExercise.exerciseTypes.includes("Workout")) {
  showError("Only workout exercises can have links");
  return;
}

// 3. Check current link count
const currentLinks = await exerciseLinkService.getLinks(currentExerciseId, "Warmup");
if (currentLinks.totalCount >= 10) {
  showError("Maximum warmup links reached");
  return;
}

// 4. Create the link
const newLink = await exerciseLinkService.createLink(currentExerciseId, {
  targetExerciseId,
  linkType: "Warmup",
  displayOrder: currentLinks.totalCount + 1
});

// 5. Update UI
addLinkToUI(newLink);
showSuccess("Warmup exercise linked successfully");
```

### Reordering Links
```typescript
// 1. User drags item to new position
const reorderedLinks = reorderArray(links, oldIndex, newIndex);

// 2. Update display orders
const updates = reorderedLinks.map((link, index) => ({
  linkId: link.id,
  displayOrder: index + 1
}));

// 3. Send updates to API
await Promise.all(
  updates.map(update => 
    exerciseLinkService.updateLink(exerciseId, update.linkId, {
      displayOrder: update.displayOrder,
      isActive: true
    })
  )
);
```

## Testing Considerations

1. **Mock Data:**
   - Create mock exercise links for testing
   - Test with maximum link counts
   - Test error scenarios

2. **Edge Cases:**
   - Empty link lists
   - Maximum links reached
   - Circular reference attempts
   - Network failures

3. **Performance:**
   - Test with many links
   - Test rapid reordering
   - Test concurrent operations

## Migration Notes

- No changes to existing Exercise endpoints
- Links are completely optional
- Backward compatible with exercises without links