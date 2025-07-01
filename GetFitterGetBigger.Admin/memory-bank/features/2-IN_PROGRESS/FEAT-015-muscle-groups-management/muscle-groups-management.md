# Muscle Groups Management Implementation Guide

This document provides detailed implementation guidelines for integrating Muscle Groups CRUD operations into the Admin application.

---
feature: muscle-groups-management
status: 0-SUBMITTED
created: 2025-07-01
---

## Feature Overview

Implement a comprehensive muscle groups management interface in the Admin application that allows Personal Trainers to create, read, update, and delete muscle groups. Each muscle group is associated with a specific body part and can be used when defining exercises.

## Technical Architecture

### Component Structure
- **muscle-groups/**
  - **MuscleGroupsList**: Main list view component
  - **MuscleGroupForm**: Add/Edit form component
  - **MuscleGroupDeleteDialog**: Deletion confirmation
  - **MuscleGroupFilters**: Body part filtering

### Service Layer
- **muscleGroupService**: API integration for muscle group operations
- **bodyPartService**: Body parts fetching (if not exists)

### State Management
- **muscleGroupSlice**: Redux slice for muscle groups state
- **bodyPartSlice**: Redux slice for body parts state

## Implementation Steps

### 1. API Service Implementation

Create a service that provides the following methods:

- **getAll()**: Fetches all muscle groups from `/api/ReferenceTables/MuscleGroups`
- **getByBodyPart(bodyPartId)**: Fetches muscle groups filtered by body part
- **create(data)**: Creates a new muscle group via POST request
- **update(id, data)**: Updates an existing muscle group via PUT request
- **delete(id)**: Soft deletes a muscle group via DELETE request

All methods should handle authentication tokens and return appropriate data or errors.

### 2. Redux State Management

#### State Structure
The muscle groups state should include:
- **items**: Array of muscle groups
- **selectedBodyPart**: Currently selected body part filter
- **loading**: Loading state indicator
- **error**: Error message if any

#### Actions Required
- **fetchMuscleGroups**: Retrieve all muscle groups
- **fetchMuscleGroupsByBodyPart**: Retrieve filtered muscle groups
- **createMuscleGroup**: Add new muscle group
- **updateMuscleGroup**: Modify existing muscle group
- **deleteMuscleGroup**: Soft delete muscle group
- **setSelectedBodyPart**: Update filter selection
- **clearError**: Clear error state

#### Error Handling
Handle the following states:
- Loading states during API calls
- Success states with data updates
- Error states with appropriate messages
- Conflict errors (409) for duplicate names

### 3. UI Components

#### List Component Requirements

**MuscleGroupsList Component**

Main list view that should:
- Display all muscle groups in a grid or list layout
- Show muscle group name and associated body part
- Provide Edit and Delete actions for each item
- Include an "Add Muscle Group" button
- Integrate body part filtering
- Handle loading and error states
- Open form dialog for add/edit operations
- Show confirmation dialog for deletions

**State Management:**
- Track form visibility (add/edit mode)
- Store currently editing muscle group
- Store muscle group pending deletion
- Apply body part filtering to displayed items

**User Interactions:**
- Click "Add" to open empty form
- Click "Edit" to open pre-filled form
- Click "Delete" to show confirmation dialog
- Filter muscle groups by body part
- Refresh list after successful operations

#### Form Component Requirements

**MuscleGroupForm Component**

Modal form that should:
- Display as a centered modal overlay
- Show appropriate title ("Add Muscle Group" or "Edit Muscle Group")
- Provide input fields for name and body part selection
- Load body parts list on mount
- Pre-fill fields when editing
- Validate required fields before submission
- Handle API errors appropriately
- Close on cancel or successful submission

**Form Fields:**
1. **Name Input**
   - Text input, max 100 characters
   - Required field validation
   - Trim whitespace before submission
   - Show error for duplicate names

2. **Body Part Dropdown**
   - Select dropdown with all body parts
   - Required field validation
   - Default to "Select a body part" placeholder
   - Load options from body parts state

**Error Handling:**
- Display field-specific validation errors
- Show conflict error (409) for duplicate names
- Display general error message for other failures
- Clear errors when user modifies input

**Additional Components:**
- **MuscleGroupDeleteDialog**: Confirmation dialog for deletions
- **MuscleGroupFilters**: Body part filtering component

## Testing Requirements

### Unit Tests
- Service layer methods
- Redux actions and reducers
- Component rendering and interactions
- Form validation logic

### Integration Tests
- Full CRUD flow
- Error handling scenarios
- Body part filtering
- Conflict resolution (duplicate names)

### E2E Tests
- Complete user journey for creating a muscle group
- Editing existing muscle groups
- Deletion with confirmation
- Error states and recovery

## Accessibility Requirements

1. All form inputs must have proper labels
2. Error messages must be announced to screen readers
3. Modal dialogs must trap focus
4. Keyboard navigation must work throughout
5. Color contrast must meet WCAG AA standards

## Security Considerations

1. All API calls must include authentication token
2. Input validation on client and server
3. XSS prevention through proper encoding
4. CSRF protection if applicable

## Performance Optimizations

1. Implement pagination if muscle groups list grows large
2. Debounce search/filter operations
3. Cache body parts list
4. Optimistic UI updates with rollback on error
5. Lazy load the muscle groups management module

## Future Enhancements

1. Bulk operations (create/update multiple)
2. Import/export functionality
3. Muscle group images/diagrams
4. Advanced filtering and search
5. Activity history and audit trail