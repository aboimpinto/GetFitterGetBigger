namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for creating a new exercise link
/// </summary>
public record CreateExerciseLinkCommand
{
    /// <summary>
    /// The ID of the source exercise
    /// </summary>
    public string SourceExerciseId { get; init; } = string.Empty;
    
    /// <summary>
    /// The ID of the target exercise to link
    /// </summary>
    public string TargetExerciseId { get; init; } = string.Empty;
    
    /// <summary>
    /// The type of link (Warmup or Cooldown)
    /// </summary>
    public string LinkType { get; init; } = string.Empty;
    
    /// <summary>
    /// The display order for this link
    /// </summary>
    public int DisplayOrder { get; init; }
}