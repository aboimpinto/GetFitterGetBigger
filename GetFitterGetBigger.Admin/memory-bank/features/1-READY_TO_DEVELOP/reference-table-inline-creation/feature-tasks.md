# Reference Table Inline Creation - Implementation Tasks

## Prerequisites

### API Verification
- [x] MuscleGroup CRUD endpoints are complete and in production (merged Jan 29, 2025)
  - POST, PUT, DELETE endpoints available at /api/ReferenceTables/MuscleGroups
  - DTOs implemented: CreateMuscleGroupDto, UpdateMuscleGroupDto, MuscleGroupDto
  - All tests passing and feature deployed
- [ ] [TODO] Verify Equipment CRUD endpoints availability
- [ ] [TODO] Verify MetricTypes CRUD endpoints availability
- [ ] [TODO] Verify MovementPatterns CRUD endpoints availability

## Core Implementation Tasks

### 1. Component Development
- [ ] [TODO] Create InlineCreatableSelect base component
  - Props: items, onSelect, allowCreate, onCreateNew, placeholder
  - Support for both controlled and uncontrolled modes
  - Proper TypeScript typing
  - Unit tests with 90% coverage

- [ ] [TODO] Create ReferenceTableModal component
  - Generic modal for creating reference data
  - Field configuration support
  - Validation framework integration
  - Loading and error states
  - Success animations

### 2. Reference Table Configuration
- [ ] [TODO] Create reference table configuration system
  ```typescript
  interface ReferenceTableConfig {
    tableName: string;
    displayName: string;
    allowInlineCreate: boolean;
    cacheStrategy: 'static' | 'dynamic';
    createFields: FieldConfig[];
    apiEndpoint: string;
  }
  ```

- [ ] [TODO] Define configurations for all reference tables
  - Equipment (CRUD enabled)
  - MuscleGroups (CRUD enabled)
  - MetricTypes (CRUD enabled)
  - MovementPatterns (CRUD enabled)
  - DifficultyLevels (Read-only)
  - BodyParts (Read-only)
  - KineticChainTypes (Read-only)
  - MuscleRoles (Read-only)

### 3. API Integration
- [ ] [TODO] Create reference table API service
  - Generic CRUD operations
  - Type-safe request/response handling
  - Error transformation
  - Retry logic for network failures

- [ ] [TODO] Implement cache management
  - Cache invalidation on create
  - Optimistic updates
  - Background refresh
  - Offline queue for failed creates

### 4. Exercise Form Integration
- [ ] [TODO] Replace MuscleGroup select with InlineCreatableSelect
  - Maintain existing validation
  - Handle multiple selection
  - Show muscle role assignment

- [ ] [TODO] Replace Equipment select with InlineCreatableSelect
  - Single selection mode
  - Required field validation
  - Auto-focus on modal open

- [ ] [TODO] Add keyboard shortcuts
  - Ctrl+N to open create modal
  - Escape to cancel
  - Enter to submit

### 5. UI/UX Enhancements
- [ ] [TODO] Add visual indicators for CRUD tables
  - Plus icon with tooltip
  - Different border color
  - Help text on hover

- [ ] [TODO] Implement loading states
  - Skeleton loader during fetch
  - Progress indicator during create
  - Success toast notifications

- [ ] [TODO] Add duplicate detection
  - Check for similar names
  - Suggest existing items
  - Fuzzy matching algorithm

### 6. Testing
- [ ] [TODO] Write unit tests for all components
  - InlineCreatableSelect component
  - ReferenceTableModal component
  - API service methods
  - Cache management logic

- [ ] [TODO] Write integration tests
  - Complete creation flow
  - Error handling scenarios
  - Cache invalidation verification
  - Multi-user scenarios

- [ ] [TODO] Create E2E tests
  - Exercise creation with new reference data
  - Validation error handling
  - Network failure recovery
  - Mobile responsiveness

### 7. Documentation
- [ ] [TODO] Create user documentation
  - How to use inline creation
  - Supported reference tables
  - Troubleshooting guide

- [ ] [TODO] Create developer documentation
  - Component API reference
  - Configuration guide
  - Extension points

### 8. Performance Optimization
- [ ] [TODO] Implement lazy loading for reference data
  - Load on demand
  - Preload based on user patterns
  - Memory management

- [ ] [TODO] Optimize modal animations
  - GPU acceleration
  - Reduced motion support
  - Mobile performance

## Testing Checklist

### Unit Tests
- [ ] InlineCreatableSelect renders correctly
- [ ] Modal opens/closes properly
- [ ] Validation works as expected
- [ ] API calls are made correctly
- [ ] Cache is invalidated on success

### Integration Tests
- [ ] Create new equipment and use immediately
- [ ] Create muscle group with special characters
- [ ] Handle duplicate names gracefully
- [ ] Network timeout recovery
- [ ] Multi-tab synchronization

### User Acceptance Tests
- [ ] PT can create equipment while adding exercise
- [ ] Newly created items appear in all relevant dropdowns
- [ ] Form data is preserved during creation
- [ ] Error messages are clear and helpful
- [ ] Mobile experience is smooth

## Definition of Done

- [ ] All tasks completed and marked with commit hashes
- [ ] All tests passing (unit, integration, E2E)
- [ ] Code review completed
- [ ] Documentation updated
- [ ] No console errors or warnings
- [ ] Performance metrics met
- [ ] Accessibility standards met (WCAG 2.1 AA)
- [ ] User acceptance confirmed