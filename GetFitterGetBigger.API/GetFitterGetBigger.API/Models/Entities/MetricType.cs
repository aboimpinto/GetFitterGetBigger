using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record MetricType : ReferenceDataBase, IPureReference, IEmptyEntity<MetricType>
{
    public MetricTypeId MetricTypeId { get; init; }
    
    public string Id => MetricTypeId.ToString();
    
    public bool IsEmpty => MetricTypeId.IsEmpty;
    
    // Navigation properties
    public ICollection<ExerciseMetricSupport> Exercises { get; init; } = new List<ExerciseMetricSupport>();
    
    private MetricType() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static MetricType Empty { get; } = new()
    {
        MetricTypeId = MetricTypeId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<MetricType> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                MetricTypeId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<MetricType> Create(
            MetricTypeId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<MetricType>()
                .EnsureNotEmpty(value, MetricTypeErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, MetricTypeErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new MetricType
                {
                    MetricTypeId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}
