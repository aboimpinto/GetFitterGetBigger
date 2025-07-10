using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseWeightType : ReferenceDataBase
{
    public ExerciseWeightTypeId Id { get; init; }
    public string Code { get; init; } = string.Empty;
    
    private ExerciseWeightType() { }
    
    public static class Handler
    {
        public static ExerciseWeightType CreateNew(
            string code,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Code cannot be empty", nameof(code));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = ExerciseWeightTypeId.New(),
                Code = code,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static ExerciseWeightType Create(
            ExerciseWeightTypeId id,
            string code,
            string value,
            string? description,
            int displayOrder,
            bool isActive = true) =>
            new()
            {
                Id = id,
                Code = code,
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
    }
}