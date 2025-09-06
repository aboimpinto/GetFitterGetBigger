namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// Represents a context for multi-type exercises (workout, warmup, cooldown)
/// </summary>
public class ExerciseContextDto
{
    /// <summary>
    /// The context type - "Workout", "Warmup", or "Cooldown"
    /// </summary>
    public string ContextType { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the context (e.g., "As Workout Exercise")
    /// </summary>
    public string ContextLabel { get; set; } = string.Empty;

    /// <summary>
    /// Whether this context is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Theme color for the context UI
    /// </summary>
    public string ThemeColor { get; set; } = string.Empty;

    /// <summary>
    /// Available relationship types in this context
    /// </summary>
    public IEnumerable<string> AvailableRelationships { get; set; } = new List<string>();
}