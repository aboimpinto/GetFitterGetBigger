namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to WorkoutState operations
/// </summary>
public static class WorkoutStateErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid workout state ID format";
    public const string IdCannotBeEmpty = "ID cannot be empty";
    public const string ValueCannotBeEmpty = "Workout state value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Workout state value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Workout state";
}