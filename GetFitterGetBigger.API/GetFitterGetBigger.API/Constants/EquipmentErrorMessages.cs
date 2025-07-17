namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized error messages for Equipment-related operations
/// </summary>
public static class EquipmentErrorMessages
{
    // General validation
    public static string IdCannotBeEmpty => "ID cannot be empty";
    public static string InvalidIdFormat => "Invalid ID format. Expected format: 'equipment-{guid}', got: '{0}'";
    public static string NameCannotBeEmpty => "Name cannot be empty";
    public static string RequestCannotBeNull => "Request cannot be null";
    
    // Entity operations
    public static string NotFound => "Equipment not found";
    public static string FailedToCreate => "Failed to create Equipment";
    public static string FailedToUpdate => "Failed to update Equipment";
    public static string FailedToDelete => "Failed to delete Equipment";
    public static string FailedToLoad => "Failed to load Equipment";
    
    // Duplicate and dependency errors
    public static string DuplicateNameFormat => "Equipment with the name '{0}' already exists";
    public static string CannotDeleteInUse => "Cannot delete equipment that is in use by exercises";
    
    // Service method specific
    public static string InvalidEquipmentId => "Invalid equipment ID";
}