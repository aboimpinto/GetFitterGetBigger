using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercise;

/// <summary>
/// Command to add a single exercise to a workout template
/// </summary>
public class AddExerciseToTemplateCommand
{
    /// <summary>
    /// The workout template ID to add the exercise to
    /// </summary>
    public WorkoutTemplateId WorkoutTemplateId { get; set; }
    
    /// <summary>
    /// The exercise ID to add
    /// </summary>
    public ExerciseId ExerciseId { get; set; }
    
    /// <summary>
    /// The zone where the exercise will be placed
    /// </summary>
    public WorkoutZone Zone { get; set; }
    
    /// <summary>
    /// The sequence order within the zone (optional - will auto-assign if not provided)
    /// </summary>
    public int? SequenceOrder { get; set; }
    
    /// <summary>
    /// Optional notes for the exercise
    /// </summary>
    public string? Notes { get; set; }
}