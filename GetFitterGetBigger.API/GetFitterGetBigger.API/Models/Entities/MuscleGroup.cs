using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record MuscleGroup : IEnhancedReference<MuscleGroupId>, IEmptyEntity<MuscleGroup, MuscleGroupId>
{
    public MuscleGroupId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public BodyPartId BodyPartId { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // IEntity implementation (backward compatibility)
    string IEntity.Id => Id.ToString();
    
    // IEnhancedReference implementation
    public string Value => Name;
    public string? Description => null; // MuscleGroup doesn't have descriptions
    
    // ITrackedEntity implementation (through IEnhancedReference)
    DateTime ITrackedEntity.UpdatedAt => UpdatedAt ?? CreatedAt;
    
    // IEmptyEntity implementation
    public bool IsEmpty => Id.IsEmpty;
    
    public static MuscleGroup Empty { get; } = new()
    {
        Id = MuscleGroupId.Empty,
        Name = string.Empty,
        BodyPartId = BodyPartId.Empty,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = null
    };
    
    // ICacheableEntity implementation (through IEnhancedReference)
    public CacheStrategy GetCacheStrategy() => CacheStrategy.Invalidatable;
    public TimeSpan? GetCacheDuration() => TimeSpan.FromHours(1);
    
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
                BodyPartId = bodyPartId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }
        
        public static MuscleGroup Create(MuscleGroupId id, string name, BodyPartId bodyPartId, bool isActive = true, DateTime? createdAt = null, DateTime? updatedAt = null) =>
            new()
            {
                Id = id,
                Name = name,
                BodyPartId = bodyPartId,
                IsActive = isActive,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                UpdatedAt = updatedAt
            };
            
        public static MuscleGroup Update(MuscleGroup existing, string name, BodyPartId bodyPartId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return existing with
            {
                Name = name,
                BodyPartId = bodyPartId,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        public static MuscleGroup Deactivate(MuscleGroup existing)
        {
            return existing with
            {
                IsActive = false,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
