# Feature: Muscle Groups Reference Data Integration

## Feature ID
`muscle-groups-reference-data`

## Description
Integrate muscle groups reference data fetching and display across all client applications. This feature provides read-only access to muscle groups with their associated body parts, supporting exercise selection and workout planning functionality.

## API Endpoints

### Get All Muscle Groups
- **Endpoint**: `GET /api/ReferenceTables/MuscleGroups`
- **Authorization**: Any authenticated user
- **Query Parameters**:
  - `pageNumber`: number (default: 1)
  - `pageSize`: number (default: 10, max: 100)
  - `includeInactive`: boolean (default: false)
- **Response**: Paginated list of `MuscleGroupDto`

### Get Muscle Groups by Body Part
- **Endpoint**: `GET /api/ReferenceTables/MuscleGroups/ByBodyPart/{bodyPartId}`
- **Authorization**: Any authenticated user
- **Response**: Array of `MuscleGroupDto[]`

## Data Models

### MuscleGroupDto
```typescript
interface MuscleGroupDto {
  id: string;           // Format: "musclegroup-{guid}"
  name: string;         // Unique muscle group name
  bodyPartId: string;   // Associated body part ID
  bodyPartName?: string;// Associated body part name
  isActive: boolean;    // Soft delete status
  createdAt: string;    // ISO 8601 datetime
  updatedAt?: string;   // ISO 8601 datetime
}
```

### Paginated Response
```typescript
interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
```

## Implementation Requirements

### Data Fetching
1. Implement service methods to fetch muscle groups
2. Handle pagination for large datasets
3. Support filtering by body part
4. Cache responses for offline access

### State Management
1. Store muscle groups in app state/store
2. Implement selectors for:
   - All muscle groups
   - Muscle groups by body part
   - Active muscle groups only
   - Search/filter functionality

### UI Components

#### Muscle Group Selector
```typescript
interface MuscleGroupSelectorProps {
  selectedIds?: string[];
  onSelectionChange: (ids: string[]) => void;
  multiple?: boolean;
  groupByBodyPart?: boolean;
  showInactive?: boolean;
}
```

#### Muscle Group Display
```typescript
interface MuscleGroupDisplayProps {
  muscleGroup: MuscleGroupDto;
  showBodyPart?: boolean;
  compact?: boolean;
}
```

## Platform-Specific Implementation

### Mobile Apps (React Native)

#### Service Implementation
```typescript
class MuscleGroupService {
  private cache: Map<string, MuscleGroupDto[]> = new Map();
  
  async fetchMuscleGroups(includeInactive = false): Promise<MuscleGroupDto[]> {
    const cacheKey = `muscle-groups-${includeInactive}`;
    
    // Check cache first
    if (this.cache.has(cacheKey)) {
      return this.cache.get(cacheKey)!;
    }
    
    const response = await api.get('/api/ReferenceTables/MuscleGroups', {
      params: { includeInactive, pageSize: 100 }
    });
    
    const muscleGroups = response.data.items;
    this.cache.set(cacheKey, muscleGroups);
    
    // Persist to AsyncStorage
    await AsyncStorage.setItem(cacheKey, JSON.stringify(muscleGroups));
    
    return muscleGroups;
  }
  
  async fetchByBodyPart(bodyPartId: string): Promise<MuscleGroupDto[]> {
    const response = await api.get(
      `/api/ReferenceTables/MuscleGroups/ByBodyPart/${bodyPartId}`
    );
    return response.data;
  }
}
```

#### Component Example
```tsx
const MuscleGroupPicker: React.FC<MuscleGroupPickerProps> = ({
  selectedIds = [],
  onSelectionChange,
  groupByBodyPart = true
}) => {
  const [muscleGroups, setMuscleGroups] = useState<MuscleGroupDto[]>([]);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    loadMuscleGroups();
  }, []);
  
  const loadMuscleGroups = async () => {
    try {
      const data = await muscleGroupService.fetchMuscleGroups();
      setMuscleGroups(data.filter(mg => mg.isActive));
    } finally {
      setLoading(false);
    }
  };
  
  const groupedData = useMemo(() => {
    if (!groupByBodyPart) return { 'All': muscleGroups };
    
    return muscleGroups.reduce((acc, mg) => {
      const key = mg.bodyPartName || 'Other';
      if (!acc[key]) acc[key] = [];
      acc[key].push(mg);
      return acc;
    }, {} as Record<string, MuscleGroupDto[]>);
  }, [muscleGroups, groupByBodyPart]);
  
  // Render grouped sections...
};
```

### Web App (React)

#### State Management (Redux Toolkit)
```typescript
const muscleGroupSlice = createSlice({
  name: 'muscleGroups',
  initialState: {
    items: [] as MuscleGroupDto[],
    loading: false,
    error: null as string | null,
    lastFetch: null as number | null
  },
  reducers: {
    // ... reducers
  }
});

// Thunk for fetching
export const fetchMuscleGroups = createAsyncThunk(
  'muscleGroups/fetch',
  async (includeInactive = false) => {
    const response = await api.get('/api/ReferenceTables/MuscleGroups', {
      params: { includeInactive, pageSize: 100 }
    });
    return response.data.items;
  }
);
```

#### Component Example
```tsx
const MuscleGroupSelect: React.FC<MuscleGroupSelectProps> = ({
  value,
  onChange,
  multiple = false
}) => {
  const dispatch = useAppDispatch();
  const { items: muscleGroups, loading } = useAppSelector(
    state => state.muscleGroups
  );
  
  useEffect(() => {
    if (muscleGroups.length === 0) {
      dispatch(fetchMuscleGroups());
    }
  }, [dispatch, muscleGroups.length]);
  
  const groupedOptions = useMemo(() => {
    const grouped = muscleGroups.reduce((acc, mg) => {
      if (!mg.isActive) return acc;
      
      const group = acc.find(g => g.label === mg.bodyPartName);
      if (group) {
        group.options.push({
          value: mg.id,
          label: mg.name
        });
      } else {
        acc.push({
          label: mg.bodyPartName || 'Other',
          options: [{
            value: mg.id,
            label: mg.name
          }]
        });
      }
      return acc;
    }, [] as Array<{ label: string; options: Array<{ value: string; label: string }> }>);
    
    return grouped;
  }, [muscleGroups]);
  
  return (
    <Select
      isMulti={multiple}
      isLoading={loading}
      options={groupedOptions}
      value={value}
      onChange={onChange}
      placeholder="Select muscle groups..."
    />
  );
};
```

### Desktop App (Electron + React)

#### Persistent Storage
```typescript
const store = new Store({
  name: 'muscle-groups-cache'
});

class MuscleGroupRepository {
  private readonly CACHE_KEY = 'muscle-groups';
  private readonly CACHE_DURATION = 24 * 60 * 60 * 1000; // 24 hours
  
  async getMuscleGroups(forceRefresh = false): Promise<MuscleGroupDto[]> {
    const cached = store.get(this.CACHE_KEY) as {
      data: MuscleGroupDto[];
      timestamp: number;
    } | undefined;
    
    const now = Date.now();
    const isExpired = !cached || (now - cached.timestamp) > this.CACHE_DURATION;
    
    if (!forceRefresh && cached && !isExpired) {
      return cached.data;
    }
    
    const response = await api.get('/api/ReferenceTables/MuscleGroups', {
      params: { pageSize: 100 }
    });
    
    const data = response.data.items;
    store.set(this.CACHE_KEY, {
      data,
      timestamp: now
    });
    
    return data;
  }
}
```

## Error Handling

### Common Errors
1. **Network Error**: Show offline message, use cached data
2. **401 Unauthorized**: Redirect to login
3. **500 Server Error**: Show retry option

### Error Messages
```typescript
const ERROR_MESSAGES = {
  FETCH_FAILED: 'Failed to load muscle groups. Please try again.',
  OFFLINE: 'You are offline. Showing cached muscle groups.',
  UNAUTHORIZED: 'Please log in to view muscle groups.'
};
```

## Testing Requirements

### Unit Tests
1. Service methods for data fetching
2. State management logic
3. Component rendering with different props
4. Error handling scenarios

### Integration Tests
1. API communication
2. Cache persistence
3. Offline functionality
4. Body part filtering

## Accessibility

1. **Keyboard Navigation**: Full support in selectors
2. **Screen Readers**: Proper ARIA labels
3. **High Contrast**: Visible in all themes
4. **Focus Management**: Clear focus indicators

## Performance Considerations

1. **Initial Load**: Fetch only active muscle groups
2. **Caching**: 24-hour cache with background refresh
3. **Pagination**: Load all in one request (typically < 50 items)
4. **Memoization**: Prevent unnecessary re-renders

## Migration Guide

### Updating from ReferenceDataDto to MuscleGroupDto

```typescript
// Old code
const muscleGroupName = muscleGroup.value;
const description = muscleGroup.description;

// New code
const muscleGroupName = muscleGroup.name;
const bodyPart = muscleGroup.bodyPartName;
// Note: description is no longer available
```

### Update TypeScript Interfaces
```typescript
// Old
interface Exercise {
  primaryMuscleGroups: ReferenceDataDto[];
  secondaryMuscleGroups: ReferenceDataDto[];
}

// New
interface Exercise {
  primaryMuscleGroups: MuscleGroupDto[];
  secondaryMuscleGroups: MuscleGroupDto[];
}
```

## Dependencies
- Body Parts reference data (for grouping)
- Authentication (for API access)
- Network connectivity (with offline fallback)