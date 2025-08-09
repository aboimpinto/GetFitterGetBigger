using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class KineticChainTypesControllerTests
{
    private readonly Mock<IKineticChainTypeService> _serviceMock;
    private readonly Mock<ILogger<KineticChainTypesController>> _loggerMock;
    private readonly KineticChainTypesController _controller;

    public KineticChainTypesControllerTests()
    {
        _serviceMock = new Mock<IKineticChainTypeService>();
        _loggerMock = new Mock<ILogger<KineticChainTypesController>>();
        _controller = new KineticChainTypesController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetKineticChainTypes_ReturnsOkWithData()
    {
        // Arrange
        var kineticChainTypes = new List<ReferenceDataDto>
        {
            new() { Id = TestIds.KineticChainTypeIds.Compound, Value = "COMPOUND", Description = "Multi-joint movement" },
            new() { Id = TestIds.KineticChainTypeIds.Isolation, Value = "ISOLATION", Description = "Single-joint movement" }
        };

        _serviceMock
            .Setup(s => s.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(kineticChainTypes));

        // Act
        var result = await _controller.GetKineticChainTypes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Equal(2, data.Count());
    }

    [Fact]
    public async Task GetKineticChainTypes_WithEmptyList_ReturnsOkWithEmptyData()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(new List<ReferenceDataDto>()));

        // Act
        var result = await _controller.GetKineticChainTypes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Empty(data);
    }

    [Fact]
    public async Task GetKineticChainTypeById_WithValidId_ReturnsOk()
    {
        // Arrange
        var kineticChainType = new ReferenceDataDto
            {
            Id = TestIds.KineticChainTypeIds.Compound,
            Value = "COMPOUND",
            Description = "Multi-joint movement engaging multiple muscle groups"
        };

        _serviceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(kineticChainType));

        // Act
        var result = await _controller.GetKineticChainTypeById(TestIds.KineticChainTypeIds.Compound);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ReferenceDataDto>(okResult.Value);
        Assert.Equal("COMPOUND", data.Value);
    }

    [Fact]
    public async Task GetKineticChainTypeById_WithInvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var error = ServiceError.ValidationFailed("Invalid kinetic chain type ID format");
        _serviceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetKineticChainTypeById("invalid-format");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetKineticChainTypeById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var error = ServiceError.NotFound("Kinetic chain type not found");
        _serviceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetKineticChainTypeById(TestIds.KineticChainTypeIds.NonExistent);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetKineticChainTypeByValue_WithValidValue_ReturnsOk()
    {
        // Arrange
        var kineticChainType = new ReferenceDataDto
            {
            Id = TestIds.KineticChainTypeIds.Compound,
            Value = "COMPOUND",
            Description = "Multi-joint movement engaging multiple muscle groups"
        };

        _serviceMock
            .Setup(s => s.GetByValueAsync("COMPOUND"))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(kineticChainType));

        // Act
        var result = await _controller.GetKineticChainTypeByValue("COMPOUND");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ReferenceDataDto>(okResult.Value);
        Assert.Equal("COMPOUND", data.Value);
    }

    [Fact]
    public async Task GetKineticChainTypeByValue_WithEmptyValue_ReturnsBadRequest()
    {
        // Arrange
        var error = ServiceError.ValidationFailed("Value cannot be empty");
        _serviceMock
            .Setup(s => s.GetByValueAsync(string.Empty))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetKineticChainTypeByValue(string.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetKineticChainTypeByValue_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var error = ServiceError.NotFound("Kinetic chain type not found");
        _serviceMock
            .Setup(s => s.GetByValueAsync("NONEXISTENT"))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), error));

        // Act
        var result = await _controller.GetKineticChainTypeByValue("NONEXISTENT");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}