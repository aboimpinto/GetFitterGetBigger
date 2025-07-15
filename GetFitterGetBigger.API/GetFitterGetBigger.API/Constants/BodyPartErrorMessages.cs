namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to BodyPart operations
/// </summary>
public static class BodyPartErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid body part ID format";
    public const string IdCannotBeEmpty = "ID cannot be empty";
    public const string ValueCannotBeEmpty = "Body part value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Body part value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Body part";
}