using FluentAssertions;
using GetFitterGetBigger.API.Controllers;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.SetConfiguration;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Controllers;

/// <summary>
/// Unit tests for SetConfigurationsController
/// Tests focus on HTTP response mapping, parameter validation, and proper service integration
/// </summary>
public class SetConfigurationsControllerTests
{
    [Fact]
    public async Task GetSetConfigurations_ValidRequest_ReturnsOkWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        
        var expectedDtos = new List<SetConfigurationDto>
        {
            SetConfigurationDtoBuilder.Create().WithId("set-1").Build(),
            SetConfigurationDtoBuilder.Create().WithId("set-2").Build()
        };
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.GetByWorkoutTemplateExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<IEnumerable<SetConfigurationDto>>.Success(expectedDtos));

        // Act
        var result = await controller.GetSetConfigurations(templateId, exerciseId.ToString());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetSetConfigurations_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.GetByWorkoutTemplateExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                Enumerable.Empty<SetConfigurationDto>(),
                ServiceError.NotFound("Exercise")));

        // Act
        var result = await controller.GetSetConfigurations(templateId, exerciseId.ToString());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSetConfigurations_ServiceReturnsUnauthorized_ReturnsForbid()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.GetByWorkoutTemplateExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                Enumerable.Empty<SetConfigurationDto>(),
                ServiceError.Unauthorized("Access denied")));

        // Act
        var result = await controller.GetSetConfigurations(templateId, exerciseId.ToString());

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetSetConfigurations_ServiceReturnsOtherError_ReturnsBadRequest()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var exerciseId = WorkoutTemplateExerciseId.New();
        var templateId = "template-123";
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.ValidationFailed("Invalid exercise")
        };
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.GetByWorkoutTemplateExerciseAsync(exerciseId))
            .ReturnsAsync(ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                Enumerable.Empty<SetConfigurationDto>(),
                expectedErrors));

        // Act
        var result = await controller.GetSetConfigurations(templateId, exerciseId.ToString());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSetConfiguration_ValidRequest_ReturnsOkWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var setId = SetConfigurationId.New();
        var templateId = "template-123";
        var exerciseId = "exercise-456";
        
        var expectedDto = SetConfigurationDtoBuilder.Create().WithId(setId.ToString()).Build();
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.GetByIdAsync(setId))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Success(expectedDto));

        // Act
        var result = await controller.GetSetConfiguration(templateId, exerciseId, setId.ToString());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task GetSetConfiguration_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var setId = SetConfigurationId.New();
        var templateId = "template-123";
        var exerciseId = "exercise-456";
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.GetByIdAsync(setId))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Failure(
                SetConfigurationDto.Empty,
                ServiceError.NotFound("SetConfiguration")));

        // Act
        var result = await controller.GetSetConfiguration(templateId, exerciseId, setId.ToString());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateSetConfiguration_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var templateId = "template-123";
        var exerciseId = WorkoutTemplateExerciseId.New().ToString();
        
        var request = new CreateSetConfigurationDto
        {
            SetNumber = 1,
            TargetReps = "8-12",
            TargetWeight = 80.5m,
            RestSeconds = 90
        };
        
        var expectedDto = SetConfigurationDtoBuilder.Create()
            .WithId("set-123")
            .WithSetNumber(request.SetNumber ?? 1)
            .WithTargetReps(request.TargetReps)
            .WithTargetWeight(request.TargetWeight)
            .WithRestSeconds(request.RestSeconds)
            .Build();
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateSetConfigurationCommand>()))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Success(expectedDto));

        // Act
        var result = await controller.CreateSetConfiguration(templateId, exerciseId, request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.ActionName.Should().Be(nameof(SetConfigurationsController.GetSetConfiguration));
        createdResult.Value.Should().BeEquivalentTo(expectedDto);
        
        // Verify the route values
        createdResult.RouteValues.Should().ContainKey("templateId");
        createdResult.RouteValues.Should().ContainKey("exerciseId");
        createdResult.RouteValues.Should().ContainKey("setId");
        createdResult.RouteValues["setId"].Should().Be(expectedDto.Id);
    }

    [Fact]
    public async Task CreateSetConfiguration_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var templateId = "template-123";
        var exerciseId = WorkoutTemplateExerciseId.New().ToString();
        var request = CreateSetConfigurationDtoBuilder.Create().Build();
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateSetConfigurationCommand>()))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Failure(
                SetConfigurationDto.Empty,
                ServiceError.NotFound("Exercise")));

        // Act
        var result = await controller.CreateSetConfiguration(templateId, exerciseId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateSetConfiguration_ServiceReturnsAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var templateId = "template-123";
        var exerciseId = WorkoutTemplateExerciseId.New().ToString();
        var request = CreateSetConfigurationDtoBuilder.Create().Build();
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.AlreadyExists("SetConfiguration", "1")
        };
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateSetConfigurationCommand>()))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Failure(
                SetConfigurationDto.Empty,
                expectedErrors));

        // Act
        var result = await controller.CreateSetConfiguration(templateId, exerciseId, request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = (ConflictObjectResult)result;
        conflictResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSetConfiguration_ServiceReturnsValidationError_ReturnsBadRequest()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var templateId = "template-123";
        var exerciseId = WorkoutTemplateExerciseId.New().ToString();
        var request = CreateSetConfigurationDtoBuilder.Create().Build();
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.ValidationFailed("Invalid set number")
        };
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateSetConfigurationCommand>()))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Failure(
                SetConfigurationDto.Empty,
                expectedErrors));

        // Act
        var result = await controller.CreateSetConfiguration(templateId, exerciseId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteSetConfiguration_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var setId = SetConfigurationId.New();
        var templateId = "template-123";
        var exerciseId = "exercise-456";
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.DeleteAsync(setId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = true }));

        // Act
        var result = await controller.DeleteSetConfiguration(templateId, exerciseId, setId.ToString());

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteSetConfiguration_ServiceReturnsNotFound_ReturnsNotFound()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var setId = SetConfigurationId.New();
        var templateId = "template-123";
        var exerciseId = "exercise-456";
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.DeleteAsync(setId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                new BooleanResultDto { Value = false },
                ServiceError.NotFound("SetConfiguration")));

        // Act
        var result = await controller.DeleteSetConfiguration(templateId, exerciseId, setId.ToString());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteSetConfiguration_ServiceReturnsUnauthorized_ReturnsForbid()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var setId = SetConfigurationId.New();
        var templateId = "template-123";
        var exerciseId = "exercise-456";
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.DeleteAsync(setId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                new BooleanResultDto { Value = false },
                ServiceError.Unauthorized("Access denied")));

        // Act
        var result = await controller.DeleteSetConfiguration(templateId, exerciseId, setId.ToString());

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteSetConfiguration_ServiceReturnsOtherError_ReturnsBadRequest()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var setId = SetConfigurationId.New();
        var templateId = "template-123";
        var exerciseId = "exercise-456";
        
        var expectedErrors = new List<ServiceError>
        {
            ServiceError.ValidationFailed("Cannot delete active set")
        };
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.DeleteAsync(setId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                new BooleanResultDto { Value = false },
                expectedErrors));

        // Act
        var result = await controller.DeleteSetConfiguration(templateId, exerciseId, setId.ToString());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSetConfiguration_CreatesCorrectCommand()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var templateId = "template-123";
        var exerciseId = WorkoutTemplateExerciseId.New().ToString();
        
        var request = new CreateSetConfigurationDto
        {
            SetNumber = 3,
            TargetReps = "12-15",
            TargetWeight = 75.0m,
            TargetTimeSeconds = 30,
            RestSeconds = 120
        };
        
        var expectedDto = SetConfigurationDtoBuilder.Create().Build();
        
        CreateSetConfigurationCommand? capturedCommand = null;
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateSetConfigurationCommand>()))
            .Callback<CreateSetConfigurationCommand>(cmd => capturedCommand = cmd)
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Success(expectedDto));

        // Act
        await controller.CreateSetConfiguration(templateId, exerciseId, request);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.WorkoutTemplateExerciseId.ToString().Should().Be(exerciseId);
        capturedCommand!.SetNumber.Should().Be(request.SetNumber);
        capturedCommand!.TargetReps.Should().Be(request.TargetReps);
        capturedCommand!.TargetWeight.Should().Be(request.TargetWeight);
        capturedCommand!.TargetTimeSeconds.Should().Be(request.TargetTimeSeconds);
        capturedCommand!.RestSeconds.Should().Be(request.RestSeconds);
    }

    [Fact]
    public async Task CreateSetConfiguration_LogsRequestCorrectly()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var controller = autoMocker.CreateInstance<SetConfigurationsController>();
        
        var templateId = "template-123";
        var exerciseId = WorkoutTemplateExerciseId.New().ToString();
        var request = CreateSetConfigurationDtoBuilder.Create().Build();
        
        var expectedDto = SetConfigurationDtoBuilder.Create().Build();
        
        autoMocker.GetMock<ISetConfigurationService>()
            .Setup(x => x.CreateAsync(It.IsAny<CreateSetConfigurationCommand>()))
            .ReturnsAsync(ServiceResult<SetConfigurationDto>.Success(expectedDto));

        // Act
        await controller.CreateSetConfiguration(templateId, exerciseId, request);

        // Assert
        autoMocker.GetMock<ILogger<SetConfigurationsController>>()
            .Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Creating set configuration")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
    }
}