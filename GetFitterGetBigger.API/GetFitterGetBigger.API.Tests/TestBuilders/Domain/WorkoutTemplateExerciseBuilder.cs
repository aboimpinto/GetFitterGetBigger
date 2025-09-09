using System;
using System.Collections.Generic;
using System.Linq;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

public class WorkoutTemplateExerciseBuilder
{
    private WorkoutTemplateExerciseId _id = WorkoutTemplateExerciseId.New();
    private WorkoutTemplateId _workoutTemplateId = WorkoutTemplateId.New();
    private ExerciseId _exerciseId = ExerciseId.New();
    private WorkoutZone _zone = WorkoutZone.Main;
    private int _sequenceOrder = 1;
    private string? _notes = null;
    
    // Navigation properties
    private WorkoutTemplate? _workoutTemplate = null;
    private Exercise? _exercise = null;
    private List<SetConfiguration> _configurations = new List<SetConfiguration>();

    // Common presets
    public static WorkoutTemplateExerciseBuilder AWorkoutTemplateExercise() => new WorkoutTemplateExerciseBuilder();
    
    public static WorkoutTemplateExerciseBuilder AWarmupExercise() => new WorkoutTemplateExerciseBuilder()
        .WithZone(WorkoutZone.Warmup)
        .WithSequenceOrder(1)
        .WithNotes("Dynamic stretching and mobility work");
    
    public static WorkoutTemplateExerciseBuilder AMainExercise() => new WorkoutTemplateExerciseBuilder()
        .WithZone(WorkoutZone.Main)
        .WithSequenceOrder(1)
        .WithNotes("Focus on form and control");
    
    public static WorkoutTemplateExerciseBuilder ACooldownExercise() => new WorkoutTemplateExerciseBuilder()
        .WithZone(WorkoutZone.Cooldown)
        .WithSequenceOrder(1)
        .WithNotes("Static stretching and recovery");

    // ID management
    public WorkoutTemplateExerciseBuilder WithId(WorkoutTemplateExerciseId id)
    {
        _id = id;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithId(string id)
    {
        _id = WorkoutTemplateExerciseId.ParseOrEmpty(id);
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithNewId()
    {
        _id = WorkoutTemplateExerciseId.New();
        return this;
    }

    // Required relationships
    public WorkoutTemplateExerciseBuilder WithWorkoutTemplateId(WorkoutTemplateId workoutTemplateId)
    {
        _workoutTemplateId = workoutTemplateId;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithWorkoutTemplateId(string workoutTemplateId)
    {
        _workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(workoutTemplateId);
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithWorkoutTemplate(WorkoutTemplate workoutTemplate)
    {
        _workoutTemplate = workoutTemplate;
        _workoutTemplateId = workoutTemplate.Id;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithExerciseId(ExerciseId exerciseId)
    {
        _exerciseId = exerciseId;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithExerciseId(string exerciseId)
    {
        _exerciseId = ExerciseId.ParseOrEmpty(exerciseId);
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithExercise(Exercise? exercise)
    {
        _exercise = exercise;
        _exerciseId = exercise?.Id ?? ExerciseId.Empty;
        return this;
    }

    // Basic properties
    public WorkoutTemplateExerciseBuilder WithZone(WorkoutZone zone)
    {
        _zone = zone;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder InWarmupZone()
    {
        _zone = WorkoutZone.Warmup;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder InMainZone()
    {
        _zone = WorkoutZone.Main;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder InCooldownZone()
    {
        _zone = WorkoutZone.Cooldown;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithSequenceOrder(int sequenceOrder)
    {
        _sequenceOrder = sequenceOrder;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithoutNotes()
    {
        _notes = null;
        return this;
    }
    
    /// <summary>
    /// Sets the phase - maps to zone for compatibility with tests
    /// </summary>
    public WorkoutTemplateExerciseBuilder WithPhase(string phase)
    {
        // Map phase string to WorkoutZone enum
        _zone = phase?.ToLower() switch
        {
            "warmup" => WorkoutZone.Warmup,
            "main" => WorkoutZone.Main,
            "workout" => WorkoutZone.Main, // Workout phase maps to Main zone
            "cooldown" => WorkoutZone.Cooldown,
            _ => WorkoutZone.Main // Default to Main if unknown
        };
        
        return this;
    }
    
    /// <summary>
    /// Sets the round number - this might be used for test setup but not directly in entity
    /// For now, this is a no-op since the entity doesn't have a RoundNumber property
    /// </summary>
    public WorkoutTemplateExerciseBuilder WithRoundNumber(int roundNumber)
    {
        // The entity doesn't have a RoundNumber property, so this is just for test compatibility
        // In the future, if RoundNumber becomes part of the entity, it can be stored here
        return this;
    }

    // Collections
    public WorkoutTemplateExerciseBuilder WithSetConfiguration(SetConfiguration configuration)
    {
        _configurations.Add(configuration);
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithSetConfigurations(params SetConfiguration[] configurations)
    {
        _configurations.AddRange(configurations);
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithConfigurations(ICollection<SetConfiguration> configurations)
    {
        _configurations = configurations.ToList();
        return this;
    }
    
    public WorkoutTemplateExerciseBuilder WithoutSetConfigurations()
    {
        _configurations.Clear();
        return this;
    }

    // Build method
    public WorkoutTemplateExercise Build()
    {
        var result = WorkoutTemplateExercise.Handler.CreateNew(
            _workoutTemplateId,
            _exerciseId,
            _zone,
            _sequenceOrder,
            _notes);
        
        return result.IsSuccess
            ? result.Value! with { Id = _id }
            : WorkoutTemplateExercise.Empty;
    }
    
    // Build with navigation properties
    public WorkoutTemplateExercise BuildWithNavigationProperties()
    {
        var exercise = Build();
        
        if (exercise != WorkoutTemplateExercise.Empty)
        {
            // Use reflection to set navigation properties since they're init-only
            var exerciseWithNav = exercise with 
            { 
                Exercise = _exercise,
                WorkoutTemplate = _workoutTemplate,
                Configurations = _configurations
            };
            return exerciseWithNav;
        }
        
        return exercise;
    }
}