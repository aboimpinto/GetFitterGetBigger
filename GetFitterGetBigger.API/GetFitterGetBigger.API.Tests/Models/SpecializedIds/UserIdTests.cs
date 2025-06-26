using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class UserIdTests
{
    [Fact]
    public void TryParse_ValidUserId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"user-{guid}";

        // Act
        var result = UserId.TryParse(input, out UserId userId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)userId);
        Assert.Equal(input, userId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = UserId.TryParse(null, out UserId userId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(UserId), userId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = UserId.TryParse(string.Empty, out UserId userId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(UserId), userId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"claim-{guid}"; // Wrong prefix

        // Act
        var result = UserId.TryParse(input, out UserId userId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(UserId), userId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = UserId.TryParse(input, out UserId userId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(UserId), userId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "user-invalid-guid";

        // Act
        var result = UserId.TryParse(input, out UserId userId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(UserId), userId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "user-";

        // Act
        var result = UserId.TryParse(input, out UserId userId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(UserId), userId);
    }
}