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
            ExecutionProtocol = workoutTemplate.ExecutionProtocol.ToReferenceDataDto(),
            ExecutionProtocolConfig = workoutTemplate.ExecutionProtocolConfig,
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
            return WorkoutTemplateExerciseDto.Empty;

        return new()
        {
            Id = exercise.Id.ToString(),
            Exercise = exercise.Exercise?.ToDto() ?? ExerciseDto.Empty,
            Phase = exercise.Zone.ToString(), // Map Zone to Phase for new enhanced DTO
            RoundNumber = 1, // Default round number since entity doesn't have rounds yet
            OrderInRound = exercise.SequenceOrder, // Map SequenceOrder to OrderInRound
            Metadata = "{}", // Default empty JSON metadata since entity doesn't have it yet
            Notes = exercise.Notes,
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
            return SetConfigurationDto.Empty;

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
    /// Maps WorkoutCategory entity to ReferenceDataDto
    /// </summary>
    public static ReferenceDataDto ToReferenceDataDto(this WorkoutCategory? entity)
    {
        if (entity == null || entity.IsEmpty)
            return ReferenceDataDto.Empty;

        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }

    /// <summary>
    /// Maps DifficultyLevel entity to ReferenceDataDto
    /// </summary>
    public static ReferenceDataDto ToReferenceDataDto(this DifficultyLevel? entity)
    {
        if (entity == null || entity.IsEmpty)
            return ReferenceDataDto.Empty;

        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }

    /// <summary>
    /// Maps WorkoutState entity to ReferenceDataDto
    /// </summary>
    public static ReferenceDataDto ToReferenceDataDto(this WorkoutState? entity)
    {
        if (entity == null || entity.IsEmpty)
            return ReferenceDataDto.Empty;

        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }

    /// <summary>
    /// Maps ExecutionProtocol entity to ReferenceDataDto
    /// </summary>
    public static ReferenceDataDto ToReferenceDataDto(this ExecutionProtocol? entity)
    {
        if (entity == null || entity.IsEmpty)
            return ReferenceDataDto.Empty;

        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }

    /// <summary>
    /// Maps WorkoutObjective entity to ReferenceDataDto
    /// </summary>
    public static ReferenceDataDto ToReferenceDataDto(this WorkoutObjective? entity)
    {
        if (entity == null || entity.IsEmpty)
            return ReferenceDataDto.Empty;

        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}