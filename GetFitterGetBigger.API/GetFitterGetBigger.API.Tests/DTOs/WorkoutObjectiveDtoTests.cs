using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.DTOs;

public class WorkoutObjectiveDtoTests
{
    [Fact]
    public void WorkoutObjectiveDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var dto = new WorkoutObjectiveDto();
        
        // Assert
        Assert.Equal(string.Empty, dto.WorkoutObjectiveId);
        Assert.Equal(string.Empty, dto.Value);
        Assert.Null(dto.Description);
        Assert.Equal(0, dto.DisplayOrder);
        Assert.False(dto.IsActive);
    }
    
    [Fact]
    public void WorkoutObjectiveDto_CanSetProperties()
    {
        // Arrange & Act
        var dto = new WorkoutObjectiveDto
        {
            WorkoutObjectiveId = "workoutobjective-123e4567-e89b-12d3-a456-426614174000",
            Value = "Muscular Strength",
            Description = "Focus on building maximum strength and power",
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Assert
        Assert.Equal("workoutobjective-123e4567-e89b-12d3-a456-426614174000", dto.WorkoutObjectiveId);
        Assert.Equal("Muscular Strength", dto.Value);
        Assert.Equal("Focus on building maximum strength and power", dto.Description);
        Assert.Equal(1, dto.DisplayOrder);
        Assert.True(dto.IsActive);
    }
    
    [Fact]
    public void WorkoutObjectiveDto_DescriptionCanBeNull()
    {
        // Arrange & Act
        var dto = new WorkoutObjectiveDto
        {
            WorkoutObjectiveId = "workoutobjective-123e4567-e89b-12d3-a456-426614174000",
            Value = "Endurance",
            DisplayOrder = 2,
            IsActive = true
        };
        
        // Assert
        Assert.Null(dto.Description);
    }
    
    [Fact]
    public void WorkoutObjectivesResponseDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var response = new WorkoutObjectivesResponseDto();
        
        // Assert
        Assert.NotNull(response.WorkoutObjectives);
        Assert.Empty(response.WorkoutObjectives);
    }
    
    [Fact]
    public void WorkoutObjectivesResponseDto_CanAddItems()
    {
        // Arrange
        var response = new WorkoutObjectivesResponseDto();
        var objective1 = new WorkoutObjectiveDto
        {
            WorkoutObjectiveId = "workoutobjective-123e4567-e89b-12d3-a456-426614174000",
            Value = "Strength",
            DisplayOrder = 1,
            IsActive = true
        };
        var objective2 = new WorkoutObjectiveDto
        {
            WorkoutObjectiveId = "workoutobjective-223e4567-e89b-12d3-a456-426614174000",
            Value = "Hypertrophy",
            DisplayOrder = 2,
            IsActive = true
        };
        
        // Act
        response.WorkoutObjectives.Add(objective1);
        response.WorkoutObjectives.Add(objective2);
        
        // Assert
        Assert.Equal(2, response.WorkoutObjectives.Count);
        Assert.Contains(objective1, response.WorkoutObjectives);
        Assert.Contains(objective2, response.WorkoutObjectives);
    }
}