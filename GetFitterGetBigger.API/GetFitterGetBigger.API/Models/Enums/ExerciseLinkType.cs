namespace GetFitterGetBigger.API.Models.Enums;

/// <summary>
/// Defines the types of links that can exist between exercises in the four-way linking system.
/// Each enum value has an explicit integer for stable PostgreSQL storage and migration compatibility.
/// </summary>
public enum ExerciseLinkType
{
    /// <summary>
    /// Warmup exercise link - migrated from existing "Warmup" string value.
    /// Links TO warmup exercises FROM workout exercises.
    /// When created, automatically generates a reverse WORKOUT link from target to source.
    /// </summary>
    WARMUP = 0,

    /// <summary>
    /// Cooldown exercise link - migrated from existing "Cooldown" string value.
    /// Links TO cooldown exercises FROM workout exercises.
    /// When created, automatically generates a reverse WORKOUT link from target to source.
    /// </summary>
    COOLDOWN = 1,

    /// <summary>
    /// Main workout exercise link - new functionality for reverse linking.
    /// Links TO workout exercises FROM warmup/cooldown exercises.
    /// This type is only auto-created as reverse links, never manually created.
    /// </summary>
    WORKOUT = 2,

    /// <summary>
    /// Alternative exercise option - new functionality for bidirectional linking.
    /// Links TO alternative exercises FROM any non-REST exercise.
    /// When created, automatically generates a reverse ALTERNATIVE link from target to source.
    /// Both directions use the same ALTERNATIVE type.
    /// </summary>
    ALTERNATIVE = 3
}