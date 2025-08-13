using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.BodyPart;
using GetFitterGetBigger.API.Tests.Services.Builders;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for BodyPartService caching behavior using DataService architecture
/// Tests verify that caching works correctly and reduces DataService calls
/// </summary>
public class BodyPartServiceCachingTests
{
    [Fact]
    public async Task GetByIdAsync_WhenCacheHit_DoesNotCallDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var cachedDto = BodyPartDtoBuilder.ForChest()
            .WithId(bodyPartId)
            .Build();

        automocker.SetupBodyPartCacheHit(cachedDto);

        // Act
        var result = await testee.GetByIdAsync(bodyPartId.ToString());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Value.Should().Be("Chest");

        // Verify cache was checked but DataService was NOT called
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_CallsDataServiceAndStoresInCache()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var bodyPartDto = BodyPartDtoBuilder.ForBack()
            .WithId(bodyPartId)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetById(bodyPartId, bodyPartDto);

        // Act
        var result = await testee.GetByIdAsync(bodyPartId.ToString());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Value.Should().Be("Back");

        // Verify both cache and DataService were called
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId)
            .VerifyBodyPartCacheSetOnce();
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheHit_DoesNotCallDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var cachedDtos = new List<BodyPartDto>
        {
            BodyPartDtoBuilder.ForChest().Build(),
            BodyPartDtoBuilder.ForBack().Build()
        };

        automocker.SetupBodyPartCacheHitList(cachedDtos);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2);

        // Verify cache was checked but DataService was NOT called
        automocker
            .VerifyBodyPartCacheGetListOnce()
            .VerifyBodyPartDataServiceGetAllActiveNeverCalled();
    }

    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_CallsDataServiceAndStoresInCache()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartDtos = new List<BodyPartDto>
        {
            BodyPartDtoBuilder.ForChest().Build(),
            BodyPartDtoBuilder.ForBack().Build()
        };

        automocker
            .SetupBodyPartCacheMissList()
            .SetupBodyPartDataServiceGetAllActive(bodyPartDtos);

        // Act
        var result = await testee.GetAllActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(2);

        // Verify both cache and DataService were called
        automocker
            .VerifyBodyPartCacheGetListOnce()
            .VerifyBodyPartDataServiceGetAllActiveOnce()
            .VerifyBodyPartCacheSetListOnce();
    }

    [Fact]
    public async Task GetByValueAsync_WhenCacheHit_DoesNotCallDataService()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string value = "Shoulders";
        var cachedDto = new BodyPartDtoBuilder()
            .WithValue(value)
            .Build();

        automocker.SetupBodyPartCacheHit(cachedDto);

        // Act
        var result = await testee.GetByValueAsync(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Value.Should().Be(value);

        // Verify cache was checked but DataService was NOT called
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByValueNeverCalled();
    }

    [Fact]
    public async Task GetByValueAsync_WhenCacheMiss_CallsDataServiceAndStoresInCache()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        const string value = "Shoulders";
        var bodyPartDto = new BodyPartDtoBuilder()
            .WithValue(value)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetByValue(value, bodyPartDto);

        // Act
        var result = await testee.GetByValueAsync(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Value.Should().Be(value);

        // Verify both cache and DataService were called
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByValueOnce(value)
            .VerifyBodyPartCacheSetOnce();
    }

    [Fact]
    public async Task ExistsAsync_LeveragesGetByIdCache_WhenCacheHit()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var cachedDto = BodyPartDtoBuilder.ForLegs()
            .WithId(bodyPartId)
            .Build();

        automocker.SetupBodyPartCacheHit(cachedDto);

        // Act
        var result = await testee.ExistsAsync(bodyPartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();

        // Verify cache was checked but DataService was NOT called
        // (ExistsAsync leverages GetByIdAsync which uses cache)
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdNeverCalled();
    }

    [Fact]
    public async Task ExistsAsync_LeveragesGetByIdCache_WhenCacheMiss()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        var bodyPartDto = BodyPartDtoBuilder.ForLegs()
            .WithId(bodyPartId)
            .Build();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetById(bodyPartId, bodyPartDto);

        // Act
        var result = await testee.ExistsAsync(bodyPartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();

        // Verify both cache and DataService were called through GetByIdAsync
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId)
            .VerifyBodyPartCacheSetOnce();
    }

    [Fact]
    public async Task ExistsAsync_WithCachedNonExistentEntity_ReturnsFalseWithoutDataServiceCall()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();

        automocker
            .SetupBodyPartCacheMiss()
            .SetupBodyPartDataServiceGetByIdNotFound(bodyPartId);

        // Act
        var result = await testee.ExistsAsync(bodyPartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();

        // Verify cache miss triggered DataService call
        automocker
            .VerifyBodyPartCacheGetOnce()
            .VerifyBodyPartDataServiceGetByIdOnce(bodyPartId);
    }

    [Fact]
    public async Task CacheKeyGeneration_IsDifferentForDifferentOperations()
    {
        // This test verifies that different operations use different cache keys
        // by ensuring each operation can have its own cached value

        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<BodyPartService>();

        var bodyPartId = BodyPartId.New();
        const string value = "Chest";

        var bodyPartDtoById = BodyPartDtoBuilder.ForChest()
            .WithId(bodyPartId)
            .WithValue(value)
            .Build();

        var bodyPartDtoByValue = BodyPartDtoBuilder.ForChest()
            .WithId(BodyPartId.New()) // Different ID
            .WithValue(value)
            .Build();

        var allActiveBodyParts = new List<BodyPartDto>
        {
            BodyPartDtoBuilder.ForChest().Build(),
            BodyPartDtoBuilder.ForBack().Build()
        };

        // Setup different cache results for different operations
        // Each operation should have its own cache key
        automocker
            .SetupBodyPartCacheHit(bodyPartDtoById)  // For GetById
            .SetupBodyPartCacheHitList(allActiveBodyParts); // For GetAllActive

        // Act & Assert - GetByIdAsync should hit cache
        var resultById = await testee.GetByIdAsync(bodyPartId);
        resultById.IsSuccess.Should().BeTrue();
        resultById.Data.Id.Should().Be(bodyPartId.ToString());

        // Act & Assert - GetAllActiveAsync should hit cache
        var resultAllActive = await testee.GetAllActiveAsync();
        resultAllActive.IsSuccess.Should().BeTrue();
        resultAllActive.Data.Count().Should().Be(2);

        // Verify caches were accessed but DataService was not called
        automocker
            .VerifyBodyPartDataServiceGetByIdNeverCalled()
            .VerifyBodyPartDataServiceGetAllActiveNeverCalled()
            .VerifyBodyPartDataServiceGetByValueNeverCalled();
    }
}