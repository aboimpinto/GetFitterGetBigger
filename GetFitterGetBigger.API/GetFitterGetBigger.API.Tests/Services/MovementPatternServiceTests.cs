using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern;
using GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.DataServices;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for MovementPatternService using AutoMocker pattern
/// Tests the MovementPattern service layer with proper mocking and isolation
/// </summary>
public class MovementPatternServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var expectedMovementPatterns = new[]
        {
            ReferenceDataDtoTestBuilder.Default().WithValue("Push").Build(),
            ReferenceDataDtoTestBuilder.Default().WithValue("Pull").Build()
        };

        autoMocker.SetupReferenceDataCacheHitList(expectedMovementPatterns);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMovementPatterns);
        autoMocker.VerifyReferenceDataCacheGetListOnce();
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.GetAllActiveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllActiveAsync_CacheMiss_LoadsFromDataServiceAndCaches()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var expectedMovementPatterns = new[]
        {
            ReferenceDataDtoTestBuilder.Default().WithValue("Push").Build(),
            ReferenceDataDtoTestBuilder.Default().WithValue("Pull").Build()
        };

        autoMocker.SetupReferenceDataCacheMissList()
                  .SetupMovementPatternDataServiceGetAllActive(expectedMovementPatterns);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMovementPatterns);
        autoMocker.VerifyReferenceDataCacheGetListOnce()
                  .VerifyReferenceDataCacheSetListOnce();
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.GetAllActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var movementPatternId = MovementPatternId.New();
        var expectedMovementPattern = ReferenceDataDtoTestBuilder.Default().WithId(movementPatternId.ToString()).WithValue("Push").Build();

        autoMocker.SetupReferenceDataCacheHit(expectedMovementPattern);

        // Act
        var result = await testee.GetByIdAsync(movementPatternId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMovementPattern);
        autoMocker.VerifyReferenceDataCacheGetOnce();
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_CacheMiss_LoadsFromDataServiceAndCaches()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var movementPatternId = MovementPatternId.New();
        var expectedMovementPattern = ReferenceDataDtoTestBuilder.Default().WithId(movementPatternId.ToString()).WithValue("Push").Build();

        autoMocker.SetupReferenceDataCacheMiss()
                  .SetupMovementPatternDataServiceGetById(movementPatternId, expectedMovementPattern);

        // Act
        var result = await testee.GetByIdAsync(movementPatternId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMovementPattern);
        autoMocker.VerifyReferenceDataCacheGetOnce()
                  .VerifyReferenceDataCacheSetOnce();
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.GetByIdAsync(movementPatternId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var emptyId = MovementPatternId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IEternalCacheService>().Verify(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()), Times.Never);
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.GetByIdAsync(It.IsAny<MovementPatternId>()), Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var movementPatternId = MovementPatternId.New();
        var movementPatternDto = ReferenceDataDtoTestBuilder.Default()
            .WithId(movementPatternId.ToString())
            .WithValue("Push")
            .Build();

        autoMocker
            .SetupReferenceDataCacheMiss()
            .SetupMovementPatternDataServiceGetById(movementPatternId, movementPatternDto);

        // Act
        var result = await testee.ExistsAsync(movementPatternId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.GetByIdAsync(movementPatternId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MovementPatternService>();
        
        var emptyId = MovementPatternId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IMovementPatternDataService>().Verify(x => x.ExistsAsync(It.IsAny<MovementPatternId>()), Times.Never);
    }
}