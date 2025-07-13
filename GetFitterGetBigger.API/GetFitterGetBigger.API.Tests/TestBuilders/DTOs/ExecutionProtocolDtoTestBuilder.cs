using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Tests.TestBuilders.DTOs;

/// <summary>
/// Test builder for creating ExecutionProtocolDto instances
/// </summary>
public class ExecutionProtocolDtoTestBuilder
{
    private string _executionProtocolId = "executionprotocol-11111111-1111-1111-1111-111111111111";
    private string _value = "Standard";
    private string? _description = "Standard protocol with balanced rep and time components";
    private string _code = "STANDARD";
    private bool _timeBase = true;
    private bool _repBase = true;
    private string? _restPattern = "60-90 seconds between sets";
    private string? _intensityLevel = "Moderate to High";
    private int _displayOrder = 1;
    private bool _isActive = true;

    private ExecutionProtocolDtoTestBuilder() { }

    /// <summary>
    /// Creates a builder for STANDARD protocol DTO
    /// </summary>
    public static ExecutionProtocolDtoTestBuilder Standard() => new ExecutionProtocolDtoTestBuilder()
        .WithExecutionProtocolId(TestIds.ExecutionProtocolIds.Standard)
        .WithValue("Standard")
        .WithDescription("Standard protocol with balanced rep and time components")
        .WithCode("STANDARD")
        .WithTimeBase(true)
        .WithRepBase(true)
        .WithRestPattern("60-90 seconds between sets")
        .WithIntensityLevel("Moderate to High")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for SUPERSET protocol DTO
    /// </summary>
    public static ExecutionProtocolDtoTestBuilder Superset() => new ExecutionProtocolDtoTestBuilder()
        .WithExecutionProtocolId(TestIds.ExecutionProtocolIds.Superset)
        .WithValue("Superset")
        .WithDescription("Perform exercises back-to-back without rest")
        .WithCode("SUPERSET")
        .WithTimeBase(false)
        .WithRepBase(true)
        .WithRestPattern("Rest after completing both exercises")
        .WithIntensityLevel("High")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for DROP SET protocol DTO
    /// </summary>
    public static ExecutionProtocolDtoTestBuilder DropSet() => new ExecutionProtocolDtoTestBuilder()
        .WithExecutionProtocolId(TestIds.ExecutionProtocolIds.DropSet)
        .WithValue("Drop Set")
        .WithDescription("Reduce weight after reaching failure")
        .WithCode("DROP_SET")
        .WithTimeBase(false)
        .WithRepBase(true)
        .WithRestPattern("Minimal rest between drops")
        .WithIntensityLevel("Very High")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for AMRAP protocol DTO
    /// </summary>
    public static ExecutionProtocolDtoTestBuilder AMRAP() => new ExecutionProtocolDtoTestBuilder()
        .WithExecutionProtocolId(TestIds.ExecutionProtocolIds.AMRAP)
        .WithValue("AMRAP")
        .WithDescription("As Many Reps As Possible in given time")
        .WithCode("AMRAP")
        .WithTimeBase(true)
        .WithRepBase(false)
        .WithRestPattern("Fixed rest periods")
        .WithIntensityLevel("High")
        .WithDisplayOrder(4);

    public ExecutionProtocolDtoTestBuilder WithExecutionProtocolId(string executionProtocolId)
    {
        _executionProtocolId = executionProtocolId;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithTimeBase(bool timeBase)
    {
        _timeBase = timeBase;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithRepBase(bool repBase)
    {
        _repBase = repBase;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithRestPattern(string? restPattern)
    {
        _restPattern = restPattern;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithIntensityLevel(string? intensityLevel)
    {
        _intensityLevel = intensityLevel;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithDisplayOrder(int displayOrder)
    {
        _displayOrder = displayOrder;
        return this;
    }

    public ExecutionProtocolDtoTestBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public ExecutionProtocolDto Build()
    {
        return new ExecutionProtocolDto
        {
            ExecutionProtocolId = _executionProtocolId,
            Value = _value,
            Description = _description,
            Code = _code,
            TimeBase = _timeBase,
            RepBase = _repBase,
            RestPattern = _restPattern,
            IntensityLevel = _intensityLevel,
            DisplayOrder = _displayOrder,
            IsActive = _isActive
        };
    }
}