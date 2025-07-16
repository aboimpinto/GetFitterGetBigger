namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to WorkoutCategory operations
/// </summary>
public static class WorkoutCategoryErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid workout category ID format";
    public const string IdCannotBeEmpty = "WorkoutCategory ID cannot be empty";
    public const string ValueCannotBeEmpty = "Value cannot be empty";
    public const string ValueExceedsMaxLength = "Value cannot exceed 100 characters";
    public const string IconIsRequired = "Icon is required";
    public const string IconExceedsMaxLength = "Icon cannot exceed 50 characters";
    public const string ColorIsRequired = "Color is required";
    public const string InvalidHexColorCode = "Color must be a valid hex color code";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Workout category not found";
}