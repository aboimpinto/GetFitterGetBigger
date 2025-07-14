using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class CacheServiceTests
{
    private readonly Mock<ILogger<CacheService>> _loggerMock;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cacheService;

    public CacheServiceTests()
    {
        _loggerMock = new Mock<ILogger<CacheService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new CacheService(_memoryCache, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenItemExists_ReturnsItem()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value.Id, result.Id);
        Assert.Equal(value.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_StoresItemInCache()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value.Id, result.Id);
    }

    [Fact]
    public async Task RemoveAsync_RemovesItemFromCache()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveByPatternAsync_RemovesMatchingItems()
    {
        // Arrange
        var pattern = "test-pattern:";
        var key1 = $"{pattern}item1";
        var key2 = $"{pattern}item2";
        var key3 = "different-pattern:item3";
        
        await _cacheService.SetAsync(key1, new TestObject { Id = 1 }, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(key2, new TestObject { Id = 2 }, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(key3, new TestObject { Id = 3 }, TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveByPatternAsync(pattern);

        // Assert
        Assert.Null(await _cacheService.GetAsync<TestObject>(key1));
        Assert.Null(await _cacheService.GetAsync<TestObject>(key2));
        Assert.NotNull(await _cacheService.GetAsync<TestObject>(key3));
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenItemExists_ReturnsCachedItem()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestObject { Id = 1, Name = "Cached" };
        await _cacheService.SetAsync(key, cachedValue, TimeSpan.FromMinutes(5));
        
        var factoryCalled = false;
        Func<Task<TestObject>> factory = async () =>
        {
            factoryCalled = true;
            await Task.Delay(1); // Simulate async work
            return new TestObject { Id = 2, Name = "New" };
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory, TimeSpan.FromMinutes(5));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedValue.Id, result.Id);
        Assert.Equal(cachedValue.Name, result.Name);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenItemDoesNotExist_CallsFactoryAndCachesResult()
    {
        // Arrange
        var key = "test-key";
        var newValue = new TestObject { Id = 2, Name = "New" };
        
        var factoryCalled = false;
        Func<Task<TestObject>> factory = async () =>
        {
            factoryCalled = true;
            await Task.Delay(1); // Simulate async work
            return newValue;
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, factory, TimeSpan.FromMinutes(5));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newValue.Id, result.Id);
        Assert.Equal(newValue.Name, result.Name);
        Assert.True(factoryCalled);
        
        // Verify item was cached
        var cachedResult = await _cacheService.GetAsync<TestObject>(key);
        Assert.NotNull(cachedResult);
        Assert.Equal(newValue.Id, cachedResult.Id);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}