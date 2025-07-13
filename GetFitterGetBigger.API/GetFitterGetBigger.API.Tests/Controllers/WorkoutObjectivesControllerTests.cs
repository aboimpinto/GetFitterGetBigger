using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class WorkoutObjectivesControllerTests
{
    private readonly Mock<IWorkoutObjectiveService> _mockService;
    private readonly Mock<ILogger<WorkoutObjectivesController>> _mockLogger;
    private readonly WorkoutObjectivesController _controller;

    public WorkoutObjectivesControllerTests()
    {
        _mockService = new Mock<IWorkoutObjectiveService>();
        _mockLogger = new Mock<ILogger<WorkoutObjectivesController>>();
        _controller = new WorkoutObjectivesController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllWorkoutObjectives_ReturnsOkResultWithObjectives()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>
        {
            ReferenceDataDtoTestBuilder.MuscularStrength().Build(),
            ReferenceDataDtoTestBuilder.MuscularHypertrophy().Build()
        };
        _mockService.Setup(s => s.GetAllAsDtosAsync()).ReturnsAsync(objectives);

        // Act
        var result = await _controller.GetAllWorkoutObjectives();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedObjectives = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Equal(2, returnedObjectives.Count());
        _mockService.Verify(s => s.GetAllAsDtosAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_ValidId_ReturnsOkResultWithObjective()
    {
        // Arrange
        var id = TestIds.WorkoutObjectiveIds.MuscularStrength;
        var objective = ReferenceDataDtoTestBuilder.MuscularStrength().Build();
        _mockService.Setup(s => s.GetByIdAsDtoAsync(id)).ReturnsAsync(objective);

        // Act
        var result = await _controller.GetWorkoutObjectiveById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedObjective = Assert.IsType<ReferenceDataDto>(okResult.Value);
        Assert.Equal(id, returnedObjective.Id);
        Assert.Equal("Muscular Strength", returnedObjective.Value);
        _mockService.Verify(s => s.GetByIdAsDtoAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var id = $"workoutobjective-{TestIds.NotFoundId}";
        _mockService.Setup(s => s.GetByIdAsDtoAsync(id)).ReturnsAsync((ReferenceDataDto?)null);

        // Act
        var result = await _controller.GetWorkoutObjectiveById(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
        _mockService.Verify(s => s.GetByIdAsDtoAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_EmptyObjectiveList_LogsCorrectCount()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>();
        _mockService.Setup(s => s.GetAllAsDtosAsync()).ReturnsAsync(objectives);

        // Act
        var result = await _controller.GetAllWorkoutObjectives();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedObjectives = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Empty(returnedObjectives);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved 0 workout objectives")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}