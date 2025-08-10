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
    public async Task WithAutoCacheAsync_OnCacheHit_ReturnsFromCache_DoesNotCallLoader()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestDto { Id = "1", Value = "Cached" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Hit(cachedValue));

        var loaderCalled = false;
        async Task<ServiceResult<TestDto>> Loader()
        {
            loaderCalled = true;
            await Task.Delay(1);
            return ServiceResult<TestDto>.Success(new TestDto { Id = "2", Value = "New" });
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCacheAsync(Loader);

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
    public async Task WithAutoCacheAsync_OnCacheMiss_CallsLoader_AndCachesResult()
    {
        // Arrange
        var key = "test-key";
        var loadedValue = new TestDto { Id = "2", Value = "Loaded" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        var loaderCalled = false;
        async Task<ServiceResult<TestDto>> Loader()
        {
            loaderCalled = true;
            await Task.Delay(1);
            return ServiceResult<TestDto>.Success(loadedValue);
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCacheAsync(Loader);

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
    public async Task WithAutoCacheAsync_WhenLoaderReturnsFailure_DoesNotCache()
    {
        // Arrange
        var key = "test-key";
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<ServiceResult<TestDto>> Loader()
        {
            await Task.Delay(1);
            return ServiceResult<TestDto>.Failure(
                TestDto.Empty,
                ServiceError.NotFound("TestDto", "123"));
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCacheAsync(Loader);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEmpty);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCacheAsync_WhenLoaderReturnsNull_DoesNotCache()
    {
        // Arrange
        var key = "test-key";
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<ServiceResult<TestDto>> Loader()
        {
            await Task.Delay(1);
            return ServiceResult<TestDto>.Success(null!); // Null data
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithAutoCacheAsync(Loader);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
        
        _mockEternalCacheService.Verify(x => x.GetAsync<TestDto>(key), Times.Once);
        _mockEternalCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestDto>()), Times.Never);
    }

    [Fact]
    public async Task WithAutoCacheAsync_WithLogging_OnCacheHit_LogsHitMessage()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestDto { Id = "1", Value = "Cached" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Hit(cachedValue));

        async Task<ServiceResult<TestDto>> Loader()
        {
            await Task.Delay(1);
            return ServiceResult<TestDto>.Success(new TestDto { Id = "2", Value = "New" });
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .WithAutoCacheAsync(Loader);

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
    public async Task WithAutoCacheAsync_WithLogging_OnCacheMiss_LogsMissAndCacheSetMessages()
    {
        // Arrange
        var key = "test-key";
        var loadedValue = new TestDto { Id = "2", Value = "Loaded" };
        
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestDto>(key))
            .ReturnsAsync(CacheResult<TestDto>.Miss());

        async Task<ServiceResult<TestDto>> Loader()
        {
            await Task.Delay(1);
            return ServiceResult<TestDto>.Success(loadedValue);
        }

        // Act
        var result = await CacheLoad.For<TestDto>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .WithAutoCacheAsync(Loader);

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
        
        // Verify cache set was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cached")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task WithAutoCacheAsync_RealWorldScenario_WorksCorrectly()
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
        async Task<ServiceResult<TestDto[]>> LoadFromDatabase()
        {
            loaderCallCount++;
            await Task.Delay(1); // Simulate database call
            return ServiceResult<TestDto[]>.Success(bodyParts);
        }

        // Act - First call (cache miss)
        var result1 = await CacheLoad.For<TestDto[]>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "BodyParts")
            .WithAutoCacheAsync(LoadFromDatabase);

        // Act - Second call (cache hit)
        var result2 = await CacheLoad.For<TestDto[]>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "BodyParts")
            .WithAutoCacheAsync(LoadFromDatabase);

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

    private class TestDto
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        
        public bool IsEmpty => string.IsNullOrEmpty(Id);
        
        public static TestDto Empty { get; } = new() { Id = string.Empty, Value = string.Empty };
    }
}