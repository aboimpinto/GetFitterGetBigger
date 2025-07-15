using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseType : ReferenceDataBase, IPureReference, IEmptyEntity<ExerciseType>
{
    public ExerciseTypeId ExerciseTypeId { get; init; }
    
    public string Id => ExerciseTypeId.ToString();
    
    public bool IsEmpty => ExerciseTypeId.IsEmpty;
    
    private ExerciseType() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static ExerciseType Empty { get; } = new()
    {
        ExerciseTypeId = ExerciseTypeId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<ExerciseType> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                ExerciseTypeId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<ExerciseType> Create(
            ExerciseTypeId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<ExerciseType>()
                .EnsureNotEmpty(value, ExerciseTypeErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, ExerciseTypeErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new ExerciseType
                {
                    ExerciseTypeId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}