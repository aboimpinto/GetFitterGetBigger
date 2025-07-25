using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

/// <summary>
/// Command to reorder exercises within a zone
/// </summary>
public record ReorderTemplateExercisesCommand
{
    /// <summary>
    /// The workout template containing the exercises
    /// </summary>
    public WorkoutTemplateId WorkoutTemplateId { get; init; }

    /// <summary>
    /// The zone to reorder exercises in
    /// </summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>
    /// Ordered list of exercise IDs in their new sequence
    /// </summary>
    public List<WorkoutTemplateExerciseId> ExerciseIds { get; init; } = new();

    /// <summary>
    /// The user performing the action
    /// </summary>
    public UserId UserId { get; init; }
}