using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record MuscleGroup
{
    public MuscleGroupId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public BodyPart BodyPart { get; init; } = BodyPart.Core;
    
    // Navigation properties
    public ICollection<ExerciseTargetedMuscle> Exercises { get; init; } = new List<ExerciseTargetedMuscle>();
    
    private MuscleGroup() { }
    
    public static class Handler
    {
        public static MuscleGroup CreateNew(string name, BodyPart bodyPart)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new()
            {
                Id = MuscleGroupId.New(),
                Name = name,
                BodyPart = bodyPart
            };
        }
        
        public static MuscleGroup Create(MuscleGroupId id, string name, BodyPart bodyPart) =>
            new()
            {
                Id = id,
                Name = name,
                BodyPart = bodyPart
            };
    }
}
