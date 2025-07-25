using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.SetConfigurations;

/// <summary>
/// Command for creating a new set configuration
/// </summary>
public record CreateSetConfigurationCommand
{
    /// <summary>
    /// The workout template exercise this set belongs to
    /// </summary>
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }
    
    /// <summary>
    /// The set number (optional - will auto-assign if not provided)
    /// </summary>
    public int? SetNumber { get; init; }
    
    /// <summary>
    /// Target reps (can be a range like "8-12")
    /// </summary>
    public string? TargetReps { get; init; }
    
    /// <summary>
    /// Target weight in kilograms
    /// </summary>
    public decimal? TargetWeight { get; init; }
    
    /// <summary>
    /// Target time in seconds
    /// </summary>
    public int? TargetTimeSeconds { get; init; }
    
    /// <summary>
    /// Rest time in seconds after this set
    /// </summary>
    public int RestSeconds { get; init; } = 90;
    
    /// <summary>
    /// User performing the action
    /// </summary>
    public UserId UserId { get; init; }
}