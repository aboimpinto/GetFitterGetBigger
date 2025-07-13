using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record WorkoutObjective : ReferenceDataBase
{
    public WorkoutObjectiveId Id { get; init; }
    
    private WorkoutObjective() { }
    
    public static class Handler
    {
        public static WorkoutObjective CreateNew(
            string value,
            string? description,
            int displayOrder,
            bool isActive = true)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));
                
            return new()
            {
                Id = WorkoutObjectiveId.New(),
                Value = value,
                Description = description,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };
        }
        
        public static WorkoutObjective Create(
            WorkoutObjectiveId id,
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
            
        public static WorkoutObjective Update(
            WorkoutObjective objective,
            string? value = null,
            string? description = null,
            int? displayOrder = null,
            bool? isActive = null) =>
            objective with
            {
                Value = value ?? objective.Value,
                Description = description ?? objective.Description,
                DisplayOrder = displayOrder ?? objective.DisplayOrder,
                IsActive = isActive ?? objective.IsActive
            };
            
        public static WorkoutObjective Deactivate(WorkoutObjective objective) =>
            objective with { IsActive = false };
    }
}