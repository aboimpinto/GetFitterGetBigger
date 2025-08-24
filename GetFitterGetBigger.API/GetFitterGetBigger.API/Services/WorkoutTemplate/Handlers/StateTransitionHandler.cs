using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using Olimpo.EntityFramework.Persistency;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;

/// <summary>
/// Handles state transitions for workout templates
/// </summary>
public class StateTransitionHandler(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IWorkoutStateService workoutStateService,
    ILogger<StateTransitionHandler> logger)
{
    /// <summary>
    /// Changes the state of a workout template
    /// </summary>
    public async Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(
        WorkoutTemplateId templateId, 
        WorkoutStateId newStateId)
    {
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(templateId, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureNotEmpty(newStateId, WorkoutTemplateErrorMessages.InvalidStateIdFormat)
            .EnsureExistsAsync(
                async () => await WorkoutStateExistsAsync(newStateId),
                "WorkoutState")
            .WhenValidAsync(async () => await PerformStateChangeAsync(templateId, newStateId));
    }

    private async Task<ServiceResult<WorkoutTemplateDto>> PerformStateChangeAsync(
        WorkoutTemplateId templateId,
        WorkoutStateId newStateId)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var existingTemplate = await repository.GetByIdWithDetailsAsync(templateId);

        return await ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(existingTemplate, WorkoutTemplateErrorMessages.NotFound)
            .EnsureAsync(
                async () => await IsValidStateTransitionAsync(existingTemplate, newStateId),
                WorkoutTemplateErrorMessages.InvalidStateTransition)
            .WhenValidAsync(async () =>
                {
                    // Update the state - create new record instance with updated values
                    var updatedTemplate = existingTemplate with 
                    { 
                        WorkoutStateId = newStateId,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var savedTemplate = await repository.UpdateAsync(updatedTemplate);
                    await unitOfWork.CommitAsync();

                    logger.LogInformation(
                        "Changed state of WorkoutTemplate {TemplateId} from {OldState} to {NewState}",
                        templateId, existingTemplate.WorkoutStateId, newStateId);

                    return ServiceResult<WorkoutTemplateDto>.Success(savedTemplate.ToDto());
                });
    }

    private async Task<bool> IsValidStateTransitionAsync(
        WorkoutTemplateEntity template,
        WorkoutStateId newStateId)
    {
        var stateDetails = await LoadStateDetailsForTransitionAsync(
            template.WorkoutStateId, 
            newStateId);
        
        return stateDetails switch
        {
            // If we couldn't load the states, transition is invalid
            null => false,
            
            // Apply business rules for valid state transitions
            var (currentState, newState) => IsTransitionAllowed(currentState.Value, newState.Value)
        };
    }
    
    private async Task<(WorkoutStateDto CurrentState, WorkoutStateDto NewState)?> LoadStateDetailsForTransitionAsync(
        WorkoutStateId currentStateId,
        WorkoutStateId newStateId)
    {
        var currentStateResult = await workoutStateService.GetByIdAsync(currentStateId);
        var newStateResult = await workoutStateService.GetByIdAsync(newStateId);
        
        // Both states must be loaded successfully for a valid transition
        return (currentStateResult.IsSuccess && newStateResult.IsSuccess)
            ? (currentStateResult.Data, newStateResult.Data)
            : null;
    }
    
    private static bool IsTransitionAllowed(string fromState, string toState)
    {
        // Business rules: Define which state transitions are NOT allowed
        return (fromState, toState) switch
        {
            (WorkoutStateValues.Production, WorkoutStateValues.Draft) => false,  // Production -> Draft: Not allowed
            (WorkoutStateValues.Archived, WorkoutStateValues.Draft) => false,   // Archived -> Draft: Not allowed
            _ => true  // All other transitions are allowed
        };
    }

    private async Task<bool> WorkoutStateExistsAsync(WorkoutStateId stateId)
    {
        var result = await workoutStateService.ExistsAsync(stateId);
        return result.IsSuccess && result.Data.Value;
    }

    /// <summary>
    /// Constants for workout state values used in business rules
    /// </summary>
    private static class WorkoutStateValues
    {
        public const string Draft = "Draft";
        public const string Production = "Production";
        public const string Archived = "Archived";
    }
}