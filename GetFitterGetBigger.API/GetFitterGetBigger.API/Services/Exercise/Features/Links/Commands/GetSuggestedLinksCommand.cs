using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for getting suggested exercise links
/// </summary>
public record GetSuggestedLinksCommand
{
    /// <summary>
    /// The exercise ID to get suggestions for
    /// </summary>
    public ExerciseId ExerciseId { get; init; } = ExerciseId.Empty;
    
    /// <summary>
    /// Number of suggestions to return
    /// </summary>
    public int Count { get; init; } = 5;
}