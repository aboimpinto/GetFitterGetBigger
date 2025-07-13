using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

using GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Test builder for creating valid WorkoutObjective entities with proper validation
/// </summary>
public class WorkoutObjectiveTestBuilder
{
    private string _value = "Muscular Strength";
    private string? _description = "Build maximum strength through heavy loads and low repetitions";
    private int _displayOrder = 1;
    private bool _isActive = true;
    private WorkoutObjectiveId? _id = null;

    private WorkoutObjectiveTestBuilder() { }

    /// <summary>
    /// Creates a builder for MUSCULAR STRENGTH workout objective
    /// </summary>
    public static WorkoutObjectiveTestBuilder MuscularStrength() => new WorkoutObjectiveTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.MuscularStrength)
        .WithValue("Muscular Strength")
        .WithDescription("Build maximum strength through heavy loads and low repetitions")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for MUSCULAR HYPERTROPHY workout objective
    /// </summary>
    public static WorkoutObjectiveTestBuilder MuscularHypertrophy() => new WorkoutObjectiveTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.MuscularHypertrophy)
        .WithValue("Muscular Hypertrophy")
        .WithDescription("Increase muscle size through moderate loads and volume")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for MUSCULAR ENDURANCE workout objective
    /// </summary>
    public static WorkoutObjectiveTestBuilder MuscularEndurance() => new WorkoutObjectiveTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.MuscularEndurance)
        .WithValue("Muscular Endurance")
        .WithDescription("Improve ability to perform repetitive contractions")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for POWER DEVELOPMENT workout objective
    /// </summary>
    public static WorkoutObjectiveTestBuilder PowerDevelopment() => new WorkoutObjectiveTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.PowerDevelopment)
        .WithValue("Power Development")
        .WithDescription("Develop explosive strength and speed")
        .WithDisplayOrder(4);

    /// <summary>
    /// Creates a builder for an INACTIVE workout objective (for testing)
    /// </summary>
    public static WorkoutObjectiveTestBuilder InactiveObjective() => new WorkoutObjectiveTestBuilder()
        .WithId(TestIds.WorkoutObjectiveIds.InactiveObjective)
        .WithValue("Inactive Objective")
        .WithDescription("This objective is inactive")
        .WithDisplayOrder(5)
        .IsActive(false);

    public WorkoutObjectiveTestBuilder WithId(WorkoutObjectiveId id)
    {
        _id = id;
        return this;
    }

    public WorkoutObjectiveTestBuilder WithId(string idString)
    {
        _id = WorkoutObjectiveId.From(idString);
        if (_id == null || _id.Value.IsEmpty)
        {
            throw new ArgumentException($"Invalid WorkoutObjectiveId format: '{idString}'. Expected format: 'workoutobjective-{{guid}}' or valid GUID");
        }
        return this;
    }

    public WorkoutObjectiveTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Workout objective value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public WorkoutObjectiveTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public WorkoutObjectiveTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public WorkoutObjectiveTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Builds a WorkoutObjective entity with validation
    /// </summary>
    public WorkoutObjective Build()
    {
        // If ID is provided, use it, otherwise generate new
        var id = _id ?? WorkoutObjectiveId.New();
        
        return WorkoutObjective.Handler.Create(
            id: id,
            value: _value,
            description: _description,
            displayOrder: _displayOrder,
            isActive: _isActive
        );
    }

    /// <summary>
    /// Builds and returns just the WorkoutObjectiveId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to WorkoutObjective for convenience
    /// </summary>
    public static implicit operator WorkoutObjective(WorkoutObjectiveTestBuilder builder)
    {
        return builder.Build();
    }
}