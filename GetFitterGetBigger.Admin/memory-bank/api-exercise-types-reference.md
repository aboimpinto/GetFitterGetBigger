# Exercise Types API Reference

This document provides detailed information about the Exercise Types reference table endpoints for the Admin application.

## Overview

Exercise Types categorize exercises into different phases of a workout session. The available types are:
- **Warmup**: Exercises performed to prepare the body for more intense activity
- **Workout**: Main exercises that form the core of the training session
- **Cooldown**: Exercises performed to help the body recover after intense activity
- **Rest**: Periods of rest between exercises or sets

## Business Rules

- The "Rest" type has special business rules - it cannot be combined with other exercise types
- When assigning exercise types to an exercise, you must validate this constraint on the client side

## API Endpoints

### Base URL
```javascript
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5214';
const EXERCISE_TYPES_BASE = `${API_BASE_URL}/api/ReferenceTables/ExerciseTypes`;
```

### 1. Get All Exercise Types

**Purpose**: Retrieve all available exercise types for dropdowns and multi-select components

```javascript
// GET /api/ReferenceTables/ExerciseTypes
async function getAllExerciseTypes() {
  const response = await fetch(`${EXERCISE_TYPES_BASE}`, {
    headers: {
      'Authorization': `Bearer ${authToken}`
    }
  });

  if (!response.ok) throw new Error(`Error: ${response.status}`);
  return response.json();
}
```

**Response Structure**:
```javascript
[
  {
    id: "exercisetype-uuid-1",
    value: "Warmup",
    name: "Warmup"
  },
  {
    id: "exercisetype-uuid-2",
    value: "Workout",
    name: "Workout"
  },
  {
    id: "exercisetype-uuid-3",
    value: "Cooldown",
    name: "Cooldown"
  },
  {
    id: "exercisetype-uuid-4",
    value: "Rest",
    name: "Rest"
  }
]
```

### 2. Get Exercise Type by ID

**Purpose**: Retrieve a specific exercise type by its ID

```javascript
// GET /api/ReferenceTables/ExerciseTypes/{id}
async function getExerciseTypeById(id) {
  const response = await fetch(`${EXERCISE_TYPES_BASE}/${id}`, {
    headers: {
      'Authorization': `Bearer ${authToken}`
    }
  });

  if (!response.ok) {
    if (response.status === 404) throw new Error('Exercise type not found');
    throw new Error(`Error: ${response.status}`);
  }
  
  return response.json();
}
```

### 3. Get Exercise Type by Value

**Purpose**: Retrieve a specific exercise type by its value (e.g., "Warmup", "Workout")

```javascript
// GET /api/ReferenceTables/ExerciseTypes/ByValue/{value}
async function getExerciseTypeByValue(value) {
  const response = await fetch(`${EXERCISE_TYPES_BASE}/ByValue/${value}`, {
    headers: {
      'Authorization': `Bearer ${authToken}`
    }
  });

  if (!response.ok) {
    if (response.status === 404) throw new Error('Exercise type not found');
    throw new Error(`Error: ${response.status}`);
  }
  
  return response.json();
}
```

## UI Implementation Guidelines

### Exercise Type Selection

When implementing exercise type selection in the Exercise form:

1. **Multi-Select Component**:
   ```javascript
   // Component state
   const [selectedExerciseTypes, setSelectedExerciseTypes] = useState([]);
   const [exerciseTypes, setExerciseTypes] = useState([]);
   
   // Validation function
   const validateExerciseTypes = (selectedIds) => {
     const hasRest = selectedIds.some(id => 
       exerciseTypes.find(et => et.id === id)?.value === 'Rest'
     );
     
     if (hasRest && selectedIds.length > 1) {
       return 'Rest type cannot be combined with other exercise types';
     }
     return null;
   };
   ```

2. **Visual Indicators**:
   - Use distinct colors or icons for each exercise type
   - Show a warning when "Rest" is selected with other types
   - Consider using badges or chips for selected types

### Exercise List Display

When displaying exercises with their types:

```javascript
// In the exercise list/table
<div className="exercise-types">
  {exercise.exerciseTypes.map(type => (
    <span key={type.id} className={`badge badge-${type.value.toLowerCase()}`}>
      {type.name}
    </span>
  ))}
</div>
```

### Filtering by Exercise Type

Add exercise type filtering to the exercise list:

```javascript
// Add to filter parameters
const [filterExerciseTypes, setFilterExerciseTypes] = useState([]);

// Update the getExercises function call
const exercises = await getExercises({
  ...otherParams,
  exerciseTypeIds: filterExerciseTypes
});
```

## Caching Strategy

Exercise types are static reference data that rarely changes. Implement aggressive caching:

```javascript
const CACHE_DURATION = 24 * 60 * 60 * 1000; // 24 hours

class ExerciseTypeService {
  constructor() {
    this.cache = null;
    this.cacheTimestamp = null;
  }

  async getAll() {
    const now = Date.now();
    
    if (this.cache && this.cacheTimestamp && (now - this.cacheTimestamp < CACHE_DURATION)) {
      return this.cache;
    }

    const data = await getAllExerciseTypes();
    this.cache = data;
    this.cacheTimestamp = now;
    
    return data;
  }

  clearCache() {
    this.cache = null;
    this.cacheTimestamp = null;
  }
}
```

## Integration with Exercise CRUD

When creating or updating exercises, include exercise types:

```javascript
// In the exercise form submission
const exerciseData = {
  name: formData.name,
  description: formData.description,
  coachNotes: formData.coachNotes,
  exerciseTypeIds: selectedExerciseTypes, // Array of exercise type IDs
  // ... other fields
};

// Validate before submission
const typeError = validateExerciseTypes(selectedExerciseTypes);
if (typeError) {
  showError(typeError);
  return;
}
```

## Testing Considerations

1. **Validation Testing**:
   - Test that Rest type cannot be combined with others
   - Test empty exercise type selection (allowed)
   - Test all valid combinations

2. **UI Testing**:
   - Test multi-select behavior
   - Test visual feedback for validation errors
   - Test filtering by exercise types

3. **Performance Testing**:
   - Verify caching works correctly
   - Test behavior when cache expires
   - Test concurrent requests