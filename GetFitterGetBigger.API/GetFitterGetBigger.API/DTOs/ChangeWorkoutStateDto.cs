namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for changing workout template state
/// </summary>
public class ChangeWorkoutStateDto
{
    /// <summary>
    /// The new workout state ID
    /// <example>workoutstate-02000001-0000-0000-0000-000000000002</example>
    /// </summary>
    public required string WorkoutStateId { get; init; }
}