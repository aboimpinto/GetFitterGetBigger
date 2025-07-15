using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record DifficultyLevel : ReferenceDataBase, IPureReference, IEmptyEntity<DifficultyLevel>
{
    public DifficultyLevelId DifficultyLevelId { get; init; }
    
    // IEntity.Id implementation
    public string Id => DifficultyLevelId.ToString();
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    // IEmptyEntity implementation
    public bool IsEmpty => DifficultyLevelId.IsEmpty;
    
    public static DifficultyLevel Empty { get; } = new()
    {
        DifficultyLevelId = DifficultyLevelId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    // ICacheableEntity implementation
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    private DifficultyLevel() { }
    
    public static class Handler
    {
        public static EntityResult<DifficultyLevel> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                DifficultyLevelId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<DifficultyLevel> Create(
            DifficultyLevelId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<DifficultyLevel>()
                .EnsureNotEmpty(value, DifficultyLevelErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, DifficultyLevelErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new DifficultyLevel
                {
                    DifficultyLevelId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}
