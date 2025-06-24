using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record Equipment
{
    public EquipmentId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    
    // Navigation properties
    public ICollection<ExerciseEquipment> Exercises { get; init; } = new List<ExerciseEquipment>();
    
    private Equipment() { }
    
    public static class Handler
    {
        public static Equipment CreateNew(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new()
            {
                Id = EquipmentId.New(),
                Name = name
            };
        }
        
        public static Equipment Create(EquipmentId id, string name) =>
            new()
            {
                Id = id,
                Name = name
            };
    }
}
