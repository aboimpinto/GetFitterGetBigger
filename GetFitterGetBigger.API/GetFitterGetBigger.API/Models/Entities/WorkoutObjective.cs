using System;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutObjective : ReferenceDataBase, IPureReference, IEmptyEntity<WorkoutObjective>
{
    public WorkoutObjectiveId WorkoutObjectiveId { get; init; }
    
    public string Id => WorkoutObjectiveId.ToString();
    
    public bool IsEmpty => WorkoutObjectiveId.IsEmpty;
    
    private WorkoutObjective() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static WorkoutObjective Empty { get; } = new()
    {
        WorkoutObjectiveId = WorkoutObjectiveId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<WorkoutObjective> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                WorkoutObjectiveId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<WorkoutObjective> Create(
            WorkoutObjectiveId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<WorkoutObjective>()
                .EnsureNotEmpty(value, WorkoutObjectiveErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, WorkoutObjectiveErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new WorkoutObjective
                {
                    WorkoutObjectiveId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}