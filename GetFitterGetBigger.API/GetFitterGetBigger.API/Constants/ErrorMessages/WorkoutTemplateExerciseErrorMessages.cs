namespace GetFitterGetBigger.API.Constants.ErrorMessages;

/// <summary>
/// Centralized error messages for WorkoutTemplateExercise-related operations
/// </summary>
public static class WorkoutTemplateExerciseErrorMessages
{
    // General Messages (removed unused)
    
    // Not Found Messages
    public static string ExerciseNotFound => "Exercise not found";
    public static string WorkoutTemplateNotFound => "Workout template not found";
    public static string TemplateExerciseNotFound => "Template exercise not found";
    public static string SourceTemplateNotFound => "Source template not found";
    public static string TargetTemplateNotFound => "Target template not found";
    
    // Validation Messages
    public static string InvalidExerciseId => "Invalid exercise ID";
    public static string InvalidWorkoutTemplateId => "Invalid workout template ID";
    [Obsolete("Legacy V1 API - Use V2 error messages instead")]
    public static string InvalidTemplateIdOrZone => "Invalid template ID or zone";
    [Obsolete("Legacy V1 API - Use V2 error messages instead")]
    public static string InvalidTemplateIdOrExerciseList => "Invalid template ID or exercise list";
    
    // Zone Validation Messages
    public static string InvalidZoneWarmupMainCooldown => "Invalid zone: {0}. Must be Warmup, Main, or Cooldown";
    [Obsolete("Legacy V1 API - Use InvalidZoneWarmupMainCooldown instead")]
    public static string InvalidZone => "Invalid zone: {0}";
    
    // Exercise-specific Messages
    
    // State Validation Messages
    public static string CanOnlyAddExercisesToDraftTemplates => "Can only add exercises to templates in DRAFT state";
    public static string CanOnlyUpdateExercisesInDraftTemplates => "Can only update exercises in templates in DRAFT state";
    public static string CanOnlyRemoveExercisesFromDraftTemplates => "Can only remove exercises from templates in DRAFT state";
    public static string CanOnlyReorderExercisesInDraftTemplates => "Can only reorder exercises in templates in DRAFT state";
    public static string CanOnlyChangeZonesInDraftTemplates => "Can only change zones in templates in DRAFT state";
    public static string CanOnlyDuplicateExercisesToDraftTemplates => "Can only duplicate exercises to templates in DRAFT state";
    
    // Duplication Messages
    [Obsolete("Legacy V1 API - Use NoExercisesInPhase instead")]
    public static string SourceTemplateHasNoExercisesToDuplicate => "Source template has no exercises to duplicate";
    
    // V2 API - Phase and Round Validation Messages
    public static string InvalidPhase => "Phase cannot be empty";
    public static string MustBeWarmupWorkoutCooldown => "Phase must be 'Warmup', 'Workout', or 'Cooldown'";
    public static string RoundNumberMustBePositive => "Round number must be greater than 0";
    public static string OrderMustBePositive => "Order in round must be greater than 0";
    public static string MetadataRequired => "Exercise metadata cannot be empty";
    public static string InvalidJsonMetadata => "Metadata must be valid JSON";
    
    // V2 API - Business Logic Messages
    public static string TemplateNotInDraftState => "Template must be in Draft state to modify exercises";
    public static string ExerciseNotActiveOrNotFound => "Exercise not found or not active";
    public static string ExerciseNotFoundInTemplate => "Exercise not found in this template";
    public static string DuplicateExerciseInRound => "Exercise already exists in this round";
    
    // V2 API - Auto-linking Messages
    public static string AutoLinkingFailed => "Failed to add linked warmup/cooldown exercises";
    public static string OrphanCleanupFailed => "Failed to clean up orphaned exercises";
    
    // V2 API - Round Management Messages
    public static string SourceRoundNotFound => "Source round not found";
    public static string TargetRoundAlreadyExists => "Target round already exists";
    public static string CannotCopyToSameRound => "Cannot copy round to itself";
    
    // V2 API - Metadata Validation Messages
    public static string InvalidMetadataForExerciseType => "Metadata is invalid for this exercise type";
    public static string InvalidMetadataForExecutionProtocol => "Metadata is invalid for this execution protocol";
    public static string RestExerciseOnlyAcceptsDuration => "REST exercises only accept duration in metadata";
    public static string WeightExerciseRequiresWeightMetadata => "Weight-based exercises require weight in metadata";
    
    // V2 API - Format Validation Messages
    public static string InvalidTemplateIdFormat => "Invalid workout template ID format";
    public static string InvalidExerciseIdFormat => "Invalid exercise ID format";
    public static string InvalidGuidFormat => "Invalid GUID format";
}