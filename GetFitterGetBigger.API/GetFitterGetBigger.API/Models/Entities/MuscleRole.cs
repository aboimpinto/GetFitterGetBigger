using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record MuscleRole : ReferenceDataBase
{
    public MuscleRoleId Id { get; init; }
    
    private MuscleRole() { }
    
    public static class Handler
    {
        public static MuscleRole CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = MuscleRoleId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static MuscleRole Create(
            MuscleRoleId id,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true) =>
            new()
            {
                Id = id,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
    }
}
