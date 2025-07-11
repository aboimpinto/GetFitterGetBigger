using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid ExerciseWeightType entities with proper validation
/// </summary>
public class ExerciseWeightTypeTestBuilder
{
    private string _code = "WEIGHT_REQUIRED";
    private string _value = "Weight Required";
    private string _description = "Exercise requires external weight";
    private int _displayOrder = 3;
    private bool _isActive = true;
    private ExerciseWeightTypeId? _id = null;

    private ExerciseWeightTypeTestBuilder() { }

    /// <summary>
    /// Creates a builder for BODYWEIGHT_ONLY weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder BodyweightOnly() => new ExerciseWeightTypeTestBuilder()
        .WithId(ExerciseWeightTypeId.From(Guid.Parse("a1b3c5d7-5b7c-4d8e-9f0a-1b2c3d4e5f6a")))
        .WithCode("BODYWEIGHT_ONLY")
        .WithValue("Bodyweight Only")
        .WithDescription("Exercise uses only body weight, no external weight allowed")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for BODYWEIGHT_OPTIONAL weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder BodyweightOptional() => new ExerciseWeightTypeTestBuilder()
        .WithId(ExerciseWeightTypeId.From(Guid.Parse("1b3d5f7a-5b7c-4d8e-9f0a-1b2c3d4e5f6a")))
        .WithCode("BODYWEIGHT_OPTIONAL")
        .WithValue("Bodyweight Optional")
        .WithDescription("Exercise can be performed with or without external weight")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for WEIGHT_REQUIRED weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder WeightRequired() => new ExerciseWeightTypeTestBuilder()
        .WithId(ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")))
        .WithCode("WEIGHT_REQUIRED")
        .WithValue("Weight Required")
        .WithDescription("Exercise requires external weight")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for MACHINE_WEIGHT weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder MachineWeight() => new ExerciseWeightTypeTestBuilder()
        .WithId(ExerciseWeightTypeId.From(Guid.Parse("3d5f7b9d-7b8c-6d9e-0f1a-2b3c4d5e6f7a")))
        .WithCode("MACHINE_WEIGHT")
        .WithValue("Machine Weight")
        .WithDescription("Exercise uses machine with weight stack or plates")
        .WithDisplayOrder(4);

    /// <summary>
    /// Creates a builder for NO_WEIGHT weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder NoWeight() => new ExerciseWeightTypeTestBuilder()
        .WithId(ExerciseWeightTypeId.From(Guid.Parse("5f7b9d1f-7b8c-6d9e-0f1a-2b3c4d5e6f7a")))
        .WithCode("NO_WEIGHT")
        .WithValue("No Weight")
        .WithDescription("Exercise does not involve weight (e.g., stretching, cardio)")
        .WithDisplayOrder(5);

    /// <summary>
    /// Creates a builder for BARBELL weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder Barbell() => WeightRequired()
        .WithCode("BARBELL")
        .WithValue("Barbell")
        .WithDescription("Exercise performed with a barbell");

    /// <summary>
    /// Creates a builder for DUMBBELL weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder Dumbbell() => WeightRequired()
        .WithCode("DUMBBELL")
        .WithValue("Dumbbell")
        .WithDescription("Exercise performed with dumbbells");

    /// <summary>
    /// Creates a builder for KETTLEBELL weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder Kettlebell() => WeightRequired()
        .WithCode("KETTLEBELL")
        .WithValue("Kettlebell")
        .WithDescription("Exercise performed with kettlebells");

    /// <summary>
    /// Creates a builder for BODYWEIGHT weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder Bodyweight() => BodyweightOnly();

    /// <summary>
    /// Creates a builder for CABLE weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder Cable() => MachineWeight()
        .WithCode("CABLE")
        .WithValue("Cable")
        .WithDescription("Exercise performed on cable machine");

    /// <summary>
    /// Creates a builder for MACHINE weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder Machine() => MachineWeight();

    /// <summary>
    /// Creates a builder for RESISTANCE_BAND weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder ResistanceBand() => WeightRequired()
        .WithCode("RESISTANCE_BAND")
        .WithValue("Resistance Band")
        .WithDescription("Exercise performed with resistance bands");

    /// <summary>
    /// Creates a builder for WEIGHT_PLATE weight type
    /// </summary>
    public static ExerciseWeightTypeTestBuilder WeightPlate() => WeightRequired()
        .WithCode("WEIGHT_PLATE")
        .WithValue("Weight Plate")
        .WithDescription("Exercise performed with weight plates");

    public ExerciseWeightTypeTestBuilder WithId(ExerciseWeightTypeId id)
    {
        _id = id;
        return this;
    }

    public ExerciseWeightTypeTestBuilder WithCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Exercise weight type code cannot be empty", nameof(code));
        }
        _code = code;
        return this;
    }

    public ExerciseWeightTypeTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Exercise weight type value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public ExerciseWeightTypeTestBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Exercise weight type description cannot be empty", nameof(description));
        }
        _description = description;
        return this;
    }

    public ExerciseWeightTypeTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public ExerciseWeightTypeTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Builds an ExerciseWeightType entity with validation
    /// </summary>
    public ExerciseWeightType Build()
    {
        // If ID is provided, use it, otherwise generate new
        var id = _id ?? ExerciseWeightTypeId.New();
        
        return ExerciseWeightType.Handler.Create(
            id: id,
            code: _code,
            value: _value,
            description: _description,
            displayOrder: _displayOrder,
            isActive: _isActive
        );
    }

    /// <summary>
    /// Builds and returns just the ExerciseWeightTypeId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to ExerciseWeightType for convenience
    /// </summary>
    public static implicit operator ExerciseWeightType(ExerciseWeightTypeTestBuilder builder)
    {
        return builder.Build();
    }
}