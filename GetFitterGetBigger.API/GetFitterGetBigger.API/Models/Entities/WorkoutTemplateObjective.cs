using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutTemplateObjective : IEmptyEntity<WorkoutTemplateObjective>
{
    public WorkoutTemplateId WorkoutTemplateId { get; init; }
    public WorkoutObjectiveId WorkoutObjectiveId { get; init; }
    
    // IEntity implementation
    public string Id => $"{WorkoutTemplateId}_{WorkoutObjectiveId}"; // Composite key
    public bool IsActive => true; // Always active
    
    // Navigation properties
    public WorkoutTemplate? WorkoutTemplate { get; init; }
    public WorkoutObjective? WorkoutObjective { get; init; }
    
    private WorkoutTemplateObjective() { }
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => WorkoutTemplateId.IsEmpty && WorkoutObjectiveId.IsEmpty;
    
    /// <summary>
    /// Static factory for creating an empty WorkoutTemplateObjective instance
    /// </summary>
    public static WorkoutTemplateObjective Empty => new() 
    { 
        WorkoutTemplateId = WorkoutTemplateId.Empty,
        WorkoutObjectiveId = WorkoutObjectiveId.Empty
    };
    
    public static class Handler
    {
        public static EntityResult<WorkoutTemplateObjective> Create(WorkoutTemplateId workoutTemplateId, WorkoutObjectiveId workoutObjectiveId) =>
            Validate.For<WorkoutTemplateObjective>()
                .Ensure(() => !workoutTemplateId.IsEmpty, "Workout template ID cannot be empty")
                .Ensure(() => !workoutObjectiveId.IsEmpty, "Workout objective ID cannot be empty")
                .OnSuccess(() => new WorkoutTemplateObjective
                {
                    WorkoutTemplateId = workoutTemplateId,
                    WorkoutObjectiveId = workoutObjectiveId
                });
    }
}