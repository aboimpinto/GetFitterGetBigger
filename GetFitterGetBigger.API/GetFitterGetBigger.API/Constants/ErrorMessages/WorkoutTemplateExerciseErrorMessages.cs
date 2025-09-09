namespace GetFitterGetBigger.API.Constants.ErrorMessages;

/// <summary>
/// Centralized error messages for WorkoutTemplateExercise-related operations
/// </summary>
public static class WorkoutTemplateExerciseErrorMessages
{
    // General Messages
    public static string CommandCannotBeNull => "Command cannot be null";
    public static string InvalidCommandParameters => "Invalid command parameters";
    
    // Not Found Messages
    public static string ExerciseNotFound => "Exercise not found";
    public static string WorkoutTemplateNotFound => "Workout template not found";
    public static string TemplateExerciseNotFound => "Template exercise not found";
    public static string SourceTemplateNotFound => "Source template not found";
    public static string TargetTemplateNotFound => "Target template not found";
    
    // Validation Messages
    public static string InvalidExerciseId => "Invalid exercise ID";
    public static string InvalidWorkoutTemplateId => "Invalid workout template ID";
    public static string InvalidUserId => "Invalid user ID";
    public static string InvalidTemplateIdOrZone => "Invalid template ID or zone";
    public static string InvalidTemplateIdOrExerciseList => "Invalid template ID or exercise list";
    
    // Zone Validation Messages
    public static string InvalidZoneWarmupMainCooldown => "Invalid zone: {0}. Must be Warmup, Main, or Cooldown";
    public static string InvalidZone => "Invalid zone: {0}";
    
    // Exercise-specific Messages
    public static string ExerciseNotFoundWithId => "Exercise {0} not found";
    
    // State Validation Messages
    public static string DraftStateRequired => "DRAFT";
    public static string CanOnlyAddExercisesToDraftTemplates => "Can only add exercises to templates in DRAFT state";
    public static string CanOnlyUpdateExercisesInDraftTemplates => "Can only update exercises in templates in DRAFT state";
    public static string CanOnlyRemoveExercisesFromDraftTemplates => "Can only remove exercises from templates in DRAFT state";
    public static string CanOnlyReorderExercisesInDraftTemplates => "Can only reorder exercises in templates in DRAFT state";
    public static string CanOnlyChangeZonesInDraftTemplates => "Can only change zones in templates in DRAFT state";
    public static string CanOnlyDuplicateExercisesToDraftTemplates => "Can only duplicate exercises to templates in DRAFT state";
    
    // Duplication Messages
    public static string SourceTemplateHasNoExercisesToDuplicate => "Source template has no exercises to duplicate";
}