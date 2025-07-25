using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class WorkoutStateIdTests
{
    [Fact]
    public void ParseOrEmpty_ValidWorkoutStateId_ReturnsValidId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"{WorkoutStateId.Empty.GetPrefix()}-{guid}";

        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(input);

        // Assert
        Assert.False(workoutStateId.IsEmpty);
        Assert.Equal(guid, (Guid)workoutStateId);
        Assert.Equal(input, workoutStateId.ToString());
    }

    [Fact]
    public void ParseOrEmpty_NullInput_ReturnsEmpty()
    {
        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(null);

        // Assert
        Assert.True(workoutStateId.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, workoutStateId);
    }

    [Fact]
    public void ParseOrEmpty_EmptyString_ReturnsEmpty()
    {
        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(string.Empty);

        // Assert
        Assert.True(workoutStateId.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, workoutStateId);
    }

    [Fact]
    public void ParseOrEmpty_WrongPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"workout-{guid}"; // Wrong prefix to test failure

        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutStateId.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, workoutStateId);
    }

    [Fact]
    public void ParseOrEmpty_NoPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutStateId.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, workoutStateId);
    }

    [Fact]
    public void ParseOrEmpty_InvalidGuid_ReturnsEmpty()
    {
        // Arrange
        var input = $"{WorkoutStateId.Empty.GetPrefix()}-not-a-guid";

        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutStateId.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, workoutStateId);
    }

    [Fact]
    public void ParseOrEmpty_PrefixOnly_ReturnsEmpty()
    {
        // Arrange
        var input = $"{WorkoutStateId.Empty.GetPrefix()}-";

        // Act
        var workoutStateId = WorkoutStateId.ParseOrEmpty(input);

        // Assert
        Assert.True(workoutStateId.IsEmpty);
        Assert.Equal(WorkoutStateId.Empty, workoutStateId);
    }

    [Fact]
    public void New_CreatesNonEmptyId()
    {
        // Act
        var workoutStateId = WorkoutStateId.New();

        // Assert
        Assert.False(workoutStateId.IsEmpty);
        Assert.NotEqual(Guid.Empty, (Guid)workoutStateId);
    }

    [Fact]
    public void From_CreatesIdFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var workoutStateId = WorkoutStateId.From(guid);

        // Assert
        Assert.False(workoutStateId.IsEmpty);
        Assert.Equal(guid, (Guid)workoutStateId);
    }

    [Fact]
    public void ToString_EmptyId_ReturnsEmptyString()
    {
        // Arrange
        var workoutStateId = WorkoutStateId.Empty;

        // Assert
        Assert.Equal(string.Empty, workoutStateId.ToString());
    }

    [Fact]
    public void GetPrefix_ReturnsCorrectPrefix()
    {
        // Arrange
        var workoutStateId = WorkoutStateId.New();

        // Act
        var prefix = workoutStateId.GetPrefix();

        // Assert
        Assert.Equal("workoutstate", prefix);
    }
}