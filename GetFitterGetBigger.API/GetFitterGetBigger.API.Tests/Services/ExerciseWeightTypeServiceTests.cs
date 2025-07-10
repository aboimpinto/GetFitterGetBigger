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
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExerciseWeightTypeServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IExerciseWeightTypeRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<ExerciseWeightTypeService>> _mockLogger;
    private readonly ExerciseWeightTypeService _service;
    
    private readonly List<ExerciseWeightType> _testData;
    
    public ExerciseWeightTypeServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IExerciseWeightTypeRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ExerciseWeightTypeService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IExerciseWeightTypeRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new ExerciseWeightTypeService(
            _mockUnitOfWorkProvider.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
            
        // Initialize test data
        _testData = new List<ExerciseWeightType>
        {
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a")),
                "BODYWEIGHT_ONLY",
                "Bodyweight Only",
                "Exercises that cannot have external weight added",
                1,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f")),
                "BODYWEIGHT_OPTIONAL",
                "Bodyweight Optional",
                "Exercises that can be performed with or without additional weight",
                2,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")),
                "WEIGHT_REQUIRED",
                "Weight Required",
                "Exercises that must have external weight specified",
                3,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b")),
                "MACHINE_WEIGHT",
                "Machine Weight",
                "Exercises performed on machines with weight stacks",
                4,
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c")),
                "NO_WEIGHT",
                "No Weight",
                "Exercises that do not use weight as a metric",
                5,
                true)
        };
    }
    
    #region GetAllAsync Tests
    
    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExerciseWeightType>>(It.IsAny<string>()))
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<ExerciseWeightType>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExerciseWeightType>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<ExerciseWeightType>?)null);
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(_testData.Count, result.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<ExerciseWeightType>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ExerciseWeightType>>(),
            It.Is<TimeSpan>(ts => ts.TotalHours == 24)), Times.Once);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    #endregion
    
    #region GetAllAsDtosAsync Tests
    
    [Fact]
    public async Task GetAllAsDtosAsync_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExerciseWeightType>>(It.IsAny<string>()))
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
        }
    }
    
    #endregion
    
    #region GetByIdAsync Tests
    
    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var id = _testData[0].Id;
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[0]);
            
        // Act
        var result = await _service.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockCacheService.Verify(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var id = _testData[0].Id;
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(_testData[0]);
            
        // Act
        var result = await _service.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ExerciseWeightType>(),
            It.Is<TimeSpan>(ts => ts.TotalHours == 24)), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        // Act
        var result = await _service.GetByIdAsync(id);
        
        // Assert
        Assert.Null(result);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ExerciseWeightType>(),
            It.IsAny<TimeSpan>()), Times.Never);
    }
    
    #endregion
    
    #region GetByIdAsDtoAsync Tests
    
    [Fact]
    public async Task GetByIdAsDtoAsync_WithValidId_ReturnsDto()
    {
        // Arrange
        var id = _testData[0].Id.ToString();
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[0]);
            
        // Act
        var result = await _service.GetByIdAsDtoAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(_testData[0].Value, result.Value);
        Assert.Equal(_testData[0].Description, result.Description);
    }
    
    [Fact]
    public async Task GetByIdAsDtoAsync_WithInvalidIdFormat_ReturnsNull()
    {
        // Arrange
        var invalidId = "invalid-id-format";
        
        // Act
        var result = await _service.GetByIdAsDtoAsync(invalidId);
        
        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExerciseWeightTypeId>()), Times.Never);
    }
    
    #endregion
    
    #region GetByValueAsync Tests
    
    [Fact]
    public async Task GetByValueAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var value = "Weight Required";
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[2]);
            
        // Act
        var result = await _service.GetByValueAsync(value);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result.Value);
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var value = "Weight Required";
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(_testData[2]);
            
        // Act
        var result = await _service.GetByValueAsync(value);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result.Value);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ExerciseWeightType>(),
            It.Is<TimeSpan>(ts => ts.TotalHours == 24)), Times.Once);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithNullOrEmpty_ReturnsNull()
    {
        // Act & Assert - null value
        var result1 = await _service.GetByValueAsync(null!);
        Assert.Null(result1);
        
        // Act & Assert - empty value
        var result2 = await _service.GetByValueAsync(string.Empty);
        Assert.Null(result2);
        
        // Act & Assert - whitespace value
        var result3 = await _service.GetByValueAsync("   ");
        Assert.Null(result3);
        
        _mockRepository.Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    #endregion
    
    #region GetByCodeAsync Tests
    
    [Fact]
    public async Task GetByCodeAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var code = "BODYWEIGHT_ONLY";
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[0]);
            
        // Act
        var result = await _service.GetByCodeAsync(code);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.Code);
        _mockRepository.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var code = "WEIGHT_REQUIRED";
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(_testData[2]);
            
        // Act
        var result = await _service.GetByCodeAsync(code);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.Code);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<ExerciseWeightType>(),
            It.Is<TimeSpan>(ts => ts.TotalHours == 24)), Times.Once);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WithNullOrEmpty_ReturnsNull()
    {
        // Act & Assert - null code
        var result1 = await _service.GetByCodeAsync(null!);
        Assert.Null(result1);
        
        // Act & Assert - empty code
        var result2 = await _service.GetByCodeAsync(string.Empty);
        Assert.Null(result2);
        
        // Act & Assert - whitespace code
        var result3 = await _service.GetByCodeAsync("   ");
        Assert.Null(result3);
        
        _mockRepository.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }
    
    #endregion
    
    #region ExistsAsync Tests
    
    [Fact]
    public async Task ExistsAsync_WhenWeightTypeExists_ReturnsTrue()
    {
        // Arrange
        var id = _testData[0].Id;
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[0]);
            
        // Act
        var result = await _service.ExistsAsync(id);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenWeightTypeDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        // Act
        var result = await _service.ExistsAsync(id);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenWeightTypeIsInactive_ReturnsFalse()
    {
        // Arrange
        var inactiveType = ExerciseWeightType.Handler.Create(
            ExerciseWeightTypeId.New(),
            "INACTIVE",
            "Inactive Type",
            "This is inactive",
            6,
            false);
            
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(inactiveType);
            
        // Act
        var result = await _service.ExistsAsync(inactiveType.Id);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region IsValidWeightForTypeAsync Tests
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_BodyweightOnly_ValidatesCorrectly()
    {
        // Arrange
        var id = _testData[0].Id; // BODYWEIGHT_ONLY
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[0]);
            
        // Act & Assert
        Assert.True(await _service.IsValidWeightForTypeAsync(id, null));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, 0));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, 10));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, -5));
    }
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_BodyweightOptional_ValidatesCorrectly()
    {
        // Arrange
        var id = _testData[1].Id; // BODYWEIGHT_OPTIONAL
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[1]);
            
        // Act & Assert - any weight value is valid
        Assert.True(await _service.IsValidWeightForTypeAsync(id, null));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, 0));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, 10));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, -5));
    }
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_WeightRequired_ValidatesCorrectly()
    {
        // Arrange
        var id = _testData[2].Id; // WEIGHT_REQUIRED
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[2]);
            
        // Act & Assert
        Assert.False(await _service.IsValidWeightForTypeAsync(id, null));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, 0));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, 10));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, -5));
    }
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_MachineWeight_ValidatesCorrectly()
    {
        // Arrange
        var id = _testData[3].Id; // MACHINE_WEIGHT
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[3]);
            
        // Act & Assert
        Assert.False(await _service.IsValidWeightForTypeAsync(id, null));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, 0));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, 10));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, -5));
    }
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_NoWeight_ValidatesCorrectly()
    {
        // Arrange
        var id = _testData[4].Id; // NO_WEIGHT
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(_testData[4]);
            
        // Act & Assert
        Assert.True(await _service.IsValidWeightForTypeAsync(id, null));
        Assert.True(await _service.IsValidWeightForTypeAsync(id, 0));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, 10));
        Assert.False(await _service.IsValidWeightForTypeAsync(id, -5));
    }
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_UnknownWeightType_ReturnsFalse()
    {
        // Arrange
        var unknownType = ExerciseWeightType.Handler.Create(
            ExerciseWeightTypeId.New(),
            "UNKNOWN_CODE",
            "Unknown Type",
            "Unknown weight type",
            6,
            true);
            
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync(unknownType);
            
        // Act & Assert
        Assert.False(await _service.IsValidWeightForTypeAsync(unknownType.Id, null));
        Assert.False(await _service.IsValidWeightForTypeAsync(unknownType.Id, 10));
    }
    
    [Fact]
    public async Task IsValidWeightForTypeAsync_NonExistentType_ReturnsFalse()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((ExerciseWeightType?)null);
            
        // Act
        var result = await _service.IsValidWeightForTypeAsync(id, 10);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region Caching Behavior Tests
    
    [Fact]
    public async Task CacheKeys_UseCorrectFormat()
    {
        // Arrange
        var id = _testData[0].Id;
        var value = "Bodyweight Only";
        var code = "BODYWEIGHT_ONLY";
        
        string? capturedGetAllKey = null;
        string? capturedByIdKey = null;
        string? capturedByValueKey = null;
        string? capturedByCodeKey = null;
        
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<ExerciseWeightType>>(It.IsAny<string>()))
            .Callback<string>(key => capturedGetAllKey = key)
            .ReturnsAsync((IEnumerable<ExerciseWeightType>?)null);
            
        _mockCacheService
            .Setup(x => x.GetAsync<ExerciseWeightType>(It.IsAny<string>()))
            .Callback<string>(key =>
            {
                if (key.Contains("GetById"))
                    capturedByIdKey = key;
                else if (key.Contains("GetByValue"))
                    capturedByValueKey = key;
                else if (key.Contains("code"))
                    capturedByCodeKey = key;
            })
            .ReturnsAsync((ExerciseWeightType?)null);
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ExerciseWeightTypeId>())).ReturnsAsync(_testData[0]);
        _mockRepository.Setup(x => x.GetByValueAsync(It.IsAny<string>())).ReturnsAsync(_testData[0]);
        _mockRepository.Setup(x => x.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync(_testData[0]);
        
        // Act
        await _service.GetAllAsync();
        await _service.GetByIdAsync(id);
        await _service.GetByValueAsync(value);
        await _service.GetByCodeAsync(code);
        
        // Assert
        Assert.Equal("ReferenceTable:ExerciseWeightTypes:GetAll", capturedGetAllKey);
        Assert.Equal($"ReferenceTable:ExerciseWeightTypes:GetById:{id}", capturedByIdKey);
        Assert.Equal($"ReferenceTable:ExerciseWeightTypes:GetByValue:{value.ToLowerInvariant()}", capturedByValueKey);
        Assert.Equal($"ExerciseWeightTypes:code:{code}", capturedByCodeKey);
    }
    
    #endregion
}