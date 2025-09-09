using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// DTO for all exercises in a workout template, organized by phases and rounds
/// </summary>
public record WorkoutTemplateExercisesDto(
    WorkoutTemplateId TemplateId,
    string TemplateName,
    ExecutionProtocolDto ExecutionProtocol,
    WorkoutPhaseDto Warmup,
    WorkoutPhaseDto Workout,
    WorkoutPhaseDto Cooldown)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static WorkoutTemplateExercisesDto Empty => new(
        WorkoutTemplateId.Empty,
        string.Empty,
        ExecutionProtocolDto.Empty,
        WorkoutPhaseDto.Empty,
        WorkoutPhaseDto.Empty,
        WorkoutPhaseDto.Empty);
}