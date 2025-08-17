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
        public static string InvalidIdFormat => "Invalid ID format. Expected format: 'equipment-{guid}', got: '{0}'";
        public static string NameCannotBeEmpty => "Name cannot be empty";
    }
    
    /// <summary>
    /// Business rule violation error messages
    /// </summary>
    public static class BusinessRules
    {
        public static string CannotDeleteInUse => "Cannot delete equipment that is in use by exercises";
    }
    
    // Service-specific constants needed for validation patterns
    public const string ValueCannotBeEmpty = "Equipment value cannot be empty";
    public const string CommandCannotBeNull = "Equipment command cannot be null";
}