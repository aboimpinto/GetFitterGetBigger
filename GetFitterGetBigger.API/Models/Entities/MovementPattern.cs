using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record MovementPattern
{
    public MovementPatternId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    
    // Navigation properties
    public ICollection<ExerciseMovementPattern> Exercises { get; init; } = new List<ExerciseMovementPattern>();
    
    private MovementPattern() { }
    
    public static class Handler
    {
        public static MovementPattern CreateNew(string name, string? description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new()
            {
                Id = MovementPatternId.New(),
                Name = name,
                Description = description
            };
        }
        
        public static MovementPattern Create(MovementPatternId id, string name, string? description = null) =>
            new()
            {
                Id = id,
                Name = name,
                Description = description
            };
    }
}
