using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class ExerciseTypeTests
{
    [Fact]
    public void CreateNew_ValidInput_CreatesExerciseType()
    {
        // Arrange
        var value = "Warmup";
        var description = "Exercises for warming up";
        var displayOrder = 1;
        
        // Act
        var exerciseType = ExerciseType.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.NotEqual(default(ExerciseTypeId), exerciseType.Id);
        Assert.Equal(value, exerciseType.Value);
        Assert.Equal(description, exerciseType.Description);
        Assert.Equal(displayOrder, exerciseType.DisplayOrder);
        Assert.True(exerciseType.IsActive);
    }
    
    [Fact]
    public void CreateNew_WithIsActiveFalse_CreatesInactiveExerciseType()
    {
        // Arrange
        var value = "Deprecated";
        var description = "No longer used";
        var displayOrder = 99;
        var isActive = false;
        
        // Act
        var exerciseType = ExerciseType.Handler.CreateNew(value, description, displayOrder, isActive);
        
        // Assert
        Assert.False(exerciseType.IsActive);
    }
    
    [Fact]
    public void Create_ValidInput_CreatesExerciseTypeWithSpecificId()
    {
        // Arrange
        var id = ExerciseTypeId.New();
        var value = "Workout";
        var description = "Main workout exercises";
        var displayOrder = 2;
        var isActive = true;
        
        // Act
        var exerciseType = ExerciseType.Handler.Create(id, value, description, displayOrder, isActive);
        
        // Assert
        Assert.Equal(id, exerciseType.Id);
        Assert.Equal(value, exerciseType.Value);
        Assert.Equal(description, exerciseType.Description);
        Assert.Equal(displayOrder, exerciseType.DisplayOrder);
        Assert.Equal(isActive, exerciseType.IsActive);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyValue_ThrowsArgumentException(string value)
    {
        // Arrange
        var description = "Description";
        var displayOrder = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ExerciseType.Handler.CreateNew(value, description, displayOrder));
        Assert.Contains("Value cannot be empty", exception.Message);
    }
    
    [Fact]
    public void CreateNew_NullValue_ThrowsArgumentException()
    {
        // Arrange
        string? value = null;
        var description = "Description";
        var displayOrder = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ExerciseType.Handler.CreateNew(value!, description, displayOrder));
        Assert.Contains("Value cannot be empty", exception.Message);
    }
    
    [Fact]
    public void CreateNew_NullDescription_CreatesExerciseTypeWithNullDescription()
    {
        // Arrange
        var value = "Rest";
        string? description = null;
        var displayOrder = 4;
        
        // Act
        var exerciseType = ExerciseType.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.Null(exerciseType.Description);
    }
    
    [Fact]
    public void ExerciseType_WithSameValues_ComparePropertiesIndividually()
    {
        // Arrange
        var id = ExerciseTypeId.New();
        var value = "Test";
        var description = "Test Description";
        var order = 1;
        var isActive = true;
        
        // Act
        var type1 = ExerciseType.Handler.Create(id, value, description, order, isActive);
        var type2 = ExerciseType.Handler.Create(id, value, description, order, isActive);
        
        // Assert - Records with collections don't have value equality by default
        // We need to compare properties individually
        Assert.Equal(type1.Id, type2.Id);
        Assert.Equal(type1.Value, type2.Value);
        Assert.Equal(type1.Description, type2.Description);
        Assert.Equal(type1.DisplayOrder, type2.DisplayOrder);
        Assert.Equal(type1.IsActive, type2.IsActive);
        
        // Two instances with same values but different collection references are not equal
        Assert.NotEqual(type1, type2);
        Assert.False(type1 == type2);
    }
    
    [Fact]
    public void ExerciseType_ExtendsReferenceDataBase()
    {
        // Arrange & Act
        var exerciseType = ExerciseType.Handler.CreateNew("Test", "Description", 1);
        
        // Assert
        Assert.IsAssignableFrom<ReferenceDataBase>(exerciseType);
    }
}