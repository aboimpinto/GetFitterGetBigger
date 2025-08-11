namespace GetFitterGetBigger.API.Constants
{
    /// <summary>
    /// Centralized error messages for Exercise-related operations
    /// </summary>
    public static class ExerciseErrorMessages
    {
        // Duplicate name errors
        public static string DuplicateNameFormat => "Exercise with name '{0}' already exists.";
        
        // REST exercise validation
        public static string RestExerciseCannotHaveKineticChain => "Kinetic chain type must not be specified for REST exercises.";
        public static string RestExerciseCannotHaveWeightType => "REST exercises cannot have a weight type.";
        public static string RestExerciseCannotBeCombined => "REST exercises cannot be combined with other exercise types.";
        
        // Non-REST exercise validation
        public static string NonRestExerciseMustHaveKineticChain => "Non-REST exercises must have a valid kinetic chain.";
        public static string NonRestExerciseMustHaveWeightType => "Non-REST exercises must have a valid weight type.";
        public static string NonRestExerciseMustHaveMuscleGroups => "Non-REST exercises must have at least one muscle group.";
        
        // Update-specific messages
        public static string NonRestExerciseMustHaveKineticChainUpdate => "Non-REST exercises must have a kinetic chain.";
        
        // General validation
        public static string InvalidIdFormat => "Invalid Exercise ID format. Expected format: exercise-{guid}";
        public static string InvalidExerciseId => "Invalid exercise ID";
        public static string ExerciseNotFound => "Exercise not found";
        public static string DifficultyLevelRequired => "Difficulty level is required.";
        public static string ExerciseNameRequired => "Exercise name is required.";
        public static string ExerciseNameMaxLength => "Exercise name cannot exceed 255 characters.";
        public static string ExerciseDescriptionRequired => "Exercise description is required.";
        public static string AtLeastOneExerciseTypeRequired => "At least one exercise type must be specified.";
        public static string InvalidExerciseTypeIds => "Invalid exercise type IDs provided.";
        
        // Muscle group validation
        public static string InvalidMuscleGroupFormat => "Invalid muscle group: ID={0}, Role={1}";
        public static string MuscleGroupRequired => "Muscle groups are required for non-REST exercises.";
        public static string InvalidMuscleGroupId => "Invalid muscle group ID found.";
        public static string InvalidMuscleRoleId => "Invalid muscle role ID found.";
        
        // Exercise type validation
        public static string InvalidExerciseTypeConfiguration => "Invalid exercise type configuration.";
        public static string SomeExerciseTypesDoNotExist => "Some exercise types do not exist.";
    }
}