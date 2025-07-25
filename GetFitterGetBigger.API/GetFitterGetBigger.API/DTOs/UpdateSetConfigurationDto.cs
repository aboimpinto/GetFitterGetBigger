namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for updating a set configuration
/// </summary>
public class UpdateSetConfigurationDto
{
    /// <summary>
    /// Target reps (can be a range like "8-12")
    /// <example>10-15</example>
    /// </summary>
    public string? TargetReps { get; init; }

    /// <summary>
    /// Target weight in kilograms
    /// <example>85.0</example>
    /// </summary>
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// Target time in seconds
    /// <example>45</example>
    /// </summary>
    public int? TargetTimeSeconds { get; init; }

    /// <summary>
    /// Rest time in seconds after this set
    /// <example>120</example>
    /// </summary>
    public int? RestSeconds { get; init; }
}