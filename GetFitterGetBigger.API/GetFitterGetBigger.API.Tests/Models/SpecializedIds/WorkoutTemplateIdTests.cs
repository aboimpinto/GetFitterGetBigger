using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class WorkoutTemplateIdTests
{
    [Fact]
    public void ParseOrEmpty_ValidWorkoutTemplateId_ReturnsValidId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"{WorkoutTemplateId.Empty.GetPrefix()}-{guid}";

        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(input);

        // Assert
        Assert.False(workoutTemplateId.IsEmpty);
        Assert.Equal(guid, (Guid)workoutTemplateId);
        Assert.Equal(input, workoutTemplateId.ToString());
    }

    [Fact]
    public void ParseOrEmpty_NullInput_ReturnsEmpty()
    {
        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(null);

        // Assert
        Assert.True(workoutTemplateId.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, workoutTemplateId);
    }

    [Fact]
    public void ParseOrEmpty_EmptyString_ReturnsEmpty()
    {
        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(string.Empty);

        // Assert
        Assert.True(workoutTemplateId.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, workoutTemplateId);
    }

    [Fact]
    public void ParseOrEmpty_WrongPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"workout-{guid}"; // Wrong prefix to test failure

        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateId.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, workoutTemplateId);
    }

    [Fact]
    public void ParseOrEmpty_NoPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateId.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, workoutTemplateId);
    }

    [Fact]
    public void ParseOrEmpty_InvalidGuid_ReturnsEmpty()
    {
        // Arrange
        var input = $"{WorkoutTemplateId.Empty.GetPrefix()}-not-a-guid";

        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateId.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, workoutTemplateId);
    }

    [Fact]
    public void ParseOrEmpty_PrefixOnly_ReturnsEmpty()
    {
        // Arrange
        var input = $"{WorkoutTemplateId.Empty.GetPrefix()}-";

        // Act
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutTemplateId.IsEmpty);
        Assert.Equal(WorkoutTemplateId.Empty, workoutTemplateId);
    }

    [Fact]
    public void New_CreatesNonEmptyId()
    {
        // Act
        var workoutTemplateId = WorkoutTemplateId.New();

        // Assert
        Assert.False(workoutTemplateId.IsEmpty);
        Assert.NotEqual(Guid.Empty, (Guid)workoutTemplateId);
    }

    [Fact]
    public void From_CreatesIdFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var workoutTemplateId = WorkoutTemplateId.From(guid);

        // Assert
        Assert.False(workoutTemplateId.IsEmpty);
        Assert.Equal(guid, (Guid)workoutTemplateId);
    }

    [Fact]
    public void ToString_EmptyId_ReturnsEmptyString()
    {
        // Arrange
        var workoutTemplateId = WorkoutTemplateId.Empty;

        // Assert
        Assert.Equal(string.Empty, workoutTemplateId.ToString());
    }

    [Fact]
    public void GetPrefix_ReturnsCorrectPrefix()
    {
        // Arrange
        var workoutTemplateId = WorkoutTemplateId.New();

        // Act
        var prefix = workoutTemplateId.GetPrefix();

        // Assert
        Assert.Equal("workouttemplate", prefix);
    }
}