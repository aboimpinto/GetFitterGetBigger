using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized constants for WorkoutState values and IDs.
/// These values must match the seeded data in the database migrations.
/// </summary>
public static class WorkoutStateConstants
{
    // String value constants
    
    /// <summary>
    /// Draft state value - template under construction
    /// </summary>
    public const string Draft = "Draft";
    
    /// <summary>
    /// Production state value - active template for use
    /// </summary>
    public const string Production = "Production";
    
    /// <summary>
    /// Archived state value - retired template
    /// </summary>
    public const string Archived = "Archived";
    
    // ID constants for database references
    
    /// <summary>
    /// DRAFT ID - Template under construction (02000001-0000-0000-0000-000000000001).
    /// Used for new templates and duplicates.
    /// </summary>
    public static readonly WorkoutStateId DraftId = 
        WorkoutStateId.ParseOrEmpty("workoutstate-02000001-0000-0000-0000-000000000001");
    
    /// <summary>
    /// PRODUCTION ID - Active template for use (02000001-0000-0000-0000-000000000002).
    /// Templates in active use by clients.
    /// </summary>
    public static readonly WorkoutStateId ProductionId = 
        WorkoutStateId.ParseOrEmpty("workoutstate-02000001-0000-0000-0000-000000000002");
    
    /// <summary>
    /// ARCHIVED ID - Retired template (02000001-0000-0000-0000-000000000003).
    /// Used for soft-deleted templates.
    /// </summary>
    public static readonly WorkoutStateId ArchivedId = 
        WorkoutStateId.ParseOrEmpty("workoutstate-02000001-0000-0000-0000-000000000003");
}