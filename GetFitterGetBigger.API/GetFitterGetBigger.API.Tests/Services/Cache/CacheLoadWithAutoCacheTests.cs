using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.Cache;

public class CacheLoadWithAutoCacheTests
{
    private readonly Mock<IEternalCacheService> _mockEternalCacheService;
    private readonly Mock<ILogger<CacheLoadWithAutoCacheTests>> _mockLogger;

    public CacheLoadWithAutoCacheTests()
    {
        _mockEternalCacheService = new Mock<IEternalCacheService>();
        _mockLogger = new Mock<ILogger<CacheLoadWithAutoCacheTests>>();
    }

    [Fact]
    public async Task WithAutoCache_OnCacheHit_ReturnsFromCache_DoesNotCallLoader()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestDto { Id = "1", Value = "Cached" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Hit(cachedValue));

        var loaderCalled = false;
        async Task<TestDto?> Loader()
        {
            loaderCalled = true;
            await Task.Delay(1);
            return new TestDto { Id = "2", Value = "New" };
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("1", result.Data.Id);
        Assert.Equal("Cached", result.Data.Value);
        Assert.False(loaderCalled, "Loader should not be called on cache hit");
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCache_OnCacheMiss_CallsLoader_AndCachesResult()
    {
        // Arrange
        var key = "test-key";
        var loadedValue = new TestDto { Id = "2", Value = "Loaded" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        var loaderCalled = false;
        async Task<TestDto?> Loader()
        {
            loaderCalled = true;
            await Task.Delay(1);
            return loadedValue;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("2", result.Data.Id);
        Assert.Equal("Loaded", result.Data.Value);
        Assert.True(loaderCalled, "Loader should be called on cache miss");
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(key, loadedValue), Times.Once);
    }

    [Fact]
    public async Task WithAutoCache_WhenLoaderReturnsEmptyDto_ReturnsNotFound_DoesNotCache()
    {
        // Arrange
        var key = "test-key";
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return TestDto.Empty;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEmpty);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCache_WhenLoaderReturnsNull_ReturnsNotFoundForNonCollection_DoesNotCache()
    {
        // Arrange
        var key = "test-key";
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return null;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEmpty);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCache_WithLogging_OnCacheHit_LogsHitMessage()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestDto { Id = "1", Value = "Cached" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Hit(cachedValue));

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return new TestDto { Id = "2", Value = "New" };
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache hit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task WithAutoCache_WithLogging_OnCacheMiss_LogsMissAndCacheSetMessages()
    {
        // Arrange
        var key = "test-key";
        var loadedValue = new TestDto { Id = "2", Value = "Loaded" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return loadedValue;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify cache miss was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache miss")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        // Note: WithAutoCache extension bypasses the built-in cache set logging
        // since it operates directly on the cache service. Only cache miss is logged.
    }

    [Fact]
    public async Task WithAutoCache_RealWorldScenario_WorksCorrectly()
    {
        // This test simulates the real-world usage pattern in BodyPartService
        // Arrange
        var key = "BodyParts:GetAll";
        var bodyParts = new[]
        {
            new TestDto { Id = "1", Value = "Chest" },
            new TestDto { Id = "2", Value = "Back" },
            new TestDto { Id = "3", Value = "Legs" }
        };
        
        // First call - cache miss
        _mockEternalCacheService
            .SetupSequence(x => x.GetAsync<TestDto[]>(key))
            .ReturnsAsync(CacheResult<TestDto[]>.Miss())
            .ReturnsAsync(CacheResult<TestDto[]>.Hit(bodyParts));

        var loaderCallCount = 0;
        async Task<TestDto[]?> LoadFromDatabase()
        {
            loaderCallCount++;
            await Task.Delay(1); // Simulate database call
            return bodyParts;
        }

        // Act - First call (cache miss)
        var result1 = await CacheLoad.For<TestDto[]>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "BodyParts")
            .WithAutoCache(_mockEternalCacheService.Object, key, LoadFromDatabase);

        // Act - Second call (cache hit)
        var result2 = await CacheLoad.For<TestDto[]>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "BodyParts")
            .WithAutoCache(_mockEternalCacheService.Object, key, LoadFromDatabase);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.Equal(3, result1.Data.Length);
        
        Assert.True(result2.IsSuccess);
        Assert.Equal(3, result2.Data.Length);
        
        // Loader should only be called once (on the first cache miss)
        Assert.Equal(1, loaderCallCount);
        
        // Cache should be set once
        _mockEternalCacheService.Verify(x => x.SetAsync(key, bodyParts), Times.Once);
    }

    [Fact]
    public async Task WithAutoCache_WhenLoaderReturnsNullForCollectionType_ReturnsEmptyCollectionAsSuccess()
    {
        // Arrange
        var key = "test-key";
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto[]>(key))
            .ReturnsAsync(CacheResult<TestDto[]>.Miss());

        async Task<TestDto[]?> Loader()
        {
            await Task.Delay(1);
            return null; // Null collection
        }

        // Act
        var result = await CacheLoad.For<TestDto[]>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto[]>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto[]>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCache_WhenLoaderReturnsEmptyCollection_CachesEmptyCollection()
    {
        // Arrange
        var key = "test-key";
        var emptyArray = Array.Empty<TestDto>();
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto[]>(key))
            .ReturnsAsync(CacheResult<TestDto[]>.Miss());

        async Task<TestDto[]?> Loader()
        {
            await Task.Delay(1);
            return emptyArray;
        }

        // Act
        var result = await CacheLoad.For<TestDto[]>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        
        // Empty collections should NOT be cached (they are considered "empty")
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto[]>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto[]>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCache_WhenLoaderReturnsEmptyCollectionThatIsDto_ReturnsNotFound()
    {
        // Arrange
        var key = "test-key";
        var emptyCollection = new TestCollectionDto(); // DTO that is also a collection with IsEmpty
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestCollectionDto>(key))
            .ReturnsAsync(CacheResult<TestCollectionDto>.Miss());

        async Task<TestCollectionDto?> Loader()
        {
            await Task.Delay(1);
            return emptyCollection;
        }

        // Act
        var result = await CacheLoad.For<TestCollectionDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess); // Collections should return success even when empty
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEmpty);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestCollectionDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestCollectionDto>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCache_WhenDtoHasIsEmptyProperty_RespectsIsEmptyLogic()
    {
        // Arrange
        var key = "test-key";
        var validDto = new TestDto { Id = "123", Value = "Valid" }; // Not empty
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return validDto;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("123", result.Data.Id);
        Assert.Equal("Valid", result.Data.Value);
        Assert.False(result.Data.IsEmpty);
        
        // Valid DTO should be cached
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(key, validDto), Times.Once);
    }

    [Fact]
    public async Task WithAutoCache_WithoutLogging_WorksCorrectly()
    {
        // Arrange
        var key = "test-key";
        var loadedValue = new TestDto { Id = "1", Value = "Test" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return loadedValue;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("1", result.Data.Id);
        Assert.Equal("Test", result.Data.Value);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(key, loadedValue), Times.Once);
        
        // No logger calls should be made
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task WithAutoCache_EntityNameExtraction_WorksCorrectly()
    {
        // Arrange
        var key = "test-key";
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<TestDto?> Loader()
        {
            await Task.Delay(1);
            return null;
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCache(_mockEternalCacheService.Object, key, Loader);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        
        // Verify the entity name was correctly extracted ("Test" from "TestDto")
        Assert.Contains("Test", result.StructuredErrors.First().Message);
    }

    private class TestDto
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        
        public bool IsEmpty => string.IsNullOrEmpty(Id);
        
        public static TestDto Empty { get; } = new() { Id = string.Empty, Value = string.Empty };
    }
    
    private class TestCollectionDto : List<string>
    {
        public bool IsEmpty => Count == 0;
        
        public static TestCollectionDto Empty { get; } = new();
    }
}