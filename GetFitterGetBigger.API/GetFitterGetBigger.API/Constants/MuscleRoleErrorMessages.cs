namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages for muscle role operations
/// </summary>
public static class MuscleRoleErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid ID format. Expected format: 'musclerole-{guid}'";
    public const string ValueCannotBeEmpty = "Value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Muscle role value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
}