using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutLogSet
{
    public WorkoutLogSetId Id { get; init; }
    public WorkoutLogId LogId { get; init; }
    public ExerciseId ExerciseId { get; init; }
    public int SetOrder { get; init; }
    public int? RepsCompleted { get; init; }
    public decimal? WeightUsedKg { get; init; }
    public int? DurationCompletedSec { get; init; }
    public int? DistanceCompletedM { get; init; }
    public string? Notes { get; init; }
    
    // Navigation properties
    public WorkoutLog WorkoutLog { get; init; } = null!;
    public Exercise Exercise { get; init; } = null!;
    
    private WorkoutLogSet() { }
    
    public static class Handler
    {
        public static WorkoutLogSet CreateNew(
            WorkoutLogId logId, 
            ExerciseId exerciseId, 
            int setOrder, 
            int? repsCompleted = null, 
            decimal? weightUsedKg = null, 
            int? durationCompletedSec = null, 
            int? distanceCompletedM = null, 
            string? notes = null) =>
            
            new()
            {
                Id = WorkoutLogSetId.New(),
                LogId = logId,
                ExerciseId = exerciseId,
                SetOrder = setOrder,
                RepsCompleted = repsCompleted,
                WeightUsedKg = weightUsedKg,
                DurationCompletedSec = durationCompletedSec,
                DistanceCompletedM = distanceCompletedM,
                Notes = notes
            };
        
        public static WorkoutLogSet Create(
            WorkoutLogSetId id,
            WorkoutLogId logId, 
            ExerciseId exerciseId, 
            int setOrder, 
            int? repsCompleted = null, 
            decimal? weightUsedKg = null, 
            int? durationCompletedSec = null, 
            int? distanceCompletedM = null, 
            string? notes = null) =>
            
            new()
            {
                Id = id,
                LogId = logId,
                ExerciseId = exerciseId,
                SetOrder = setOrder,
                RepsCompleted = repsCompleted,
                WeightUsedKg = weightUsedKg,
                DurationCompletedSec = durationCompletedSec,
                DistanceCompletedM = distanceCompletedM,
                Notes = notes
            };
    }
}
