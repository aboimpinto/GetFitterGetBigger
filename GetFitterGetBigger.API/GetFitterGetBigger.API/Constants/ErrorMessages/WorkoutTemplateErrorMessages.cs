namespace GetFitterGetBigger.API.Constants.ErrorMessages;

/// <summary>
/// Centralized error messages for WorkoutTemplate-related operations
/// </summary>
public static class WorkoutTemplateErrorMessages
{
    // ID Format Validation
    public static string InvalidIdFormat => "Invalid WorkoutTemplateId format. Expected format: 'workouttemplate-{guid}'";
    public static string InvalidStateIdFormat => "New state ID is required or invalid";
    
    // Not Found Errors
    public static string NotFound => "Workout template not found";
    public static string OriginalNotFound => "Original workout template not found";
    
    // Validation Errors
    public static string NameRequired => "Template name is required";
    
    // Duplicate/Conflict Errors
    public static string NameAlreadyExists => "A workout template with this name already exists";
    
    // State Transition Errors
    public static string InvalidStateTransition => "Invalid state transition";
    
    // Exercise Related
    public static string NoSuggestedExercisesFound => "No suggested exercises found for the given criteria";
    
    
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
    
    // Additional Validation Messages
    public static string NewStateIdRequired => "New state ID is required";
    public static string NameCannotBeEmpty => "Name cannot be empty";
    public static string CategoryIdRequired => "Category ID is required for suggestions";
    public static string MaxSuggestionsRange => "Max suggestions must be between 1 and 50";
    public static string CannotDeleteWithExecutionLogs => "Cannot delete workout template with execution logs";
    
    // Pagination Validation Messages
    public static string PageNumberInvalid => "Page number must be at least 1";
    public static string PageSizeInvalid => "Page size must be between 1 and 100";
}