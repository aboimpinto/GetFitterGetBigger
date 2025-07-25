using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class WorkoutTemplateExerciseIdTests
{
    [Fact]
    public void ParseOrEmpty_ValidWorkoutTemplateExerciseId_ReturnsValidId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"{WorkoutTemplateExerciseId.Empty.GetPrefix()}-{guid}";

        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(input);

        // Assert
        Assert.False(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(guid, (Guid)workoutTemplateExerciseId);
        Assert.Equal(input, workoutTemplateExerciseId.ToString());
    }

    [Fact]
    public void ParseOrEmpty_NullInput_ReturnsEmpty()
    {
        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(null);

        // Assert
        Assert.True(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(WorkoutTemplateExerciseId.Empty, workoutTemplateExerciseId);
    }

    [Fact]
    public void ParseOrEmpty_EmptyString_ReturnsEmpty()
    {
        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(string.Empty);

        // Assert
        Assert.True(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(WorkoutTemplateExerciseId.Empty, workoutTemplateExerciseId);
    }

    [Fact]
    public void ParseOrEmpty_WrongPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercise-{guid}"; // Wrong prefix to test failure

        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(WorkoutTemplateExerciseId.Empty, workoutTemplateExerciseId);
    }

    [Fact]
    public void ParseOrEmpty_NoPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(WorkoutTemplateExerciseId.Empty, workoutTemplateExerciseId);
    }

    [Fact]
    public void ParseOrEmpty_InvalidGuid_ReturnsEmpty()
    {
        // Arrange
        var input = $"{WorkoutTemplateExerciseId.Empty.GetPrefix()}-not-a-guid";

        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(WorkoutTemplateExerciseId.Empty, workoutTemplateExerciseId);
    }

    [Fact]
    public void ParseOrEmpty_PrefixOnly_ReturnsEmpty()
    {
        // Arrange
        var input = $"{WorkoutTemplateExerciseId.Empty.GetPrefix()}-";

        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(WorkoutTemplateExerciseId.Empty, workoutTemplateExerciseId);
    }

    [Fact]
    public void New_CreatesNonEmptyId()
    {
        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.New();

        // Assert
        Assert.False(workoutTemplateExerciseId.IsEmpty);
        Assert.NotEqual(Guid.Empty, (Guid)workoutTemplateExerciseId);
    }

    [Fact]
    public void From_CreatesIdFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.From(guid);

        // Assert
        Assert.False(workoutTemplateExerciseId.IsEmpty);
        Assert.Equal(guid, (Guid)workoutTemplateExerciseId);
    }

    [Fact]
    public void ToString_EmptyId_ReturnsEmptyString()
    {
        // Arrange
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.Empty;

        // Assert
        Assert.Equal(string.Empty, workoutTemplateExerciseId.ToString());
    }

    [Fact]
    public void GetPrefix_ReturnsCorrectPrefix()
    {
        // Arrange
        var workoutTemplateExerciseId = WorkoutTemplateExerciseId.New();

        // Act
        var prefix = workoutTemplateExerciseId.GetPrefix();

        // Assert
        Assert.Equal("workouttemplateexercise", prefix);
    }
}