namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for exercise link responses
/// </summary>
public class ExerciseLinkDto
{
    /// <summary>
    /// The ID of the exercise link in the format "exerciselink-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the target exercise in the format "exercise-{guid}"
    /// </summary>
    public string TargetExerciseId { get; set; } = string.Empty;
    
    /// <summary>
    /// The target exercise details (only populated if includeExerciseDetails is true)
    /// </summary>
    public ExerciseDto? TargetExercise { get; set; }
    
    /// <summary>
    /// The type of link (Warmup or Cooldown)
    /// </summary>
    public string LinkType { get; set; } = string.Empty;
    
    /// <summary>
    /// The display order for this link
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Whether the link is active
    /// </summary>
    public bool IsActive { get; set; }
}