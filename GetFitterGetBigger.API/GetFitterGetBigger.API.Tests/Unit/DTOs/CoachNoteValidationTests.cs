using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Unit.DTOs;

public class CoachNoteValidationTests
{
    [Fact]
    public void CreateExerciseRequest_WithNullCoachNotes_IsValid()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Test Exercise",
            Description = "Test description",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-123", MuscleRoleId = "musclerole-456" }
            },
            CoachNotes = null // Explicitly null
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void CreateExerciseRequest_WithEmptyCoachNotes_IsValid()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Test Exercise",
            Description = "Test description",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-123", MuscleRoleId = "musclerole-456" }
            },
            CoachNotes = new List<CoachNoteRequest>() // Empty list
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void CreateExerciseRequest_WithoutCoachNotesProperty_IsValid()
    {
        // Arrange - CoachNotes will use default value (empty list)
        var request = new CreateExerciseRequest
        {
            Name = "Test Exercise",
            Description = "Test description",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-123", MuscleRoleId = "musclerole-456" }
            }
            // CoachNotes not specified - uses default empty list
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void CoachNoteRequest_WithEmptyText_IsInvalid()
    {
        // Arrange
        var coachNote = new CoachNoteRequest
        {
            Text = "", // Empty text
            Order = 0
        };

        // Act
        var validationResults = ValidateModel(coachNote);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains("Coach note text is required", validationResults[0].ErrorMessage);
    }

    [Fact]
    public void CoachNoteRequest_WithValidData_IsValid()
    {
        // Arrange
        var coachNote = new CoachNoteRequest
        {
            Text = "Keep your back straight",
            Order = 0
        };

        // Act
        var validationResults = ValidateModel(coachNote);

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