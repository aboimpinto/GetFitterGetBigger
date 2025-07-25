using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

/// <summary>
/// Command to change the zone of an exercise
/// </summary>
public record ChangeExerciseZoneCommand
{
    /// <summary>
    /// The workout template exercise to move
    /// </summary>
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }

    /// <summary>
    /// The new zone for the exercise
    /// </summary>
    public string NewZone { get; init; } = string.Empty;

    /// <summary>
    /// Optional new sequence order in the new zone
    /// </summary>
    public int? NewSequenceOrder { get; init; }

    /// <summary>
    /// The user performing the action
    /// </summary>
    public UserId UserId { get; init; }
}