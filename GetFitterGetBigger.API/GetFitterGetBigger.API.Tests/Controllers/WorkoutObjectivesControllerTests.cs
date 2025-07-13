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
        var objectives = new List<WorkoutObjectiveDto>
        {
            new WorkoutObjectiveDtoTestBuilder().WithMuscularStrength().Build(),
            new WorkoutObjectiveDtoTestBuilder().WithMuscularHypertrophy().Build()
        };
        _mockService.Setup(s => s.GetAllAsWorkoutObjectiveDtosAsync(false)).ReturnsAsync(objectives);

        // Act
        var result = await _controller.GetAllWorkoutObjectives();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<WorkoutObjectivesResponseDto>(okResult.Value);
        Assert.Equal(2, response.WorkoutObjectives.Count);
        _mockService.Verify(s => s.GetAllAsWorkoutObjectiveDtosAsync(false), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_ValidId_ReturnsOkResultWithObjective()
    {
        // Arrange
        var id = TestIds.WorkoutObjectiveIds.MuscularStrength;
        var objective = new WorkoutObjectiveDtoTestBuilder().WithMuscularStrength().Build();
        _mockService.Setup(s => s.GetByIdAsWorkoutObjectiveDtoAsync(id, false)).ReturnsAsync(objective);

        // Act
        var result = await _controller.GetWorkoutObjectiveById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedObjective = Assert.IsType<WorkoutObjectiveDto>(okResult.Value);
        Assert.Equal(id, returnedObjective.WorkoutObjectiveId);
        Assert.Equal("Muscular Strength", returnedObjective.Value);
        _mockService.Verify(s => s.GetByIdAsWorkoutObjectiveDtoAsync(id, false), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_NotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var id = WorkoutObjectiveId.From(TestIds.NotFoundId);
        _mockService.Setup(s => s.GetByIdAsWorkoutObjectiveDtoAsync(id.ToString(), false)).ReturnsAsync((WorkoutObjectiveDto?)null);

        // Act
        var result = await _controller.GetWorkoutObjectiveById(id.ToString());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
        _mockService.Verify(s => s.GetByIdAsWorkoutObjectiveDtoAsync(id.ToString(), false), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_EmptyObjectiveList_LogsCorrectCount()
    {
        // Arrange
        var objectives = new List<WorkoutObjectiveDto>();
        _mockService.Setup(s => s.GetAllAsWorkoutObjectiveDtosAsync(false)).ReturnsAsync(objectives);

        // Act
        var result = await _controller.GetAllWorkoutObjectives();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<WorkoutObjectivesResponseDto>(okResult.Value);
        Assert.Empty(response.WorkoutObjectives);
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