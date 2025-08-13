namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages for KineticChainType entity validation and operations
/// </summary>
public static class KineticChainTypeErrorMessages
{
    // Entity validation messages
    public const string ValueCannotBeEmptyEntity = "Kinetic chain type value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Service error messages
    public const string ValueCannotBeEmpty = "Kinetic chain type value cannot be empty";
    public const string InvalidIdFormat = "Invalid kinetic chain type ID format. Expected format: 'kineticchaintype-{guid}'";
    public const string IdCannotBeEmpty = "Kinetic chain type ID cannot be empty";
    public const string NotFound = "Kinetic chain type not found";
    
    // Controller error messages
    public const string GetByIdError = "Error retrieving kinetic chain type with ID: {0}";
    public const string GetAllError = "Error retrieving kinetic chain types";
}