# Exercise Types API Reference - Client Applications

This document provides information about the Exercise Types reference table for client applications (Mobile, Web, Desktop).

## Overview

Exercise Types categorize exercises into different workout phases. This is a **read-only** reference table with exactly four fixed values:
- **Warmup**: Exercises to prepare the body for activity
- **Workout**: Main training exercises
- **Cooldown**: Post-workout recovery exercises
- **Rest**: Rest periods between exercises

## Important Business Rules

Client applications must be aware of these rules for proper display:

1. **Every exercise has at least one exercise type**
2. **Rest Type Display**: 
   - Rest exercises are standalone and never combined with other types
   - If an exercise has "Rest" type, it will have no other types
3. **Valid Combinations**: An exercise can have:
   - Rest only
   - Any combination of Warmup, Workout, and Cooldown (but never all four types)

## API Endpoints

### Base Configuration

**Mobile (React Native)**
```javascript
const EXERCISE_TYPES_BASE = `${API_BASE_URL}/api/ReferenceTables/ExerciseTypes`;
```

**Web (React)**
```javascript
const EXERCISE_TYPES_BASE = `${API_BASE_URL}/api/ReferenceTables/ExerciseTypes`;
```

**Desktop (Avalonia/C#)**
```csharp
private const string ExerciseTypesBase = "/api/ReferenceTables/ExerciseTypes";
```

### Get All Exercise Types

**Purpose**: Load exercise types for filtering and display

**Endpoint**: `GET /api/ReferenceTables/ExerciseTypes`

**Response**:
```json
[
  {
    "id": "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
    "value": "Warmup",
    "description": "Exercises to prepare the body for activity"
  },
  {
    "id": "exercisetype-22334455-6677-8899-aabb-ccddeeff0011",
    "value": "Workout",
    "description": "Main training exercises"
  },
  {
    "id": "exercisetype-33445566-7788-99aa-bbcc-ddeeff001122",
    "value": "Cooldown",
    "description": "Exercises for post-workout recovery"
  },
  {
    "id": "exercisetype-44556677-8899-aabb-ccdd-eeff00112233",
    "value": "Rest",
    "description": "Rest periods between exercises"
  }
]
```

## Implementation Examples

### Mobile (React Native)

```javascript
// services/exerciseTypeService.js
import AsyncStorage from '@react-native-async-storage/async-storage';

class ExerciseTypeService {
  constructor() {
    this.cache = null;
  }

  async getAll() {
    // Try cache first
    if (this.cache) return this.cache;

    // Try local storage
    const cached = await AsyncStorage.getItem('exerciseTypes');
    if (cached) {
      this.cache = JSON.parse(cached);
      return this.cache;
    }

    // Fetch from API
    const response = await fetch(`${EXERCISE_TYPES_BASE}`, {
      headers: await getHeaders()
    });

    if (!response.ok) throw new Error('Failed to fetch exercise types');

    const data = await response.json();
    this.cache = data;
    
    // Cache for offline use
    await AsyncStorage.setItem('exerciseTypes', JSON.stringify(data));
    
    return data;
  }
}

// components/ExerciseTypeBadges.js
import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

const ExerciseTypeBadges = ({ types }) => {
  const getTypeStyle = (value) => {
    switch(value) {
      case 'Warmup': return styles.warmup;
      case 'Workout': return styles.workout;
      case 'Cooldown': return styles.cooldown;
      case 'Rest': return styles.rest;
      default: return styles.default;
    }
  };

  return (
    <View style={styles.container}>
      {types.map(type => (
        <View key={type.id} style={[styles.badge, getTypeStyle(type.value)]}>
          <Text style={styles.badgeText}>{type.value}</Text>
        </View>
      ))}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 4,
  },
  badge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgeText: {
    fontSize: 12,
    fontWeight: '600',
  },
  warmup: {
    backgroundColor: '#FEF3C7',
    color: '#92400E',
  },
  workout: {
    backgroundColor: '#DBEAFE',
    color: '#1E40AF',
  },
  cooldown: {
    backgroundColor: '#D1FAE5',
    color: '#065F46',
  },
  rest: {
    backgroundColor: '#F3F4F6',
    color: '#374151',
  },
});
```

### Web (React)

```javascript
// hooks/useExerciseTypes.js
import { useState, useEffect } from 'react';
import { exerciseTypeService } from '../services/exerciseTypeService';

export const useExerciseTypes = () => {
  const [exerciseTypes, setExerciseTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const loadExerciseTypes = async () => {
      try {
        const types = await exerciseTypeService.getAll();
        setExerciseTypes(types);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    loadExerciseTypes();
  }, []);

  return { exerciseTypes, loading, error };
};

// components/ExerciseCard.jsx
const ExerciseCard = ({ exercise }) => {
  const getTypeIcon = (value) => {
    switch(value) {
      case 'Warmup': return '🔥';
      case 'Workout': return '💪';
      case 'Cooldown': return '🧊';
      case 'Rest': return '😴';
      default: return '📋';
    }
  };

  return (
    <div className="exercise-card">
      <h3>{exercise.name}</h3>
      <p>{exercise.description}</p>
      
      <div className="exercise-types">
        {exercise.exerciseTypes.map(type => (
          <span 
            key={type.id} 
            className={`type-badge type-${type.value.toLowerCase()}`}
            title={type.description}
          >
            {getTypeIcon(type.value)} {type.value}
          </span>
        ))}
      </div>
      
      {/* Rest of exercise details */}
    </div>
  );
};
```

### Desktop (Avalonia/C#)

```csharp
// Models/ExerciseTypeDto.cs
public class ExerciseTypeDto
{
    public string Id { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
}

// Services/ExerciseTypeService.cs
public class ExerciseTypeService
{
    private readonly HttpClient _httpClient;
    private List<ExerciseTypeDto> _cache;
    private readonly string _cacheKey = "ExerciseTypes";

    public ExerciseTypeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        LoadFromCache();
    }

    public async Task<List<ExerciseTypeDto>> GetAllAsync()
    {
        if (_cache != null) return _cache;

        var response = await _httpClient.GetAsync("api/ReferenceTables/ExerciseTypes");
        response.EnsureSuccessStatusCode();

        _cache = await response.Content.ReadFromJsonAsync<List<ExerciseTypeDto>>();
        SaveToCache();

        return _cache;
    }

    private void LoadFromCache()
    {
        var cached = Preferences.Get(_cacheKey, string.Empty);
        if (!string.IsNullOrEmpty(cached))
        {
            _cache = JsonSerializer.Deserialize<List<ExerciseTypeDto>>(cached);
        }
    }

    private void SaveToCache()
    {
        if (_cache != null)
        {
            Preferences.Set(_cacheKey, JsonSerializer.Serialize(_cache));
        }
    }
}

// ViewModels/ExerciseViewModel.cs
public class ExerciseViewModel : ViewModelBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ObservableCollection<ExerciseTypeDto> ExerciseTypes { get; set; }
    
    public string ExerciseTypesDisplay => 
        string.Join(", ", ExerciseTypes.Select(t => t.Value));
    
    public bool IsRestExercise => 
        ExerciseTypes.Any(t => t.Value == "Rest");
    
    public SolidColorBrush PrimaryTypeColor
    {
        get
        {
            var primaryType = ExerciseTypes.FirstOrDefault();
            if (primaryType == null) return Brushes.Gray;
            
            return primaryType.Value switch
            {
                "Warmup" => new SolidColorBrush(Color.Parse("#FEF3C7")),
                "Workout" => new SolidColorBrush(Color.Parse("#DBEAFE")),
                "Cooldown" => new SolidColorBrush(Color.Parse("#D1FAE5")),
                "Rest" => new SolidColorBrush(Color.Parse("#F3F4F6")),
                _ => Brushes.Gray
            };
        }
    }
}
```

## Display Guidelines

### Exercise Lists
- Show exercise type badges prominently
- Use consistent colors across all platforms
- Consider using icons for better visual recognition

### Exercise Details
- Display all assigned exercise types
- For Rest exercises, consider special styling or placement
- Show type descriptions on hover/tap for more context

### Filtering
```javascript
// Example filter implementation
const filterExercisesByType = (exercises, selectedTypeId) => {
  if (!selectedTypeId) return exercises;
  
  return exercises.filter(exercise => 
    exercise.exerciseTypes.some(type => type.id === selectedTypeId)
  );
};

// Filter UI component
const ExerciseTypeFilter = ({ onFilterChange }) => {
  const { exerciseTypes } = useExerciseTypes();
  
  return (
    <select onChange={(e) => onFilterChange(e.target.value)}>
      <option value="">All Types</option>
      {exerciseTypes.map(type => (
        <option key={type.id} value={type.id}>
          {type.value}
        </option>
      ))}
    </select>
  );
};
```

## Workout Display Considerations

When displaying exercises during workout execution:

1. **Rest Exercises**: 
   - Display prominently with timer
   - Different UI treatment (e.g., full-screen rest timer)
   - Clear indication that this is a rest period

2. **Type Indicators**:
   - Show exercise type during workout
   - Help users understand workout structure
   - Visual cues for workout phases

```javascript
// Example workout exercise display
const WorkoutExerciseView = ({ exercise, onComplete }) => {
  const isRest = exercise.exerciseTypes.some(t => t.value === 'Rest');
  
  if (isRest) {
    return <RestTimerView duration={exercise.restDuration} onComplete={onComplete} />;
  }
  
  return (
    <ExerciseExecutionView 
      exercise={exercise}
      showTypes={true}
      onComplete={onComplete}
    />
  );
};
```

## Performance Tips

1. **Cache Aggressively**: Exercise types never change
2. **Load Once**: Fetch on app start and keep in memory
3. **Offline Support**: Always save to local storage
4. **Bundle Size**: Consider hardcoding if needed for performance

## Important Notes

- Exercise types are **static data** - they will never change
- The four values (Warmup, Workout, Cooldown, Rest) are fixed
- Clients have **read-only access** - no create/update/delete operations
- Always handle the special case of Rest exercises in UI