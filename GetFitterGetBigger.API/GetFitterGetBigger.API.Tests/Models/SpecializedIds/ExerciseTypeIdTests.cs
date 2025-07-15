using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ExerciseTypeIdTests
{
    [Fact]
    public void ParseOrEmpty_ValidExerciseTypeId_ReturnsValidId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercisetype-{guid}";

        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(input);

        // Assert
        Assert.False(exerciseTypeId.IsEmpty);
        Assert.Equal(guid, (Guid)exerciseTypeId);
        Assert.Equal(input, exerciseTypeId.ToString());
    }

    [Fact]
    public void ParseOrEmpty_NullInput_ReturnsEmpty()
    {
        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(null);

        // Assert
        Assert.True(exerciseTypeId.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, exerciseTypeId);
    }

    [Fact]
    public void ParseOrEmpty_EmptyString_ReturnsEmpty()
    {
        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(string.Empty);

        // Assert
        Assert.True(exerciseTypeId.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, exerciseTypeId);
    }

    [Fact]
    public void ParseOrEmpty_WrongPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercise-{guid}";

        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(exerciseTypeId.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, exerciseTypeId);
    }

    [Fact]
    public void ParseOrEmpty_NoPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(exerciseTypeId.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, exerciseTypeId);
    }

    [Fact]
    public void ParseOrEmpty_InvalidGuid_ReturnsEmpty()
    {
        // Arrange
        var input = "exercisetype-not-a-guid";

        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(exerciseTypeId.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, exerciseTypeId);
    }

    [Fact]
    public void ParseOrEmpty_PrefixOnly_ReturnsEmpty()
    {
        // Arrange
        var input = "exercisetype-";

        // Act
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(exerciseTypeId.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, exerciseTypeId);
    }
}