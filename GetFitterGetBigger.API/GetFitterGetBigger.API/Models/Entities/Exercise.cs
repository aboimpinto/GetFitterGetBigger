using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Models.Entities;

public record Exercise
{
    public ExerciseId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public string? VideoUrl { get; init; }
    public DifficultyLevelId DifficultyLevelId { get; init; }
    public KineticChainTypeId KineticChainTypeId { get; init; }
    public bool IsUnilateral { get; init; } = false;
    
    // Navigation properties
    public DifficultyLevel? DifficultyLevel { get; init; }
    public KineticChainType? KineticChainType { get; init; }
    public ICollection<ExerciseMovementPattern> MovementPatterns { get; init; } = new List<ExerciseMovementPattern>();
    public ICollection<ExerciseTargetedMuscle> TargetedMuscles { get; init; } = new List<ExerciseTargetedMuscle>();
    public ICollection<ExerciseEquipment> Equipment { get; init; } = new List<ExerciseEquipment>();
    public ICollection<ExerciseMetricSupport> SupportedMetrics { get; init; } = new List<ExerciseMetricSupport>();
    public ICollection<WorkoutLogSet> WorkoutLogSets { get; init; } = new List<WorkoutLogSet>();
    
    private Exercise() { }
    
    public static class Handler
    {
        public static Exercise CreateNew(
            string name, 
            string description, 
            string instructions, 
            string? videoUrl, 
            DifficultyLevelId difficultyLevelId, 
            KineticChainTypeId kineticChainTypeId, 
            bool isUnilateral)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }
            
            return new()
            {
                Id = ExerciseId.New(),
                Name = name,
                Description = description,
                Instructions = instructions,
                VideoUrl = videoUrl,
                DifficultyLevelId = difficultyLevelId,
                KineticChainTypeId = kineticChainTypeId,
                IsUnilateral = isUnilateral
            };
        }
        
        public static Exercise Create(
            ExerciseId id,
            string name, 
            string description, 
            string instructions, 
            string? videoUrl, 
            DifficultyLevelId difficultyLevelId, 
            KineticChainTypeId kineticChainTypeId, 
            bool isUnilateral) =>
            
            new()
            {
                Id = id,
                Name = name,
                Description = description,
                Instructions = instructions,
                VideoUrl = videoUrl,
                DifficultyLevelId = difficultyLevelId,
                KineticChainTypeId = kineticChainTypeId,
                IsUnilateral = isUnilateral
            };
    }
}
