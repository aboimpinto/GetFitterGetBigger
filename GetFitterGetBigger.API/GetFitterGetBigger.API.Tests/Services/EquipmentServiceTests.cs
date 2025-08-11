using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Constants;
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
    private readonly Mock<ILogger<EquipmentService>> _mockLogger;
    private readonly EquipmentService _service;
    
    private readonly List<Equipment> _testData;
    
    public EquipmentServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IEquipmentRepository>();
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
    public async Task GetAllAsync_ReturnsAllEquipment()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Equipment>());
            
        // Act
        var result = await _service.GetAllAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsEquipment()
    {
        // Arrange
        var equipment = _testData.First();
        var equipmentId = equipment.EquipmentId;
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipmentId))
            .ReturnsAsync(equipment);
            
        // Act
        var result = await _service.GetByIdAsync(equipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(equipment.Name, result.Data.Name);
        Assert.Equal(equipmentId.ToString(), result.Data.Id);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsValidationError()
    {
        // Arrange
        var emptyId = EquipmentId.Empty;
        
        // Act
        var result = await _service.GetByIdAsync(emptyId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.InvalidEquipmentId, string.Join(" ", result.Errors));
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsEmpty()
    {
        // Arrange
        var equipmentId = EquipmentId.New();
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipmentId))
            .ReturnsAsync((Equipment?)null);
            
        // Act
        var result = await _service.GetByIdAsync(equipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data.IsEmpty);
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_ValidName_ReturnsEquipment()
    {
        // Arrange
        var equipment = _testData.First();
        var name = equipment.Name;
        
        _mockRepository
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(equipment);
            
        // Act
        var result = await _service.GetByNameAsync(name);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(equipment.Name, result.Data.Name);
    }

    [Fact]
    public async Task GetByNameAsync_EmptyName_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetByNameAsync("");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.NameCannotBeEmpty, string.Join(" ", result.Errors));
    }

    [Fact]
    public async Task GetByNameAsync_NotFound_ReturnsEmpty()
    {
        // Arrange
        var name = "NonexistentEquipment";
        
        _mockRepository
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync((Equipment?)null);
            
        // Act
        var result = await _service.GetByNameAsync(name);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data.IsEmpty);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidCommand_ReturnsCreatedEquipment()
    {
        // Arrange
        var command = new CreateEquipmentCommand { Name = "Test Equipment" };
        var createdEquipment = Equipment.Handler.CreateNew(command.Name.Trim());
        
        _mockRepository
            .Setup(x => x.ExistsAsync(command.Name.Trim()))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(createdEquipment);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(command.Name, result.Data.Name);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Equipment>()), Times.Once);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullCommand_ReturnsValidationError()
    {
        // Act
        var result = await _service.CreateAsync(null!);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.RequestCannotBeNull, string.Join(" ", result.Errors));
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateEquipmentCommand { Name = "" };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.NameCannotBeEmpty, string.Join(" ", result.Errors));
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateEquipmentCommand { Name = "Existing Equipment" };
        
        _mockRepository
            .Setup(x => x.ExistsAsync(command.Name.Trim()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", string.Join(" ", result.Errors));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedEquipment()
    {
        // Arrange
        var equipment = _testData.First();
        var equipmentId = equipment.EquipmentId;
        var command = new UpdateEquipmentCommand { Name = "Updated Equipment" };
        var updatedEquipment = Equipment.Handler.Update(equipment, command.Name.Trim());
        
        _mockRepository
            .Setup(x => x.ExistsAsync(equipmentId))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(x => x.ExistsAsync(command.Name.Trim(), equipmentId))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipmentId))
            .ReturnsAsync(equipment);
            
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(updatedEquipment);
            
        // Act
        var result = await _service.UpdateAsync(equipmentId, command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(command.Name, result.Data.Name);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Equipment>()), Times.Once);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsValidationError()
    {
        // Arrange
        var emptyId = EquipmentId.Empty;
        var command = new UpdateEquipmentCommand { Name = "Test Equipment" };
        
        // Act
        var result = await _service.UpdateAsync(emptyId, command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.InvalidEquipmentId, string.Join(" ", result.Errors));
    }

    [Fact]
    public async Task UpdateAsync_EquipmentNotExists_ReturnsValidationError()
    {
        // Arrange
        var equipmentId = EquipmentId.New();
        var command = new UpdateEquipmentCommand { Name = "Test Equipment" };
        
        _mockRepository
            .Setup(x => x.ExistsAsync(equipmentId))
            .ReturnsAsync(false);
            
        // Act
        var result = await _service.UpdateAsync(equipmentId, command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", string.Join(" ", result.Errors));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var equipmentId = EquipmentId.New();
        
        _mockRepository
            .Setup(x => x.ExistsAsync(equipmentId))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(x => x.IsInUseAsync(equipmentId))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.DeactivateAsync(equipmentId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.DeleteAsync(equipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockRepository.Verify(x => x.DeactivateAsync(equipmentId), Times.Once);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ReturnsValidationError()
    {
        // Arrange
        var emptyId = EquipmentId.Empty;
        
        // Act
        var result = await _service.DeleteAsync(emptyId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.InvalidEquipmentId, string.Join(" ", result.Errors));
    }

    [Fact]
    public async Task DeleteAsync_EquipmentInUse_ReturnsValidationError()
    {
        // Arrange
        var equipmentId = EquipmentId.New();
        
        _mockRepository
            .Setup(x => x.ExistsAsync(equipmentId))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(x => x.IsInUseAsync(equipmentId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.DeleteAsync(equipmentId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.DependencyExists, result.PrimaryErrorCode);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ValidId_ReturnsEquipment()
    {
        // Arrange
        var equipment = _testData.First();
        var equipmentId = equipment.EquipmentId;
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(equipmentId))
            .ReturnsAsync(equipment);
            
        // Act
        var result = await _service.ExistsAsync(equipmentId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(equipment.Name, result.Data.Name);
    }

    [Fact]
    public async Task ExistsAsync_InvalidId_ReturnsValidationError()
    {
        // Arrange
        var emptyId = EquipmentId.Empty;
        
        // Act
        var result = await _service.ExistsAsync(emptyId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(EquipmentErrorMessages.Validation.InvalidEquipmentId, string.Join(" ", result.Errors));
    }

    #endregion
}