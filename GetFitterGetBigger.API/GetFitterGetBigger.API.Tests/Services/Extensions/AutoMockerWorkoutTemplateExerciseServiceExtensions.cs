using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Extensions;

public static class AutoMockerWorkoutTemplateExerciseServiceExtensions
{
    /// <summary>
    /// Sets up all required handlers for WorkoutTemplateExerciseService with basic successful mocks
    /// </summary>
    public static AutoMocker SetupWorkoutTemplateExerciseServiceHandlers(this AutoMocker mocker)
    {
        // Setup AutoLinkingHandler
        mocker.GetMock<IAutoLinkingHandler>();

        // Setup ReorderExerciseHandler
        mocker.GetMock<IReorderExerciseHandler>();

        // Setup CopyRoundHandler
        mocker.GetMock<ICopyRoundHandler>();

        // Setup ValidationHandler with basic validation responses
        var validationHandlerMock = mocker.GetMock<IValidationHandler>();
        validationHandlerMock
            .Setup(h => h.DoesTemplateExistAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
        validationHandlerMock
            .Setup(h => h.IsTemplateInDraftStateAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);

        // Setup LegacyMethodsHandler with default failure responses
        var legacyMethodsHandlerMock = mocker.GetMock<ILegacyMethodsHandler>();
        
        // Setup EnhancedMethodsHandler
        mocker.GetMock<IEnhancedMethodsHandler>();

        return mocker;
    }

    /// <summary>
    /// Sets up the ValidationHandler to return that a template does NOT exist
    /// </summary>
    public static AutoMocker SetupTemplateNotExists(this AutoMocker mocker, WorkoutTemplateId templateId)
    {
        mocker.GetMock<IValidationHandler>()
            .Setup(h => h.DoesTemplateExistAsync(templateId))
            .ReturnsAsync(false);
        
        return mocker;
    }

    /// <summary>
    /// Sets up the ValidationHandler to return that a template is NOT in draft state
    /// </summary>
    public static AutoMocker SetupTemplateNotInDraftState(this AutoMocker mocker, WorkoutTemplateId templateId)
    {
        mocker.GetMock<IValidationHandler>()
            .Setup(h => h.IsTemplateInDraftStateAsync(templateId))
            .ReturnsAsync(false);
        
        return mocker;
    }

    /// <summary>
    /// Sets up LegacyMethodsHandler to return validation failure for AddExerciseAsync
    /// </summary>
    public static AutoMocker SetupAddExerciseValidationFailure(this AutoMocker mocker)
    {
        mocker.GetMock<ILegacyMethodsHandler>()
            .Setup(h => h.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty, 
                ServiceError.ValidationFailed("Validation failed")));
        
        return mocker;
    }

    /// <summary>
    /// Sets up LegacyMethodsHandler to return not found for AddExerciseAsync
    /// </summary>
    public static AutoMocker SetupAddExerciseNotFound(this AutoMocker mocker)
    {
        mocker.GetMock<ILegacyMethodsHandler>()
            .Setup(h => h.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty, 
                ServiceError.NotFound("Template or exercise not found")));
        
        return mocker;
    }

    /// <summary>
    /// Sets up LegacyMethodsHandler to return success for AddExerciseAsync
    /// </summary>
    public static AutoMocker SetupAddExerciseSuccess(this AutoMocker mocker)
    {
        var successDto = new WorkoutTemplateExerciseDto
        {
            Id = WorkoutTemplateExerciseId.From(Guid.NewGuid()).ToString(),
            Exercise = new ExerciseDto { Id = ExerciseId.From(Guid.NewGuid()).ToString() },
            Phase = "Main",
            RoundNumber = 1,
            OrderInRound = 1
        };

        mocker.GetMock<ILegacyMethodsHandler>()
            .Setup(h => h.AddExerciseAsync(It.IsAny<AddExerciseToTemplateCommand>()))
            .ReturnsAsync(ServiceResult<WorkoutTemplateExerciseDto>.Success(successDto));
        
        return mocker;
    }

    /// <summary>
    /// Sets up LegacyMethodsHandler to return validation failure for RemoveExerciseAsync
    /// </summary>
    public static AutoMocker SetupRemoveExerciseValidationFailure(this AutoMocker mocker)
    {
        mocker.GetMock<ILegacyMethodsHandler>()
            .Setup(h => h.RemoveExerciseAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty, 
                ServiceError.ValidationFailed("Validation failed")));
        
        return mocker;
    }

    /// <summary>
    /// Sets up LegacyMethodsHandler to return not found for RemoveExerciseAsync
    /// </summary>
    public static AutoMocker SetupRemoveExerciseNotFound(this AutoMocker mocker)
    {
        mocker.GetMock<ILegacyMethodsHandler>()
            .Setup(h => h.RemoveExerciseAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty, 
                ServiceError.NotFound("Exercise not found")));
        
        return mocker;
    }

    /// <summary>
    /// Sets up LegacyMethodsHandler to return success for RemoveExerciseAsync
    /// </summary>
    public static AutoMocker SetupRemoveExerciseSuccess(this AutoMocker mocker)
    {
        mocker.GetMock<ILegacyMethodsHandler>()
            .Setup(h => h.RemoveExerciseAsync(It.IsAny<WorkoutTemplateExerciseId>()))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
        
        return mocker;
    }
}