using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Extensions;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.Constants;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Extensions;

public class ExerciseWeightTypeMappingExtensionsTests
{
    [Fact]
    public void ToReferenceDataDto_WithValidEntity_ReturnsCorrectDto()
    {
        // Arrange
        var id = ExerciseWeightTypeTestConstants.BodyweightOnlyId;
        var entity = ExerciseWeightType.Handler.Create(
            id,
            ExerciseWeightTypeTestConstants.BodyweightOnlyCode,
            ExerciseWeightTypeTestConstants.BodyweightOnlyValue,
            ExerciseWeightTypeTestConstants.BodyweightOnlyDescription,
            1,
            true).Value;
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyValue, dto.Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyDescription, dto.Description);
    }
    
    [Fact]
    public void ToReferenceDataDto_WithEmptyEntity_ReturnsEmptyDto()
    {
        // Arrange
        var entity = ExerciseWeightType.Empty;
        
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.Value);
        Assert.Null(dto.Description);
    }
    
    [Fact]
    public void ToReferenceDataDto_WithEntityWithNullDescription_HandlesCorrectly()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var entity = ExerciseWeightType.Handler.Create(
            id,
            ExerciseWeightTypeTestConstants.TestCode,
            ExerciseWeightTypeTestConstants.TestValue,
            null,
            1,
            true).Value;
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal(ExerciseWeightTypeTestConstants.TestValue, dto.Value);
        Assert.Null(dto.Description);
    }
    
    [Fact]
    public void ToReferenceDataDto_PreservesIdFormat()
    {
        // Arrange
        var guid = ExerciseWeightTypeTestConstants.TestFixedGuid;
        var id = ExerciseWeightTypeId.From(guid);
        var entity = ExerciseWeightType.Handler.Create(
            id,
            ExerciseWeightTypeTestConstants.TestCode,
            ExerciseWeightTypeTestConstants.TestValue,
            ExerciseWeightTypeTestConstants.TestDescription,
            1,
            true).Value;
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(ExerciseWeightTypeTestConstants.TestFixedDtoId, dto.Id);
    }
    
    [Fact]
    public void ToReferenceDataDtos_WithMultipleEntities_ReturnsCorrectDtos()
    {
        // Arrange
        var entities = new List<ExerciseWeightType>
        {
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.BodyweightOnlyId,
                ExerciseWeightTypeTestConstants.BodyweightOnlyCode,
                ExerciseWeightTypeTestConstants.BodyweightOnlyValue,
                ExerciseWeightTypeTestConstants.DescriptionA,
                1,
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.BodyweightOptionalId,
                ExerciseWeightTypeTestConstants.WeightRequiredCode,
                ExerciseWeightTypeTestConstants.WeightRequiredValue,
                ExerciseWeightTypeTestConstants.DescriptionB,
                2,
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.WeightRequiredId,
                ExerciseWeightTypeTestConstants.NoWeightCode,
                ExerciseWeightTypeTestConstants.NoWeightValue,
                ExerciseWeightTypeTestConstants.DescriptionC,
                3,
                true).Value
        };
        
        // Act
        var dtos = entities.ToReferenceDataDtos().ToList();
        
        // Assert
        Assert.Equal(3, dtos.Count);
        
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyDtoId, dtos[0].Id);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyValue, dtos[0].Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.DescriptionA, dtos[0].Description);
        
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOptionalDtoId, dtos[1].Id);
        Assert.Equal(ExerciseWeightTypeTestConstants.WeightRequiredValue, dtos[1].Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.DescriptionB, dtos[1].Description);
        
        Assert.Equal(ExerciseWeightTypeTestConstants.WeightRequiredDtoId, dtos[2].Id);
        Assert.Equal(ExerciseWeightTypeTestConstants.NoWeightValue, dtos[2].Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.DescriptionC, dtos[2].Description);
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
                ExerciseWeightTypeTestConstants.CodeC,
                ExerciseWeightTypeTestConstants.ValueC,
                null,
                3,
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.New(),
                ExerciseWeightTypeTestConstants.CodeA,
                ExerciseWeightTypeTestConstants.ValueA,
                null,
                1,
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.New(),
                ExerciseWeightTypeTestConstants.CodeB,
                ExerciseWeightTypeTestConstants.ValueB,
                null,
                2,
                true).Value
        };
        
        // Act
        var dtos = entities.ToReferenceDataDtos().ToList();
        
        // Assert
        Assert.Equal(3, dtos.Count);
        Assert.Equal(ExerciseWeightTypeTestConstants.ValueC, dtos[0].Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.ValueA, dtos[1].Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.ValueB, dtos[2].Value);
    }
    
    [Fact]
    public void ToReferenceDataDto_HandlesInactiveEntity()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        var entity = ExerciseWeightType.Handler.Create(
            id,
            ExerciseWeightTypeTestConstants.InactiveCode,
            ExerciseWeightTypeTestConstants.InactiveValue,
            "This is inactive",
            1,
            false).Value; // Inactive
            
        // Act
        var dto = entity.ToReferenceDataDto();
        
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.ToString(), dto.Id);
        Assert.Equal(ExerciseWeightTypeTestConstants.InactiveValue, dto.Value);
        Assert.Equal("This is inactive", dto.Description);
        // Note: The DTO doesn't include IsActive property, which is correct for reference data DTOs
    }
}