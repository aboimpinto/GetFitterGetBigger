using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record DifficultyLevel : ReferenceDataBase
{
    public DifficultyLevelId Id { get; init; }
    
    // Navigation properties
    public ICollection<Exercise> Exercises { get; init; } = new List<Exercise>();
    
    private DifficultyLevel() { }
    
    public static class Handler
    {
        public static DifficultyLevel CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = DifficultyLevelId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static DifficultyLevel Create(
            DifficultyLevelId id,
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
