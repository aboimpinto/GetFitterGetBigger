using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.DTOs;

public class WorkoutCategoryDtoTests
{
    [Fact]
    public void WorkoutCategoryDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var dto = new WorkoutCategoryDto();
        
        // Assert
        Assert.Equal(string.Empty, dto.WorkoutCategoryId);
        Assert.Equal(string.Empty, dto.Value);
        Assert.Null(dto.Description);
        Assert.Equal(string.Empty, dto.Icon);
        Assert.Equal(string.Empty, dto.Color);
        Assert.Null(dto.PrimaryMuscleGroups);
        Assert.Equal(0, dto.DisplayOrder);
        Assert.False(dto.IsActive);
    }
    
    [Fact]
    public void WorkoutCategoryDto_CanSetProperties()
    {
        // Arrange & Act
        var dto = new WorkoutCategoryDto
            {
            WorkoutCategoryId = "workoutcategory-123e4567-e89b-12d3-a456-426614174000",
            Value = "Strength Training",
            Description = "Workouts focused on building muscle strength and power",
            Icon = "💪",
            Color = "#FF5722",
            PrimaryMuscleGroups = "Chest,Shoulders,Triceps",
            DisplayOrder = 1,
            IsActive = true
        };
        
        // Assert
        Assert.Equal("workoutcategory-123e4567-e89b-12d3-a456-426614174000", dto.WorkoutCategoryId);
        Assert.Equal("Strength Training", dto.Value);
        Assert.Equal("Workouts focused on building muscle strength and power", dto.Description);
        Assert.Equal("💪", dto.Icon);
        Assert.Equal("#FF5722", dto.Color);
        Assert.Equal("Chest,Shoulders,Triceps", dto.PrimaryMuscleGroups);
        Assert.Equal(1, dto.DisplayOrder);
        Assert.True(dto.IsActive);
    }
    
    [Fact]
    public void WorkoutCategoryDto_OptionalFieldsCanBeNull()
    {
        // Arrange & Act
        var dto = new WorkoutCategoryDto
            {
            WorkoutCategoryId = "workoutcategory-123e4567-e89b-12d3-a456-426614174000",
            Value = "Cardio",
            Icon = "🏃",
            Color = "#4CAF50",
            DisplayOrder = 2,
            IsActive = true
        };
        
        // Assert
        Assert.Null(dto.Description);
        Assert.Null(dto.PrimaryMuscleGroups);
    }
    
    [Fact]
    public void WorkoutCategoriesResponseDto_DefaultValues_AreSet()
    {
        // Arrange & Act
        var response = new WorkoutCategoriesResponseDto();
        
        // Assert
        Assert.NotNull(response.WorkoutCategories);
        Assert.Empty(response.WorkoutCategories);
    }
    
    [Fact]
    public void WorkoutCategoriesResponseDto_CanAddItems()
    {
        // Arrange
        var response = new WorkoutCategoriesResponseDto();
        var category1 = new WorkoutCategoryDto
            {
            WorkoutCategoryId = "workoutcategory-123e4567-e89b-12d3-a456-426614174000",
            Value = "Upper Body",
            Icon = "💪",
            Color = "#FF5722",
            DisplayOrder = 1,
            IsActive = true
        };
        var category2 = new WorkoutCategoryDto
            {
            WorkoutCategoryId = "workoutcategory-223e4567-e89b-12d3-a456-426614174000",
            Value = "Lower Body",
            Icon = "🦵",
            Color = "#2196F3",
            DisplayOrder = 2,
            IsActive = true
        };
        
        // Act
        response.WorkoutCategories.Add(category1);
        response.WorkoutCategories.Add(category2);
        
        // Assert
        Assert.Equal(2, response.WorkoutCategories.Count);
        Assert.Contains(category1, response.WorkoutCategories);
        Assert.Contains(category2, response.WorkoutCategories);
    }
}