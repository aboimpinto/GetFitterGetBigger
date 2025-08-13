using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.MetricType;
using GetFitterGetBigger.API.Services.ReferenceTables.MetricType.DataServices;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for MetricTypeService using AutoMocker pattern
/// Tests the MetricType service layer with proper mocking and isolation
/// </summary>
public class MetricTypeServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var expectedMetricTypes = new[]
        {
            ReferenceDataDtoTestBuilder.Default().WithValue("Weight").Build(),
            ReferenceDataDtoTestBuilder.Default().WithValue("Distance").Build()
        };

        autoMocker.SetupReferenceDataCacheHitList(expectedMetricTypes);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMetricTypes);
        autoMocker.VerifyReferenceDataCacheGetListOnce();
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.GetAllActiveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllActiveAsync_CacheMiss_LoadsFromDataServiceAndCaches()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var expectedMetricTypes = new[]
        {
            ReferenceDataDtoTestBuilder.Default().WithValue("Weight").Build(),
            ReferenceDataDtoTestBuilder.Default().WithValue("Distance").Build()
        };

        autoMocker.SetupReferenceDataCacheMissList()
                  .SetupMetricTypeDataServiceGetAllActive(expectedMetricTypes);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMetricTypes);
        autoMocker.VerifyReferenceDataCacheGetListOnce()
                  .VerifyReferenceDataCacheSetListOnce();
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.GetAllActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_CacheHit_ReturnsFromCache()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var metricTypeId = MetricTypeId.New();
        var expectedMetricType = ReferenceDataDtoTestBuilder.Default().WithId(metricTypeId.ToString()).WithValue("Weight").Build();

        autoMocker.SetupReferenceDataCacheHit(expectedMetricType);

        // Act
        var result = await testee.GetByIdAsync(metricTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMetricType);
        autoMocker.VerifyReferenceDataCacheGetOnce();
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<MetricTypeId>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_CacheMiss_LoadsFromDataServiceAndCaches()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var metricTypeId = MetricTypeId.New();
        var expectedMetricType = ReferenceDataDtoTestBuilder.Default().WithId(metricTypeId.ToString()).WithValue("Weight").Build();

        autoMocker.SetupReferenceDataCacheMiss()
                  .SetupMetricTypeDataServiceGetById(metricTypeId, expectedMetricType);

        // Act
        var result = await testee.GetByIdAsync(metricTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedMetricType);
        autoMocker.VerifyReferenceDataCacheGetOnce()
                  .VerifyReferenceDataCacheSetOnce();
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.GetByIdAsync(metricTypeId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var emptyId = MetricTypeId.Empty;

        // Act
        var result = await testee.GetByIdAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IEternalCacheService>().Verify(x => x.GetAsync<ReferenceDataDto>(It.IsAny<string>()), Times.Never);
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.GetByIdAsync(It.IsAny<MetricTypeId>()), Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var metricTypeId = MetricTypeId.New();
        var metricTypeDto = ReferenceDataDtoTestBuilder.Default()
            .WithId(metricTypeId.ToString())
            .WithValue("Weight")
            .Build();

        autoMocker
            .SetupReferenceDataCacheMiss()
            .SetupMetricTypeDataServiceGetById(metricTypeId, metricTypeDto);

        // Act
        var result = await testee.ExistsAsync(metricTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.GetByIdAsync(metricTypeId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<MetricTypeService>();
        
        var emptyId = MetricTypeId.Empty;

        // Act
        var result = await testee.ExistsAsync(emptyId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        autoMocker.GetMock<IMetricTypeDataService>().Verify(x => x.ExistsAsync(It.IsAny<MetricTypeId>()), Times.Never);
    }
}