using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.TestBuilders;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid ExecutionProtocol entities with proper validation
/// </summary>
public class ExecutionProtocolTestBuilder
{
    private string _value = "Standard";
    private string? _description = "Traditional set and rep scheme";
    private string _code = "STANDARD";
    private bool _timeBase = false;
    private bool _repBase = true;
    private string? _restPattern = "Fixed Rest";
    private string? _intensityLevel = "Moderate";
    private int _displayOrder = 1;
    private bool _isActive = true;
    private ExecutionProtocolId? _id = null;

    private ExecutionProtocolTestBuilder() { }

    /// <summary>
    /// Creates a builder for STANDARD execution protocol
    /// </summary>
    public static ExecutionProtocolTestBuilder Standard() => new ExecutionProtocolTestBuilder()
        .WithId(TestIds.ExecutionProtocolIds.Standard)
        .WithValue("Standard")
        .WithDescription("Traditional set and rep scheme")
        .WithCode("STANDARD")
        .WithTimeBase(false)
        .WithRepBase(true)
        .WithRestPattern("Fixed Rest")
        .WithIntensityLevel("Moderate")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for SUPERSET execution protocol
    /// </summary>
    public static ExecutionProtocolTestBuilder Superset() => new ExecutionProtocolTestBuilder()
        .WithId(TestIds.ExecutionProtocolIds.Superset)
        .WithValue("Superset")
        .WithDescription("Two exercises performed back-to-back")
        .WithCode("SUPERSET")
        .WithTimeBase(false)
        .WithRepBase(true)
        .WithRestPattern("Minimal Rest")
        .WithIntensityLevel("High")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for DROP SET execution protocol
    /// </summary>
    public static ExecutionProtocolTestBuilder DropSet() => new ExecutionProtocolTestBuilder()
        .WithId(TestIds.ExecutionProtocolIds.DropSet)
        .WithValue("Drop Set")
        .WithDescription("Reduce weight and continue for more reps")
        .WithCode("DROPSET")
        .WithTimeBase(false)
        .WithRepBase(true)
        .WithRestPattern("No Rest")
        .WithIntensityLevel("Very High")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for AMRAP execution protocol
    /// </summary>
    public static ExecutionProtocolTestBuilder AMRAP() => new ExecutionProtocolTestBuilder()
        .WithId(TestIds.ExecutionProtocolIds.AMRAP)
        .WithValue("AMRAP")
        .WithDescription("As Many Reps As Possible")
        .WithCode("AMRAP")
        .WithTimeBase(true)
        .WithRepBase(false)
        .WithRestPattern("Fixed Time")
        .WithIntensityLevel("Maximum")
        .WithDisplayOrder(4);

    /// <summary>
    /// Creates a builder for an INACTIVE execution protocol (for testing)
    /// </summary>
    public static ExecutionProtocolTestBuilder InactiveProtocol() => new ExecutionProtocolTestBuilder()
        .WithId(TestIds.ExecutionProtocolIds.InactiveProtocol)
        .WithValue("Inactive Protocol")
        .WithDescription("This protocol is inactive")
        .WithCode("INACTIVE")
        .WithTimeBase(false)
        .WithRepBase(true)
        .WithRestPattern("Fixed Rest")
        .WithIntensityLevel("Low")
        .WithDisplayOrder(5)
        .IsActive(false);

    public ExecutionProtocolTestBuilder WithId(ExecutionProtocolId id)
    {
        _id = id;
        return this;
    }

    public ExecutionProtocolTestBuilder WithId(string idString)
    {
        _id = ExecutionProtocolId.ParseOrEmpty(idString);
        if (_id?.IsEmpty ?? false)
        {
            throw new ArgumentException($"Invalid ExecutionProtocolId format: '{idString}'. Expected format: 'executionprotocol-{{guid}}' or valid GUID");
        }
        return this;
    }

    public ExecutionProtocolTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Execution protocol value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public ExecutionProtocolTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ExecutionProtocolTestBuilder WithCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Execution protocol code cannot be empty", nameof(code));
        }
        // Could add regex validation here if needed
        _code = code;
        return this;
    }

    public ExecutionProtocolTestBuilder WithTimeBase(bool timeBase)
    {
        _timeBase = timeBase;
        return this;
    }

    public ExecutionProtocolTestBuilder WithRepBase(bool repBase)
    {
        _repBase = repBase;
        return this;
    }

    public ExecutionProtocolTestBuilder WithRestPattern(string? restPattern)
    {
        _restPattern = restPattern;
        return this;
    }

    public ExecutionProtocolTestBuilder WithIntensityLevel(string? intensityLevel)
    {
        _intensityLevel = intensityLevel;
        return this;
    }

    public ExecutionProtocolTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public ExecutionProtocolTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Builds an ExecutionProtocol entity with validation
    /// </summary>
    public ExecutionProtocol Build()
    {
        // If ID is provided, use it, otherwise generate new
        var id = _id ?? ExecutionProtocolId.New();
        
        var result = ExecutionProtocol.Handler.Create(
            id: id,
            value: _value,
            description: _description,
            code: _code,
            timeBase: _timeBase,
            repBase: _repBase,
            restPattern: _restPattern,
            intensityLevel: _intensityLevel,
            displayOrder: _displayOrder,
            isActive: _isActive
        );
        
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to create ExecutionProtocol: {string.Join(", ", result.Errors)}");
        }
        
        return result.Value;
    }

    /// <summary>
    /// Builds and returns just the ExecutionProtocolId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().ExecutionProtocolId.ToString();
    }

    /// <summary>
    /// Implicit conversion to ExecutionProtocol for convenience
    /// </summary>
    public static implicit operator ExecutionProtocol(ExecutionProtocolTestBuilder builder)
    {
        return builder.Build();
    }
}