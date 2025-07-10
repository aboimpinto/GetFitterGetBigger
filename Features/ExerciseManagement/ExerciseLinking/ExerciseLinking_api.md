# Exercise Linking API Specification

## Overview
The Exercise Linking feature enables Personal Trainers to create relationships between exercises based on their type (Warmup, Workout, Cooldown), streamlining workout creation and ensuring proper exercise sequencing.

## Data Models

### ExerciseLink Entity
```typescript
interface ExerciseLink {
  id: string; // Format: "exerciselink-{guid}"
  sourceExerciseId: string; // Must be a Workout type exercise
  targetExerciseId: string; // Must be Warmup or Cooldown type
  linkType: LinkType; // "Warmup" | "Cooldown"
  displayOrder: number;
  createdBy: string;
  createdAt: Date;
  modifiedBy?: string;
  modifiedAt?: Date;
}

enum LinkType {
  Warmup = "Warmup",
  Cooldown = "Cooldown"
}
```

### Linked Exercise Response
```typescript
interface LinkedExercise {
  link: ExerciseLink;
  exercise: Exercise; // Full exercise details
}

interface ExerciseLinksResponse {
  exerciseId: string;
  warmups: LinkedExercise[];
  cooldowns: LinkedExercise[];
}
```

## Endpoints

### 1. Create Exercise Link
```http
POST /api/exercises/{id}/links
```

**Request Body:**
```json
{
  "targetExerciseId": "exercise-xyz",
  "linkType": "Warmup",
  "displayOrder": 1
}
```

**Validation:**
- Source exercise must be Workout type
- Target exercise must match linkType (Warmup/Cooldown)
- No circular references allowed
- No duplicate links
- Display order must be positive integer

**Response:**
- Status: 201 Created
- Location: `/api/exercises/{id}/links/{linkId}`
- Body: Created ExerciseLink object

### 2. Get Exercise Links
```http
GET /api/exercises/{id}/links
```

**Query Parameters:**
- `linkType` (optional): Filter by "Warmup" or "Cooldown"

**Response:**
```json
{
  "exerciseId": "exercise-abc",
  "warmups": [
    {
      "link": {
        "id": "exerciselink-123",
        "sourceExerciseId": "exercise-abc",
        "targetExerciseId": "exercise-def",
        "linkType": "Warmup",
        "displayOrder": 1
      },
      "exercise": {
        // Full exercise object
      }
    }
  ],
  "cooldowns": [
    {
      "link": {
        "id": "exerciselink-456",
        "sourceExerciseId": "exercise-abc",
        "targetExerciseId": "exercise-ghi",
        "linkType": "Cooldown",
        "displayOrder": 1
      },
      "exercise": {
        // Full exercise object
      }
    }
  ]
}
```

### 3. Update Exercise Link
```http
PUT /api/exercises/{id}/links/{linkId}
```

**Request Body:**
```json
{
  "displayOrder": 2
}
```

**Note:** Only displayOrder can be updated. To change target exercise or type, delete and recreate.

**Response:** 204 No Content

### 4. Delete Exercise Link
```http
DELETE /api/exercises/{id}/links/{linkId}
```

**Response:** 204 No Content

### 5. Get Suggested Links
```http
GET /api/exercises/{id}/suggested-links
```

**Query Parameters:**
- `linkType` (optional): Get suggestions for specific type
- `limit` (optional): Number of suggestions (default: 5)

**Response:**
```json
{
  "warmupSuggestions": [
    {
      "exercise": { /* Exercise object */ },
      "confidence": 0.85,
      "reason": "Frequently used together"
    }
  ],
  "cooldownSuggestions": [
    {
      "exercise": { /* Exercise object */ },
      "confidence": 0.72,
      "reason": "Common pattern for muscle group"
    }
  ]
}
```

### 6. Bulk Operations
```http
POST /api/exercises/{id}/links/bulk
```

**Request Body:**
```json
{
  "operations": [
    {
      "action": "create",
      "targetExerciseId": "exercise-123",
      "linkType": "Warmup",
      "displayOrder": 1
    },
    {
      "action": "delete",
      "linkId": "exerciselink-456"
    },
    {
      "action": "update",
      "linkId": "exerciselink-789",
      "displayOrder": 3
    }
  ]
}
```

**Response:**
```json
{
  "successful": 3,
  "failed": 0,
  "results": [
    {
      "operation": 0,
      "status": "success",
      "linkId": "exerciselink-new"
    }
  ]
}
```

## Business Logic

### Validation Rules
1. **Source Exercise Type**
   - Must be of type "Workout"
   - Cannot be REST type
   - Must be active

2. **Target Exercise Type**
   - Warmup links: Target must include "Warmup" type
   - Cooldown links: Target must include "Cooldown" type
   - Cannot be REST type
   - Must be active

3. **Circular Reference Prevention**
   - Cannot link A→B if B→A exists
   - Cannot link to self
   - Recursive checking for chain links

4. **Duplicate Prevention**
   - Same source-target-type combination not allowed
   - Case: Exercise A cannot have two warmup links to Exercise B

### Display Order Management
- Orders are unique within linkType for an exercise
- Auto-increment for new links if not specified
- Reorder operation updates all affected links
- Gaps in ordering are allowed

### Cascading Effects
- Deleting an exercise removes all its links (both as source and target)
- Deactivating an exercise hides its links but preserves them
- Reactivating an exercise restores its links

## Security

### Authentication
- All endpoints require Bearer token

### Authorization
- **Create/Update/Delete**: PT-Tier or Admin-Tier
- **Read**: All authenticated users
- **Suggested Links**: All authenticated users

### Audit Trail
- Track who creates each link
- Track modifications with timestamp
- Preserve deletion history for analytics

## Performance Considerations

### Caching
- Cache linked exercises with 5-minute TTL
- Invalidate cache on link modifications
- Warm cache for popular exercises

### Query Optimization
- Eager load exercise details
- Index on sourceExerciseId, linkType
- Composite index for ordering queries

### Bulk Operations
- Transaction support for atomicity
- Batch database operations
- Maximum 50 operations per request

## Integration Points

### Workout Builder
- Auto-include linked exercises option
- Override link suggestions
- Preserve link relationships in templates

### Analytics
- Track link usage in workouts
- Identify popular link patterns
- PT link creation patterns
- Link effectiveness metrics

### Exercise Management
- Show link count in exercise list
- Link indicator badges
- Quick link management from exercise details