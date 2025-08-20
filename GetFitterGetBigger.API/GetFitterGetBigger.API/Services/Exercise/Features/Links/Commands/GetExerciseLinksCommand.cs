namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;

/// <summary>
/// Command for getting exercise links
/// </summary>
public record GetExerciseLinksCommand
{
    /// <summary>
    /// The ID of the exercise to get links for
    /// </summary>
    public string ExerciseId { get; init; } = string.Empty;
    
    /// <summary>
    /// Optional filter by link type (Warmup or Cooldown)
    /// </summary>
    public string? LinkType { get; init; }
    
    /// <summary>
    /// Whether to include full exercise details for linked exercises
    /// </summary>
    public bool IncludeExerciseDetails { get; init; }
}