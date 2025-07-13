using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

using GetFitterGetBigger.API.Tests.TestBuilders;

/// <summary>
/// Test builder for creating valid WorkoutCategory entities with proper validation
/// </summary>
public class WorkoutCategoryTestBuilder
{
    private string _value = "Upper Body Push";
    private string? _description = "Pushing exercises for chest, shoulders, and triceps";
    private string? _icon = "push-icon";
    private string? _color = "#FF5733";
    private string? _primaryMuscleGroups = "Chest, Shoulders, Triceps";
    private int _displayOrder = 1;
    private bool _isActive = true;
    private WorkoutCategoryId? _id = null;

    private WorkoutCategoryTestBuilder() { }

    /// <summary>
    /// Creates a builder for UPPER BODY PUSH workout category
    /// </summary>
    public static WorkoutCategoryTestBuilder UpperBodyPush() => new WorkoutCategoryTestBuilder()
        .WithId(TestIds.WorkoutCategoryIds.UpperBodyPush)
        .WithValue("Upper Body Push")
        .WithDescription("Pushing exercises for chest, shoulders, and triceps")
        .WithIcon("push-icon")
        .WithColor("#FF5733")
        .WithPrimaryMuscleGroups("Chest, Shoulders, Triceps")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for UPPER BODY PULL workout category
    /// </summary>
    public static WorkoutCategoryTestBuilder UpperBodyPull() => new WorkoutCategoryTestBuilder()
        .WithId(TestIds.WorkoutCategoryIds.UpperBodyPull)
        .WithValue("Upper Body Pull")
        .WithDescription("Pulling exercises for back and biceps")
        .WithIcon("pull-icon")
        .WithColor("#33FF57")
        .WithPrimaryMuscleGroups("Back, Biceps")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for LOWER BODY workout category
    /// </summary>
    public static WorkoutCategoryTestBuilder LowerBody() => new WorkoutCategoryTestBuilder()
        .WithId(TestIds.WorkoutCategoryIds.LowerBody)
        .WithValue("Lower Body")
        .WithDescription("Exercises for legs and glutes")
        .WithIcon("legs-icon")
        .WithColor("#3357FF")
        .WithPrimaryMuscleGroups("Quadriceps, Hamstrings, Glutes")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for CORE workout category
    /// </summary>
    public static WorkoutCategoryTestBuilder Core() => new WorkoutCategoryTestBuilder()
        .WithId(TestIds.WorkoutCategoryIds.Core)
        .WithValue("Core")
        .WithDescription("Exercises for abs and core stability")
        .WithIcon("core-icon")
        .WithColor("#FF33F5")
        .WithPrimaryMuscleGroups("Abs, Obliques")
        .WithDisplayOrder(4);

    /// <summary>
    /// Creates a builder for an INACTIVE workout category (for testing)
    /// </summary>
    public static WorkoutCategoryTestBuilder InactiveCategory() => new WorkoutCategoryTestBuilder()
        .WithId(TestIds.WorkoutCategoryIds.InactiveCategory)
        .WithValue("Inactive Category")
        .WithDescription("This category is inactive")
        .WithIcon("inactive-icon")
        .WithColor("#000000")
        .WithPrimaryMuscleGroups("None")
        .WithDisplayOrder(5)
        .IsActive(false);

    public WorkoutCategoryTestBuilder WithId(WorkoutCategoryId id)
    {
        _id = id;
        return this;
    }

    public WorkoutCategoryTestBuilder WithId(string idString)
    {
        _id = WorkoutCategoryId.From(idString);
        if (_id == null || _id.Value.IsEmpty)
        {
            throw new ArgumentException($"Invalid WorkoutCategoryId format: '{idString}'. Expected format: 'workoutcategory-{{guid}}' or valid GUID");
        }
        return this;
    }

    public WorkoutCategoryTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Workout category value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public WorkoutCategoryTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public WorkoutCategoryTestBuilder WithIcon(string? icon)
    {
        _icon = icon;
        return this;
    }

    public WorkoutCategoryTestBuilder WithColor(string? color)
    {
        _color = color;
        return this;
    }

    public WorkoutCategoryTestBuilder WithPrimaryMuscleGroups(string? primaryMuscleGroups)
    {
        _primaryMuscleGroups = primaryMuscleGroups;
        return this;
    }

    public WorkoutCategoryTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public WorkoutCategoryTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Builds a WorkoutCategory entity with validation
    /// </summary>
    public WorkoutCategory Build()
    {
        // If ID is provided, use it, otherwise generate new
        var id = _id ?? WorkoutCategoryId.New();
        
        return WorkoutCategory.Handler.Create(
            id: id,
            value: _value,
            description: _description,
            icon: _icon ?? string.Empty,
            color: _color ?? string.Empty,
            primaryMuscleGroups: _primaryMuscleGroups,
            displayOrder: _displayOrder,
            isActive: _isActive
        );
    }

    /// <summary>
    /// Builds and returns just the WorkoutCategoryId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to WorkoutCategory for convenience
    /// </summary>
    public static implicit operator WorkoutCategory(WorkoutCategoryTestBuilder builder)
    {
        return builder.Build();
    }
}