using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class EternalCacheServiceTests
{
    private readonly Mock<ILogger<EternalCacheService>> _loggerMock;
    private readonly IMemoryCache _memoryCache;
    private readonly IEternalCacheService _eternalCacheService;

    public EternalCacheServiceTests()
    {
        _loggerMock = new Mock<ILogger<EternalCacheService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _eternalCacheService = new EternalCacheService(_memoryCache, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenItemExists_ReturnsCacheHit()
    {
        // Arrange
        var key = "test-key";
        var value = new TestDto { Id = "1", Value = "Test" };
        await _eternalCacheService.SetAsync(key, value);

        // Act
        var result = await _eternalCacheService.GetAsync<TestDto>(key);

        // Assert
        Assert.True(result.IsHit);
        Assert.NotNull(result.Value);
        Assert.Equal(value.Id, result.Value.Id);
        Assert.Equal(value.Value, result.Value.Value);
    }

    [Fact]
    public async Task GetAsync_WhenItemDoesNotExist_ReturnsCacheMiss()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _eternalCacheService.GetAsync<TestDto>(key);

        // Assert
        Assert.False(result.IsHit);
        // Don't access Value on a cache miss - it throws an exception by design
    }

    [Fact]
    public async Task SetAsync_StoresItemWithEternalExpiration()
    {
        // Arrange
        var key = "eternal-key";
        var value = new TestDto { Id = "1", Value = "Eternal" };

        // Act
        await _eternalCacheService.SetAsync(key, value);
        var result = await _eternalCacheService.GetAsync<TestDto>(key);

        // Assert
        Assert.True(result.IsHit);
        Assert.NotNull(result.Value);
        Assert.Equal(value.Id, result.Value.Id);
        
        // Verify logger was called with eternal cache message
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("365 days")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrEmptyAsync_WhenItemExists_ReturnsItem()
    {
        // Arrange
        var key = "test-key";
        var value = new TestEmptyDto { Id = "1", Value = "Test" };
        await _eternalCacheService.SetAsync(key, value);

        // Act
        var result = await _eternalCacheService.GetOrEmptyAsync<TestEmptyDto>(key);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(value.Id, result.Id);
    }

    [Fact]
    public async Task GetOrEmptyAsync_WhenItemDoesNotExist_ReturnsEmpty()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _eternalCacheService.GetOrEmptyAsync<TestEmptyDto>(key);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(TestEmptyDto.Empty.Id, result.Id);
    }

    [Fact]
    public async Task GetOrCreateEmptyAwareAsync_WhenItemExists_ReturnsCachedItem()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = new TestEmptyDto { Id = "1", Value = "Cached" };
        await _eternalCacheService.SetAsync(key, cachedValue);
        
        var factoryCalled = false;
        async Task<TestEmptyDto> Factory()
        {
            factoryCalled = true;
            await Task.Delay(1);
            return new TestEmptyDto { Id = "2", Value = "New" };
        }

        // Act
        var result = await _eternalCacheService.GetOrCreateEmptyAwareAsync(key, Factory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedValue.Id, result.Id);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateEmptyAwareAsync_WhenItemDoesNotExist_CallsFactoryAndCaches()
    {
        // Arrange
        var key = "test-key";
        var newValue = new TestEmptyDto { Id = "2", Value = "New" };
        
        var factoryCalled = false;
        async Task<TestEmptyDto> Factory()
        {
            factoryCalled = true;
            await Task.Delay(1);
            return newValue;
        }

        // Act
        var result = await _eternalCacheService.GetOrCreateEmptyAwareAsync(key, Factory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newValue.Id, result.Id);
        Assert.True(factoryCalled);
        
        // Verify item was cached
        var cachedResult = await _eternalCacheService.GetAsync<TestEmptyDto>(key);
        Assert.True(cachedResult.IsHit);
        Assert.Equal(newValue.Id, cachedResult.Value.Id);
    }

    [Fact]
    public async Task GetOrCreateEmptyAwareAsync_WhenFactoryReturnsEmpty_DoesNotCache()
    {
        // Arrange
        var key = "test-key";
        
        async Task<TestEmptyDto> Factory()
        {
            await Task.Delay(1);
            return TestEmptyDto.Empty;
        }

        // Act
        var result = await _eternalCacheService.GetOrCreateEmptyAwareAsync(key, Factory);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        
        // Verify item was NOT cached
        var cachedResult = await _eternalCacheService.GetAsync<TestEmptyDto>(key);
        Assert.False(cachedResult.IsHit);
    }


    private class TestDto
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private class TestEmptyDto : IEmptyEntity<TestEmptyDto>
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        
        public bool IsEmpty => string.IsNullOrEmpty(Id);
        public bool IsActive { get; set; } = true;
        
        public static TestEmptyDto Empty { get; } = new() { Id = string.Empty, Value = string.Empty };
    }
}