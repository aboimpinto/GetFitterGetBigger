using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Builder for creating CreateSetConfigurationDto instances in tests
/// </summary>
public class CreateSetConfigurationDtoTestBuilder
{
    private int? _setNumber = 1;
    private string? _targetReps = "8-12";
    private decimal? _targetWeight = 80.0m;
    private int? _targetTimeSeconds;
    private int _restSeconds = 90;

    public static CreateSetConfigurationDtoTestBuilder Create() => new();

    public CreateSetConfigurationDtoTestBuilder WithSetNumber(int? setNumber)
    {
        _setNumber = setNumber;
        return this;
    }

    public CreateSetConfigurationDtoTestBuilder WithTargetReps(string? targetReps)
    {
        _targetReps = targetReps;
        return this;
    }

    public CreateSetConfigurationDtoTestBuilder WithTargetWeight(decimal? targetWeight)
    {
        _targetWeight = targetWeight;
        return this;
    }

    public CreateSetConfigurationDtoTestBuilder WithTargetTimeSeconds(int? targetTimeSeconds)
    {
        _targetTimeSeconds = targetTimeSeconds;
        return this;
    }

    public CreateSetConfigurationDtoTestBuilder WithRestSeconds(int restSeconds)
    {
        _restSeconds = restSeconds;
        return this;
    }

    public CreateSetConfigurationDto Build()
    {
        return new CreateSetConfigurationDto
        {
            SetNumber = _setNumber,
            TargetReps = _targetReps,
            TargetWeight = _targetWeight,
            TargetTimeSeconds = _targetTimeSeconds,
            RestSeconds = _restSeconds
        };
    }
}

/// <summary>
/// Shorter alias for the builder
/// </summary>
public static class CreateSetConfigurationDtoBuilder
{
    public static CreateSetConfigurationDtoTestBuilder Create() => CreateSetConfigurationDtoTestBuilder.Create();
}