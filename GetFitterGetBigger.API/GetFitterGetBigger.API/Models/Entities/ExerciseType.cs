using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseType : ReferenceDataBase
{
    public ExerciseTypeId Id { get; init; }
    
    private ExerciseType() { }
    
    public static class Handler
    {
        public static ExerciseType CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = ExerciseTypeId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static ExerciseType Create(
            ExerciseTypeId id,
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