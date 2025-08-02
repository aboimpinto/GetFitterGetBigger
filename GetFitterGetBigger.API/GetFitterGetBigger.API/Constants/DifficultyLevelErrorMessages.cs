namespace GetFitterGetBigger.API.Constants
{
    /// <summary>
    /// Centralized error messages for DifficultyLevel-related operations
    /// </summary>
    public static class DifficultyLevelErrorMessages
    {
        // Validation errors
        public static string InvalidIdFormat => "Invalid difficulty level ID format";
        public static string IdCannotBeEmpty => "Difficulty level ID cannot be empty";
        public static string ValueCannotBeEmpty => "Difficulty level value cannot be empty";
        
        // Not found errors
        public static string NotFound => "Difficulty level not found";
        public static string NotFoundByValue => "Difficulty level not found";
        
        // General validation
        public static string InvalidDifficultyLevelId => "Invalid difficulty level ID";
        public static string DifficultyLevelRequired => "Difficulty level is required";
        
        // Entity validation messages (reference common messages)
        public static string ValueCannotBeEmptyEntity => ReferenceDataErrorMessages.ValueCannotBeEmpty;
        public static string DisplayOrderMustBeNonNegative => ReferenceDataErrorMessages.DisplayOrderMustBeNonNegative;
    }
}