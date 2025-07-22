using System;
using System.Collections.Generic;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid WorkoutTemplateExercise entities with proper validation
/// </summary>
public class WorkoutTemplateExerciseBuilder
{
    private WorkoutTemplateExerciseId? _id = null;
    private WorkoutTemplateId _workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.UpperBodyPushTemplate);
    private ExerciseId _exerciseId = ExerciseId.New();
    private string _zone = "Main";
    private int _sequenceOrder = 1;
    private string? _notes = null;
    
    // Navigation properties
    private WorkoutTemplate? _workoutTemplate = null;
    private Exercise? _exercise = null;
    private List<SetConfiguration> _configurations = new List<SetConfiguration>();

    private WorkoutTemplateExerciseBuilder() { }

    /// <summary>
    /// Creates a builder with default values
    /// </summary>
    public static WorkoutTemplateExerciseBuilder Default() => new WorkoutTemplateExerciseBuilder();

    /// <summary>
    /// Creates a builder for a warmup exercise
    /// </summary>
    public static WorkoutTemplateExerciseBuilder Warmup() => new WorkoutTemplateExerciseBuilder()
        .WithZone("Warmup")
        .WithSequenceOrder(1)
        .WithNotes("Light weight, focus on form");

    /// <summary>
    /// Creates a builder for a main workout exercise
    /// </summary>
    public static WorkoutTemplateExerciseBuilder MainExercise() => new WorkoutTemplateExerciseBuilder()
        .WithZone("Main")
        .WithSequenceOrder(1)
        .WithNotes("Working sets");

    /// <summary>
    /// Creates a builder for a cooldown exercise
    /// </summary>
    public static WorkoutTemplateExerciseBuilder Cooldown() => new WorkoutTemplateExerciseBuilder()
        .WithZone("Cooldown")
        .WithSequenceOrder(1)
        .WithNotes("Stretching and recovery");

    /// <summary>
    /// Creates a builder for a rest period
    /// </summary>
    public static WorkoutTemplateExerciseBuilder RestPeriod() => new WorkoutTemplateExerciseBuilder()
        .WithZone("Main")
        .WithNotes("Rest between exercises");

    public WorkoutTemplateExerciseBuilder WithId(WorkoutTemplateExerciseId id)
    {
        _id = id;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithId(string idString)
    {
        var id = WorkoutTemplateExerciseId.ParseOrEmpty(idString);
        if (id.IsEmpty)
        {
            throw new ArgumentException($"Invalid WorkoutTemplateExerciseId format: '{idString}'");
        }
        _id = id;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithWorkoutTemplateId(WorkoutTemplateId templateId)
    {
        _workoutTemplateId = templateId;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithWorkoutTemplateId(string templateIdString)
    {
        var id = WorkoutTemplateId.ParseOrEmpty(templateIdString);
        if (id.IsEmpty)
        {
            throw new ArgumentException($"Invalid WorkoutTemplateId format: '{templateIdString}'");
        }
        _workoutTemplateId = id;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithExerciseId(ExerciseId exerciseId)
    {
        _exerciseId = exerciseId;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithExercise(Exercise exercise)
    {
        _exercise = exercise;
        _exerciseId = exercise.Id;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithZone(string zone)
    {
        if (!new[] { "Warmup", "Main", "Cooldown" }.Contains(zone))
        {
            throw new ArgumentException("Zone must be Warmup, Main, or Cooldown", nameof(zone));
        }
        _zone = zone;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithSequenceOrder(int order)
    {
        if (order < 1)
        {
            throw new ArgumentException("Sequence order must be positive", nameof(order));
        }
        _sequenceOrder = order;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithConfigurations(params SetConfiguration[] configurations)
    {
        _configurations = configurations.ToList();
        return this;
    }

    public WorkoutTemplateExerciseBuilder WithWorkoutTemplate(WorkoutTemplate template)
    {
        _workoutTemplate = template;
        _workoutTemplateId = template.Id;
        return this;
    }

    /// <summary>
    /// Builds a WorkoutTemplateExercise entity with validation
    /// </summary>
    public WorkoutTemplateExercise Build()
    {
        var result = _id.HasValue
            ? WorkoutTemplateExercise.Handler.Create(
                id: _id.Value,
                workoutTemplateId: _workoutTemplateId,
                exerciseId: _exerciseId,
                zone: _zone,
                sequenceOrder: _sequenceOrder,
                notes: _notes)
            : WorkoutTemplateExercise.Handler.CreateNew(
                workoutTemplateId: _workoutTemplateId,
                exerciseId: _exerciseId,
                zone: _zone,
                sequenceOrder: _sequenceOrder,
                notes: _notes);

        var templateExercise = result.Data;
        
        // Set navigation properties if provided
        if (_workoutTemplate != null)
        {
            var templateProperty = templateExercise.GetType().GetProperty("WorkoutTemplate");
            templateProperty?.SetValue(templateExercise, _workoutTemplate);
        }
        
        if (_exercise != null)
        {
            var exerciseProperty = templateExercise.GetType().GetProperty("Exercise");
            exerciseProperty?.SetValue(templateExercise, _exercise);
        }
        
        if (_configurations.Any())
        {
            var configurationsProperty = templateExercise.GetType().GetProperty("Configurations");
            configurationsProperty?.SetValue(templateExercise, _configurations);
        }

        return templateExercise;
    }

    /// <summary>
    /// Builds and returns just the WorkoutTemplateExerciseId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id;
    }

    /// <summary>
    /// Implicit conversion to WorkoutTemplateExercise for convenience
    /// </summary>
    public static implicit operator WorkoutTemplateExercise(WorkoutTemplateExerciseBuilder builder)
    {
        return builder.Build();
    }
}