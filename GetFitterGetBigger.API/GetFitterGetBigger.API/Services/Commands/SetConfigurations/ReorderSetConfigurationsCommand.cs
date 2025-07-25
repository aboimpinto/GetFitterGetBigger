using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.SetConfigurations;

/// <summary>
/// Command for reordering set configurations
/// </summary>
public record ReorderSetConfigurationsCommand
{
    /// <summary>
    /// The workout template exercise these sets belong to
    /// </summary>
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; init; }
    
    /// <summary>
    /// Dictionary mapping set configuration IDs to their new set numbers
    /// </summary>
    public Dictionary<SetConfigurationId, int> SetReorders { get; init; } = new Dictionary<SetConfigurationId, int>();
    
    /// <summary>
    /// User performing the action
    /// </summary>
    public UserId UserId { get; init; }
}