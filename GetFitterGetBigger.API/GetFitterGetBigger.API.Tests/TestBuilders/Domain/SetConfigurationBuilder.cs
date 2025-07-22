using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid SetConfiguration entities with proper validation
/// </summary>
public class SetConfigurationBuilder
{
    private SetConfigurationId? _id = null;
    private WorkoutTemplateExerciseId _workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(TestIds.WorkoutTemplateExerciseIds.TemplateExercise1);
    private int _setNumber = 1;
    private string? _targetReps = "10";
    private decimal? _targetWeight = null;
    private int? _targetTime = null;
    private int _restSeconds = 90;
    
    // Navigation properties
    private WorkoutTemplateExercise? _workoutTemplateExercise = null;

    private SetConfigurationBuilder() { }

    /// <summary>
    /// Creates a builder with default values
    /// </summary>
    public static SetConfigurationBuilder Default() => new SetConfigurationBuilder();

    /// <summary>
    /// Creates a builder for a standard strength set
    /// </summary>
    public static SetConfigurationBuilder StrengthSet() => new SetConfigurationBuilder()
        .WithTargetReps("5")
        .WithTargetWeight(100)
        .WithRestSeconds(180);

    /// <summary>
    /// Creates a builder for a hypertrophy set
    /// </summary>
    public static SetConfigurationBuilder HypertrophySet() => new SetConfigurationBuilder()
        .WithTargetReps("8-12")
        .WithTargetWeight(80)
        .WithRestSeconds(90);

    /// <summary>
    /// Creates a builder for an endurance set
    /// </summary>
    public static SetConfigurationBuilder EnduranceSet() => new SetConfigurationBuilder()
        .WithTargetReps("15-20")
        .WithTargetWeight(60)
        .WithRestSeconds(60);

    /// <summary>
    /// Creates a builder for a time-based set (e.g., plank)
    /// </summary>
    public static SetConfigurationBuilder TimeBasedSet() => new SetConfigurationBuilder()
        .WithTargetTime(60)
        .WithoutTargetReps()
        .WithRestSeconds(60);

    /// <summary>
    /// Creates a builder for an AMRAP set
    /// </summary>
    public static SetConfigurationBuilder AMRAPSet() => new SetConfigurationBuilder()
        .WithTargetReps("AMRAP")
        .WithRestSeconds(120);

    /// <summary>
    /// Creates a builder for a drop set
    /// </summary>
    public static SetConfigurationBuilder DropSet() => new SetConfigurationBuilder()
        .WithTargetReps("10+10+10")
        .WithTargetWeight(80)
        .WithRestSeconds(120);

    public SetConfigurationBuilder WithId(SetConfigurationId id)
    {
        _id = id;
        return this;
    }

    public SetConfigurationBuilder WithId(string idString)
    {
        var id = SetConfigurationId.ParseOrEmpty(idString);
        if (id.IsEmpty)
        {
            throw new ArgumentException($"Invalid SetConfigurationId format: '{idString}'");
        }
        _id = id;
        return this;
    }

    public SetConfigurationBuilder WithWorkoutTemplateExerciseId(WorkoutTemplateExerciseId exerciseId)
    {
        _workoutTemplateExerciseId = exerciseId;
        return this;
    }

    public SetConfigurationBuilder WithWorkoutTemplateExercise(WorkoutTemplateExercise exercise)
    {
        _workoutTemplateExercise = exercise;
        _workoutTemplateExerciseId = exercise.Id;
        return this;
    }

    public SetConfigurationBuilder WithSetNumber(int setNumber)
    {
        if (setNumber < 1 || setNumber > 100)
        {
            throw new ArgumentException("Set number must be between 1 and 100", nameof(setNumber));
        }
        _setNumber = setNumber;
        return this;
    }

    public SetConfigurationBuilder WithTargetReps(string? targetReps)
    {
        _targetReps = targetReps;
        return this;
    }

    public SetConfigurationBuilder WithoutTargetReps()
    {
        _targetReps = null;
        return this;
    }

    public SetConfigurationBuilder WithTargetWeight(decimal? weight)
    {
        if (weight < 0)
        {
            throw new ArgumentException("Target weight cannot be negative", nameof(weight));
        }
        _targetWeight = weight;
        return this;
    }

    public SetConfigurationBuilder WithTargetTime(int? seconds)
    {
        if (seconds != null && (seconds < 1 || seconds > 3600))
        {
            throw new ArgumentException("Target time must be between 1 and 3600 seconds", nameof(seconds));
        }
        _targetTime = seconds;
        return this;
    }

    public SetConfigurationBuilder WithRestSeconds(int seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentException("Rest seconds cannot be negative", nameof(seconds));
        }
        _restSeconds = seconds;
        return this;
    }

    /// <summary>
    /// Builds a SetConfiguration entity with validation
    /// </summary>
    public SetConfiguration Build()
    {
        var result = _id.HasValue
            ? SetConfiguration.Handler.Create(
                id: _id.Value,
                workoutTemplateExerciseId: _workoutTemplateExerciseId,
                setNumber: _setNumber,
                targetReps: _targetReps,
                targetWeight: _targetWeight,
                targetTime: _targetTime,
                restSeconds: _restSeconds)
            : SetConfiguration.Handler.CreateNew(
                workoutTemplateExerciseId: _workoutTemplateExerciseId,
                setNumber: _setNumber,
                targetReps: _targetReps,
                targetWeight: _targetWeight,
                targetTime: _targetTime,
                restSeconds: _restSeconds);

        var setConfig = result.Data;
        
        // Set navigation properties if provided
        if (_workoutTemplateExercise != null)
        {
            var exerciseProperty = setConfig.GetType().GetProperty("WorkoutTemplateExercise");
            exerciseProperty?.SetValue(setConfig, _workoutTemplateExercise);
        }

        return setConfig;
    }

    /// <summary>
    /// Builds and returns just the SetConfigurationId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id;
    }

    /// <summary>
    /// Implicit conversion to SetConfiguration for convenience
    /// </summary>
    public static implicit operator SetConfiguration(SetConfigurationBuilder builder)
    {
        return builder.Build();
    }
}