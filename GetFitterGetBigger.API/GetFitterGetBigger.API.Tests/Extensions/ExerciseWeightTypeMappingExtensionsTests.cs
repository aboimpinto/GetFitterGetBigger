using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Extensions;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Extensions;

public class ExerciseWeightTypeMappingExtensionsTests
{
    [Fact]
    public void ToReferenceDataDto_WithValidEntity_ReturnsCorrectDto()
    {
        // Arrange
        var id = ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"));
        var entity = ExerciseWeightType.Handler.Create(
            id,
            "BODYWEIGHT_ONLY",
            "Bodyweight Only",
            "Exercises that cannot have external weight added",
            1,
            true);
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal("Bodyweight Only", dto.Value);
        Assert.Equal("Exercises that cannot have external weight added", dto.Description);
    }
    
    [Fact]
    public void ToReferenceDataDto_WithNullEntity_ReturnsNull()
    {
        // Arrange
        ExerciseWeightType? entity = null;
        
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.Null(dto);
    }
    
    [Fact]
    public void ToReferenceDataDto_WithEntityWithNullDescription_HandlesCorrectly()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var entity = ExerciseWeightType.Handler.Create(
            id,
            "TEST_CODE",
            "Test Value",
            null,
            1,
            true);
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal("Test Value", dto.Value);
        Assert.Null(dto.Description);
    }
    
    [Fact]
    public void ToReferenceDataDto_PreservesIdFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var id = ExerciseWeightTypeId.From(guid);
        var entity = ExerciseWeightType.Handler.Create(
            id,
            "CODE",
            "Value",
            "Description",
            1,
            true);
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal($"exerciseweighttype-{guid}", dto.Id);
    }
    
    [Fact]
    public void ToReferenceDataDtos_WithMultipleEntities_ReturnsCorrectDtos()
    {
        // Arrange
        var entities = new List<ExerciseWeightType>
        {
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a")),
                "BODYWEIGHT_ONLY",
                "Bodyweight Only",
                "Description 1",
                1,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f")),
                "WEIGHT_REQUIRED",
                "Weight Required",
                "Description 2",
                2,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")),
                "NO_WEIGHT",
                "No Weight",
                "Description 3",
                3,
                true)
        };
        
        // Act
        var dtos = entities.ToReferenceDataDtos().ToList();
        
        // Assert
        Assert.Equal(3, dtos.Count);
        
        Assert.Equal("exerciseweighttype-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a", dtos[0].Id);
        Assert.Equal("Bodyweight Only", dtos[0].Value);
        Assert.Equal("Description 1", dtos[0].Description);
        
        Assert.Equal("exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f", dtos[1].Id);
        Assert.Equal("Weight Required", dtos[1].Value);
        Assert.Equal("Description 2", dtos[1].Description);
        
        Assert.Equal("exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a", dtos[2].Id);
        Assert.Equal("No Weight", dtos[2].Value);
        Assert.Equal("Description 3", dtos[2].Description);
    }
    
    [Fact]
    public void ToReferenceDataDtos_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        var entities = new List<ExerciseWeightType>();
        
        // Act
        var dtos = entities.ToReferenceDataDtos().ToList();
        
        // Assert
        Assert.Empty(dtos);
    }
    
    [Fact]
    public void ToReferenceDataDtos_PreservesOrder()
    {
        // Arrange
        var entities = new List<ExerciseWeightType>
        {
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.New(),
                "CODE_C",
                "Value C",
                null,
                3,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.New(),
                "CODE_A",
                "Value A",
                null,
                1,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.New(),
                "CODE_B",
                "Value B",
                null,
                2,
                true)
        };
        
        // Act
        var dtos = entities.ToReferenceDataDtos().ToList();
        
        // Assert
        Assert.Equal(3, dtos.Count);
        Assert.Equal("Value C", dtos[0].Value);
        Assert.Equal("Value A", dtos[1].Value);
        Assert.Equal("Value B", dtos[2].Value);
    }
    
    [Fact]
    public void ToReferenceDataDto_HandlesInactiveEntity()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var entity = ExerciseWeightType.Handler.Create(
            id,
            "INACTIVE_CODE",
            "Inactive Value",
            "This is inactive",
            1,
            false); // Inactive
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal("Inactive Value", dto.Value);
        Assert.Equal("This is inactive", dto.Description);
        // Note: The DTO doesn't include IsActive property, which is correct for reference data DTOs
    }
}