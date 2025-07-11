# Test Builders Usage Guide

This document explains how to use the Test Builders to quickly fix tests that fail due to the new ExerciseWeightType validation rules.

## Quick Fix Guide

### Problem
Tests are failing with: `"Exercise weight type must be specified for non-REST exercises"`

### Solution
Replace manual `CreateExerciseRequest` and `UpdateExerciseRequest` creation with builders.

## Before & After Examples

### ❌ Before (Manual Creation)
```csharp
var request = new CreateExerciseRequest
{
    Name = "Test Exercise",
    Description = "Test Description",
    DifficultyId = "difficultylevel-" + Guid.NewGuid(),
    KineticChainId = "kineticchaintype-" + Guid.NewGuid(),
    ExerciseTypeIds = new List<string> { "exercisetype-" + Guid.NewGuid() },
    ExerciseWeightTypeId = "exerciseweighttype-" + Guid.NewGuid(), // MUST ADD THIS
    MuscleGroups = new List<MuscleGroupWithRoleRequest>
    {
        new MuscleGroupWithRoleRequest
        {
            MuscleGroupId = "musclegroup-" + Guid.NewGuid(),
            MuscleRoleId = "musclerole-" + Guid.NewGuid()
        }
    }
};
```

### ✅ After (Builder Pattern)
```csharp
var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
    .WithName("Test Exercise")
    .WithDescription("Test Description")
    .Build();
```

## Import Statement Required

Add this import to any test file using builders:
```csharp
using GetFitterGetBigger.API.Tests.TestBuilders;
```

## Builder Types Available

### 1. CreateExerciseRequestBuilder

#### For Workout/Warmup/Cooldown Exercises:
```csharp
// Default - creates valid workout exercise with all required fields
var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
    .WithName("Custom Name")
    .Build();

// or simply
var request = new CreateExerciseRequestBuilder()
    .WithName("Custom Name")
    .Build();
```

#### For REST Exercises:
```csharp
var request = CreateExerciseRequestBuilder.ForRestExercise()
    .WithName("Rest Period")
    .Build();
```

### 2. UpdateExerciseRequestBuilder

#### For Workout/Warmup/Cooldown Exercises:
```csharp
var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
    .WithName("Updated Name")
    .WithIsActive(true)
    .Build();
```

#### For REST Exercises:
```csharp
var request = UpdateExerciseRequestBuilder.ForRestExercise()
    .WithName("Updated Rest")
    .Build();
```

## Available Builder Methods

### Common Methods (Both Builders)
- `.WithName(string name)`
- `.WithDescription(string description)`
- `.WithExerciseTypes(params string[] typeIds)`
- `.WithDifficultyId(string difficultyId)`
- `.WithKineticChainId(string? kineticChainId)`
- `.WithExerciseWeightTypeId(string? exerciseWeightTypeId)`
- `.WithMuscleGroups((string MuscleGroupId, string RoleId)... groups)`
- `.WithCoachNotes((string Text, int Order)... notes)`
- `.WithVideoUrl(string url)`
- `.WithImageUrl(string url)`
- `.WithIsUnilateral(bool isUnilateral)`

### Update-Only Methods
- `.WithIsActive(bool? isActive)`
- `.WithCoachNotes((string Id, string Text, int Order)... notes)` - For updating existing notes

## Quick Fix Process

1. **Add import**: `using GetFitterGetBigger.API.Tests.TestBuilders;`

2. **Identify exercise type**:
   - If it's a REST exercise → Use `.ForRestExercise()`
   - Otherwise → Use `.ForWorkoutExercise()` (default)

3. **Replace manual creation**:
   ```csharp
   // Replace this
   var request = new CreateExerciseRequest { ... };
   
   // With this
   var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
       .WithName("Your Name")
       .Build();
   ```

## Test Constants Available

Use `TestConstants` for consistent IDs:

```csharp
// Exercise Types
TestConstants.ExerciseTypeIds.Rest
TestConstants.ExerciseTypeIds.Workout
TestConstants.ExerciseTypeIds.Warmup
TestConstants.ExerciseTypeIds.Cooldown

// Exercise Weight Types
TestConstants.ExerciseWeightTypeIds.BodyweightOnly
TestConstants.ExerciseWeightTypeIds.WeightRequired
TestConstants.ExerciseWeightTypeIds.MachineWeight

// Other constants available for:
// - DifficultyLevelIds
// - KineticChainTypeIds
// - MuscleGroupIds
// - MuscleRoleIds
// - EquipmentIds
// etc.
```

## Mass Update Strategy

For fixing many tests quickly:

1. **Search and replace pattern**:
   - Find: `new CreateExerciseRequest`
   - Replace: `CreateExerciseRequestBuilder.ForWorkoutExercise()`

2. **Update the structure**:
   - Remove the object initializer `{ ... }`
   - Add builder methods `.WithName().WithDescription().Build()`

3. **Handle REST exercises separately**:
   - Look for tests that use REST exercise types
   - Use `.ForRestExercise()` instead

## Example Transformations

### Simple Case
```csharp
// Before
var request = new CreateExerciseRequest
{
    Name = "Push-ups",
    Description = "Bodyweight exercise"
};

// After
var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
    .WithName("Push-ups")
    .WithDescription("Bodyweight exercise")
    .Build();
```

### Complex Case
```csharp
// Before
var request = new UpdateExerciseRequest
{
    Name = "Updated Exercise",
    IsActive = false,
    CoachNotes = new List<CoachNoteRequest>
    {
        new() { Text = "Tip 1", Order = 0 },
        new() { Text = "Tip 2", Order = 1 }
    }
};

// After
var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
    .WithName("Updated Exercise")
    .WithIsActive(false)
    .WithCoachNotes(("Tip 1", 0), ("Tip 2", 1))
    .Build();
```

## Benefits

1. **Automatic compliance** with validation rules
2. **Consistent test data** across all tests
3. **Reduced boilerplate** code
4. **Easy maintenance** - change defaults in one place
5. **Clear intent** - `.ForRestExercise()` vs `.ForWorkoutExercise()`

## When NOT to Use Builders

- When testing specific validation scenarios that require invalid data
- When you need precise control over every field value
- For negative test cases that intentionally violate rules

In these cases, continue using manual object creation but ensure you understand the validation rules.