# Admin Components for Workout Reference Data

This document defines the UI components for the Admin project that enable Personal Trainers to view and understand workout reference data. All components integrate into the existing ReferenceTable menu structure.

## Component Architecture

### Component Hierarchy
```
ReferenceTablesLayout
├── WorkoutObjectivesComponent
├── WorkoutCategoriesComponent
└── ExecutionProtocolsComponent
```

### Design Principles
- **Read-only interface**: Display reference data for consultation during workout template creation
- **Consistent styling**: Follow existing reference table component patterns
- **Responsive design**: Optimize for tablet and desktop use by Personal Trainers
- **Accessibility**: WCAG 2.1 AA compliance for professional use

## WorkoutObjectivesComponent

Displays workout objectives with detailed programming guidance.

### Component Structure
```
WorkoutObjectivesComponent
├── ObjectiveListView
├── ObjectiveDetailModal
└── ObjectiveSearchFilter
```

### Features
- **List view**: All workout objectives in display order
- **Detail modal**: Expanded view with full programming guidance
- **Search functionality**: Filter by objective name or description keywords
- **Responsive cards**: Visual cards showing key information

### UI Requirements

#### List View Layout
```
┌─────────────────────────────────────────────────────────┐
│ Workout Objectives                    🔍 [Search Box]   │
├─────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Muscular Strength                               [i] │ │
│ │ Develops maximum force production capabilities...   │ │
│ └─────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Hypertrophy                                     [i] │ │
│ │ Promotes muscle size increase through controlled... │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

#### Card Component Design
- **Header**: Objective name with info icon
- **Body**: First 100 characters of description with "Read more" link
- **Visual indicators**: Icon representing training type
- **Hover effects**: Subtle highlight for better UX

#### Detail Modal Layout
```
┌─────────────────────────────────────────────────────────┐
│ Muscular Strength                               [X]     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ Develops maximum force production capabilities.         │
│ Typical programming includes:                           │
│                                                         │
│ • Reps: 1-5 per set                                    │
│ • Sets: 3-5 total                                      │
│ • Rest: 3-5 minutes between sets                       │
│ • Intensity: 85-100% of 1RM                           │
│                                                         │
│ Focus on heavy compound movements with excellent        │
│ form and full recovery between efforts.                 │
│                                                         │
│                                    [Close]              │
└─────────────────────────────────────────────────────────┘
```

### Data Integration
- **API endpoint**: GET /api/workout-objectives
- **Caching**: Client-side cache for 1 hour
- **Error handling**: Graceful fallback with retry mechanism
- **Loading states**: Skeleton loading for better perceived performance

## WorkoutCategoriesComponent

Displays workout categories with visual organization and muscle group information.

### Component Structure
```
WorkoutCategoriesComponent
├── CategoryGridView
├── CategoryDetailModal
└── CategoryFilterBar
```

### Features
- **Grid layout**: Visual cards with icons and colors
- **Category filtering**: Filter by muscle groups or category type
- **Color-coded design**: Use category colors for visual organization
- **Icon display**: Show category icons for quick recognition

### UI Requirements

#### Grid View Layout
```
┌─────────────────────────────────────────────────────────┐
│ Workout Categories                [Filter] [View Toggle]│
├─────────────────────────────────────────────────────────┤
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│ │  ⏱️     │ │  💪     │ │  🦵     │ │  🏋️     │       │
│ │  HIIT   │ │  Arms   │ │  Legs   │ │ Abs&Core│       │
│ │ #FF6B35 │ │ #4ECDC4 │ │ #45B7D1 │ │ #F7DC6F │       │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘       │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│ │  🏃     │ │  🔙     │ │  🎯     │ │  🔄     │       │
│ │Shoulders│ │  Back   │ │ Chest   │ │FullBody │       │
│ │ #BB8FCE │ │ #85C1E9 │ │ #F8C471 │ │ #82E0AA │       │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘       │
└─────────────────────────────────────────────────────────┘
```

#### Category Card Design
- **Icon**: Large category icon at top
- **Name**: Category name below icon
- **Color**: Card border/background using category color
- **Hover effect**: Subtle animation and shadow
- **Click action**: Open detail modal

#### Detail Modal Layout
```
┌─────────────────────────────────────────────────────────┐
│ 💪 Arms                                         [X]     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ Upper arm muscle development focusing on biceps,        │
│ triceps, and forearms. Common exercises include        │
│ curls, extensions, and pressing movements designed     │
│ to build arm strength and definition.                  │
│                                                         │
│ Primary Muscle Groups:                                  │
│ • Biceps                                               │
│ • Triceps                                              │
│ • Forearms                                             │
│                                                         │
│ Color: #4ECDC4                                         │
│ Icon: bicep-icon                                       │
│                                                         │
│                                    [Close]              │
└─────────────────────────────────────────────────────────┘
```

### Data Integration
- **API endpoint**: GET /api/workout-categories
- **Visual assets**: Icon library integration
- **Color theming**: Dynamic CSS custom properties
- **Responsive grid**: Auto-adjusting grid based on screen size

## ExecutionProtocolsComponent

Displays execution protocols with detailed methodology explanations.

### Component Structure
```
ExecutionProtocolsComponent
├── ProtocolTableView
├── ProtocolDetailModal
└── ProtocolFilterTabs
```

### Features
- **Table view**: Structured display of protocol information
- **Filter tabs**: Group by intensity level or rest pattern
- **Code display**: Show both programmatic codes and display names
- **Methodology details**: Comprehensive execution instructions

### UI Requirements

#### Table View Layout
```
┌─────────────────────────────────────────────────────────┐
│ Execution Protocols                                     │
│ [All] [High Intensity] [Medium] [Low] [Time-Based]     │
├─────────────────────────────────────────────────────────┤
│ Protocol    │ Code      │ Time │ Reps │ Intensity │ [i] │
├─────────────┼───────────┼──────┼──────┼───────────┼─────┤
│ Standard    │ STANDARD  │  No  │ Yes  │  Medium   │ [i] │
│ AMRAP       │ AMRAP     │ Yes  │ Yes  │   High    │ [i] │
│ EMOM        │ EMOM      │ Yes  │ Yes  │  Medium   │ [i] │
│ For Time    │ FOR_TIME  │ Yes  │ Yes  │   High    │ [i] │
│ Tabata      │ TABATA    │ Yes  │  No  │   High    │ [i] │
└─────────────────────────────────────────────────────────┘
```

#### Table Features
- **Sortable columns**: Click headers to sort by any column
- **Filter tabs**: Pre-defined filter combinations
- **Info icons**: Click to open detail modal
- **Responsive design**: Stack on mobile devices

#### Detail Modal Layout
```
┌─────────────────────────────────────────────────────────┐
│ AMRAP (As Many Reps As Possible)                [X]     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ Code: AMRAP                                            │
│                                                         │
│ Description:                                           │
│ Maximum repetitions within a specified time window.    │
│ Perform as many complete reps as possible within       │
│ the time limit while maintaining proper form.          │
│                                                         │
│ Protocol Characteristics:                              │
│ • Time-based: Yes                                      │
│ • Rep-based: Yes                                       │
│ • Rest Pattern: Minimal                                │
│ • Intensity Level: High                                │
│                                                         │
│ Use Cases:                                             │
│ Conditioning, muscular endurance, metabolic training   │
│                                                         │
│                                    [Close]              │
└─────────────────────────────────────────────────────────┘
```

### Data Integration
- **API endpoint**: GET /api/execution-protocols
- **Code highlighting**: Monospace font for protocol codes
- **Filter logic**: Client-side filtering for performance
- **Sorting functionality**: Multi-column sorting capability

## Common Component Features

### Navigation Integration
All components integrate into the existing ReferenceTable menu:
```
Reference Tables
├── Exercise Weight Types
├── Difficulty Levels
├── Body Parts
├── Workout Objectives      ← New
├── Workout Categories      ← New
└── Execution Protocols     ← New
```

### Responsive Design Breakpoints
- **Desktop** (≥1024px): Full layout with sidebars
- **Tablet** (768px-1023px): Compact layout, collapsible sidebars
- **Mobile** (≤767px): Stacked layout, hidden sidebars

### Loading States
```
┌─────────────────────────────────────────────────────────┐
│ Workout Objectives                                      │
├─────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────┐ │
│ │ ████████████████                                    │ │
│ │ ████████████████████████                            │ │
│ └─────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ ████████████████                                    │ │
│ │ ████████████████████████                            │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### Error States
```
┌─────────────────────────────────────────────────────────┐
│ ⚠️ Unable to load workout objectives                    │
│                                                         │
│ Please check your connection and try again.             │
│                                                         │
│                    [Retry]                              │
└─────────────────────────────────────────────────────────┘
```

### Empty States
```
┌─────────────────────────────────────────────────────────┐
│ 📋 No workout objectives found                          │
│                                                         │
│ Try adjusting your search criteria.                    │
└─────────────────────────────────────────────────────────┘
```

## Accessibility Requirements

### WCAG 2.1 AA Compliance
- **Keyboard navigation**: Full tab navigation support
- **Screen readers**: Proper ARIA labels and roles
- **Color contrast**: 4.5:1 minimum ratio for text
- **Focus indicators**: Clear focus outlines
- **Alternative text**: Descriptive alt text for icons

### Interactive Elements
- **Buttons**: Minimum 44px touch target size
- **Links**: Descriptive link text
- **Form controls**: Associated labels
- **Modal dialogs**: Proper focus management

## Performance Considerations

### Optimization Strategies
- **Lazy loading**: Load components on demand
- **Virtual scrolling**: For large lists
- **Image optimization**: Compressed icons and graphics
- **Caching**: Aggressive client-side caching for reference data

### Bundle Size
- **Target**: < 50KB additional bundle size
- **Tree shaking**: Remove unused code
- **Code splitting**: Load components dynamically
- **Asset optimization**: Optimize icons and images

## Testing Requirements

### Component Testing
- **Unit tests**: Individual component functionality
- **Integration tests**: Component interaction with API
- **Visual regression**: Screenshot comparison testing
- **Accessibility testing**: Automated a11y checks

### User Testing
- **Usability testing**: Personal Trainer workflow validation
- **Performance testing**: Load time and responsiveness
- **Cross-browser testing**: Major browser compatibility
- **Device testing**: Tablet and desktop optimization