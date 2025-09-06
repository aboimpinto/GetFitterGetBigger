using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

public interface IExerciseLinkValidationService
{
    /// <summary>
    /// Validates if an exercise can have links (expanded for four-way linking)
    /// </summary>
    ValidationResult ValidateExerciseTypeCompatibility(ExerciseDto exercise);

    /// <summary>
    /// Validates if two exercises can be linked as alternatives (must share exercise types)
    /// </summary>
    ValidationResult ValidateAlternativeExerciseCompatibility(ExerciseDto sourceExercise, ExerciseDto targetExercise);

    /// <summary>
    /// Validates if adding a link would create a circular reference
    /// </summary>
    Task<ValidationResult> ValidateCircularReference(string sourceExerciseId, string targetExerciseId, ExerciseLinkType linkType);


    /// <summary>
    /// Validates if a link already exists between the exercises
    /// </summary>
    ValidationResult ValidateDuplicateLink(IEnumerable<ExerciseLinkDto> existingLinks, string targetExerciseId, ExerciseLinkType linkType);

    /// <summary>
    /// Performs all validations for creating a new link
    /// </summary>
    Task<ValidationResult> ValidateCreateLink(ExerciseDto sourceExercise, string targetExerciseId, ExerciseLinkType linkType, IEnumerable<ExerciseLinkDto> existingLinks);

    /// <summary>
    /// Validates if a specific link type can be added based on the source exercise's current context
    /// Implements business rules: Warmup/Cooldown exercises can only add Workout and Alternative links
    /// </summary>
    ValidationResult CanAddLinkType(string exerciseContext, string linkType);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(string errorMessage, string? errorCode = null) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}