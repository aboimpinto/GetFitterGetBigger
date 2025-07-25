using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutState : ReferenceDataBase, IPureReference, IEmptyEntity<WorkoutState>
{
    public WorkoutStateId WorkoutStateId { get; init; }
    
    public string Id => WorkoutStateId.ToString();
    
    public bool IsEmpty => WorkoutStateId.IsEmpty;
    
    private WorkoutState() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static WorkoutState Empty { get; } = new()
    {
        WorkoutStateId = WorkoutStateId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<WorkoutState> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                WorkoutStateId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<WorkoutState> Create(
            WorkoutStateId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<WorkoutState>()
                .EnsureNotEmpty(value, WorkoutStateErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, WorkoutStateErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new WorkoutState
                {
                    WorkoutStateId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}