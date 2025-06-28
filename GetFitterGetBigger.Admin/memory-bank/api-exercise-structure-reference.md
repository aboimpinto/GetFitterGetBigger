# Exercise API Structure Reference

This document provides a quick reference for the current Exercise API structure after recent updates.

## Response Structure Changes

### Paginated List Response (GET /api/exercises)

```javascript
{
  // Array of exercise items
  items: [
    // Exercise objects (see below)
  ],
  
  // Pagination metadata
  currentPage: 1,
  pageSize: 20,
  totalCount: 150,
  totalPages: 8,
  hasPreviousPage: false,
  hasNextPage: true
}
```

### Exercise Object Structure

```javascript
{
  id: "exercise-uuid",
  name: "Barbell Back Squat",
  description: "A fundamental compound exercise...",
  
  // Coach notes array (replaces instructions)
  coachNotes: [
    {
      id: "coachnote-uuid",
      text: "Position the barbell on your upper back",
      order: 0
    }
  ],
  
  // Exercise types with full details
  exerciseTypes: [
    {
      id: "exercisetype-uuid",
      value: "Workout",
      name: "Workout",
      description: "Main training exercises"
    }
  ],
  
  // Difficulty with value and description
  difficulty: {
    id: "difficultylevel-uuid",
    value: "Intermediate",
    description: "Intermediate level"
  },
  
  // Muscle groups with roles
  muscleGroups: [
    {
      muscleGroup: {
        id: "musclegroup-uuid",
        name: "Quadriceps"
      },
      role: {
        id: "musclerole-uuid",
        value: "Primary",
        name: "Primary"
      }
    }
  ],
  
  // Equipment, body parts, movement patterns as object arrays
  equipment: [{ id: "equipment-uuid", name: "Barbell" }],
  bodyParts: [{ id: "bodypart-uuid", name: "Upper Back" }],
  movementPatterns: [{ id: "pattern-uuid", name: "Squat" }],
  
  // Boolean flags (camelCase)
  isUnilateral: false,
  isActive: true,
  
  // Media URLs (camelCase)
  imageUrl: "https://...",
  videoUrl: "https://..."
}
```

## Request Structure Changes

### Create/Update Exercise Request

```javascript
{
  name: "Barbell Back Squat",
  description: "A fundamental compound exercise...",
  
  // Coach notes array (no ID for new notes)
  coachNotes: [
    { text: "Position the barbell on your upper back", order: 0 },
    { text: "Keep your chest up and core engaged", order: 1 }
  ],
  
  // Reference IDs (arrays)
  exerciseTypeIds: ["exercisetype-workout-id"],
  difficultyId: "difficultylevel-intermediate-id",
  
  // Muscle groups with role assignment
  muscleGroups: [
    {
      muscleGroupId: "musclegroup-quadriceps-id",
      muscleRoleId: "musclerole-primary-id"
    },
    {
      muscleGroupId: "musclegroup-glutes-id",
      muscleRoleId: "musclerole-secondary-id"
    }
  ],
  
  // Other reference arrays
  equipmentIds: ["equipment-barbell-id"],
  bodyPartIds: [],
  movementPatternIds: ["pattern-squat-id"],
  
  // Boolean and media fields
  isUnilateral: false,
  imageUrl: null,
  videoUrl: null
}
```

## Key Field Changes Summary

### Naming Convention
- All fields now use **camelCase**: `isUnilateral`, `imageUrl`, `videoUrl`
- No more snake_case or PascalCase in JSON

### Structural Changes
1. **Pagination**: Response wrapped in object with `items` array and metadata
2. **Coach Notes**: Array replaces single `instructions` string
3. **Exercise Types**: Full objects with `value` and `description`
4. **Difficulty**: Object with `value` and `description`
5. **Muscle Groups**: Nested structure with role relationship
6. **Equipment/Body Parts/Movement Patterns**: Full objects, not just IDs
7. **Active Status**: New `isActive` field for soft-delete support

### Request Changes
1. **Coach Notes**: Send array without IDs for new notes
2. **Muscle Groups**: Array of `{muscleGroupId, muscleRoleId}` objects
3. **Reference Arrays**: Use `*Ids` suffix (e.g., `equipmentIds`, `bodyPartIds`)
4. **Exercise Types**: Use `exerciseTypeIds` array

## Validation Rules

1. **Exercise Name**: Required, max 200 chars, must be unique
2. **Description**: Required, max 1000 chars
3. **Coach Notes**: Each note max 1000 chars, order must be non-negative
4. **Muscle Groups**: At least one required
5. **Exercise Types**: "Rest" type cannot be combined with other types
6. **URLs**: Valid URL format if provided

## HTTP Status Codes

- **200 OK**: Successful GET requests
- **201 Created**: Successful POST (returns created exercise)
- **204 No Content**: Successful PUT/DELETE
- **400 Bad Request**: Validation errors
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Insufficient permissions (needs PT-Tier or Admin-Tier)
- **404 Not Found**: Exercise not found
- **409 Conflict**: Duplicate exercise name