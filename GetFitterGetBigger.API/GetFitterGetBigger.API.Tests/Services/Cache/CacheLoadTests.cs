using System.Threading.Tasks;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.Cache;

public class CacheLoadTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IEternalCacheService> _mockEternalCacheService;
    private readonly Mock<ILogger<CacheLoadTests>> _mockLogger;

    public CacheLoadTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockEternalCacheService = new Mock<IEternalCacheService>();
        _mockLogger = new Mock<ILogger<CacheLoadTests>>();
    }

    [Fact]
    public async Task CacheLoad_WithRegularCacheService_OnHit_ExecutesOnHitFunction()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestData { Id = 1, Name = "Cached" };
        _mockCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync(cachedValue);

        // Act
        var result = await CacheLoad.For<TestData>(_mockCacheService.Object, key)
            .MatchAsync(
                onHit: data => $"Hit: {data.Name}",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss";
                });

        // Assert
        Assert.Equal("Hit: Cached", result);
        _mockCacheService.Verify(x => x.GetAsync<TestData>(key), Times.Once);
    }

    [Fact]
    public async Task CacheLoad_WithRegularCacheService_OnMiss_ExecutesOnMissFunction()
    {
        // Arrange
        var key = "test-key";
        _mockCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync((TestData?)null);

        // Act
        var result = await CacheLoad.For<TestData>(_mockCacheService.Object, key)
            .MatchAsync(
                onHit: data => "Hit",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss: Loaded from DB";
                });

        // Assert
        Assert.Equal("Miss: Loaded from DB", result);
        _mockCacheService.Verify(x => x.GetAsync<TestData>(key), Times.Once);
    }

    [Fact]
    public async Task CacheLoad_WithEternalCacheService_OnHit_ExecutesOnHitFunction()
    {
        // Arrange
        var key = "eternal-key";
        var cachedValue = new TestData { Id = 1, Name = "Eternal" };
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync(CacheResult<TestData>.Hit(cachedValue));

        // Act
        var result = await CacheLoad.For<TestData>(_mockEternalCacheService.Object, key)
            .MatchAsync(
                onHit: data => $"Hit: {data.Name}",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss";
                });

        // Assert
        Assert.Equal("Hit: Eternal", result);
        _mockEternalCacheService.Verify(x => x.GetAsync<TestData>(key), Times.Once);
    }

    [Fact]
    public async Task CacheLoad_WithEternalCacheService_OnMiss_ExecutesOnMissFunction()
    {
        // Arrange
        var key = "eternal-key";
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync(CacheResult<TestData>.Miss());

        // Act
        var result = await CacheLoad.For<TestData>(_mockEternalCacheService.Object, key)
            .MatchAsync(
                onHit: data => "Hit",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss: Loaded from DB";
                });

        // Assert
        Assert.Equal("Miss: Loaded from DB", result);
        _mockEternalCacheService.Verify(x => x.GetAsync<TestData>(key), Times.Once);
    }

    [Fact]
    public async Task CacheLoad_WithLogging_OnHit_LogsDebugMessage()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestData { Id = 1, Name = "Cached" };
        _mockCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync(cachedValue);

        // Act
        var result = await CacheLoad.For<TestData>(_mockCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .MatchAsync(
                onHit: data => $"Hit: {data.Name}",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss";
                });

        // Assert
        Assert.Equal("Hit: Cached", result);
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
    public async Task CacheLoad_WithLogging_OnMiss_LogsDebugMessage()
    {
        // Arrange
        var key = "test-key";
        _mockCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync((TestData?)null);

        // Act
        var result = await CacheLoad.For<TestData>(_mockCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .MatchAsync(
                onHit: data => "Hit",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss: Loaded";
                });

        // Assert
        Assert.Equal("Miss: Loaded", result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache miss")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EternalCacheLoad_WithLogging_OnHit_LogsDebugMessage()
    {
        // Arrange
        var key = "eternal-key";
        var cachedValue = new TestData { Id = 1, Name = "Eternal" };
        _mockEternalCacheService
            .Setup(x => x.GetAsync<TestData>(key))
            .ReturnsAsync(CacheResult<TestData>.Hit(cachedValue));

        // Act
        var result = await CacheLoad.For<TestData>(_mockEternalCacheService.Object, key)
            .WithLogging(_mockLogger.Object, "TestEntity")
            .MatchAsync(
                onHit: data => $"Hit: {data.Name}",
                onMiss: async () =>
                {
                    await Task.Delay(1);
                    return "Miss";
                });

        // Assert
        Assert.Equal("Hit: Eternal", result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache hit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}