using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ExerciseWeightTypeIdTests
{
    [Fact]
    public void TryParse_ValidExerciseWeightTypeId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exerciseweighttype-{guid}";

        // Act
        var result = ExerciseWeightTypeId.TryParse(input, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)exerciseWeightTypeId);
        Assert.Equal(input, exerciseWeightTypeId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = ExerciseWeightTypeId.TryParse(null, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseWeightTypeId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = ExerciseWeightTypeId.TryParse(string.Empty, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseWeightTypeId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercisetype-{guid}";

        // Act
        var result = ExerciseWeightTypeId.TryParse(input, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseWeightTypeId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = ExerciseWeightTypeId.TryParse(input, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseWeightTypeId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "exerciseweighttype-not-a-guid";

        // Act
        var result = ExerciseWeightTypeId.TryParse(input, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseWeightTypeId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "exerciseweighttype-";

        // Act
        var result = ExerciseWeightTypeId.TryParse(input, out ExerciseWeightTypeId exerciseWeightTypeId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, exerciseWeightTypeId);
    }

    [Fact]
    public void New_CreatesUniqueIds()
    {
        // Act
        var id1 = ExerciseWeightTypeId.New();
        var id2 = ExerciseWeightTypeId.New();

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id1.ToString(), id2.ToString());
    }

    [Fact]
    public void From_CreatesIdWithSpecificGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = ExerciseWeightTypeId.From(guid);

        // Assert
        Assert.Equal(guid, (Guid)id);
        Assert.Equal($"exerciseweighttype-{guid}", id.ToString());
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var guid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var id = ExerciseWeightTypeId.From(guid);

        // Act
        var result = id.ToString();

        // Assert
        Assert.Equal("exerciseweighttype-11111111-1111-1111-1111-111111111111", result);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_Works()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = ExerciseWeightTypeId.From(guid);

        // Act
        Guid convertedGuid = id;

        // Assert
        Assert.Equal(guid, convertedGuid);
    }
}