using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ClaimIdTests
{
    [Fact]
    public void TryParse_ValidClaimId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"claim-{guid}";

        // Act
        var result = ClaimId.TryParse(input, out ClaimId claimId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)claimId);
        Assert.Equal(input, claimId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = ClaimId.TryParse(null, out ClaimId claimId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ClaimId), claimId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = ClaimId.TryParse(string.Empty, out ClaimId claimId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ClaimId), claimId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"user-{guid}"; // Wrong prefix

        // Act
        var result = ClaimId.TryParse(input, out ClaimId claimId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ClaimId), claimId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = ClaimId.TryParse(input, out ClaimId claimId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ClaimId), claimId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "claim-invalid-guid";

        // Act
        var result = ClaimId.TryParse(input, out ClaimId claimId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ClaimId), claimId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "claim-";

        // Act
        var result = ClaimId.TryParse(input, out ClaimId claimId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ClaimId), claimId);
    }
}