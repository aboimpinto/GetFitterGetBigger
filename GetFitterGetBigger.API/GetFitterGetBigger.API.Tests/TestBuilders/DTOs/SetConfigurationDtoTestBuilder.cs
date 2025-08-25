using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Builder for creating SetConfigurationDto instances in tests
/// </summary>
public class SetConfigurationDtoTestBuilder
{
    private string _id = SetConfigurationId.New().ToString();
    private string _workoutTemplateExerciseId = WorkoutTemplateExerciseId.New().ToString();
    private int _setNumber = 1;
    private string? _targetReps = "8-12";
    private decimal? _targetWeight = 80.0m;
    private int? _targetTimeSeconds;
    private int _restSeconds = 90;

    public static SetConfigurationDtoTestBuilder Create() => new();

    public SetConfigurationDtoTestBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public SetConfigurationDtoTestBuilder WithWorkoutTemplateExerciseId(string workoutTemplateExerciseId)
    {
        _workoutTemplateExerciseId = workoutTemplateExerciseId;
        return this;
    }

    public SetConfigurationDtoTestBuilder WithSetNumber(int setNumber)
    {
        _setNumber = setNumber;
        return this;
    }

    public SetConfigurationDtoTestBuilder WithTargetReps(string? targetReps)
    {
        _targetReps = targetReps;
        return this;
    }

    public SetConfigurationDtoTestBuilder WithTargetWeight(decimal? targetWeight)
    {
        _targetWeight = targetWeight;
        return this;
    }

    public SetConfigurationDtoTestBuilder WithTargetTimeSeconds(int? targetTimeSeconds)
    {
        _targetTimeSeconds = targetTimeSeconds;
        return this;
    }

    public SetConfigurationDtoTestBuilder WithRestSeconds(int restSeconds)
    {
        _restSeconds = restSeconds;
        return this;
    }

    public SetConfigurationDto Build()
    {
        return new SetConfigurationDto
        {
            Id = _id,
            WorkoutTemplateExerciseId = _workoutTemplateExerciseId,
            SetNumber = _setNumber,
            TargetReps = _targetReps,
            TargetWeight = _targetWeight,
            TargetTime = _targetTimeSeconds,
            RestSeconds = _restSeconds
        };
    }
}

/// <summary>
/// Shorter alias for the builder
/// </summary>
public static class SetConfigurationDtoBuilder
{
    public static SetConfigurationDtoTestBuilder Create() => SetConfigurationDtoTestBuilder.Create();
}