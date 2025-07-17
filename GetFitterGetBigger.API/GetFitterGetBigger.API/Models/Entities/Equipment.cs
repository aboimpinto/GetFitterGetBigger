using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record Equipment : IEnhancedReference, IEmptyEntity<Equipment>
{
    public EquipmentId EquipmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // IEntity implementation (through IEnhancedReference)
    public string Id => EquipmentId.ToString();
    
    // IEnhancedReference implementation
    public string Value => Name;
    public string? Description => null; // Equipment doesn't have descriptions
    
    // ITrackedEntity implementation (through IEnhancedReference)
    DateTime ITrackedEntity.UpdatedAt => UpdatedAt ?? CreatedAt;
    
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
    
    // ICacheableEntity implementation (through IEnhancedReference)
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Invalidatable;
    public TimeSpan? GetCacheDuration() => TimeSpan.FromHours(1);
    
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
