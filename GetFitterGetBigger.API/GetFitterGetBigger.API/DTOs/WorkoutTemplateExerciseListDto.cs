using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// DTO for workout template exercises grouped by zone
/// </summary>
public record WorkoutTemplateExerciseListDto : IEmptyDto<WorkoutTemplateExerciseListDto>
{
    /// <summary>
    /// The workout template ID
    /// </summary>
    public string WorkoutTemplateId { get; init; } = string.Empty;

    /// <summary>
    /// Warmup zone exercises
    /// </summary>
    public List<WorkoutTemplateExerciseDto> WarmupExercises { get; init; } = new();

    /// <summary>
    /// Main zone exercises
    /// </summary>
    public List<WorkoutTemplateExerciseDto> MainExercises { get; init; } = new();

    /// <summary>
    /// Cooldown zone exercises
    /// </summary>
    public List<WorkoutTemplateExerciseDto> CooldownExercises { get; init; } = new();

    /// <summary>
    /// Total number of exercises
    /// </summary>
    public int TotalExercises => WarmupExercises.Count + MainExercises.Count + CooldownExercises.Count;

    /// <summary>
    /// Total estimated duration in minutes
    /// </summary>
    public int TotalEstimatedDurationMinutes { get; init; }

    /// <summary>
    /// All exercises in a single flat list
    /// </summary>
    public List<WorkoutTemplateExerciseDto> Exercises => 
        WarmupExercises.Concat(MainExercises).Concat(CooldownExercises).ToList();

    /// <summary>
    /// Indicates if this is an empty instance
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(WorkoutTemplateId);

    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static WorkoutTemplateExerciseListDto Empty => new();
}