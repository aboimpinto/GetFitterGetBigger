using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

/// <summary>
/// Command to add an exercise to a workout template
/// </summary>
public record AddExerciseToTemplateCommand
{
    /// <summary>
    /// The workout template to add the exercise to
    /// </summary>
    public WorkoutTemplateId WorkoutTemplateId { get; init; }

    /// <summary>
    /// The exercise to add
    /// </summary>
    public ExerciseId ExerciseId { get; init; }

    /// <summary>
    /// The zone to add the exercise to (Warmup, Main, Cooldown)
    /// </summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>
    /// Optional notes for the exercise
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// The user performing the action
    /// </summary>
    public UserId UserId { get; init; }

    /// <summary>
    /// Optional sequence order. If not provided, will be added at the end
    /// </summary>
    public int? SequenceOrder { get; init; }
}