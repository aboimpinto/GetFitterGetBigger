# Exercise API Integration Guide for Client Applications

This document provides all necessary information for client applications (Mobile, Web, Desktop) to retrieve and display exercises. Client applications have read-only access to the exercise library.

## Overview

The Exercise API allows client applications to:
- Browse the complete exercise library with pagination
- Search and filter exercises
- View detailed exercise information including coach notes and media
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
      setExercises(prev => page === 1 ? data.items : [...prev, ...data.items]);
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
  const [data, setData] = useState({ 
    items: [], 
    currentPage: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  });
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
  "items": [
    {
      "id": "exercise-a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "name": "Barbell Back Squat",
      "description": "A fundamental compound exercise for lower body strength.",
      "difficulty": {
        "id": "difficultylevel-xxx",
        "value": "Intermediate",
        "description": "Requires good form and moderate strength"
      },
      "isUnilateral": false,
      "isActive": true,
      "imageUrl": "https://api.getfitterbigger.com/images/squat.jpeg",
      "videoUrl": "https://api.getfitterbigger.com/videos/squat.mp4",
      "coachNotes": [
        {
          "id": "note-1",
          "text": "Keep your chest up and core engaged throughout the movement",
          "order": 1
        },
        {
          "id": "note-2",
          "text": "Drive through your heels and squeeze glutes at the top",
          "order": 2
        }
      ],
      "exerciseTypes": [
        {
          "id": "type-1",
          "value": "Strength",
          "description": "Primary strength building exercise"
        }
      ],
      "muscleGroups": [
        {
          "muscleGroup": {
            "id": "musclegroup-xxx",
            "name": "Quadriceps",
            "bodyPartId": "bodypart-legs-xxx",
            "bodyPartName": "Legs",
            "isActive": true,
            "createdAt": "2025-07-01T10:00:00Z",
            "updatedAt": "2025-07-01T10:00:00Z"
          },
          "role": {
            "id": "role-1",
            "value": "Primary",
            "description": "Main muscle targeted"
          }
        },
        {
          "muscleGroup": {
            "id": "musclegroup-yyy",
            "name": "Glutes",
            "bodyPartId": "bodypart-legs-xxx",
            "bodyPartName": "Legs",
            "isActive": true,
            "createdAt": "2025-07-01T10:00:00Z",
            "updatedAt": "2025-07-01T10:00:00Z"
          },
          "role": {
            "id": "role-2",
            "value": "Secondary",
            "description": "Supporting muscle"
          }
        }
      ],
      "equipment": [
        {
          "id": "equipment-xxx",
          "value": "Barbell",
          "description": "Olympic barbell"
        }
      ],
      "movementPatterns": [
        {
          "id": "pattern-1",
          "value": "Squat",
          "description": "Hip and knee flexion pattern"
        }
      ],
      "bodyParts": [
        {
          "id": "bodypart-1",
          "value": "Lower Body",
          "description": "Legs and glutes"
        }
      ]
    }
  ],
  "currentPage": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
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
        <Text style={styles.sectionTitle}>Coach Notes</Text>
        {exercise.coachNotes
          .sort((a, b) => a.order - b.order)
          .map((note, index) => (
            <View key={note.id} style={styles.noteItem}>
              <Text style={styles.noteNumber}>{index + 1}.</Text>
              <Text style={styles.noteText}>{note.text}</Text>
            </View>
          ))}
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Target Muscles</Text>
        {exercise.muscleGroups.map(mg => (
          <View key={mg.muscleGroup.id} style={styles.muscleItem}>
            <Text style={styles.muscleName}>{mg.muscleGroup.name}</Text>
            <Text style={styles.muscleRole}>{mg.role.value}</Text>
          </View>
        ))}
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Exercise Types</Text>
        {exercise.exerciseTypes.map(type => (
          <Text key={type.id} style={styles.typeTag}>{type.value}</Text>
        ))}
      </View>

      {exercise.equipment.length > 0 && (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Equipment</Text>
          <View style={styles.equipmentList}>
            {exercise.equipment.map(eq => (
              <Text key={eq.id} style={styles.equipmentTag}>{eq.value}</Text>
            ))}
          </View>
        </View>
      )}

      {exercise.videoUrl && (
        <TouchableOpacity onPress={() => openVideo(exercise.videoUrl)}>
          <Text style={styles.videoLink}>Watch Video</Text>
        </TouchableOpacity>
      )}
    </ScrollView>
  );
};
```

**Web (React)**
```javascript
// components/ExerciseDetail.js
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getExerciseById } from '../services/exerciseService';

const ExerciseDetail = () => {
  const { exerciseId } = useParams();
  const [exercise, setExercise] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const loadExercise = async () => {
      try {
        setLoading(true);
        const data = await getExerciseById(exerciseId);
        setExercise(data);
      } catch (err) {
        setError('Failed to load exercise details');
      } finally {
        setLoading(false);
      }
    };

    loadExercise();
  }, [exerciseId]);

  if (loading) return <div className="loading-spinner">Loading...</div>;
  if (error) return <div className="error-message">{error}</div>;
  if (!exercise) return <div>Exercise not found</div>;

  return (
    <div className="exercise-detail">
      <div className="exercise-header">
        <h1>{exercise.name}</h1>
        <span className="difficulty-badge difficulty-{exercise.difficulty.value.toLowerCase()}">
          {exercise.difficulty.value}
        </span>
      </div>

      {exercise.imageUrl && (
        <img src={exercise.imageUrl} alt={exercise.name} className="exercise-image" />
      )}

      <p className="exercise-description">{exercise.description}</p>

      <div className="coach-notes-section">
        <h2>Coach Notes</h2>
        <ol className="coach-notes">
          {exercise.coachNotes
            .sort((a, b) => a.order - b.order)
            .map(note => (
              <li key={note.id}>{note.text}</li>
            ))}
        </ol>
      </div>

      <div className="muscle-groups-section">
        <h2>Target Muscles</h2>
        <div className="muscle-groups">
          {exercise.muscleGroups.map(mg => (
            <div key={mg.muscleGroup.id} className="muscle-group">
              <span className="muscle-name">{mg.muscleGroup.name}</span>
              <span className={`role-badge role-${mg.role.value.toLowerCase()}`}>
                {mg.role.value}
              </span>
            </div>
          ))}
        </div>
      </div>

      <div className="exercise-metadata">
        <div className="exercise-types">
          <h3>Exercise Types</h3>
          <div className="tags">
            {exercise.exerciseTypes.map(type => (
              <span key={type.id} className="type-tag">{type.value}</span>
            ))}
          </div>
        </div>

        {exercise.equipment.length > 0 && (
          <div className="equipment-section">
            <h3>Equipment</h3>
            <div className="tags">
              {exercise.equipment.map(eq => (
                <span key={eq.id} className="equipment-tag">{eq.value}</span>
              ))}
            </div>
          </div>
        )}

        {exercise.movementPatterns.length > 0 && (
          <div className="movement-patterns">
            <h3>Movement Patterns</h3>
            <div className="tags">
              {exercise.movementPatterns.map(pattern => (
                <span key={pattern.id} className="pattern-tag">{pattern.value}</span>
              ))}
            </div>
          </div>
        )}
      </div>

      {exercise.videoUrl && (
        <div className="video-section">
          <a href={exercise.videoUrl} target="_blank" rel="noopener noreferrer" 
             className="video-link">
            Watch Exercise Video
          </a>
        </div>
      )}
    </div>
  );
};

export default ExerciseDetail;
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

// Models/ExerciseDto.cs
public class ExerciseDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DifficultyDto Difficulty { get; set; }
    public bool IsUnilateral { get; set; }
    public bool IsActive { get; set; }
    public string ImageUrl { get; set; }
    public string VideoUrl { get; set; }
    public List<CoachNoteDto> CoachNotes { get; set; }
    public List<ExerciseTypeDto> ExerciseTypes { get; set; }
    public List<MuscleGroupRoleDto> MuscleGroups { get; set; }
    public List<EquipmentDto> Equipment { get; set; }
    public List<MovementPatternDto> MovementPatterns { get; set; }
    public List<BodyPartDto> BodyParts { get; set; }
}

public class CoachNoteDto
{
    public string Id { get; set; }
    public string Text { get; set; }
    public int Order { get; set; }
}

public class MuscleGroupRoleDto
{
    public MuscleGroupDto MuscleGroup { get; set; }
    public RoleDto Role { get; set; }
}

public class DifficultyDto
{
    public string Id { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
}

// Views/ExerciseDetailView.axaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="GetFitterGetBigger.Desktop.Views.ExerciseDetailView">
    <ScrollViewer>
        <StackPanel Margin="20">
            <TextBlock Text="{Binding Exercise.Name}" FontSize="24" FontWeight="Bold" />
            <TextBlock Text="{Binding Exercise.Difficulty.Value}" 
                       Classes="difficulty-badge" Margin="0,10" />
            
            <Image Source="{Binding Exercise.ImageUrl}" 
                   MaxHeight="300" Margin="0,10" />
            
            <TextBlock Text="{Binding Exercise.Description}" 
                       TextWrapping="Wrap" Margin="0,10" />
            
            <TextBlock Text="Coach Notes" FontSize="18" FontWeight="Bold" Margin="0,20,0,10" />
            <ItemsControl Items="{Binding Exercise.CoachNotes}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal" Margin="0,5">
                            <TextBlock Text="{Binding Order}" Margin="0,0,10,0" />
                            <TextBlock Text="{Binding Text}" TextWrapping="Wrap" />
                        </StackPanel>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
            
            <TextBlock Text="Target Muscles" FontSize="18" FontWeight="Bold" Margin="0,20,0,10" />
            <ItemsControl Items="{Binding Exercise.MuscleGroups}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal" Margin="0,5">
                            <TextBlock Text="{Binding MuscleGroup.Name}" Margin="0,0,10,0" />
                            <TextBlock Text="{Binding Role.Value}" 
                                       Classes="role-badge" />
                        </StackPanel>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </ScrollViewer>
</UserControl>
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
2. **Coach Notes**: 
   - Display ordered notes with clear numbering
   - Expandable/collapsible on mobile for space
   - Typography that's easy to read during workouts
3. **Muscle Groups**: 
   - Visual indicators for Primary/Secondary/Stabilizer roles
   - Show muscle group descriptions on hover/tap
4. **Equipment**: List required equipment prominently with descriptions
5. **Exercise Types**: Display type badges (Strength, Cardio, etc.)
6. **Movement Patterns**: Show movement pattern tags for filtering
7. **Active Status**: Only show active exercises to clients

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

## Complete Service Implementation Examples

### Mobile (React Native) Exercise Service
```javascript
// services/exerciseService.js
import AsyncStorage from '@react-native-async-storage/async-storage';
import { API_BASE_URL } from '../config/api';

export const exerciseService = {
  // Get paginated list of exercises
  async getExercises(params = {}) {
    try {
      const token = await AsyncStorage.getItem('authToken');
      const queryParams = new URLSearchParams({
        page: params.page || 1,
        pageSize: params.pageSize || 20,
        ...(params.name && { name: params.name }),
        ...(params.difficultyId && { difficultyId: params.difficultyId })
      });

      // Handle array parameters
      params.muscleGroupIds?.forEach(id => queryParams.append('muscleGroupIds', id));
      params.equipmentIds?.forEach(id => queryParams.append('equipmentIds', id));
      params.exerciseTypeIds?.forEach(id => queryParams.append('exerciseTypeIds', id));

      const response = await fetch(
        `${API_BASE_URL}/api/exercises?${queryParams}`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        }
      );

      if (!response.ok) {
        if (response.status === 401) {
          // Handle token refresh or re-authentication
          throw new Error('Unauthorized');
        }
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching exercises:', error);
      throw error;
    }
  },

  // Get single exercise details
  async getExerciseById(exerciseId) {
    try {
      const token = await AsyncStorage.getItem('authToken');
      const response = await fetch(
        `${API_BASE_URL}/api/exercises/${exerciseId}`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching exercise details:', error);
      throw error;
    }
  }
};
```

### Web (React) Exercise Service with Axios
```javascript
// services/exerciseService.js
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5214';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add auth token to requests
api.interceptors.request.use(
  config => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

// Handle token expiration
api.interceptors.response.use(
  response => response,
  async error => {
    if (error.response?.status === 401) {
      // Handle token refresh or redirect to login
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const exerciseService = {
  async getExercises(params = {}) {
    const response = await api.get('/api/exercises', { params });
    return response.data;
  },

  async getExerciseById(exerciseId) {
    const response = await api.get(`/api/exercises/${exerciseId}`);
    return response.data;
  }
};
```

### Desktop (Avalonia) Exercise Service
```csharp
// Services/ExerciseService.cs
using System.Net.Http.Json;
using System.Web;

public class ExerciseService : IExerciseService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public ExerciseService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _httpClient.BaseAddress = new Uri("http://localhost:5214");
    }

    public async Task<PaginatedResult<ExerciseDto>> GetExercisesAsync(
        int page = 1, 
        int pageSize = 20, 
        string? name = null,
        string? difficultyId = null,
        List<string>? muscleGroupIds = null,
        List<string>? equipmentIds = null)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["page"] = page.ToString();
        query["pageSize"] = pageSize.ToString();
        
        if (!string.IsNullOrEmpty(name))
            query["name"] = name;
        
        if (!string.IsNullOrEmpty(difficultyId))
            query["difficultyId"] = difficultyId;
        
        if (muscleGroupIds?.Any() == true)
        {
            foreach (var id in muscleGroupIds)
                query.Add("muscleGroupIds", id);
        }
        
        if (equipmentIds?.Any() == true)
        {
            foreach (var id in equipmentIds)
                query.Add("equipmentIds", id);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/exercises?{query}");
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", await _authService.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaginatedResult<ExerciseDto>>();
    }

    public async Task<ExerciseDto> GetExerciseByIdAsync(string exerciseId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/exercises/{exerciseId}");
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", await _authService.GetTokenAsync());

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ExerciseDto>();
    }
}

// Models/PaginatedResult.cs
public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

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
  
  const data = await exerciseService.getExercises(params);
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

**Mobile Implementation with Infinite Scroll**
```javascript
// components/ExerciseList.js
import React, { useState, useCallback } from 'react';
import { FlatList, ActivityIndicator, Text } from 'react-native';
import { exerciseService } from '../services/exerciseService';

const ExerciseList = ({ filters }) => {
  const [exercises, setExercises] = useState([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadExercises = useCallback(async (pageNum, isRefresh = false) => {
    if (loading || (!hasMore && !isRefresh)) return;

    try {
      setLoading(true);
      const data = await exerciseService.getExercises({
        ...filters,
        page: pageNum,
        pageSize: 20
      });

      if (isRefresh) {
        setExercises(data.items);
        setPage(1);
        setHasMore(data.hasNextPage);
      } else {
        setExercises(prev => [...prev, ...data.items]);
        setHasMore(data.hasNextPage);
      }
    } catch (error) {
      console.error('Error loading exercises:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [filters, loading, hasMore]);

  const handleLoadMore = () => {
    if (hasMore && !loading) {
      const nextPage = page + 1;
      setPage(nextPage);
      loadExercises(nextPage);
    }
  };

  const handleRefresh = () => {
    setRefreshing(true);
    loadExercises(1, true);
  };

  return (
    <FlatList
      data={exercises}
      keyExtractor={item => item.id}
      renderItem={({ item }) => <ExerciseCard exercise={item} />}
      onEndReached={handleLoadMore}
      onEndReachedThreshold={0.5}
      refreshing={refreshing}
      onRefresh={handleRefresh}
      ListFooterComponent={() => 
        loading && exercises.length > 0 ? <ActivityIndicator /> : null
      }
      ListEmptyComponent={() => 
        !loading ? <Text>No exercises found</Text> : null
      }
    />
  );
};
```

**Web Implementation with Traditional Pagination**
```javascript
// components/ExercisePagination.js
import React from 'react';
import { useSearchParams } from 'react-router-dom';

const ExercisePagination = ({ data, filters }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  const { items, currentPage, totalPages, hasPreviousPage, hasNextPage } = data;

  const handlePageChange = (newPage) => {
    setSearchParams({ ...filters, page: newPage });
  };

  return (
    <div className="exercise-pagination">
      <div className="exercise-grid">
        {items.map(exercise => (
          <ExerciseCard key={exercise.id} exercise={exercise} />
        ))}
      </div>
      
      <div className="pagination-controls">
        <button 
          onClick={() => handlePageChange(currentPage - 1)}
          disabled={!hasPreviousPage}
          className="pagination-btn"
        >
          Previous
        </button>
        
        <div className="page-numbers">
          {[...Array(totalPages)].map((_, index) => {
            const pageNum = index + 1;
            const isNearCurrent = Math.abs(pageNum - currentPage) <= 2;
            const isEdge = pageNum === 1 || pageNum === totalPages;
            
            if (isNearCurrent || isEdge) {
              return (
                <button
                  key={pageNum}
                  onClick={() => handlePageChange(pageNum)}
                  className={`page-number ${pageNum === currentPage ? 'active' : ''}`}
                >
                  {pageNum}
                </button>
              );
            } else if (pageNum === currentPage - 3 || pageNum === currentPage + 3) {
              return <span key={pageNum}>...</span>;
            }
            return null;
          })}
        </div>
        
        <button 
          onClick={() => handlePageChange(currentPage + 1)}
          disabled={!hasNextPage}
          className="pagination-btn"
        >
          Next
        </button>
      </div>
      
      <div className="pagination-info">
        Showing {(currentPage - 1) * data.pageSize + 1} - 
        {Math.min(currentPage * data.pageSize, data.totalCount)} of {data.totalCount} exercises
      </div>
    </div>
  );
};
```

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
      const data = await exerciseService.getExercises({ page, pageSize: 50 });
      // Only sync active exercises
      const activeExercises = data.items.filter(ex => ex.isActive);
      allExercises.push(...activeExercises);
      hasMore = data.hasNextPage;
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

## Display Components Examples

### Exercise Card Component (Mobile)
```javascript
// components/ExerciseCard.js
import React from 'react';
import { View, Text, Image, TouchableOpacity, StyleSheet } from 'react-native';

const ExerciseCard = ({ exercise, onPress }) => {
  const primaryMuscles = exercise.muscleGroups
    .filter(mg => mg.role.value === 'Primary')
    .map(mg => mg.muscleGroup.name);

  return (
    <TouchableOpacity style={styles.card} onPress={() => onPress(exercise)}>
      <Image source={{ uri: exercise.imageUrl }} style={styles.thumbnail} />
      <View style={styles.content}>
        <Text style={styles.name}>{exercise.name}</Text>
        <Text style={styles.difficulty}>{exercise.difficulty.value}</Text>
        <Text style={styles.muscles}>{primaryMuscles.join(', ')}</Text>
        {exercise.equipment.length > 0 && (
          <View style={styles.equipment}>
            {exercise.equipment.map(eq => (
              <Text key={eq.id} style={styles.equipmentTag}>{eq.value}</Text>
            ))}
          </View>
        )}
      </View>
    </TouchableOpacity>
  );
};
```

### Exercise Types Filter (Web)
```javascript
// components/ExerciseFilters.js
import React from 'react';

const ExerciseFilters = ({ filters, onChange, availableTypes, availableEquipment }) => {
  return (
    <div className="exercise-filters">
      <div className="filter-group">
        <label>Exercise Types</label>
        <div className="checkbox-group">
          {availableTypes.map(type => (
            <label key={type.id} className="checkbox-label">
              <input
                type="checkbox"
                checked={filters.exerciseTypeIds?.includes(type.id)}
                onChange={(e) => {
                  const typeIds = e.target.checked
                    ? [...(filters.exerciseTypeIds || []), type.id]
                    : filters.exerciseTypeIds.filter(id => id !== type.id);
                  onChange({ ...filters, exerciseTypeIds: typeIds });
                }}
              />
              <span>{type.value}</span>
              <small>{type.description}</small>
            </label>
          ))}
        </div>
      </div>
      
      <div className="filter-group">
        <label>Equipment</label>
        <div className="checkbox-group">
          {availableEquipment.map(equipment => (
            <label key={equipment.id} className="checkbox-label">
              <input
                type="checkbox"
                checked={filters.equipmentIds?.includes(equipment.id)}
                onChange={(e) => {
                  const equipmentIds = e.target.checked
                    ? [...(filters.equipmentIds || []), equipment.id]
                    : filters.equipmentIds.filter(id => id !== equipment.id);
                  onChange({ ...filters, equipmentIds: equipmentIds });
                }}
              />
              <span>{equipment.value}</span>
            </label>
          ))}
        </div>
      </div>
    </div>
  );
};
```

## Next Steps

1. Implement exercise service layer with proper error handling
2. Create reusable components for displaying coach notes
3. Build exercise detail views with new field structure
4. Add offline support for mobile/desktop with active exercise filtering
5. Implement search with debouncing for exercise names
6. Add unit tests for pagination and data transformation
7. Create UI components for exercise type and equipment filters