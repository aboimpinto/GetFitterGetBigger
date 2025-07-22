using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

public class SetConfigurationBuilder
{
    private SetConfigurationId _id = SetConfigurationId.New();
    private WorkoutTemplateExerciseId _workoutTemplateExerciseId = WorkoutTemplateExerciseId.New();
    private int _setNumber = 1;
    private string? _targetReps = "10";
    private decimal? _targetWeight = null;
    private int? _targetTimeSeconds = null;
    private int _restSeconds = 60;
    
    // Navigation properties
    private WorkoutTemplateExercise? _workoutTemplateExercise = null;

    // Common presets
    public static SetConfigurationBuilder ASetConfiguration() => new SetConfigurationBuilder();
    
    public static SetConfigurationBuilder AStrengthSet() => new SetConfigurationBuilder()
        .WithSetNumber(1)
        .WithTargetReps("5")
        .WithTargetWeight(100)
        .WithRestSeconds(180);
    
    public static SetConfigurationBuilder AHypertrophySet() => new SetConfigurationBuilder()
        .WithSetNumber(1)
        .WithTargetReps("8-12")
        .WithTargetWeight(75)
        .WithRestSeconds(90);
    
    public static SetConfigurationBuilder AnEnduranceSet() => new SetConfigurationBuilder()
        .WithSetNumber(1)
        .WithTargetReps("15-20")
        .WithTargetWeight(50)
        .WithRestSeconds(60);
    
    public static SetConfigurationBuilder ATimeBasedSet() => new SetConfigurationBuilder()
        .WithSetNumber(1)
        .WithoutTargetReps()
        .WithoutTargetWeight()
        .WithTargetTimeSeconds(60)
        .WithRestSeconds(30);
    
    public static SetConfigurationBuilder ABodyweightSet() => new SetConfigurationBuilder()
        .WithSetNumber(1)
        .WithTargetReps("10-15")
        .WithoutTargetWeight()
        .WithRestSeconds(60);

    // ID management
    public SetConfigurationBuilder WithId(SetConfigurationId id)
    {
        _id = id;
        return this;
    }
    
    public SetConfigurationBuilder WithId(string id)
    {
        _id = SetConfigurationId.ParseOrEmpty(id);
        return this;
    }
    
    public SetConfigurationBuilder WithNewId()
    {
        _id = SetConfigurationId.New();
        return this;
    }

    // Required relationships
    public SetConfigurationBuilder WithWorkoutTemplateExerciseId(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        _workoutTemplateExerciseId = workoutTemplateExerciseId;
        return this;
    }
    
    public SetConfigurationBuilder WithWorkoutTemplateExerciseId(string workoutTemplateExerciseId)
    {
        _workoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(workoutTemplateExerciseId);
        return this;
    }
    
    public SetConfigurationBuilder WithWorkoutTemplateExercise(WorkoutTemplateExercise workoutTemplateExercise)
    {
        _workoutTemplateExercise = workoutTemplateExercise;
        _workoutTemplateExerciseId = workoutTemplateExercise.Id;
        return this;
    }

    // Basic properties
    public SetConfigurationBuilder WithSetNumber(int setNumber)
    {
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
    
    public SetConfigurationBuilder WithTargetWeight(decimal? targetWeight)
    {
        _targetWeight = targetWeight;
        return this;
    }
    
    public SetConfigurationBuilder WithoutTargetWeight()
    {
        _targetWeight = null;
        return this;
    }
    
    public SetConfigurationBuilder WithTargetTimeSeconds(int? targetTimeSeconds)
    {
        _targetTimeSeconds = targetTimeSeconds;
        return this;
    }
    
    public SetConfigurationBuilder WithoutTargetTimeSeconds()
    {
        _targetTimeSeconds = null;
        return this;
    }
    
    public SetConfigurationBuilder WithRestSeconds(int restSeconds)
    {
        _restSeconds = restSeconds;
        return this;
    }
    
    public SetConfigurationBuilder WithNoRest()
    {
        _restSeconds = 0;
        return this;
    }

    // Range helpers
    public SetConfigurationBuilder WithRepsRange(int min, int max)
    {
        _targetReps = $"{min}-{max}";
        return this;
    }
    
    public SetConfigurationBuilder WithFixedReps(int reps)
    {
        _targetReps = reps.ToString();
        return this;
    }

    // Build method
    public SetConfiguration Build()
    {
        var result = SetConfiguration.Handler.CreateNew(
            _workoutTemplateExerciseId,
            _setNumber,
            _targetReps,
            _targetWeight,
            _targetTimeSeconds,
            _restSeconds);
        
        return result.IsSuccess
            ? result.Value! with { Id = _id }
            : SetConfiguration.Empty;
    }
    
    // Build with navigation properties
    public SetConfiguration BuildWithNavigationProperties()
    {
        var configuration = Build();
        
        // For now, tests should mock repository methods to return configurations with navigation properties
        
        return configuration;
    }
}