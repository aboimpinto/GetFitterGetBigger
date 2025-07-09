# FEAT-022: Exercise Links - Propagation Notes

## Overview
This document provides notes for propagating the Exercise Links feature to the Admin and Clients projects.

## What to Propagate

1. **API Endpoints Documentation** (`api-endpoints-documentation.md`)
   - Complete endpoint specifications with request/response examples
   - TypeScript interfaces for all DTOs
   - Business rules and validation errors

2. **Feature Description** (`feature-description.md`)
   - Business requirements and use cases
   - Technical constraints

## Admin Project Implementation Notes

### UI Components Needed
1. **Exercise Link Management**
   - Add/Edit/Delete links from exercise detail page
   - Drag-and-drop for reordering (display order)
   - Filter toggle for Warmup/Cooldown
   - Target exercise preview cards

2. **Validation UI**
   - Prevent REST exercise selection
   - Show counter (X/10) for link limits
   - Client-side circular reference check

3. **Enhanced Features**
   - Quick search modal for exercises
   - Bulk operations toolbar
   - Copy links between exercises

### TypeScript Service Interface
```typescript
interface ExerciseLinkService {
  createLink(exerciseId: string, data: CreateExerciseLinkDto): Promise<ExerciseLinkDto>;
  getLinks(exerciseId: string, linkType?: string, includeDetails?: boolean): Promise<ExerciseLinksResponseDto>;
  getSuggestedLinks(exerciseId: string, count?: number): Promise<ExerciseLinkDto[]>;
  updateLink(exerciseId: string, linkId: string, data: UpdateExerciseLinkDto): Promise<ExerciseLinkDto>;
  deleteLink(exerciseId: string, linkId: string): Promise<void>;
}
```

## Clients Project Implementation Notes

### Workout Flow Integration
1. **Pre-Workout**
   - Display warmup exercises before main workout
   - Allow skipping individual warmups
   - Show estimated warmup duration

2. **Post-Workout**
   - Display cooldown exercises after main workout
   - Allow skipping cooldowns
   - Track completion status

3. **UI Considerations**
   - Clear visual distinction (colors/badges)
   - Respect display order
   - Smooth transitions between exercises

### Performance Optimizations
```typescript
// Fetch exercise with all links in one call
interface WorkoutService {
  getExerciseWithLinks(exerciseId: string): Promise<{
    exercise: Exercise;
    warmupExercises: ExerciseLink[];
    cooldownExercises: ExerciseLink[];
  }>;
}
```

- Use `includeExerciseDetails=true` to reduce API calls
- Cache linked exercises locally
- Preload next exercise in sequence

## Common Considerations

### Error Handling
- Handle 400 errors for validation failures
- Show user-friendly messages for business rule violations
- Implement retry logic for network failures

### State Management
- Track link modifications locally before saving
- Implement optimistic updates for better UX
- Handle concurrent modifications gracefully

### Testing Focus Areas
1. **Admin**
   - Max links enforcement (10 per type)
   - Circular reference prevention
   - Bulk operations
   - Drag-and-drop reordering

2. **Clients**
   - Workout flow with links
   - Skip functionality
   - Offline support
   - Loading states

## Migration Notes
- No breaking changes to existing Exercise endpoints
- Links are optional - exercises work without them
- Hard delete pattern - once deleted, links cannot be recovered