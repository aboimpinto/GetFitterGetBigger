namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for creating a set configuration
/// </summary>
public class CreateSetConfigurationDto
{
    /// <summary>
    /// The set number (optional - will auto-assign if not provided)
    /// <example>1</example>
    /// </summary>
    public int? SetNumber { get; init; }

    /// <summary>
    /// Target reps (can be a range like "8-12")
    /// <example>8-12</example>
    /// </summary>
    public string? TargetReps { get; init; }

    /// <summary>
    /// Target weight in kilograms
    /// <example>80.5</example>
    /// </summary>
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// Target time in seconds
    /// <example>30</example>
    /// </summary>
    public int? TargetTimeSeconds { get; init; }

    /// <summary>
    /// Rest time in seconds after this set
    /// <example>90</example>
    /// </summary>
    public int RestSeconds { get; init; } = 90;
}