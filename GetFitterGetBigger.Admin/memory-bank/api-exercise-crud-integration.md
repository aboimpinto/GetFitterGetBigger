# Exercise CRUD API Integration Guide for Admin

This document provides all necessary information for the Admin application to implement Exercise management functionality. The Admin application is used by Personal Trainers to create and manage exercises.

## Overview

The Exercise CRUD API allows Personal Trainers to:
- Create new exercises with detailed instructions and media
- View and search through the exercise library
- Update existing exercises
- Delete exercises (with automatic soft-delete for referenced exercises)

## API Configuration

### Base Setup
```javascript
// API Base URL (configure per environment)
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5214';
// Production URL: Not yet assigned
// Swagger documentation available at: http://localhost:5214/swagger

// Headers for all requests
const headers = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${authToken}` // Get from auth service
};
```

### Authentication Requirements
- All endpoints require a valid Bearer token
- Create, Update, and Delete operations require `PT-Tier` or `Admin-Tier` claim
- The token should be obtained through the authentication flow and stored securely

## Endpoints Implementation

### 1. List Exercises

**Purpose**: Display paginated exercise list with search and filtering

```javascript
// GET /api/exercises
async function getExercises(params = {}) {
  const queryParams = new URLSearchParams({
    page: params.page || 1,
    pageSize: params.pageSize || 10,
    ...(params.name && { name: params.name }),
    ...(params.difficultyId && { difficultyId: params.difficultyId }),
    ...(params.isActive !== undefined && { isActive: params.isActive })
  });

  // Add array parameters
  if (params.muscleGroupIds?.length) {
    params.muscleGroupIds.forEach(id => queryParams.append('muscleGroupIds', id));
  }
  if (params.equipmentIds?.length) {
    params.equipmentIds.forEach(id => queryParams.append('equipmentIds', id));
  }

  const response = await fetch(`${API_BASE_URL}/api/exercises?${queryParams}`, {
    headers
  });

  if (!response.ok) throw new Error(`Error: ${response.status}`);
  return response.json();
}
```

**Response Structure**:
```javascript
{
  pagination: {
    total: 150,
    pages: 15,
    currentPage: 1,
    limit: 10
  },
  exercises: [
    {
      id: "exercise-uuid",
      name: "Barbell Back Squat",
      description: "A fundamental compound exercise...",
      difficulty: { id: "difficultylevel-uuid", name: "Intermediate" },
      isUnilateral: false,
      imageUrl: "https://...",
      videoUrl: "https://...",
      muscleGroups: [
        { id: "musclegroup-uuid", name: "Quadriceps", role: "Primary" }
      ],
      equipment: [{ id: "equipment-uuid", name: "Barbell" }],
      bodyParts: [],
      movementPatterns: []
    }
  ]
}
```

### 2. Get Single Exercise

**Purpose**: View detailed exercise information for editing

```javascript
// GET /api/exercises/{id}
async function getExercise(exerciseId) {
  const response = await fetch(`${API_BASE_URL}/api/exercises/${exerciseId}`, {
    headers
  });

  if (!response.ok) {
    if (response.status === 404) throw new Error('Exercise not found');
    throw new Error(`Error: ${response.status}`);
  }
  
  return response.json();
}
```

### 3. Create Exercise

**Purpose**: Add new exercises to the library

```javascript
// POST /api/exercises
async function createExercise(exerciseData) {
  const response = await fetch(`${API_BASE_URL}/api/exercises`, {
    method: 'POST',
    headers,
    body: JSON.stringify({
      name: exerciseData.name,
      description: exerciseData.description,
      instructions: exerciseData.instructions,
      difficultyId: exerciseData.difficultyId,
      isUnilateral: exerciseData.isUnilateral,
      imageUrl: exerciseData.imageUrl || null,
      videoUrl: exerciseData.videoUrl || null,
      muscleGroupsWithRoles: exerciseData.muscleGroupsWithRoles, // Required, min 1
      equipmentIds: exerciseData.equipmentIds || [],
      bodyPartIds: exerciseData.bodyPartIds || [],
      movementPatternIds: exerciseData.movementPatternIds || []
    })
  });

  if (!response.ok) {
    const error = await response.json();
    if (response.status === 400) {
      // Handle validation errors
      throw new ValidationError(error.errors);
    }
    if (response.status === 409) {
      throw new Error('Exercise name already exists');
    }
    throw new Error(`Error: ${response.status}`);
  }

  // Get the created exercise ID from Location header
  const location = response.headers.get('Location');
  const createdId = location?.split('/').pop();
  
  return response.json();
}
```

**Validation Rules**:
- `name`: Required, max 200 chars, unique
- `description`: Required, max 1000 chars
- `instructions`: Required, max 5000 chars
- `muscleGroupsWithRoles`: At least one required
  - Each item needs `muscleGroupId` and `role` ("Primary", "Secondary", or "Stabilizer")

### 4. Update Exercise

**Purpose**: Modify existing exercises

```javascript
// PUT /api/exercises/{id}
async function updateExercise(exerciseId, exerciseData) {
  const response = await fetch(`${API_BASE_URL}/api/exercises/${exerciseId}`, {
    method: 'PUT',
    headers,
    body: JSON.stringify(exerciseData) // Same structure as create
  });

  if (!response.ok) {
    if (response.status === 404) throw new Error('Exercise not found');
    if (response.status === 409) throw new Error('Exercise name already exists');
    const error = await response.json();
    if (response.status === 400) {
      throw new ValidationError(error.errors);
    }
    throw new Error(`Error: ${response.status}`);
  }

  // 204 No Content on success
  return true;
}
```

### 5. Delete Exercise

**Purpose**: Remove exercises from the library

```javascript
// DELETE /api/exercises/{id}
async function deleteExercise(exerciseId) {
  const response = await fetch(`${API_BASE_URL}/api/exercises/${exerciseId}`, {
    method: 'DELETE',
    headers
  });

  if (!response.ok) {
    if (response.status === 404) throw new Error('Exercise not found');
    throw new Error(`Error: ${response.status}`);
  }

  // 204 No Content on success
  return true;
}
```

**Note**: The API automatically handles soft-delete vs hard-delete based on whether the exercise is referenced in workouts.

## UI Implementation Guidelines

### Exercise List Page
1. **Search Bar**: Real-time search by exercise name
2. **Filters**: 
   - Difficulty level dropdown
   - Muscle groups multi-select
   - Equipment multi-select
   - Show/hide inactive exercises toggle
3. **Pagination**: Show page numbers or infinite scroll
4. **Actions**: Edit and Delete buttons for each exercise
5. **Create Button**: Prominent "Add Exercise" button

### Exercise Form (Create/Edit)
1. **Basic Information**:
   - Name input (with uniqueness validation)
   - Description textarea
   - Instructions rich text editor
   - Difficulty level dropdown
   - Unilateral exercise checkbox

2. **Media**:
   - Image URL input with preview
   - Video URL input with validation

3. **Categorization**:
   - Muscle Groups section with role selection
   - Equipment multi-select
   - Body Parts multi-select
   - Movement Patterns multi-select

4. **Validation**: Show inline errors for all fields

### Error Handling

```javascript
class ValidationError extends Error {
  constructor(errors) {
    super('Validation failed');
    this.errors = errors;
  }
}

// Usage in component
try {
  await createExercise(formData);
  showSuccess('Exercise created successfully');
  navigateToList();
} catch (error) {
  if (error instanceof ValidationError) {
    // Display field-specific errors
    Object.entries(error.errors).forEach(([field, messages]) => {
      setFieldError(field, messages[0]);
    });
  } else {
    showError(error.message);
  }
}
```

## Reference Data Loading

Before implementing exercise CRUD, load these reference tables:

```javascript
// Load all reference data on app initialization
async function loadReferenceData() {
  const [difficulties, muscleGroups, equipment, bodyParts, movementPatterns] = 
    await Promise.all([
      fetch(`${API_BASE_URL}/api/reference-tables/difficulty-levels`).then(r => r.json()),
      fetch(`${API_BASE_URL}/api/reference-tables/muscle-groups`).then(r => r.json()),
      fetch(`${API_BASE_URL}/api/reference-tables/equipment`).then(r => r.json()),
      fetch(`${API_BASE_URL}/api/reference-tables/body-parts`).then(r => r.json()),
      fetch(`${API_BASE_URL}/api/reference-tables/movement-patterns`).then(r => r.json())
    ]);

  // Store in app state/context for use in forms and filters
  return { difficulties, muscleGroups, equipment, bodyParts, movementPatterns };
}
```

## Performance Optimizations

1. **Debounce Search**: Add 300ms debounce on name search
2. **Cache Reference Data**: Load once and store in context
3. **Optimistic Updates**: Update UI before API confirms
4. **Lazy Load Images**: Use intersection observer for exercise images
5. **Pagination**: Default to 20 items per page for admin view

## Testing Considerations

1. **Mock API Responses**: Create mock data matching the exact structure
2. **Error Scenarios**: Test all error codes (400, 401, 403, 404, 409, 500)
3. **Validation**: Test all field validation rules
4. **Permissions**: Test with and without admin claims
5. **Edge Cases**: Empty lists, maximum field lengths, special characters

## Next Steps

1. Implement reusable API service layer
2. Create exercise list component with filtering
3. Build exercise form component with validation
4. Add proper error handling and loading states
5. Implement reference data caching
6. Add unit and integration tests