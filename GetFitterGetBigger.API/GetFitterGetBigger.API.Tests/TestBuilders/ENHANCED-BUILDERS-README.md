# Enhanced Test Builders - Migration Guide

## Overview

The enhanced test builders (V2) provide a type-safe, validation-rich approach to creating test data. They prevent common errors at compile-time and make test intent clearer.

## Key Benefits

1. **Type Safety**: Use domain objects instead of string IDs
2. **Compile-Time Validation**: Can't call invalid methods (e.g., `WithKineticChain()` on REST exercises)
3. **Runtime Validation**: Validates required fields before building
4. **Clear Intent**: Method names and structure clearly indicate what's being built
5. **No Magic Strings**: All IDs come from domain objects

## Quick Start Examples

### Creating a REST Exercise

```csharp
// Old way (unclear, error-prone)
var request = new CreateExerciseRequestBuilder()
    .WithExerciseTypeIds(new[] { "exercisetype-f7e6d5c4-b3a2-9180-8f7e-6d5c4b3a2190" })
    .WithKineticChainId(null)  // Easy to forget
    .WithExerciseWeightTypeId(null)  // Easy to forget
    .WithMuscleGroups(new List<MuscleGroupWithRoleRequest>())  // Easy to forget
    .Build();

// New way (clear, validated)
var request = CreateExerciseRequestBuilderV2.ForRestExercise()
    .WithName("Active Recovery")
    .WithDescription("Light movement between sets")
    .Build();  // Automatically sets correct defaults for REST
```

### Creating a Workout Exercise

```csharp
// Old way (string-based, no validation)
var request = new CreateExerciseRequestBuilder()
    .WithName("Bench Press")
    .WithExerciseTypeIds(new[] { "exercisetype-a1b2c3d4-e5f6-7890-abcd-ef1234567890" })
    .WithKineticChainId("kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")
    .WithMuscleGroups(new List<MuscleGroupWithRoleRequest>
    {
        new() { 
            MuscleGroupId = "musclegroup-5f4e3d2c-1b0a-9988-7766-554433221100",
            MuscleRoleId = "musclerole-a5b6c7d8-e9f0-1234-5678-90abcdef1234"
        }
    })
    .Build();  // No validation, might be missing required fields!

// New way (domain objects, validated)
var request = CreateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Bench Press")
    .WithKineticChain(KineticChainTypeTestBuilder.Compound())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
    .AddMuscleGroup(
        MuscleGroupTestBuilder.Chest(), 
        MuscleRoleTestBuilder.Primary()
    )
    .AddMuscleGroup(
        MuscleGroupTestBuilder.Triceps(), 
        MuscleRoleTestBuilder.Secondary()
    )
    .Build();  // Validates all required fields are present
```

## Domain Object Builders

### MuscleGroup Examples

```csharp
// Pre-defined muscle groups
var chest = MuscleGroupTestBuilder.Chest();
var back = MuscleGroupTestBuilder.Back();
var shoulders = MuscleGroupTestBuilder.Shoulders();
var biceps = MuscleGroupTestBuilder.Biceps();
var triceps = MuscleGroupTestBuilder.Triceps();
var quadriceps = MuscleGroupTestBuilder.Quadriceps();
var hamstrings = MuscleGroupTestBuilder.Hamstrings();
var glutes = MuscleGroupTestBuilder.Glutes();
var calves = MuscleGroupTestBuilder.Calves();
var abs = MuscleGroupTestBuilder.Abs();
var obliques = MuscleGroupTestBuilder.Obliques();
var forearms = MuscleGroupTestBuilder.Forearms();

// Custom muscle group
var custom = MuscleGroupTestBuilder.Custom()
    .WithName("Neck")
    .WithBodyPartId(TestConstants.BodyPartIds.UpperBody);
```

### MuscleRole Examples

```csharp
var primary = MuscleRoleTestBuilder.Primary();
var secondary = MuscleRoleTestBuilder.Secondary();
var stabilizer = MuscleRoleTestBuilder.Stabilizer();
```

### ExerciseType Examples

```csharp
var warmup = ExerciseTypeTestBuilder.Warmup();
var workout = ExerciseTypeTestBuilder.Workout();
var cooldown = ExerciseTypeTestBuilder.Cooldown();
var rest = ExerciseTypeTestBuilder.Rest();
```

### KineticChainType Examples

```csharp
var compound = KineticChainTypeTestBuilder.Compound();
var isolation = KineticChainTypeTestBuilder.Isolation();
var functional = KineticChainTypeTestBuilder.Functional();
var power = KineticChainTypeTestBuilder.Power();
```

### ExerciseWeightType Examples

```csharp
var barbell = ExerciseWeightTypeTestBuilder.Barbell();
var dumbbell = ExerciseWeightTypeTestBuilder.Dumbbell();
var kettlebell = ExerciseWeightTypeTestBuilder.Kettlebell();
var bodyweight = ExerciseWeightTypeTestBuilder.Bodyweight();
var cable = ExerciseWeightTypeTestBuilder.Cable();
var machine = ExerciseWeightTypeTestBuilder.Machine();
var band = ExerciseWeightTypeTestBuilder.ResistanceBand();
var plate = ExerciseWeightTypeTestBuilder.WeightPlate();
```

### CoachNote Examples

```csharp
// Add coach notes with builder
var request = CreateExerciseRequestBuilderV2.ForWorkoutExercise()
    .AddCoachNote(CoachNoteTestBuilder.Create()
        .WithText("Keep core tight throughout")
        .WithOrder(1))
    .AddCoachNote(CoachNoteTestBuilder.Create()
        .WithText("Control the descent")
        .WithOrder(2));

// Or add pre-built notes
var formNote = CoachNoteTestBuilder.FormCue();
var safetyNote = CoachNoteTestBuilder.SafetyTip();
```

## Complex Exercise Examples

### Squat Exercise

```csharp
var squat = CreateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Barbell Back Squat")
    .WithDescription("Compound lower body exercise")
    .WithDifficulty(DifficultyLevelTestBuilder.Intermediate())
    .WithKineticChain(KineticChainTypeTestBuilder.Compound())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
    .AddMuscleGroup(MuscleGroupTestBuilder.Quadriceps(), MuscleRoleTestBuilder.Primary())
    .AddMuscleGroup(MuscleGroupTestBuilder.Glutes(), MuscleRoleTestBuilder.Primary())
    .AddMuscleGroup(MuscleGroupTestBuilder.Hamstrings(), MuscleRoleTestBuilder.Secondary())
    .AddMuscleGroup(MuscleGroupTestBuilder.Calves(), MuscleRoleTestBuilder.Secondary())
    .AddMuscleGroup(MuscleGroupTestBuilder.Abs(), MuscleRoleTestBuilder.Stabilizer())
    .AddCoachNote(CoachNoteTestBuilder.Create()
        .WithText("Keep knees tracking over toes")
        .WithOrder(1))
    .AddCoachNote(CoachNoteTestBuilder.Create()
        .WithText("Maintain neutral spine")
        .WithOrder(2))
    .WithVideoUrl("https://example.com/squat-demo.mp4")
    .Build();
```

### Bicep Curl Exercise

```csharp
var bicepCurl = CreateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Dumbbell Bicep Curl")
    .WithDescription("Isolation exercise for biceps")
    .WithDifficulty(DifficultyLevelTestBuilder.Beginner())
    .WithKineticChain(KineticChainTypeTestBuilder.Isolation())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Dumbbell())
    .AddMuscleGroup(MuscleGroupTestBuilder.Biceps(), MuscleRoleTestBuilder.Primary())
    .AddMuscleGroup(MuscleGroupTestBuilder.Forearms(), MuscleRoleTestBuilder.Secondary())
    .WithIsUnilateral(true)
    .Build();
```

## Updating Exercises

### Update REST Exercise

```csharp
var updateRequest = UpdateExerciseRequestBuilderV2.ForRestExercise()
    .WithName("Extended Rest")
    .WithDescription("Longer recovery period")
    .WithIsActive(true)
    .Build();
```

### Update Workout Exercise with Coach Note Changes

```csharp
var updateRequest = UpdateExerciseRequestBuilderV2.ForWorkoutExercise()
    .WithName("Updated Bench Press")
    .WithKineticChain(KineticChainTypeTestBuilder.Compound())
    .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
    .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
    .UpdateCoachNote("coachnote-123", "Updated form cue", 1)
    .AddCoachNote(CoachNoteTestBuilder.Create()
        .WithText("New safety tip")
        .WithOrder(2))
    .Build();
```

## Migration Guide

### Step 1: Update Creation Patterns

Replace string-based builders with type-safe versions:

```csharp
// Instead of:
new CreateExerciseRequestBuilder()
    .WithExerciseTypeIds(new[] { TestConstants.ExerciseTypeIds.Workout })

// Use:
CreateExerciseRequestBuilderV2.ForWorkoutExercise()
```

### Step 2: Replace String IDs with Domain Objects

```csharp
// Instead of:
.WithKineticChainId("kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")

// Use:
.WithKineticChain(KineticChainTypeTestBuilder.Compound())
```

### Step 3: Update Muscle Group Definitions

```csharp
// Instead of:
.WithMuscleGroups(new List<MuscleGroupWithRoleRequest>
{
    new() { 
        MuscleGroupId = TestConstants.MuscleGroupIds.Chest,
        MuscleRoleId = TestConstants.MuscleRoleIds.Primary
    }
})

// Use:
.AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
```

### Step 4: Handle Validation Errors

The new builders validate required fields:

```csharp
try
{
    var request = CreateExerciseRequestBuilderV2.ForWorkoutExercise()
        .WithName("Invalid Exercise")
        .Build();  // Will throw!
}
catch (InvalidOperationException ex)
{
    // ex.Message will contain:
    // Validation failed:
    //   - KineticChain is required for non-REST exercises
    //   - ExerciseWeightType is required for non-REST exercises
    //   - At least one muscle group is required for non-REST exercises
}
```

## Best Practices

1. **Use Named Constructors**: Prefer `ForWorkoutExercise()` over generic builders
2. **Leverage Domain Builders**: Use `MuscleGroupTestBuilder.Chest()` instead of creating custom
3. **Let Validation Help**: Don't suppress validation errors - fix the test data
4. **Keep Tests Readable**: The verbose syntax makes tests self-documenting
5. **Avoid String IDs**: Always use domain object builders

## Common Patterns

### Testing Exercise Creation Validation

```csharp
[Fact]
public void CreateExercise_WithoutRequiredFields_ThrowsValidationError()
{
    // Arrange & Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
        CreateExerciseRequestBuilderV2.ForWorkoutExercise()
            .WithName("Incomplete Exercise")
            .Build()
    );
    
    Assert.Contains("KineticChain is required", exception.Message);
    Assert.Contains("ExerciseWeightType is required", exception.Message);
    Assert.Contains("At least one muscle group is required", exception.Message);
}
```

### Testing REST vs Workout Separation

```csharp
[Fact]
public void CreateRestExercise_CannotAccessWorkoutMethods()
{
    // This won't compile - REST builder doesn't have WithKineticChain!
    // CreateExerciseRequestBuilderV2.ForRestExercise()
    //     .WithKineticChain(KineticChainTypeTestBuilder.Compound());
    
    // REST exercises are automatically configured correctly
    var rest = CreateExerciseRequestBuilderV2.ForRestExercise().Build();
    Assert.Null(rest.KineticChainId);
    Assert.Null(rest.ExerciseWeightTypeId);
    Assert.Empty(rest.MuscleGroups);
}
```

## Troubleshooting

### "Method not found" errors
You're likely using the wrong builder type. REST exercises have limited methods compared to workout exercises.

### Validation errors on Build()
Check that you've set all required fields:
- Workout exercises need: KineticChain, WeightType, and at least one MuscleGroup
- REST exercises automatically set these to null/empty

### ID format errors
Always use the domain builders. They handle ID formatting correctly:
```csharp
// Don't do this:
.WithId("musclegroup-123")  // Wrong format!

// Do this:
MuscleGroupTestBuilder.Chest()  // Correct format guaranteed
```