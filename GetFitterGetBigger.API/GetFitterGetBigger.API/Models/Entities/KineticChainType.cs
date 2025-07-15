using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;

namespace GetFitterGetBigger.API.Models.Entities;

public record KineticChainType : ReferenceDataBase, IPureReference, IEmptyEntity<KineticChainType>
{
    public KineticChainTypeId KineticChainTypeId { get; init; }
    
    public string Id => KineticChainTypeId.ToString();
    
    public bool IsEmpty => KineticChainTypeId.IsEmpty;
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    private KineticChainType() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static KineticChainType Empty { get; } = new()
    {
        KineticChainTypeId = KineticChainTypeId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    public static class Handler
    {
        public static EntityResult<KineticChainType> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                KineticChainTypeId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<KineticChainType> Create(
            KineticChainTypeId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<KineticChainType>()
                .EnsureNotEmpty(value, KineticChainTypeErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, KineticChainTypeErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new KineticChainType
                {
                    KineticChainTypeId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}
