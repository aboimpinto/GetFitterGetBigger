using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record MovementPattern : ReferenceDataBase, IPureReference, IEmptyEntity<MovementPattern>
{
    public MovementPatternId MovementPatternId { get; init; }
    
    public string Id => MovementPatternId.ToString();
    
    public bool IsEmpty => MovementPatternId.IsEmpty;
    
    // Navigation properties
    public ICollection<ExerciseMovementPattern> Exercises { get; init; } = new List<ExerciseMovementPattern>();
    
    private MovementPattern() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static MovementPattern Empty { get; } = new()
    {
        MovementPatternId = MovementPatternId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<MovementPattern> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                MovementPatternId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<MovementPattern> Create(
            MovementPatternId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<MovementPattern>()
                .EnsureNotEmpty(value, MovementPatternErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, MovementPatternErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new MovementPattern
                {
                    MovementPatternId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}
