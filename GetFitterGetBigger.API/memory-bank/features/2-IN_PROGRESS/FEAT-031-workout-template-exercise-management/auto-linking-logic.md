# Auto-Linking Logic: Warmup/Cooldown Management

This document defines how warmup and cooldown exercises are automatically managed when adding or removing workout exercises.

## Overview

When a PT adds a workout exercise, the system automatically adds its associated warmup and cooldown exercises (if defined in ExerciseLinks). When removing a workout exercise, orphaned warmup/cooldown exercises are cleaned up.

**IMPORTANT CLARIFICATIONS**:
- Warmup exercises CANNOT link to other warmup exercises (no recursive warmup chains)
- A workout exercise CAN appear as its own warmup (e.g., Squats as both warmup and workout)
- No circular dependency issues because warmups don't have their own warmups
- Auto-linking is one-directional: Workout → Warmup/Cooldown only

## Add Exercise Flow

### When Adding a Workout Exercise

1. **User Action**: PT adds exercise to Workout phase
2. **System Queries ExerciseLinks**:
   ```sql
   SELECT * FROM ExerciseLinks 
   WHERE SourceExerciseId = @exerciseId 
   AND LinkType IN ('WARMUP', 'COOLDOWN')
   ```
3. **Check Existing Exercises**:
   ```sql
   SELECT * FROM WorkoutTemplateExercise 
   WHERE WorkoutTemplateId = @templateId 
   AND ExerciseId IN (@linkedExerciseIds)
   ```
4. **Add Missing Exercises**:
   - Add to appropriate phase (Warmup or Cooldown)
   - Use empty metadata `{}` (PT must configure)
   - Determine order (max + 1 in round)

### When Adding Warmup/Cooldown Exercise

- No auto-linking occurs
- Only the specified exercise is added
- PT has full control

## Remove Exercise Flow

### When Removing a Workout Exercise

1. **Identify Exercise**: Find by GUID
2. **Find Linked Exercises**:
   ```sql
   SELECT TargetExerciseId FROM ExerciseLinks 
   WHERE SourceExerciseId = @exerciseId 
   AND LinkType IN ('WARMUP', 'COOLDOWN')
   ```
3. **Check If Still Needed**:
   ```sql
   -- Find if ANY other workout exercise uses these warmup/cooldown
   SELECT DISTINCT el.TargetExerciseId
   FROM WorkoutTemplateExercise wte
   JOIN ExerciseLinks el ON wte.ExerciseId = el.SourceExerciseId
   WHERE wte.WorkoutTemplateId = @templateId 
     AND wte.Phase = 'Workout'
     AND wte.Id != @exerciseBeingRemoved
     AND el.TargetExerciseId IN (@linkedExerciseIds)
   ```
4. **Remove Orphans**: Delete warmup/cooldown not used by any other workout exercise
5. **Re-sequence**: Update OrderInRound for remaining exercises

### When Removing Warmup/Cooldown Exercise

- Simply remove the exercise
- No cascade logic needed
- Re-sequence remaining exercises in round

## Example Scenarios

### Core Scenarios with Shared Warmup

Assume: Both Squats and Lunges link to Jumping Jacks as warmup

#### Scenario 1: Manual Add Before Auto-Link
```
1. PT manually adds "Jumping Jacks" to warmup
2. PT adds "Squats" to workout → System detects "Jumping Jacks" already exists, skips auto-add
3. PT removes "Squats" → "Jumping Jacks" stays (was manually added)
```

#### Scenario 2: Auto-Link Then Share
```
1. PT adds "Squats" to workout → Auto-adds "Jumping Jacks" to warmup
2. PT adds "Lunges" to workout → "Jumping Jacks" already exists, skip auto-add
3. PT removes "Squats" → "Jumping Jacks" stays (still needed by "Lunges")
4. PT removes "Lunges" → "Jumping Jacks" removed (no longer needed)
```

#### Scenario 3: Add, Remove, Re-add Pattern
```
1. PT adds "Squats" → Auto-adds "Jumping Jacks"
2. PT manually removes "Jumping Jacks"
3. PT adds "Lunges" → Auto-adds "Jumping Jacks" again
4. PT manually removes "Jumping Jacks" again
```

### Additional Complex Scenarios

#### Scenario 4: Multiple Exercises Share Warmup
```
1. Add "Barbell Squat" → Auto-adds "Leg Swings" to warmup
2. Add "Barbell Lunges" → "Leg Swings" already exists, skip
3. Remove "Barbell Squat" → "Leg Swings" still needed by "Barbell Lunges", keep
4. Remove "Barbell Lunges" → "Leg Swings" now orphaned, remove
```

#### Scenario 5: Complex Linkage
```
Exercise Links:
- Barbell Squat → Leg Swings (warmup), Pigeon Pose (cooldown)
- Deadlift → Back Extensions (warmup), Pigeon Pose (cooldown)

Actions:
1. Add Barbell Squat → Adds Leg Swings, Pigeon Pose
2. Add Deadlift → Adds Back Extensions, Pigeon Pose already exists
3. Remove Barbell Squat → Removes Leg Swings, keeps Pigeon Pose (used by Deadlift)
```

### Important Notes

1. **Manual vs Auto Detection**: System doesn't track whether exercise was manually or automatically added
2. **Orphan Detection**: Based solely on whether any workout exercise currently needs the warmup/cooldown
3. **Multiple Rounds**: Same warmup exercise can be added to different rounds if needed
4. **Empty Metadata**: Auto-added exercises start with `{}` metadata - PT must configure

## Implementation Details

### Service Method Signatures

```csharp
private async Task<List<ExerciseDto>> GetLinkedExercisesAsync(int exerciseId)
{
    // Query ExerciseLinks for warmup/cooldown
}

private async Task<List<ExerciseDto>> GetOrphanedExercisesAsync(
    int templateId, 
    Guid removedExerciseId, 
    List<int> linkedExerciseIds)
{
    // Find exercises not used by any other workout exercise
}

private async Task AutoAddLinkedExercisesAsync(
    int templateId, 
    int workoutExerciseId,
    string phase,
    int roundNumber)
{
    // Add missing warmup/cooldown exercises
}
```

### Transaction Management

All operations must be atomic:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Add main exercise
    // Add linked exercises
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Business Rules

1. **No Duplicates**: Never add an exercise that already exists in the phase
2. **Empty Metadata**: Auto-added exercises start with `{}` metadata
3. **Order Management**: Auto-added exercises go to end of round
4. **Phase Appropriate**: Warmup links → Warmup phase, Cooldown links → Cooldown phase
5. **Orphan Cleanup**: Only remove if not used by ANY workout exercise

## Edge Cases

### Self-Referencing Exercise
- Exercise A can link to itself as warmup (e.g., bodyweight squats as warmup for weighted squats)
- **Solution**: This is valid and supported - add normally

### Missing Exercise
- ExerciseLink references non-existent exercise
- **Solution**: Skip and log warning

### Multiple Link Types
- Same exercise linked as both warmup and cooldown
- **Solution**: Add to both phases if not already present

### No Circular Dependencies
- **Not Possible**: Since warmup exercises don't have their own warmups, circular chains cannot occur
- **Example**: Even if Squats → Lunges (warmup) and Lunges → Squats (warmup), no issue because warmups don't trigger auto-linking

## Performance Considerations

1. **Batch Queries**: Get all links in one query
2. **Cached Data**: ExerciseLinks cached for 10 minutes
3. **Single Transaction**: All adds/removes in one transaction
4. **Indexed Lookups**: Use indexes on WorkoutTemplateId, ExerciseId

## Testing Scenarios

1. **Add workout exercise with no links** → Only main exercise added
2. **Add workout exercise with warmup/cooldown** → All three added
3. **Add second workout with same warmup** → Warmup not duplicated
4. **Remove workout with unique warmup** → Warmup removed
5. **Remove workout with shared warmup** → Warmup retained
6. **Manual add then auto-add** → No conflict
7. **Add/remove in same transaction** → Rollback works correctly

## Future Enhancements

1. **Tracking Source**: Could track which workout exercise triggered auto-add (decided against for simplicity)
2. **Bulk Operations**: Add multiple workout exercises, batch the auto-linking
3. **Smart Ordering**: Place auto-added exercises based on exercise type
4. **Alternative Suggestions**: If warmup missing, suggest alternatives