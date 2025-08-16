namespace GetFitterGetBigger.API.Constants.ErrorMessages;

/// <summary>
/// Centralized error messages for SetConfiguration-related operations
/// </summary>
public static class SetConfigurationErrorMessages
{
    // General Messages
    public static string CommandCannotBeNull => "Command cannot be null";
    public static string NotFound => "Set configuration not found";
    
    // ID Validation
    public static string WorkoutTemplateExerciseIdRequired => "WorkoutTemplateExerciseId is required";
    public static string SetConfigurationIdRequired => "SetConfigurationId is required";
    public static string UserIdRequired => "UserId is required";
    
    // Set Number Validation
    public static string SetNumberMustBeGreaterThanZero => "SetNumber must be greater than 0";
    public static string DuplicateSetNumberFound => "Duplicate SetNumber {0} found";
    public static string DuplicateSetNumberInReorder => "Duplicate SetNumber {0} in reorder command";
    
    // Target Values Validation
    public static string RestSecondsCannotBeNegative => "RestSeconds cannot be negative";
    public static string TargetWeightCannotBeNegative => "TargetWeight cannot be negative";
    public static string TargetTimeSecondsMustBeGreaterThanZero => "TargetTimeSeconds must be greater than 0";
    
    // Bulk Operations
    public static string AtLeastOneSetConfigurationRequired => "At least one set configuration is required";
    public static string AtLeastOneSetConfigurationUpdateRequired => "At least one set configuration update is required";
    public static string AtLeastOneSetReorderRequired => "At least one set reorder is required";
    public static string SetConfigurationIdRequiredForAllUpdates => "SetConfigurationId is required for all updates";
    public static string SetConfigurationIdRequiredForAllReorders => "SetConfigurationId is required for all reorders";
    
    // Set-specific Validation with formatting
    public static string RestSecondsCannotBeNegativeForSet => "RestSeconds cannot be negative for set {0}";
    public static string TargetWeightCannotBeNegativeForSet => "TargetWeight cannot be negative for set {0}";
    public static string TargetTimeSecondsMustBeGreaterThanZeroForSet => "TargetTimeSeconds must be greater than 0 for set {0}";
    
    // Operation Failures
    public static string FailedToReorderSetConfigurations => "Failed to reorder set configurations";
    public static string SetConfigurationNotFoundWithId => "Set configuration {0} not found";
}