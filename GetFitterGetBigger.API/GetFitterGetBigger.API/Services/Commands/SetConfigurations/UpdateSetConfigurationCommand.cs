using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.SetConfigurations;

/// <summary>
/// Command for updating an existing set configuration
/// </summary>
public record UpdateSetConfigurationCommand
{
    /// <summary>
    /// The set configuration ID to update
    /// </summary>
    public SetConfigurationId SetConfigurationId { get; init; }
    
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
    public int RestSeconds { get; init; }
    
    /// <summary>
    /// User performing the action
    /// </summary>
    public UserId UserId { get; init; }
}