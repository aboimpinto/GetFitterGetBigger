# Exercise API Integration Guide for Client Applications

This document provides all necessary information for client applications (Mobile, Web, Desktop) to retrieve and display exercises. Client applications have read-only access to the exercise library.

## Overview

The Exercise API allows client applications to:
- Browse the complete exercise library
- Search and filter exercises
- View detailed exercise information including instructions and media
- Access exercises for workout execution

**Note**: Clients have read-only access. All exercise management (create, update, delete) is done through the Admin application.

## API Configuration

### Base Setup

#### Mobile (React Native)
```javascript
// config/api.js
import AsyncStorage from '@react-native-async-storage/async-storage';

const API_BASE_URL = __DEV__ 
  ? 'http://localhost:5214'  // Development
  : 'https://api.getfitterbigger.com'; // Production (not yet assigned)

// Swagger documentation available at: http://localhost:5214/swagger

export const getHeaders = async () => {
  const token = await AsyncStorage.getItem('authToken');
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  };
};
```

#### Web (React)
```javascript
// services/api.js
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5214';
// Production URL: Not yet assigned
// Swagger documentation available at: http://localhost:5214/swagger

export const getHeaders = () => {
  const token = localStorage.getItem('authToken');
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  };
};
```

#### Desktop (Avalonia)
```csharp
// Services/ApiService.cs
public class ApiService
{
    private readonly string _baseUrl;
    private readonly IAuthService _authService;
    
    public ApiService(IConfiguration config, IAuthService authService)
    {
        _baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5214"; // Production URL not yet assigned
        _authService = authService;
    }
    
    private HttpClient GetClient()
    {
        var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authService.GetToken());
        return client;
    }
}
```

### Authentication Requirements
- All endpoints require a valid Bearer token
- Tokens are obtained through the authentication flow
- Handle 401 responses by refreshing token or re-authenticating
- Client access is determined by user's tier: `Free-Tier`, `WorkoutPlan-Tier` (future), `DietPlan-Tier` (future)

## Available Endpoints

### 1. List Exercises

**Purpose**: Display exercises in workout screens, exercise browsers, or search results

#### Request
```
GET /api/exercises?page=1&pageSize=20&name=squat
```

#### Implementation Examples

**Mobile (React Native)**
```javascript
// services/exerciseService.js
export const getExercises = async (params = {}) => {
  try {
    const headers = await getHeaders();
    const queryParams = new URLSearchParams({
      page: params.page || 1,
      pageSize: params.pageSize || 20,
      ...(params.name && { name: params.name }),
      ...(params.difficultyId && { difficultyId: params.difficultyId })
    });

    // Handle array parameters
    params.muscleGroupIds?.forEach(id => queryParams.append('muscleGroupIds', id));
    params.equipmentIds?.forEach(id => queryParams.append('equipmentIds', id));

    const response = await fetch(
      `${API_BASE_URL}/api/exercises?${queryParams}`,
      { headers }
    );

    if (!response.ok) throw new Error(`Error: ${response.status}`);
    return response.json();
  } catch (error) {
    console.error('Failed to fetch exercises:', error);
    throw error;
  }
};

// Usage in component
const ExerciseList = () => {
  const [exercises, setExercises] = useState([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);

  useEffect(() => {
    loadExercises();
  }, [page]);

  const loadExercises = async () => {
    try {
      setLoading(true);
      const data = await getExercises({ page, pageSize: 20 });
      setExercises(prev => page === 1 ? data.exercises : [...prev, ...data.exercises]);
    } catch (error) {
      Alert.alert('Error', 'Failed to load exercises');
    } finally {
      setLoading(false);
    }
  };

  return (
    <FlatList
      data={exercises}
      renderItem={({ item }) => <ExerciseCard exercise={item} />}
      onEndReached={() => setPage(p => p + 1)}
      onEndReachedThreshold={0.5}
    />
  );
};
```

**Web (React)**
```javascript
// hooks/useExercises.js
export const useExercises = (filters) => {
  const [data, setData] = useState({ exercises: [], pagination: null });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchExercises = async () => {
      try {
        setLoading(true);
        const result = await getExercises(filters);
        setData(result);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchExercises();
  }, [filters]);

  return { ...data, loading, error };
};
```

#### Response Structure
```json
{
  "pagination": {
    "total": 150,
    "pages": 8,
    "currentPage": 1,
    "limit": 20
  },
  "exercises": [
    {
      "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "name": "Barbell Back Squat",
      "description": "A fundamental compound exercise for lower body strength.",
      "difficulty": {
        "id": "difficultylevel-xxx",
        "name": "Intermediate"
      },
      "isUnilateral": false,
      "imageUrl": "https://api.getfitterbigger.com/images/squat.jpeg",
      "videoUrl": "https://api.getfitterbigger.com/videos/squat.mp4",
      "muscleGroups": [
        {
          "id": "musclegroup-xxx",
          "name": "Quadriceps",
          "role": "Primary"
        }
      ],
      "equipment": [
        {
          "id": "equipment-xxx",
          "name": "Barbell"
        }
      ]
    }
  ]
}
```

### 2. Get Exercise Details

**Purpose**: Display full exercise information during workout execution

#### Request
```
GET /api/exercises/{exerciseId}
```

#### Implementation Examples

**Mobile (React Native)**
```javascript
// screens/ExerciseDetailScreen.js
const ExerciseDetailScreen = ({ route }) => {
  const { exerciseId } = route.params;
  const [exercise, setExercise] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadExerciseDetails();
  }, [exerciseId]);

  const loadExerciseDetails = async () => {
    try {
      const headers = await getHeaders();
      const response = await fetch(
        `${API_BASE_URL}/api/exercises/${exerciseId}`,
        { headers }
      );
      
      if (!response.ok) throw new Error('Failed to load exercise');
      const data = await response.json();
      setExercise(data);
    } catch (error) {
      Alert.alert('Error', 'Failed to load exercise details');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <ActivityIndicator />;
  if (!exercise) return <Text>Exercise not found</Text>;

  return (
    <ScrollView>
      <Image source={{ uri: exercise.imageUrl }} style={styles.image} />
      <Text style={styles.title}>{exercise.name}</Text>
      <Text style={styles.difficulty}>{exercise.difficulty.name}</Text>
      <Text style={styles.description}>{exercise.description}</Text>
      
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Instructions</Text>
        <Text style={styles.instructions}>{exercise.instructions}</Text>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Target Muscles</Text>
        {exercise.muscleGroups.map(mg => (
          <Text key={mg.id}>{mg.name} ({mg.role})</Text>
        ))}
      </View>

      {exercise.videoUrl && (
        <TouchableOpacity onPress={() => openVideo(exercise.videoUrl)}>
          <Text style={styles.videoLink}>Watch Video</Text>
        </TouchableOpacity>
      )}
    </ScrollView>
  );
};
```

**Desktop (Avalonia with ReactiveUI)**
```csharp
// ViewModels/ExerciseDetailViewModel.cs
public class ExerciseDetailViewModel : ViewModelBase
{
    private readonly IExerciseService _exerciseService;
    private ExerciseDto _exercise;

    public ExerciseDetailViewModel(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    public ExerciseDto Exercise
    {
        get => _exercise;
        private set => this.RaiseAndSetIfChanged(ref _exercise, value);
    }

    public async Task LoadExercise(string exerciseId)
    {
        try
        {
            Exercise = await _exerciseService.GetExerciseAsync(exerciseId);
        }
        catch (Exception ex)
        {
            await ShowError("Failed to load exercise details");
        }
    }
}
```

## UI/UX Guidelines

### Exercise List Display
1. **Card Layout**: Show name, difficulty, primary muscles, and thumbnail
2. **Search**: Implement debounced search (300ms) for smooth experience
3. **Filters**: Quick filters for difficulty and equipment availability
4. **Loading**: Show skeleton loaders or progress indicators
5. **Empty States**: Clear messages when no exercises match filters

### Exercise Detail View
1. **Media Section**: 
   - Image gallery with pinch-to-zoom on mobile
   - Video player integration or external player launch
2. **Instructions**: 
   - Clear, numbered steps
   - Expandable/collapsible on mobile for space
3. **Muscle Groups**: Visual indicators for Primary/Secondary/Stabilizer
4. **Equipment**: List required equipment prominently
5. **Related Exercises**: Show similar exercises (same muscle group/pattern)

### Platform-Specific Considerations

#### Mobile (iOS/Android)
- Implement pull-to-refresh on exercise lists
- Use native video players for exercise videos
- Cache images for offline viewing
- Implement swipe gestures for navigation
- Optimize for various screen sizes

#### Web
- Responsive grid layout for exercise cards
- Modal or drawer for exercise details
- Keyboard navigation support
- Progressive image loading
- Print-friendly exercise sheets

#### Desktop
- Multi-column layouts for larger screens
- Keyboard shortcuts for common actions
- Right-click context menus
- Drag-and-drop for workout building
- Multi-window support

## Performance Optimization

### Caching Strategy
```javascript
// Mobile caching example
const CACHE_DURATION = 1000 * 60 * 60; // 1 hour

export const getCachedExercises = async (params) => {
  const cacheKey = `exercises_${JSON.stringify(params)}`;
  const cached = await AsyncStorage.getItem(cacheKey);
  
  if (cached) {
    const { data, timestamp } = JSON.parse(cached);
    if (Date.now() - timestamp < CACHE_DURATION) {
      return data;
    }
  }
  
  const data = await getExercises(params);
  await AsyncStorage.setItem(cacheKey, JSON.stringify({
    data,
    timestamp: Date.now()
  }));
  
  return data;
};
```

### Image Optimization
- Use appropriate image sizes (thumbnails for lists, full size for details)
- Implement lazy loading for images
- Cache images locally on mobile/desktop
- Use progressive loading for better perceived performance

### Pagination Best Practices
- Mobile: Infinite scroll with 20 items per page
- Web: Traditional pagination or infinite scroll based on UX
- Desktop: Larger page sizes (50-100) with virtual scrolling
- Preload next page when user nears end of current page

## Error Handling

```javascript
// Centralized error handler
export const handleApiError = (error, context) => {
  if (error.status === 401) {
    // Redirect to login or refresh token
    navigateToAuth();
  } else if (error.status === 404) {
    showError('Exercise not found');
  } else if (error.status >= 500) {
    showError('Server error. Please try again later.');
  } else {
    showError('Something went wrong. Please try again.');
  }
  
  // Log for debugging
  console.error(`API Error in ${context}:`, error);
};
```

## Offline Support (Mobile/Desktop)

```javascript
// Sync exercises for offline use
export const syncExercisesForOffline = async () => {
  try {
    const allExercises = [];
    let page = 1;
    let hasMore = true;
    
    while (hasMore) {
      const data = await getExercises({ page, pageSize: 50 });
      allExercises.push(...data.exercises);
      hasMore = page < data.pagination.pages;
      page++;
    }
    
    await AsyncStorage.setItem('offline_exercises', JSON.stringify({
      exercises: allExercises,
      syncedAt: Date.now()
    }));
    
    // Download images for offline viewing
    await downloadExerciseImages(allExercises);
    
  } catch (error) {
    console.error('Failed to sync exercises:', error);
  }
};
```

## Testing Considerations

1. **Mock Data**: Create realistic mock data for development
2. **Network Conditions**: Test with slow/intermittent connections
3. **Error Scenarios**: Test all HTTP error codes
4. **Performance**: Test with large exercise lists (1000+ items)
5. **Accessibility**: Ensure screen readers can navigate exercises

## Next Steps

1. Implement exercise service layer with caching
2. Create reusable exercise list component
3. Build exercise detail views for each platform
4. Add offline support for mobile/desktop
5. Implement search with debouncing
6. Add unit and integration tests