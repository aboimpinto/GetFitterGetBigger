using FluentAssertions;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Controllers;

/// <summary>
/// Unit tests for WorkoutTemplateExercisesController
/// Tests focus on HTTP response mapping, parameter validation, and proper service integration
/// </summary>
public class WorkoutTemplateExercisesControllerTests
{
    [Fact]
    public async Task GetWorkoutTemplateExercises_ValidRequest_ReturnsOkWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = WorkoutTemplateId.New();
        var templateIdString = templateId.ToString();
        
        var expectedDto = new WorkoutTemplateExerciseListDto
        {
            WorkoutTemplateId = templateIdString,
            MainExercises = new List<WorkoutTemplateExerciseDto>
            {
                WorkoutTemplateExerciseDtoBuilder.Create().WithId("exercise-1").Build(),
                WorkoutTemplateExerciseDtoBuilder.Create().WithId("exercise-2").Build()
            }
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Success(expectedDto));

        // Act
        var result = await controller.GetWorkoutTemplateExercises(templateIdString);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task GetWorkoutTemplateExercises_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = WorkoutTemplateId.New();
        var templateIdString = templateId.ToString();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Failure(
                WorkoutTemplateExerciseListDto.Empty,
                ServiceError.NotFound("WorkoutTemplate")));

        // Act
        var result = await controller.GetWorkoutTemplateExercises(templateIdString);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetWorkoutTemplateExercises_ServiceReturnsUnauthorized_ReturnsForbid()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = WorkoutTemplateId.New();
        var templateIdString = templateId.ToString();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Failure(
                WorkoutTemplateExerciseListDto.Empty,
                ServiceError.Unauthorized("Access denied")));

        // Act
        var result = await controller.GetWorkoutTemplateExercises(templateIdString);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetWorkoutTemplateExercises_ServiceReturnsOtherError_ReturnsBadRequest()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = WorkoutTemplateId.New();
        var templateIdString = templateId.ToString();
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.ValidationFailed("Invalid template")
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByWorkoutTemplateAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseListDto>.Failure(
                WorkoutTemplateExerciseListDto.Empty,
                expectedErrors));

        // Act
        var result = await controller.GetWorkoutTemplateExercises(templateIdString);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWorkoutTemplateExercise_ValidRequest_ReturnsOkWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        var exerciseIdString = exerciseId.ToString();
        
        var expectedDto = WorkoutTemplateExerciseDtoBuilder.Create()
            .WithId(exerciseIdString)
            .Build();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Success(expectedDto));

        // Act
        var result = await controller.GetWorkoutTemplateExercise(templateId, exerciseIdString);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task GetWorkoutTemplateExercise_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        var exerciseIdString = exerciseId.ToString();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetByIdAsync(exerciseId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                ServiceError.NotFound("WorkoutTemplateExercise")));

        // Act
        var result = await controller.GetWorkoutTemplateExercise(templateId, exerciseIdString);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AddExerciseToTemplate_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = "template-123";
        
        var request = new AddExerciseToTemplateDto
        {
            ExerciseId = ExerciseId.New().ToString(),
            Zone = "Main",
            Notes = "Focus on form",
            SequenceOrder = 1
        };
        
        var expectedDto = WorkoutTemplateExerciseDtoBuilder.Create()
            .WithId("workout-exercise-123")
            .WithZone(request.Zone)
            .WithNotes(request.Notes)
            .WithSequenceOrder(request.SequenceOrder ?? 1)
            .Build();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Success(expectedDto));

        // Act
        var result = await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.ActionName.Should().Be(nameof(WorkoutTemplateExercisesController.GetWorkoutTemplateExercise));
        createdResult.Value.Should().BeEquivalentTo(expectedDto);
        
        // Verify the route values
        createdResult.RouteValues.Should().ContainKey("templateId");
        createdResult.RouteValues.Should().ContainKey("exerciseId");
        createdResult.RouteValues["exerciseId"].Should().Be(expectedDto.Id);
    }

    [Fact]
    public async Task AddExerciseToTemplate_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = "template-123";
        var request = new AddExerciseToTemplateDto
        {
            ExerciseId = ExerciseId.New().ToString(),
            Zone = "Main"
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                ServiceError.NotFound("Exercise")));

        // Act
        var result = await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AddExerciseToTemplate_ServiceReturnsAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = "template-123";
        var request = new AddExerciseToTemplateDto
        {
            ExerciseId = ExerciseId.New().ToString(),
            Zone = "Main"
        };
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.AlreadyExists("Exercise", request.ExerciseId)
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                expectedErrors));

        // Act
        var result = await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = (ConflictObjectResult)result;
        conflictResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task AddExerciseToTemplate_ServiceReturnsValidationError_ReturnsBadRequest()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = "template-123";
        var request = new AddExerciseToTemplateDto
        {
            ExerciseId = ExerciseId.New().ToString(),
            Zone = "Main"
        };
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.ValidationFailed("Invalid zone")
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                expectedErrors));

        // Act
        var result = await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveExerciseFromTemplate_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        var exerciseIdString = exerciseId.ToString();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.RemoveExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));

        // Act
        var result = await controller.RemoveExerciseFromTemplate(templateId, exerciseIdString);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveExerciseFromTemplate_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        var exerciseIdString = exerciseId.ToString();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.RemoveExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                new BooleanResultDto { Value = false },
                ServiceError.NotFound("WorkoutTemplateExercise")));

        // Act
        var result = await controller.RemoveExerciseFromTemplate(templateId, exerciseIdString);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RemoveExerciseFromTemplate_ServiceReturnsUnauthorized_ReturnsForbid()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        var exerciseIdString = exerciseId.ToString();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.RemoveExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                new BooleanResultDto { Value = false },
                ServiceError.Unauthorized("Access denied")));

        // Act
        var result = await controller.RemoveExerciseFromTemplate(templateId, exerciseIdString);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task RemoveExerciseFromTemplate_ServiceReturnsOtherError_ReturnsBadRequest()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        var exerciseIdString = exerciseId.ToString();
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.ValidationFailed("Cannot remove exercise with active sets")
        };
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.RemoveExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                new BooleanResultDto { Value = false },
                expectedErrors));

        // Act
        var result = await controller.RemoveExerciseFromTemplate(templateId, exerciseIdString);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task AddExerciseToTemplate_CreatesCorrectCommand()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = "template-123";
        var request = new AddExerciseToTemplateDto
        {
            ExerciseId = ExerciseId.New().ToString(),
            Zone = "Warmup",
            Notes = "Start slowly",
            SequenceOrder = 2
        };
        
        var expectedDto = WorkoutTemplateExerciseDtoBuilder.Create().Build();
        
        AddExerciseToTemplateCommand? capturedCommand = null;
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .Callback<AddExerciseToTemplateCommand>(cmd => capturedCommand = cmd)
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Success(expectedDto));

        // Act
        await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        capturedCommand.Should().NotBeNull();
        // The controller parses the templateId string to create WorkoutTemplateId, so compare the parsed result
        capturedCommand!.WorkoutTemplateId.Should().Be(WorkoutTemplateId.ParseOrEmpty(templateId));
        capturedCommand!.ExerciseId.Should().Be(ExerciseId.ParseOrEmpty(request.ExerciseId));
        capturedCommand!.Zone.Should().Be(request.Zone);
        capturedCommand!.Notes.Should().Be(request.Notes);
        capturedCommand!.SequenceOrder.Should().Be(request.SequenceOrder);
    }

    [Fact]
    public async Task AddExerciseToTemplate_LogsRequestCorrectly()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<WorkoutTemplateExercisesController>();
        
        var templateId = "template-123";
        var request = new AddExerciseToTemplateDto
        {
            ExerciseId = ExerciseId.New().ToString(),
            Zone = "Main",
            Notes = "Focus on form"
        };
        
        var expectedDto = WorkoutTemplateExerciseDtoBuilder.Create().Build();
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Success(expectedDto));

        // Act
        await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        autoMocker.GetMock<ILogger<WorkoutTemplateExercisesController>>()
            .Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Adding exercise")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
    }
}