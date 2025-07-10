# Exercise Weight Type Admin Workflows

## Overview
This document describes the user workflows for Personal Trainers managing exercises with weight types and creating workouts with proper weight assignments.

## Primary Workflows

### 1. Creating a New Exercise with Weight Type

**User Story**: As a Personal Trainer, I want to create a new exercise and specify how it handles weight so that workouts use appropriate weight assignments.

**Steps**:
1. Navigate to Exercise Management section
2. Click "Add New Exercise" button
3. Fill in exercise details:
   - Enter exercise name (e.g., "Bulgarian Split Squat")
   - Add description
   - **Select Weight Type** from dropdown:
     - Review options with descriptions
     - Choose "Bodyweight Optional" for this example
   - Select muscle groups
   - Choose required equipment
4. Save exercise
5. System validates all required fields including weight type
6. Success message confirms exercise created

**Expected Outcome**: Exercise is created with proper weight type classification, ready for use in workouts

**Decision Points**:
- If unsure about weight type, hover over info icon for descriptions
- Consider how clients will perform the exercise
- Think about progression possibilities

### 2. Bulk Updating Exercise Weight Types

**User Story**: As a Personal Trainer, I need to update multiple exercises with correct weight types after system migration.

**Steps**:
1. Navigate to Exercise Management
2. Click "Bulk Actions" → "Update Weight Types"
3. System shows exercises without weight types or needing review
4. For each exercise group:
   - Select multiple exercises using checkboxes
   - Choose appropriate weight type from dropdown
   - Click "Apply to Selected"
5. Review changes in preview panel
6. Click "Save All Changes"
7. System processes updates and shows progress
8. Review summary of updated exercises

**Expected Outcome**: Multiple exercises updated efficiently with appropriate weight types

**Batch Selection Helpers**:
- "Select all Barbell exercises" → Auto-assigns WEIGHT_REQUIRED
- "Select all Bodyweight exercises" → Auto-assigns BODYWEIGHT_ONLY
- "Select all Machine exercises" → Auto-assigns MACHINE_WEIGHT

### 3. Creating a Workout with Weight-Type-Aware Exercises

**User Story**: As a Personal Trainer, I want to create a workout where each exercise has appropriate weight assignments based on its type.

**Steps**:
1. Navigate to Workout Builder
2. Create new workout or edit existing
3. Add exercises to workout:
   - Search and select "Push-ups" (BODYWEIGHT_ONLY)
   - Weight field is hidden automatically
   - Add sets and reps only
4. Add weighted exercise:
   - Select "Barbell Squat" (WEIGHT_REQUIRED)
   - Weight field appears with required indicator
   - Enter weight value (must be > 0)
   - Add sets and reps
5. Add optional weight exercise:
   - Select "Pull-ups" (BODYWEIGHT_OPTIONAL)
   - Weight field shows with "+" prefix
   - Can leave empty or add weight for progression
6. Save workout
7. System validates all weight assignments

**Expected Outcome**: Workout created with valid weight assignments for each exercise type

**Validation Scenarios**:
- Attempting to add weight to push-ups → Error: "Bodyweight exercises cannot have external weight"
- Saving barbell squat without weight → Error: "Weight is required for this exercise"
- Pull-ups can be saved with or without weight → Success

### 4. Filtering Exercises by Weight Type

**User Story**: As a Personal Trainer, I want to find all exercises of a specific weight type when building specialized workouts.

**Steps**:
1. Navigate to Exercise Library
2. Open filter panel
3. Expand "Weight Type" filter section
4. Select desired weight types:
   - Check "Bodyweight Only" for calisthenics workout
   - Check "Bodyweight Optional" for progression options
5. Apply filters
6. View filtered exercise list with weight type badges
7. Select exercises for workout

**Expected Outcome**: Filtered list shows only exercises matching selected weight types

**Common Filter Combinations**:
- Home workout: BODYWEIGHT_ONLY + BODYWEIGHT_OPTIONAL + NO_WEIGHT
- Gym workout: WEIGHT_REQUIRED + MACHINE_WEIGHT + BODYWEIGHT_OPTIONAL
- Rehabilitation: NO_WEIGHT + BODYWEIGHT_ONLY

### 5. Understanding Weight Type Impact

**User Story**: As a Personal Trainer, I want to understand how weight types affect workout creation to avoid errors.

**Steps**:
1. Access Exercise Weight Type reference
2. View information panel showing:
   - All weight types with descriptions
   - Validation rules for each type
   - Example exercises for each type
3. Click on specific weight type for details:
   - See detailed description
   - View validation rules
   - See UI behavior explanation
4. Use this knowledge when creating exercises

**Reference Quick Guide**:
| Weight Type | When to Use | Weight Input |
|-------------|-------------|--------------|
| BODYWEIGHT_ONLY | Pure bodyweight movements | Hidden |
| BODYWEIGHT_OPTIONAL | Can progress with weight | Optional |
| WEIGHT_REQUIRED | Must use external load | Required |
| MACHINE_WEIGHT | Gym machine exercises | Required |
| NO_WEIGHT | Stretches, mobility | Hidden |

## Error Recovery Workflows

### Incorrect Weight Type Assignment

**Scenario**: Exercise created with wrong weight type

**Steps**:
1. Navigate to exercise details
2. Click "Edit Exercise"
3. Change weight type selection
4. System warns if exercise is used in active workouts
5. Confirm change understanding impact
6. Save changes
7. System updates exercise and notifies about affected workouts

### Invalid Weight in Workout

**Scenario**: Workout template has invalid weight for exercise type

**Steps**:
1. Error appears when saving workout
2. System highlights problematic exercise
3. Shows specific error (e.g., "Weight required for Barbell Squat")
4. Trainer enters valid weight
5. Resubmit workout
6. Success confirmation

## Advanced Workflows

### 6. Exercise Migration Workflow

**User Story**: As an Admin, I need to assign weight types to all existing exercises in the system.

**Steps**:
1. Access Admin → Exercise Migration
2. View migration dashboard:
   - Total exercises: 500
   - Without weight type: 500
   - Migration progress: 0%
3. Use automated suggestions:
   - System suggests weight types based on exercise names
   - Review suggestions in batches
   - Approve or modify suggestions
4. Handle exceptions manually:
   - Filter to "Needs Review" exercises
   - Assign weight types individually
5. Run validation check:
   - System verifies all exercises have valid weight types
   - Shows any conflicts or issues
6. Complete migration
7. Enable weight type enforcement system-wide

**Expected Outcome**: All exercises properly classified, system ready for weight validation

### 7. Creating Exercise Templates by Weight Type

**User Story**: As a Personal Trainer, I want to create workout templates organized by weight types for different training scenarios.

**Steps**:
1. Navigate to Workout Templates
2. Create new template category "Home Workouts"
3. Filter exercises to BODYWEIGHT_ONLY + BODYWEIGHT_OPTIONAL
4. Build template with these exercises
5. Create "Gym Strength" category
6. Filter to WEIGHT_REQUIRED + MACHINE_WEIGHT
7. Build appropriate template
8. Save templates with clear naming

**Template Organization**:
- Bodyweight Basics (BODYWEIGHT_ONLY)
- Progressive Calisthenics (BODYWEIGHT_OPTIONAL)
- Strength Training (WEIGHT_REQUIRED)
- Machine Circuit (MACHINE_WEIGHT)
- Flexibility Routine (NO_WEIGHT)

## Best Practices

### For Personal Trainers

1. **Understand Each Weight Type**
   - Review weight type descriptions before assigning
   - Consider client's training environment
   - Think about exercise progression

2. **Consistent Classification**
   - Similar exercises should have same weight type
   - Maintain standards across exercise library
   - Document any special cases

3. **Workout Creation Efficiency**
   - Filter by weight type for faster exercise selection
   - Use weight type badges for quick identification
   - Understand validation rules to avoid errors

### For Administrators

1. **Migration Planning**
   - Review all exercises before bulk updates
   - Test with small batches first
   - Document any custom decisions

2. **Quality Control**
   - Regularly audit exercise weight types
   - Ensure new exercises get proper classification
   - Monitor for inconsistencies

3. **Training Support**
   - Create guide for trainers on weight types
   - Provide examples for edge cases
   - Maintain FAQ for common questions