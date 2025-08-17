namespace GetFitterGetBigger.API.Constants
{
    /// <summary>
    /// Centralized error messages for DifficultyLevel-related operations
    /// </summary>
    public static class DifficultyLevelErrorMessages
    {
        // Validation errors
        public static string InvalidIdFormat => "Invalid difficulty level ID format";
        public static string ValueCannotBeEmpty => "Difficulty level value cannot be empty";
        
        // Entity validation messages (reference common messages)
        public static string ValueCannotBeEmptyEntity => ReferenceDataErrorMessages.ValueCannotBeEmpty;
        public static string DisplayOrderMustBeNonNegative => ReferenceDataErrorMessages.DisplayOrderMustBeNonNegative;
    }
}