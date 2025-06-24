using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record MuscleGroup
{
    public MuscleGroupId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public BodyPartId BodyPartId { get; init; }
    
    // Navigation properties
    public BodyPart? BodyPart { get; init; }
    public ICollection<ExerciseTargetedMuscle> Exercises { get; init; } = new List<ExerciseTargetedMuscle>();
    
    private MuscleGroup() { }
    
    public static class Handler
    {
        public static MuscleGroup CreateNew(string name, BodyPartId bodyPartId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new()
            {
                Id = MuscleGroupId.New(),
                Name = name,
                BodyPartId = bodyPartId
            };
        }
        
        public static MuscleGroup Create(MuscleGroupId id, string name, BodyPartId bodyPartId) =>
            new()
            {
                Id = id,
                Name = name,
                BodyPartId = bodyPartId
            };
    }
}
