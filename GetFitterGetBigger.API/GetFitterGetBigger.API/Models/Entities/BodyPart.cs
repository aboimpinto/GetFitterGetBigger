using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record BodyPart : ReferenceDataBase, IPureReference, IEmptyEntity<BodyPart>
{
    public BodyPartId BodyPartId { get; init; }
    
    public string Id => BodyPartId.ToString();
    
    public bool IsEmpty => BodyPartId.IsEmpty;
    
    private BodyPart() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static BodyPart Empty { get; } = new()
    {
        BodyPartId = BodyPartId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<BodyPart> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                BodyPartId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<BodyPart> Create(
            BodyPartId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<BodyPart>()
                .EnsureNotEmpty(value, ReferenceDataErrorMessages.ValueCannotBeEmpty)
                .EnsureMinValue(displayOrder, 0, ReferenceDataErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new BodyPart
                {
                    BodyPartId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}
