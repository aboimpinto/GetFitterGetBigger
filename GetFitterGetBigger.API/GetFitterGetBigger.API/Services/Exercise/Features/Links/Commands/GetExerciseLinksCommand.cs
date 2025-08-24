using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for getting exercise links
/// </summary>
public record GetExerciseLinksCommand
{
    /// <summary>
    /// The ID of the exercise to get links for
    /// </summary>
    public ExerciseId ExerciseId { get; init; } = ExerciseId.Empty;
    
    /// <summary>
    /// Optional filter by link type (Warmup or Cooldown)
    /// </summary>
    public string? LinkType { get; init; }
    
    /// <summary>
    /// Whether to include full exercise details for linked exercises
    /// </summary>
    public bool IncludeExerciseDetails { get; init; }
}