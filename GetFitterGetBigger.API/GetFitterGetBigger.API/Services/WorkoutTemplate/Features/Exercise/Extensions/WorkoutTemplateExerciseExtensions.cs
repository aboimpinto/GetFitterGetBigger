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
            Phase = entity.Zone.ToString(), // Map Zone to Phase for new enhanced DTO
            RoundNumber = 1, // Default round number since entity doesn't have rounds yet
            OrderInRound = entity.SequenceOrder, // Map SequenceOrder to OrderInRound
            Metadata = "{}", // Default empty JSON metadata since entity doesn't have it yet
            Notes = entity.Notes,
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