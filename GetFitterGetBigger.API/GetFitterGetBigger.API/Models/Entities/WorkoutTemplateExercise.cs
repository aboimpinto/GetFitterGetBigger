using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutTemplateExercise : IEmptyEntity<WorkoutTemplateExercise>
{
    public WorkoutTemplateExerciseId Id { get; init; }
    
    // IEntity implementation
    string IEntity.Id => Id.ToString();
    bool IEntity.IsActive => true; // Always active
    
    public WorkoutTemplateId WorkoutTemplateId { get; init; }
    public ExerciseId ExerciseId { get; init; }
    public WorkoutZone Zone { get; init; }
    public int SequenceOrder { get; init; }
    public string? Notes { get; init; }
    
    // Navigation properties
    public WorkoutTemplate? WorkoutTemplate { get; init; }
    public Exercise? Exercise { get; init; }
    public ICollection<SetConfiguration> Configurations { get; init; } = new List<SetConfiguration>();
    
    // Private constructor to force usage of Handler
    private WorkoutTemplateExercise() { }
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Id.IsEmpty;
    
    /// <summary>
    /// Static factory for creating an empty WorkoutTemplateExercise instance
    /// </summary>
    public static WorkoutTemplateExercise Empty => new() 
    { 
        Id = WorkoutTemplateExerciseId.Empty,
        WorkoutTemplateId = WorkoutTemplateId.Empty,
        ExerciseId = ExerciseId.Empty,
        Zone = WorkoutZone.Main,
        SequenceOrder = 0
    };
    
    public static class Handler
    {
        public static EntityResult<WorkoutTemplateExercise> CreateNew(
            WorkoutTemplateId workoutTemplateId,
            ExerciseId exerciseId,
            WorkoutZone zone,
            int sequenceOrder,
            string? notes = null)
        {
            return Create(
                WorkoutTemplateExerciseId.New(),
                workoutTemplateId,
                exerciseId,
                zone,
                sequenceOrder,
                notes
            );
        }
        
        public static EntityResult<WorkoutTemplateExercise> Create(
            WorkoutTemplateExerciseId id,
            WorkoutTemplateId workoutTemplateId,
            ExerciseId exerciseId,
            WorkoutZone zone,
            int sequenceOrder,
            string? notes = null)
        {
            return Validate.For<WorkoutTemplateExercise>()
                .Ensure(() => !workoutTemplateId.IsEmpty, "Workout template ID cannot be empty")
                .Ensure(() => !exerciseId.IsEmpty, "Exercise ID cannot be empty")
                .EnsureMinValue(sequenceOrder, 1, "Sequence order must be at least 1")
                .Ensure(() => notes == null || notes.Length <= 500, "Notes cannot exceed 500 characters")
                .OnSuccess(() => new WorkoutTemplateExercise
                {
                    Id = id,
                    WorkoutTemplateId = workoutTemplateId,
                    ExerciseId = exerciseId,
                    Zone = zone,
                    SequenceOrder = sequenceOrder,
                    Notes = notes?.Trim()
                });
        }
        
        public static EntityResult<WorkoutTemplateExercise> UpdateSequenceOrder(WorkoutTemplateExercise exercise, int newSequenceOrder)
        {
            return Validate.For<WorkoutTemplateExercise>()
                .EnsureMinValue(newSequenceOrder, 1, "Sequence order must be at least 1")
                .OnSuccess(() => exercise with { SequenceOrder = newSequenceOrder });
        }
        
        public static EntityResult<WorkoutTemplateExercise> UpdateNotes(WorkoutTemplateExercise exercise, string? notes)
        {
            return Validate.For<WorkoutTemplateExercise>()
                .Ensure(() => notes == null || notes.Length <= 500, "Notes cannot exceed 500 characters")
                .OnSuccess(() => exercise with { Notes = notes?.Trim() });
        }
        
        public static EntityResult<WorkoutTemplateExercise> ChangeZone(WorkoutTemplateExercise exercise, WorkoutZone newZone, int newSequenceOrder)
        {
            return Validate.For<WorkoutTemplateExercise>()
                .EnsureMinValue(newSequenceOrder, 1, "Sequence order must be at least 1")
                .OnSuccess(() => exercise with 
                { 
                    Zone = newZone,
                    SequenceOrder = newSequenceOrder
                });
        }
    }
}

public enum WorkoutZone
{
    Warmup = 1,
    Main = 2,
    Cooldown = 3
}