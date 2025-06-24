using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseTargetedMuscle
{
    public ExerciseId ExerciseId { get; init; }
    public MuscleGroupId MuscleGroupId { get; init; }
    public MuscleRoleId MuscleRoleId { get; init; }
    
    // Navigation properties
    public Exercise Exercise { get; init; } = null!;
    public MuscleGroup MuscleGroup { get; init; } = null!;
    public MuscleRole? MuscleRole { get; init; }
    
    private ExerciseTargetedMuscle() { }
    
    public static class Handler
    {
        public static ExerciseTargetedMuscle Create(
            ExerciseId exerciseId, 
            MuscleGroupId muscleGroupId, 
            MuscleRoleId muscleRoleId) =>
            new()
            {
                ExerciseId = exerciseId,
                MuscleGroupId = muscleGroupId,
                MuscleRoleId = muscleRoleId
            };
    }
}
