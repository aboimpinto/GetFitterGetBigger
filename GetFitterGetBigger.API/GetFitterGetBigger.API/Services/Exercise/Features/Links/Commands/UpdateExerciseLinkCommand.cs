namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for updating an existing exercise link
/// </summary>
public record UpdateExerciseLinkCommand
{
    /// <summary>
    /// The ID of the exercise that owns the link
    /// </summary>
    public string ExerciseId { get; init; } = string.Empty;
    
    /// <summary>
    /// The ID of the link to update
    /// </summary>
    public string LinkId { get; init; } = string.Empty;
    
    /// <summary>
    /// The display order for this link
    /// </summary>
    public int DisplayOrder { get; init; }
    
    /// <summary>
    /// Whether the link is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}