using System;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.SpecializedIds;

public class CoachNoteIdTests
{
    [Fact]
    public void New_CreatesValidId()
    {
        // Act
        var id = CoachNoteId.New();
        
        // Assert
        Assert.NotEqual(default(CoachNoteId), id);
        var stringValue = id.ToString();
        Assert.StartsWith("coachnote-", stringValue);
        Assert.Equal(46, stringValue.Length); // "coachnote-" (10) + Guid (36)
    }
    
    [Fact]
    public void From_WithValidGuid_CreatesId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var id = CoachNoteId.From(guid);
        
        // Assert
        var expected = $"coachnote-{guid}";
        Assert.Equal(expected, id.ToString());
    }
    
    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var guid = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
        var id = CoachNoteId.From(guid);
        
        // Act
        var result = id.ToString();
        
        // Assert
        Assert.Equal("coachnote-123e4567-e89b-12d3-a456-426614174000", result);
    }
    
    [Fact]
    public void TryParse_WithValidString_ReturnsTrue()
    {
        // Arrange
        var validString = "coachnote-123e4567-e89b-12d3-a456-426614174000";
        
        // Act
        var result = CoachNoteId.TryParse(validString, out var id);
        
        // Assert
        Assert.True(result);
        Assert.Equal(validString, id.ToString());
    }
    
    [Fact]
    public void TryParse_WithInvalidPrefix_ReturnsFalse()
    {
        // Arrange
        var invalidString = "exercise-123e4567-e89b-12d3-a456-426614174000";
        
        // Act
        var result = CoachNoteId.TryParse(invalidString, out var id);
        
        // Assert
        Assert.False(result);
        Assert.Equal(default(CoachNoteId), id);
    }
    
    [Fact]
    public void TryParse_WithInvalidGuid_ReturnsFalse()
    {
        // Arrange
        var invalidString = "coachnote-not-a-guid";
        
        // Act
        var result = CoachNoteId.TryParse(invalidString, out var id);
        
        // Assert
        Assert.False(result);
        Assert.Equal(default(CoachNoteId), id);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("coachnote-")]
    public void TryParse_WithInvalidInput_ReturnsFalse(string input)
    {
        // Act
        var result = CoachNoteId.TryParse(input, out var id);
        
        // Assert
        Assert.False(result);
        Assert.Equal(default(CoachNoteId), id);
    }
    
    [Fact]
    public void TryParse_WithNull_ReturnsFalse()
    {
        // Act
        var result = CoachNoteId.TryParse(null!, out var id);
        
        // Assert
        Assert.False(result);
        Assert.Equal(default(CoachNoteId), id);
    }
    
    [Fact]
    public void Equality_WithSameGuid_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = CoachNoteId.From(guid);
        var id2 = CoachNoteId.From(guid);
        
        // Assert
        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
    }
    
    [Fact]
    public void Equality_WithDifferentGuid_AreNotEqual()
    {
        // Arrange
        var id1 = CoachNoteId.New();
        var id2 = CoachNoteId.New();
        
        // Assert
        Assert.NotEqual(id1, id2);
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
    }
    
    [Fact]
    public void ImplicitConversion_ToGuid_ReturnsCorrectGuid()
    {
        // Arrange
        var originalGuid = Guid.NewGuid();
        var id = CoachNoteId.From(originalGuid);
        
        // Act
        Guid convertedGuid = id;
        
        // Assert
        Assert.Equal(originalGuid, convertedGuid);
    }
}