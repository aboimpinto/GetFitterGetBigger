using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class MuscleGroupsControllerTests
{
    private readonly Mock<IMuscleGroupService> _mockService;
    private readonly Mock<ILogger<MuscleGroupsController>> _mockLogger;
    private readonly MuscleGroupsController _controller;
    
    public MuscleGroupsControllerTests()
    {
        _mockService = new Mock<IMuscleGroupService>();
        _mockLogger = new Mock<ILogger<MuscleGroupsController>>();
        _controller = new MuscleGroupsController(_mockService.Object, _mockLogger.Object);
    }
    
    #region GetAll Tests
    
    [Fact]
    public async Task GetAll_WhenSuccessful_ReturnsOkWithData()
    {
        // Arrange
        var muscleGroups = new List<MuscleGroupDto>
        {
            new() 
            { 
                Id = MuscleGroupTestConstants.TestData.BicepsId, 
                Name = MuscleGroupTestConstants.TestData.BicepsName,
                BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId,
                BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = MuscleGroupTestConstants.TestData.ChestId, 
                Name = MuscleGroupTestConstants.TestData.ChestName,
                BodyPartId = MuscleGroupTestConstants.TestData.TorsoBodyPartId,
                BodyPartName = MuscleGroupTestConstants.TestData.TorsoBodyPartName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Success(muscleGroups));
            
        // Act
        var result = await _controller.GetAll();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<MuscleGroupDto>>(okResult.Value);
        Assert.Equal(2, data.Count());
    }
    
    [Fact]
    public async Task GetAll_WhenEmptyData_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Success(Enumerable.Empty<MuscleGroupDto>()));
            
        // Act
        var result = await _controller.GetAll();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<MuscleGroupDto>>(okResult.Value);
        Assert.Empty(data);
    }
    
    #endregion
    
    #region GetById Tests
    
    [Fact]
    public async Task GetById_WhenFound_ReturnsOkWithData()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        var dto = new MuscleGroupDto 
        { 
            Id = id, 
            Name = MuscleGroupTestConstants.TestData.BicepsName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId,
            BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Success(dto));
            
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<MuscleGroupDto>(okResult.Value);
        Assert.Equal(id, data.Id);
        Assert.Equal(MuscleGroupTestConstants.TestData.BicepsName, data.Name);
    }
    
    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.NonExistentId;
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.NotFound(MuscleGroupTestConstants.ErrorMessages.NotFound)));
            
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task GetById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = MuscleGroupTestConstants.TestData.InvalidFormatId;
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InvalidFormat("ID", "musclegroup-{guid}")));
        
        // Act
        var result = await _controller.GetById(invalidId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetById_WhenValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.ValidationFailed(MuscleGroupTestConstants.ErrorMessages.ValidationFailed)));
        
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetById_WhenInternalError_ReturnsInternalServerError()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InternalError(MuscleGroupTestConstants.ErrorMessages.InternalError)));
        
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    [Fact]
    public async Task GetById_WhenUnhandledErrorCode_ReturnsInternalServerError()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        // Using Unauthorized as an example of an unhandled error code
        _mockService
            .Setup(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.Unauthorized("Unexpected error")));
        
        // Act
        var result = await _controller.GetById(id);
        
        // Assert
        // The fallback case should return 500 Internal Server Error
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    #endregion
    
    #region GetByValue Tests
    
    [Fact]
    public async Task GetByValue_WhenFound_ReturnsOkWithData()
    {
        // Arrange
        var name = MuscleGroupTestConstants.TestData.BicepsName;
        var dto = new MuscleGroupDto 
        { 
            Id = MuscleGroupTestConstants.TestData.BicepsId, 
            Name = name,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId,
            BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockService
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Success(dto));
            
        // Act
        var result = await _controller.GetByValue(name);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<MuscleGroupDto>(okResult.Value);
        Assert.Equal(name, data.Name);
    }
    
    [Fact]
    public async Task GetByValue_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var name = "NonExistent";
        
        _mockService
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.NotFound(MuscleGroupTestConstants.ErrorMessages.NotFound)));
            
        // Act
        var result = await _controller.GetByValue(name);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task GetByValue_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var emptyName = MuscleGroupTestConstants.TestData.EmptyName;
        
        _mockService
            .Setup(x => x.GetByNameAsync(emptyName))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InvalidFormat("Name", "non-empty string")));
            
        // Act
        var result = await _controller.GetByValue(emptyName);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetByValue_WhenValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var name = MuscleGroupTestConstants.TestData.BicepsName;
        
        _mockService
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.ValidationFailed(MuscleGroupTestConstants.ErrorMessages.ValidationFailed)));
            
        // Act
        var result = await _controller.GetByValue(name);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetByValue_WhenInternalError_ReturnsInternalServerError()
    {
        // Arrange
        var name = MuscleGroupTestConstants.TestData.BicepsName;
        
        _mockService
            .Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InternalError(MuscleGroupTestConstants.ErrorMessages.InternalError)));
        
        // Act
        var result = await _controller.GetByValue(name);
        
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    #endregion
    
    #region GetByBodyPart Tests
    
    [Fact]
    public async Task GetByBodyPart_WhenFound_ReturnsOkWithData()
    {
        // Arrange
        var bodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId;
        var muscleGroups = new List<MuscleGroupDto>
        {
            new() 
            { 
                Id = MuscleGroupTestConstants.TestData.BicepsId, 
                Name = MuscleGroupTestConstants.TestData.BicepsName,
                BodyPartId = bodyPartId,
                BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = MuscleGroupTestConstants.TestData.TricepsId, 
                Name = MuscleGroupTestConstants.TestData.TricepsName,
                BodyPartId = bodyPartId,
                BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _mockService
            .Setup(x => x.GetByBodyPartAsync(It.IsAny<BodyPartId>()))
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Success(muscleGroups));
            
        // Act
        var result = await _controller.GetByBodyPart(bodyPartId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<MuscleGroupDto>>(okResult.Value);
        Assert.Equal(2, data.Count());
        Assert.All(data, mg => Assert.Equal(bodyPartId, mg.BodyPartId));
    }
    
    [Fact]
    public async Task GetByBodyPart_WhenEmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        var bodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId;
        
        _mockService
            .Setup(x => x.GetByBodyPartAsync(It.IsAny<BodyPartId>()))
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Success(Enumerable.Empty<MuscleGroupDto>()));
            
        // Act
        var result = await _controller.GetByBodyPart(bodyPartId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<MuscleGroupDto>>(okResult.Value);
        Assert.Empty(data);
    }
    
    [Fact]
    public async Task GetByBodyPart_WithInvalidBodyPartId_ReturnsBadRequest()
    {
        // Arrange
        var invalidBodyPartId = MuscleGroupTestConstants.TestData.InvalidBodyPartId;
        
        _mockService
            .Setup(x => x.GetByBodyPartAsync(It.IsAny<BodyPartId>()))
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Failure(
                Enumerable.Empty<MuscleGroupDto>(),
                ServiceError.InvalidFormat("BodyPartId", "bodypart-{guid}")));
            
        // Act
        var result = await _controller.GetByBodyPart(invalidBodyPartId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetByBodyPart_WhenValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var bodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId;
        
        _mockService
            .Setup(x => x.GetByBodyPartAsync(It.IsAny<BodyPartId>()))
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Failure(
                Enumerable.Empty<MuscleGroupDto>(),
                ServiceError.ValidationFailed(MuscleGroupTestConstants.ErrorMessages.ValidationFailed)));
            
        // Act
        var result = await _controller.GetByBodyPart(bodyPartId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetByBodyPart_WhenInternalError_ReturnsInternalServerError()
    {
        // Arrange
        var bodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId;
        
        _mockService
            .Setup(x => x.GetByBodyPartAsync(It.IsAny<BodyPartId>()))
            .ReturnsAsync(ServiceResult<IEnumerable<MuscleGroupDto>>.Failure(
                Enumerable.Empty<MuscleGroupDto>(),
                ServiceError.InternalError(MuscleGroupTestConstants.ErrorMessages.InternalError)));
        
        // Act
        var result = await _controller.GetByBodyPart(bodyPartId);
        
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    #endregion
    
    #region Create Tests
    
    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.NewMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        var createdDto = new MuscleGroupDto 
        { 
            Id = "musclegroup-88888888-8888-8888-8888-888888888888", 
            Name = request.Name,
            BodyPartId = request.BodyPartId,
            BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Success(createdDto));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(MuscleGroupsController.GetById), createdResult.ActionName);
        Assert.Equal(createdDto.Id, createdResult.RouteValues?["id"]);
        var data = Assert.IsType<MuscleGroupDto>(createdResult.Value);
        Assert.Equal(request.Name, data.Name);
    }
    
    [Fact]
    public async Task Create_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var request = new CreateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.BicepsName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.AlreadyExists("MuscleGroup", request.Name)));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
    }
    
    [Fact]
    public async Task Create_WithInvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.NewMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.InvalidBodyPartId
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InvalidFormat("BodyPartId", "bodypart-{guid}")));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Create_WhenValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.EmptyName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.ValidationFailed(MuscleGroupTestConstants.ErrorMessages.ValidationFailed)));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Create_WhenBodyPartNotFound_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.NewMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.NonExistentId.Replace("musclegroup", "bodypart")
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.NotFound("Body part not found")));
            
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Create_WhenInternalError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new CreateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.NewMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.CreateAsync(It.IsAny<CreateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InternalError(MuscleGroupTestConstants.ErrorMessages.InternalError)));
        
        // Act
        var result = await _controller.Create(request);
        
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    #endregion
    
    #region Update Tests
    
    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedData()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.UpdatedMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        var updatedDto = new MuscleGroupDto 
        { 
            Id = id, 
            Name = request.Name,
            BodyPartId = request.BodyPartId,
            BodyPartName = MuscleGroupTestConstants.TestData.ArmsBodyPartName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Success(updatedDto));
            
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<MuscleGroupDto>(okResult.Value);
        Assert.Equal(request.Name, data.Name);
        Assert.NotNull(data.UpdatedAt);
    }
    
    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.NonExistentId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.UpdatedMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.NotFound(MuscleGroupTestConstants.ErrorMessages.NotFound)));
            
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task Update_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.TricepsName, // Name that already exists
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.AlreadyExists("MuscleGroup", request.Name)));
            
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
    }
    
    [Fact]
    public async Task Update_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = MuscleGroupTestConstants.TestData.InvalidFormatId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.UpdatedMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InvalidFormat("ID", "musclegroup-{guid}")));
        
        // Act
        var result = await _controller.Update(invalidId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Update_WhenValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.EmptyName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.ValidationFailed(MuscleGroupTestConstants.ErrorMessages.ValidationFailed)));
        
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Update_WhenConcurrencyConflict_ReturnsConflict()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.UpdatedMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.ConcurrencyConflict("Concurrency conflict")));
        
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
    }
    
    [Fact]
    public async Task Update_WhenInternalError_ReturnsInternalServerError()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        var request = new UpdateMuscleGroupDto 
        { 
            Name = MuscleGroupTestConstants.TestData.UpdatedMuscleGroupName,
            BodyPartId = MuscleGroupTestConstants.TestData.ArmsBodyPartId
        };
        
        _mockService
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroupId>(), It.IsAny<UpdateMuscleGroupCommand>()))
            .ReturnsAsync(ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty,
                ServiceError.InternalError(MuscleGroupTestConstants.ErrorMessages.InternalError)));
        
        // Act
        var result = await _controller.Update(id, request);
        
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    #endregion
    
    #region Delete Tests
    
    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
            
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.NonExistentId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.NotFound("MuscleGroup")));
            
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public async Task Delete_WhenInUse_ReturnsConflict()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.DependencyExists("MuscleGroup", "active exercises")));
            
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
        var invalidId = MuscleGroupTestConstants.TestData.InvalidFormatId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.InvalidFormat("ID", "musclegroup-{guid}")));
        
        // Act
        var result = await _controller.Delete(invalidId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Delete_WhenValidationFailed_ReturnsBadRequest()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.ValidationFailed("Validation failed")));
        
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task Delete_WhenConcurrencyConflict_ReturnsConflict()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.ConcurrencyConflict("MuscleGroup")));
        
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
    }
    
    [Fact]
    public async Task Delete_WhenInternalError_ReturnsInternalServerError()
    {
        // Arrange
        var id = MuscleGroupTestConstants.TestData.BicepsId;
        
        _mockService
            .Setup(x => x.DeleteAsync(It.IsAny<MuscleGroupId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.InternalError("Internal server error")));
        
        // Act
        var result = await _controller.Delete(id);
        
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.NotNull(statusCodeResult.Value);
    }
    
    #endregion
}