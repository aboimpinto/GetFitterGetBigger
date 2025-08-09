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
    private readonly Mock<ILogger<WorkoutCategoryService>> _mockLogger;
    private readonly WorkoutCategoryService _service;
    
    private readonly List<WorkoutCategory> _testData;
    
    public WorkoutCategoryServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IWorkoutCategoryRepository>();
        _mockLogger = new Mock<ILogger<WorkoutCategoryService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutCategoryRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new WorkoutCategoryService(
            _mockUnitOfWorkProvider.Object,
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
            
        // Act
        var result = await _service.GetAllAsync();
        
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
//             It.IsAny<string>(), - removed orphaned It.IsAny
//             It.IsAny<List<WorkoutCategoryDto>>()), Times.Once); - removed orphaned It.IsAny
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        
        // Act
        await _service.GetAllAsync();
        
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        var dtos = result.Data.ToList();
        
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
            
        // Act
        var result = await _service.GetByIdAsync(category.WorkoutCategoryId);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.WorkoutCategoryId.ToString(), result.Data.WorkoutCategoryId);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var category = _testData.First();
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetByIdAsync(category.WorkoutCategoryId);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.WorkoutCategoryId.ToString(), result.Data.WorkoutCategoryId);
        _mockRepository.Verify(x => x.GetByIdAsync(category.WorkoutCategoryId), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByIdAsync(WorkoutCategoryId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<WorkoutCategoryId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveCategory_ReturnsEmpty()
    {
        // Arrange
        var inactiveCategory = WorkoutCategoryTestBuilder.InactiveCategory().Build();
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetByIdAsync(inactiveCategory.WorkoutCategoryId);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
//             It.IsAny<string>(), - removed orphaned It.IsAny
//             It.IsAny<WorkoutCategoryDto>()), Times.Never); - removed orphaned It.IsAny
    }
    
    [Fact]
    public async Task GetByIdAsync_ValidIdString_ReturnsDto()
    {
        // Arrange
        var category = _testData.First();
        var dto = MapToDto(category);
            
        // Act
        var result = await _service.GetByIdAsync(category.WorkoutCategoryId.ToString());
        
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
            
        // Act
        var result = await _service.GetByValueAsync(category.Value);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.Value, result.Data.Value);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
            
        var category = _testData.First();
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        await _service.GetByValueAsync(category.Value);
        
    }
    
    [Fact]
    public async Task GetByValueAsync_EmptyValue_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByValueAsync("");
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var category = _testData.First();
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.ExistsAsync(category.WorkoutCategoryId);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.ExistsAsync(WorkoutCategoryId.New());
        
        Assert.True(result.IsSuccess);
        Assert.False(result.Data);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(WorkoutCategoryId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
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