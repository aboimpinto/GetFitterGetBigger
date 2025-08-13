namespace GetFitterGetBigger.API.Constants.ErrorMessages;

/// <summary>
/// Centralized error messages for WorkoutTemplate-related operations
/// </summary>
public static class WorkoutTemplateErrorMessages
{
    // ID Format Validation
    public static string InvalidIdFormat => "Invalid WorkoutTemplateId format. Expected format: 'workouttemplate-{guid}'";
    public static string InvalidStateIdFormat => "Invalid WorkoutStateId format. Expected format: 'workoutstate-{guid}' (e.g., 'workoutstate-02000001-0000-0000-0000-000000000002' for Production)";
    public static string InvalidCategoryIdFormat => "Invalid WorkoutCategoryId format. Expected format: 'workoutcategory-{guid}'";
    
    // Not Found Errors
    public static string NotFound => "Workout template not found";
    public static string OriginalNotFound => "Original workout template not found";
    
    // Validation Errors
    public static string NameRequired => "Template name is required";
    public static string MaxSuggestionsInvalid => "MaxSuggestions must be greater than 0";
    
    // Duplicate/Conflict Errors
    public static string DuplicateNameFormat => "Workout template with name '{0}' already exists";
    public static string NameAlreadyExists => "A workout template with this name already exists";
    
    // State Transition Errors
    public static string InvalidStateTransition => "Invalid state transition";
    public static string InvalidStateTransitionFormat => "Invalid state transition from {0} to {1}";
    public static string StateTransitionBlockedFormat => "State transition from {0} to {1} is blocked: {2}";
    
    // Exercise Related
    public static string ExercisesNotFound => "No exercises found for workout template";
    public static string NoSuggestedExercisesFound => "No suggested exercises found for the given criteria";
    
    // Equipment Related
    public static string NoEquipmentRequired => "No equipment required for this workout template";
    
    // General Operation Errors
    public static string CreateFailedFormat => "Failed to create workout template: {0}";
    public static string UpdateFailedFormat => "Failed to update workout template: {0}";
    public static string DeleteFailedFormat => "Failed to delete workout template: {0}";
    public static string DuplicateFailedFormat => "Failed to duplicate workout template: {0}";
    
    // Domain Validation Errors
    public static string NameLengthInvalid => "Name must be between 3 and 100 characters";
    public static string DescriptionTooLong => "Description cannot exceed 1000 characters";
    public static string CategoryRequired => "Category is required";
    public static string CategoryIdEmpty => "Category ID cannot be empty";
    public static string DifficultyRequired => "Difficulty level is required";
    public static string DifficultyIdEmpty => "Difficulty level ID cannot be empty";
    public static string DurationInvalid => "Estimated duration must be between 5 and 300 minutes";
    public static string TooManyTags => "Maximum 10 tags allowed";
    
    // Internal Errors
    public static string DraftStateNotFound => "Draft workout state not found in database";
}