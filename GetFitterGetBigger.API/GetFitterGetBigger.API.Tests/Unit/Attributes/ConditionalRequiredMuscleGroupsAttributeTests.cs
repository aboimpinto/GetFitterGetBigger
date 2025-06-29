using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GetFitterGetBigger.API.Attributes;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Unit.Attributes;

public class ConditionalRequiredMuscleGroupsAttributeTests
{
    private readonly ConditionalRequiredMuscleGroupsAttribute _attribute;

    public ConditionalRequiredMuscleGroupsAttributeTests()
    {
        _attribute = new ConditionalRequiredMuscleGroupsAttribute();
    }

    [Fact]
    public void IsValid_WithCreateExerciseRequest_RestExercise_EmptyMuscleGroups_ReturnsSuccess()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Rest Exercise",
            Description = "A rest period",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-rest-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WithCreateExerciseRequest_NonRestExercise_EmptyMuscleGroups_ReturnsError()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_WithCreateExerciseRequest_NonRestExercise_WithMuscleGroups_ReturnsSuccess()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() { MuscleGroupId = "musclegroup-chest-123", MuscleRoleId = "musclerole-primary-456" }
            }
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WithUpdateExerciseRequest_RestExercise_EmptyMuscleGroups_ReturnsSuccess()
    {
        // Arrange
        var updateRequest = new UpdateExerciseRequest
        {
            Name = "Rest Exercise",
            Description = "A rest period",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-rest-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(updateRequest)
        {
            MemberName = nameof(UpdateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(updateRequest.MuscleGroups, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_WithUpdateExerciseRequest_NonRestExercise_EmptyMuscleGroups_ReturnsError()
    {
        // Arrange
        var updateRequest = new UpdateExerciseRequest
        {
            Name = "Push Up",
            Description = "Upper body exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(updateRequest)
        {
            MemberName = nameof(UpdateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(updateRequest.MuscleGroups, validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", result?.ErrorMessage);
    }

    [Theory]
    [InlineData("exercisetype-rest-456")]
    [InlineData("exercisetype-REST-456")]
    [InlineData("exercisetype-Rest-456")]
    [InlineData("exercisetype-rEsT-456")]
    public void IsValid_RestDetection_CaseInsensitive_ReturnsSuccess(string restExerciseTypeId)
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Rest Exercise",
            Description = "A rest period",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { restExerciseTypeId },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_MultipleExerciseTypes_ContainsRest_ReturnsSuccess()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Rest Exercise",
            Description = "A rest period",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> 
            { 
                "exercisetype-rest-456",
                "exercisetype-warmup-789" // This should not happen due to REST exclusivity, but testing validation behavior
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_EmptyExerciseTypeIds_EmptyMuscleGroups_ReturnsError()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Exercise",
            Description = "Some exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string>(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_NullExerciseTypeIds_EmptyMuscleGroups_ReturnsError()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Exercise",
            Description = "Some exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = null!,
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_NullMuscleGroups_ReturnsError()
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Exercise",
            Description = "Some exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { "exercisetype-workout-456" },
            MuscleGroups = null!
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(null, validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Invalid muscle groups format", result?.ErrorMessage);
    }

    [Fact]
    public void IsValid_InvalidContext_ReturnsError()
    {
        // Arrange
        var invalidObject = new { Name = "Invalid" };
        var validationContext = new ValidationContext(invalidObject)
        {
            MemberName = "SomeProperty"
        };

        // Act
        var result = _attribute.GetValidationResult(new List<MuscleGroupWithRoleRequest>(), validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Invalid context for muscle group validation", result?.ErrorMessage);
    }

    [Theory]
    [InlineData("exercisetype-warmup-456")]
    [InlineData("exercisetype-workout-456")]
    [InlineData("exercisetype-cooldown-456")]
    [InlineData("exercisetype-strength-456")]
    public void IsValid_NonRestExerciseTypes_EmptyMuscleGroups_ReturnsError(string exerciseTypeId)
    {
        // Arrange
        var createRequest = new CreateExerciseRequest
        {
            Name = "Exercise",
            Description = "Some exercise",
            DifficultyId = "difficulty-123",
            ExerciseTypeIds = new List<string> { exerciseTypeId },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>()
        };

        var validationContext = new ValidationContext(createRequest)
        {
            MemberName = nameof(CreateExerciseRequest.MuscleGroups)
        };

        // Act
        var result = _attribute.GetValidationResult(createRequest.MuscleGroups, validationContext);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", result?.ErrorMessage);
    }
}