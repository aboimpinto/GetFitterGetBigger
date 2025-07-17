using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class EquipmentControllerTests
{
    private readonly Mock<IEquipmentService> _mockService;
    private readonly Mock<ILogger<EquipmentController>> _mockLogger;
    private readonly EquipmentController _controller;
    
    public EquipmentControllerTests()
    {
        _mockService = new Mock<IEquipmentService>();
        _mockLogger = new Mock<ILogger<EquipmentController>>();
        _controller = new EquipmentController(_mockService.Object, _mockLogger.Object);
    }
    
    #region GetAll Tests
    
    [Fact]
    public async Task GetAll_WhenSuccessful_ReturnsOkWithData()
    {
        // Arrange
        var equipmentList = new List<EquipmentDto>
        {
            new() { Id = EquipmentTestConstants.TestData.BarbellId, Name = EquipmentTestConstants.TestData.BarbellName, IsActive = true },
            new() { Id = EquipmentTestConstants.TestData.DumbbellId, Name = EquipmentTestConstants.TestData.DumbbellName, IsActive = true }
        };
        
        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<EquipmentDto>>.Success(equipmentList));
            
        // Act
        var result = await _controller.GetAll();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<EquipmentDto>>(okResult.Value);
        Assert.Equal(2, data.Count());
    }
    
    [Fact]
    public async Task GetAll_WhenEmptyData_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<EquipmentDto>>.Success(Enumerable.Empty<EquipmentDto>()));
            
        // Act
        var result = await _controller.GetAll();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<EquipmentDto>>(okResult.Value);
        Assert.Empty(data);
    }
    
    #endregion
    
    #region GetById Tests
    
    [Fact]
    public async Task GetById_WhenFound_ReturnsOkWithData()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.BarbellId;
        var dto = new EquipmentDto { Id = id, Name = EquipmentTestConstants.TestData.BarbellName, IsActive = true };
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(dto));
            
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<EquipmentDto>(okResult.Value);
        Assert.Equal(id, data.Id);
        Assert.Equal(EquipmentTestConstants.TestData.BarbellName, data.Name);
    }
    
    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.NotFound("Equipment")));
            
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task GetById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "invalid-id";
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.ValidationFailed("Invalid ID")));
        
        // Act
        var result = await _controller.GetById(invalidId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        // Verify service WAS called with empty ID
        _mockService.Verify(x => x.GetByIdAsync(It.IsAny<EquipmentId>()), Times.Once);
    }
    
    #endregion
    
    #region GetByName Tests
    
    [Fact]
    public async Task GetByName_WhenFound_ReturnsOkWithData()
    {
        // Arrange
        var name = EquipmentTestConstants.TestData.BarbellName;
        var dto = new EquipmentDto { Id = EquipmentTestConstants.TestData.BarbellId, Name = name, IsActive = true };
        
        _mockService
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(dto));
            
        // Act
        var result = await _controller.GetByName(name);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<EquipmentDto>(okResult.Value);
        Assert.Equal(name, data.Name);
    }
    
    [Fact]
    public async Task GetByName_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var name = "NonExistent";
        
        _mockService
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.NotFound("Equipment")));
            
        // Act
        var result = await _controller.GetByName(name);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task GetByName_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var emptyName = string.Empty;
        
        _mockService
            .Setup(x => x.GetByNameAsync(emptyName))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.ValidationFailed("Name cannot be empty")));
            
        // Act
        var result = await _controller.GetByName(emptyName);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    #endregion
    
    #region Create Tests
    
    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateEquipmentDto { Name = EquipmentTestConstants.TestData.NewEquipmentName };
        var createdDto = new EquipmentDto 
        { 
            Id = "equipment-88888888-8888-8888-8888-888888888888", 
            Name = request.Name, 
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateEquipmentCommand>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(createdDto));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EquipmentController.GetById), createdResult.ActionName);
        Assert.Equal(createdDto.Id, createdResult.RouteValues?["id"]);
        var data = Assert.IsType<EquipmentDto>(createdResult.Value);
        Assert.Equal(request.Name, data.Name);
    }
    
    [Fact]
    public async Task Create_WithNullRequest_ReturnsBadRequest()
    {
        // Arrange
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateEquipmentCommand>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.ValidationFailed("Request cannot be null")));
            
        // Act
        var result = await _controller.Create(null!);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateEquipmentDto { Name = EquipmentTestConstants.TestData.BarbellName };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateEquipmentCommand>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.ValidationFailed($"Equipment with the name '{request.Name}' already exists")));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    #endregion
    
    #region Update Tests
    
    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedData()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.BarbellId;
        var request = new UpdateEquipmentDto { Name = EquipmentTestConstants.TestData.UpdatedEquipmentName };
        var updatedDto = new EquipmentDto 
        { 
            Id = id, 
            Name = request.Name, 
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<EquipmentId>(), It.IsAny<UpdateEquipmentCommand>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Success(updatedDto));
            
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<EquipmentDto>(okResult.Value);
        Assert.Equal(request.Name, data.Name);
        Assert.NotNull(data.UpdatedAt);
    }
    
    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        var request = new UpdateEquipmentDto { Name = "Updated" };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<EquipmentId>(), It.IsAny<UpdateEquipmentCommand>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.NotFound("Equipment")));
            
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task Update_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "invalid-id";
        var request = new UpdateEquipmentDto { Name = "Updated" };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<EquipmentId>(), It.IsAny<UpdateEquipmentCommand>()))
            .ReturnsAsync(ServiceResult<EquipmentDto>.Failure(
                new EquipmentDto(),
                ServiceError.ValidationFailed("Invalid ID")));
        
        // Act
        var result = await _controller.Update(invalidId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        // Verify service WAS called with empty ID
        _mockService.Verify(x => x.UpdateAsync(It.IsAny<EquipmentId>(), It.IsAny<UpdateEquipmentCommand>()), Times.Once);
    }
    
    #endregion
    
    #region Delete Tests
    
    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.BarbellId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<bool>.Success(true));
            
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.NonExistentId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<bool>.Failure(
                false,
                ServiceError.NotFound("Equipment")));
            
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task Delete_WhenInUse_ReturnsConflict()
    {
        // Arrange
        var id = EquipmentTestConstants.TestData.BarbellId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<bool>.Failure(
                false,
                ServiceError.DependencyExists("Equipment", "exercises that reference it")));
            
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
    }
    
    [Fact]
    public async Task Delete_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = "invalid-id";
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<EquipmentId>()))
            .ReturnsAsync(ServiceResult<bool>.Failure(
                false,
                ServiceError.ValidationFailed("Invalid ID")));
        
        // Act
        var result = await _controller.Delete(invalidId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        // Verify service WAS called with empty ID
        _mockService.Verify(x => x.DeleteAsync(It.IsAny<EquipmentId>()), Times.Once);
    }
    
    #endregion
}