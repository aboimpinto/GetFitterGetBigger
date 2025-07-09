using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

public class ExerciseLinkValidationService : IExerciseLinkValidationService
{
    private readonly IExerciseLinkService _exerciseLinkService;
    private const int MaxLinksPerType = 10;
    
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
        
        // Only Workout type exercises can have links
        var hasWorkoutType = exercise.ExerciseTypes?.Any(t => t.Value?.ToLower() == "workout") ?? false;
        if (!hasWorkoutType)
        {
            var types = string.Join(", ", exercise.ExerciseTypes?.Select(t => t.Value) ?? new[] { "Unknown" });
            return ValidationResult.Failure(
                $"Only exercises of type 'Workout' can have links. This exercise has types: {types}", 
                "INVALID_EXERCISE_TYPE"
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
    
    public ValidationResult ValidateMaximumLinks(int currentLinkCount, ExerciseLinkType linkType)
    {
        if (currentLinkCount >= MaxLinksPerType)
        {
            var linkTypeText = linkType == ExerciseLinkType.Warmup ? "warmup" : "cooldown";
            return ValidationResult.Failure(
                $"Maximum number of {linkTypeText} links ({MaxLinksPerType}) has been reached", 
                "MAX_LINKS_REACHED"
            );
        }
        
        return ValidationResult.Success();
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
            var linkTypeText = linkType == ExerciseLinkType.Warmup ? "warmup" : "cooldown";
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
        
        // Validate maximum links
        var linksOfType = existingLinks?.Count(link => link.LinkType == linkType.ToString() && link.IsActive) ?? 0;
        var maxResult = ValidateMaximumLinks(linksOfType, linkType);
        if (!maxResult.IsValid)
        {
            return maxResult;
        }
        
        // Validate duplicate link
        var duplicateResult = ValidateDuplicateLink(existingLinks, targetExerciseId, linkType);
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