using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

public class WorkoutObjectivesControllerTests
{
    private readonly Mock<IWorkoutObjectiveService> _serviceMock;
    private readonly Mock<ILogger<WorkoutObjectivesController>> _loggerMock;
    private readonly WorkoutObjectivesController _controller;

    public WorkoutObjectivesControllerTests()
    {
        _serviceMock = new Mock<IWorkoutObjectiveService>();
        _loggerMock = new Mock<ILogger<WorkoutObjectivesController>>();
        _controller = new WorkoutObjectivesController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetWorkoutObjectives_ReturnsOkResult()
    {
        // Arrange
        var workoutObjectives = new List<ReferenceDataDto>
        {
            new ReferenceDataDto
            {
                Id = "workoutobjective-10000001-1000-4000-8000-100000000001",
                Value = "Muscular Strength",
                Description = "Build maximum strength through heavy loads and low repetitions"
            }
        };

        _serviceMock.Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ReferenceDataDto>>.Success(workoutObjectives));

        // Act
        var result = await _controller.GetWorkoutObjectives();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedObjectives = Assert.IsAssignableFrom<IEnumerable<ReferenceDataDto>>(okResult.Value);
        Assert.Equal(workoutObjectives.Count(), returnedObjectives.Count());
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var workoutObjectiveId = "workoutobjective-10000001-1000-4000-8000-100000000001";
        var workoutObjective = new ReferenceDataDto
        {
            Id = workoutObjectiveId,
            Value = "Muscular Strength",
            Description = "Build maximum strength through heavy loads and low repetitions"
        };

        _serviceMock.Setup(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Success(workoutObjective));

        // Act
        var result = await _controller.GetWorkoutObjectiveById(workoutObjectiveId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedObjective = Assert.IsAssignableFrom<ReferenceDataDto>(okResult.Value);
        Assert.Equal(workoutObjective.Id, returnedObjective.Id);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_WithNotFoundId_ReturnsNotFound()
    {
        // Arrange
        var workoutObjectiveId = "workoutobjective-00000000-0000-0000-0000-000000000000";

        _serviceMock.Setup(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), ServiceError.NotFound("Not found")));

        // Act
        var result = await _controller.GetWorkoutObjectiveById(workoutObjectiveId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetWorkoutObjectiveById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = "invalid-id-format";

        _serviceMock.Setup(x => x.GetByIdAsync(It.IsAny<WorkoutObjectiveId>()))
            .ReturnsAsync(ServiceResult<ReferenceDataDto>.Failure(new ReferenceDataDto(), ServiceError.NotFound("Workout objective not found")));

        // Act
        var result = await _controller.GetWorkoutObjectiveById(invalidId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}