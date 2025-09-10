using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;

/// <summary>
/// Request model for adding an exercise to a workout template with phase/round support
/// </summary>
public record AddExerciseToTemplateRequest
{
    /// <summary>
    /// The exercise ID to add to the template
    /// </summary>
    /// <example>exercise-550e8400-e29b-41d4-a716-446655440000</example>
    [Required]
    public required string ExerciseId { get; init; }

    /// <summary>
    /// The phase this exercise belongs to (Warmup, Workout, Cooldown)
    /// </summary>
    /// <example>Workout</example>
    [Required]
    public required string Phase { get; init; }

    /// <summary>
    /// The round number within the phase
    /// </summary>
    /// <example>1</example>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Round number must be positive")]
    public int RoundNumber { get; init; }

    /// <summary>
    /// JSON metadata containing ExecutionProtocol-specific configuration
    /// </summary>
    /// <example>{"reps": 10, "weight": {"value": 60, "unit": "kg"}}</example>
    [Required]
    public required JsonDocument Metadata { get; init; }
}