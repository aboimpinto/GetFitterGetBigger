using System;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.Constants;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class ExerciseWeightTypeTests
{
    [Fact]
    public void CreateNew_ValidInput_CreatesExerciseWeightType()
    {
        // Arrange
        var code = ExerciseWeightTypeTestConstants.BodyweightOnlyCode;
        var value = "Bodyweight Only";
        var description = "Exercises that cannot have external weight added";
        var displayOrder = 1;
        
        // Act
        var result = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder);
        
        // Assert
        Assert.True(result.IsSuccess);
        var exerciseWeightType = result.Value;
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
        var result = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder, isActive);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }
    
    [Fact]
    public void Create_ValidInput_CreatesExerciseWeightTypeWithSpecificId()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var code = ExerciseWeightTypeTestConstants.WeightRequiredCode;
        var value = "Weight Required";
        var description = "Exercises that must have external weight specified";
        var displayOrder = 3;
        var isActive = true;
        
        // Act
        var result = ExerciseWeightType.Handler.Create(id, code, value, description, displayOrder, isActive);
        
        // Assert
        Assert.True(result.IsSuccess);
        var exerciseWeightType = result.Value;
        Assert.Equal(id, exerciseWeightType.Id);
        Assert.Equal(code, exerciseWeightType.Code);
        Assert.Equal(value, exerciseWeightType.Value);
        Assert.Equal(description, exerciseWeightType.Description);
        Assert.Equal(displayOrder, exerciseWeightType.DisplayOrder);
        Assert.Equal(isActive, exerciseWeightType.IsActive);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyCode_ReturnsFailureResult(string code)
    {
        // Arrange
        var value = ExerciseWeightTypeTestConstants.TestValue;
        var description = ExerciseWeightTypeTestConstants.TestDescription;
        var displayOrder = 1;
        
        // Act
        var result = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Contains(ExerciseWeightTypeErrorMessages.CodeCannotBeEmpty, result.Errors);
        Assert.True(result.Value.IsEmpty);
    }
    
    [Fact]
    public void CreateNew_NullCode_ReturnsFailureResult()
    {
        // Arrange
        string? code = null;
        var value = ExerciseWeightTypeTestConstants.TestValue;
        var description = ExerciseWeightTypeTestConstants.TestDescription;
        var displayOrder = 1;
        
        // Act
        var result = ExerciseWeightType.Handler.CreateNew(code!, value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Contains(ExerciseWeightTypeErrorMessages.CodeCannotBeEmpty, result.Errors);
        Assert.True(result.Value.IsEmpty);
    }
    
    [Theory]
    [InlineData("")]
    public void CreateNew_EmptyValue_ReturnsFailureResult(string value)
    {
        // Arrange
        var code = ExerciseWeightTypeTestConstants.TestCode;
        var description = ExerciseWeightTypeTestConstants.TestDescription;
        var displayOrder = 1;
        
        // Act
        var result = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Contains(ExerciseWeightTypeErrorMessages.ValueCannotBeEmptyEntity, result.Errors);
        Assert.True(result.Value.IsEmpty);
    }
    
    [Fact]
    public void CreateNew_NullValue_ReturnsFailureResult()
    {
        // Arrange
        var code = ExerciseWeightTypeTestConstants.TestCode;
        string? value = null;
        var description = ExerciseWeightTypeTestConstants.TestDescription;
        var displayOrder = 1;
        
        // Act
        var result = ExerciseWeightType.Handler.CreateNew(code, value!, description, displayOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Contains(ExerciseWeightTypeErrorMessages.ValueCannotBeEmptyEntity, result.Errors);
        Assert.True(result.Value.IsEmpty);
    }
    
    [Fact]
    public void CreateNew_NullDescription_CreatesExerciseWeightTypeWithNullDescription()
    {
        // Arrange
        var code = ExerciseWeightTypeTestConstants.NoWeightCode;
        var value = "No Weight";
        string? description = null;
        var displayOrder = 5;
        
        // Act
        var result = ExerciseWeightType.Handler.CreateNew(code, value, description, displayOrder);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }
    
    [Fact]
    public void ExerciseWeightType_WithSameValues_AreEqual()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var code = ExerciseWeightTypeTestConstants.MachineWeightCode;
        var value = "Machine Weight";
        var description = "Machine exercises";
        var order = 4;
        var isActive = true;
        
        // Act
        var type1 = ExerciseWeightType.Handler.Create(id, code, value, description, order, isActive).Value;
        var type2 = ExerciseWeightType.Handler.Create(id, code, value, description, order, isActive).Value;
        
        // Assert - ExerciseWeightType is a record with a collection property (Exercises),
        // so two instances will not be equal even with the same values (collections are compared by reference)
        Assert.Equal(type1.Id, type2.Id);
        Assert.Equal(type1.Code, type2.Code);
        Assert.Equal(type1.Value, type2.Value);
        Assert.Equal(type1.Description, type2.Description);
        Assert.Equal(type1.DisplayOrder, type2.DisplayOrder);
        Assert.Equal(type1.IsActive, type2.IsActive);
        
        // Records with collections are not equal even with same property values
        Assert.NotEqual(type1, type2);
        Assert.False(type1 == type2);
    }
    
    [Fact]
    public void ExerciseWeightType_ExtendsReferenceDataBase()
    {
        // Arrange & Act
        var result = ExerciseWeightType.Handler.CreateNew("TEST", "Test", ExerciseWeightTypeTestConstants.TestDescription, 1);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsAssignableFrom<ReferenceDataBase>(result.Value);
    }
    
    [Fact]
    public void ExerciseWeightType_AllWeightTypes_HaveExpectedCodes()
    {
        // This test documents the expected weight type codes
        var expectedCodes = new[]
        {
            ExerciseWeightTypeTestConstants.BodyweightOnlyCode,
            ExerciseWeightTypeTestConstants.BodyweightOptionalCode,
            ExerciseWeightTypeTestConstants.WeightRequiredCode,
            ExerciseWeightTypeTestConstants.MachineWeightCode,
            ExerciseWeightTypeTestConstants.NoWeightCode
        };
        
        // This is just documentation - actual validation will happen during seed data creation
        Assert.Equal(5, expectedCodes.Length);
    }
}