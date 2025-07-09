# FEAT-018: Exercise Linking - Implementation Checklist

## Overview
This checklist ensures all aspects of the Exercise Linking feature are properly implemented in the Admin application.

## Prerequisites
- [ ] API FEAT-022 is deployed and accessible
- [ ] Exercise management feature is working properly
- [ ] Exercise types (Warmup, Workout, Cooldown, Rest) are properly displayed

## API Integration Setup

### 1. Service Implementation
- [ ] Create `IExerciseLinkService` interface
- [ ] Implement `ExerciseLinkService` with all 5 endpoints
- [ ] Add proper error handling for all API calls
- [ ] Implement retry logic for network failures
- [ ] Add service to dependency injection

### 2. TypeScript Models
- [ ] Add `ExerciseLinkDto` interface
- [ ] Add `ExerciseLinksResponseDto` interface
- [ ] Add `CreateExerciseLinkDto` interface
- [ ] Add `UpdateExerciseLinkDto` interface
- [ ] Update `ExerciseDto` to include link indicators

## UI Components

### 1. Exercise Detail Page Enhancement
- [ ] Add "Linked Exercises" section (only for Workout type exercises)
- [ ] Create tabs/sections for Warmups and Cooldowns
- [ ] Add "Add Warmup" and "Add Cooldown" buttons
- [ ] Display link count (e.g., "3/10 Warmups")
- [ ] Show empty states when no links exist

### 2. Exercise Link Cards
- [ ] Create `ExerciseLinkCard` component
- [ ] Display exercise name and thumbnail
- [ ] Show link type badge (Warmup/Cooldown)
- [ ] Display order number
- [ ] Add hover state with actions (Edit, Delete)
- [ ] Make cards draggable for reordering
- [ ] Add click handler to navigate to exercise details

### 3. Add Link Modal
- [ ] Create `AddExerciseLinkModal` component
- [ ] Add exercise search functionality
- [ ] Filter exercises by type (show only valid targets)
- [ ] Exclude already linked exercises
- [ ] Exclude REST exercises
- [ ] Show exercise preview on selection
- [ ] Allow setting initial display order
- [ ] Add loading state during save

### 4. Exercise List Enhancement
- [ ] Add link indicator icon to exercise cards
- [ ] Show tooltip with link counts
- [ ] Add "Has links" filter option
- [ ] Update exercise type badges

## State Management

### 1. Store Setup
- [ ] Add exercise links state slice
- [ ] Add actions for CRUD operations
- [ ] Add loading and error states
- [ ] Implement optimistic updates
- [ ] Add cache management (1 hour TTL)

### 2. Actions
- [ ] `loadExerciseLinks(exerciseId, linkType?)`
- [ ] `createExerciseLink(exerciseId, linkData)`
- [ ] `updateExerciseLink(exerciseId, linkId, updateData)`
- [ ] `deleteExerciseLink(exerciseId, linkId)`
- [ ] `reorderExerciseLinks(exerciseId, linkType, newOrder)`
- [ ] `loadSuggestedLinks(exerciseId)`

## Business Logic Implementation

### 1. Validation Rules
- [ ] Prevent linking non-Workout exercises as source
- [ ] Validate target exercise types match link type
- [ ] Prevent REST exercise selection
- [ ] Check maximum links (10 per type)
- [ ] Implement circular reference check (client-side)
- [ ] Prevent duplicate links

### 2. User Feedback
- [ ] Show success notifications for all actions
- [ ] Display specific error messages from API
- [ ] Add confirmation dialog for deletions
- [ ] Show progress during bulk operations
- [ ] Indicate when max links reached

### 3. Performance Optimizations
- [ ] Implement debounced search in modal
- [ ] Batch reorder updates
- [ ] Cache exercise details
- [ ] Lazy load linked exercise details
- [ ] Use `includeExerciseDetails` parameter wisely

## Accessibility

### 1. Keyboard Navigation
- [ ] All actions accessible via keyboard
- [ ] Tab order makes sense
- [ ] Escape closes modals
- [ ] Enter confirms actions

### 2. Screen Reader Support
- [ ] Proper ARIA labels on all buttons
- [ ] Announce state changes
- [ ] Describe drag and drop actions
- [ ] Label form fields clearly

### 3. Visual Accessibility
- [ ] Sufficient color contrast
- [ ] Don't rely on color alone
- [ ] Focus indicators visible
- [ ] Error states clearly marked

## Testing

### 1. Unit Tests
- [ ] Service methods
- [ ] Component rendering
- [ ] State management logic
- [ ] Validation functions

### 2. Integration Tests
- [ ] Full CRUD workflow
- [ ] Error handling paths
- [ ] State updates
- [ ] API communication

### 3. E2E Tests
- [ ] Add warmup link flow
- [ ] Add cooldown link flow
- [ ] Reorder links
- [ ] Delete link with confirmation
- [ ] Maximum links enforcement
- [ ] Error scenarios

### 4. Manual Testing Scenarios
- [ ] Create first link for an exercise
- [ ] Add multiple warmups and cooldowns
- [ ] Reorder links via drag and drop
- [ ] Try to exceed 10 links limit
- [ ] Try to link REST exercise
- [ ] Try to create circular reference
- [ ] Delete and restore link
- [ ] Search and filter in modal
- [ ] Test on mobile devices
- [ ] Test with slow network

## Deployment Checklist

### 1. Pre-deployment
- [ ] All tests passing
- [ ] Code review completed
- [ ] Documentation updated
- [ ] API endpoints verified

### 2. Configuration
- [ ] API base URL configured
- [ ] Error tracking setup
- [ ] Feature flags (if applicable)

### 3. Post-deployment
- [ ] Verify feature in production
- [ ] Monitor error rates
- [ ] Check performance metrics
- [ ] Gather user feedback

## Known Limitations
- No authorization currently (to be added later)
- Suggested links algorithm is basic
- No bulk operations yet
- No template support

## Future Enhancements to Consider
1. Bulk link management
2. Link templates
3. Copy links between exercises
4. Analytics dashboard
5. Import/export functionality