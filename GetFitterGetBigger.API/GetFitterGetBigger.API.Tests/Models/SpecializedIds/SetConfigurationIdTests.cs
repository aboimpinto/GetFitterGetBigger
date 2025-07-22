using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class SetConfigurationIdTests
{
    [Fact]
    public void ParseOrEmpty_ValidSetConfigurationId_ReturnsValidId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"{SetConfigurationId.Empty.GetPrefix()}-{guid}";

        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(input);

        // Assert
        Assert.False(setConfigurationId.IsEmpty);
        Assert.Equal(guid, (Guid)setConfigurationId);
        Assert.Equal(input, setConfigurationId.ToString());
    }

    [Fact]
    public void ParseOrEmpty_NullInput_ReturnsEmpty()
    {
        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(null);

        // Assert
        Assert.True(setConfigurationId.IsEmpty);
        Assert.Equal(SetConfigurationId.Empty, setConfigurationId);
    }

    [Fact]
    public void ParseOrEmpty_EmptyString_ReturnsEmpty()
    {
        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(string.Empty);

        // Assert
        Assert.True(setConfigurationId.IsEmpty);
        Assert.Equal(SetConfigurationId.Empty, setConfigurationId);
    }

    [Fact]
    public void ParseOrEmpty_WrongPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = $"config-{guid}"; // Wrong prefix to test failure

        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(input);

        // Assert
        Assert.True(setConfigurationId.IsEmpty);
        Assert.Equal(SetConfigurationId.Empty, setConfigurationId);
    }

    [Fact]
    public void ParseOrEmpty_NoPrefix_ReturnsEmpty()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var input = guid.ToString();

        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(input);

        // Assert
        Assert.True(setConfigurationId.IsEmpty);
        Assert.Equal(SetConfigurationId.Empty, setConfigurationId);
    }

    [Fact]
    public void ParseOrEmpty_InvalidGuid_ReturnsEmpty()
    {
        // Arrange
        var input = $"{SetConfigurationId.Empty.GetPrefix()}-not-a-guid";

        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(input);

        // Assert
        Assert.True(setConfigurationId.IsEmpty);
        Assert.Equal(SetConfigurationId.Empty, setConfigurationId);
    }

    [Fact]
    public void ParseOrEmpty_PrefixOnly_ReturnsEmpty()
    {
        // Arrange
        var input = $"{SetConfigurationId.Empty.GetPrefix()}-";

        // Act
        var setConfigurationId = SetConfigurationId.ParseOrEmpty(input);

        // Assert
        Assert.True(setConfigurationId.IsEmpty);
        Assert.Equal(SetConfigurationId.Empty, setConfigurationId);
    }

    [Fact]
    public void New_CreatesNonEmptyId()
    {
        // Act
        var setConfigurationId = SetConfigurationId.New();

        // Assert
        Assert.False(setConfigurationId.IsEmpty);
        Assert.NotEqual(Guid.Empty, (Guid)setConfigurationId);
    }

    [Fact]
    public void From_CreatesIdFromGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var setConfigurationId = SetConfigurationId.From(guid);

        // Assert
        Assert.False(setConfigurationId.IsEmpty);
        Assert.Equal(guid, (Guid)setConfigurationId);
    }

    [Fact]
    public void ToString_EmptyId_ReturnsEmptyString()
    {
        // Arrange
        var setConfigurationId = SetConfigurationId.Empty;

        // Assert
        Assert.Equal(string.Empty, setConfigurationId.ToString());
    }

    [Fact]
    public void GetPrefix_ReturnsCorrectPrefix()
    {
        // Arrange
        var setConfigurationId = SetConfigurationId.New();

        // Act
        var prefix = setConfigurationId.GetPrefix();

        // Assert
        Assert.Equal("setconfiguration", prefix);
    }
}