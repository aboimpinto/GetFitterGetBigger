using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutLog
{
    public WorkoutLogId Id { get; init; }
    public UserId UserId { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EndTime { get; init; }
    
    // Navigation properties
    public User User { get; init; } = null!;
    public ICollection<WorkoutLogSet> Sets { get; init; } = new List<WorkoutLogSet>();
    
    private WorkoutLog() { }
    
    public static class Handler
    {
        public static WorkoutLog CreateNew(UserId userId, DateTime startTime) =>
            new()
            {
                Id = WorkoutLogId.New(),
                UserId = userId,
                StartTime = startTime
            };
        
        public static WorkoutLog Create(WorkoutLogId id, UserId userId, DateTime startTime, DateTime? endTime = null) =>
            new()
            {
                Id = id,
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime
            };
        
        public static WorkoutLog CompleteWorkout(WorkoutLog workoutLog, DateTime endTime) =>
            workoutLog with { EndTime = endTime };
    }
}
