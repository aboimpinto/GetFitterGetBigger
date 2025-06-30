# equipment-reference-data

Equipment Reference Data Integration for Client Applications

## Overview
Integrate equipment reference data into client applications (Android, iOS, Web, Desktop) to display equipment requirements for exercises and enable equipment-based filtering.

## User Stories
As a client app user, I want to:
- See what equipment is required for each exercise
- Filter exercises by available equipment
- View equipment requirements in workout details
- Access equipment info offline

## Acceptance Criteria
- [ ] Equipment data is fetched and cached locally
- [ ] Equipment names display in exercise details
- [ ] Equipment filter available in exercise lists
- [ ] Offline access to equipment data
- [ ] Graceful handling of missing equipment data
- [ ] Equipment info refreshes appropriately

## Technical Requirements
- [ ] Integrate GET `/api/ReferenceTables/Equipment` endpoint
- [ ] Implement local caching mechanism
- [ ] Add equipment to data models
- [ ] Update UI components to display equipment
- [ ] Handle offline scenarios
- [ ] Implement data synchronization strategy

## Platform-Specific Requirements

### Mobile (Android/iOS)
- [ ] Store in local database (SQLite/Core Data)
- [ ] Pull-to-refresh capability
- [ ] Offline indicator when using cached data
- [ ] Background sync if applicable

### Web
- [ ] Use appropriate browser storage (localStorage/IndexedDB)
- [ ] Service worker caching for offline
- [ ] Show data freshness indicator

### Desktop
- [ ] Integrate with app's database layer
- [ ] Sync on application startup
- [ ] Manual refresh option in settings

## API Integration
- Endpoint: GET `/api/ReferenceTables/Equipment`
- Response: Array of EquipmentDto objects
- Authentication: Any authenticated user
- Caching: Recommended 24-hour cache

## UI/UX Updates
1. Exercise detail views
   - Equipment section showing required items
2. Exercise list views
   - Equipment filter dropdown/chips
3. Workout views
   - Equipment summary for entire workout
4. Settings/Info
   - Last sync timestamp
   - Manual refresh option

## Data Model Updates
```typescript
interface Equipment {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}
```

## Estimated Effort
Per platform:
- Implementation: 6-8 hours
- Testing: 2-3 hours
- Total: 8-11 hours per platform

## Dependencies
- API documentation in feature-description.md
- Existing exercise and workout features

## Notes
- Equipment list is relatively stable, aggressive caching recommended
- Consider bundling with other reference data fetches
- Future enhancement: equipment icons/images