using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;

/// <summary>
/// Request model for updating exercise metadata
/// </summary>
public record UpdateExerciseMetadataRequest
{
    /// <summary>
    /// Updated JSON metadata containing ExecutionProtocol-specific configuration
    /// </summary>
    /// <example>{"reps": 12, "weight": {"value": 65, "unit": "kg"}}</example>
    [Required]
    public required JsonDocument Metadata { get; init; }
}