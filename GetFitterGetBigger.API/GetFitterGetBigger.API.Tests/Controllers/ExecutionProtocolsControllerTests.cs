using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
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
    public async Task GetAllExecutionProtocols_ReturnsOkResultWithProtocols()
    {
        // Arrange
        var protocols = new List<ExecutionProtocolDto>
        {
            ExecutionProtocolDtoTestBuilder.Standard().Build(),
            ExecutionProtocolDtoTestBuilder.Superset().Build()
        };
        _mockService.Setup(s => s.GetAllAsExecutionProtocolDtosAsync(false)).ReturnsAsync(protocols);

        // Act
        var result = await _controller.GetAllExecutionProtocols();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExecutionProtocolsResponseDto>(okResult.Value);
        Assert.Equal(2, response.ExecutionProtocols.Count);
        _mockService.Verify(s => s.GetAllAsExecutionProtocolDtosAsync(false), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolById_ValidId_ReturnsOkResultWithProtocol()
    {
        // Arrange
        var id = TestIds.ExecutionProtocolIds.Standard;
        var protocol = ExecutionProtocolDtoTestBuilder.Standard().Build();
        _mockService.Setup(s => s.GetByIdAsExecutionProtocolDtoAsync(id, false)).ReturnsAsync(protocol);

        // Act
        var result = await _controller.GetExecutionProtocolById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProtocol = Assert.IsType<ExecutionProtocolDto>(okResult.Value);
        Assert.Equal(id, returnedProtocol.ExecutionProtocolId);
        Assert.Equal("Standard", returnedProtocol.Value);
        Assert.Equal("STANDARD", returnedProtocol.Code);
        _mockService.Verify(s => s.GetByIdAsExecutionProtocolDtoAsync(id, false), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolById_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var id = ExecutionProtocolId.From(TestIds.NotFoundId);
        _mockService.Setup(s => s.GetByIdAsExecutionProtocolDtoAsync(id.ToString(), false)).ReturnsAsync((ExecutionProtocolDto?)null);

        // Act
        var result = await _controller.GetExecutionProtocolById(id.ToString());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
        _mockService.Verify(s => s.GetByIdAsExecutionProtocolDtoAsync(id.ToString(), false), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolByCode_ValidCode_ReturnsOkResultWithProtocol()
    {
        // Arrange
        var code = "STANDARD";
        var protocol = ExecutionProtocolDtoTestBuilder.Standard().Build();
        _mockService.Setup(s => s.GetByCodeAsDtoAsync(code)).ReturnsAsync(protocol);

        // Act
        var result = await _controller.GetExecutionProtocolByCode(code);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProtocol = Assert.IsType<ExecutionProtocolDto>(okResult.Value);
        Assert.Equal(code, returnedProtocol.Code);
        Assert.Equal("Standard", returnedProtocol.Value);
        _mockService.Verify(s => s.GetByCodeAsDtoAsync(code), Times.Once);
    }

    [Fact]
    public async Task GetExecutionProtocolByCode_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var code = "INVALID_CODE";
        _mockService.Setup(s => s.GetByCodeAsDtoAsync(code)).ReturnsAsync((ExecutionProtocolDto?)null);

        // Act
        var result = await _controller.GetExecutionProtocolByCode(code);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
        _mockService.Verify(s => s.GetByCodeAsDtoAsync(code), Times.Once);
    }

    [Fact]
    public async Task GetAllExecutionProtocols_EmptyProtocolList_LogsCorrectCount()
    {
        // Arrange
        var protocols = new List<ExecutionProtocolDto>();
        _mockService.Setup(s => s.GetAllAsExecutionProtocolDtosAsync(false)).ReturnsAsync(protocols);

        // Act
        var result = await _controller.GetAllExecutionProtocols();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExecutionProtocolsResponseDto>(okResult.Value);
        Assert.Empty(response.ExecutionProtocols);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved 0 execution protocols")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}