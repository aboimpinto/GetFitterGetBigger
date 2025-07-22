# Workout Template Core Unit Tests Specifications

## Overview
This document defines unit test specifications for the Workout Template Core feature, organized by component and functionality.

## Test Categories

### 1. WorkoutTemplate Entity Tests

#### Template Creation Tests
- **Test**: Create valid workout template
  - **Given**: Valid template data with all required fields
  - **When**: Template is created
  - **Then**: Template saved with generated ID and timestamps

- **Test**: Create template with missing required fields
  - **Given**: Template data missing name or category
  - **When**: Attempt to create template
  - **Then**: Validation error returned with field details

- **Test**: Create template with invalid duration
  - **Given**: Duration less than 10 or greater than 240 minutes
  - **When**: Attempt to create template
  - **Then**: Validation error for duration range

- **Test**: Create template with invalid difficulty level
  - **Given**: Difficulty level not in allowed values
  - **When**: Attempt to create template
  - **Then**: Validation error for difficulty level

#### Template Update Tests
- **Test**: Update template properties
  - **Given**: Existing template
  - **When**: Update name, description, or settings
  - **Then**: Changes saved with updated timestamp

- **Test**: Update template with version conflict
  - **Given**: Outdated version number
  - **When**: Attempt to update
  - **Then**: Conflict error returned

- **Test**: Update inactive template
  - **Given**: Template marked as inactive
  - **When**: Attempt to update
  - **Then**: Error indicating template is inactive

### 2. WorkoutTemplateExercise Tests

#### Exercise Addition Tests
- **Test**: Add exercise to valid zone
  - **Given**: Template and valid exercise
  - **When**: Add to Warmup, Main, or Cooldown zone
  - **Then**: Exercise added with correct sequence

- **Test**: Add exercise with duplicate sequence
  - **Given**: Existing exercise at sequence position
  - **When**: Attempt to add at same position
  - **Then**: Validation error for duplicate sequence

- **Test**: Add exercise to invalid zone
  - **Given**: Zone value not in allowed list
  - **When**: Attempt to add exercise
  - **Then**: Validation error for invalid zone

#### Exercise Ordering Tests
- **Test**: Maintain sequence order within zone
  - **Given**: Multiple exercises in zone
  - **When**: Query exercises
  - **Then**: Returned in sequence order

- **Test**: Reorder exercises in zone
  - **Given**: Exercises with sequences 1, 2, 3
  - **When**: Change order to 2, 1, 3
  - **Then**: New order persisted correctly

#### Exercise Removal Tests
- **Test**: Remove exercise from template
  - **Given**: Exercise in template
  - **When**: Remove exercise
  - **Then**: Exercise deleted, sequences adjusted

- **Test**: Remove exercise with warmup association
  - **Given**: Main exercise with linked warmup
  - **When**: Remove warmup
  - **Then**: Warning generated but removal allowed

### 3. SetConfiguration Tests

#### Configuration Creation Tests
- **Test**: Create standard protocol configuration
  - **Given**: Standard execution protocol
  - **When**: Configure sets and reps
  - **Then**: Configuration saved correctly

- **Test**: Create configuration with rep range
  - **Given**: Target reps as "8-12"
  - **When**: Save configuration
  - **Then**: Range format preserved

- **Test**: Create time-based configuration
  - **Given**: Exercise requiring duration
  - **When**: Set duration in seconds
  - **Then**: Duration saved, reps optional

#### Configuration Validation Tests
- **Test**: Validate positive set count
  - **Given**: Zero or negative sets
  - **When**: Attempt to save
  - **Then**: Validation error

- **Test**: Validate rep format
  - **Given**: Invalid rep format (e.g., "abc")
  - **When**: Attempt to save
  - **Then**: Format validation error

- **Test**: Require reps or duration
  - **Given**: Neither reps nor duration provided
  - **When**: Attempt to save
  - **Then**: Validation error requiring one

### 4. Business Logic Tests

#### Equipment Aggregation Tests
- **Test**: Aggregate equipment from exercises
  - **Given**: Exercises with different equipment
  - **When**: Query template equipment
  - **Then**: Complete unique equipment list

- **Test**: Update equipment on exercise change
  - **Given**: Template with exercises
  - **When**: Add/remove exercise
  - **Then**: Equipment list updated automatically

- **Test**: Handle exercises with no equipment
  - **Given**: Bodyweight exercises
  - **When**: Aggregate equipment
  - **Then**: Empty or "None" in list

#### Warmup/Cooldown Suggestion Tests
- **Test**: Suggest warmups for main exercise
  - **Given**: Main exercise with warmup associations
  - **When**: Add to main zone
  - **Then**: Warmup suggestions returned

- **Test**: Avoid duplicate warmup suggestions
  - **Given**: Warmup already in template
  - **When**: Add related main exercise
  - **Then**: No duplicate suggestion

- **Test**: Handle exercises without associations
  - **Given**: Exercise with no warmups defined
  - **When**: Add to template
  - **Then**: Empty suggestions list

#### Zone Validation Tests
- **Test**: Validate zone sequence rules
  - **Given**: Exercises in all zones
  - **When**: Validate template
  - **Then**: Warmup < Main < Cooldown enforced

- **Test**: Require main zone exercises
  - **Given**: Template with only warmup/cooldown
  - **When**: Validate template
  - **Then**: Error requiring main exercises

### 5. Authorization Tests

#### Access Control Tests
- **Test**: PT can create templates
  - **Given**: User with PT-Tier claim
  - **When**: Create template
  - **Then**: Template created successfully

- **Test**: Non-PT cannot create templates
  - **Given**: User with Free-Tier only
  - **When**: Attempt to create
  - **Then**: Authorization error

- **Test**: Users can view public templates
  - **Given**: Public template
  - **When**: Any authenticated user queries
  - **Then**: Template visible

- **Test**: Private template access restricted
  - **Given**: Private template
  - **When**: Non-owner queries
  - **Then**: Template not returned

#### Ownership Tests
- **Test**: Owner can modify template
  - **Given**: User owns template
  - **When**: Update template
  - **Then**: Update successful

- **Test**: Non-owner cannot modify
  - **Given**: User doesn't own template
  - **When**: Attempt update
  - **Then**: Authorization error

### 6. Data Validation Tests

#### String Length Tests
- **Test**: Validate name length
  - **Given**: Name over 100 characters
  - **When**: Save template
  - **Then**: Length validation error

- **Test**: Validate description length
  - **Given**: Description over 500 characters
  - **When**: Save template
  - **Then**: Length validation error

- **Test**: Validate tag constraints
  - **Given**: More than 10 tags or tag over 30 chars
  - **When**: Save template
  - **Then**: Tag validation error

#### Reference Data Tests
- **Test**: Validate category exists
  - **Given**: Invalid category ID
  - **When**: Create template
  - **Then**: Reference validation error

- **Test**: Validate objective exists
  - **Given**: Invalid objective ID
  - **When**: Create template
  - **Then**: Reference validation error

- **Test**: Validate protocol availability
  - **Given**: Unavailable protocol selected
  - **When**: Create template
  - **Then**: Availability validation error

### 7. Query and Filter Tests

#### List Query Tests
- **Test**: Filter by category
  - **Given**: Templates in different categories
  - **When**: Filter by specific category
  - **Then**: Only matching templates returned

- **Test**: Search by name and description
  - **Given**: Search term "strength"
  - **When**: Search templates
  - **Then**: Templates with term in name/description

- **Test**: Filter by difficulty
  - **Given**: Templates of all difficulties
  - **When**: Filter "Beginner"
  - **Then**: Only beginner templates

#### Pagination Tests
- **Test**: Page through results
  - **Given**: 50 templates, page size 20
  - **When**: Request page 2
  - **Then**: Items 21-40 returned

- **Test**: Handle last page correctly
  - **Given**: 45 templates, page size 20
  - **When**: Request page 3
  - **Then**: Items 41-45 returned

### 8. Edge Case Tests

#### Concurrent Modification Tests
- **Test**: Handle concurrent updates
  - **Given**: Two users editing same template
  - **When**: Both save changes
  - **Then**: Version conflict handled gracefully

#### Data Integrity Tests
- **Test**: Cascade delete behavior
  - **Given**: Template with exercises
  - **When**: Delete template
  - **Then**: Exercises and configs deleted

- **Test**: Maintain referential integrity
  - **Given**: Exercise referenced by template
  - **When**: Attempt to delete exercise
  - **Then**: Integrity constraint prevents deletion

#### Performance Tests
- **Test**: Handle large exercise counts
  - **Given**: Template with 50+ exercises
  - **When**: Load template
  - **Then**: Performance within limits

- **Test**: Efficient equipment aggregation
  - **Given**: Many exercises with equipment
  - **When**: Calculate equipment list
  - **Then**: No N+1 queries

## Test Data Requirements

### Reference Data
- Valid workout categories
- Valid workout objectives
- Valid execution protocols
- Sample exercises with associations

### User Data
- PT-tier user accounts
- Free-tier user accounts
- Admin user accounts

### Template Data
- Public templates
- Private templates
- Templates with various exercise counts
- Templates in different states

## Performance Benchmarks

### Response Time Targets
- Single template query: < 100ms
- Template list (20 items): < 200ms
- Template creation: < 300ms
- Exercise addition: < 150ms

### Load Test Scenarios
- 100 concurrent template reads
- 20 concurrent template creates
- 50 concurrent exercise updates
- 1000 templates in database