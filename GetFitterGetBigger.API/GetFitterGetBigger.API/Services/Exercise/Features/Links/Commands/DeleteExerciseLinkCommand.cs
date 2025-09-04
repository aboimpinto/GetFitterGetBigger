using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for deleting an exercise link
/// </summary>
public record DeleteExerciseLinkCommand
{
    /// <summary>
    /// The source exercise ID
    /// </summary>
    public ExerciseId ExerciseId { get; init; } = ExerciseId.Empty;
    
    /// <summary>
    /// The link ID to delete
    /// </summary>
    public ExerciseLinkId LinkId { get; init; } = ExerciseLinkId.Empty;
    
    /// <summary>
    /// Whether to delete the reverse bidirectional link
    /// </summary>
    public bool DeleteReverse { get; init; } = true;
}