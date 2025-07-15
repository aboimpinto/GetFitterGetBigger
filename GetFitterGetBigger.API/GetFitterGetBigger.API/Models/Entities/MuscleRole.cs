using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.Validation;
using GetFitterGetBigger.API.Services.Interfaces;

namespace GetFitterGetBigger.API.Models.Entities;

public record MuscleRole : ReferenceDataBase, IPureReference, IEmptyEntity<MuscleRole>
{
    public MuscleRoleId MuscleRoleId { get; init; }
    public string Id => MuscleRoleId.ToString();
    
    public bool IsEmpty => MuscleRoleId.IsEmpty;
    
    public static MuscleRole Empty { get; } = new()
    {
        MuscleRoleId = MuscleRoleId.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
    
    private MuscleRole() { }
    
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Eternal;
    
    public TimeSpan? GetCacheDuration() => null; // Eternal caching
    
    public static class Handler
    {
        public static EntityResult<MuscleRole> CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Create(
                MuscleRoleId.New(),
                value,
                description,
                displayOrder,
                isActive
            );
        }
        
        public static EntityResult<MuscleRole> Create(
            MuscleRoleId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            return Validate.For<MuscleRole>()
                .EnsureNotEmpty(value, MuscleRoleErrorMessages.ValueCannotBeEmptyEntity)
                .EnsureMinValue(displayOrder, 0, MuscleRoleErrorMessages.DisplayOrderMustBeNonNegative)
                .OnSuccess(() => new MuscleRole
                {
                    MuscleRoleId = id,
                    Value = value,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = isActive
                });
        }
    }
}
