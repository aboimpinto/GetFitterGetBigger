namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages for workout category operations
/// </summary>
public static class WorkoutCategoryErrorMessages
{
    public const string InvalidIdFormat = "Invalid workout category ID format";
    public const string IdCannotBeEmpty = "Workout category ID cannot be empty";
    public const string ValueCannotBeEmpty = "Workout category value cannot be empty";
    public const string NotFound = "Workout category not found";
    public const string ValueExceedsMaxLength = "Workout category value cannot exceed 100 characters";
    public const string IconIsRequired = "Workout category icon is required";
    public const string IconExceedsMaxLength = "Workout category icon cannot exceed 50 characters";
    public const string ColorIsRequired = "Workout category color is required";
    public const string InvalidHexColorCode = "Color must be a valid hex color code (e.g., #FF5722)";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
}