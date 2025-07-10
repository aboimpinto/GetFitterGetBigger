# Exercise Weight Type Feature

## Overview

The Exercise Weight Type feature adds crucial metadata to exercises that defines how they handle weight assignments. This prevents logical inconsistencies in workout creation, such as attempting to add external weight to bodyweight-only exercises like Air Squats.

## Business Problem

Currently, the system allows users to assign weight to any exercise, regardless of whether it makes logical sense. This can lead to:
- User confusion when creating workouts
- Invalid workout prescriptions
- Poor user experience in the workout builder interface
- Inconsistent data that complicates progress tracking

## Solution

Add an `ExerciseWeightType` field to the Exercise entity that categorizes how each exercise handles weight assignments.

## Exercise Weight Type Categories

### 1. BodyweightOnly
Exercises that cannot have external weight added.
- Examples: Air Squat, Regular Push-up, Mountain Climbers, Burpees
- UI Behavior: Weight input field is hidden
- Validation: Reject any weight assignment attempts

### 2. BodyweightOptional
Exercises that can be performed with or without additional weight.
- Examples: Pull-ups, Dips, Push-ups (can be weighted), Lunges
- UI Behavior: Shows optional weight field with "+" prefix to indicate additional weight
- Validation: Accept null/zero or positive weight values

### 3. WeightRequired
Exercises that must have external weight specified.
- Examples: Barbell Squat, Dumbbell Press, Deadlift, Bench Press
- UI Behavior: Weight input field is required
- Validation: Reject if weight is null or zero

### 4. MachineWeight
Exercises performed on machines with weight stacks or plates.
- Examples: Leg Press, Cable Row, Lat Pulldown
- UI Behavior: Shows weight field with machine-specific units
- Validation: Accept positive weight values

### 5. NoWeight
Exercises that don't use weight as a metric.
- Examples: Plank (time-based), Stretches, Mobility exercises
- UI Behavior: Weight input field is hidden
- Validation: Reject any weight assignment attempts

## Implementation Requirements

### Database Changes

1. **Add to Exercise Table:**
   ```sql
   ALTER TABLE Exercises ADD COLUMN ExerciseWeightTypeId INT;
   ALTER TABLE Exercises ADD CONSTRAINT FK_Exercise_ExerciseWeightType 
     FOREIGN KEY (ExerciseWeightTypeId) REFERENCES ExerciseWeightTypes(Id);
   ```

2. **Create Reference Table:**
   ```sql
   CREATE TABLE ExerciseWeightTypes (
     Id INT PRIMARY KEY,
     Code VARCHAR(50) NOT NULL UNIQUE,
     Name VARCHAR(100) NOT NULL,
     Description TEXT,
     UIBehavior TEXT,
     ValidationRules TEXT,
     IsActive BOOLEAN DEFAULT TRUE,
     SortOrder INT,
     CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
     UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
   );
   ```

### Reference Data

This is a fixed reference table that should not grow dynamically. The five categories cover all possible exercise weight scenarios. Any new requirements should be carefully evaluated before considering additions.

### Integration Points

1. **Exercise Creation/Editing:**
   - Admin must select ExerciseWeightType when creating/editing exercises
   - Default to most appropriate type based on exercise category

2. **Workout Template Creation:**
   - SetGroup validation must respect ExerciseWeightType rules
   - UI must adapt based on selected exercise's weight type

3. **Workout Logging:**
   - Enforce weight type rules when users log actual performance
   - Provide appropriate UI feedback

4. **API Endpoints:**
   - Include ExerciseWeightType in exercise responses
   - Validate weight assignments in workout creation endpoints

### Migration Strategy

1. **Phase 1: Add Infrastructure**
   - Create ExerciseWeightTypes reference table
   - Add ExerciseWeightTypeId column to Exercises (nullable initially)

2. **Phase 2: Categorize Existing Exercises**
   - Analyze existing exercises and assign appropriate weight types
   - Use smart defaults based on exercise names and categories
   - Manual review for edge cases

3. **Phase 3: Enforce Rules**
   - Make ExerciseWeightTypeId non-nullable
   - Implement validation in SetGroup
   - Update UI components

### UI/UX Considerations

1. **Exercise Selection:**
   - Show weight type icon/badge next to exercise name
   - Group exercises by weight type in selection lists

2. **Weight Input:**
   - Dynamic form fields based on weight type
   - Clear labeling (e.g., "Additional Weight" for BodyweightOptional)
   - Appropriate input constraints (min/max values)

3. **Validation Feedback:**
   - Clear error messages when rules are violated
   - Contextual help text explaining weight type restrictions

## Success Metrics

1. Reduction in user-reported workout creation errors
2. Decreased support tickets related to weight assignment confusion
3. Improved data quality in workout logs
4. Faster workout creation times due to intelligent UI

## Future Considerations

1. **Smart Defaults:** Use machine learning to suggest appropriate weights based on user history and exercise type
2. **Progressive Overload:** Weight type awareness in progression algorithms
3. **Equipment Integration:** Link weight types to available equipment in user's gym profile