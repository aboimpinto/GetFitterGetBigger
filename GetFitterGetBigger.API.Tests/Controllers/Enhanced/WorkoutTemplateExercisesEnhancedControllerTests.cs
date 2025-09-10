using System.Text.Json;
using GetFitterGetBigger.API.Controllers.Enhanced;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace GetFitterGetBigger.API.Tests.Controllers.Enhanced;

public class WorkoutTemplateExercisesEnhancedControllerTests
{
    private readonly IWorkoutTemplateExerciseService _mockService;
    private readonly WorkoutTemplateExercisesEnhancedController _controller;

    public WorkoutTemplateExercisesEnhancedControllerTests()
    {
        _mockService = Substitute.For<IWorkoutTemplateExerciseService>();
        _controller = new WorkoutTemplateExercisesEnhancedController(_mockService);
    }

    [Fact]
    public async Task AddExercise_WithValidRequest_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
        var request = new AddExerciseToTemplateRequest
        {
            ExerciseId = "exercise-550e8400-e29b-41d4-a716-446655440000",
            Phase = "Workout",
            RoundNumber = 1,
            Metadata = JsonDocument.Parse("""{"reps": 10}""")
        };

        var resultData = AddExerciseResultDto.Empty;
        var serviceResult = ServiceResult<AddExerciseResultDto>.Success(resultData);

        _mockService
            .AddExerciseAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<AddExerciseDto>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.AddExercise(templateId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(WorkoutTemplateExercisesEnhancedController.GetTemplateExercises));
        createdResult.Value.Should().BeOfType<AddExerciseResponseDto>();

        await _mockService.Received(1)
            .AddExerciseAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<AddExerciseDto>());
    }

    [Fact]
    public async Task AddExercise_WhenServiceReturnsNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var templateId = "invalid-id";
        var request = new AddExerciseToTemplateRequest
        {
            ExerciseId = "exercise-id",
            Phase = "Workout",
            RoundNumber = 1,
            Metadata = JsonDocument.Parse("""{"reps": 10}""")
        };

        var serviceError = ServiceError.NotFound("Template not found");
        var serviceResult = ServiceResult<AddExerciseResultDto>.Failure(AddExerciseResultDto.Empty, serviceError);

        _mockService
            .AddExerciseAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<AddExerciseDto>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.AddExercise(templateId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeOfType<ErrorResponseDto>();
    }

    [Fact]
    public async Task RemoveExercise_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
        var exerciseId = "workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000";

        var resultData = RemoveExerciseResultDto.Empty;
        var serviceResult = ServiceResult<RemoveExerciseResultDto>.Success(resultData);

        _mockService
            .RemoveExerciseAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<WorkoutTemplateExerciseId>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.RemoveExercise(templateId, exerciseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<RemoveExerciseResponseDto>();

        await _mockService.Received(1)
            .RemoveExerciseAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<WorkoutTemplateExerciseId>());
    }

    [Fact]
    public async Task GetTemplateExercises_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";

        var resultData = WorkoutTemplateExercisesDto.Empty;
        var serviceResult = ServiceResult<WorkoutTemplateExercisesDto>.Success(resultData);

        _mockService
            .GetTemplateExercisesAsync(Arg.Any<WorkoutTemplateId>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.GetTemplateExercises(templateId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<WorkoutTemplateExercisesResponseDto>();

        await _mockService.Received(1)
            .GetTemplateExercisesAsync(Arg.Any<WorkoutTemplateId>());
    }

    [Fact]
    public async Task UpdateExerciseMetadata_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
        var exerciseId = "workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000";
        var request = new UpdateExerciseMetadataRequest
        {
            Metadata = JsonDocument.Parse("""{"reps": 12, "weight": {"value": 65, "unit": "kg"}}""")
        };

        var resultData = UpdateMetadataResultDto.Empty;
        var serviceResult = ServiceResult<UpdateMetadataResultDto>.Success(resultData);

        _mockService
            .UpdateExerciseMetadataAsync(
                Arg.Any<WorkoutTemplateId>(), 
                Arg.Any<WorkoutTemplateExerciseId>(), 
                Arg.Any<string>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.UpdateExerciseMetadata(templateId, exerciseId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<UpdateMetadataResponseDto>();

        await _mockService.Received(1)
            .UpdateExerciseMetadataAsync(
                Arg.Any<WorkoutTemplateId>(), 
                Arg.Any<WorkoutTemplateExerciseId>(), 
                Arg.Any<string>());
    }

    [Fact]
    public async Task ReorderExercise_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
        var exerciseId = "workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000";
        var request = new ReorderExerciseRequest
        {
            NewOrderInRound = 2
        };

        var resultData = ReorderResultDto.Empty;
        var serviceResult = ServiceResult<ReorderResultDto>.Success(resultData);

        _mockService
            .ReorderExerciseAsync(
                Arg.Any<WorkoutTemplateId>(), 
                Arg.Any<WorkoutTemplateExerciseId>(), 
                Arg.Any<int>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.ReorderExercise(templateId, exerciseId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ReorderResponseDto>();

        await _mockService.Received(1)
            .ReorderExerciseAsync(
                Arg.Any<WorkoutTemplateId>(), 
                Arg.Any<WorkoutTemplateExerciseId>(), 
                2);
    }

    [Fact]
    public async Task CopyRound_WithValidRequest_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
        var request = new CopyRoundRequest
        {
            SourcePhase = "Workout",
            SourceRoundNumber = 1,
            TargetPhase = "Workout",
            TargetRoundNumber = 2
        };

        var resultData = CopyRoundResultDto.Empty;
        var serviceResult = ServiceResult<CopyRoundResultDto>.Success(resultData);

        _mockService
            .CopyRoundAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<CopyRoundDto>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.CopyRound(templateId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(WorkoutTemplateExercisesEnhancedController.GetTemplateExercises));
        createdResult.Value.Should().BeOfType<CopyRoundResponseDto>();

        await _mockService.Received(1)
            .CopyRoundAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<CopyRoundDto>());
    }

    [Fact]
    public async Task AddExercise_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var templateId = "workouttemplate-550e8400-e29b-41d4-a716-446655440000";
        var request = new AddExerciseToTemplateRequest
        {
            ExerciseId = "exercise-id",
            Phase = "Workout",
            RoundNumber = 1,
            Metadata = JsonDocument.Parse("""{"reps": 10}""")
        };

        var serviceError = ServiceError.ValidationFailed("Invalid phase");
        var serviceResult = ServiceResult<AddExerciseResultDto>.Failure(AddExerciseResultDto.Empty, serviceError);

        _mockService
            .AddExerciseAsync(Arg.Any<WorkoutTemplateId>(), Arg.Any<AddExerciseDto>())
            .Returns(serviceResult);

        // Act
        var result = await _controller.AddExercise(templateId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<ErrorResponseDto>();
    }

    [Fact]
    public void Controller_ShouldHaveCorrectRouteAndAuthorization()
    {
        // Arrange & Act
        var controllerType = typeof(WorkoutTemplateExercisesEnhancedController);

        // Assert
        var routeAttribute = controllerType.GetCustomAttributes(typeof(RouteAttribute), false)
            .FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v2/workout-templates/{templateId}/exercises");

        var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .FirstOrDefault();
        authorizeAttribute.Should().NotBeNull();
    }
}