namespace GetFitterGetBigger.API.Constants;

/// <summary>
/// Centralized error messages for ExerciseLink-related operations
/// </summary>
public static class ExerciseLinkErrorMessages
{
    // ID validation
    public const string InvalidSourceExerciseId = "Invalid source exercise ID format";
    public const string InvalidTargetExerciseId = "Invalid target exercise ID format";
    public const string InvalidLinkId = "Invalid exercise link ID format";
    
    // Business rules
    public const string CannotLinkToSelf = "Cannot link an exercise to itself";
    public const string SourceExerciseNotFound = "Source exercise not found or inactive";
    public const string TargetExerciseNotFound = "Target exercise not found or inactive";
    public const string SourceMustBeWorkout = "Source exercise must be of type 'Workout'";
    public const string TargetMustMatchLinkType = "Target exercise must match the specified link type";
    public const string RestExercisesCannotHaveLinks = "REST exercises cannot have links";
    public const string RestExercisesCannotBeLinked = "REST exercises cannot be linked";
    public const string LinkAlreadyExists = "A link of this type already exists between these exercises";
    public const string CircularReferenceDetected = "This link would create a circular reference";
    public const string MaximumLinksReached = "Maximum number of links of this type (10) has been reached";
    public const string LinkNotFound = "Exercise link not found";
    public const string LinkDoesNotBelongToExercise = "Link does not belong to the specified exercise";
    
    // Validation
    public const string LinkTypeRequired = "Link type is required";
    public const string InvalidLinkType = "Link type must be either 'Warmup' or 'Cooldown'";
    public const string DisplayOrderMustBeNonNegative = "Display order must be a non-negative number";
    public const string CountMustBeBetween1And20 = "Count must be between 1 and 20";
}