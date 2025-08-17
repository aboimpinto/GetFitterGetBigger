namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized error messages for MuscleGroup-related operations
/// </summary>
public static class MuscleGroupErrorMessages
{
    /// <summary>
    /// Business rule violation error messages
    /// </summary>
    public static class BusinessRules
    {
        public static string CannotDeleteInUse => "Cannot deactivate muscle group as it is being used by active exercises";
    }
    
    // Service-specific constants needed for validation patterns
    public const string InvalidIdFormat = "Invalid muscle group ID format. Expected format: 'musclegroup-{guid}'";
    public const string ValueCannotBeEmpty = "Muscle group value cannot be empty";
    public const string NameCannotBeEmpty = "Muscle group name cannot be empty";
    public const string BodyPartIdCannotBeEmpty = "Body part ID cannot be empty";
}