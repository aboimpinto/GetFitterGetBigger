using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid ExerciseType entities with proper validation
/// </summary>
public class ExerciseTypeTestBuilder
{
    private string _value = "Workout";
    private string _description = "Main workout exercise";
    private int _displayOrder = 2;
    private ExerciseTypeId? _id = null;

    private ExerciseTypeTestBuilder() { }

    /// <summary>
    /// Creates a builder for REST exercise type
    /// </summary>
    public static ExerciseTypeTestBuilder Rest() => new ExerciseTypeTestBuilder()
        .WithId(ExerciseTypeId.From(Guid.Parse("d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a")))
        .WithValue("Rest")
        .WithDescription("Rest period between exercises")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for WORKOUT exercise type
    /// </summary>
    public static ExerciseTypeTestBuilder Workout() => new ExerciseTypeTestBuilder()
        .WithId(ExerciseTypeId.From(Guid.Parse("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e")))
        .WithValue("Workout")
        .WithDescription("Main workout exercise")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for WARMUP exercise type
    /// </summary>
    public static ExerciseTypeTestBuilder Warmup() => new ExerciseTypeTestBuilder()
        .WithId(ExerciseTypeId.From(Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d")))
        .WithValue("Warmup")
        .WithDescription("Warmup exercise to prepare the body")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for COOLDOWN exercise type
    /// </summary>
    public static ExerciseTypeTestBuilder Cooldown() => new ExerciseTypeTestBuilder()
        .WithId(ExerciseTypeId.From(Guid.Parse("c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f")))
        .WithValue("Cooldown")
        .WithDescription("Cooldown exercise for recovery")
        .WithDisplayOrder(4);

    /// <summary>
    /// Creates a builder for a custom exercise type
    /// </summary>
    public static ExerciseTypeTestBuilder Custom(string value) => new ExerciseTypeTestBuilder()
        .WithValue(value)
        .WithDescription($"Custom exercise type: {value}");

    public ExerciseTypeTestBuilder WithId(ExerciseTypeId id)
    {
        _id = id;
        return this;
    }

    public ExerciseTypeTestBuilder WithId(string idString)
    {
        if (!idString.StartsWith("exercisetype-"))
        {
            throw new ArgumentException($"Invalid ExerciseTypeId format: '{idString}'. Expected format: 'exercisetype-{{guid}}'");
        }
        
        var guidPart = idString["exercisetype-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in ExerciseTypeId: '{guidPart}'");
        }
        
        _id = ExerciseTypeId.From(guid);
        return this;
    }

    public ExerciseTypeTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Exercise type value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public ExerciseTypeTestBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Exercise type description cannot be empty", nameof(description));
        }
        _description = description;
        return this;
    }

    public ExerciseTypeTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }


    /// <summary>
    /// Builds an ExerciseType entity with validation
    /// </summary>
    public ExerciseType Build()
    {
        // If ID is provided, use Create method, otherwise generate new
        var result = _id.HasValue
            ? ExerciseType.Handler.Create(
                id: _id.Value,
                value: _value,
                description: _description,
                displayOrder: _displayOrder,
                isActive: true
            )
            : ExerciseType.Handler.Create(
                id: ExerciseTypeId.New(),
                value: _value,
                description: _description,
                displayOrder: _displayOrder,
                isActive: true
            );

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to create ExerciseType: {string.Join(", ", result.Errors)}");
        }

        return result.Value;
    }

    /// <summary>
    /// Builds and returns just the ExerciseTypeId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().ExerciseTypeId.ToString();
    }

    /// <summary>
    /// Gets whether this is a REST type exercise
    /// </summary>
    public bool IsRestType => _value?.ToUpperInvariant() == "REST";

    /// <summary>
    /// Implicit conversion to ExerciseType for convenience
    /// </summary>
    public static implicit operator ExerciseType(ExerciseTypeTestBuilder builder)
    {
        return builder.Build();
    }
}