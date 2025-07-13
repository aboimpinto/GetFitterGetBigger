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

public class WorkoutCategoryServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWorkoutCategoryRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<WorkoutCategoryService>> _mockLogger;
    private readonly WorkoutCategoryService _service;
    
    private readonly List<WorkoutCategory> _testData;
    
    public WorkoutCategoryServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IWorkoutCategoryRepository>();
        _mockCacheService = new Mock<ICacheService>();
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
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategory>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<WorkoutCategory>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategory>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<WorkoutCategory>?)null);
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<WorkoutCategory>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<WorkoutCategory>>(),
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
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategory>>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((IEnumerable<WorkoutCategory>?)null);
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        
        // Act
        await _service.GetAllAsync();
        
        // Assert
        Assert.Equal("ReferenceTable:WorkoutCategories:GetAll", capturedKey);
    }
    
    [Fact]
    public async Task GetAllAsDtosAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<WorkoutCategory>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsDtosAsync();
        var dtos = result.ToList();
        
        // Assert
        Assert.Equal(_testData.Count, dtos.Count);
        for (int i = 0; i < _testData.Count; i++)
        {
            Assert.Equal(_testData[i].Id.ToString(), dtos[i].WorkoutCategoryId);
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
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync(category);
            
        // Act
        var result = await _service.GetByIdAsync(category.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var category = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync((WorkoutCategory?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(category.Id))
            .ReturnsAsync(category);
            
        // Act
        var result = await _service.GetByIdAsync(category.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(category.Id), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.Is<WorkoutCategory>(wc => wc.Id == category.Id),
            It.Is<TimeSpan>(ts => ts.TotalHours == 1)), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(WorkoutCategoryId.Empty);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveCategory_ReturnsNull()
    {
        // Arrange
        var inactiveCategory = WorkoutCategoryTestBuilder.InactiveCategory().Build();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync((WorkoutCategory?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(inactiveCategory.Id))
            .ReturnsAsync(inactiveCategory);
            
        // Act
        var result = await _service.GetByIdAsync(inactiveCategory.Id);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<WorkoutCategory>(),
            It.IsAny<TimeSpan>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_ValidId_ReturnsDto()
    {
        // Arrange
        var category = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync(category);
            
        // Act
        var result = await _service.GetByIdAsDtoAsync(category.Id.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id.ToString(), result.WorkoutCategoryId);
        Assert.Equal(category.Value, result.Value);
        Assert.Equal(category.Description, result.Description);
        Assert.Equal(category.Icon, result.Icon);
        Assert.Equal(category.Color, result.Color);
        Assert.Equal(category.PrimaryMuscleGroups, result.PrimaryMuscleGroups);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_InvalidIdFormat_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsDtoAsync("invalid-id");
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var category = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync(category);
            
        // Act
        var result = await _service.GetByValueAsync(category.Value);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Value, result.Value);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((WorkoutCategory?)null);
            
        var category = _testData.First();
        _mockRepository
            .Setup(x => x.GetByValueAsync(category.Value))
            .ReturnsAsync(category);
            
        // Act
        await _service.GetByValueAsync(category.Value);
        
        // Assert
        Assert.Equal($"ReferenceTable:WorkoutCategories:GetByValue:{category.Value.ToLowerInvariant()}", capturedKey);
    }
    
    [Fact]
    public async Task GetByValueAsync_EmptyValue_ReturnsNull()
    {
        // Act
        var result = await _service.GetByValueAsync("");
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var category = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync(category);
            
        // Act
        var result = await _service.ExistsAsync(category.Id);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()))
            .ReturnsAsync((WorkoutCategory?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()))
            .ReturnsAsync((WorkoutCategory?)null);
            
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
        _mockCacheService.Verify(x => x.GetAsync<WorkoutCategory>(It.IsAny<string>()), Times.Never);
    }
}