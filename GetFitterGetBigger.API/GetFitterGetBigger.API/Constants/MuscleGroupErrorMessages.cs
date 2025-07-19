namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized error messages for MuscleGroup-related operations
/// </summary>
public static class MuscleGroupErrorMessages
{
    /// <summary>
    /// Validation error messages
    /// </summary>
    public static class Validation
    {
        public static string IdCannotBeEmpty => "ID cannot be empty";
        public static string InvalidIdFormat => "Invalid ID format. Expected format: 'musclegroup-{guid}', got: '{0}'";
        public static string NameCannotBeEmpty => "Name cannot be empty";
        public static string NameTooLong => "Name must not exceed 100 characters";
        public static string RequestCannotBeNull => "Request cannot be null";
        public static string InvalidMuscleGroupId => "Invalid muscle group ID";
        public static string BodyPartIdRequired => "Body part ID is required";
        public static string InvalidBodyPartId => "Body part does not exist";
    }
    
    /// <summary>
    /// Entity operation error messages
    /// </summary>
    public static class Operations
    {
        public static string NotFound => "Muscle group not found";
        public static string FailedToCreate => "Failed to create muscle group";
        public static string FailedToUpdate => "Failed to update muscle group";
        public static string FailedToDelete => "Failed to delete muscle group";
        public static string FailedToLoad => "Failed to load muscle group";
    }
    
    /// <summary>
    /// Business rule violation error messages
    /// </summary>
    public static class BusinessRules
    {
        public static string DuplicateNameFormat => "A muscle group with the name '{0}' already exists";
        public static string CannotDeleteInUse => "Cannot deactivate muscle group as it is being used by active exercises";
    }
}