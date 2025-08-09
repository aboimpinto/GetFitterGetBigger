using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.DTOs;

public class CoachNoteDtoTests
{
    [Fact]
    public void CoachNoteDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var dto = new CoachNoteDto();
        
        // Assert
        Assert.Equal(string.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.Text);
        Assert.Equal(0, dto.Order);
    }
    
    [Fact]
    public void CoachNoteDto_CanSetProperties()
    {
        // Arrange & Act
        var dto = new CoachNoteDto
            {
            Id = "coachnote-123e4567-e89b-12d3-a456-426614174000",
            Text = "Keep your back straight during the movement",
            Order = 1
        };
        
        // Assert
        Assert.Equal("coachnote-123e4567-e89b-12d3-a456-426614174000", dto.Id);
        Assert.Equal("Keep your back straight during the movement", dto.Text);
        Assert.Equal(1, dto.Order);
    }
    
    [Fact]
    public void CoachNoteRequest_DefaultValues_AreSet()
    {
        // Arrange & Act
        var request = new CoachNoteRequest();
        
        // Assert
        Assert.Null(request.Id);
        Assert.Equal(string.Empty, request.Text);
        Assert.Equal(0, request.Order);
    }
    
    [Fact]
    public void CoachNoteRequest_CanSetProperties()
    {
        // Arrange
        var request = new CoachNoteRequest();
        
        // Act
        request.Id = "coachnote-123e4567-e89b-12d3-a456-426614174000";
        request.Text = "Breathe out during the exertion phase";
        request.Order = 2;
        
        // Assert
        Assert.Equal("coachnote-123e4567-e89b-12d3-a456-426614174000", request.Id);
        Assert.Equal("Breathe out during the exertion phase", request.Text);
        Assert.Equal(2, request.Order);
    }
    
    [Fact]
    public void CoachNoteRequest_IdCanBeNull()
    {
        // Arrange
        var request = new CoachNoteRequest
            {
            Text = "Focus on controlled movement",
            Order = 0
        };
        
        // Assert
        Assert.Null(request.Id);
        Assert.Equal("Focus on controlled movement", request.Text);
        Assert.Equal(0, request.Order);
    }
}