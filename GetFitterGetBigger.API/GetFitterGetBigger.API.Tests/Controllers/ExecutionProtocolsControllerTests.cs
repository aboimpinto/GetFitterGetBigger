using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class ExecutionProtocolsControllerTests
{
    private readonly Mock<IExecutionProtocolService> _mockService;
    private readonly Mock<ILogger<ExecutionProtocolsController>> _mockLogger;
    private readonly ExecutionProtocolsController _controller;

    public ExecutionProtocolsControllerTests()
    {
        _mockService = new Mock<IExecutionProtocolService>();
        _mockLogger = new Mock<ILogger<ExecutionProtocolsController>>();
        _controller = new ExecutionProtocolsController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetExecutionProtocols_ReturnsOkWithExecutionProtocols()
    {
        // Arrange
        var executionProtocols = new List<ExecutionProtocolDto>
        {
            ExecutionProtocolDtoTestBuilder.Standard().Build(),
            ExecutionProtocolDtoTestBuilder.Superset().Build()
        };
        var serviceResult = ServiceResult<IEnumerable<ExecutionProtocolDto>>.Success(executionProtocols);

        _mockService
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocols();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProtocols = Assert.IsAssignableFrom<IEnumerable<ExecutionProtocolDto>>(okResult.Value);
        Assert.Equal(2, returnedProtocols.Count());
        _mockService.Verify(x => x.GetAllActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolById_WithValidId_ReturnsOkWithExecutionProtocol()
    {
        // Arrange
        var id = ExecutionProtocolTestConstants.ValidExecutionProtocolId;
        var executionProtocol = new ExecutionProtocolDto
            {
            ExecutionProtocolId = id,
            Value = ExecutionProtocolTestConstants.StandardValue,
            Description = ExecutionProtocolTestConstants.StandardDescription,
            Code = ExecutionProtocolTestConstants.StandardCode,
            TimeBase = true,
            RepBase = true,
            RestPattern = ExecutionProtocolTestConstants.StandardRestPattern,
            IntensityLevel = ExecutionProtocolTestConstants.ModerateIntensity,
            DisplayOrder = ExecutionProtocolTestConstants.StandardDisplayOrder,
            IsActive = true
        };
        var serviceResult = ServiceResult<ExecutionProtocolDto>.Success(executionProtocol);

        _mockService
            .Setup(x => x.GetByIdAsync(It.Is<ExecutionProtocolId>(epId => epId.ToString() == id)))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocolById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProtocol = Assert.IsType<ExecutionProtocolDto>(okResult.Value);
        Assert.Equal(id, returnedProtocol.ExecutionProtocolId);
        Assert.Equal(ExecutionProtocolTestConstants.StandardValue, returnedProtocol.Value);
        Assert.Equal(ExecutionProtocolTestConstants.StandardCode, returnedProtocol.Code);
        _mockService.Verify(x => x.GetByIdAsync(It.Is<ExecutionProtocolId>(epId => epId.ToString() == id)), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolById_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = ExecutionProtocolTestConstants.InvalidFormatId;
        var serviceResult = ServiceResult<ExecutionProtocolDto>.Failure(
            new ExecutionProtocolDto(),
            ServiceError.ValidationFailed(ExecutionProtocolErrorMessages.InvalidIdFormat));

        _mockService
            .Setup(x => x.GetByIdAsync(ExecutionProtocolId.Empty))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocolById(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        _mockService.Verify(x => x.GetByIdAsync(ExecutionProtocolId.Empty), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = ExecutionProtocolTestConstants.NonExistentExecutionProtocolId;
        var serviceResult = ServiceResult<ExecutionProtocolDto>.Failure(
            new ExecutionProtocolDto(),
            ServiceError.NotFound(ExecutionProtocolErrorMessages.NotFound));

        _mockService
            .Setup(x => x.GetByIdAsync(It.Is<ExecutionProtocolId>(epId => epId.ToString() == id)))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocolById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(x => x.GetByIdAsync(It.Is<ExecutionProtocolId>(epId => epId.ToString() == id)), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolByCode_WithExistingCode_ReturnsOk()
    {
        // Arrange
        var code = ExecutionProtocolTestConstants.StandardCode;
        var executionProtocol = new ExecutionProtocolDto
            {
            ExecutionProtocolId = ExecutionProtocolTestConstants.TestExecutionProtocolId,
            Value = "Standard",
            Description = "Standard protocol",
            Code = code,
            TimeBase = true,
            RepBase = true,
            RestPattern = ExecutionProtocolTestConstants.StandardRestPattern,
            IntensityLevel = ExecutionProtocolTestConstants.ModerateIntensity,
            DisplayOrder = ExecutionProtocolTestConstants.StandardDisplayOrder,
            IsActive = true
        };
        var serviceResult = ServiceResult<ExecutionProtocolDto>.Success(executionProtocol);

        _mockService
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocolByCode(code);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProtocol = Assert.IsType<ExecutionProtocolDto>(okResult.Value);
        Assert.Equal(code, returnedProtocol.Code);
        Assert.Equal(ExecutionProtocolTestConstants.StandardValue, returnedProtocol.Value);
        _mockService.Verify(x => x.GetByCodeAsync(code), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolByCode_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var code = ExecutionProtocolTestConstants.NonExistentCode;
        var serviceResult = ServiceResult<ExecutionProtocolDto>.Failure(
            new ExecutionProtocolDto(),
            ServiceError.NotFound(ExecutionProtocolErrorMessages.NotFound));

        _mockService
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocolByCode(code);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockService.Verify(x => x.GetByCodeAsync(code), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolByCode_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        var code = ExecutionProtocolTestConstants.EmptyString;
        var serviceResult = ServiceResult<ExecutionProtocolDto>.Failure(
            new ExecutionProtocolDto(),
            ServiceError.ValidationFailed(ExecutionProtocolErrorMessages.CodeCannotBeEmpty));

        _mockService
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.GetExecutionProtocolByCode(code);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        _mockService.Verify(x => x.GetByCodeAsync(code), Times.Once);
    }
}