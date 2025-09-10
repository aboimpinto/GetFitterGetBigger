using FluentAssertions;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
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
        
        var expectedDto = new WorkoutTemplateExercisesDto(
            templateId,
            "Test Template",
            ExecutionProtocolDto.Empty,
            new WorkoutPhaseDto(
                new List<RoundDto>
                {
                    new RoundDto(
                        1,
                        new List<WorkoutTemplateExerciseDto>
                        {
                            WorkoutTemplateExerciseDtoBuilder.Create().WithId("exercise-1").Build(),
                            WorkoutTemplateExerciseDtoBuilder.Create().WithId("exercise-2").Build()
                        })
                }),
            new WorkoutPhaseDto(new List<RoundDto>()),
            new WorkoutPhaseDto(new List<RoundDto>()));
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.GetTemplateExercisesAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExercisesDto>.Success(expectedDto));

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
            .Setup(x => x.GetTemplateExercisesAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExercisesDto>.Failure(
                WorkoutTemplateExercisesDto.Empty,
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
            .Setup(x => x.GetTemplateExercisesAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExercisesDto>.Failure(
                WorkoutTemplateExercisesDto.Empty,
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
            .Setup(x => x.GetTemplateExercisesAsync(templateId))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExercisesDto>.Failure(
                WorkoutTemplateExercisesDto.Empty,
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
            .Setup(x => x.GetExerciseByIdAsync(exerciseId))
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
            .Setup(x => x.GetExerciseByIdAsync(exerciseId))
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
        
        var expectedResultDto = new AddExerciseResultDto(
            new List<WorkoutTemplateExerciseDto>
            {
                WorkoutTemplateExerciseDtoBuilder.Create()
                    .WithId("workout-exercise-123")
                    .WithPhase(request.Zone)
                    .WithNotes(request.Notes)
                    .WithOrderInRound(request.SequenceOrder ?? 1)
                    .Build()
            },
            "Exercise added successfully");
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<AddExerciseDto>()))
            .ReturnsAsync(ServiceResult<AddExerciseResultDto>.Success(expectedResultDto));

        // Act
        var result = await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.ActionName.Should().Be(nameof(WorkoutTemplateExercisesController.GetWorkoutTemplateExercise));
        createdResult.Value.Should().BeEquivalentTo(expectedResultDto);
        
        // Verify the route values
        createdResult.RouteValues.Should().ContainKey("templateId");
        createdResult.RouteValues.Should().ContainKey("exerciseId");
        createdResult.RouteValues["exerciseId"].Should().Be(expectedResultDto.AddedExercises.FirstOrDefault()?.Id ?? string.Empty);
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
            .Setup(x => x.AddExerciseAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<AddExerciseDto>()))
            .ReturnsAsync(ServiceResult<AddExerciseResultDto>.Failure(
                AddExerciseResultDto.Empty,
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
            .Setup(x => x.AddExerciseAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<AddExerciseDto>()))
            .ReturnsAsync(ServiceResult<AddExerciseResultDto>.Failure(
                AddExerciseResultDto.Empty,
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
            .Setup(x => x.AddExerciseAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<AddExerciseDto>()))
            .ReturnsAsync(ServiceResult<AddExerciseResultDto>.Failure(
                AddExerciseResultDto.Empty,
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
            .Setup(x => x.RemoveExerciseAsync(WorkoutTemplateId.ParseOrEmpty(templateId), exerciseId))
            .ReturnsAsync(ServiceResult<RemoveExerciseResultDto>.Success(new RemoveExerciseResultDto(
                new List<WorkoutTemplateExerciseDto>
                {
                    WorkoutTemplateExerciseDtoBuilder.Create().WithId(exerciseIdString).Build()
                },
                "Exercise removed successfully")));

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
            .Setup(x => x.RemoveExerciseAsync(WorkoutTemplateId.ParseOrEmpty(templateId), exerciseId))
            .ReturnsAsync(ServiceResult<RemoveExerciseResultDto>.Failure(
                RemoveExerciseResultDto.Empty,
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
            .Setup(x => x.RemoveExerciseAsync(WorkoutTemplateId.ParseOrEmpty(templateId), exerciseId))
            .ReturnsAsync(ServiceResult<RemoveExerciseResultDto>.Failure(
                RemoveExerciseResultDto.Empty,
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
            .Setup(x => x.RemoveExerciseAsync(WorkoutTemplateId.ParseOrEmpty(templateId), exerciseId))
            .ReturnsAsync(ServiceResult<RemoveExerciseResultDto>.Failure(
                RemoveExerciseResultDto.Empty,
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
        
        var expectedResultDto = new AddExerciseResultDto(
            new List<WorkoutTemplateExerciseDto> { WorkoutTemplateExerciseDtoBuilder.Create().Build() },
            "Exercise added successfully");
        
        WorkoutTemplateId? capturedTemplateId = null;
        AddExerciseDto? capturedDto = null;
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<AddExerciseDto>()))
            .Callback<WorkoutTemplateId, AddExerciseDto>((tId, dto) => { capturedTemplateId = tId; capturedDto = dto; })
            .ReturnsAsync(ServiceResult<AddExerciseResultDto>.Success(expectedResultDto));

        // Act
        await controller.AddExerciseToTemplate(templateId, request);

        // Assert
        capturedTemplateId.Should().NotBeNull();
        capturedDto.Should().NotBeNull();
        capturedTemplateId.Should().Be(WorkoutTemplateId.ParseOrEmpty(templateId));
        capturedDto!.ExerciseId.Should().Be(ExerciseId.ParseOrEmpty(request.ExerciseId));
        capturedDto!.Phase.Should().Be(request.Zone);
        capturedDto!.Metadata.Should().Be(request.Notes ?? string.Empty);
        capturedDto!.RoundNumber.Should().Be(1);
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
        
        var expectedResultDto = new AddExerciseResultDto(
            new List<WorkoutTemplateExerciseDto> { WorkoutTemplateExerciseDtoBuilder.Create().Build() },
            "Exercise added successfully");
        
        autoMocker.GetMock<IWorkoutTemplateExerciseService>()
            .Setup(x => x.AddExerciseAsync(It.IsAny<WorkoutTemplateId>(), It.IsAny<AddExerciseDto>()))
            .ReturnsAsync(ServiceResult<AddExerciseResultDto>.Success(expectedResultDto));

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