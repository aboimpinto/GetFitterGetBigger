using System;
using System.Linq;
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
        var result = ExerciseType.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.True(result.IsSuccess);
        var exerciseType = result.Value;
        Assert.NotEqual(default(ExerciseTypeId), exerciseType.ExerciseTypeId);
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
        var result = ExerciseType.Handler.CreateNew(value, description, displayOrder, isActive);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
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
        var result = ExerciseType.Handler.Create(id, value, description, displayOrder, isActive);
        
        // Assert
        Assert.True(result.IsSuccess);
        var exerciseType = result.Value;
        Assert.Equal(id, exerciseType.ExerciseTypeId);
        Assert.Equal(value, exerciseType.Value);
        Assert.Equal(description, exerciseType.Description);
        Assert.Equal(displayOrder, exerciseType.DisplayOrder);
        Assert.Equal(isActive, exerciseType.IsActive);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyValue_ReturnsFailure(string value)
    {
        // Arrange
        var description = "Description";
        var displayOrder = 1;
        
        // Act
        var result = ExerciseType.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Exercise type value cannot be empty", result.Errors.First());
    }
    
    [Fact]
    public void CreateNew_NullValue_ReturnsFailure()
    {
        // Arrange
        string? value = null;
        var description = "Description";
        var displayOrder = 1;
        
        // Act
        var result = ExerciseType.Handler.CreateNew(value!, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Exercise type value cannot be empty", result.Errors.First());
    }
    
    [Fact]
    public void CreateNew_NullDescription_CreatesExerciseTypeWithNullDescription()
    {
        // Arrange
        var value = "Rest";
        string? description = null;
        var displayOrder = 4;
        
        // Act
        var result = ExerciseType.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
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
        var result1 = ExerciseType.Handler.Create(id, value, description, order, isActive);
        var result2 = ExerciseType.Handler.Create(id, value, description, order, isActive);
        
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        var type1 = result1.Value;
        var type2 = result2.Value;
        
        // Assert - Since ExerciseType is a record without collections, 
        // two instances with the same values should be equal
        Assert.Equal(type1.ExerciseTypeId, type2.ExerciseTypeId);
        Assert.Equal(type1.Value, type2.Value);
        Assert.Equal(type1.Description, type2.Description);
        Assert.Equal(type1.DisplayOrder, type2.DisplayOrder);
        Assert.Equal(type1.IsActive, type2.IsActive);
        
        // Records with same values are equal
        Assert.Equal(type1, type2);
        Assert.True(type1 == type2);
    }
    
    [Fact]
    public void ExerciseType_ExtendsReferenceDataBase()
    {
        // Arrange & Act
        var result = ExerciseType.Handler.CreateNew("Test", "Description", 1);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsAssignableFrom<ReferenceDataBase>(result.Value);
    }
    
    [Fact]
    public void CreateNew_NegativeDisplayOrder_ReturnsFailure()
    {
        // Arrange
        var value = "Test";
        var description = "Description";
        var displayOrder = -1;
        
        // Act
        var result = ExerciseType.Handler.CreateNew(value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Display order must be non-negative", result.Errors.First());
    }
    
    [Fact]
    public void ExerciseType_ImplementsIEmptyEntity()
    {
        // Arrange & Act
        var empty = ExerciseType.Empty;
        
        // Assert
        Assert.True(empty.IsEmpty);
        Assert.Equal(ExerciseTypeId.Empty, empty.ExerciseTypeId);
        Assert.Equal(string.Empty, empty.Value);
        Assert.Null(empty.Description);
        Assert.Equal(0, empty.DisplayOrder);
        Assert.False(empty.IsActive);
    }
}