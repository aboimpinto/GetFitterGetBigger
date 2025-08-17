namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to MovementPattern operations
/// </summary>
public static class MovementPatternErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid movement pattern ID format";
    public const string ValueCannotBeEmpty = "Movement pattern value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Movement pattern value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Movement pattern";
}