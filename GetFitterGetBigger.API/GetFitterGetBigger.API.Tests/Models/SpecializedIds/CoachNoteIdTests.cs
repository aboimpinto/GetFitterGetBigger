using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class CoachNoteIdTests
{
    [Fact]
    public void TryParse_ValidCoachNoteId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"coachnote-{guid}";

        // Act
        var result = CoachNoteId.TryParse(input, out CoachNoteId coachNoteId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)coachNoteId);
        Assert.Equal(input, coachNoteId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = CoachNoteId.TryParse(null, out CoachNoteId coachNoteId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coachNoteId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = CoachNoteId.TryParse(string.Empty, out CoachNoteId coachNoteId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coachNoteId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"exercise-{guid}";

        // Act
        var result = CoachNoteId.TryParse(input, out CoachNoteId coachNoteId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coachNoteId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = CoachNoteId.TryParse(input, out CoachNoteId coachNoteId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coachNoteId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "coachnote-not-a-guid";

        // Act
        var result = CoachNoteId.TryParse(input, out CoachNoteId coachNoteId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coachNoteId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "coachnote-";

        // Act
        var result = CoachNoteId.TryParse(input, out CoachNoteId coachNoteId);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coachNoteId);
    }
}