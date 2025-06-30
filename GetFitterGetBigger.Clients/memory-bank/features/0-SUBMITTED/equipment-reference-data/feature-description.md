# Equipment CRUD API Integration

This document provides the API specifications for Equipment operations that need to be integrated into client applications (Android, iOS, Web, Desktop).

---
feature: equipment-reference-data
status: 0-SUBMITTED
created: 2025-06-30
api_version: 1.0
---

## Overview

Equipment is a reference table that provides the list of available equipment options for exercises. Client applications primarily need read access to display equipment information when viewing exercises and workouts.

### Business Context
- Equipment items are associated with exercises
- Clients can view equipment requirements for exercises
- Write operations are admin-only (not available to regular clients)
- Equipment data should be cached locally for performance

## Authentication Requirements

- **Read Operations**: Any authenticated user (including Free-Tier)
- **Write Operations**: Not available for client applications

## Endpoints

### Get All Equipment
**URL**: `GET /api/ReferenceTables/Equipment`
**Headers**: `Authorization: Bearer {token}`
**Method**: GET
**Response**: 200 OK

```json
[
  {
    "id": "equipment-123e4567-e89b-12d3-a456-426614174000",
    "name": "Barbell",
    "isActive": true,
    "createdAt": "2025-01-30T12:00:00Z",
    "updatedAt": null
  },
  {
    "id": "equipment-456e7890-e89b-12d3-a456-426614174000",
    "name": "Dumbbell",
    "isActive": true,
    "createdAt": "2025-01-30T12:00:00Z",
    "updatedAt": "2025-01-30T14:00:00Z"
  }
]
```

**Notes**:
- Only returns active equipment (isActive = true)
- Results are not paginated
- No query parameters supported

## Data Models

### EquipmentDto
```typescript
interface EquipmentDto {
  id: string;           // Format: equipment-{guid}
  name: string;
  isActive: boolean;    // Always true for client responses
  createdAt: string;    // ISO 8601 datetime
  updatedAt: string | null;
}
```

## Implementation Guidelines

### Caching Strategy
1. Cache equipment list on first load
2. Refresh cache on app startup
3. Consider refresh after 24 hours
4. Store in local database for offline access

### UI Integration
1. Display equipment requirements in exercise details
2. Show equipment filters in exercise lists
3. Group exercises by equipment in workout views
4. Handle missing equipment gracefully

### Platform-Specific Considerations

#### Mobile (Android/iOS)
- Store in local SQLite/Core Data
- Implement pull-to-refresh
- Show offline indicator when cached data is used

#### Web
- Use browser localStorage or IndexedDB
- Implement service worker caching
- Show last update timestamp

#### Desktop
- Store in application database
- Sync on application start
- Provide manual refresh option

### Error Handling
- Handle network failures gracefully
- Fall back to cached data when offline
- Show appropriate error messages
- Log errors for debugging

## Usage Examples

### Fetching Equipment List
```javascript
// Example implementation
async function fetchEquipment() {
  try {
    const response = await fetch(`${API_BASE_URL}/api/ReferenceTables/Equipment`, {
      headers: {
        'Authorization': `Bearer ${authToken}`
      }
    });
    
    if (response.ok) {
      const equipment = await response.json();
      // Cache the data
      await cacheEquipment(equipment);
      return equipment;
    }
    
    // Fall back to cached data on error
    return getCachedEquipment();
  } catch (error) {
    console.error('Failed to fetch equipment:', error);
    return getCachedEquipment();
  }
}
```

### Displaying Equipment in Exercise
```javascript
// Example usage in exercise component
function ExerciseDetail({ exercise }) {
  const equipment = exercise.equipment || [];
  
  return (
    <div>
      <h3>{exercise.name}</h3>
      {equipment.length > 0 && (
        <div>
          <h4>Equipment Required:</h4>
          <ul>
            {equipment.map(item => (
              <li key={item.id}>{item.name}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
```

## Performance Considerations
- Equipment list is relatively small and stable
- Cache aggressively to reduce API calls
- Consider bundling with other reference data
- Use ETags or Last-Modified headers if available

## Dependencies
- Required for exercise display functionality
- Should be loaded before exercise lists
- Part of initial app data synchronization