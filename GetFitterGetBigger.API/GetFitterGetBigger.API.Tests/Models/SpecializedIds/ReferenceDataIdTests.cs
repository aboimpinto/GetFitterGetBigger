using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class ReferenceDataIdTests
{
    [Fact]
    public void TryParse_ValidReferenceDataId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"refdata-{guid}";

        // Act
        var result = ReferenceDataId.TryParse(input, out ReferenceDataId refDataId);

        // Assert
        Assert.True(result);
        Assert.Equal(guid, (Guid)refDataId);
        Assert.Equal(input, refDataId.ToString());
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = ReferenceDataId.TryParse(null, out ReferenceDataId refDataId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ReferenceDataId), refDataId);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = ReferenceDataId.TryParse(string.Empty, out ReferenceDataId refDataId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ReferenceDataId), refDataId);
    }

    [Fact]
    public void TryParse_WrongPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"user-{guid}"; // Wrong prefix

        // Act
        var result = ReferenceDataId.TryParse(input, out ReferenceDataId refDataId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ReferenceDataId), refDataId);
    }

    [Fact]
    public void TryParse_NoPrefix_ReturnsFalse()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var result = ReferenceDataId.TryParse(input, out ReferenceDataId refDataId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ReferenceDataId), refDataId);
    }

    [Fact]
    public void TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var input = "refdata-invalid-guid";

        // Act
        var result = ReferenceDataId.TryParse(input, out ReferenceDataId refDataId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ReferenceDataId), refDataId);
    }

    [Fact]
    public void TryParse_PrefixOnly_ReturnsFalse()
    {
        // Arrange
        var input = "refdata-";

        // Act
        var result = ReferenceDataId.TryParse(input, out ReferenceDataId refDataId);

        // Assert
        Assert.False(result);
        Assert.Equal(default(ReferenceDataId), refDataId);
    }
}