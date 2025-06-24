using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record KineticChainType : ReferenceDataBase
{
    public KineticChainTypeId Id { get; init; }
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    private KineticChainType() { }
    
    public static class Handler
    {
        public static KineticChainType CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = KineticChainTypeId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static KineticChainType Create(
            KineticChainTypeId id,
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
