using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercise;

/// <summary>
/// Command to reorder exercises within a zone of a workout template
/// </summary>
public class ReorderExercisesCommand
{
    /// <summary>
    /// The workout template ID
    /// </summary>
    public WorkoutTemplateId WorkoutTemplateId { get; set; }
    
    /// <summary>
    /// The zone to reorder exercises in
    /// </summary>
    public WorkoutZone Zone { get; set; }
    
    /// <summary>
    /// Dictionary mapping exercise IDs to their new sequence orders
    /// </summary>
    public Dictionary<WorkoutTemplateExerciseId, int> ExerciseOrders { get; set; } = new();
}