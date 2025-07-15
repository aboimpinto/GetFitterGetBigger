using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class MuscleRolesControllerTests
{
    private readonly Mock<IMuscleRoleService> _serviceMock;
    private readonly Mock<ILogger<MuscleRolesController>> _loggerMock;
    private readonly MuscleRolesController _controller;

    public MuscleRolesControllerTests()
    {
        _serviceMock = new Mock<IMuscleRoleService>();
        _loggerMock = new Mock<ILogger<MuscleRolesController>>();
        _controller = new MuscleRolesController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMuscleRoles_ReturnsOkWithData()
    {
        // Arrange
        var muscleRoles = new List<ReferenceDataDto>
        {
            new() { Id = TestIds.MuscleRoleIds.Primary, Value = MuscleRoleTestConstants.Values.Primary, Description = MuscleRoleTestConstants.Descriptions.Primary },
            new() { Id = TestIds.MuscleRoleIds.Secondary, Value = MuscleRoleTestConstants.Values.Secondary, Description = MuscleRoleTestConstants.Descriptions.Secondary }
        };

        _serviceMock
            .Setup(s => s.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(muscleRoles));

        // Act
        var result = await _controller.GetMuscleRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Equal(2, data.Count());
    }

    [Fact]
    public async Task GetMuscleRoles_WithEmptyList_ReturnsOkWithEmptyData()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(new List<ReferenceDataDto>()));

        // Act
        var result = await _controller.GetMuscleRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Empty(data);
    }

    [Fact]
    public async Task GetMuscleRoleById_WithValidId_ReturnsOk()
    {
        // Arrange
        var muscleRole = new ReferenceDataDto
        {
            Id = TestIds.MuscleRoleIds.Primary,
            Value = MuscleRoleTestConstants.Values.Primary,
            Description = MuscleRoleTestConstants.Descriptions.Primary
        };

        _serviceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<MuscleRoleId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(muscleRole));

        // Act
        var result = await _controller.GetMuscleRoleById(TestIds.MuscleRoleIds.Primary);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ReferenceDataDto>(okResult.Value);
        Assert.Equal(MuscleRoleTestConstants.Values.Primary, data.Value);
    }

    [Fact]
    public async Task GetMuscleRoleById_WithInvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var error = ServiceError.ValidationFailed("Invalid muscle role ID format");
        _serviceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<MuscleRoleId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetMuscleRoleById("invalid-format");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleRoleById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var error = ServiceError.NotFound("Muscle role not found");
        _serviceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<MuscleRoleId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetMuscleRoleById(TestIds.MuscleRoleIds.NonExistent);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMuscleRoleByValue_WithValidValue_ReturnsOk()
    {
        // Arrange
        var muscleRole = new ReferenceDataDto
        {
            Id = TestIds.MuscleRoleIds.Primary,
            Value = MuscleRoleTestConstants.Values.Primary,
            Description = MuscleRoleTestConstants.Descriptions.Primary
        };

        _serviceMock
            .Setup(s => s.GetByValueAsync(MuscleRoleTestConstants.Values.Primary))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(muscleRole));

        // Act
        var result = await _controller.GetMuscleRoleByValue(MuscleRoleTestConstants.Values.Primary);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ReferenceDataDto>(okResult.Value);
        Assert.Equal(MuscleRoleTestConstants.Values.Primary, data.Value);
    }

    [Fact]
    public async Task GetMuscleRoleByValue_WithEmptyValue_ReturnsBadRequest()
    {
        // Arrange
        var error = ServiceError.ValidationFailed("Value cannot be empty");
        _serviceMock
            .Setup(s => s.GetByValueAsync(string.Empty))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetMuscleRoleByValue(string.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleRoleByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var error = ServiceError.NotFound("Muscle role not found");
        _serviceMock
            .Setup(s => s.GetByValueAsync("NONEXISTENT"))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetMuscleRoleByValue("NONEXISTENT");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}