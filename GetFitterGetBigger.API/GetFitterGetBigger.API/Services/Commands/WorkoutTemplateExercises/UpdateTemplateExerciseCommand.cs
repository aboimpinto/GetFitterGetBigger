using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

/// <summary>
/// Command to update an exercise in a workout template
/// </summary>
public record UpdateTemplateExerciseCommand
{
    /// <summary>
    /// The workout template exercise to update
    /// </summary>
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }

    /// <summary>
    /// Updated notes for the exercise
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// The user performing the action
    /// </summary>
    public UserId UserId { get; init; }
}