using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

namespace GetFitterGetBigger.API.Mappers;

/// <summary>
/// Maps between web request DTOs and workout template exercise service commands.
/// Implements the ToCommand pattern following the No Null Command Pattern.
/// </summary>
public static class WorkoutTemplateExerciseRequestMapper
{
    /// <summary>
    /// Maps AddExerciseToTemplateDto to AddExerciseToTemplateCommand.
    /// Always returns a valid command object, never null.
    /// </summary>
    /// <param name="request">The request DTO from the web layer</param>
    /// <param name="templateId">The workout template ID from the route</param>
    /// <returns>A valid command object</returns>
    public static AddExerciseToTemplateCommand ToCommand(
        this AddExerciseToTemplateDto request,
        WorkoutTemplateId templateId)
    {
        return new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = templateId,
            ExerciseId = ExerciseId.ParseOrEmpty(request?.ExerciseId),
            Zone = request?.Zone ?? string.Empty,
            Notes = request?.Notes,
            SequenceOrder = request?.SequenceOrder,
            UserId = UserId.Empty // TODO: Get from authentication context
        };
    }
}