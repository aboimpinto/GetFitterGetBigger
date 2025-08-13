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
    
    // Backward compatibility properties
    [Obsolete("Use Validation.IdCannotBeEmpty instead")]
    public static string IdCannotBeEmpty => Validation.IdCannotBeEmpty;
    
    [Obsolete("Use Validation.InvalidIdFormat instead")]
    public static string InvalidIdFormat => Validation.InvalidIdFormat;
    
    [Obsolete("Use Validation.NameCannotBeEmpty instead")]
    public static string NameCannotBeEmpty => Validation.NameCannotBeEmpty;
    
    [Obsolete("Use Validation.RequestCannotBeNull instead")]
    public static string RequestCannotBeNull => Validation.RequestCannotBeNull;
    
    [Obsolete("Use Operations.NotFound instead")]
    public static string NotFound => Operations.NotFound;
    
    [Obsolete("Use Operations.FailedToCreate instead")]
    public static string FailedToCreate => Operations.FailedToCreate;
    
    [Obsolete("Use Operations.FailedToUpdate instead")]
    public static string FailedToUpdate => Operations.FailedToUpdate;
    
    [Obsolete("Use Operations.FailedToDelete instead")]
    public static string FailedToDelete => Operations.FailedToDelete;
    
    [Obsolete("Use Operations.FailedToLoad instead")]
    public static string FailedToLoad => Operations.FailedToLoad;
    
    [Obsolete("Use BusinessRules.DuplicateNameFormat instead")]
    public static string DuplicateNameFormat => BusinessRules.DuplicateNameFormat;
    
    [Obsolete("Use BusinessRules.CannotDeleteInUse instead")]
    public static string CannotDeleteInUse => BusinessRules.CannotDeleteInUse;
    
    [Obsolete("Use Validation.InvalidEquipmentId instead")]
    public static string InvalidEquipmentId => Validation.InvalidEquipmentId;
    
    // Service-specific constants needed for validation patterns
    public const string ValueCannotBeEmpty = "Equipment value cannot be empty";
    public const string CommandCannotBeNull = "Equipment command cannot be null";
    public const string NameMustBeUnique = "Equipment name must be unique";
    public const string CannotDeleteReferenced = "Cannot delete equipment that is referenced by other entities";
}