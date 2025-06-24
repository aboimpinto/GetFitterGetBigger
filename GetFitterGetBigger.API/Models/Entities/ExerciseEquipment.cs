using System;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record ExerciseEquipment
{
    public ExerciseId ExerciseId { get; init; }
    public EquipmentId EquipmentId { get; init; }
    
    // Navigation properties
    public Exercise Exercise { get; init; } = null!;
    public Equipment Equipment { get; init; } = null!;
    
    private ExerciseEquipment() { }
    
    public static class Handler
    {
        public static ExerciseEquipment Create(ExerciseId exerciseId, EquipmentId equipmentId) =>
            new()
            {
                ExerciseId = exerciseId,
                EquipmentId = equipmentId
            };
    }
}
