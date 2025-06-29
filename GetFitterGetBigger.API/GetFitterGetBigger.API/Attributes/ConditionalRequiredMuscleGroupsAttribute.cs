using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Attributes;

/// <summary>
/// Custom validation attribute that requires muscle groups only when the exercise is not a REST type.
/// REST exercises are allowed to have empty muscle groups since they represent recovery periods.
/// </summary>
public class ConditionalRequiredMuscleGroupsAttribute : ValidationAttribute
{
    public ConditionalRequiredMuscleGroupsAttribute()
    {
        ErrorMessage = "At least one muscle group must be specified for non-REST exercises";
    }

    public override bool IsValid(object? value)
    {
        // If value is null, it's invalid (empty list is different from null)
        if (value == null)
            return false;

        // Cast to expected type
        if (value is not List<MuscleGroupWithRoleRequest> muscleGroups)
            return false;

        // If we don't have access to the parent object, we can't validate contextually
        // This will be handled in the validation context override
        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Get the muscle groups list
        if (value is not List<MuscleGroupWithRoleRequest> muscleGroups)
        {
            return new ValidationResult("Invalid muscle groups format");
        }

        // Get the parent object (CreateExerciseRequest or UpdateExerciseRequest)
        var parentObject = validationContext.ObjectInstance;
        List<string>? exerciseTypeIds = null;

        // Extract exercise type IDs based on the parent object type
        switch (parentObject)
        {
            case CreateExerciseRequest createRequest:
                exerciseTypeIds = createRequest.ExerciseTypeIds;
                break;
            case UpdateExerciseRequest updateRequest:
                exerciseTypeIds = updateRequest.ExerciseTypeIds;
                break;
            default:
                return new ValidationResult("Invalid context for muscle group validation");
        }

        // Check if this is a REST exercise
        bool isRestExercise = IsRestExercise(exerciseTypeIds);

        // If it's a REST exercise, muscle groups are optional
        if (isRestExercise)
        {
            return ValidationResult.Success;
        }

        // For non-REST exercises, at least one muscle group is required
        if (muscleGroups == null || muscleGroups.Count == 0)
        {
            return new ValidationResult(ErrorMessage ?? "At least one muscle group must be specified for non-REST exercises");
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Determines if the exercise is a REST exercise based on the exercise type IDs.
    /// Checks for "rest" in a case-insensitive manner and handles the format "exercisetype-{guid}".
    /// </summary>
    /// <param name="exerciseTypeIds">List of exercise type IDs</param>
    /// <returns>True if any exercise type contains "rest", false otherwise</returns>
    private static bool IsRestExercise(List<string>? exerciseTypeIds)
    {
        if (exerciseTypeIds == null || exerciseTypeIds.Count == 0)
            return false;

        return exerciseTypeIds.Any(typeId =>
        {
            if (string.IsNullOrWhiteSpace(typeId))
                return false;

            // Extract the type name from the format "exercisetype-{guid}"
            // The type name should come after "exercisetype-" but before the GUID
            // However, based on the existing codebase, we need to check for "rest" in the ID
            return typeId.Contains("rest", StringComparison.OrdinalIgnoreCase);
        });
    }
}