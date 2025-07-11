# Enhanced Test Builders - Quick Migration Reference

## REST Exercise Migration

### Old Pattern
```csharp
new CreateExerciseRequestBuilder()
    .WithName("Rest")
    .WithExerciseTypeIds(new[] { TestConstants.ExerciseTypeIds.Rest })
    .WithKineticChainId(null)
    .WithExerciseWeightTypeId(null)
    .WithMuscleGroups(new List<MuscleGroupWithRoleRequest>())
    .Build();
```

### New Pattern
```csharp
CreateExerciseRequestBuilderV2.ForRestExercise()
    .WithName("Rest")
    .Build();
```

## Workout Exercise Migration

### Old Pattern
```csharp
new CreateExerciseRequestBuilder()
    .WithName("Bench Press")
    .WithExerciseTypeIds(new[] { TestConstants.ExerciseTypeIds.Workout })
    .WithKineticChainId(TestConstants.KineticChainTypeIds.Compound)
    .WithExerciseWeightTypeId(TestConstants.ExerciseWeightTypeIds.Barbell)
    .WithMuscleGroups(new List<MuscleGroupWithRoleRequest>
    {
        new() { 
            MuscleGroupId = TestConstants.MuscleGroupIds.Chest,
            MuscleRoleId = TestConstants.MuscleRoleIds.Primary
        }
    })
    .Build();
```

### New Pattern
```csharp
CreateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Bench Press")
    .WithKineticChain(KineticChainTypeTestBuilder.Compound())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
    .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
    .Build();
```

## Common Replacements

### Exercise Types
- `TestConstants.ExerciseTypeIds.Warmup` → `ExerciseTypeTestBuilder.Warmup()`
- `TestConstants.ExerciseTypeIds.Workout` → `ExerciseTypeTestBuilder.Workout()`
- `TestConstants.ExerciseTypeIds.Cooldown` → `ExerciseTypeTestBuilder.Cooldown()`
- `TestConstants.ExerciseTypeIds.Rest` → `ExerciseTypeTestBuilder.Rest()`

### Kinetic Chain Types
- `TestConstants.KineticChainTypeIds.Compound` → `KineticChainTypeTestBuilder.Compound()`
- `TestConstants.KineticChainTypeIds.Isolation` → `KineticChainTypeTestBuilder.Isolation()`
- String IDs like `"kineticchaintype-..."` → Use named builders

### Exercise Weight Types
- `TestConstants.ExerciseWeightTypeIds.Barbell` → `ExerciseWeightTypeTestBuilder.Barbell()`
- `TestConstants.ExerciseWeightTypeIds.Dumbbell` → `ExerciseWeightTypeTestBuilder.Dumbbell()`
- `TestConstants.ExerciseWeightTypeIds.Bodyweight` → `ExerciseWeightTypeTestBuilder.Bodyweight()`

### Muscle Groups
- `TestConstants.MuscleGroupIds.Chest` → `MuscleGroupTestBuilder.Chest()`
- `TestConstants.MuscleGroupIds.Back` → `MuscleGroupTestBuilder.Back()`
- `TestConstants.MuscleGroupIds.Shoulders` → `MuscleGroupTestBuilder.Shoulders()`

### Muscle Roles
- `TestConstants.MuscleRoleIds.Primary` → `MuscleRoleTestBuilder.Primary()`
- `TestConstants.MuscleRoleIds.Secondary` → `MuscleRoleTestBuilder.Secondary()`
- `TestConstants.MuscleRoleIds.Stabilizer` → `MuscleRoleTestBuilder.Stabilizer()`

### Difficulty Levels
- `TestConstants.DifficultyLevelIds.Beginner` → `DifficultyLevelTestBuilder.Beginner()`
- `TestConstants.DifficultyLevelIds.Intermediate` → `DifficultyLevelTestBuilder.Intermediate()`
- `TestConstants.DifficultyLevelIds.Advanced` → `DifficultyLevelTestBuilder.Advanced()`

## Update Request Migration

### Old Pattern
```csharp
new UpdateExerciseRequestBuilder()
    .WithName("Updated Exercise")
    .WithKineticChainId(TestConstants.KineticChainTypeIds.Compound)
    .WithExerciseWeightTypeId(TestConstants.ExerciseWeightTypeIds.Barbell)
    .WithMuscleGroups(muscleGroups)
    .Build();
```

### New Pattern
```csharp
UpdateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Updated Exercise")
    .WithKineticChain(KineticChainTypeTestBuilder.Compound())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
    .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
    .Build();
```

## Key Differences

1. **Entry Points**: Use `ForRestExercise()` or `ForWorkoutExercise()` instead of constructor
2. **Domain Objects**: Replace string IDs with builder objects
3. **Validation**: New builders validate required fields on `Build()`
4. **Type Safety**: Can't call invalid methods (e.g., `WithKineticChain()` on REST)
5. **Coach Notes**: Use `AddCoachNote()` with builder instead of tuples

## Migration Checklist

- [ ] Replace `new CreateExerciseRequestBuilder()` with `CreateExerciseRequestBuilderV2.ForXxx()`
- [ ] Replace `new UpdateExerciseRequestBuilder()` with `UpdateExerciseRequestBuilderV2.ForXxx()`
- [ ] Replace string IDs with domain builders
- [ ] Replace `WithMuscleGroups(list)` with multiple `AddMuscleGroup()` calls
- [ ] Replace coach note tuples with `AddCoachNote(builder)`
- [ ] Handle `InvalidOperationException` from validation
- [ ] Remove unnecessary null/empty assignments for REST exercises