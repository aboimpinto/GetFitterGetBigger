using GetFitterGetBigger.API.Models.SpecializedIds;
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
}