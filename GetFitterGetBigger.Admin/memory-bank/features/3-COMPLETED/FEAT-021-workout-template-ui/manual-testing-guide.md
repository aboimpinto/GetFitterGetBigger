# Manual Testing Guide - Workout Template UI

## Overview
This guide provides comprehensive test scenarios for manually testing the Workout Template management feature in the GetFitterGetBigger Admin application. All testing should be performed with a user account having PT-Tier or Admin-Tier authorization.

## Prerequisites
1. Ensure the API is running at `http://localhost:5214`
2. Log in with PT-Tier or Admin-Tier credentials
3. Navigate to Training > Workout Templates

## Test Scenarios

### 1. Template List View

#### 1.1 Initial Load
- [ ] Verify the list page loads successfully
- [ ] Check that the page header shows "Workout Templates"
- [ ] Verify breadcrumb shows: Home > Training > Workout Templates
- [ ] Confirm "New Template" button is visible in the top-right corner

#### 1.2 Empty State
- [ ] If no templates exist, verify empty state message is displayed
- [ ] Confirm "Create your first template" button is shown

#### 1.3 Data Display
- [ ] Verify templates are displayed in a grid layout
- [ ] Each card should show:
  - Template name
  - Category badge
  - Difficulty badge
  - State indicator (Draft/Production/Archived)
  - Public/Private visibility icon
  - Duration in minutes
  - Exercise count
  - Equipment count (placeholder: "Coming soon")
  - Created/Updated dates
  - Action buttons (View, Edit, Delete - based on state)

#### 1.4 Pagination
- [ ] If more than 10 templates exist, verify pagination controls appear
- [ ] Test navigating between pages
- [ ] Verify page size selector works (10, 25, 50 items)

### 2. Search and Filtering

#### 2.1 Search Functionality
- [ ] Type a template name in the search box
- [ ] Verify search triggers after 300ms delay (debounce)
- [ ] Confirm results update without page refresh
- [ ] Test partial name matching
- [ ] Clear search and verify all templates return

#### 2.2 Filter by Category
- [ ] Click category dropdown
- [ ] Select a specific category (e.g., "Strength Training")
- [ ] Verify only templates with that category are shown
- [ ] Select "All Categories" to clear filter

#### 2.3 Filter by Difficulty
- [ ] Click difficulty dropdown
- [ ] Select a difficulty level (Beginner/Intermediate/Advanced)
- [ ] Verify filtering works correctly
- [ ] Test combining with category filter

#### 2.4 Filter by State
- [ ] Toggle between All/Draft/Production/Archived
- [ ] Verify correct templates are shown for each state
- [ ] Confirm state badges match filter

#### 2.5 Filter by Visibility
- [ ] Toggle between All/Public/Private
- [ ] Verify visibility icons match filter

#### 2.6 Combined Filters
- [ ] Apply multiple filters simultaneously
- [ ] Verify all filters work together correctly
- [ ] Use "Clear Filters" button
- [ ] Confirm all filters are reset

### 3. Template Creation

#### 3.1 Navigation
- [ ] Click "New Template" button
- [ ] Verify URL changes to `/workout-templates/new`
- [ ] Confirm breadcrumb shows: Home > Training > Workout Templates > New Template

#### 3.2 Form Fields
- [ ] Verify all required fields are marked with asterisk (*)
- [ ] Test each field:
  - **Name**: Enter a unique name (3-100 characters)
  - **Description**: Enter description (max 500 characters)
  - **Category**: Select from dropdown
  - **Difficulty**: Select from dropdown
  - **Duration**: Enter number (1-300 minutes)
  - **Is Public**: Toggle checkbox

#### 3.3 Validation
- [ ] Try to save with empty required fields
- [ ] Verify validation messages appear
- [ ] Enter invalid data:
  - Name too short (< 3 chars)
  - Name too long (> 100 chars)
  - Description too long (> 500 chars)
  - Duration outside range (< 1 or > 300)
- [ ] Fix validation errors and verify messages disappear

#### 3.4 Name Uniqueness
- [ ] Enter a name that already exists
- [ ] Verify async validation shows "Name already exists" after typing stops
- [ ] Modify name to be unique
- [ ] Confirm validation message clears

#### 3.5 Auto-Save (Draft Templates)
- [ ] Start filling the form
- [ ] Wait 5 seconds after making changes
- [ ] Verify "Auto-saving..." indicator appears
- [ ] Confirm "Auto-saved" message shows
- [ ] Navigate away and return to verify data persists

#### 3.6 Unsaved Changes Warning
- [ ] Make changes to the form
- [ ] Try to navigate away before saving
- [ ] Verify warning dialog appears: "You have unsaved changes"
- [ ] Click "Stay on Page" and verify you remain on form
- [ ] Try again and click "Leave Page"
- [ ] Confirm navigation proceeds

#### 3.7 Save Template
- [ ] Fill all required fields with valid data
- [ ] Click "Save" button
- [ ] Verify success notification appears
- [ ] Confirm redirect to detail page
- [ ] Check template appears in list view

### 4. Template Detail View

#### 4.1 Navigation
- [ ] Click "View" button on a template card
- [ ] Verify URL changes to `/workout-templates/{id}`
- [ ] Confirm breadcrumb shows template name

#### 4.2 Information Display
- [ ] Verify all template information is displayed:
  - Name (as page header)
  - State badge
  - Category and Difficulty badges
  - Duration
  - Visibility status
  - Description
  - Exercise list (if any)
  - Equipment section (shows "Coming soon")
  - Created/Updated timestamps

#### 4.3 State-Based Actions
- [ ] For DRAFT templates, verify these buttons are visible:
  - Edit
  - Move to Production
  - Delete
  - Duplicate
- [ ] For PRODUCTION templates, verify:
  - Edit button is visible
  - Archive button is visible
  - Delete button is NOT visible
  - Duplicate button is visible
- [ ] For ARCHIVED templates, verify:
  - Edit button is NOT visible
  - Delete button is visible
  - Duplicate button is visible

### 5. Template Editing

#### 5.1 Navigation
- [ ] Click "Edit" button from detail page or list view
- [ ] Verify URL changes to `/workout-templates/{id}/edit`
- [ ] Confirm form is pre-populated with existing data

#### 5.2 Field Restrictions by State
- [ ] For DRAFT templates: All fields should be editable
- [ ] For PRODUCTION templates:
  - Name should be read-only
  - Category should be read-only
  - Difficulty should be read-only
  - Duration, description, visibility should be editable
- [ ] For ARCHIVED templates: Edit should not be accessible

#### 5.3 Save Changes
- [ ] Modify editable fields
- [ ] Click "Save"
- [ ] Verify success notification
- [ ] Confirm changes are reflected in detail view

#### 5.4 Data Recovery
- [ ] Make changes to form
- [ ] Refresh browser without saving
- [ ] Verify recovery dialog appears: "Recover unsaved changes?"
- [ ] Click "Recover" and verify form data is restored
- [ ] Test "Discard" option on another attempt

### 6. State Transitions

#### 6.1 Draft to Production
- [ ] Open a DRAFT template
- [ ] Click "Move to Production"
- [ ] Verify confirmation dialog appears
- [ ] Confirm action
- [ ] Verify:
  - Success notification appears
  - Template state changes to PRODUCTION
  - Edit restrictions are applied
  - Delete button disappears

#### 6.2 Production to Archived
- [ ] Open a PRODUCTION template
- [ ] Click "Archive"
- [ ] Verify confirmation dialog
- [ ] Confirm action
- [ ] Verify:
  - Template state changes to ARCHIVED
  - Edit button disappears
  - Delete button appears

#### 6.3 Invalid Transitions
- [ ] Verify ARCHIVED templates cannot be moved to other states
- [ ] Confirm no invalid state transition buttons appear

### 7. Template Duplication

#### 7.1 Duplicate from Detail View
- [ ] Open any template
- [ ] Click "Duplicate"
- [ ] Verify redirect to edit form
- [ ] Confirm:
  - Name is modified with "(Copy)" suffix
  - All other fields match original
  - State is set to DRAFT
- [ ] Save duplicated template
- [ ] Verify both original and copy exist in list

### 8. Template Deletion

#### 8.1 Delete Draft Template
- [ ] Open a DRAFT template
- [ ] Click "Delete"
- [ ] Verify confirmation dialog: "Are you sure you want to delete this template?"
- [ ] Click "Cancel" and verify nothing happens
- [ ] Click "Delete" again and confirm
- [ ] Verify:
  - Success notification appears
  - Redirect to list view
  - Template no longer appears in list

#### 8.2 Delete Archived Template
- [ ] Repeat deletion test with ARCHIVED template
- [ ] Verify same behavior

#### 8.3 Cannot Delete Production
- [ ] Open a PRODUCTION template
- [ ] Verify Delete button is NOT present

### 9. Error Handling

#### 9.1 Network Errors
- [ ] Disconnect network or stop API
- [ ] Try to load template list
- [ ] Verify error message appears with "Retry" button
- [ ] Restore connection and click "Retry"
- [ ] Confirm data loads successfully

#### 9.2 Validation Errors
- [ ] Try to save template with duplicate name
- [ ] Verify error message is clear and helpful
- [ ] Fix error and verify save succeeds

#### 9.3 Concurrent Edit (409 Conflict)
- [ ] Open same template in two browser tabs
- [ ] Edit and save in first tab
- [ ] Edit and try to save in second tab
- [ ] Verify conflict error message appears
- [ ] Reload and verify latest changes are shown

### 10. Loading States

#### 10.1 List Loading
- [ ] Navigate to template list
- [ ] Verify skeleton loaders appear while data loads
- [ ] Confirm smooth transition when data appears

#### 10.2 Form Loading
- [ ] Open edit form
- [ ] Verify loading indicators while fetching template data
- [ ] Confirm form appears smoothly when loaded

### 11. Success Notifications

#### 11.1 CRUD Operations
- [ ] Create template - verify "Template created successfully"
- [ ] Update template - verify "Template updated successfully"
- [ ] Delete template - verify "Template deleted successfully"
- [ ] Duplicate template - verify appropriate success message

#### 11.2 State Transitions
- [ ] Move to Production - verify success message
- [ ] Archive template - verify success message

### 12. Keyboard Navigation

#### 12.1 Basic Navigation
- [ ] Use Tab key to navigate through form fields
- [ ] Verify focus indicators are visible
- [ ] Use Enter key to submit forms
- [ ] Use Escape key to close dialogs

#### 12.2 Dropdown Navigation
- [ ] Use arrow keys in dropdown menus
- [ ] Press Enter to select option
- [ ] Press Escape to close dropdown

### 13. Responsive Design (if implemented)

#### 13.1 Mobile View (< 768px)
- [ ] Verify layout adjusts for mobile
- [ ] Check touch interactions work
- [ ] Confirm all features remain accessible

#### 13.2 Tablet View (768px - 1024px)
- [ ] Verify appropriate layout adjustments
- [ ] Test both portrait and landscape orientations

### 14. Performance

#### 14.1 Search Performance
- [ ] Type quickly in search box
- [ ] Verify debouncing prevents excessive API calls
- [ ] Confirm results update smoothly

#### 14.2 Pagination Performance
- [ ] Navigate between pages
- [ ] Verify quick loading times
- [ ] Check no unnecessary full page refreshes

## Known Limitations & Placeholders

### Equipment Information
- Equipment aggregation is not yet implemented in the API
- UI shows "Equipment information coming soon" placeholder
- This is expected behavior

### Exercise Suggestions
- AI-powered exercise suggestions feature is postponed
- No UI elements for suggestions should be visible
- This is expected behavior

## Bug Reporting

If you encounter any issues:
1. Note the exact steps to reproduce
2. Check browser console for errors
3. Take screenshots if applicable
4. Report with:
   - Browser and version
   - User role (PT-Tier/Admin-Tier)
   - Exact error messages
   - Steps to reproduce

## Test Completion

After completing all test scenarios:
- [ ] All core functionality works as expected
- [ ] No critical bugs found
- [ ] User experience is smooth and intuitive
- [ ] Error messages are helpful
- [ ] Loading states provide good feedback
- [ ] The feature is ready for production use