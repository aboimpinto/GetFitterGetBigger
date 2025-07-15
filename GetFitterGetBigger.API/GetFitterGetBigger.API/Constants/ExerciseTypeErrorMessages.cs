namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to ExerciseType operations
/// </summary>
public static class ExerciseTypeErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid exercise type ID format";
    public const string IdCannotBeEmpty = "ID cannot be empty";
    public const string ValueCannotBeEmpty = "Exercise type value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Exercise type value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Exercise type";
}