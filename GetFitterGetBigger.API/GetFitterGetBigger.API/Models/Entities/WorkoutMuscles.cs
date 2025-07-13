using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

/// <summary>
/// Relationship entity linking workout templates to muscle groups with engagement levels.
/// Future: Will be used when WorkoutTemplate entity is implemented.
/// </summary>
public record WorkoutMuscles
{
    public WorkoutMusclesId Id { get; init; }
    // Future: Replace with WorkoutTemplateId when WorkoutTemplate entity is created
    public Guid WorkoutTemplateId { get; init; }
    public MuscleGroupId MuscleGroupId { get; init; }
    public int EngagementLevel { get; init; }
    public int LoadEstimation { get; init; }
    public bool IsActive { get; init; } = true;
    
    // Navigation properties
    // Future: Add WorkoutTemplate navigation property when entity exists
    public MuscleGroup MuscleGroup { get; init; } = null!;
    
    private WorkoutMuscles() { }
    
    public static class Handler
    {
        public static WorkoutMuscles Create(
            Guid workoutTemplateId,
            MuscleGroupId muscleGroupId,
            int engagementLevel,
            int loadEstimation,
            bool isActive = true)
        {
            ValidateParameters(engagementLevel, loadEstimation);
            
            return new()
            {
                Id = WorkoutMusclesId.New(),
                WorkoutTemplateId = workoutTemplateId,
                MuscleGroupId = muscleGroupId,
                EngagementLevel = engagementLevel,
                LoadEstimation = loadEstimation,
                IsActive = isActive
            };
        }
        
        public static WorkoutMuscles CreateWithId(
            WorkoutMusclesId id,
            Guid workoutTemplateId,
            MuscleGroupId muscleGroupId,
            int engagementLevel,
            int loadEstimation,
            bool isActive = true)
        {
            ValidateParameters(engagementLevel, loadEstimation);
            
            return new()
            {
                Id = id,
                WorkoutTemplateId = workoutTemplateId,
                MuscleGroupId = muscleGroupId,
                EngagementLevel = engagementLevel,
                LoadEstimation = loadEstimation,
                IsActive = isActive
            };
        }
        
        public static WorkoutMuscles Update(
            WorkoutMuscles workoutMuscles,
            int? engagementLevel = null,
            int? loadEstimation = null,
            bool? isActive = null)
        {
            var newEngagementLevel = engagementLevel ?? workoutMuscles.EngagementLevel;
            var newLoadEstimation = loadEstimation ?? workoutMuscles.LoadEstimation;
            
            ValidateParameters(newEngagementLevel, newLoadEstimation);
            
            return workoutMuscles with
            {
                EngagementLevel = newEngagementLevel,
                LoadEstimation = newLoadEstimation,
                IsActive = isActive ?? workoutMuscles.IsActive
            };
        }
        
        public static WorkoutMuscles Deactivate(WorkoutMuscles workoutMuscles) =>
            workoutMuscles with { IsActive = false };
        
        private static void ValidateParameters(int engagementLevel, int loadEstimation)
        {
            if (engagementLevel < 1 || engagementLevel > 10)
                throw new ArgumentException("Engagement level must be between 1 and 10", nameof(engagementLevel));
                
            if (loadEstimation < 1 || loadEstimation > 10)
                throw new ArgumentException("Load estimation must be between 1 and 10", nameof(loadEstimation));
        }
    }
}