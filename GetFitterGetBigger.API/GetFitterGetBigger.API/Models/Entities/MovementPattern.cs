using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

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
        public static MovementPattern CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                MovementPatternId = MovementPatternId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static MovementPattern Create(
            MovementPatternId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true) =>
            new()
            {
                MovementPatternId = id,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
    }
}
