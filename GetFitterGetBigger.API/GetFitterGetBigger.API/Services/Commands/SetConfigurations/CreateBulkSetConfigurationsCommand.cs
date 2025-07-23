using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.SetConfigurations;

/// <summary>
/// Command for creating multiple set configurations in bulk
/// </summary>
public record CreateBulkSetConfigurationsCommand
{
    /// <summary>
    /// The workout template exercise these sets belong to
    /// </summary>
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }
    
    /// <summary>
    /// List of set configurations to create
    /// </summary>
    public IEnumerable<SetConfigurationData> SetConfigurations { get; init; } = new List<SetConfigurationData>();
    
    /// <summary>
    /// User performing the action
    /// </summary>
    public UserId UserId { get; init; }
}

/// <summary>
/// Data for a single set configuration in bulk operations
/// </summary>
public record SetConfigurationData
{
    /// <summary>
    /// The set number
    /// </summary>
    public int SetNumber { get; init; }
    
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
}