# FEAT-019: Exercise Weight Type Implementation Plan

## Overview
Implementation plan for Exercise Weight Type classification system in the Admin application. This feature enhances exercise management with standardized weight type classification and dynamic validation.

## Tasks

### 1. API Integration Layer
- [ ] Create ExerciseWeightTypeService for API communication
  - GET /api/exercise-weight-types endpoint integration
  - GET /api/exercise-weight-types/{id} endpoint integration
  - Error handling for network failures and invalid responses
  - Caching strategy for weight types reference data
- [ ] Add TypeScript interfaces for ExerciseWeightType
  - ExerciseWeightType interface with id, code, name, description properties
  - ExerciseWeightTypeDto for API responses
  - WeightValidationRule interface for client-side validation
- [ ] Implement client-side validation service
  - Weight validation logic for each weight type code
  - Real-time validation functions
  - Error message generation

### 2. Core Component Development
- [ ] Create ExerciseWeightTypeSelector component
  - Dropdown with all active weight types
  - Tooltip descriptions for each option
  - Required field validation
  - Disabled state support
- [ ] Create WeightInputField component
  - Dynamic visibility based on weight type
  - Real-time validation with error messaging
  - Unit conversion support (kg/lbs)
  - Contextual placeholder text
- [ ] Create ExerciseWeightTypeBadge component
  - Color-coded badges for each weight type
  - Icon support with accessible labels
  - Size variants (small, medium, large)
  - Tooltip with weight type description

### 3. Enhanced Exercise Management
- [ ] Update ExerciseListView component
  - Add weight type badge column
  - Filter by weight type functionality
  - Sort by weight type option
  - Bulk selection for weight type updates
- [ ] Update ExerciseFormView component
  - Integrate ExerciseWeightTypeSelector
  - Add WeightInputField with dynamic behavior
  - Form validation including weight type requirements
  - Warning messages for weight type changes
- [ ] Create ExerciseBulkUpdateView component
  - Multi-select exercise interface
  - Weight type assignment for selected exercises
  - Preview changes before submission
  - Progress indication for bulk operations

### 4. Workout Integration Components
- [ ] Create WorkoutExerciseWeightInput component
  - Automatic weight type detection from exercise
  - Contextual help messages
  - Weight validation based on exercise type
  - Integration with workout template forms
- [ ] Update WorkoutTemplateForm component
  - Integrate WorkoutExerciseWeightInput
  - Weight type awareness in exercise selection
  - Validation summary for weight assignments

### 5. Reference Data Management
- [ ] Create ExerciseWeightTypeReferencePanel component
  - Read-only display of all weight types
  - Detailed descriptions and validation rules
  - Visual examples for each type
  - Common exercises categorized by type
- [ ] Create WeightTypeValidationRulesDisplay component
  - Table format showing validation rules
  - Visual indicators for allowed/prohibited values
  - Interactive examples demonstrating validation

### 6. State Management
- [ ] Set up Redux/Context for ExerciseWeightType
  - Actions: loadWeightTypes, selectWeightType, validateWeight
  - Reducers: weightTypes, selectedType, validation state
  - Selectors: getWeightTypes, getWeightTypeByCode
- [ ] Implement caching strategy
  - Local storage for weight types reference data
  - Cache invalidation strategy
  - Background refresh mechanism
- [ ] Connect components to state
  - Form components connected to validation state
  - List components connected to filter state
  - Loading states throughout UI

### 7. UI Implementation & Styling
- [ ] Implement responsive design patterns
  - Desktop: Full-width forms with side-by-side labels
  - Tablet: Condensed layout with essential information
  - Mobile: Single-column card-based interface
- [ ] Add loading states and error boundaries
  - Skeleton loading for weight type dropdown
  - Error fallback for failed API calls
  - Retry mechanisms for network failures
- [ ] Create accessibility features
  - ARIA labels for weight type badges
  - Keyboard navigation for dropdowns
  - Screen reader announcements for validation changes

### 8. Testing Implementation
- [ ] Unit tests for components
  - ExerciseWeightTypeSelector with all weight types
  - WeightInputField validation logic
  - ExerciseWeightTypeBadge rendering variants
  - Form integration and validation flows
- [ ] Integration tests for API services
  - ExerciseWeightTypeService API calls
  - Error handling scenarios
  - Caching behavior verification
- [ ] E2E tests for workflows
  - Create exercise with weight type selection
  - Edit exercise and change weight type
  - Bulk update exercise weight types
  - Workout template creation with weight validation

### 9. Migration Support
- [ ] Create ExerciseMigrationPanel component (Admin only)
  - Preview existing exercises without weight types
  - Suggest weight type assignments based on exercise characteristics
  - Batch migration interface with progress tracking
- [ ] Implement migration validation
  - Compatibility checks for existing workouts
  - Warning system for exercises used in active templates
  - Rollback capability for migration errors

## Technical Decisions

### Component Architecture
- **Blazor Server Components**: Leveraging existing Admin architecture
- **Component Communication**: Event callbacks for parent-child communication
- **State Management**: Centralized state for weight types with component-level state for forms

### Validation Strategy
- **Client-Side First**: Immediate feedback with TypeScript validation
- **Server Confirmation**: Final validation on form submission
- **Progressive Enhancement**: Graceful degradation if JavaScript fails

### Caching Implementation
- **Reference Data**: Weight types cached on application start
- **Cache Duration**: Session-based with background refresh
- **Cache Invalidation**: Manual refresh option in admin panel

### Error Handling
- **Network Errors**: Retry with exponential backoff
- **Validation Errors**: Inline messaging with field highlighting
- **Form Errors**: Summary display with focus management

## API Integration Details

### Endpoint Documentation
```csharp
// GET /api/exercise-weight-types
public class ExerciseWeightTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }  // BODYWEIGHT_ONLY, WEIGHT_REQUIRED, etc.
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
```

### Validation Rules Implementation
```csharp
public static class WeightValidation
{
    public static ValidationResult ValidateWeight(string weightTypeCode, decimal? weight)
    {
        return weightTypeCode switch
        {
            "BODYWEIGHT_ONLY" => weight is null or 0 ? Valid : Invalid("Bodyweight exercises cannot have external weight"),
            "NO_WEIGHT" => weight is null or 0 ? Valid : Invalid("This exercise doesn't track weight"),
            "BODYWEIGHT_OPTIONAL" => weight >= 0 ? Valid : Invalid("Weight must be positive if specified"),
            "WEIGHT_REQUIRED" => weight > 0 ? Valid : Invalid("Please specify the weight to use"),
            "MACHINE_WEIGHT" => weight > 0 ? Valid : Invalid("Please enter the machine weight setting"),
            _ => Invalid("Unknown weight type")
        };
    }
}
```

## Open Questions
1. **Migration Strategy**: How should existing exercises without weight types be handled during deployment?
2. **Bulk Update Scope**: Should bulk updates be limited by user permissions or exercise ownership?
3. **Validation Timing**: Should weight validation occur on every keystroke or on blur/submit?
4. **Cache Strategy**: Should weight types be cached indefinitely or refreshed periodically?
5. **Error Recovery**: What fallback behavior should occur if weight type API is unavailable?

## Dependencies
- **API Endpoints**: /api/exercise-weight-types must be deployed and accessible
- **Database Migration**: ExerciseWeightTypes table and Exercise.WeightTypeId column
- **Reference Data**: 5 predefined weight types seeded in database
- **UI Framework**: Compatible with existing Blazor component library

## Success Criteria
- All exercises can be assigned appropriate weight types
- Weight input validation works correctly for all 5 weight type categories
- UI provides clear feedback for weight requirements
- Bulk operations complete efficiently for large exercise sets
- Mobile responsive design maintains full functionality
- Accessibility standards met for all components