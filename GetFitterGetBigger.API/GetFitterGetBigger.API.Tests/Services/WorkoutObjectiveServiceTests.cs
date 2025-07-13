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

public class WorkoutObjectiveServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWorkoutObjectiveRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<WorkoutObjectiveService>> _mockLogger;
    private readonly WorkoutObjectiveService _service;
    
    private readonly List<WorkoutObjective> _testData;
    
    public WorkoutObjectiveServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IWorkoutObjectiveRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<WorkoutObjectiveService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutObjectiveRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new WorkoutObjectiveService(
            _mockUnitOfWorkProvider.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
            
        _testData = new List<WorkoutObjective>
        {
            WorkoutObjectiveTestBuilder.MuscularStrength().Build(),
            WorkoutObjectiveTestBuilder.MuscularHypertrophy().Build(),
            WorkoutObjectiveTestBuilder.MuscularEndurance().Build(),
            WorkoutObjectiveTestBuilder.PowerDevelopment().Build()
        };
    }
    
    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutObjective>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<WorkoutObjective>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutObjective>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<WorkoutObjective>?)null);
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<WorkoutObjective>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<WorkoutObjective>>(),
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
            .Setup(x => x.GetAsync<IEnumerable<WorkoutObjective>>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((IEnumerable<WorkoutObjective>?)null);
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        
        // Act
        await _service.GetAllAsync();
        
        // Assert
        Assert.Equal("ReferenceTable:WorkoutObjectives:GetAll", capturedKey);
    }
    
    [Fact]
    public async Task GetAllAsDtosAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutObjective>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsDtosAsync();
        var dtos = result.ToList();
        
        // Assert
        Assert.Equal(_testData.Count, dtos.Count);
        for (int i = 0; i < _testData.Count; i++)
        {
            Assert.Equal(_testData[i].Id.ToString(), dtos[i].Id);
            Assert.Equal(_testData[i].Value, dtos[i].Value);
            Assert.Equal(_testData[i].Description, dtos[i].Description);
            // DisplayOrder and IsActive are not part of ReferenceDataDto
        }
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var objective = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync(objective);
            
        // Act
        var result = await _service.GetByIdAsync(objective.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(objective.Id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var objective = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync((WorkoutObjective?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(objective.Id))
            .ReturnsAsync(objective);
            
        // Act
        var result = await _service.GetByIdAsync(objective.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(objective.Id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(objective.Id), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.Is<WorkoutObjective>(wo => wo.Id == objective.Id),
            It.Is<TimeSpan>(ts => ts.TotalHours == 1)), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(WorkoutObjectiveId.Empty);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveObjective_ReturnsNull()
    {
        // Arrange
        var inactiveObjective = WorkoutObjectiveTestBuilder.InactiveObjective().Build();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync((WorkoutObjective?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(inactiveObjective.Id))
            .ReturnsAsync(inactiveObjective);
            
        // Act
        var result = await _service.GetByIdAsync(inactiveObjective.Id);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<WorkoutObjective>(),
            It.IsAny<TimeSpan>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_ValidId_ReturnsDto()
    {
        // Arrange
        var objective = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync(objective);
            
        // Act
        var result = await _service.GetByIdAsDtoAsync(objective.Id.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(objective.Id.ToString(), result.Id);
        Assert.Equal(objective.Value, result.Value);
        Assert.Equal(objective.Description, result.Description);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_InvalidIdFormat_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsDtoAsync("invalid-id");
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var objective = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync(objective);
            
        // Act
        var result = await _service.GetByValueAsync(objective.Value);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(objective.Value, result.Value);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((WorkoutObjective?)null);
            
        var objective = _testData.First();
        _mockRepository
            .Setup(x => x.GetByValueAsync(objective.Value))
            .ReturnsAsync(objective);
            
        // Act
        await _service.GetByValueAsync(objective.Value);
        
        // Assert
        Assert.Equal($"ReferenceTable:WorkoutObjectives:GetByValue:{objective.Value.ToLowerInvariant()}", capturedKey);
    }
    
    [Fact]
    public async Task GetByValueAsync_EmptyValue_ReturnsNull()
    {
        // Act
        var result = await _service.GetByValueAsync("");
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var objective = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync(objective);
            
        // Act
        var result = await _service.ExistsAsync(objective.Id);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()))
            .ReturnsAsync((WorkoutObjective?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()))
            .ReturnsAsync((WorkoutObjective?)null);
            
        // Act
        var result = await _service.ExistsAsync(WorkoutObjectiveId.New());
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(WorkoutObjectiveId.Empty);
        
        // Assert
        Assert.False(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutObjective>(It.IsAny<string>()), Times.Never);
    }
}