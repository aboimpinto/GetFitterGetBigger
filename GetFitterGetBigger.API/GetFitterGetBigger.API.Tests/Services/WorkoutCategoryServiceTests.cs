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
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class WorkoutCategoryServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWorkoutCategoryRepository> _mockRepository;
    private readonly Mock<IEternalCacheService> _mockCacheService;
    private readonly Mock<ILogger<WorkoutCategoryService>> _mockLogger;
    private readonly WorkoutCategoryService _service;
    
    private readonly List<WorkoutCategory> _testData;
    
    public WorkoutCategoryServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IWorkoutCategoryRepository>();
        _mockCacheService = new Mock<IEternalCacheService>();
        _mockLogger = new Mock<ILogger<WorkoutCategoryService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutCategoryRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new WorkoutCategoryService(
            _mockUnitOfWorkProvider.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
            
        _testData = new List<WorkoutCategory>
        {
            WorkoutCategoryTestBuilder.UpperBodyPush().Build(),
            WorkoutCategoryTestBuilder.UpperBodyPull().Build(),
            WorkoutCategoryTestBuilder.LowerBody().Build(),
            WorkoutCategoryTestBuilder.Core().Build()
        };
    }
    
    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var cachedDtos = _testData.Select(MapToDto).ToList();
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutCategoryDto>>.Hit(cachedDtos));
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutCategoryDto>>.Miss());
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync<List<WorkoutCategoryDto>>(
            It.IsAny<string>(),
            It.IsAny<List<WorkoutCategoryDto>>()), Times.Once);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutCategoryDto>>.Miss());
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        
        // Act
        await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(WorkoutCategoryTestConstants.CacheKeys.AllCacheKey, capturedKey);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<WorkoutCategoryDto>>.Miss());
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        var dtos = result.Data.ToList();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, dtos.Count);
        for (int i = 0; i < _testData.Count; i++)
        {
            Assert.Equal(_testData[i].WorkoutCategoryId.ToString(), dtos[i].WorkoutCategoryId);
            Assert.Equal(_testData[i].Value, dtos[i].Value);
            Assert.Equal(_testData[i].Description, dtos[i].Description);
            Assert.Equal(_testData[i].Icon, dtos[i].Icon);
            Assert.Equal(_testData[i].Color, dtos[i].Color);
            Assert.Equal(_testData[i].PrimaryMuscleGroups, dtos[i].PrimaryMuscleGroups);
            Assert.Equal(_testData[i].DisplayOrder, dtos[i].DisplayOrder);
            Assert.Equal(_testData[i].IsActive, dtos[i].IsActive);
        }
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var category = _testData.First();
        var dto = MapToDto(category);
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Hit(dto));
            
        // Act
        var result = await _service.GetByIdAsync(category.WorkoutCategoryId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.WorkoutCategoryId.ToString(), result.Data.WorkoutCategoryId);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var category = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(category.WorkoutCategoryId))
            .ReturnsAsync(category);
            
        // Act
        var result = await _service.GetByIdAsync(category.WorkoutCategoryId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.WorkoutCategoryId.ToString(), result.Data.WorkoutCategoryId);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(category.WorkoutCategoryId), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync<WorkoutCategoryDto>(
            It.IsAny<string>(),
            It.Is<WorkoutCategoryDto>(wc => wc.WorkoutCategoryId == category.WorkoutCategoryId.ToString())), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByIdAsync(WorkoutCategoryId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveCategory_ReturnsEmpty()
    {
        // Arrange
        var inactiveCategory = WorkoutCategoryTestBuilder.InactiveCategory().Build();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(inactiveCategory.WorkoutCategoryId))
            .ReturnsAsync(WorkoutCategory.Empty);
            
        // Act
        var result = await _service.GetByIdAsync(inactiveCategory.WorkoutCategoryId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<WorkoutCategoryDto>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_ValidIdString_ReturnsDto()
    {
        // Arrange
        var category = _testData.First();
        var dto = MapToDto(category);
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Hit(dto));
            
        // Act
        var result = await _service.GetByIdAsync(category.WorkoutCategoryId.ToString());
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.WorkoutCategoryId.ToString(), result.Data.WorkoutCategoryId);
        Assert.Equal(category.Value, result.Data.Value);
        Assert.Equal(category.Description, result.Data.Description);
        Assert.Equal(category.Icon, result.Data.Icon);
        Assert.Equal(category.Color, result.Data.Color);
        Assert.Equal(category.PrimaryMuscleGroups, result.Data.PrimaryMuscleGroups);
    }
    
    
    [Fact]
    public async Task GetByValueAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var category = _testData.First();
        var dto = MapToDto(category);
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Hit(dto));
            
        // Act
        var result = await _service.GetByValueAsync(category.Value);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.Value, result.Data.Value);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
            
        var category = _testData.First();
        _mockRepository
            .Setup(x => x.GetByValueAsync(category.Value))
            .ReturnsAsync(category);
            
        // Act
        await _service.GetByValueAsync(category.Value);
        
        // Assert
        Assert.Equal(WorkoutCategoryTestConstants.CacheKeys.ValueCacheKey(category.Value), capturedKey);
    }
    
    [Fact]
    public async Task GetByValueAsync_EmptyValue_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByValueAsync("");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var category = _testData.First();
        var dto = MapToDto(category);
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Hit(dto));
            
        // Act
        var result = await _service.ExistsAsync(category.WorkoutCategoryId);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<WorkoutCategoryDto>.Miss());
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()))
            .ReturnsAsync(WorkoutCategory.Empty);
            
        // Act
        var result = await _service.ExistsAsync(WorkoutCategoryId.New());
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(WorkoutCategoryId.Empty);
        
        // Assert
        Assert.False(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategoryDto>(It.IsAny<string>()), Times.Never);
    }
    
    private static WorkoutCategoryDto MapToDto(WorkoutCategory entity) => new()
    {
        WorkoutCategoryId = entity.WorkoutCategoryId.ToString(),
        Value = entity.Value,
        Description = entity.Description,
        Icon = entity.Icon,
        Color = entity.Color,
        PrimaryMuscleGroups = entity.PrimaryMuscleGroups,
        DisplayOrder = entity.DisplayOrder,
        IsActive = entity.IsActive
    };
}