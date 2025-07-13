using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExecutionProtocolServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IExecutionProtocolRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<ExecutionProtocolService>> _mockLogger;
    private readonly ExecutionProtocolService _service;
    
    private readonly List<ExecutionProtocol> _testData;
    
    public ExecutionProtocolServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IExecutionProtocolRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ExecutionProtocolService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IExecutionProtocolRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new ExecutionProtocolService(
            _mockUnitOfWorkProvider.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
            
        _testData = new List<ExecutionProtocol>
        {
            ExecutionProtocolTestBuilder.Standard().Build(),
            ExecutionProtocolTestBuilder.Superset().Build(),
            ExecutionProtocolTestBuilder.DropSet().Build(),
            ExecutionProtocolTestBuilder.AMRAP().Build()
        };
    }
    
    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExecutionProtocol>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<ExecutionProtocol>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExecutionProtocol>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<ExecutionProtocol>?)null);
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<ExecutionProtocol>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ExecutionProtocol>>(),
            It.Is<TimeSpan>(ts => ts.TotalHours == 1)), Times.Once);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExecutionProtocol>>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((IEnumerable<ExecutionProtocol>?)null);
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        
        // Act
        await _service.GetAllAsync();
        
        // Assert
        Assert.Equal("ReferenceTable:ExecutionProtocols:GetAll", capturedKey);
    }
    
    [Fact]
    public async Task GetAllAsDtosAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExecutionProtocol>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsDtosAsync();
        var dtos = result.ToList();
        
        // Assert
        Assert.Equal(_testData.Count, dtos.Count);
        for (int i = 0; i < _testData.Count; i++)
        {
            Assert.Equal(_testData[i].Id.ToString(), dtos[i].ExecutionProtocolId);
            Assert.Equal(_testData[i].Value, dtos[i].Value);
            Assert.Equal(_testData[i].Description, dtos[i].Description);
            Assert.Equal(_testData[i].Code, dtos[i].Code);
            Assert.Equal(_testData[i].TimeBase, dtos[i].TimeBase);
            Assert.Equal(_testData[i].RepBase, dtos[i].RepBase);
            Assert.Equal(_testData[i].RestPattern, dtos[i].RestPattern);
            Assert.Equal(_testData[i].IntensityLevel, dtos[i].IntensityLevel);
            Assert.Equal(_testData[i].DisplayOrder, dtos[i].DisplayOrder);
            Assert.Equal(_testData[i].IsActive, dtos[i].IsActive);
        }
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByIdAsync(protocol.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExecutionProtocolId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(protocol.Id))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByIdAsync(protocol.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(protocol.Id), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.Is<ExecutionProtocol>(ep => ep.Id == protocol.Id),
            It.Is<TimeSpan>(ts => ts.TotalHours == 1)), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(ExecutionProtocolId.Empty);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExecutionProtocolId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveProtocol_ReturnsNull()
    {
        // Arrange
        var inactiveProtocol = ExecutionProtocolTestBuilder.InactiveProtocol().Build();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(inactiveProtocol.Id))
            .ReturnsAsync(inactiveProtocol);
            
        // Act
        var result = await _service.GetByIdAsync(inactiveProtocol.Id);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ExecutionProtocol>(),
            It.IsAny<TimeSpan>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_ValidId_ReturnsDto()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByIdAsDtoAsync(protocol.Id.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Id.ToString(), result.ExecutionProtocolId);
        Assert.Equal(protocol.Value, result.Value);
        Assert.Equal(protocol.Description, result.Description);
        Assert.Equal(protocol.Code, result.Code);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_InvalidIdFormat_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsDtoAsync("invalid-id");
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByValueAsync(protocol.Value);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Value, result.Value);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((ExecutionProtocol?)null);
            
        var protocol = _testData.First();
        _mockRepository
            .Setup(x => x.GetByValueAsync(protocol.Value))
            .ReturnsAsync(protocol);
            
        // Act
        await _service.GetByValueAsync(protocol.Value);
        
        // Assert
        Assert.Equal($"ReferenceTable:ExecutionProtocols:GetByValue:{protocol.Value.ToLowerInvariant()}", capturedKey);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByCodeAsync(protocol.Code);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Code, result.Code);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        _mockRepository
            .Setup(x => x.GetByCodeAsync(protocol.Code))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByCodeAsync(protocol.Code);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Code, result.Code);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByCodeAsync(protocol.Code), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.Is<ExecutionProtocol>(ep => ep.Code == protocol.Code),
            It.Is<TimeSpan>(ts => ts.TotalHours == 1)), Times.Once);
    }
    
    [Fact]
    public async Task GetByCodeAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((ExecutionProtocol?)null);
            
        var protocol = _testData.First();
        _mockRepository
            .Setup(x => x.GetByCodeAsync(protocol.Code))
            .ReturnsAsync(protocol);
            
        // Act
        await _service.GetByCodeAsync(protocol.Code);
        
        // Assert
        Assert.Equal($"ExecutionProtocols:byCode:{protocol.Code.ToLowerInvariant()}", capturedKey);
    }
    
    [Fact]
    public async Task GetByCodeAsync_EmptyCode_ReturnsNull()
    {
        // Act
        var result = await _service.GetByCodeAsync("");
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByCodeAsDtoAsync_ValidCode_ReturnsDto()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.GetByCodeAsDtoAsync(protocol.Code);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(protocol.Id.ToString(), result.ExecutionProtocolId);
        Assert.Equal(protocol.Value, result.Value);
        Assert.Equal(protocol.Code, result.Code);
        Assert.Equal(protocol.TimeBase, result.TimeBase);
        Assert.Equal(protocol.RepBase, result.RepBase);
    }
    
    [Fact]
    public async Task GetByCodeAsDtoAsync_InvalidCode_ReturnsNull()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        _mockRepository
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        // Act
        var result = await _service.GetByCodeAsDtoAsync("INVALID_CODE");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var protocol = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync(protocol);
            
        // Act
        var result = await _service.ExistsAsync(protocol.Id);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<ExecutionProtocolId>()))
            .ReturnsAsync((ExecutionProtocol?)null);
            
        // Act
        var result = await _service.ExistsAsync(ExecutionProtocolId.New());
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(ExecutionProtocolId.Empty);
        
        // Assert
        Assert.False(result);
        _mockCacheService.Verify(x => x.GetAsync<ExecutionProtocol>(It.IsAny<string>()), Times.Never);
    }
}