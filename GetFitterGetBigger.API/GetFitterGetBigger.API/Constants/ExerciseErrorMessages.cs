namespace GetFitterGetBigger.API.Constants
{
    /// <summary>
    /// Centralized error messages for Exercise-related operations
    /// </summary>
    public static class ExerciseErrorMessages
    {
        // General validation
        public static string InvalidIdFormat => "Invalid Exercise ID format. Expected format: exercise-{guid}";
        public static string DifficultyLevelRequired => "Difficulty level is required.";
        public static string ExerciseNameRequired => "Exercise name is required.";
        public static string ExerciseNameMaxLength => "Exercise name cannot exceed 255 characters.";
        public static string ExerciseDescriptionRequired => "Exercise description is required.";
        
        // Exercise type validation
        public static string InvalidExerciseTypeConfiguration => "Invalid exercise type configuration.";
        
        // Combined validation error messages
        public static string InvalidKineticChainForExerciseType => "REST exercises cannot have kinetic chain; Non-REST exercises must have kinetic chain";
        public static string InvalidWeightTypeForExerciseType => "REST exercises cannot have weight type";
        public static string InvalidMuscleGroupsForExerciseType => "REST exercises cannot have muscle groups; Non-REST exercises must have at least one muscle group";
    }
}