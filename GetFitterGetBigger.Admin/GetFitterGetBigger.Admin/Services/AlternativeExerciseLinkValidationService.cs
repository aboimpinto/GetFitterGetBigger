using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Results;

namespace GetFitterGetBigger.Admin.Services
{
    /// <summary>
    /// Service for validating alternative exercise link compatibility
    /// Handles type compatibility, muscle group overlap scoring, and bidirectional validation
    /// </summary>
    public interface IAlternativeExerciseLinkValidationService
    {
        /// <summary>
        /// Validates alternative exercise types compatibility
        /// </summary>
        ValidationResult ValidateExerciseTypes(ExerciseDto sourceExercise, ExerciseDto targetExercise);

        /// <summary>
        /// Calculates muscle group overlap percentage
        /// </summary>
        int CalculateMuscleGroupOverlap(ExerciseDto sourceExercise, ExerciseDto targetExercise);
    }

    /// <summary>
    /// Implementation of alternative exercise validation service
    /// </summary>
    public class AlternativeExerciseLinkValidationService : IAlternativeExerciseLinkValidationService
    {
        public AlternativeExerciseLinkValidationService()
        {
            // Simplified service without external dependencies
        }

        public ValidationResult ValidateExerciseTypes(ExerciseDto sourceExercise, ExerciseDto targetExercise)
        {
            if (sourceExercise == null)
            {
                return ValidationResult.Failure("Source exercise cannot be null", "SOURCE_EXERCISE_NULL");
            }

            if (targetExercise == null)
            {
                return ValidationResult.Failure("Target exercise cannot be null", "TARGET_EXERCISE_NULL");
            }

            // Get exercise types (case-insensitive)
            var sourceTypes = sourceExercise.ExerciseTypes?
                .Select(t => t.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet() ?? new HashSet<string>();

            var targetTypes = targetExercise.ExerciseTypes?
                .Select(t => t.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet() ?? new HashSet<string>();

            if (!sourceTypes.Any())
            {
                return ValidationResult.Failure(
                    "Source exercise must have at least one exercise type",
                    "MISSING_SOURCE_TYPES");
            }

            if (!targetTypes.Any())
            {
                return ValidationResult.Failure(
                    "Target exercise must have at least one exercise type",
                    "MISSING_TARGET_TYPES");
            }

            // Check for shared types
            var sharedTypes = sourceTypes.Intersect(targetTypes).ToList();
            if (!sharedTypes.Any())
            {
                var sourceTypeNames = string.Join(", ", sourceExercise.ExerciseTypes?.Select(t => t.Value) ?? Array.Empty<string>());
                var targetTypeNames = string.Join(", ", targetExercise.ExerciseTypes?.Select(t => t.Value) ?? Array.Empty<string>());
                
                return ValidationResult.Failure(
                    $"Alternative exercises must share at least one exercise type. " +
                    $"Source: [{sourceTypeNames}], Target: [{targetTypeNames}]",
                    "NO_SHARED_TYPES");
            }

            return ValidationResult.Success();
        }

        public int CalculateMuscleGroupOverlap(ExerciseDto sourceExercise, ExerciseDto targetExercise)
        {
            if (sourceExercise?.MuscleGroups == null || targetExercise?.MuscleGroups == null)
            {
                return 0;
            }

            // Get primary muscle groups
            var sourcePrimary = sourceExercise.MuscleGroups
                .Where(mg => mg.Role?.Value?.ToLower() == "primary")
                .Select(mg => mg.MuscleGroup?.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet();

            var targetPrimary = targetExercise.MuscleGroups
                .Where(mg => mg.Role?.Value?.ToLower() == "primary")
                .Select(mg => mg.MuscleGroup?.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet();

            // Get secondary muscle groups
            var sourceSecondary = sourceExercise.MuscleGroups
                .Where(mg => mg.Role?.Value?.ToLower() == "secondary")
                .Select(mg => mg.MuscleGroup?.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet();

            var targetSecondary = targetExercise.MuscleGroups
                .Where(mg => mg.Role?.Value?.ToLower() == "secondary")
                .Select(mg => mg.MuscleGroup?.Value?.ToLower())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToHashSet();

            if (!sourcePrimary.Any() && !sourceSecondary.Any() && 
                !targetPrimary.Any() && !targetSecondary.Any())
            {
                return 0;
            }

            // Calculate weighted overlap
            var primaryOverlap = sourcePrimary.Intersect(targetPrimary).Count();
            var secondaryOverlap = sourceSecondary.Intersect(targetSecondary).Count();
            var crossOverlap = sourcePrimary.Intersect(targetSecondary).Count() + 
                              sourceSecondary.Intersect(targetPrimary).Count();

            // Weighted scoring: Primary-to-Primary = 60%, Secondary-to-Secondary = 30%, Cross = 10%
            var totalSourceMuscles = sourcePrimary.Count + sourceSecondary.Count;
            var totalTargetMuscles = targetPrimary.Count + targetSecondary.Count;
            var maxMuscles = Math.Max(totalSourceMuscles, totalTargetMuscles);

            if (maxMuscles == 0) return 0;

            var weightedScore = (primaryOverlap * 0.6) + (secondaryOverlap * 0.3) + (crossOverlap * 0.1);
            var percentage = (int)Math.Round((weightedScore / maxMuscles) * 100);

            return Math.Min(100, Math.Max(0, percentage));
        }
    }
}