using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.DTOs;

public class ExecutionProtocolDtoTests
{
    [Fact]
    public void ExecutionProtocolDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var dto = new ExecutionProtocolDto();
        
        // Assert
        Assert.Equal(string.Empty, dto.ExecutionProtocolId);
        Assert.Equal(string.Empty, dto.Value);
        Assert.Null(dto.Description);
        Assert.Equal(string.Empty, dto.Code);
        Assert.False(dto.TimeBase);
        Assert.False(dto.RepBase);
        Assert.Null(dto.RestPattern);
        Assert.Null(dto.IntensityLevel);
        Assert.Equal(0, dto.DisplayOrder);
        Assert.False(dto.IsActive);
    }
    
    [Fact]
    public void ExecutionProtocolDto_CanSetProperties()
    {
        // Arrange & Act
        var dto = new ExecutionProtocolDto
        {
            ExecutionProtocolId = "executionprotocol-123e4567-e89b-12d3-a456-426614174000",
            Value = "Standard",
            Description = "Standard protocol with balanced rep and time components",
            Code = "STANDARD",
            TimeBase = true,
            RepBase = true,
            RestPattern = "60-90 seconds between sets",
            IntensityLevel = "Moderate to High",
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Assert
        Assert.Equal("executionprotocol-123e4567-e89b-12d3-a456-426614174000", dto.ExecutionProtocolId);
        Assert.Equal("Standard", dto.Value);
        Assert.Equal("Standard protocol with balanced rep and time components", dto.Description);
        Assert.Equal("STANDARD", dto.Code);
        Assert.True(dto.TimeBase);
        Assert.True(dto.RepBase);
        Assert.Equal("60-90 seconds between sets", dto.RestPattern);
        Assert.Equal("Moderate to High", dto.IntensityLevel);
        Assert.Equal(1, dto.DisplayOrder);
        Assert.True(dto.IsActive);
    }
    
    [Fact]
    public void ExecutionProtocolDto_OptionalFieldsCanBeNull()
    {
        // Arrange & Act
        var dto = new ExecutionProtocolDto
        {
            ExecutionProtocolId = "executionprotocol-123e4567-e89b-12d3-a456-426614174000",
            Value = "High Intensity",
            Code = "HIIT",
            TimeBase = true,
            RepBase = false,
            DisplayOrder = 2,
            IsActive = true
        };
        
        // Assert
        Assert.Null(dto.Description);
        Assert.Null(dto.RestPattern);
        Assert.Null(dto.IntensityLevel);
    }
    
    [Fact]
    public void ExecutionProtocolDto_CanHaveMixedBases()
    {
        // Arrange & Act
        var dto1 = new ExecutionProtocolDto { TimeBase = true, RepBase = false };
        var dto2 = new ExecutionProtocolDto { TimeBase = false, RepBase = true };
        var dto3 = new ExecutionProtocolDto { TimeBase = true, RepBase = true };
        var dto4 = new ExecutionProtocolDto { TimeBase = false, RepBase = false };
        
        // Assert
        Assert.True(dto1.TimeBase && !dto1.RepBase);
        Assert.True(!dto2.TimeBase && dto2.RepBase);
        Assert.True(dto3.TimeBase && dto3.RepBase);
        Assert.True(!dto4.TimeBase && !dto4.RepBase);
    }
    
    [Fact]
    public void ExecutionProtocolsResponseDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var response = new ExecutionProtocolsResponseDto();
        
        // Assert
        Assert.NotNull(response.ExecutionProtocols);
        Assert.Empty(response.ExecutionProtocols);
    }
    
    [Fact]
    public void ExecutionProtocolsResponseDto_CanAddItems()
    {
        // Arrange
        var response = new ExecutionProtocolsResponseDto();
        var protocol1 = new ExecutionProtocolDto
        {
            ExecutionProtocolId = "executionprotocol-123e4567-e89b-12d3-a456-426614174000",
            Value = "Standard",
            Code = "STANDARD",
            DisplayOrder = 1,
            IsActive = true
        };
        var protocol2 = new ExecutionProtocolDto
        {
            ExecutionProtocolId = "executionprotocol-223e4567-e89b-12d3-a456-426614174000",
            Value = "Superset",
            Code = "SUPERSET",
            DisplayOrder = 2,
            IsActive = true
        };
        
        // Act
        response.ExecutionProtocols.Add(protocol1);
        response.ExecutionProtocols.Add(protocol2);
        
        // Assert
        Assert.Equal(2, response.ExecutionProtocols.Count);
        Assert.Contains(protocol1, response.ExecutionProtocols);
        Assert.Contains(protocol2, response.ExecutionProtocols);
    }
}