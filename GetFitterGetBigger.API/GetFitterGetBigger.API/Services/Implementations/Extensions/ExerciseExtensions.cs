using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;
using GetFitterGetBigger.API.Builders;

namespace GetFitterGetBigger.API.Services.Implementations.Extensions;

/// <summary>
/// Extension methods for Exercise entity to DTO conversions
/// </summary>
public static class ExerciseExtensions
{
    /// <summary>
    /// Converts an Exercise entity to ExerciseDto
    /// </summary>
    public static ExerciseDto ToDto(this ExerciseEntity exercise)
    {
        if (exercise.IsEmpty)
        {
            return ExerciseDto.Empty;
        }
        
        return new ExerciseDtoBuilder(exercise)
            .WithBasicInfo()
            .WithCoachNotes()
            .WithExerciseTypes()
            .WithMuscleGroups()
            .WithEquipment()
            .WithBodyParts()
            .WithMovementPatterns()
            .WithReferenceData()
            .Build();
    }
}