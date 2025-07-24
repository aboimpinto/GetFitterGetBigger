# Workout Templates API Integration

## Overview
This document provides the API integration guide for workout template functionality in client applications (mobile, web, desktop).

**IMPORTANT NOTES**:
1. **No User Context Required**: Templates are not tied to specific users. Any authenticated client can view public templates.
2. **Read-Only Exercise Access**: Clients can only view exercises in templates. Exercise management is restricted to admin users.
3. **Limited Operations**: Clients have read-only access to templates. Creation and management are admin-only operations.

## Base Configuration
- **Base URL**: `http://localhost:5214/api`
- **Content-Type**: `application/json`
- **Authentication**: Bearer token (required for all endpoints)

## Client-Accessible Endpoints

### 1. Get Public Templates
Retrieves all public workout templates available to clients.

**Endpoint**: `GET /workout-templates/public`

**Response** (200 OK):
```json
[
  {
    "id": "workouttemplate-03000001-0000-0000-0000-000000000001",
    "name": "Beginner Full Body Workout",
    "description": "Perfect for starting your fitness journey",
    "category": {
      "id": "workoutcategory-20000002-2000-4000-8000-200000000005",
      "value": "Full Body",
      "description": "Compound exercises engaging multiple muscle groups"
    },
    "difficulty": {
      "id": "difficultylevel-a51b5e6f-e29b-41a7-9e65-4a3cb1e8ff1a",
      "value": "Beginner",
      "description": "Suitable for those new to fitness"
    },
    "workoutState": {
      "id": "workoutstate-02000002-0000-0000-0000-000000000002",
      "value": "PRODUCTION",
      "description": "Active template for use"
    },
    "estimatedDurationMinutes": 45,
    "isPublic": true,
    "tags": ["beginner", "full-body", "strength"],
    "exercises": [],
    "objectives": [],
    "createdAt": "2025-01-20T10:00:00Z",
    "updatedAt": "2025-01-22T14:30:00Z"
  }
]
```

### 2. Get Template by ID
Retrieves a specific template if it's public or assigned to the user.

**Endpoint**: `GET /workout-templates/{id}`

**Response** (200 OK): Single template object

**Error Response** (404 Not Found):
```json
{
  "error": "Workout template not found or not accessible"
}
```

### 3. Search Public Templates
Search public templates by name.

**Endpoint**: `GET /workout-templates/search?namePattern=beginner`

**Query Parameters**:
- `namePattern`: Search string (case-insensitive)

**Response** (200 OK): Array of matching public templates

### 4. Filter by Category
Get public templates in a specific category.

**Endpoint**: `GET /workout-templates/filter/category/{categoryId}`

**Response** (200 OK): Array of templates in that category

### 5. Filter by Difficulty
Get public templates for a specific difficulty level.

**Endpoint**: `GET /workout-templates/filter/difficulty/{difficultyId}`

**Response** (200 OK): Array of templates at that difficulty

### 6. Get Template Exercises
Retrieves the exercise list for a template.

**Endpoint**: `GET /workout-templates/{id}/exercises`

**Response** (200 OK):
```json
[
  {
    "exerciseId": "exercise-01000001-0000-0000-0000-000000000001",
    "exerciseName": "Push-ups",
    "orderIndex": 1,
    "sets": 3,
    "targetReps": "10-15",
    "restSeconds": 60,
    "notes": "Keep core tight throughout"
  },
  {
    "exerciseId": "exercise-01000002-0000-0000-0000-000000000002",
    "exerciseName": "Bodyweight Squats",
    "orderIndex": 2,
    "sets": 3,
    "targetReps": "15-20",
    "restSeconds": 60,
    "notes": "Go as deep as comfortable"
  }
]
```

### 7. Get Paged Public Templates
Get public templates with pagination.

**Endpoint**: `GET /workout-templates/public?page=1&pageSize=20`

**Query Parameters**:
- `page` (default: 1)
- `pageSize` (default: 20, max: 50)

**Response** (200 OK):
```json
{
  "items": [...],
  "totalCount": 85,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Reference Data Endpoints

### Get Workout Categories
**Endpoint**: `GET /workout-templates/categories`

**Response** (200 OK):
```json
[
  {
    "id": "workoutcategory-20000002-2000-4000-8000-200000000001",
    "value": "Upper Body - Push",
    "description": "Push exercises targeting chest, shoulders, triceps"
  },
  {
    "id": "workoutcategory-20000002-2000-4000-8000-200000000005",
    "value": "Full Body",
    "description": "Compound exercises engaging multiple muscle groups"
  }
]
```

### Get Difficulty Levels
**Endpoint**: `GET /workout-templates/difficulties`

### Get Workout Objectives
**Endpoint**: `GET /workout-templates/objectives`

## Data Models

### WorkoutTemplateDto
```typescript
interface WorkoutTemplateDto {
  id: string;
  name: string;
  description?: string;
  category: ReferenceDataDto;
  difficulty: ReferenceDataDto;
  workoutState: ReferenceDataDto;
  estimatedDurationMinutes: number;
  isPublic: boolean;
  tags: string[];
  exercises: WorkoutTemplateExerciseDto[];
  objectives: ReferenceDataDto[];
  createdAt: string;
  updatedAt: string;
}
```

### WorkoutTemplateExerciseDto
```typescript
interface WorkoutTemplateExerciseDto {
  exerciseId: string;
  exerciseName: string;
  orderIndex: number;
  sets: number;
  targetReps: string;
  restSeconds: number;
  notes?: string;
}
```

### ReferenceDataDto
```typescript
interface ReferenceDataDto {
  id: string;
  value: string;
  description: string;
}
```

## Client Implementation Guidelines

### 1. Template Display
- Show only PRODUCTION state templates to clients
- Display category and difficulty prominently
- Show estimated duration
- List exercises in order

### 2. Filtering & Search
- Implement client-side filtering after fetching
- Cache category and difficulty data
- Use debounced search for better UX

### 3. Offline Support
- Cache viewed templates locally
- Sync favorites when online
- Handle offline gracefully

### 4. Performance
- Paginate template lists
- Lazy load exercise details
- Cache reference data aggressively

### 5. User Experience
- Show template preview before selection
- Display exercise count and duration
- Allow favoriting templates (client-side)
- Track template usage (client-side)

## Mobile-Specific Considerations

### Data Usage
- Minimize API calls
- Batch requests when possible
- Compress images if any

### Caching Strategy
```typescript
// Example caching approach
const CACHE_DURATION = 24 * 60 * 60 * 1000; // 24 hours

async function getCachedTemplates() {
  const cached = await storage.get('public_templates');
  if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
    return cached.data;
  }
  return null;
}
```

### Offline Queue
- Queue template selections
- Sync when connection restored
- Handle conflicts gracefully

## Error Handling

### Network Errors
```typescript
try {
  const templates = await api.getPublicTemplates();
} catch (error) {
  if (error.code === 'NETWORK_ERROR') {
    // Show offline message
    // Load from cache
  }
}
```

### API Errors
- 401: Redirect to login
- 404: Show "not found" message
- 500: Show generic error, retry option

## Security Considerations

1. **Authentication**: All endpoints require valid Bearer token
2. **Data Privacy**: Clients only see public templates
3. **Rate Limiting**: Implement client-side throttling
4. **Token Storage**: Use secure storage for auth tokens

## Future Enhancements (Not Yet Implemented)

1. **Assigned Templates**: Personal trainer assigns templates to clients
2. **Progress Tracking**: Track workout completion
3. **Template Ratings**: Client feedback system
4. **Customization**: Modify assigned templates
5. **Social Features**: Share progress with others

## Testing Considerations

### Mock Data
```typescript
const mockTemplate: WorkoutTemplateDto = {
  id: "workouttemplate-test-0001",
  name: "Test Template",
  category: {
    id: "category-test",
    value: "Test Category",
    description: "For testing"
  },
  // ... other fields
};
```

### API Mocking
- Use MSW or similar for development
- Test offline scenarios
- Simulate slow networks

---

**Note**: This API is designed for read-only access from client applications. All template management operations must be performed through the Admin application.