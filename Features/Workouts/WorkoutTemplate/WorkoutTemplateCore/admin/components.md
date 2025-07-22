# Workout Template Core Admin UI Components

## Overview
This document defines the conceptual UI components for the Workout Template Core feature in the Admin project. Components are described in a technology-agnostic manner focusing on functionality and user interaction.

## Component Hierarchy

```
WorkoutTemplateManagement
├── WorkoutTemplateList
│   ├── TemplateSearchBar
│   ├── TemplateFilterPanel
│   ├── TemplateGrid
│   │   └── TemplateCard
│   └── PaginationControls
├── WorkoutTemplateEditor
│   ├── TemplateMetadataForm
│   ├── ExerciseManager
│   │   ├── ExerciseZonePanel (Warmup/Main/Cooldown)
│   │   ├── ExerciseSelector
│   │   └── ExerciseCard
│   ├── SetConfigurationPanel
│   └── TemplatePreview
└── SharedComponents
    ├── EquipmentSummary
    ├── DifficultySelector
    └── TagManager
```

## Core Components

### 1. WorkoutTemplateList
**Purpose**: Main component for browsing and managing workout templates.

**Features**:
- Grid/List view toggle
- Search functionality
- Advanced filtering
- Sorting options
- Quick actions (view, edit, duplicate, delete)

**Layout**:
- Header with search bar and view controls
- Left sidebar with filters
- Main content area with template cards/rows
- Bottom pagination

### 2. TemplateSearchBar
**Purpose**: Quick search across template names and descriptions.

**Features**:
- Real-time search suggestions
- Search history
- Clear search button
- Search scope selector (name, description, tags)

**UI Elements**:
- Text input with search icon
- Dropdown for recent searches
- Loading indicator
- Result count display

### 3. TemplateFilterPanel
**Purpose**: Advanced filtering options for template discovery.

**Filters**:
- Workout Category (dropdown)
- Workout Objective (dropdown)
- Difficulty Level (checkbox group)
- Execution Protocol (dropdown)
- Duration Range (slider)
- Public/Private (toggle)
- Created By (user selector)
- Tags (multi-select)

**UI Elements**:
- Collapsible sections
- Applied filters summary
- Clear all filters button
- Filter count badge

### 4. TemplateCard
**Purpose**: Compact representation of a workout template.

**Display Information**:
- Template name (prominent)
- Category and objective badges
- Difficulty indicator
- Duration estimate
- Exercise count
- Creator name
- Public/Private icon
- Tag pills

**Actions**:
- View details
- Quick edit
- Duplicate
- Delete (with confirmation)

### 5. WorkoutTemplateEditor
**Purpose**: Comprehensive interface for creating and editing templates.

**Layout**:
- Step-by-step wizard or tabbed interface
- Save/Cancel toolbar
- Validation messages area
- Auto-save indicator

**Sections**:
1. Basic Information
2. Exercise Selection
3. Set Configuration
4. Review & Save

### 6. TemplateMetadataForm
**Purpose**: Form for template basic information.

**Fields**:
- Template Name (text input)
- Description (textarea)
- Workout Category (dropdown)
- Workout Objective (dropdown)
- Execution Protocol (dropdown with availability indicator)
- Estimated Duration (number input with slider)
- Difficulty Level (radio buttons with descriptions)
- Public/Private Toggle
- Tags (tag input component)

**Features**:
- Field validation indicators
- Help tooltips
- Character counters
- Dynamic protocol information

### 7. ExerciseManager
**Purpose**: Central component for organizing exercises within zones.

**Layout**:
- Three columns for Warmup, Main, Cooldown
- Drag-and-drop between zones
- Add exercise buttons per zone
- Zone exercise counts

**Features**:
- Visual zone separation
- Automatic warmup/cooldown suggestions
- Exercise reordering
- Bulk operations

### 8. ExerciseZonePanel
**Purpose**: Container for exercises within a specific zone.

**Features**:
- Zone header with count
- Sortable exercise list
- Empty state with add button
- Zone-specific validations
- Collapse/expand functionality

**Visual Design**:
- Color-coded zones
- Drop zone indicators
- Sequence numbers
- Alert badges for warnings

### 9. ExerciseSelector
**Purpose**: Modal/panel for selecting exercises to add.

**Features**:
- Category-based browsing
- Search functionality
- Exercise preview
- Multiple selection support
- Smart suggestions based on:
  - Current category
  - Already selected exercises
  - Push/pull balance
  - Equipment consistency

**Layout**:
- Search bar at top
- Category tabs/filter
- Exercise grid/list
- Selected exercises basket
- Add selected button

### 10. ExerciseCard (within template)
**Purpose**: Represents an exercise within the template.

**Display**:
- Exercise name
- Primary muscle groups
- Equipment icons
- Exercise notes indicator
- Set configuration summary
- Warmup/cooldown link indicators

**Actions**:
- Edit notes
- Configure sets
- Remove (with warning if linked)
- Drag handle for reordering

### 11. SetConfigurationPanel
**Purpose**: Configure sets for a specific exercise.

**Features**:
- Multiple configuration support
- Protocol-specific fields
- Configuration templates
- Copy from other exercises

**Fields (Standard Protocol)**:
- Number of Sets
- Target Reps (supports ranges)
- Rest Period
- Intensity Guidelines

**UI Elements**:
- Add configuration button
- Configuration cards
- Validation messages
- Preview of configuration

### 12. TemplatePreview
**Purpose**: Read-only view of the complete template.

**Sections**:
- Template summary
- Equipment checklist
- Exercise flow by zone
- Estimated time breakdown
- Print-friendly view

## Shared Components

### 13. EquipmentSummary
**Purpose**: Display aggregated equipment requirements.

**Features**:
- Equipment list with icons
- Availability indicators
- Equipment alternatives
- Total equipment count

### 14. DifficultySelector
**Purpose**: Visual difficulty level selector.

**Design**:
- Three-option selector
- Visual indicators (colors/icons)
- Descriptions on hover
- Clear selection state

### 15. TagManager
**Purpose**: Add and manage template tags.

**Features**:
- Tag autocomplete
- Popular tags suggestions
- Custom tag creation
- Tag limit enforcement
- Remove tags easily

## Responsive Design Requirements

### Desktop (>1200px)
- Full three-column layout for exercise zones
- Side-by-side panels
- Expanded filter options
- Hover interactions

### Tablet (768px-1200px)
- Two-column layout for exercises
- Collapsible sidebars
- Touch-optimized controls
- Stacked forms

### Mobile (<768px)
- Single column layout
- Bottom sheet modals
- Swipe gestures for zones
- Simplified navigation

## Interaction Patterns

### Drag and Drop
- Visual feedback during drag
- Valid drop zone highlighting
- Automatic scrolling
- Touch device support
- Undo capability

### Validation Feedback
- Inline field validation
- Summary validation panel
- Warning vs error distinction
- Progressive disclosure of issues

### Loading States
- Skeleton screens
- Progress indicators
- Partial content loading
- Optimistic updates

### Empty States
- Contextual messages
- Action prompts
- Helpful illustrations
- Quick start guides

## Accessibility Requirements

### Keyboard Navigation
- Tab order compliance
- Keyboard shortcuts
- Focus indicators
- Skip navigation links

### Screen Reader Support
- Semantic HTML structure
- ARIA labels and descriptions
- Live regions for updates
- Alternative text for icons

### Visual Accessibility
- Color contrast compliance
- Focus indicators
- Error identification beyond color
- Resizable text support

## Performance Considerations

### Lazy Loading
- Exercise images on demand
- Pagination for large lists
- Progressive form sections
- Deferred non-critical components

### Caching
- Template drafts
- Exercise selections
- Filter preferences
- Search history

### Optimistic UI
- Immediate visual feedback
- Background saving
- Conflict resolution
- Retry mechanisms