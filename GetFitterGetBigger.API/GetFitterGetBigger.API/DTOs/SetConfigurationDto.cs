namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// DTO for set configuration
/// </summary>
public record SetConfigurationDto
{
    /// <summary>
    /// The set configuration ID
    /// </summary>
    public string Id { get; init; } = string.Empty;

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
    public int? TargetTime { get; init; }

    /// <summary>
    /// Rest time in seconds after this set
    /// </summary>
    public int RestSeconds { get; init; }

    /// <summary>
    /// Optional notes for this set
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}