using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;

/// <summary>
/// Command to duplicate exercises from one template to another
/// </summary>
public record DuplicateTemplateExercisesCommand
{
    /// <summary>
    /// The source workout template to copy exercises from
    /// </summary>
    public WorkoutTemplateId SourceTemplateId { get; init; }

    /// <summary>
    /// The target workout template to copy exercises to
    /// </summary>
    public WorkoutTemplateId TargetTemplateId { get; init; }

    /// <summary>
    /// Whether to include set configurations
    /// </summary>
    public bool IncludeSetConfigurations { get; init; } = true;

    /// <summary>
    /// The user performing the action
    /// </summary>
    public UserId UserId { get; init; }
}