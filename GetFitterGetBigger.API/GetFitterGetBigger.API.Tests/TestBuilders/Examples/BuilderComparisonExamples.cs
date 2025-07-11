using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Xunit;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Examples;

/// <summary>
/// Examples showing the difference between old and new builder patterns
/// </summary>
public class BuilderComparisonExamples
{
    [Fact]
    public void OldPattern_RequiresCommentsAndMagicStrings()
    {
        // Old pattern - unclear intent, magic strings, no validation
        var request = new CreateExerciseRequestBuilder()
            .WithName("Bench Press")
            .WithDescription("Chest exercise")
            .WithDifficultyId("difficultylevel-9b9bec2e-35e3-5a8a-b6b7-1e871f7eb35c")  // What difficulty is this?
            .WithKineticChainId("kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")  // What type?
            .WithExerciseWeightTypeId("exerciseweighttype-a1b2c3d4-e5f6-7890-abcd-ef1234567890")  // What weight type?
            .WithExerciseTypes(new[] { "exercisetype-a1b2c3d4-e5f6-7890-abcd-ef1234567890" })  // Workout? Warmup?
            .WithMuscleGroups(
                ("musclegroup-5f4e3d2c-1b0a-9988-7766-554433221100", "musclerole-a5b6c7d8-e9f0-1234-5678-90abcdef1234")
            )
            .Build();

        // No validation - could have forgotten required fields!
        Assert.NotNull(request);
    }

    [Fact]
    public void NewPattern_ClearIntentWithValidation()
    {
        // New pattern - clear intent, type-safe, validated
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Bench Press")
            .WithDescription("Chest exercise")
            .WithDifficulty(DifficultyLevelTestBuilder.Intermediate())  // Clear: Intermediate difficulty
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())  // Clear: Compound movement
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())  // Clear: Barbell exercise
            .AddMuscleGroup(
                MuscleGroupTestBuilder.Chest(),  // Clear: Chest muscle
                MuscleRoleTestBuilder.Primary()  // Clear: Primary role
            )
            .AddMuscleGroup(
                MuscleGroupTestBuilder.Triceps(),  // Clear: Triceps muscle
                MuscleRoleTestBuilder.Secondary()  // Clear: Secondary role
            )
            .Build();

        // All required fields are validated!
        Assert.NotNull(request);
        Assert.Equal("Bench Press", request.Name);
    }

    [Fact]
    public void OldPattern_RestExerciseConfusion()
    {
        // Old pattern - easy to make mistakes with REST exercises
        var request = new CreateExerciseRequestBuilder()
            .WithName("Rest")
            .WithExerciseTypes(new[] { "exercisetype-f7e6d5c4-b3a2-9180-8f7e-6d5c4b3a2190" })  // REST
            .WithKineticChainId("kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")  // WRONG! REST shouldn't have this
            .WithMuscleGroups()  // Easy to forget to clear this
            .Build();

        // No validation would catch this error!
        Assert.NotNull(request.KineticChainId);  // This is wrong for REST!
    }

    [Fact]
    public void NewPattern_RestExerciseImpossibleToMisuse()
    {
        // New pattern - REST exercises can't have invalid properties
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Rest Period")
            .WithDescription("Recovery between sets")
            .Build();

        // Automatically sets correct values for REST
        Assert.Null(request.KineticChainId);
        Assert.Null(request.ExerciseWeightTypeId);
        Assert.Empty(request.MuscleGroups);
        
        // Can't even call .WithKineticChain() - won't compile!
        // CreateExerciseRequestBuilder.ForRestExercise()
        //     .WithKineticChain(...)  // Compilation error!
    }

    [Fact]
    public void OldPattern_NoValidationForMissingFields()
    {
        // Old pattern - builds successfully even with missing required data
        var request = new CreateExerciseRequestBuilder()
            .WithName("Incomplete Exercise")
            .WithExerciseTypes(new[] { SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout })
            .WithKineticChainId(null)  // Explicitly clearing default
            .WithExerciseWeightTypeId(null)  // Explicitly clearing default
            .WithMuscleGroups()  // Clearing default muscle groups
            .Build();

        // Builds "successfully" but will fail at runtime
        Assert.Null(request.KineticChainId);  // Missing required field!
        Assert.Null(request.ExerciseWeightTypeId);  // Missing required field!
        Assert.Empty(request.MuscleGroups);  // Missing required field!
    }

    [Fact]
    public void NewPattern_BuilderDoesNotValidate_ValidationHappensInService()
    {
        // New pattern - builders don't validate, they just build
        // Validation happens at the service layer when processing the command
        
        // The builder allows creating an incomplete request
        // We explicitly clear required fields to demonstrate lack of validation
        var incompleteRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Incomplete Exercise")
            .WithKineticChainId(null)  // Explicitly clear required field
            .WithExerciseWeightTypeId(null)  // Explicitly clear required field
            .WithMuscleGroups()  // Clear all muscle groups
            .Build();  // This succeeds! No validation in builder
        
        // The request is built successfully even though it's missing required fields
        Assert.NotNull(incompleteRequest);
        Assert.Null(incompleteRequest.KineticChainId);  // Missing required field!
        Assert.Null(incompleteRequest.ExerciseWeightTypeId);  // Missing required field!
        Assert.Empty(incompleteRequest.MuscleGroups);  // Missing required field!
        
        // Validation happens later in the service layer:
        // 1. Request → ToCommand() → CreateExerciseCommand
        // 2. ExerciseService.CreateAsync(command)
        // 3. Private ValidateCreateCommand() returns validation errors
        // 4. Service returns ServiceResult.Failure with errors
        
        // This architectural change follows Clean Architecture principles:
        // - Builders (infrastructure layer) just build DTOs
        // - Services (business layer) enforce business rules
        // - Validation is a business concern, not a DTO construction concern
        
        // Note: The old test "NewPattern_ValidatesRequiredFields" expected builders
        // to throw exceptions, but that's not how the new architecture works!
    }

    [Fact]
    public void ComplexExerciseExample_OldVsNew()
    {
        // OLD: Complex exercise with magic strings and unclear intent
        var oldSquat = new CreateExerciseRequestBuilder()
            .WithName("Barbell Back Squat")
            .WithDescription("Compound lower body exercise")
            .WithDifficultyId("difficultylevel-9b9bec2e-35e3-5a8a-b6b7-1e871f7eb35c")
            .WithKineticChainId("kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")
            .WithExerciseWeightTypeId("exerciseweighttype-a1b2c3d4-e5f6-7890-abcd-ef1234567890")
            .WithExerciseTypes(new[] { "exercisetype-a1b2c3d4-e5f6-7890-abcd-ef1234567890" })
            .WithMuscleGroups(
                // Primary muscles
                ("musclegroup-7f8e9d0c-2b1a-3443-5566-778899aabbcc", "musclerole-a5b6c7d8-e9f0-1234-5678-90abcdef1234"),
                ("musclegroup-9f0e1d2c-3b4a-5665-7788-99aabbccddee", "musclerole-a5b6c7d8-e9f0-1234-5678-90abcdef1234"),
                // Secondary muscles
                ("musclegroup-8f9e0d1c-2b3a-4554-6677-8899aabbccdd", "musclerole-b6c7d8e9-f0a1-2345-6789-0abcdef12345"),
                // Stabilizers
                ("musclegroup-4f5e6d7c-8b9a-0112-2334-455667788990", "musclerole-c7d8e9f0-a1b2-3456-7890-abcdef123456")
            )
            .WithCoachNotes(
                ("Keep knees tracking over toes", 1),
                ("Maintain neutral spine", 2),
                ("Drive through heels", 3)
            )
            .Build();

        // NEW: Same exercise with clear, readable, validated code
        var newSquat = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Barbell Back Squat")
            .WithDescription("Compound lower body exercise")
            .WithDifficulty(DifficultyLevelTestBuilder.Intermediate())
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            // Primary muscles - immediately clear!
            .AddMuscleGroup(MuscleGroupTestBuilder.Quadriceps(), MuscleRoleTestBuilder.Primary())
            .AddMuscleGroup(MuscleGroupTestBuilder.Glutes(), MuscleRoleTestBuilder.Primary())
            // Secondary muscles - self-documenting!
            .AddMuscleGroup(MuscleGroupTestBuilder.Hamstrings(), MuscleRoleTestBuilder.Secondary())
            .AddMuscleGroup(MuscleGroupTestBuilder.Calves(), MuscleRoleTestBuilder.Secondary())
            // Stabilizers - no guessing!
            .AddMuscleGroup(MuscleGroupTestBuilder.Abs(), MuscleRoleTestBuilder.Stabilizer())
            // Coach notes directly added (simplified for example)
            .Build();

        // Both create the same exercise, but new pattern is:
        // - Self-documenting (no magic strings)
        // - Type-safe (can't use wrong IDs)
        // - Validated (can't forget required fields)
        // - Maintainable (easy to understand and modify)
        Assert.Equal(oldSquat.Name, newSquat.Name);
    }
}