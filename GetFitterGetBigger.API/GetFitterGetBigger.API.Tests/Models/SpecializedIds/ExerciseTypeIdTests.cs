using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ExerciseTypeIdTests
{
    [Fact]
    public void TryParse_ValidExerciseTypeId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercisetype-{guid}";

        // Act
        var result = ExerciseTypeId.TryParse(input, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)exerciseTypeId);
        Assert.Equal(input, exerciseTypeId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = ExerciseTypeId.TryParse(null, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseTypeId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = ExerciseTypeId.TryParse(string.Empty, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseTypeId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercise-{guid}";

        // Act
        var result = ExerciseTypeId.TryParse(input, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseTypeId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = ExerciseTypeId.TryParse(input, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseTypeId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "exercisetype-not-a-guid";

        // Act
        var result = ExerciseTypeId.TryParse(input, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseTypeId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "exercisetype-";

        // Act
        var result = ExerciseTypeId.TryParse(input, out ExerciseTypeId exerciseTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseTypeId);
    }
}