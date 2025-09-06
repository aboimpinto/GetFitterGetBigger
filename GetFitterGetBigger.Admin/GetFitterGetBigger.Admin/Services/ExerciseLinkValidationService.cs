using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

public class ExerciseLinkValidationService : IExerciseLinkValidationService
{
    private readonly IExerciseLinkService _exerciseLinkService;

    public ExerciseLinkValidationService(IExerciseLinkService exerciseLinkService)
    {
        _exerciseLinkService = exerciseLinkService ?? throw new ArgumentNullException(nameof(exerciseLinkService));
    }

    public ValidationResult ValidateExerciseTypeCompatibility(ExerciseDto exercise)
    {
        if (exercise == null)
        {
            return ValidationResult.Failure("Exercise cannot be null", "EXERCISE_NULL");
        }

        // REST exercises cannot have any links
        var hasRestType = exercise.ExerciseTypes?.Any(t => t.Value?.ToLower() == "rest") ?? false;
        if (hasRestType)
        {
            return ValidationResult.Failure(
                "REST exercises cannot have relationships with other exercises",
                "REST_EXERCISE_NO_LINKS"
            );
        }

        // Four-way linking: exercises with any valid type can have links
        var hasValidTypes = exercise.ExerciseTypes?.Any(t => 
            t.Value?.ToLower() == "workout" || 
            t.Value?.ToLower() == "warmup" || 
            t.Value?.ToLower() == "cooldown") ?? false;
        
        if (!hasValidTypes)
        {
            var types = string.Join(", ", exercise.ExerciseTypes?.Select(t => t.Value) ?? new[] { "Unknown" });
            return ValidationResult.Failure(
                $"Only exercises of type 'Workout', 'Warmup', or 'Cooldown' can have links. This exercise has types: {types}",
                "INVALID_EXERCISE_TYPE"
            );
        }

        return ValidationResult.Success();
    }

    public ValidationResult ValidateAlternativeExerciseCompatibility(ExerciseDto sourceExercise, ExerciseDto targetExercise)
    {
        if (sourceExercise == null)
        {
            return ValidationResult.Failure("Source exercise cannot be null", "SOURCE_EXERCISE_NULL");
        }

        if (targetExercise == null)
        {
            return ValidationResult.Failure("Target exercise cannot be null", "TARGET_EXERCISE_NULL");
        }

        // Self-reference check
        if (sourceExercise.Id == targetExercise.Id)
        {
            return ValidationResult.Failure("An exercise cannot be an alternative to itself", "SELF_REFERENCE");
        }

        // Both exercises must have exercise types
        var sourceTypes = sourceExercise.ExerciseTypes?.Select(t => t.Value?.ToLower()).Where(v => !string.IsNullOrEmpty(v)).ToList();
        var targetTypes = targetExercise.ExerciseTypes?.Select(t => t.Value?.ToLower()).Where(v => !string.IsNullOrEmpty(v)).ToList();

        if (sourceTypes == null || !sourceTypes.Any())
        {
            return ValidationResult.Failure("Source exercise must have at least one exercise type", "MISSING_SOURCE_TYPES");
        }

        if (targetTypes == null || !targetTypes.Any())
        {
            return ValidationResult.Failure("Target exercise must have at least one exercise type", "MISSING_TARGET_TYPES");
        }

        // Alternative exercises must share at least one exercise type
        var commonTypes = sourceTypes.Intersect(targetTypes).ToList();
        if (!commonTypes.Any())
        {
            var sourceTypeNames = string.Join(", ", sourceExercise.ExerciseTypes?.Select(t => t.Value) ?? new[] { "Unknown" });
            var targetTypeNames = string.Join(", ", targetExercise.ExerciseTypes?.Select(t => t.Value) ?? new[] { "Unknown" });
            
            return ValidationResult.Failure(
                $"Alternative exercises must share at least one exercise type. Source types: {sourceTypeNames}. Target types: {targetTypeNames}.",
                "NO_SHARED_TYPES"
            );
        }

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateCircularReference(string sourceExerciseId, string targetExerciseId, ExerciseLinkType linkType)
    {
        if (string.IsNullOrEmpty(sourceExerciseId) || string.IsNullOrEmpty(targetExerciseId))
        {
            return ValidationResult.Failure("Exercise IDs cannot be null or empty", "INVALID_EXERCISE_ID");
        }

        // Check direct self-reference
        if (sourceExerciseId == targetExerciseId)
        {
            return ValidationResult.Failure("An exercise cannot be linked to itself", "SELF_REFERENCE");
        }

        try
        {
            // Check if the target exercise has the source as a link (would create a cycle)
            var targetLinks = await _exerciseLinkService.GetLinksAsync(targetExerciseId);

            if (targetLinks?.Links?.Any(link => link.TargetExerciseId == sourceExerciseId) == true)
            {
                return ValidationResult.Failure(
                    "This would create a circular reference. The target exercise already has a link back to the source exercise",
                    "CIRCULAR_REFERENCE"
                );
            }

            // For a more thorough check, we could traverse the entire link graph
            // but for now, we'll just check direct circular references

            return ValidationResult.Success();
        }
        catch (Exception)
        {
            // If we can't check for circular references, we'll allow the operation
            // The API should also validate this
            return ValidationResult.Success();
        }
    }


    public ValidationResult ValidateDuplicateLink(IEnumerable<ExerciseLinkDto> existingLinks, string targetExerciseId, ExerciseLinkType linkType)
    {
        if (existingLinks == null)
        {
            return ValidationResult.Success();
        }

        var duplicateExists = existingLinks.Any(link =>
            link.TargetExerciseId == targetExerciseId &&
            link.LinkType == linkType.ToString() &&
            link.IsActive);

        if (duplicateExists)
        {
            var linkTypeText = linkType switch
            {
                ExerciseLinkType.Warmup => "warmup",
                ExerciseLinkType.Cooldown => "cooldown",
                ExerciseLinkType.Alternative => "alternative",
                _ => "linked"
            };
            
            return ValidationResult.Failure(
                $"This exercise is already linked as a {linkTypeText} exercise",
                "DUPLICATE_LINK"
            );
        }

        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateCreateLink(
        ExerciseDto sourceExercise,
        string targetExerciseId,
        ExerciseLinkType linkType,
        IEnumerable<ExerciseLinkDto> existingLinks)
    {
        // Validate exercise type compatibility
        var typeResult = ValidateExerciseTypeCompatibility(sourceExercise);
        if (!typeResult.IsValid)
        {
            return typeResult;
        }

        // For alternative links, we need the target exercise to validate compatibility
        if (linkType == ExerciseLinkType.Alternative)
        {
            try
            {
                // Get the target exercise details for alternative link validation
                // Note: This would need to be injected as IExerciseService
                // For now, we'll do basic validation and let the API handle detailed validation
                
                // Basic self-reference check
                if (sourceExercise.Id == targetExerciseId)
                {
                    return ValidationResult.Failure("An exercise cannot be an alternative to itself", "SELF_REFERENCE");
                }
            }
            catch (Exception)
            {
                // If we can't validate alternative compatibility, let the API handle it
                return ValidationResult.Success();
            }
        }


        // Validate duplicate link
        var duplicateResult = ValidateDuplicateLink(existingLinks ?? Enumerable.Empty<ExerciseLinkDto>(), targetExerciseId, linkType);
        if (!duplicateResult.IsValid)
        {
            return duplicateResult;
        }

        // Validate circular reference
        var circularResult = await ValidateCircularReference(sourceExercise.Id, targetExerciseId, linkType);
        if (!circularResult.IsValid)
        {
            return circularResult;
        }

        return ValidationResult.Success();
    }
}