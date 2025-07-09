using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ExerciseLinkIdTests
{
    [Fact]
    public void TryParse_ValidExerciseLinkId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exerciselink-{guid}";

        // Act
        var result = ExerciseLinkId.TryParse(input, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)exerciseLinkId);
        Assert.Equal(input, exerciseLinkId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = ExerciseLinkId.TryParse(null, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseLinkId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = ExerciseLinkId.TryParse(string.Empty, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseLinkId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercise-{guid}";

        // Act
        var result = ExerciseLinkId.TryParse(input, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseLinkId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = ExerciseLinkId.TryParse(input, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseLinkId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "exerciselink-not-a-guid";

        // Act
        var result = ExerciseLinkId.TryParse(input, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseLinkId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "exerciselink-";

        // Act
        var result = ExerciseLinkId.TryParse(input, out ExerciseLinkId exerciseLinkId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseLinkId);
    }

    [Fact]
    public void New_CreatesValidId()
    {
        // Act
        var id = ExerciseLinkId.New();
        var idString = id.ToString();

        // Assert
        Assert.StartsWith("exerciselink-", idString);
        Assert.True(ExerciseLinkId.TryParse(idString, out var parsedId));
        Assert.Equal(id, parsedId);
    }

    [Fact]
    public void From_CreatesIdWithSpecificGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = ExerciseLinkId.From(guid);

        // Assert
        Assert.Equal(guid, (Guid)id);
        Assert.Equal($"exerciselink-{guid}", id.ToString());
    }

    [Fact]
    public void ImplicitOperator_ConvertsToGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = ExerciseLinkId.From(guid);

        // Act
        Guid convertedGuid = id;

        // Assert
        Assert.Equal(guid, convertedGuid);
    }
}