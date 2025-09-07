# Metadata Patterns: Exercise Configuration

This document defines the JSON metadata structures used in WorkoutTemplateExercise.

## Core Metadata Patterns

### 1. Time-Based Exercises
Used for: Cardio, stretching, planks, REST exercises

```json
{
  "duration": 30,
  "unit": "seconds"  // or "minutes"
}
```

### 2. Repetition-Based Exercises
Used for: Bodyweight exercises without weight

```json
{
  "reps": 10
}
```

### 3. Weight + Repetition Exercises
Used for: Barbell, dumbbell, machine exercises

```json
{
  "reps": 20,
  "weight": {
    "value": 10,
    "unit": "kg"  // or "lbs"
  }
}
```

### 4. Distance-Based Exercises (Future)
Used for: Running, rowing, cycling

```json
{
  "distance": 400,
  "unit": "meters",  // or "miles", "km"
  "targetTime": 90,   // optional
  "timeUnit": "seconds"
}
```

## Advanced Patterns

### Superset Indication
```json
{
  "reps": 10,
  "weight": {"value": 60, "unit": "kg"},
  "supersetGroup": "A"  // Exercises with same group performed back-to-back
}
```

### Time Under Tension
```json
{
  "reps": 10,
  "weight": {"value": 60, "unit": "kg"},
  "tempo": {
    "eccentric": 3,    // 3 seconds lowering
    "pause": 1,        // 1 second pause at bottom
    "concentric": 1,   // 1 second lifting
    "rest": 0          // 0 second pause at top
  }
}
```

### Drop Set Indication
```json
{
  "reps": 8,
  "weight": {"value": 15, "unit": "kg"},
  "dropSet": 1  // 1st, 2nd, 3rd set in the drop
}
```

### Cluster Set
```json
{
  "reps": 3,
  "weight": {"value": 140, "unit": "kg"},
  "miniRest": 15  // Seconds between mini-sets
}
```

## ExecutionProtocol-Specific Configurations

### REPS_AND_SETS
Standard patterns apply. No special configuration needed.

### EMOM (Future)
```json
{
  "reps": 10,
  "weight": {"value": 40, "unit": "kg"},
  "targetCompletionTime": 30  // Should finish within 30 seconds
}
```

### TABATA (Future)
```json
{
  // Often just empty as work/rest times defined at template level
  // Or specify target reps per work period
  "targetReps": 15
}
```

### AMRAP
```json
{
  "reps": 10,  // Target reps per round
  "weight": {"value": 40, "unit": "kg"}
  // Actual rounds completed tracked during workout performance
}
```

## Special Cases

### REST Exercise
REST is a normal exercise with ExerciseType = REST that only accepts duration:

```json
{
  "duration": 60,
  "unit": "seconds"
}
```

### Empty Metadata
When auto-adding warmup/cooldown, initially added with empty metadata:

```json
{}
```
PT must then configure the exercise with appropriate values.

### Progressive Loading Example
Same exercise, different rounds, increasing weight:

```json
// Round 1
{"reps": 10, "weight": {"value": 40, "unit": "kg"}}

// Round 2
{"reps": 10, "weight": {"value": 50, "unit": "kg"}}

// Round 3
{"reps": 10, "weight": {"value": 60, "unit": "kg"}}
```

## Validation Rules

### Required Fields by Exercise Type
- **REST**: Must have `duration` and `unit`
- **Weight-based**: Must have `weight` object if ExerciseWeightType != BODYWEIGHT
- **All non-REST**: Must have either `reps`, `duration`, or `distance`

### Valid Units
- **Weight**: "kg", "lbs"
- **Time**: "seconds", "minutes", "hours"
- **Distance**: "meters", "kilometers", "miles", "feet"

### Value Constraints
- All numeric values must be positive
- Duration in seconds: 1-3600 (1 hour max)
- Reps: 1-1000
- Weight: 0-1000 (unit dependent)

## Migration from Structured Data

If migrating from a structured system with separate columns:

```sql
-- Old structure
Reps: 10
Sets: 3
Weight: 50
WeightUnit: 'kg'
RestBetweenSets: 90

-- New metadata
{
  "reps": 10,
  "sets": 3,  // Optional, often managed by rounds
  "weight": {
    "value": 50,
    "unit": "kg"
  },
  "restBetweenSets": 90  // Optional, often REST exercise instead
}
```

## Future Extensibility

The JSON structure allows for future additions without schema changes:

```json
{
  "reps": 10,
  "weight": {"value": 50, "unit": "kg"},
  
  // Future additions
  "difficulty": "RIR2",  // Reps in Reserve
  "notes": "Focus on form",
  "videoUrl": "https://...",
  "alternativeExerciseId": 456,
  "equipmentSettings": {
    "seatHeight": 5,
    "backAngle": 45
  }
}
```

## Best Practices

1. **Keep it minimal**: Only include necessary fields
2. **Use consistent units**: Stick to one system (metric or imperial) per template
3. **Validate on input**: Check metadata matches exercise capabilities
4. **Default smartly**: Provide sensible defaults in UI based on exercise type
5. **Version carefully**: If structure changes significantly, consider versioning

## Examples by Exercise

### Barbell Squat
```json
{"reps": 10, "weight": {"value": 60, "unit": "kg"}}
```

### Push-ups
```json
{"reps": 15}
```

### Plank
```json
{"duration": 45, "unit": "seconds"}
```

### Running
```json
{"distance": 5, "unit": "kilometers", "targetTime": 30, "timeUnit": "minutes"}
```

### REST
```json
{"duration": 90, "unit": "seconds"}
```

### Burpees (Time-based)
```json
{"duration": 30, "unit": "seconds"}
```

### Burpees (Rep-based)
```json
{"reps": 10}
```