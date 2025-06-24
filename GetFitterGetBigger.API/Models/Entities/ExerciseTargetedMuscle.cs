using System;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseTargetedMuscle
{
    public ExerciseId ExerciseId { get; init; }
    public MuscleGroupId MuscleGroupId { get; init; }
    public MuscleRole Role { get; init; } = MuscleRole.Primary;
    
    // Navigation properties
    public Exercise Exercise { get; init; } = null!;
    public MuscleGroup MuscleGroup { get; init; } = null!;
    
    private ExerciseTargetedMuscle() { }
    
    public static class Handler
    {
        public static ExerciseTargetedMuscle Create(ExerciseId exerciseId, MuscleGroupId muscleGroupId, MuscleRole role) =>
            new()
            {
                ExerciseId = exerciseId,
                MuscleGroupId = muscleGroupId,
                Role = role
            };
    }
}
