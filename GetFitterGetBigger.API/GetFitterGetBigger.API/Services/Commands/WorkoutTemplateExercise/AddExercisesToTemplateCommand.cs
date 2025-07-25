using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercise;

/// <summary>
/// Command to add multiple exercises to a workout template in bulk
/// </summary>
public class AddExercisesToTemplateCommand
{
    /// <summary>
    /// The workout template ID to add exercises to
    /// </summary>
    public WorkoutTemplateId WorkoutTemplateId { get; set; }
    
    /// <summary>
    /// The exercises to add
    /// </summary>
    public List<ExerciseToAdd> Exercises { get; set; } = new();
}

/// <summary>
/// Represents an exercise to be added to a template
/// </summary>
public class ExerciseToAdd
{
    /// <summary>
    /// The exercise ID
    /// </summary>
    public ExerciseId ExerciseId { get; set; }
    
    /// <summary>
    /// The zone for the exercise
    /// </summary>
    public Models.Entities.WorkoutZone Zone { get; set; }
    
    /// <summary>
    /// Optional sequence order (will auto-assign if not provided)
    /// </summary>
    public int? SequenceOrder { get; set; }
    
    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }
}