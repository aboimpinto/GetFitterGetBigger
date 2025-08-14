namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized error messages for Equipment-related operations
/// </summary>
public static class EquipmentErrorMessages
{
    /// <summary>
    /// Validation error messages
    /// </summary>
    public static class Validation
    {
        public static string IdCannotBeEmpty => "ID cannot be empty";
        public static string InvalidIdFormat => "Invalid ID format. Expected format: 'equipment-{guid}', got: '{0}'";
        public static string NameCannotBeEmpty => "Name cannot be empty";
        public static string RequestCannotBeNull => "Request cannot be null";
        public static string InvalidEquipmentId => "Invalid equipment ID";
    }
    
    /// <summary>
    /// Entity operation error messages
    /// </summary>
    public static class Operations
    {
        public static string NotFound => "Equipment not found";
        public static string FailedToCreate => "Failed to create Equipment";
        public static string FailedToUpdate => "Failed to update Equipment";
        public static string FailedToDelete => "Failed to delete Equipment";
        public static string FailedToLoad => "Failed to load Equipment";
    }
    
    /// <summary>
    /// Business rule violation error messages
    /// </summary>
    public static class BusinessRules
    {
        public static string DuplicateNameFormat => "Equipment with the name '{0}' already exists";
        public static string CannotDeleteInUse => "Cannot delete equipment that is in use by exercises";
    }
    
    // Service-specific constants needed for validation patterns
    public const string ValueCannotBeEmpty = "Equipment value cannot be empty";
    public const string CommandCannotBeNull = "Equipment command cannot be null";
    public const string NameMustBeUnique = "Equipment name must be unique";
    public const string CannotDeleteReferenced = "Cannot delete equipment that is referenced by other entities";
}