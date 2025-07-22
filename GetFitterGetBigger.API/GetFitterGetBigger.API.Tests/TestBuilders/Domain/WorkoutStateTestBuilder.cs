using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid WorkoutState entities with proper validation
/// </summary>
public class WorkoutStateTestBuilder
{
    private string _value = "DRAFT";
    private string? _description = "Template under construction";
    private int _displayOrder = 1;
    private bool _isActive = true;
    private WorkoutStateId? _id = null;

    private WorkoutStateTestBuilder() { }

    /// <summary>
    /// Creates a builder with default values for a draft workout state
    /// </summary>
    public static WorkoutStateTestBuilder Default() => new WorkoutStateTestBuilder();

    /// <summary>
    /// Creates a builder for DRAFT state
    /// </summary>
    public static WorkoutStateTestBuilder Draft() => new WorkoutStateTestBuilder()
        .WithValue("DRAFT")
        .WithDescription("Template under construction")
        .WithDisplayOrder(1)
        .WithId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Draft));

    /// <summary>
    /// Creates a builder for PRODUCTION state
    /// </summary>
    public static WorkoutStateTestBuilder Production() => new WorkoutStateTestBuilder()
        .WithValue("PRODUCTION")
        .WithDescription("Active template for use")
        .WithDisplayOrder(2)
        .WithId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production));

    /// <summary>
    /// Creates a builder for ARCHIVED state
    /// </summary>
    public static WorkoutStateTestBuilder Archived() => new WorkoutStateTestBuilder()
        .WithValue("ARCHIVED")
        .WithDescription("Retired template")
        .WithDisplayOrder(3)
        .WithId(WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Archived));

    /// <summary>
    /// Creates a builder for custom workout state
    /// </summary>
    public static WorkoutStateTestBuilder Custom() => new WorkoutStateTestBuilder();

    public WorkoutStateTestBuilder WithId(WorkoutStateId id)
    {
        _id = id;
        return this;
    }

    public WorkoutStateTestBuilder WithId(string idString)
    {
        var id = WorkoutStateId.ParseOrEmpty(idString);
        if (id.IsEmpty)
        {
            throw new ArgumentException($"Invalid WorkoutStateId format: '{idString}'. Expected format: 'workoutstate-{{guid}}'");
        }
        _id = id;
        return this;
    }

    public WorkoutStateTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Workout state value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public WorkoutStateTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public WorkoutStateTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public WorkoutStateTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    public WorkoutStateTestBuilder IsInactive() => IsActive(false);

    /// <summary>
    /// Builds a WorkoutState entity with validation
    /// </summary>
    public WorkoutState Build()
    {
        // If ID is provided, use Create method, otherwise use CreateNew
        var result = _id.HasValue
            ? WorkoutState.Handler.Create(
                id: _id.Value,
                value: _value,
                description: _description,
                displayOrder: _displayOrder,
                isActive: _isActive)
            : WorkoutState.Handler.CreateNew(
                value: _value,
                description: _description,
                displayOrder: _displayOrder,
                isActive: _isActive);

        return result.Value;
    }

    /// <summary>
    /// Builds and returns just the WorkoutStateId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id;
    }

    /// <summary>
    /// Implicit conversion to WorkoutState for convenience
    /// </summary>
    public static implicit operator WorkoutState(WorkoutStateTestBuilder builder)
    {
        return builder.Build();
    }
}