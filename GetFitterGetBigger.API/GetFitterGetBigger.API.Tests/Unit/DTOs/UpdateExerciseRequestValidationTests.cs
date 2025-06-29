using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Unit.DTOs;

public class UpdateExerciseRequestValidationTests
{
    [Fact]
    public void UpdateExerciseRequest_RestExercise_EmptyMuscleGroups_IsValid()
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            Name = "Rest Period",
            Description = "Recovery time",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { "exercisetype-rest-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateExerciseRequest_NonRestExercise_EmptyMuscleGroups_IsInvalid()
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", validationResults[0].ErrorMessage);
    }

    [Fact]
    public void UpdateExerciseRequest_NonRestExercise_WithMuscleGroups_IsValid()
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            }
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("exercisetype-rest-456")]
    [InlineData("exercisetype-REST-456")]
    [InlineData("exercisetype-Rest-456")]
    [InlineData("exercisetype-rEsT-456")]
    public void UpdateExerciseRequest_RestExercise_CaseInsensitive_EmptyMuscleGroups_IsValid(string restExerciseTypeId)
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            Name = "Rest Period",
            Description = "Recovery time",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { restExerciseTypeId },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateExerciseRequest_EmptyExerciseTypes_EmptyMuscleGroups_IsInvalid()
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            Name = "Some Exercise",
            Description = "Some exercise description",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", validationResults[0].ErrorMessage);
    }

    [Fact]
    public void UpdateExerciseRequest_RequiredFields_Validation()
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            // Missing required fields: Name, Description, DifficultyId
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            }
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.True(validationResults.Count >= 3); // Name, Description, DifficultyId
        Assert.Contains(validationResults, vr => vr.ErrorMessage!.Contains("Exercise name is required"));
        Assert.Contains(validationResults, vr => vr.ErrorMessage!.Contains("Description is required"));
        Assert.Contains(validationResults, vr => vr.ErrorMessage!.Contains("Difficulty level is required"));
    }

    [Fact]
    public void UpdateExerciseRequest_RestExercise_WithMuscleGroups_IsValid()
    {
        // Arrange - REST exercises CAN have muscle groups, they're just not required
        var request = new UpdateExerciseRequest
        {
            Name = "Rest Period",
            Description = "Recovery time",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { "exercisetype-rest-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            }
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("exercisetype-warmup-456")]
    [InlineData("exercisetype-workout-456")]
    [InlineData("exercisetype-cooldown-456")]
    [InlineData("exercisetype-strength-456")]
    public void UpdateExerciseRequest_NonRestExerciseTypes_EmptyMuscleGroups_IsInvalid(string exerciseTypeId)
    {
        // Arrange
        var request = new UpdateExerciseRequest
        {
            Name = "Some Exercise",
            Description = "Some exercise description",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { exerciseTypeId },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", validationResults[0].ErrorMessage);
    }

    [Fact]
    public void UpdateExerciseRequest_OptionalFields_DoNotAffectValidation()
    {
        // Arrange - UpdateExerciseRequest has some optional fields that CreateExerciseRequest doesn't have
        var request = new UpdateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-beginner-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            },
            IsUnilateral = true,
            IsActive = false
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}