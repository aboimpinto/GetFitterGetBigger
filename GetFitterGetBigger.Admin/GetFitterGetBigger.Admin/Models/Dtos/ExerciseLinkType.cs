namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// Enum representing the types of exercise links
/// </summary>
public enum ExerciseLinkType
{
    /// <summary>
    /// Warmup exercise link
    /// </summary>
    Warmup,

    /// <summary>
    /// Cooldown exercise link
    /// </summary>
    Cooldown,

    /// <summary>
    /// Alternative exercise link (bidirectional relationship)
    /// </summary>
    Alternative
}