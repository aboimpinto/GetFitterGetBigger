# ExerciseWeightType Migration Guide

## Overview

This guide explains how the ExerciseWeightType feature affects existing data and provides guidance for migrating existing exercises to use the new weight type system.

## Database Changes

### New Table: ExerciseWeightTypes

A new reference table has been added with the following predefined values:

| ID | Code | Value | Description |
|----|------|-------|-------------|
| b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a | BODYWEIGHT_ONLY | Bodyweight Only | Exercises that only use bodyweight with no option for additional weight |
| a1b3c5d7-5b7c-4d8e-9f0a-1b2c3d4e5f6a | BODYWEIGHT_OPTIONAL | Bodyweight Optional | Exercises that can be performed with just bodyweight or with added weight |
| c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a | WEIGHT_REQUIRED | Weight Required | Exercises that must have external weight specified |
| d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b | MACHINE_WEIGHT | Machine Weight | Exercises performed on machines with weight stacks or plates |
| e5d7a8b9-9d0e-8f1a-2b3c-4d5e6f7a8b9c | NO_WEIGHT | No Weight | Exercises that don't involve weight (stretching, mobility, etc.) |

### Exercises Table Changes

- Added column: `ExerciseWeightTypeId` (UUID, nullable)
- Added foreign key constraint to ExerciseWeightTypes table

## Migration Strategy

### Phase 1: Database Migration (Automatic)

The Entity Framework migration will:
1. Create the ExerciseWeightTypes table
2. Seed the 5 predefined weight types
3. Add the ExerciseWeightTypeId column to Exercises table (nullable)
4. All existing exercises will have NULL ExerciseWeightTypeId initially

### Phase 2: Data Classification (Manual - Recommended)

While not required, it's recommended to classify existing exercises by weight type for better data consistency and future weight validation.

#### SQL Script for Common Exercise Classifications

```sql
-- Bodyweight Only Exercises
UPDATE Exercises 
SET ExerciseWeightTypeId = 'b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a'
WHERE LOWER(Name) IN (
    'push-up', 'push up', 'pushup',
    'sit-up', 'sit up', 'situp',
    'crunch', 'crunches',
    'plank', 'planks',
    'burpee', 'burpees',
    'mountain climber', 'mountain climbers',
    'jumping jack', 'jumping jacks'
)
AND ExerciseWeightTypeId IS NULL;

-- Bodyweight Optional Exercises
UPDATE Exercises 
SET ExerciseWeightTypeId = 'a1b3c5d7-5b7c-4d8e-9f0a-1b2c3d4e5f6a'
WHERE LOWER(Name) IN (
    'pull-up', 'pull up', 'pullup', 'chin-up', 'chin up', 'chinup',
    'dip', 'dips',
    'pistol squat', 'pistol squats',
    'muscle-up', 'muscle up', 'muscleup',
    'handstand push-up', 'handstand push up'
)
AND ExerciseWeightTypeId IS NULL;

-- Weight Required Exercises
UPDATE Exercises 
SET ExerciseWeightTypeId = 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a'
WHERE (
    LOWER(Name) LIKE '%barbell%' OR
    LOWER(Name) LIKE '%dumbbell%' OR
    LOWER(Name) LIKE '%kettlebell%' OR
    LOWER(Name) IN ('deadlift', 'squat', 'bench press', 'overhead press', 'row')
)
AND ExerciseWeightTypeId IS NULL;

-- Machine Weight Exercises
UPDATE Exercises 
SET ExerciseWeightTypeId = 'd4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b'
WHERE (
    LOWER(Name) LIKE '%machine%' OR
    LOWER(Name) LIKE '%cable%' OR
    LOWER(Name) LIKE '%smith%' OR
    LOWER(Name) LIKE 'leg press%' OR
    LOWER(Name) LIKE 'lat pulldown%' OR
    LOWER(Name) LIKE 'seated row%'
)
AND ExerciseWeightTypeId IS NULL;

-- No Weight Exercises (REST type or mobility work)
UPDATE Exercises 
SET ExerciseWeightTypeId = 'e5d7a8b9-9d0e-8f1a-2b3c-4d5e6f7a8b9c'
WHERE ExerciseTypeId IN (
    SELECT Id FROM ExerciseTypes WHERE Code = 'REST'
)
OR LOWER(Name) LIKE '%stretch%'
OR LOWER(Name) LIKE '%mobility%'
OR LOWER(Name) LIKE '%foam roll%'
AND ExerciseWeightTypeId IS NULL;
```

#### Manual Review Required

After running the automated classification, manually review and update exercises that:
1. Were not matched by the script
2. May have been incorrectly classified
3. Have ambiguous names that could fit multiple categories

### Phase 3: Application Updates

1. **API Responses**: Exercise endpoints now include `exerciseWeightType` in responses
2. **Exercise Creation/Update**: New optional field `exerciseWeightTypeId` in requests
3. **Future Weight Validation**: When workout logging is implemented, weight validation will be enforced based on the exercise's weight type

## Backward Compatibility

- Exercises with NULL ExerciseWeightTypeId will continue to work normally
- No weight validation is enforced for exercises without a weight type
- Existing API consumers don't need immediate updates
- The exerciseWeightType field in responses will be null for unclassified exercises

## Rollback Procedure

If needed, the migration can be rolled back:

```sql
-- Remove foreign key constraint
ALTER TABLE Exercises DROP CONSTRAINT FK_Exercises_ExerciseWeightTypes_ExerciseWeightTypeId;

-- Remove the column
ALTER TABLE Exercises DROP COLUMN ExerciseWeightTypeId;

-- Drop the ExerciseWeightTypes table
DROP TABLE ExerciseWeightTypes;
```

## Best Practices

1. **Gradual Migration**: Don't rush to classify all exercises at once
2. **Test First**: Test weight type assignments with a small set of exercises
3. **User Communication**: Inform users about the new weight validation rules before enforcement
4. **Monitor Logs**: Watch for validation errors when weight enforcement begins
5. **Provide Defaults**: Consider setting sensible defaults for common exercise patterns

## Timeline Recommendations

1. **Week 1-2**: Deploy migration, monitor for issues
2. **Week 3-4**: Begin classifying high-usage exercises
3. **Month 2**: Complete classification of active exercises
4. **Month 3**: Enable weight validation with warnings only
5. **Month 4**: Fully enforce weight validation rules

## Support Queries

### Find Unclassified Exercises
```sql
SELECT Id, Name, Description 
FROM Exercises 
WHERE ExerciseWeightTypeId IS NULL 
AND IsActive = 1
ORDER BY Name;
```

### Count Exercises by Weight Type
```sql
SELECT 
    ewt.Value as WeightType,
    COUNT(e.Id) as ExerciseCount
FROM ExerciseWeightTypes ewt
LEFT JOIN Exercises e ON e.ExerciseWeightTypeId = ewt.Id
GROUP BY ewt.Id, ewt.Value
UNION
SELECT 
    'Unclassified' as WeightType,
    COUNT(*) as ExerciseCount
FROM Exercises
WHERE ExerciseWeightTypeId IS NULL;
```

### Find Potential Misclassifications
```sql
-- Exercises with "barbell" in name but not marked as weight required
SELECT Id, Name, ewt.Value as CurrentWeightType
FROM Exercises e
LEFT JOIN ExerciseWeightTypes ewt ON e.ExerciseWeightTypeId = ewt.Id
WHERE LOWER(e.Name) LIKE '%barbell%'
AND (e.ExerciseWeightTypeId != 'c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a' OR e.ExerciseWeightTypeId IS NULL);
```