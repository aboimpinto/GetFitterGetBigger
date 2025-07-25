using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.SetConfigurations;

/// <summary>
/// Command for updating multiple set configurations in bulk
/// </summary>
public record UpdateBulkSetConfigurationsCommand
{
    /// <summary>
    /// The workout template exercise these sets belong to
    /// </summary>
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }
    
    /// <summary>
    /// List of set configuration updates
    /// </summary>
    public IEnumerable<SetConfigurationUpdateData> SetConfigurationUpdates { get; init; } = new List<SetConfigurationUpdateData>();
    
    /// <summary>
    /// User performing the action
    /// </summary>
    public UserId UserId { get; init; }
}

/// <summary>
/// Data for updating a single set configuration in bulk operations
/// </summary>
public record SetConfigurationUpdateData
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
}