namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// Enum representing the context types for multi-type exercises
/// </summary>
public enum ExerciseContextType
{
    /// <summary>
    /// Exercise used as a workout exercise (primary context)
    /// Shows: warmups, cooldowns, alternative workouts
    /// </summary>
    Workout,

    /// <summary>
    /// Exercise used as a warmup exercise (secondary context)
    /// Shows: workouts using this warmup (read-only), alternative warmups (editable)
    /// </summary>
    Warmup,

    /// <summary>
    /// Exercise used as a cooldown exercise (secondary context)
    /// Shows: workouts using this cooldown (read-only), alternative cooldowns (editable)
    /// </summary>
    Cooldown
}