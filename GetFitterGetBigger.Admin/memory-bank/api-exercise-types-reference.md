# Exercise Types API Reference

This document provides detailed information about the Exercise Types reference table endpoints for the Admin application.

## Overview

Exercise Types categorize exercises into different phases of a workout session. This is a **read-only** reference table with exactly four fixed values:
- **Warmup**: Exercises performed to prepare the body for more intense activity
- **Workout**: Main exercises that form the core of the training session
- **Cooldown**: Exercises performed to help the body recover after intense activity
- **Rest**: Periods of rest between exercises or sets

## Business Rules

### Critical Rules for Exercise Type Assignment:
1. **Every exercise MUST have at least one exercise type**
2. **Rest Type Exclusivity**: 
   - "Rest" type CANNOT be combined with any other exercise types
   - An exercise can be either "Rest" OR any combination of the other three types (Warmup, Workout, Cooldown)
3. **Never All Types**: An exercise cannot have all four types assigned
4. **Valid Combinations**:
   - Rest only
   - Warmup only
   - Workout only
   - Cooldown only
   - Warmup + Workout
   - Warmup + Cooldown
   - Workout + Cooldown
   - Warmup + Workout + Cooldown

### UI Behavior Requirements:
- When user selects "Rest", automatically deselect all other exercise types
- When "Rest" is selected, disable/gray out the other exercise type options
- If user wants to select other types while "Rest" is selected, they must first unselect "Rest"
- Show validation error if user tries to save without any exercise type selected

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
    id: "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
    value: "Warmup",
    description: "Exercises to prepare the body for activity"
  },
  {
    id: "exercisetype-22334455-6677-8899-aabb-ccddeeff0011",
    value: "Workout",
    description: "Main training exercises"
  },
  {
    id: "exercisetype-33445566-7788-99aa-bbcc-ddeeff001122",
    value: "Cooldown",
    description: "Exercises for post-workout recovery"
  },
  {
    id: "exercisetype-44556677-8899-aabb-ccdd-eeff00112233",
    value: "Rest",
    description: "Rest periods between exercises"
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

### Exercise Type Selection Component

```javascript
import React, { useState, useEffect } from 'react';

const ExerciseTypeSelector = ({ value, onChange, error }) => {
  const [exerciseTypes, setExerciseTypes] = useState([]);
  const [selectedTypes, setSelectedTypes] = useState(value || []);

  useEffect(() => {
    loadExerciseTypes();
  }, []);

  const handleTypeToggle = (typeId) => {
    const type = exerciseTypes.find(t => t.id === typeId);
    const isRest = type?.value === 'Rest';
    const currentlySelected = [...selectedTypes];
    const isSelected = currentlySelected.includes(typeId);

    if (isRest && !isSelected) {
      // Selecting Rest - clear all other selections
      setSelectedTypes([typeId]);
      onChange([typeId]);
    } else if (!isRest && !isSelected) {
      // Selecting non-Rest type - remove Rest if present
      const filtered = currentlySelected.filter(id => {
        const t = exerciseTypes.find(et => et.id === id);
        return t?.value !== 'Rest';
      });
      filtered.push(typeId);
      setSelectedTypes(filtered);
      onChange(filtered);
    } else if (isSelected) {
      // Deselecting
      const filtered = currentlySelected.filter(id => id !== typeId);
      setSelectedTypes(filtered);
      onChange(filtered);
    }
  };

  const isRestSelected = selectedTypes.some(id => {
    const type = exerciseTypes.find(t => t.id === id);
    return type?.value === 'Rest';
  });

  return (
    <div className="exercise-type-selector">
      <label className="block text-sm font-medium mb-2">
        Exercise Types <span className="text-red-500">*</span>
      </label>
      
      <div className="space-y-2">
        {exerciseTypes.map(type => {
          const isSelected = selectedTypes.includes(type.id);
          const isDisabled = isRestSelected && type.value !== 'Rest' && !isSelected;
          
          return (
            <label
              key={type.id}
              className={`flex items-center p-3 border rounded cursor-pointer
                ${isSelected ? 'bg-blue-50 border-blue-500' : 'border-gray-300'}
                ${isDisabled ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-50'}
              `}
            >
              <input
                type="checkbox"
                checked={isSelected}
                onChange={() => !isDisabled && handleTypeToggle(type.id)}
                disabled={isDisabled}
                className="mr-3"
              />
              <div className="flex-1">
                <div className="font-medium">{type.value}</div>
                <div className="text-sm text-gray-600">{type.description}</div>
              </div>
            </label>
          );
        })}
      </div>

      {error && (
        <div className="mt-2 text-sm text-red-600">{error}</div>
      )}
      
      {isRestSelected && (
        <div className="mt-2 text-sm text-amber-600">
          ⚠️ Rest exercises cannot have other types. Deselect Rest to choose other types.
        </div>
      )}
    </div>
  );
};
```

### Validation Function

```javascript
const validateExerciseTypes = (selectedIds, exerciseTypes) => {
  // Check if at least one type is selected
  if (!selectedIds || selectedIds.length === 0) {
    return 'At least one exercise type must be selected';
  }

  // Check if all types are selected (not allowed)
  if (selectedIds.length === 4) {
    return 'An exercise cannot have all exercise types';
  }

  // Check Rest exclusivity rule
  const hasRest = selectedIds.some(id => 
    exerciseTypes.find(et => et.id === id)?.value === 'Rest'
  );
  
  if (hasRest && selectedIds.length > 1) {
    return 'Rest type cannot be combined with other exercise types';
  }

  return null; // No errors
};
```

### Exercise List Display

```javascript
const ExerciseTypeBadge = ({ type }) => {
  const getTypeColor = (value) => {
    switch(value) {
      case 'Warmup': return 'bg-yellow-100 text-yellow-800';
      case 'Workout': return 'bg-blue-100 text-blue-800';
      case 'Cooldown': return 'bg-green-100 text-green-800';
      case 'Rest': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getTypeColor(type.value)}`}>
      {type.value}
    </span>
  );
};

// In the exercise list/table
<td className="px-6 py-4">
  <div className="flex flex-wrap gap-1">
    {exercise.exerciseTypes.map(type => (
      <ExerciseTypeBadge key={type.id} type={type} />
    ))}
  </div>
</td>
```

## Integration with Exercise CRUD

### Create/Update Exercise Form

```javascript
const ExerciseForm = ({ exercise, onSubmit }) => {
  const [formData, setFormData] = useState({
    name: exercise?.name || '',
    description: exercise?.description || '',
    exerciseTypeIds: exercise?.exerciseTypes?.map(t => t.id) || [],
    // ... other fields
  });
  
  const [errors, setErrors] = useState({});
  const [exerciseTypes, setExerciseTypes] = useState([]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Validate exercise types
    const typeError = validateExerciseTypes(formData.exerciseTypeIds, exerciseTypes);
    if (typeError) {
      setErrors({ ...errors, exerciseTypes: typeError });
      return;
    }

    // Submit the form
    try {
      await onSubmit(formData);
    } catch (error) {
      // Handle submission error
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Other form fields */}
      
      <ExerciseTypeSelector
        value={formData.exerciseTypeIds}
        onChange={(ids) => setFormData({ ...formData, exerciseTypeIds: ids })}
        error={errors.exerciseTypes}
      />
      
      <button type="submit" className="btn btn-primary">
        {exercise ? 'Update Exercise' : 'Create Exercise'}
      </button>
    </form>
  );
};
```

## Caching Strategy

Exercise types are static reference data that never changes. Implement aggressive caching:

```javascript
class ExerciseTypeService {
  constructor() {
    this.cache = null;
  }

  async getAll() {
    // Cache forever since this is static data
    if (this.cache) {
      return this.cache;
    }

    const data = await getAllExerciseTypes();
    this.cache = data;
    
    // Store in localStorage for persistence across sessions
    localStorage.setItem('exerciseTypes', JSON.stringify(data));
    
    return data;
  }

  // Load from localStorage on app start
  loadFromStorage() {
    const stored = localStorage.getItem('exerciseTypes');
    if (stored) {
      this.cache = JSON.parse(stored);
    }
  }

  clearCache() {
    this.cache = null;
    localStorage.removeItem('exerciseTypes');
  }
}

// Initialize on app start
const exerciseTypeService = new ExerciseTypeService();
exerciseTypeService.loadFromStorage();
```

## Testing Scenarios

### Validation Tests
```javascript
describe('Exercise Type Validation', () => {
  test('should require at least one exercise type', () => {
    const error = validateExerciseTypes([], exerciseTypes);
    expect(error).toBe('At least one exercise type must be selected');
  });

  test('should not allow all exercise types', () => {
    const allTypeIds = exerciseTypes.map(t => t.id);
    const error = validateExerciseTypes(allTypeIds, exerciseTypes);
    expect(error).toBe('An exercise cannot have all exercise types');
  });

  test('should not allow Rest with other types', () => {
    const restId = exerciseTypes.find(t => t.value === 'Rest').id;
    const warmupId = exerciseTypes.find(t => t.value === 'Warmup').id;
    const error = validateExerciseTypes([restId, warmupId], exerciseTypes);
    expect(error).toBe('Rest type cannot be combined with other exercise types');
  });

  test('should allow valid combinations', () => {
    const warmupId = exerciseTypes.find(t => t.value === 'Warmup').id;
    const workoutId = exerciseTypes.find(t => t.value === 'Workout').id;
    const error = validateExerciseTypes([warmupId, workoutId], exerciseTypes);
    expect(error).toBeNull();
  });
});
```

### UI Behavior Tests
- Test Rest selection clears other selections
- Test other selections clear Rest
- Test disabled state when Rest is selected
- Test error messages display correctly
- Test form submission with invalid combinations

## Important Notes

1. **This is a read-only reference table** - The Admin application cannot create, update, or delete exercise types
2. **The four values are fixed** - Warmup, Workout, Cooldown, and Rest will never change
3. **Client-side validation is critical** - Always validate the Rest exclusivity rule before API calls
4. **Consider UX carefully** - Make the Rest exclusivity rule obvious in the UI to prevent user frustration