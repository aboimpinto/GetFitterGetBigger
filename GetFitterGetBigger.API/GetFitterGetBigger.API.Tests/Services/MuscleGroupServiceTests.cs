using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for MuscleGroupService using AutoMocker pattern
/// Tests the MuscleGroup service layer with proper mocking and isolation
/// </summary>
public class MuscleGroupServiceTests
{
    // Cache tests removed - MuscleGroup is CRUD-enabled and doesn't use caching
    // [Fact]
    // public async Task GetAllActiveAsync_CacheHit_ReturnsFromCache()
    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MuscleGroupService>();
        
        var emptyId = MuscleGroupId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.VerifyMuscleGroupCacheGetNeverCalled()
                  .VerifyMuscleGroupDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task ExistsAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MuscleGroupService>();
        
        var muscleGroupId = MuscleGroupId.New();
        var muscleGroupDto = MuscleGroupDtoTestBuilder.Chest()
            .WithId(muscleGroupId.ToString())
            .Build();

        autoMocker
            .SetupMuscleGroupCacheMiss()
            .SetupMuscleGroupDataServiceGetById(muscleGroupId, muscleGroupDto);

        // Act
        var result = await testee.ExistsAsync(muscleGroupId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        autoMocker.VerifyMuscleGroupDataServiceGetByIdOnce(muscleGroupId);
    }

    [Fact]
    public async Task ExistsAsync_NonExistentId_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MuscleGroupService>();
        
        var muscleGroupId = MuscleGroupId.New();

        autoMocker
            .SetupMuscleGroupCacheMiss()
            .SetupMuscleGroupDataServiceGetByIdNotFound(muscleGroupId);

        // Act
        var result = await testee.ExistsAsync(muscleGroupId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        autoMocker.VerifyMuscleGroupDataServiceGetByIdOnce(muscleGroupId);
    }

    [Fact]
    public async Task ExistsAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MuscleGroupService>();
        
        var emptyId = MuscleGroupId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.VerifyMuscleGroupDataServiceExistsAsyncNeverCalled();
    }

}