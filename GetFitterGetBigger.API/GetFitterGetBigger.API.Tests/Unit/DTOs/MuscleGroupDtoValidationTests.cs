using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Unit.DTOs;

public class MuscleGroupDtoValidationTests
{
    [Fact]
    public void CreateMuscleGroupDto_ValidData_PassesValidation()
    {
        // Arrange
        var dto = new CreateMuscleGroupDto
        {
            Name = "Biceps",
            BodyPartId = "bodypart-12345678-1234-1234-1234-123456789012"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }
    
    [Fact]
    public void CreateMuscleGroupDto_MissingName_FailsValidation()
    {
        // Arrange
        var dto = new CreateMuscleGroupDto
        {
            Name = "",
            BodyPartId = "bodypart-12345678-1234-1234-1234-123456789012"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.False(isValid);
        Assert.Single(results);
        Assert.Contains("required", results[0].ErrorMessage, System.StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("This is a very long muscle group name that exceeds the maximum allowed length of one hundred characters which should fail validation")]
    public void CreateMuscleGroupDto_InvalidNameLength_FailsValidation(string name)
    {
        // Arrange
        var dto = new CreateMuscleGroupDto
        {
            Name = name,
            BodyPartId = "bodypart-12345678-1234-1234-1234-123456789012"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.False(isValid);
        if (string.IsNullOrEmpty(name))
        {
            Assert.Contains("required", results[0].ErrorMessage, System.StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.Contains("between 1 and 100 characters", results[0].ErrorMessage);
        }
    }
    
    [Fact]
    public void CreateMuscleGroupDto_MissingBodyPartId_FailsValidation()
    {
        // Arrange
        var dto = new CreateMuscleGroupDto
        {
            Name = "Biceps",
            BodyPartId = ""
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.False(isValid);
        Assert.Single(results);
        Assert.Contains("required", results[0].ErrorMessage, System.StringComparison.OrdinalIgnoreCase);
    }
    
    [Theory]
    [InlineData("bodypart-12345678")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("musclegroup-12345678-1234-1234-1234-123456789012")]
    [InlineData("bodypart-XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX")]
    [InlineData("bodypart-12345678-1234-1234-1234-12345678901g")]
    public void CreateMuscleGroupDto_InvalidBodyPartIdFormat_FailsValidation(string bodyPartId)
    {
        // Arrange
        var dto = new CreateMuscleGroupDto
        {
            Name = "Biceps",
            BodyPartId = bodyPartId
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.False(isValid);
        Assert.Single(results);
        Assert.Contains("bodypart-{guid}", results[0].ErrorMessage);
    }
    
    [Fact]
    public void UpdateMuscleGroupDto_ValidData_PassesValidation()
    {
        // Arrange
        var dto = new UpdateMuscleGroupDto
        {
            Name = "Triceps",
            BodyPartId = "bodypart-12345678-1234-1234-1234-123456789012"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }
    
    [Fact]
    public void UpdateMuscleGroupDto_MissingName_FailsValidation()
    {
        // Arrange
        var dto = new UpdateMuscleGroupDto
        {
            Name = "",
            BodyPartId = "bodypart-12345678-1234-1234-1234-123456789012"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.False(isValid);
        Assert.Single(results);
        Assert.Contains("required", results[0].ErrorMessage, System.StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void UpdateMuscleGroupDto_InvalidBodyPartIdFormat_FailsValidation()
    {
        // Arrange
        var dto = new UpdateMuscleGroupDto
        {
            Name = "Triceps",
            BodyPartId = "invalid-format"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);
        
        // Assert
        Assert.False(isValid);
        Assert.Single(results);
        Assert.Contains("bodypart-{guid}", results[0].ErrorMessage);
    }
}