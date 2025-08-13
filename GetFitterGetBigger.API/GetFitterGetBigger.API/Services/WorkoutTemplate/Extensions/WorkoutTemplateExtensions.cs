using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Extension methods for WorkoutTemplate entity to DTO mapping
/// </summary>
public static class WorkoutTemplateExtensions
{
    /// <summary>
    /// Maps a WorkoutTemplate entity to its DTO representation
    /// </summary>
    public static WorkoutTemplateDto ToDto(this WorkoutTemplateEntity workoutTemplate)
    {
        if (workoutTemplate.IsEmpty)
            return WorkoutTemplateDto.Empty;

        return new()
        {
            Id = workoutTemplate.Id.ToString(),
            Name = workoutTemplate.Name,
            Description = workoutTemplate.Description,
            Category = workoutTemplate.Category.ToReferenceDataDto(),
            Difficulty = workoutTemplate.Difficulty.ToReferenceDataDto(),
            EstimatedDurationMinutes = workoutTemplate.EstimatedDurationMinutes,
            Tags = workoutTemplate.Tags.ToList(),
            IsPublic = workoutTemplate.IsPublic,
            WorkoutState = workoutTemplate.WorkoutState.ToReferenceDataDto(),
            Objectives = workoutTemplate.Objectives?.Select(o => o.WorkoutObjective.ToReferenceDataDto()).ToList() ?? [],
            Exercises = workoutTemplate.Exercises?.Select(e => e.ToDto()).ToList() ?? [],
            CreatedAt = workoutTemplate.CreatedAt,
            UpdatedAt = workoutTemplate.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a WorkoutTemplateExercise entity to its DTO representation
    /// </summary>
    public static WorkoutTemplateExerciseDto ToDto(this WorkoutTemplateExercise exercise)
    {
        if (exercise.IsEmpty)
            return new WorkoutTemplateExerciseDto();

        return new()
        {
            Id = exercise.Id.ToString(),
            Exercise = exercise.Exercise != null ? new ExerciseDto 
            { 
                Id = exercise.Exercise.Id.ToString(),
                Name = exercise.Exercise.Name,
                Description = exercise.Exercise.Description
            } : ExerciseDto.Empty,
            Zone = exercise.Zone.ToString(),
            SequenceOrder = exercise.SequenceOrder,
            Notes = exercise.Notes,
            SetConfigurations = exercise.Configurations?.Select(c => c.ToDto()).ToList() ?? [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a SetConfiguration entity to its DTO representation
    /// </summary>
    public static SetConfigurationDto ToDto(this SetConfiguration config)
    {
        if (config.IsEmpty)
            return new SetConfigurationDto();

        return new()
        {
            Id = config.Id.ToString(),
            SetNumber = config.SetNumber,
            TargetReps = config.TargetReps,
            TargetWeight = config.TargetWeight,
            TargetTime = config.TargetTimeSeconds,
            RestSeconds = config.RestSeconds,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps any reference data entity to ReferenceDataDto using reflection
    /// </summary>
    public static ReferenceDataDto ToReferenceDataDto(this object? entity)
    {
        if (entity == null)
            return ReferenceDataDto.Empty;

        // Use reflection to check for IsEmpty property
        var entityType = entity.GetType();
        var isEmptyProperty = entityType.GetProperty("IsEmpty");
        if (isEmptyProperty != null)
        {
            var isEmpty = (bool)isEmptyProperty.GetValue(entity)!;
            if (isEmpty)
                return ReferenceDataDto.Empty;
        }

        // Get Id property
        var idProperty = entityType.GetProperty("Id");
        var idValue = idProperty?.GetValue(entity)?.ToString() ?? string.Empty;

        // Try to get Value or Name property
        var valueProperty = entityType.GetProperty("Value") ?? entityType.GetProperty("Name");
        var valueString = valueProperty?.GetValue(entity) as string ?? string.Empty;

        // Get Description property if it exists
        var descriptionProperty = entityType.GetProperty("Description");
        var descriptionString = descriptionProperty?.GetValue(entity) as string;

        return new ReferenceDataDto
        {
            Id = idValue,
            Value = valueString,
            Description = descriptionString
        };
    }
}