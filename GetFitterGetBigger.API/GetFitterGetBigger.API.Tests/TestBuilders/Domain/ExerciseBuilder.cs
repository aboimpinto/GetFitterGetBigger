using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

public class ExerciseBuilder
{
    private ExerciseId _id = ExerciseId.New();
    private string _name = "Test Exercise";
    private string _description = "Test Description";
    private string? _videoUrl = null;
    private string? _imageUrl = null;
    private bool _isUnilateral = false;
    private bool _isActive = true;
    private DifficultyLevelId _difficultyId = DifficultyLevelId.New();
    private KineticChainTypeId? _kineticChainId = null;
    private ExerciseWeightTypeId? _exerciseWeightTypeId = null;
    
    // Navigation properties
    private DifficultyLevel? _difficulty = null;
    private KineticChainType? _kineticChain = null;
    private ExerciseWeightType? _exerciseWeightType = null;
    private List<CoachNote> _coachNotes = new List<CoachNote>();
    private List<ExerciseExerciseType> _exerciseExerciseTypes = new List<ExerciseExerciseType>();
    private List<ExerciseMuscleGroup> _exerciseMuscleGroups = new List<ExerciseMuscleGroup>();
    private List<ExerciseEquipment> _exerciseEquipment = new List<ExerciseEquipment>();
    private List<ExerciseBodyPart> _exerciseBodyParts = new List<ExerciseBodyPart>();
    private List<ExerciseMovementPattern> _exerciseMovementPatterns = new List<ExerciseMovementPattern>();

    // Common presets
    public static ExerciseBuilder AnExercise() => new ExerciseBuilder();
    
    public static ExerciseBuilder AWorkoutExercise() => new ExerciseBuilder()
        .WithName("Bench Press")
        .WithDescription("Chest exercise performed on a bench")
        .WithKineticChainId(KineticChainTypeId.New())
        .AsActive();
    
    public static ExerciseBuilder ARestExercise() => new ExerciseBuilder()
        .WithName("Rest Period")
        .WithDescription("Rest between sets")
        .WithoutKineticChain()
        .WithoutExerciseWeightType()
        .AsActive();
    
    public static ExerciseBuilder AUnilateralExercise() => new ExerciseBuilder()
        .WithName("Single Arm Dumbbell Row")
        .WithDescription("Unilateral back exercise")
        .AsUnilateral()
        .WithKineticChainId(KineticChainTypeId.New())
        .AsActive();
    
    public static ExerciseBuilder AnInactiveExercise() => new ExerciseBuilder()
        .WithName("Deprecated Exercise")
        .WithDescription("This exercise is no longer used")
        .AsInactive();

    // ID management
    public ExerciseBuilder WithId(ExerciseId id)
    {
        _id = id;
        return this;
    }
    
    public ExerciseBuilder WithNewId()
    {
        _id = ExerciseId.New();
        return this;
    }

    // Basic properties
    public ExerciseBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ExerciseBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }
    
    public ExerciseBuilder WithVideoUrl(string videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }
    
    public ExerciseBuilder WithImageUrl(string imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }
    
    public ExerciseBuilder AsUnilateral()
    {
        _isUnilateral = true;
        return this;
    }
    
    public ExerciseBuilder AsBilateral()
    {
        _isUnilateral = false;
        return this;
    }
    
    public ExerciseBuilder AsActive()
    {
        _isActive = true;
        return this;
    }
    
    public ExerciseBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    // Required relationships
    public ExerciseBuilder WithDifficultyId(DifficultyLevelId difficultyId)
    {
        _difficultyId = difficultyId;
        return this;
    }
    
    public ExerciseBuilder WithDifficulty(DifficultyLevel difficulty)
    {
        _difficulty = difficulty;
        _difficultyId = difficulty.DifficultyLevelId;
        return this;
    }

    // Optional relationships
    public ExerciseBuilder WithKineticChainId(KineticChainTypeId kineticChainId)
    {
        _kineticChainId = kineticChainId;
        return this;
    }
    
    public ExerciseBuilder WithoutKineticChain()
    {
        _kineticChainId = null;
        _kineticChain = null;
        return this;
    }
    
    public ExerciseBuilder WithKineticChain(KineticChainType kineticChain)
    {
        _kineticChain = kineticChain;
        _kineticChainId = kineticChain.KineticChainTypeId;
        return this;
    }
    
    public ExerciseBuilder WithExerciseWeightTypeId(ExerciseWeightTypeId exerciseWeightTypeId)
    {
        _exerciseWeightTypeId = exerciseWeightTypeId;
        return this;
    }
    
    public ExerciseBuilder WithoutExerciseWeightType()
    {
        _exerciseWeightTypeId = null;
        _exerciseWeightType = null;
        return this;
    }
    
    public ExerciseBuilder WithExerciseWeightType(ExerciseWeightType exerciseWeightType)
    {
        _exerciseWeightType = exerciseWeightType;
        _exerciseWeightTypeId = exerciseWeightType.Id;
        return this;
    }

    // Collections
    public ExerciseBuilder WithCoachNote(CoachNote coachNote)
    {
        _coachNotes.Add(coachNote);
        return this;
    }
    
    public ExerciseBuilder WithCoachNotes(params CoachNote[] coachNotes)
    {
        _coachNotes.AddRange(coachNotes);
        return this;
    }
    
    public ExerciseBuilder WithExerciseType(ExerciseExerciseType exerciseType)
    {
        _exerciseExerciseTypes.Add(exerciseType);
        return this;
    }
    
    public ExerciseBuilder WithExerciseTypes(params ExerciseExerciseType[] exerciseTypes)
    {
        _exerciseExerciseTypes.AddRange(exerciseTypes);
        return this;
    }
    
    /// <summary>
    /// Convenience method to add exercise types by string name (for testing purposes)
    /// Creates ExerciseTypeId from string and associates with the exercise being built
    /// </summary>
    public ExerciseBuilder WithExerciseTypes(params string[] exerciseTypeNames)
    {
        foreach (var typeName in exerciseTypeNames)
        {
            // For testing purposes, create a mock ExerciseTypeId based on the string
            // In real scenarios, you'd look up the actual ExerciseTypeId from the database
            var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(typeName);
            if (exerciseTypeId.IsEmpty)
            {
                // If parsing fails, create a new ID for testing
                exerciseTypeId = ExerciseTypeId.New();
            }
            
            var exerciseType = ExerciseExerciseType.Handler.Create(_id, exerciseTypeId);
            _exerciseExerciseTypes.Add(exerciseType);
        }
        return this;
    }
    
    public ExerciseBuilder WithMuscleGroup(ExerciseMuscleGroup muscleGroup)
    {
        _exerciseMuscleGroups.Add(muscleGroup);
        return this;
    }
    
    public ExerciseBuilder WithMuscleGroups(params ExerciseMuscleGroup[] muscleGroups)
    {
        _exerciseMuscleGroups.AddRange(muscleGroups);
        return this;
    }
    
    public ExerciseBuilder WithEquipment(ExerciseEquipment equipment)
    {
        _exerciseEquipment.Add(equipment);
        return this;
    }
    
    public ExerciseBuilder WithEquipmentList(params ExerciseEquipment[] equipment)
    {
        _exerciseEquipment.AddRange(equipment);
        return this;
    }
    
    public ExerciseBuilder WithBodyPart(ExerciseBodyPart bodyPart)
    {
        _exerciseBodyParts.Add(bodyPart);
        return this;
    }
    
    public ExerciseBuilder WithBodyParts(params ExerciseBodyPart[] bodyParts)
    {
        _exerciseBodyParts.AddRange(bodyParts);
        return this;
    }
    
    public ExerciseBuilder WithMovementPattern(ExerciseMovementPattern movementPattern)
    {
        _exerciseMovementPatterns.Add(movementPattern);
        return this;
    }
    
    public ExerciseBuilder WithMovementPatterns(params ExerciseMovementPattern[] movementPatterns)
    {
        _exerciseMovementPatterns.AddRange(movementPatterns);
        return this;
    }

    // Build method
    public Exercise Build()
    {
        var exercise = Exercise.Handler.Create(
            _id,
            _name,
            _description,
            _videoUrl,
            _imageUrl,
            _isUnilateral,
            _isActive,
            _difficultyId,
            _kineticChainId,
            _exerciseWeightTypeId);
        
        // Since Exercise uses init-only properties, we need to create a new instance with all navigation properties
        // This requires reflection or a factory method that accepts all properties
        // For now, we'll return the exercise as-is and tests can set up mocks to return exercises with navigation properties
        
        return exercise;
    }
    
    // Build with navigation properties (for when you need a fully loaded exercise)
    public Exercise BuildWithNavigationProperties()
    {
        var exercise = Build();
        
        // This would require either:
        // 1. A new factory method in Exercise.Handler that accepts navigation properties
        // 2. Using reflection to set init-only properties
        // 3. Creating a test-specific derived type
        // For now, tests should mock repository methods to return exercises with navigation properties
        
        return exercise;
    }
}