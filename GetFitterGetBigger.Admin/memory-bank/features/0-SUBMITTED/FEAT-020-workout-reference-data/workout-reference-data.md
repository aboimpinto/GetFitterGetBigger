# FEAT-020: Workout Reference Data Implementation Plan

## Overview
Implementation plan for Personal Trainer reference data consultation components within the Admin application. This feature provides read-only access to workout objectives, categories, and execution protocols to enhance workout template creation decisions.

## Tasks

### 1. API Integration Layer (Est: 3-4h)
- [ ] Create WorkoutReferenceDataService for API communication
- [ ] Add TypeScript interfaces for all reference data models
- [ ] Implement caching strategy with 1-hour TTL
- [ ] Add comprehensive error handling for network failures
- [ ] Create mock data for development and testing

#### Technical Details
```typescript
// API Service Interface
interface IWorkoutReferenceDataService {
  getWorkoutObjectives(includeInactive?: boolean): Promise<WorkoutObjective[]>;
  getWorkoutObjectiveById(id: string): Promise<WorkoutObjective>;
  getWorkoutCategories(includeInactive?: boolean): Promise<WorkoutCategory[]>;
  getWorkoutCategoryById(id: string): Promise<WorkoutCategory>;
  getExecutionProtocols(includeInactive?: boolean): Promise<ExecutionProtocol[]>;
  getExecutionProtocolById(id: string): Promise<ExecutionProtocol>;
  getExecutionProtocolByCode(code: string): Promise<ExecutionProtocol>;
}

// Data Models
interface WorkoutObjective {
  workoutObjectiveId: string;
  value: string;
  description: string;
  displayOrder: number;
  isActive: boolean;
}

interface WorkoutCategory {
  workoutCategoryId: string;
  value: string;
  description: string;
  icon: string;
  color: string;
  primaryMuscleGroups: string;
  displayOrder: number;
  isActive: boolean;
}

interface ExecutionProtocol {
  executionProtocolId: string;
  code: string;
  value: string;
  description: string;
  timeBase: boolean;
  repBase: boolean;
  restPattern: string;
  intensityLevel: string;
  displayOrder: number;
  isActive: boolean;
}
```

### 2. Component Development (Est: 8-10h)

#### 2.1 WorkoutObjectivesComponent (Est: 3h)
- [ ] Create main component with list view layout
- [ ] Implement search functionality for objectives
- [ ] Add detail modal for comprehensive programming guidance
- [ ] Create responsive card design
- [ ] Add loading and error states

#### 2.2 WorkoutCategoriesComponent (Est: 3h)
- [ ] Create grid-based component layout
- [ ] Implement visual category cards with icons and colors
- [ ] Add category filtering by muscle groups
- [ ] Create detail modal with muscle group information
- [ ] Implement responsive grid behavior

#### 2.3 ExecutionProtocolsComponent (Est: 3h)
- [ ] Create table-based component layout
- [ ] Implement sortable columns functionality
- [ ] Add filter tabs for intensity/pattern grouping
- [ ] Create comprehensive detail modal
- [ ] Add protocol code highlighting

#### 2.4 Shared Components (Est: 1h)
- [ ] Create reusable DetailModal component
- [ ] Create SearchBox component for consistent search UI
- [ ] Create FilterBar component for category filtering
- [ ] Implement consistent loading skeleton components

### 3. State Management (Est: 2-3h)
- [ ] Implement Redux/Context for reference data caching
- [ ] Create actions for data fetching and caching
- [ ] Add reducers for search and filter state
- [ ] Connect components to centralized state
- [ ] Implement optimistic UI updates

#### State Structure
```typescript
interface ReferenceDataState {
  workoutObjectives: {
    data: WorkoutObjective[];
    loading: boolean;
    error: string | null;
    lastFetched: Date | null;
  };
  workoutCategories: {
    data: WorkoutCategory[];
    loading: boolean;
    error: string | null;
    lastFetched: Date | null;
  };
  executionProtocols: {
    data: ExecutionProtocol[];
    loading: boolean;
    error: string | null;
    lastFetched: Date | null;
  };
  ui: {
    searchQueries: Record<string, string>;
    activeFilters: Record<string, string[]>;
    openModals: Record<string, boolean>;
  };
}
```

### 4. UI Implementation (Est: 4-5h)
- [ ] Integrate components into existing Reference Tables menu
- [ ] Implement responsive design for tablet/desktop
- [ ] Add proper loading states with skeleton screens
- [ ] Create error boundaries for graceful error handling
- [ ] Implement WCAG 2.1 AA accessibility features
- [ ] Add keyboard navigation support

#### Navigation Integration
```typescript
// Extend existing reference tables menu
const ReferenceTablesMenu = {
  items: [
    { path: '/reference-tables/exercise-weight-types', title: 'Exercise Weight Types' },
    { path: '/reference-tables/difficulty-levels', title: 'Difficulty Levels' },
    { path: '/reference-tables/body-parts', title: 'Body Parts' },
    { path: '/reference-tables/workout-objectives', title: 'Workout Objectives' }, // New
    { path: '/reference-tables/workout-categories', title: 'Workout Categories' }, // New
    { path: '/reference-tables/execution-protocols', title: 'Execution Protocols' }, // New
  ]
};
```

### 5. Testing (Est: 5-6h)

#### 5.1 Unit Tests (Est: 3h)
- [ ] Test API service methods with mocked responses
- [ ] Test component rendering with various data states
- [ ] Test search and filter functionality
- [ ] Test modal open/close behavior
- [ ] Test error handling scenarios

#### 5.2 Integration Tests (Est: 2h)
- [ ] Test complete user workflows (browse → search → view details)
- [ ] Test API integration with real endpoints
- [ ] Test caching behavior and cache invalidation
- [ ] Test responsive design across device sizes

#### 5.3 Accessibility Tests (Est: 1h)
- [ ] Automated accessibility testing with jest-axe
- [ ] Keyboard navigation testing
- [ ] Screen reader compatibility testing
- [ ] Color contrast validation

#### Testing Examples
```typescript
// Component Test Example
describe('WorkoutObjectivesComponent', () => {
  it('displays workout objectives in card format', async () => {
    const mockObjectives = [
      { workoutObjectiveId: '1', value: 'Strength', description: 'Test description', displayOrder: 1, isActive: true }
    ];
    
    render(<WorkoutObjectivesComponent />, {
      wrapper: ({ children }) => (
        <Provider store={mockStore({ workoutObjectives: { data: mockObjectives, loading: false, error: null } })}>
          {children}
        </Provider>
      )
    });
    
    expect(screen.getByText('Strength')).toBeInTheDocument();
    expect(screen.getByText('Test description')).toBeInTheDocument();
  });
  
  it('opens detail modal when info icon is clicked', async () => {
    // Test modal functionality
  });
  
  it('filters objectives based on search query', async () => {
    // Test search functionality
  });
});

// API Service Test Example
describe('WorkoutReferenceDataService', () => {
  it('fetches workout objectives successfully', async () => {
    const mockResponse = [{ workoutObjectiveId: '1', value: 'Strength' }];
    jest.spyOn(global, 'fetch').mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse,
    });
    
    const service = new WorkoutReferenceDataService();
    const result = await service.getWorkoutObjectives();
    
    expect(result).toEqual(mockResponse);
  });
  
  it('handles network errors gracefully', async () => {
    // Test error handling
  });
});
```

## Technical Decisions

### Component Technology Stack
- **Framework**: React with TypeScript for type safety
- **State Management**: Redux Toolkit for predictable state management
- **UI Library**: Existing Admin UI component library
- **Testing**: Jest + React Testing Library + jest-axe
- **Styling**: Existing CSS modules/styled-components pattern

### Caching Strategy
- **Client-side**: Redux with 1-hour TTL matching API cache
- **Storage**: Memory-based cache (no persistent storage needed)
- **Invalidation**: Time-based expiration with manual refresh option
- **Performance**: Aggressive caching for reference data (changes infrequently)

### Error Handling Approach
- **Network Errors**: Retry mechanism with exponential backoff
- **UI Errors**: Error boundaries with fallback components
- **User Communication**: Clear error messages with actionable guidance
- **Logging**: Error tracking for monitoring and debugging

### Accessibility Implementation
- **Keyboard Navigation**: Full tab navigation with proper focus management
- **Screen Readers**: Semantic HTML with ARIA labels and roles
- **Visual**: High contrast support and focus indicators
- **Standards**: WCAG 2.1 AA compliance verification

## Open Questions

### For Product Team
1. Should we add export functionality for reference data?
2. Do we need bookmarking/favorites for frequently accessed information?
3. Should we show usage statistics (which templates use specific objectives)?
4. Is offline access a requirement for gym environments?

### For Design Team
1. Should category icons be customizable or fixed set?
2. Do we need dark mode support for reference tables?
3. Should we add animations/transitions for better UX?
4. Any specific branding requirements for reference data display?

### For API Team
1. Are there plans for real-time updates to reference data?
2. Should we implement pagination for large reference datasets?
3. Do we need advanced search capabilities (full-text search)?
4. Are there any rate limiting considerations for reference endpoints?

## Success Metrics

### User Experience Metrics
- **Task Completion Rate**: 95%+ for reference data lookup tasks
- **Time to Information**: < 30 seconds to find specific reference data
- **User Satisfaction**: 4.5/5 rating for reference data utility
- **Error Recovery**: < 10 seconds to resolve common issues

### Technical Performance Metrics
- **Load Time**: < 2 seconds for initial component load
- **Search Response**: < 500ms for search/filter operations
- **Cache Hit Rate**: 90%+ for reference data requests
- **Bundle Size**: < 50KB additional to existing bundle

### Business Impact Metrics
- **Feature Adoption**: 80%+ of trainers use reference data within first month
- **Template Quality**: Improved consistency in workout programming
- **Support Reduction**: Decreased questions about training objectives/protocols
- **Trainer Productivity**: Faster template creation workflows

## Implementation Timeline

### Phase 1: Foundation (Week 1)
- API integration layer
- Basic component structure
- State management setup

### Phase 2: Core Features (Week 2)
- Component implementation
- Search and filter functionality
- Modal dialogs

### Phase 3: Polish & Testing (Week 3)
- UI refinements
- Comprehensive testing
- Accessibility compliance
- Performance optimization

### Phase 4: Integration & Launch (Week 4)
- Menu integration
- User acceptance testing
- Production deployment
- Documentation and training