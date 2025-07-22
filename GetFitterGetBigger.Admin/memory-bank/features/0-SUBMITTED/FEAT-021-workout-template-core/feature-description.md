# Feature: Workout Template Core

## Feature ID: FEAT-021
## Created: 2025-07-22
## Status: SUBMITTED
## Source: Features/Workouts/WorkoutTemplate/WorkoutTemplateCore/

## Summary
Implement comprehensive workout template management UI in the Admin application, enabling Personal Trainers to create, manage, and share reusable workout blueprints. The feature includes a sophisticated template editor with zone-based exercise organization, intelligent suggestions, and drag-and-drop functionality.

## Business Context
This feature addresses the need for Personal Trainers to efficiently create structured workout programs as defined in the source RAW file at Features/Workouts/WorkoutTemplate/WorkoutTemplateCore/WorkoutTemplateCore_RAW.md. The system enables trainers to encapsulate their expertise into reusable templates that ensure consistency and provide clear guidance for clients.

## User Stories
As a Personal Trainer:
- I want to create workout templates with exercises organized by zones (warmup, main, cooldown) so that I can ensure proper workout structure
- I want to receive smart exercise suggestions based on the workout category so that I can build balanced programs efficiently
- I want to configure sets and reps for each exercise so that I can customize intensity for different fitness levels
- I want to duplicate and modify existing templates so that I can quickly create variations for different clients
- I want to share public templates with other trainers so that we can collaborate and learn from each other
- I want to see automatic equipment requirements so that clients know what they need before starting

## UI/UX Requirements

### Page/Component Structure

#### 1. Workout Templates List Page (`/workout-templates`)
- **Header Section**:
  - Page title "Workout Templates"
  - "Create New Template" primary button
  - View toggle (Grid/List)
- **Search and Filter Bar**:
  - Search input with real-time results
  - Quick filters (My Templates, Public, Private)
- **Filter Sidebar** (collapsible):
  - Workout Category dropdown
  - Workout Objective dropdown
  - Difficulty Level checkboxes
  - Duration range slider (10-240 min)
  - Tags multi-select
- **Main Content Area**:
  - Template cards/rows with key info
  - Pagination controls
  - Empty state with CTA

#### 2. Template Editor Page (`/workout-templates/new` or `/workout-templates/{id}/edit`)
- **Multi-step Form or Tabbed Interface**:
  - Tab 1: Basic Information
  - Tab 2: Exercise Selection
  - Tab 3: Set Configuration
  - Tab 4: Review & Save
- **Toolbar**:
  - Save/Cancel buttons
  - Auto-save indicator
  - Template preview button

#### 3. Key Components

**TemplateMetadataForm**:
- Template name input (required, max 100 chars)
- Description textarea (optional, max 500 chars)
- Category, Objective, Protocol dropdowns
- Duration slider with number input
- Difficulty radio buttons with visual indicators
- Public/Private toggle
- Tag input with autocomplete

**ExerciseManager**:
- Three-column layout for zones
- Drag-and-drop between zones
- Add exercise button per zone
- Exercise count badges
- Visual zone indicators (colors/icons)

**ExerciseSelector Modal**:
- Category-based filtering
- Search functionality
- Exercise preview panel
- Multi-select capability
- Smart suggestions section

**SetConfigurationPanel**:
- Inline editing on exercise cards
- Support for rep ranges (e.g., "8-12")
- Multiple configurations per exercise
- Intensity guideline input

### User Workflows

1. **Create New Template Flow**:
   - Click "Create New Template" → Template editor opens
   - Fill basic info → Select category triggers exercise filtering
   - Add main exercises → System suggests warmups
   - Configure sets inline → Drag to reorder
   - Review auto-generated equipment list → Save

2. **Quick Template Actions**:
   - Hover template card → Show action buttons
   - Click duplicate → Opens editor with copy
   - Click delete → Confirmation modal
   - Click edit → Opens in editor

3. **Exercise Management Flow**:
   - Click "Add Exercise" in zone → Modal opens
   - Search/browse exercises → Select multiple
   - Click "Add Selected" → Exercises appear in zone
   - Drag between zones → Visual feedback
   - Remove exercise → Warning if has linked warmup/cooldown

### Forms and Validations

**Template Form**:
- Name: Required, max 100 chars, trim whitespace
- Description: Optional, max 500 chars
- Category: Required dropdown
- Objective: Required dropdown
- Protocol: Required dropdown (show "Not Available" for future protocols)
- Duration: Required, 10-240 minutes
- Difficulty: Required radio selection
- Tags: Max 10 tags, each max 30 chars

**Exercise Configuration**:
- At least one exercise in Main zone required
- Sequence must be unique within zone
- Sets: 1-50, integer only
- Reps: Valid format (number or "X-Y" range)
- Either reps or duration required

### Visual Design Notes
- Use color coding for zones (Warmup: Blue, Main: Green, Cooldown: Purple)
- Exercise cards should show muscle groups and equipment icons
- Drag handles visible on hover
- Public templates show sharing icon
- Difficulty indicators: Beginner (1 star), Intermediate (2 stars), Advanced (3 stars)
- Equipment summary as icon grid with tooltips
- Loading skeletons for async operations
- Success toasts for save operations
- Warning modals for destructive actions

## Technical Requirements

### API Integration

#### Workout Template Endpoints
- `GET /api/workout-templates` - List templates with pagination and filtering
- `POST /api/workout-templates` - Create new template
- `GET /api/workout-templates/{id}` - Get template details
- `PUT /api/workout-templates/{id}` - Update template
- `DELETE /api/workout-templates/{id}` - Delete template

#### Exercise Management Endpoints
- `GET /api/workout-templates/{id}/exercises` - Get exercises by zone
- `POST /api/workout-templates/{id}/exercises` - Add exercise to template
- `PUT /api/workout-templates/{id}/exercises/{exerciseId}` - Update exercise
- `DELETE /api/workout-templates/{id}/exercises/{exerciseId}` - Remove exercise
- `PUT /api/workout-templates/{id}/exercises/reorder` - Bulk reorder

#### Business Operations
- `POST /api/workout-templates/{id}/duplicate` - Duplicate template
- `GET /api/workout-templates/{id}/exercise-suggestions` - Get smart suggestions
- `POST /api/workout-templates/{id}/validate` - Validate template

#### Reference Data Endpoints
- `GET /api/referenceTables/workoutCategories` - Workout categories
- `GET /api/referenceTables/workoutObjectives` - Workout objectives
- `GET /api/referenceTables/executionProtocols` - Execution protocols
- `GET /api/exercises` - Exercise library (existing)

### State Management
- Template form state with multi-step/tab navigation
- Exercise selection state (selected exercises before adding)
- Drag-and-drop state for reordering
- Filter state persistence on list page
- Optimistic updates for better UX
- Cache public templates list

### Component Dependencies
- Existing components to reuse:
  - PaginationControls
  - SearchInput
  - TagInput
  - ConfirmationModal
  - Toast notifications
- New shared components needed:
  - DifficultySelector
  - DurationSlider
  - ExerciseCard
  - ZonePanel
  - EquipmentSummary

### Error Handling
- Form validation errors inline
- API errors in toast notifications
- Network retry for failed requests
- Graceful degradation for missing data
- Specific messages for business rule violations

## Acceptance Criteria
- [ ] User can create new workout template with all required fields
- [ ] System validates zone sequence rules (warmup before main, cooldown after main)
- [ ] User can drag and drop exercises between zones
- [ ] System suggests warmup/cooldown exercises when adding main exercises
- [ ] Equipment list automatically aggregates from selected exercises
- [ ] User can configure sets/reps with support for ranges
- [ ] Public templates are visible to all PT users
- [ ] Private templates only visible to creator
- [ ] User can duplicate any visible template
- [ ] User can only edit/delete their own templates
- [ ] Search works across name, description, and tags
- [ ] Filters work correctly and can be combined
- [ ] Pagination handles large result sets
- [ ] All forms show appropriate validation messages
- [ ] Success/error states display correctly
- [ ] UI is responsive on tablet and desktop

## Dependencies
- API Feature: FEAT-026 (Workout Template Core endpoints)
- Reference Data: 
  - Workout Categories (from FEAT-025)
  - Workout Objectives (from FEAT-025)
  - Execution Protocols (from FEAT-025)
  - Exercise library (existing)
- Other Features: Exercise Management UI (for exercise selector)

## Implementation Notes

### Performance Considerations
- Implement virtual scrolling for large exercise lists
- Debounce search input (300ms)
- Cache reference data on app init
- Lazy load exercise details in selector
- Use optimistic updates for drag-and-drop
- Batch API calls when reordering multiple exercises

### Accessibility
- Keyboard navigation for all interactive elements
- ARIA labels for zone panels
- Screen reader announcements for drag-and-drop
- Focus management in modals
- High contrast mode support

### Browser Compatibility
- Support latest versions of Chrome, Firefox, Safari, Edge
- Ensure drag-and-drop works on touch devices
- Test on tablets (primary PT device)

### Security
- Validate all inputs client-side and server-side
- Sanitize HTML in description fields
- Implement CSRF protection
- Check authorization before showing edit/delete actions

### Technical Decisions
- Use React DnD for drag-and-drop functionality
- Implement form with React Hook Form for complex validation
- Use React Query for API state management
- Lazy load heavy components (ExerciseSelector)
- Implement route-based code splitting