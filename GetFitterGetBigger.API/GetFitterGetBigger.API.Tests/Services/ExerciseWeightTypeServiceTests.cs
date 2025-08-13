using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType.DataServices;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for ExerciseWeightTypeService using AutoMocker pattern
/// Tests the ExerciseWeightType service layer with proper mocking and isolation
/// </summary>
public class ExerciseWeightTypeServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var expectedExerciseWeightTypes = new[]
        {
            ReferenceDataDtoTestBuilder.Default().WithValue("Bodyweight").Build(),
            ReferenceDataDtoTestBuilder.Default().WithValue("Weighted").Build()
        };

        autoMocker.SetupReferenceDataCacheHitList(expectedExerciseWeightTypes);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedExerciseWeightTypes);
        autoMocker.VerifyReferenceDataCacheGetListOnce();
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.GetAllActiveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllActiveAsync_CacheMiss_LoadsFromDataServiceAndCaches()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var expectedExerciseWeightTypes = new[]
        {
            ReferenceDataDtoTestBuilder.Default().WithValue("Bodyweight").Build(),
            ReferenceDataDtoTestBuilder.Default().WithValue("Weighted").Build()
        };

        autoMocker.SetupReferenceDataCacheMissList()
                  .SetupExerciseWeightTypeDataServiceGetAllActive(expectedExerciseWeightTypes);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedExerciseWeightTypes);
        autoMocker.VerifyReferenceDataCacheGetListOnce()
                  .VerifyReferenceDataCacheSetListOnce();
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.GetAllActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var exerciseWeightTypeId = ExerciseWeightTypeId.New();
        var expectedExerciseWeightType = ReferenceDataDtoTestBuilder.Default().WithId(exerciseWeightTypeId.ToString()).WithValue("Bodyweight").Build();

        autoMocker.SetupReferenceDataCacheHit(expectedExerciseWeightType);

        // Act
        var result = await testee.GetByIdAsync(exerciseWeightTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedExerciseWeightType);
        autoMocker.VerifyReferenceDataCacheGetOnce();
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_CacheMiss_LoadsFromDataServiceAndCaches()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var exerciseWeightTypeId = ExerciseWeightTypeId.New();
        var expectedExerciseWeightType = ReferenceDataDtoTestBuilder.Default().WithId(exerciseWeightTypeId.ToString()).WithValue("Bodyweight").Build();

        autoMocker.SetupReferenceDataCacheMiss()
                  .SetupExerciseWeightTypeDataServiceGetById(exerciseWeightTypeId, expectedExerciseWeightType);

        // Act
        var result = await testee.GetByIdAsync(exerciseWeightTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedExerciseWeightType);
        autoMocker.VerifyReferenceDataCacheGetOnce()
                  .VerifyReferenceDataCacheSetOnce();
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.GetByIdAsync(exerciseWeightTypeId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var emptyId = ExerciseWeightTypeId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IEternalCacheService>().Verify(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()), Times.Never);
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var exerciseWeightTypeId = ExerciseWeightTypeId.New();
        var exerciseWeightTypeDto = ReferenceDataDtoTestBuilder.Default()
            .WithId(exerciseWeightTypeId.ToString())
            .WithValue("Free Weight")
            .Build();

        autoMocker
            .SetupReferenceDataCacheMiss()
            .SetupExerciseWeightTypeDataServiceGetById(exerciseWeightTypeId, exerciseWeightTypeDto);

        // Act
        var result = await testee.ExistsAsync(exerciseWeightTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.GetByIdAsync(exerciseWeightTypeId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExerciseWeightTypeService>();
        
        var emptyId = ExerciseWeightTypeId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IExerciseWeightTypeDataService>().Verify(x => x.ExistsAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
    }
}