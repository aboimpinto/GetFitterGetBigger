using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Enhanced;

/// <summary>
/// Enhanced DTO for workout template exercise with JSON metadata support
/// Used by the new phase/round-based API endpoints
/// </summary>
public record WorkoutTemplateExerciseEnhancedDto(
    string Id,
    string ExerciseId,
    string ExerciseName,
    string ExerciseType,
    string Phase,
    int RoundNumber,
    int OrderInRound,
    JsonDocument Metadata,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static WorkoutTemplateExerciseEnhancedDto Empty => new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        JsonDocument.Parse("{}"),
        DateTime.MinValue,
        DateTime.MinValue);
}