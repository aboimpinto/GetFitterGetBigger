using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;

public static class WorkoutTemplateExerciseExtensions
{
    public static WorkoutTemplateExerciseDto ToDto(this WorkoutTemplateExercise entity)
    {
        return new WorkoutTemplateExerciseDto
        {
            Id = entity.Id.ToString(),
            Exercise = entity.Exercise != null ? MapExerciseToDto(entity.Exercise) : ExerciseDto.Empty,
            Zone = entity.Zone.ToString(),
            SequenceOrder = entity.SequenceOrder,
            Notes = entity.Notes,
            SetConfigurations = entity.Configurations
                .OrderBy(c => c.SetNumber)
                .Select(c => new SetConfigurationDto
                {
                    Id = c.Id.ToString(),
                    SetNumber = c.SetNumber,
                    TargetReps = c.TargetReps,
                    TargetWeight = c.TargetWeight,
                    TargetTime = c.TargetTimeSeconds,
                    RestSeconds = c.RestSeconds,
                    Notes = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static List<RoundDto> OrganizeByRound(this IEnumerable<WorkoutTemplateExercise> exercises)
    {
        // Group exercises by zone for now (rounds not implemented yet)
        return exercises
            .GroupBy(e => e.Zone)
            .Select(g => new RoundDto(
                1, // Default to round 1 for now
                g.OrderBy(e => e.SequenceOrder)
                    .Select(e => e.ToDto())
                    .ToList()))
            .ToList();
    }

    public static bool IsValidPhase(this string phase)
    {
        var validPhases = new[] { "Warmup", "Workout", "Main", "Cooldown" };
        return validPhases.Contains(phase, StringComparer.OrdinalIgnoreCase);
    }

    private static ExerciseDto MapExerciseToDto(GetFitterGetBigger.API.Models.Entities.Exercise entity)
    {
        return new ExerciseDto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Description = entity.Description,
            Difficulty = entity.Difficulty != null ? new ReferenceDataDto 
            { 
                Id = entity.Difficulty.Id.ToString(), 
                Value = entity.Difficulty.Value 
            } : ReferenceDataDto.Empty
        };
    }
}