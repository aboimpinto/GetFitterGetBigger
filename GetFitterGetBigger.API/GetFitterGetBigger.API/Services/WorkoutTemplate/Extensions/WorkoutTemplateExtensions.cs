using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Services.Implementations.Extensions;
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
            Exercise = exercise.Exercise?.ToDto() ?? ExerciseDto.Empty,
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

        if (IsEntityEmpty(entity))
            return ReferenceDataDto.Empty;

        var entityType = entity.GetType();
        var id = ExtractIdValue(entity, entityType);
        var value = ExtractValueOrName(entity, entityType);
        var description = ExtractDescription(entity, entityType);

        return new ReferenceDataDto
        {
            Id = id,
            Value = value,
            Description = description
        };
    }

    /// <summary>
    /// Checks if the entity is considered empty using reflection
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity is empty, false otherwise</returns>
    private static bool IsEntityEmpty(object entity)
    {
        var entityType = entity.GetType();
        var isEmptyProperty = entityType.GetProperty("IsEmpty");
        
        if (isEmptyProperty == null)
            return false;

        var isEmpty = (bool)isEmptyProperty.GetValue(entity)!;
        return isEmpty;
    }

    /// <summary>
    /// Extracts the ID value from the entity
    /// </summary>
    /// <param name="entity">The entity to extract from</param>
    /// <param name="entityType">The type of the entity</param>
    /// <returns>The ID value as string</returns>
    private static string ExtractIdValue(object entity, Type entityType)
    {
        var idProperty = entityType.GetProperty("Id");
        return idProperty?.GetValue(entity)?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Extracts the Value or Name property from the entity
    /// </summary>
    /// <param name="entity">The entity to extract from</param>
    /// <param name="entityType">The type of the entity</param>
    /// <returns>The value or name as string</returns>
    private static string ExtractValueOrName(object entity, Type entityType)
    {
        var valueProperty = entityType.GetProperty("Value") ?? entityType.GetProperty("Name");
        return valueProperty?.GetValue(entity) as string ?? string.Empty;
    }

    /// <summary>
    /// Extracts the Description property from the entity if it exists
    /// </summary>
    /// <param name="entity">The entity to extract from</param>
    /// <param name="entityType">The type of the entity</param>
    /// <returns>The description as string or null if not found</returns>
    private static string? ExtractDescription(object entity, Type entityType)
    {
        var descriptionProperty = entityType.GetProperty("Description");
        return descriptionProperty?.GetValue(entity) as string;
    }
}