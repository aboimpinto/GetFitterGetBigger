# Muscle Groups Reference Data

## Overview
Fetch and display muscle groups reference data for use in exercises and workout planning. This feature provides read-only access to muscle groups with their associated body parts.

## API Endpoint
`GET /api/ReferenceTables/MuscleGroups`

## Data Model Changes
### Previous Structure (ReferenceDataDto)
```typescript
interface ReferenceDataDto {
  id: string;           // Format: "musclegroup-{guid}"
  value: string;        // e.g., "Quadriceps"
  description: string;  // e.g., "Front thigh muscles"
}
```

### New Structure (MuscleGroupDto)
```typescript
interface MuscleGroupDto {
  id: string;           // Format: "musclegroup-{guid}"
  name: string;         // e.g., "Quadriceps" (previously "value")
  bodyPartId: string;   // Format: "bodypart-{guid}"
  bodyPartName?: string;// e.g., "Legs"
  isActive: boolean;    // Filter inactive items
  createdAt: string;    // ISO 8601 datetime
  updatedAt?: string;   // ISO 8601 datetime
}
```

## Breaking Changes
1. **Field Rename**: `value` → `name`
2. **Field Removed**: `description` no longer exists
3. **New Fields**: `bodyPartId`, `bodyPartName`, `isActive`, `createdAt`, `updatedAt`
4. **Response Type**: Changed from `ReferenceDataDto[]` to `MuscleGroupDto[]`

## Implementation Priority
- **Mobile Apps**: HIGH - Required for exercise selection and filtering
- **Web App**: HIGH - Required for workout planning interface
- **Desktop App**: MEDIUM - Required for comprehensive exercise library

## Caching Strategy
- Cache duration: 24 hours
- Invalidate on app launch if older than 7 days
- Store in local database/storage for offline access

## UI/UX Guidelines
1. Display muscle groups grouped by body part
2. Show muscle group name prominently
3. Include body part as secondary information
4. Filter out inactive muscle groups by default
5. Support search by muscle group name

## Platform-Specific Notes

### Mobile (React Native)
- Use AsyncStorage for caching
- Implement pull-to-refresh for manual updates
- Group by body part in selection lists

### Web (React)
- Use localStorage or IndexedDB for caching
- Implement typeahead search
- Display in categorized dropdown menus

### Desktop (Electron + React)
- Use electron-store for persistent caching
- Support keyboard navigation
- Display in expandable tree view by body part

## Related Features
- Exercise Management (muscle groups are primary/secondary targets)
- Workout Planning (filter exercises by muscle groups)
- Body Part Filtering (muscle groups are children of body parts)