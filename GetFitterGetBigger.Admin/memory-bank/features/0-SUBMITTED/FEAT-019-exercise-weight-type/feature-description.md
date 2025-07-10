# Feature: Exercise Weight Type Management

## Feature ID: FEAT-019
## Created: 2025-07-10
## Status: SUBMITTED
## Source: Features/ExerciseWeightType/

## Summary
Implement exercise weight type classification system in the Admin application, enabling Personal Trainers to assign appropriate weight types to exercises and ensure proper weight validation during workout creation. This feature provides a standardized approach to handling weight assignments with 5 predefined weight type categories.

## Business Context
This feature addresses the need for consistent weight handling across different exercise types as defined in the source documentation at Features/ExerciseWeightType/. The system prevents invalid workout configurations where bodyweight exercises might be assigned external weights or weighted exercises might be created without proper weight specifications.

The business problem being solved is the current inconsistency in weight assignments, which leads to confusion for personal trainers and invalid workout templates. By implementing a structured weight type classification, the system ensures that:
- Bodyweight exercises cannot have external weight inappropriately assigned
- Weight-required exercises must have weight specified
- Optional weight exercises provide clear guidance to trainers

## User Stories
As a Personal Trainer:
- I want to assign weight types to exercises so that the system validates weight inputs appropriately
- I want to see clear visual indicators of exercise weight requirements so that I can create accurate workout templates
- I want the weight input fields to adapt based on exercise type so that I cannot make invalid weight assignments
- I want to bulk update exercise weight types so that I can efficiently migrate existing exercise data

As an Admin:
- I want to view weight type reference data so that I understand the classification system
- I want to monitor exercise weight type assignments so that I can ensure data quality

## UI/UX Requirements

### Page/Component Structure
1. **Exercise Management Integration**
   - Enhanced exercise list with weight type badges
   - Exercise form with weight type selector
   - Bulk update interface for weight type assignments

2. **Weight Type Reference Panel**
   - Read-only view of all weight types with descriptions
   - Validation rules display
   - Common examples for each type

3. **Dynamic Weight Input Components**
   - Adaptive weight fields based on selected exercise type
   - Real-time validation with contextual messaging
   - Unit conversion support (kg/lbs)

### User Workflows
1. **Exercise Creation with Weight Type**
   - Select exercise weight type from dropdown
   - System adapts weight input behavior based on selection
   - Validation prevents submission with invalid weight assignments
   - Success confirmation with weight type badge display

2. **Exercise Weight Type Management**
   - View existing exercises with weight type classifications
   - Filter exercises by weight type
   - Bulk select exercises for weight type updates
   - Preview changes before applying bulk updates

3. **Workout Template Creation Enhancement**
   - Automatic weight type detection for selected exercises
   - Contextual help messages based on exercise weight type
   - Dynamic weight input validation
   - Error prevention for incompatible weight assignments

### Forms and Validations
- **Weight Type Selection**: Required field with dropdown of 5 predefined types
- **Weight Input**: Dynamic validation based on selected type:
  - BODYWEIGHT_ONLY: Weight must be null or 0
  - NO_WEIGHT: Weight must be null or 0  
  - BODYWEIGHT_OPTIONAL: Weight can be null, 0, or positive number
  - WEIGHT_REQUIRED: Weight must be positive number (> 0)
  - MACHINE_WEIGHT: Weight must be positive number (> 0)

### Visual Design Notes
- Use color-coded badges for weight type identification
- Implement icons alongside text for accessibility
- Provide tooltip descriptions for weight type options
- Clear visual hierarchy between required and optional weight inputs
- Responsive design for mobile exercise management

## Technical Requirements

### API Integration
**GET /api/exercise-weight-types**
- Retrieve all weight types for dropdown population
- No authentication required (public reference data)
- Response: Array of weight type objects with id, code, name, description

**GET /api/exercise-weight-types/{id}**  
- Retrieve specific weight type details
- Used for validation and detailed information display
- Response: Single weight type object

### State Management
- Cache weight types on application initialization
- Maintain selected weight type state in exercise forms
- Track weight input validation state per exercise
- Handle real-time validation updates

### Component Dependencies
- ExerciseWeightTypeSelector: Dropdown with descriptions
- WeightInputField: Dynamic input with type-aware validation
- ExerciseWeightTypeBadge: Visual indicator component
- ExerciseFormWithWeightType: Enhanced exercise management form
- WorkoutExerciseWeightInput: Specialized workout template component

## Acceptance Criteria
- [ ] User can select from 5 predefined weight types when creating/editing exercises
- [ ] System validates weight inputs according to selected weight type rules
- [ ] UI displays appropriate weight input fields based on exercise weight type
- [ ] Error states show clear, contextual validation messages
- [ ] Success states show weight type badges with exercises
- [ ] Weight type reference panel displays all types with descriptions
- [ ] Bulk update interface allows efficient weight type assignment
- [ ] Mobile responsive design maintains functionality across screen sizes
- [ ] Accessibility requirements met for keyboard navigation and screen readers

## Dependencies
- API Feature: FEAT-023 (Exercise Weight Type reference table)
- Reference Data: 5 predefined weight types from API
- Other Features: Exercise Management (exercises must have weight types assigned)

## Implementation Notes
This feature enhances existing exercise management workflows rather than creating entirely new pages. The focus is on integrating weight type selection and validation into current exercise CRUD operations while providing clear visual feedback to users about weight requirements.

Key technical considerations:
- Weight types are read-only reference data from the API
- Validation occurs client-side with server-side confirmation
- Component design should be reusable across exercise and workout contexts
- Migration workflow needed for existing exercises without weight types

The feature leverages comprehensive UI component specifications already defined in Features/ExerciseWeightType/admin/components.md, ensuring consistent implementation across all Admin application contexts.