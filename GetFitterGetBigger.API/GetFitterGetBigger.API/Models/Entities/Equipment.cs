using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record Equipment : IEmptyEntity<Equipment, EquipmentId>
{
    public EquipmentId EquipmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // IEntity<EquipmentId> implementation
    public EquipmentId Id => EquipmentId;
    
    // IEntity implementation (backward compatibility)
    string IEntity.Id => EquipmentId.ToString();
    
    // IEmptyEntity implementation
    public bool IsEmpty => EquipmentId.IsEmpty;
    
    public static Equipment Empty { get; } = new()
    {
        EquipmentId = EquipmentId.Empty,
        Name = string.Empty,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = null
    };
    
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
                EquipmentId = EquipmentId.New(),
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }
        
        public static Equipment Create(EquipmentId id, string name, bool isActive = true, DateTime? createdAt = null, DateTime? updatedAt = null) =>
            new()
            {
                EquipmentId = id,
                Name = name,
                IsActive = isActive,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                UpdatedAt = updatedAt
            };
            
        public static Equipment Update(Equipment existing, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return existing with
            {
                Name = name,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        public static Equipment Deactivate(Equipment existing)
        {
            return existing with
            {
                IsActive = false,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
