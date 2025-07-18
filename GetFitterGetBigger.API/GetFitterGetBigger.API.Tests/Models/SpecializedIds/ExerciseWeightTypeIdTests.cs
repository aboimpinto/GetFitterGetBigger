using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ExerciseWeightTypeIdTests
{
    [Fact]
    public void ParseOrEmpty_ValidExerciseWeightTypeId_ReturnsId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"{ExerciseWeightTypeId.Empty.GetPrefix()}-{guid}";

        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(input);

        // Assert
        Assert.False(result.IsEmpty);
        Assert.Equal(guid, (Guid)result);
        Assert.Equal(input, result.ToString());
    }

    [Fact]
    public void ParseOrEmpty_NullInput_ReturnsEmpty()
    {
        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(null);

        // Assert
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, result);
    }

    [Fact]
    public void ParseOrEmpty_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(string.Empty);

        // Assert
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, result);
    }

    [Fact]
    public void ParseOrEmpty_WrongPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercisetype-{guid}"; // Wrong prefix to test failure

        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, result);
    }

    [Fact]
    public void ParseOrEmpty_NoPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, result);
    }

    [Fact]
    public void ParseOrEmpty_InvalidGuid_ReturnsEmpty()
    {
        // Arrange
        var input = $"{ExerciseWeightTypeId.Empty.GetPrefix()}-not-a-guid";

        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, result);
    }

    [Fact]
    public void ParseOrEmpty_PrefixOnly_ReturnsEmpty()
    {
        // Arrange
        var input = $"{ExerciseWeightTypeId.Empty.GetPrefix()}-";

        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, result);
    }

    [Fact]
    public void ParseOrEmpty_EmptyGuid_ReturnsValidId()
    {
        // Arrange
        var input = $"{ExerciseWeightTypeId.Empty.GetPrefix()}-00000000-0000-0000-0000-000000000000";

        // Act
        var result = ExerciseWeightTypeId.ParseOrEmpty(input);

        // Assert
        Assert.True(result.IsEmpty); // Empty GUID should result in IsEmpty being true
        Assert.Equal(Guid.Empty, (Guid)result);
    }

    [Fact]
    public void Empty_ReturnsEmptyId()
    {
        // Act
        var empty = ExerciseWeightTypeId.Empty;

        // Assert
        Assert.True(empty.IsEmpty);
        Assert.Equal(Guid.Empty, (Guid)empty);
        Assert.Equal(string.Empty, empty.ToString());
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
        Assert.False(id1.IsEmpty);
        Assert.False(id2.IsEmpty);
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
        Assert.Equal($"{ExerciseWeightTypeId.Empty.GetPrefix()}-{guid}", id.ToString());
        Assert.False(id.IsEmpty);
    }

    [Fact]
    public void From_WithEmptyGuid_CreatesEmptyId()
    {
        // Act
        var id = ExerciseWeightTypeId.From(Guid.Empty);

        // Assert
        Assert.True(id.IsEmpty);
        Assert.Equal(ExerciseWeightTypeId.Empty, id);
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
        Assert.Equal($"{ExerciseWeightTypeId.Empty.GetPrefix()}-11111111-1111-1111-1111-111111111111", result);
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