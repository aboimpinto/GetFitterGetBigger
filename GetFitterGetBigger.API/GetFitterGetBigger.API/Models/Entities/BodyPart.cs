using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

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
        public static BodyPart CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                BodyPartId = BodyPartId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static BodyPart Create(
            BodyPartId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true) =>
            new()
            {
                BodyPartId = id,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
    }
}
