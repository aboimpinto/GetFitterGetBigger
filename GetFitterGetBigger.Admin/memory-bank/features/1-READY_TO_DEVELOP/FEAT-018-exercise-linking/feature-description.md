# FEAT-018: Exercise Linking UI

## Feature ID: FEAT-018
## Created: 2025-07-09
## Status: READY TO DEVELOP

## Overview

This feature implements the user interface for managing exercise links in the Admin application. Personal Trainers will be able to link exercises together, defining warmup and cooldown relationships that streamline workout creation.

## API Dependency

This feature depends on **API FEAT-022** which provides the backend endpoints for exercise linking functionality.

## User Stories

### As a Personal Trainer, I want to:
1. Link warmup exercises to workout exercises so they are automatically suggested
2. Link cooldown exercises to workout exercises for proper recovery
3. View all linked exercises for a specific workout exercise
4. Edit the order of linked exercises
5. Remove exercise links when they're no longer relevant
6. See suggested links based on common patterns

## UI Components

### 1. Exercise Detail Enhancement

**Location**: Exercise detail/edit page

**New Section**: "Linked Exercises"
- Tab or collapsible section showing:
  - Warmup exercises (with order)
  - Cooldown exercises (with order)
- Only visible for "Workout" type exercises
- Add/Remove buttons for managing links

### 2. Link Management Modal

**Trigger**: "Add Warmup" or "Add Cooldown" button

**Features**:
- Search/filter exercises by name
- Filter to show only appropriate exercise types
- Preview exercise details
- Multi-select for adding multiple links
- Drag-and-drop or number input for ordering

### 3. Exercise Link Card

**Component**: Reusable card showing linked exercise
- Exercise name and thumbnail
- Type badge (Warmup/Cooldown)
- Order number
- Quick actions: Reorder, Remove
- Click to view exercise details

### 4. Exercise List Enhancement

**Location**: Main exercise list/grid

**Visual Indicators**:
- Icon showing if exercise has links
- Tooltip showing link count
- Filter option: "Has links"

## User Workflows

### Adding Exercise Links

1. Navigate to a workout exercise detail page
2. Click "Manage Links" or scroll to "Linked Exercises" section
3. Click "Add Warmup" or "Add Cooldown"
4. Search and select exercises from modal
5. Set display order (drag or number input)
6. Save changes
7. See success notification

### Viewing Exercise Links

1. From exercise list, see link indicators
2. Click exercise to view details
3. "Linked Exercises" section shows:
   - Warmup exercises in order
   - Cooldown exercises in order
4. Click any linked exercise to navigate to its details

### Editing Link Order

1. In "Linked Exercises" section
2. Drag exercises to reorder OR
3. Click edit icon and change order numbers
4. Auto-save or explicit save button
5. See success notification

### Removing Links

1. Hover over linked exercise card
2. Click remove (X) icon
3. Confirm removal in dialog
4. Link removed immediately
5. See success notification

## Component Structure

### ExerciseLinkManager
- Parent component for all linking functionality
- Manages state for links
- Handles API calls

### LinkedExercisesList
- Displays warmup/cooldown exercises
- Handles reordering
- Emits events for actions

### AddExerciseLinksModal
- Exercise search and selection
- Filters by exercise type
- Multi-select capability

### LinkedExerciseCard
- Individual linked exercise display
- Quick actions
- Drag handle for reordering

## State Management

### Exercise State Enhancement
```typescript
interface ExerciseDetailState {
  exercise: Exercise;
  links: {
    warmups: ExerciseLink[];
    cooldowns: ExerciseLink[];
  };
  isLoadingLinks: boolean;
  linkError: string | null;
}
```

### Actions
- `loadExerciseLinks(exerciseId)`
- `addExerciseLink(link)`
- `updateLinkOrder(linkId, newOrder)`
- `removeExerciseLink(linkId)`
- `reorderLinks(linkType, newOrder)`

## API Integration

### Services to Implement

#### ExerciseLinkService
```typescript
interface IExerciseLinkService {
  getExerciseLinks(exerciseId: string, linkType?: string): Promise<ExerciseLinksDto>;
  createLink(exerciseId: string, request: CreateLinkRequest): Promise<ExerciseLink>;
  updateLink(exerciseId: string, linkId: string, request: UpdateLinkRequest): Promise<ExerciseLink>;
  deleteLink(exerciseId: string, linkId: string): Promise<void>;
  getSuggestedLinks(exerciseId: string): Promise<SuggestedLink[]>;
}
```

### Error Handling
- Display user-friendly messages for:
  - Duplicate link attempts
  - Invalid exercise type combinations
  - Network errors
  - Validation failures

## UI/UX Requirements

### Visual Design
- Clear distinction between warmup and cooldown sections
- Intuitive drag-and-drop with visual feedback
- Loading states for async operations
- Success/error notifications

### Responsive Design
- Mobile-friendly link management
- Touch-friendly drag handles
- Appropriate modal sizes for different screens

### Accessibility
- Keyboard navigation for all actions
- Screen reader announcements
- ARIA labels for interactive elements
- Focus management in modals

## Performance Considerations

### Caching
- Cache exercise links per exercise
- Invalidate on modifications
- Prefetch links for exercise list view

### Optimizations
- Debounce search in link modal
- Lazy load exercise details in cards
- Batch API calls when possible

## Testing Requirements

### Component Tests
- Link manager CRUD operations
- Drag-and-drop reordering
- Modal search and selection
- Error state handling

### Integration Tests
- Full workflow: add, reorder, remove
- API error handling
- State management flow
- Cache invalidation

### E2E Tests
- Complete link management workflow
- Cross-browser compatibility
- Mobile responsiveness

## Future Enhancements

1. **Bulk Operations**: Link multiple exercises at once
2. **Templates**: Save common link combinations
3. **Analytics View**: Show most used links
4. **Quick Actions**: Add links from exercise list
5. **Import/Export**: Share link configurations

## Acceptance Criteria

1. ✓ PTs can add warmup links to workout exercises
2. ✓ PTs can add cooldown links to workout exercises
3. ✓ Links can be reordered via drag-and-drop
4. ✓ Links can be removed with confirmation
5. ✓ Only appropriate exercise types can be linked
6. ✓ Visual indicators show which exercises have links
7. ✓ All actions provide user feedback
8. ✓ Mobile-responsive design
9. ✓ Accessible via keyboard navigation
10. ✓ Proper error handling and messages