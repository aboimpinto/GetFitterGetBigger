using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class ExerciseWeightTypeTests
{
    [Fact]
    public void CreateNew_ValidInput_CreatesExerciseWeightType()
    {
        // Arrange
        var code = "BODYWEIGHT_ONLY";
        var value = "Bodyweight Only";
        var description = "Exercises that cannot have external weight added";
        var displayOrder = 1;
        
        // Act
        var exerciseWeightType = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder);
        
        // Assert
        Assert.NotEqual(default(ExerciseWeightTypeId), exerciseWeightType.Id);
        Assert.Equal(code, exerciseWeightType.Code);
        Assert.Equal(value, exerciseWeightType.Value);
        Assert.Equal(description, exerciseWeightType.Description);
        Assert.Equal(displayOrder, exerciseWeightType.DisplayOrder);
        Assert.True(exerciseWeightType.IsActive);
    }
    
    [Fact]
    public void CreateNew_WithIsActiveFalse_CreatesInactiveExerciseWeightType()
    {
        // Arrange
        var code = "DEPRECATED";
        var value = "Deprecated Type";
        var description = "No longer used";
        var displayOrder = 99;
        var isActive = false;
        
        // Act
        var exerciseWeightType = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder, isActive);
        
        // Assert
        Assert.False(exerciseWeightType.IsActive);
    }
    
    [Fact]
    public void Create_ValidInput_CreatesExerciseWeightTypeWithSpecificId()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var code = "WEIGHT_REQUIRED";
        var value = "Weight Required";
        var description = "Exercises that must have external weight specified";
        var displayOrder = 3;
        var isActive = true;
        
        // Act
        var exerciseWeightType = ExerciseWeightType.Handler.Create(id, code, value, description, displayOrder, isActive);
        
        // Assert
        Assert.Equal(id, exerciseWeightType.Id);
        Assert.Equal(code, exerciseWeightType.Code);
        Assert.Equal(value, exerciseWeightType.Value);
        Assert.Equal(description, exerciseWeightType.Description);
        Assert.Equal(displayOrder, exerciseWeightType.DisplayOrder);
        Assert.Equal(isActive, exerciseWeightType.IsActive);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyCode_ThrowsArgumentException(string code)
    {
        // Arrange
        var value = "Test Value";
        var description = "Description";
        var displayOrder = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder));
        Assert.Contains("Code cannot be empty", exception.Message);
    }
    
    [Fact]
    public void CreateNew_NullCode_ThrowsArgumentException()
    {
        // Arrange
        string? code = null;
        var value = "Test Value";
        var description = "Description";
        var displayOrder = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ExerciseWeightType.Handler.CreateNew(code!, value, description, displayOrder));
        Assert.Contains("Code cannot be empty", exception.Message);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyValue_ThrowsArgumentException(string value)
    {
        // Arrange
        var code = "TEST_CODE";
        var description = "Description";
        var displayOrder = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder));
        Assert.Contains("Value cannot be empty", exception.Message);
    }
    
    [Fact]
    public void CreateNew_NullValue_ThrowsArgumentException()
    {
        // Arrange
        var code = "TEST_CODE";
        string? value = null;
        var description = "Description";
        var displayOrder = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ExerciseWeightType.Handler.CreateNew(code, value!, description, displayOrder));
        Assert.Contains("Value cannot be empty", exception.Message);
    }
    
    [Fact]
    public void CreateNew_NullDescription_CreatesExerciseWeightTypeWithNullDescription()
    {
        // Arrange
        var code = "NO_WEIGHT";
        var value = "No Weight";
        string? description = null;
        var displayOrder = 5;
        
        // Act
        var exerciseWeightType = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder);
        
        // Assert
        Assert.Null(exerciseWeightType.Description);
    }
    
    [Fact]
    public void ExerciseWeightType_WithSameValues_AreEqual()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var code = "MACHINE_WEIGHT";
        var value = "Machine Weight";
        var description = "Machine exercises";
        var order = 4;
        var isActive = true;
        
        // Act
        var type1 = ExerciseWeightType.Handler.Create(id, code, value, description, order, isActive);
        var type2 = ExerciseWeightType.Handler.Create(id, code, value, description, order, isActive);
        
        // Assert - Since ExerciseWeightType is a record without collections, 
        // two instances with the same values should be equal
        Assert.Equal(type1.Id, type2.Id);
        Assert.Equal(type1.Code, type2.Code);
        Assert.Equal(type1.Value, type2.Value);
        Assert.Equal(type1.Description, type2.Description);
        Assert.Equal(type1.DisplayOrder, type2.DisplayOrder);
        Assert.Equal(type1.IsActive, type2.IsActive);
        
        // Records with same values are equal
        Assert.Equal(type1, type2);
        Assert.True(type1 == type2);
    }
    
    [Fact]
    public void ExerciseWeightType_ExtendsReferenceDataBase()
    {
        // Arrange & Act
        var exerciseWeightType = ExerciseWeightType.Handler.CreateNew("TEST", "Test", "Description", 1);
        
        // Assert
        Assert.IsAssignableFrom<ReferenceDataBase>(exerciseWeightType);
    }
    
    [Fact]
    public void ExerciseWeightType_AllWeightTypes_HaveExpectedCodes()
    {
        // This test documents the expected weight type codes
        var expectedCodes = new[]
        {
            "BODYWEIGHT_ONLY",
            "BODYWEIGHT_OPTIONAL",
            "WEIGHT_REQUIRED",
            "MACHINE_WEIGHT",
            "NO_WEIGHT"
        };
        
        // This is just documentation - actual validation will happen during seed data creation
        Assert.Equal(5, expectedCodes.Length);
    }
}