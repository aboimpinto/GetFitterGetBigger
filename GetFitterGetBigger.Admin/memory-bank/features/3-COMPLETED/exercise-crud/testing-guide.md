# Exercise CRUD Manual Testing Guide

## Prerequisites

1. Ensure the API is running at `http://localhost:5214/`
2. Ensure the Admin application is running
3. Login to the Admin application with appropriate credentials

## Testing Checklist

### 1. Exercise List Page (`/exercises`)

- [ ] Navigate to Exercises from the main navigation
- [ ] Verify the page loads without errors
- [ ] Check that the "New Exercise" button is visible

#### Filtering
- [ ] Test search by name (type and press Enter or click Apply)
- [ ] Test filter by difficulty level
- [ ] Test filter by muscle group
- [ ] Test clearing filters
- [ ] Verify filters persist when navigating back from detail/edit pages

#### Pagination (if multiple exercises exist)
- [ ] Test Previous/Next buttons
- [ ] Test direct page navigation
- [ ] Verify page count display is correct

### 2. Create New Exercise (`/exercises/new`)

- [ ] Click "New Exercise" button
- [ ] Verify form loads with all sections

#### Basic Information
- [ ] Enter exercise name (required)
- [ ] Enter description (required)
- [ ] Enter instructions (required)
- [ ] Test validation for empty fields

#### Exercise Configuration
- [ ] Select difficulty level (required)
- [ ] Toggle "Is Unilateral" checkbox

#### Muscle Groups (required)
- [ ] Add muscle group by clicking "Add Muscle Group"
- [ ] Select muscle group from dropdown
- [ ] Select role (Primary/Secondary/Stabilizer)
- [ ] Add multiple muscle groups
- [ ] Remove muscle groups (X button)
- [ ] Test validation - at least one complete muscle group required

#### Equipment (optional)
- [ ] Select multiple equipment items
- [ ] Deselect equipment items

#### Body Parts (optional)
- [ ] Select multiple body parts
- [ ] Deselect body parts

#### Movement Patterns (optional)
- [ ] Select multiple movement patterns
- [ ] Deselect movement patterns

#### Media URLs (optional)
- [ ] Add image URL
- [ ] Add video URL

#### Form Actions
- [ ] Test Cancel button (returns to list)
- [ ] Test Create Exercise button
- [ ] Verify successful creation redirects to list
- [ ] Verify new exercise appears in list

### 3. View Exercise Details (`/exercises/{id}`)

- [ ] Click on exercise name or "View" link from list
- [ ] Verify all exercise data displays correctly:
  - [ ] Name and difficulty badge
  - [ ] Unilateral badge (if applicable)
  - [ ] Description
  - [ ] Instructions
  - [ ] Muscle groups with role badges
  - [ ] Equipment list
  - [ ] Body parts list
  - [ ] Movement patterns list
  - [ ] Media links (if provided)
- [ ] Test "Edit" button
- [ ] Test "Delete" button (shows confirmation modal)
- [ ] Test "Back to Exercises" link

### 4. Edit Exercise (`/exercises/{id}/edit`)

- [ ] Navigate via Edit button or link
- [ ] Verify form pre-populates with existing data
- [ ] Modify exercise name
- [ ] Modify muscle groups
- [ ] Add/remove equipment
- [ ] Test validation still works
- [ ] Test Cancel button
- [ ] Test Update Exercise button
- [ ] Verify changes appear in list and detail views

### 5. Delete Exercise

- [ ] From detail page, click Delete button
- [ ] Verify confirmation modal appears
- [ ] Test Cancel button (closes modal)
- [ ] Test Delete button
- [ ] Verify exercise removed from list
- [ ] Verify redirect to list page

### 6. Error Handling

- [ ] Test creating exercise with duplicate name (if API validates)
- [ ] Test network errors (stop API and try operations)
- [ ] Verify error messages display and can be dismissed
- [ ] Test recovery after errors

### 7. Performance and UX

- [ ] Verify loading states appear during operations
- [ ] Check responsive design on mobile viewport
- [ ] Test keyboard navigation (Tab through form)
- [ ] Verify no console errors during operations

## Known Issues to Verify Fixed

1. **Button Click Events**: Ensure all buttons respond to clicks (fixed with @rendermode InteractiveServer)
2. **ID Format**: Verify all IDs use format `<entity>-<guid>` (e.g., `exercise-abc123`)
3. **Muscle Group Format**: Ensure muscle groups save with correct API format
4. **JSON Deserialization**: Verify no errors when loading exercise lists or details

## Test Data Examples

### Exercise 1: Barbell Squat
- Name: Barbell Squat
- Description: A compound lower body exercise targeting quadriceps, glutes, and hamstrings
- Instructions: 
  1. Position barbell on upper back
  2. Stand with feet shoulder-width apart
  3. Lower body by bending knees and hips
  4. Descend until thighs are parallel to floor
  5. Drive through heels to return to start
- Difficulty: Intermediate
- Is Unilateral: No
- Muscle Groups:
  - Quadriceps (Primary)
  - Glutes (Primary)
  - Hamstrings (Secondary)
  - Core (Stabilizer)
- Equipment: Barbell, Squat Rack
- Body Parts: Legs, Glutes
- Movement Patterns: Squat

### Exercise 2: Bulgarian Split Squat
- Name: Bulgarian Split Squat
- Description: Unilateral leg exercise for strength and balance
- Instructions:
  1. Place rear foot on bench behind you
  2. Lower into lunge position
  3. Drive through front heel to return
- Difficulty: Intermediate
- Is Unilateral: Yes
- Muscle Groups:
  - Quadriceps (Primary)
  - Glutes (Secondary)
- Equipment: Bench
- Body Parts: Legs
- Movement Patterns: Lunge

## Reporting Issues

Document any issues found with:
1. Steps to reproduce
2. Expected behavior
3. Actual behavior
4. Any error messages
5. Browser console errors