# Workout Template Core Admin Components

## Overview
This document describes the UI components required for the Workout Template Core feature in the Admin application. Components are described in a technology-agnostic manner focusing on functionality and user experience.

## Component Hierarchy

```
WorkoutTemplateManagement
├── WorkoutTemplateList
│   ├── WorkoutTemplateFilters
│   ├── WorkoutTemplateGrid
│   └── WorkoutTemplateCard
├── WorkoutTemplateCreator
│   ├── BasicInfoStep
│   ├── ExerciseSelectionStep
│   │   ├── ExerciseSearch
│   │   ├── ExerciseSuggestions
│   │   └── SelectedExercisesList
│   ├── SetConfigurationStep
│   └── ReviewStep
├── WorkoutTemplateEditor
│   ├── TemplateHeader
│   ├── ExerciseManager
│   │   ├── ZoneSection (Warmup/Main/Cooldown)
│   │   ├── ExerciseCard
│   │   └── SetConfigurationEditor
│   └── StateTransitionPanel
└── WorkoutTemplatePreview
    ├── TemplateOverview
    ├── ExerciseTimeline
    └── EquipmentChecklist
```

## Core Components

### 1. WorkoutTemplateList
Main component for browsing and managing workout templates.

**Features**:
- Grid/List view toggle
- Pagination controls
- Quick actions (Edit, Duplicate, Archive)
- State indicators (Draft, Production, Archived)
- Search functionality
- Bulk operations

**Props**:
```json
{
  "viewMode": "grid | list",
  "pageSize": "number",
  "currentPage": "number",
  "sortBy": "string",
  "sortOrder": "asc | desc",
  "filters": "object"
}
```

### 2. WorkoutTemplateFilters
Advanced filtering panel for workout templates.

**Filter Options**:
- Search by name/description
- Workout category selector
- Workout objective selector
- Difficulty level checkboxes
- State filter (Draft/Production/Archived)
- Date range picker (created/modified)
- Public/Private toggle
- Creator filter (for admins)

**Features**:
- Collapsible filter groups
- Clear all filters button
- Save filter presets
- Filter count indicator

### 3. WorkoutTemplateGrid
Displays workout templates in a card grid layout.

**Card Information**:
- Template name and description
- Category and objective badges
- Duration and difficulty indicators
- Exercise count
- Equipment icons
- State badge
- Last modified date
- Creator avatar

**Interactions**:
- Click to view details
- Hover for quick preview
- Right-click context menu
- Drag to reorder (personal templates)

### 4. WorkoutTemplateCreator
Multi-step wizard for creating new workout templates.

**Steps**:
1. Basic Information
2. Exercise Selection
3. Set Configuration
4. Review & Create

**Features**:
- Step progress indicator
- Next/Previous navigation
- Save draft functionality
- Validation on each step
- Skip to step navigation

### 5. BasicInfoStep
First step of template creation - basic template information.

**Form Fields**:
- Template name (text input)
- Description (textarea)
- Workout category (dropdown)
- Workout objective (dropdown)
- Execution protocol (dropdown)
- Estimated duration (number input with slider)
- Difficulty level (radio buttons)
- Public visibility (toggle)
- Tags (tag input)

**Features**:
- Real-time validation
- Character counters
- Tooltips for each field
- Auto-save draft

### 6. ExerciseSelectionStep
Second step - selecting and organizing exercises.

**Layout**:
- Left panel: Exercise search and suggestions
- Center panel: Selected exercises by zone
- Right panel: Exercise details preview

**Features**:
- Drag and drop between zones
- Automatic warmup/cooldown suggestions
- Exercise search with filters
- Category-based recommendations
- Equipment requirement display
- Add custom notes to exercises

### 7. ExerciseSearch
Component for finding exercises to add to the template.

**Features**:
- Search by name
- Filter by category
- Filter by equipment
- Filter by muscle groups
- Recent exercises section
- Favorite exercises section
- Preview on hover

### 8. ExerciseSuggestions
Intelligent exercise recommendation component.

**Suggestion Types**:
- Based on workout category
- Complementary exercises (push/pull pairing)
- Associated warmups/cooldowns
- Equipment-based alternatives

**Display**:
- Suggestion reason
- Relevance score indicator
- Quick add buttons
- Dismiss suggestion option

### 9. SelectedExercisesList
Displays exercises organized by zone.

**Zone Sections**:
- Warmup zone (collapsible)
- Main zone (always expanded)
- Cooldown zone (collapsible)

**Exercise Display**:
- Exercise name and image
- Sequence number (editable)
- Equipment badges
- Notes indicator
- Remove button
- Drag handle

### 10. SetConfigurationStep
Third step - configuring sets for each exercise.

**Features**:
- Exercise timeline view
- Inline set configuration
- Copy configuration between exercises
- Protocol-specific fields
- Batch edit similar exercises

**Configuration Fields** (per exercise):
- Number of sets
- Target reps/duration
- Intensity guideline
- Rest periods (if applicable)

### 11. SetConfigurationEditor
Inline editor for exercise set configuration.

**Display Modes**:
- Collapsed: Shows summary (e.g., "3 × 8-12 reps")
- Expanded: Full configuration form

**Protocol Adaptations**:
- Standard: Sets × Reps
- AMRAP: Time limit
- EMOM: Reps per minute
- Custom protocols (future)

### 12. ReviewStep
Final step - review and create template.

**Sections**:
- Template summary card
- Exercise timeline
- Total equipment needed
- Estimated time breakdown
- Warnings/suggestions

**Actions**:
- Create as Draft
- Create and Publish
- Back to edit
- Save as template preset

### 13. WorkoutTemplateEditor
Full-featured editor for existing templates.

**Layout**:
- Header with template info and actions
- Tab navigation (Details, Exercises, Preview)
- Auto-save indicator
- Version history access

**Edit Modes**:
- Quick edit (inline fields)
- Full edit (opens wizard)
- Reorder mode (drag exercises)

### 14. ExerciseManager
Component for managing exercises within a template.

**Features**:
- Add exercises to any zone
- Reorder within zones
- Bulk operations (delete, move)
- Exercise replacement
- Set configuration overview

**Zone Management**:
- Expand/collapse zones
- Zone exercise count
- Zone duration estimate
- Add exercise button per zone

### 15. StateTransitionPanel
Component for managing workflow states.

**Display**:
- Current state indicator
- Available transitions
- Transition requirements
- Warning messages

**State Actions**:
- Move to Production (with confirmation)
- Rollback to Draft (if allowed)
- Archive template
- View state history

### 16. WorkoutTemplatePreview
Read-only preview of the workout template.

**Views**:
- Client view simulation
- Printable format
- Share preview link
- QR code for mobile preview

**Sections**:
- Overview card
- Exercise flow diagram
- Equipment checklist
- Estimated timeline
- Notes and instructions

## Common UI Elements

### State Badges
Visual indicators for workout states:
- Draft: Yellow/Orange badge
- Production: Green badge
- Archived: Gray badge

### Equipment Icons
Standardized icons for common equipment:
- Barbell, Dumbbell, Kettlebell
- Resistance bands, Cable machine
- Bodyweight indicator
- Custom equipment text

### Difficulty Indicators
Visual representation of difficulty:
- Beginner: 1 filled star
- Intermediate: 2 filled stars
- Advanced: 3 filled stars

### Duration Display
Formatted time display:
- Under 30 min: "Quick" tag
- 30-60 min: Standard display
- Over 60 min: "Extended" tag

## Responsive Design Requirements

### Desktop (1200px+)
- Full multi-column layouts
- Side-by-side panels
- Expanded filters
- Hover interactions

### Tablet (768px-1199px)
- Stacked layouts where needed
- Collapsible side panels
- Touch-friendly controls
- Reduced grid columns

### Mobile Consideration
- Admin interface is tablet/desktop only
- Mobile access shows read-only message
- Redirects to desktop version

## Accessibility Requirements

### Keyboard Navigation
- Tab order follows logical flow
- Arrow keys for list navigation
- Enter/Space for selections
- Escape to close modals

### Screen Reader Support
- Proper ARIA labels
- Form field descriptions
- State change announcements
- Progress indicators

### Visual Accessibility
- High contrast mode support
- Focus indicators
- Color-blind friendly states
- Minimum touch targets (44×44px)

## Performance Considerations

### Lazy Loading
- Exercise images on scroll
- Pagination for large lists
- Defer non-critical components

### Caching
- Cache reference data
- Remember filter preferences
- Store draft progress locally

### Optimistic Updates
- Immediate UI feedback
- Background saves
- Conflict resolution

## Error Handling

### Validation Errors
- Inline field validation
- Summary at step level
- Prevent progression with errors

### Network Errors
- Retry mechanisms
- Offline indicators
- Save draft locally

### State Conflicts
- Clear error messages
- Suggested resolutions
- Rollback options