using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record BodyPart : ReferenceDataBase
{
    public BodyPartId Id { get; init; }
    
    private BodyPart() { }
    
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
                Id = BodyPartId.New(),
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
                Id = id,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
    }
}
