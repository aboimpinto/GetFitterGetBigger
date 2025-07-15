namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Error messages specific to ExerciseWeightType operations
/// </summary>
public static class ExerciseWeightTypeErrorMessages
{
    // Validation errors
    public const string InvalidIdFormat = "Invalid exercise weight type ID format";
    public const string IdCannotBeEmpty = "ID cannot be empty";
    public const string CodeCannotBeEmpty = "Exercise weight type code cannot be empty";
    public const string ValueCannotBeEmpty = "Exercise weight type value cannot be empty";
    public const string ValueCannotBeEmptyEntity = "Exercise weight type value cannot be empty";
    public const string DisplayOrderMustBeNonNegative = "Display order must be non-negative";
    
    // Not found errors
    public const string NotFound = "Exercise weight type";
}