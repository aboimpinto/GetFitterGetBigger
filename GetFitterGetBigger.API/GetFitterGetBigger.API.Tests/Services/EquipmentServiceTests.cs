using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class EquipmentServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
    private readonly Mock<IEquipmentRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<EquipmentService>> _mockLogger;
    private readonly EquipmentService _service;
    
    private readonly List<Equipment> _testData;
    
    public EquipmentServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IEquipmentRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<EquipmentService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IEquipmentRepository>())
            .Returns(_mockRepository.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IEquipmentRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new EquipmentService(
            _mockUnitOfWorkProvider.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
            
        _testData = new List<Equipment>
        {
            EquipmentTestBuilder.Barbell().Build(),
            EquipmentTestBuilder.Dumbbell().Build(),
            EquipmentTestBuilder.CableMachine().Build(),
            EquipmentTestBuilder.ResistanceBand().Build()
        };
    }
    
    #region GetAllAsync Tests
    
    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var cachedDtos = _testData.Select(MapToDto).ToList();
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
            .ReturnsAsync(cachedDtos);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Never);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<EquipmentDto>?)null);
            
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockCacheService.Verify(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<List<EquipmentDto>>()), Times.Once);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((IEnumerable<EquipmentDto>?)null);
            
        _mockRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(_testData);
        
        // Act
        await _service.GetAllAsync();
        
        // Assert
        Assert.Equal(EquipmentTestConstants.CacheKeys.AllCacheKey, capturedKey);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveEquipment()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetAsync<IEnumerable<EquipmentDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<EquipmentDto>?)null);
            
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(_testData); // Repository already filters inactive
            
        // Act
        var result = await _service.GetAllAsync();
        var dtos = result.Data.ToList();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, dtos.Count);
        Assert.All(dtos, dto => Assert.True(dto.IsActive));
    }
    
    #endregion
    
    #region GetByIdAsync Tests
    
    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var equipment = _testData.First();
        var dto = MapToDto(equipment);
        _mockCacheService
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync(dto);
            
        // Act
        var result = await _service.GetByIdAsync(equipment.EquipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(equipment.EquipmentId.ToString(), result.Data.Id);
        _mockCacheService.Verify(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<EquipmentId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var equipment = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync((EquipmentDto?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipment.EquipmentId))
            .ReturnsAsync(equipment);
            
        // Act
        var result = await _service.GetByIdAsync(equipment.EquipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(equipment.EquipmentId.ToString(), result.Data.Id);
        _mockCacheService.Verify(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(equipment.EquipmentId), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<EquipmentDto>()), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange & Act
        var result = await _service.GetByIdAsync(EquipmentId.ParseOrEmpty("invalid-id"));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains("Invalid ID provided", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsFailure()
    {
        // Arrange & Act
        var result = await _service.GetByIdAsync(EquipmentId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains("Invalid ID provided", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        _mockCacheService
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync((EquipmentDto?)null);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(Equipment.Empty);
            
        // Act
        var result = await _service.GetByIdAsync(EquipmentId.ParseOrEmpty(id));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    }
    
    #endregion
    
    #region GetByNameAsync Tests
    
    [Fact]
    public async Task GetByNameAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var equipment = _testData.First();
        var dto = MapToDto(equipment);
        _mockCacheService
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync(dto);
            
        // Act
        var result = await _service.GetByNameAsync(equipment.Name);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(equipment.Name, result.Data.Name);
        _mockCacheService.Verify(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByNameAsync_WhenNotCached_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var equipment = _testData.First();
        _mockCacheService
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .ReturnsAsync((EquipmentDto?)null);
            
        _mockRepository
            .Setup(x => x.GetByNameAsync(equipment.Name))
            .ReturnsAsync(equipment);
            
        // Act
        var result = await _service.GetByNameAsync(equipment.Name);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(equipment.Name, result.Data.Name);
        _mockCacheService.Verify(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.GetByNameAsync(equipment.Name), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<EquipmentDto>()), Times.Once);
    }
    
    [Fact]
    public async Task GetByNameAsync_WithEmptyName_ReturnsFailure()
    {
        // Arrange & Act
        var result = await _service.GetByNameAsync(string.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name cannot be empty", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task GetByNameAsync_CacheKey_UsesCaseInsensitive()
    {
        // Arrange
        string? capturedKey = null;
        _mockCacheService
            .Setup(x => x.GetAsync<EquipmentDto>(It.IsAny<string>()))
            .Callback<string>(key => capturedKey = key)
            .ReturnsAsync((EquipmentDto?)null);
            
        _mockRepository
            .Setup(x => x.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(EquipmentTestBuilder.Barbell().Build());
        
        // Act
        await _service.GetByNameAsync("BARBELL");
        
        // Assert
        Assert.Contains("name:barbell", capturedKey);
    }
    
    #endregion
    
    #region CreateAsync Tests
    
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var request = new CreateEquipmentCommand { Name = EquipmentTestConstants.TestData.NewEquipmentName };
        var createdEquipment = EquipmentTestBuilder.Custom(request.Name).Build();
        
        _mockRepository
            .Setup(x => x.ExistsAsync(request.Name.Trim(), null))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(createdEquipment);
            
        _mockWritableUnitOfWork
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);
            
        // Act
        var result = await _service.CreateAsync(request);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Name, result.Data.Name);
        Assert.True(result.Data.IsActive);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Equipment>()), Times.Once);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_WithNullRequest_ReturnsFailure()
    {
        // Arrange & Act
        var result = await _service.CreateAsync((CreateEquipmentCommand)null!);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.None, result.PrimaryErrorCode); // Base class uses string errors
        Assert.Contains("Request cannot be null", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsFailure()
    {
        // Arrange
        var request = new CreateEquipmentCommand { Name = string.Empty };
        
        // Act
        var result = await _service.CreateAsync(request);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.None, result.PrimaryErrorCode); // Base class uses string errors
        Assert.Contains("Name cannot be empty", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var request = new CreateEquipmentCommand { Name = EquipmentTestConstants.TestData.BarbellName };
        
        _mockRepository
            .Setup(x => x.ExistsAsync(request.Name.Trim(), null))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.CreateAsync(request);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.AlreadyExists, result.PrimaryErrorCode);
        Assert.Contains($"Equipment '{request.Name}' already exists", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task CreateAsync_TrimsName()
    {
        // Arrange
        var request = new CreateEquipmentCommand { Name = "  New Equipment  " };
        Equipment? capturedEquipment = null;
        
        _mockRepository
            .Setup(x => x.ExistsAsync("New Equipment", null))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Equipment>()))
            .Callback<Equipment>(e => capturedEquipment = e)
            .ReturnsAsync((Equipment e) => e);
            
        _mockWritableUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        
        // Act
        await _service.CreateAsync(request);
        
        // Assert
        Assert.NotNull(capturedEquipment);
        Assert.Equal("New Equipment", capturedEquipment.Name);
    }
    
    #endregion
    
    #region UpdateAsync Tests
    
    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var existingEquipment = EquipmentTestBuilder.Barbell().Build();
        var request = new UpdateEquipmentCommand { Name = EquipmentTestConstants.TestData.UpdatedEquipmentName };
        var updatedEquipment = EquipmentTestBuilder.Barbell()
            .WithName(request.Name)
            .WithUpdatedAt(DateTime.UtcNow)
            .Build();
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(existingEquipment.EquipmentId))
            .ReturnsAsync(existingEquipment);
            
        _mockRepository
            .Setup(x => x.ExistsAsync(request.Name.Trim(), existingEquipment.EquipmentId))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(updatedEquipment);
            
        _mockWritableUnitOfWork
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);
            
        // Act
        var result = await _service.UpdateAsync(existingEquipment.EquipmentId, request);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Name, result.Data.Name);
        Assert.NotNull(result.Data.UpdatedAt);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Equipment>()), Times.Once);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateEquipmentCommand { Name = "Updated" };
        
        // Act
        var result = await _service.UpdateAsync(EquipmentId.ParseOrEmpty("invalid-id"), request);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains("Invalid ID provided", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        var request = new UpdateEquipmentCommand { Name = "Updated" };
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(Equipment.Empty);
            
        // Act
        var result = await _service.UpdateAsync(EquipmentId.ParseOrEmpty(id), request);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    }
    
    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var existingEquipment = EquipmentTestBuilder.Barbell().Build();
        var request = new UpdateEquipmentCommand { Name = EquipmentTestConstants.TestData.DumbbellName };
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(existingEquipment.EquipmentId))
            .ReturnsAsync(existingEquipment);
            
        _mockRepository
            .Setup(x => x.ExistsAsync(request.Name.Trim(), existingEquipment.EquipmentId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.UpdateAsync(existingEquipment.EquipmentId, request);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.AlreadyExists, result.PrimaryErrorCode);
        Assert.Contains($"Equipment '{request.Name}' already exists", result.Errors.FirstOrDefault() ?? "");
    }
    
    #endregion
    
    #region DeleteAsync Tests
    
    [Fact]
    public async Task DeleteAsync_WhenNotInUse_DeactivatesSuccessfully()
    {
        // Arrange
        var equipment = EquipmentTestBuilder.Barbell().Build();
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipment.EquipmentId))
            .ReturnsAsync(equipment);
            
        _mockRepository
            .Setup(x => x.IsInUseAsync(equipment.EquipmentId))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.DeactivateAsync(equipment.EquipmentId))
            .ReturnsAsync(true);
            
        _mockWritableUnitOfWork
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);
            
        // Act
        var result = await _service.DeleteAsync(equipment.EquipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockRepository.Verify(x => x.DeactivateAsync(equipment.EquipmentId), Times.Once);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenInUse_ReturnsFailure()
    {
        // Arrange
        var equipment = EquipmentTestBuilder.Barbell().Build();
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipment.EquipmentId))
            .ReturnsAsync(equipment);
            
        _mockRepository
            .Setup(x => x.IsInUseAsync(equipment.EquipmentId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.DeleteAsync(equipment.EquipmentId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.DependencyExists, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.DeactivateAsync(It.IsAny<EquipmentId>()), Times.Never);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange & Act
        var result = await _service.DeleteAsync(EquipmentId.ParseOrEmpty("invalid-id"));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains("Invalid ID provided", result.Errors.FirstOrDefault() ?? "");
    }
    
    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(Equipment.Empty);
            
        // Act
        var result = await _service.DeleteAsync(EquipmentId.ParseOrEmpty(id));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    }
    
    #endregion
    
    #region ExistsAsync Tests
    
    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var equipment = EquipmentTestBuilder.Barbell().Build();
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipment.EquipmentId))
            .ReturnsAsync(equipment);
            
        // Act
        var result = await _service.ExistsAsync(equipment.EquipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(equipment.EquipmentId.ToString(), result.Data.Id);
        Assert.Equal(equipment.Name, result.Data.Name);
    }
    
    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(Equipment.Empty);
            
        // Act
        var result = await _service.ExistsAsync(EquipmentId.ParseOrEmpty(id));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    }
    
    [Fact]
    public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _service.ExistsAsync(EquipmentId.ParseOrEmpty("invalid-id"));
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
    }
    
    #endregion
    
    #region Helper Methods
    
    private static EquipmentDto MapToDto(Equipment entity)
    {
        return new EquipmentDto
        {
            Id = entity.EquipmentId.ToString(),
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    #endregion
}