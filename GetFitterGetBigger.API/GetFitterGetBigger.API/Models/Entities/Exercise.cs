using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record Exercise
{
    public ExerciseId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? VideoUrl { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsUnilateral { get; init; }
    public bool IsActive { get; init; } = true;
    public DifficultyLevelId DifficultyId { get; init; }
    public KineticChainTypeId? KineticChainId { get; init; }
    public ExerciseWeightTypeId? ExerciseWeightTypeId { get; init; }
    
    // Navigation properties
    public DifficultyLevel? Difficulty { get; init; }
    public KineticChainType? KineticChain { get; init; }
    public ExerciseWeightType? ExerciseWeightType { get; init; }
    public ICollection<CoachNote> CoachNotes { get; init; } = new List<CoachNote>();
    public ICollection<ExerciseExerciseType> ExerciseExerciseTypes { get; init; } = new List<ExerciseExerciseType>();
    
    // Many-to-many relationships
    public ICollection<ExerciseMuscleGroup> ExerciseMuscleGroups { get; init; } = new List<ExerciseMuscleGroup>();
    public ICollection<ExerciseEquipment> ExerciseEquipment { get; init; } = new List<ExerciseEquipment>();
    public ICollection<ExerciseBodyPart> ExerciseBodyParts { get; init; } = new List<ExerciseBodyPart>();
    public ICollection<ExerciseMovementPattern> ExerciseMovementPatterns { get; init; } = new List<ExerciseMovementPattern>();
    
    // Private constructor to force usage of Handler
    private Exercise() { }
    
    /// <summary>
    /// Indicates if this is an empty/null object instance
    /// </summary>
    public bool IsEmpty => Id.IsEmpty;
    
    /// <summary>
    /// Static factory for creating an empty Exercise instance
    /// </summary>
    public static Exercise Empty => new() 
    { 
        Id = ExerciseId.Empty,
        Name = string.Empty,
        Description = string.Empty,
        DifficultyId = DifficultyLevelId.Empty,
        IsActive = false
    };
    
    public static class Handler
    {
        public static Exercise CreateNew(
            string name,
            string description,
            string? videoUrl,
            string? imageUrl,
            bool isUnilateral,
            DifficultyLevelId difficultyId,
            KineticChainTypeId? kineticChainId = null,
            ExerciseWeightTypeId? exerciseWeightTypeId = null)
        {
            // Validation logic
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description cannot be empty", nameof(description));
            }
            
            if (name.Length > 200)
            {
                throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));
            }
            
            if (description.Length > 1000)
            {
                throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
            }
            
            // Validate URLs if provided
            if (!string.IsNullOrEmpty(videoUrl) && !Uri.IsWellFormedUriString(videoUrl, UriKind.Absolute))
            {
                throw new ArgumentException("Video URL must be a valid absolute URL", nameof(videoUrl));
            }
            
            if (!string.IsNullOrEmpty(imageUrl) && !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                throw new ArgumentException("Image URL must be a valid absolute URL", nameof(imageUrl));
            }
            
            return new Exercise
            {
                Id = ExerciseId.New(),
                Name = name.Trim(),
                Description = description.Trim(),
                VideoUrl = videoUrl?.Trim(),
                ImageUrl = imageUrl?.Trim(),
                IsUnilateral = isUnilateral,
                IsActive = true,
                DifficultyId = difficultyId,
                KineticChainId = kineticChainId,
                ExerciseWeightTypeId = exerciseWeightTypeId
            };
        }
        
        public static Exercise Create(
            ExerciseId id,
            string name,
            string description,
            string? videoUrl,
            string? imageUrl,
            bool isUnilateral,
            bool isActive,
            DifficultyLevelId difficultyId,
            KineticChainTypeId? kineticChainId = null,
            ExerciseWeightTypeId? exerciseWeightTypeId = null)
        {
            return new Exercise
            {
                Id = id,
                Name = name,
                Description = description,
                VideoUrl = videoUrl,
                ImageUrl = imageUrl,
                IsUnilateral = isUnilateral,
                IsActive = isActive,
                DifficultyId = difficultyId,
                KineticChainId = kineticChainId,
                ExerciseWeightTypeId = exerciseWeightTypeId
            };
        }
    }
}