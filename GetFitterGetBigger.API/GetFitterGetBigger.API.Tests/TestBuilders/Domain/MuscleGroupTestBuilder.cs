using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid MuscleGroup entities with proper validation
/// </summary>
public class MuscleGroupTestBuilder
{
    private string _name = "Chest";
    private BodyPartId _bodyPartId = BodyPartId.From(Guid.Parse("aabbccdd-eeff-0011-2233-445566778899")); // Default to upper body
    private bool _isActive = true;
    private MuscleGroupId? _id = null;

    private MuscleGroupTestBuilder() { }

    /// <summary>
    /// Creates a builder with default values for a chest muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Default() => new MuscleGroupTestBuilder();

    /// <summary>
    /// Creates a builder for chest muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Chest() => new MuscleGroupTestBuilder()
        .WithName("Chest")
        .WithBodyPartId(TestConstants.BodyPartIds.Chest);

    /// <summary>
    /// Creates a builder for back muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Back() => new MuscleGroupTestBuilder()
        .WithName("Back")
        .WithBodyPartId(TestConstants.BodyPartIds.Back);

    /// <summary>
    /// Creates a builder for shoulders muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Shoulders() => new MuscleGroupTestBuilder()
        .WithName("Shoulders")
        .WithBodyPartId(TestConstants.BodyPartIds.Shoulders);

    /// <summary>
    /// Creates a builder for quadriceps muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Quadriceps() => new MuscleGroupTestBuilder()
        .WithName("Quadriceps")
        .WithBodyPartId(TestConstants.BodyPartIds.Legs);

    /// <summary>
    /// Creates a builder for hamstrings muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Hamstrings() => new MuscleGroupTestBuilder()
        .WithName("Hamstrings")
        .WithBodyPartId(TestConstants.BodyPartIds.Legs);

    /// <summary>
    /// Creates a builder for biceps muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Biceps() => new MuscleGroupTestBuilder()
        .WithName("Biceps")
        .WithBodyPartId(TestConstants.BodyPartIds.Shoulders); // Arms are grouped with shoulders

    /// <summary>
    /// Creates a builder for triceps muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Triceps() => new MuscleGroupTestBuilder()
        .WithName("Triceps")
        .WithBodyPartId(TestConstants.BodyPartIds.Shoulders); // Arms are grouped with shoulders

    /// <summary>
    /// Creates a builder for glutes muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Glutes() => new MuscleGroupTestBuilder()
        .WithName("Glutes")
        .WithBodyPartId(TestConstants.BodyPartIds.Legs);

    /// <summary>
    /// Creates a builder for calves muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Calves() => new MuscleGroupTestBuilder()
        .WithName("Calves")
        .WithBodyPartId(TestConstants.BodyPartIds.Legs);

    /// <summary>
    /// Creates a builder for abs muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Abs() => new MuscleGroupTestBuilder()
        .WithName("Abs")
        .WithBodyPartId(TestConstants.BodyPartIds.Chest); // Core is grouped with chest

    /// <summary>
    /// Creates a builder for obliques muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Obliques() => new MuscleGroupTestBuilder()
        .WithName("Obliques")
        .WithBodyPartId(TestConstants.BodyPartIds.Chest); // Core is grouped with chest

    /// <summary>
    /// Creates a builder for forearms muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Forearms() => new MuscleGroupTestBuilder()
        .WithName("Forearms")
        .WithBodyPartId(TestConstants.BodyPartIds.Shoulders); // Arms are grouped with shoulders

    /// <summary>
    /// Creates a builder for custom muscle group
    /// </summary>
    public static MuscleGroupTestBuilder Custom() => new MuscleGroupTestBuilder();

    public MuscleGroupTestBuilder WithId(MuscleGroupId id)
    {
        _id = id;
        return this;
    }

    public MuscleGroupTestBuilder WithId(string idString)
    {
        if (!MuscleGroupId.TryParse(idString, out var id))
        {
            throw new ArgumentException($"Invalid MuscleGroupId format: '{idString}'. Expected format: 'musclegroup-{{guid}}'");
        }
        _id = id;
        return this;
    }

    public MuscleGroupTestBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Muscle group name cannot be empty", nameof(name));
        }
        _name = name;
        return this;
    }

    public MuscleGroupTestBuilder WithBodyPartId(string bodyPartId)
    {
        if (!bodyPartId.StartsWith("bodypart-"))
        {
            throw new ArgumentException($"Invalid BodyPartId format: '{bodyPartId}'. Expected format: 'bodypart-{{guid}}'");
        }
        
        var guidPart = bodyPartId["bodypart-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in BodyPartId: '{guidPart}'");
        }
        
        _bodyPartId = BodyPartId.From(guid);
        return this;
    }

    public MuscleGroupTestBuilder WithBodyPartId(BodyPartId bodyPartId)
    {
        _bodyPartId = bodyPartId;
        return this;
    }

    public MuscleGroupTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    public MuscleGroupTestBuilder IsInactive() => IsActive(false);

    /// <summary>
    /// Builds a MuscleGroup entity with validation
    /// </summary>
    public MuscleGroup Build()
    {
        // If ID is provided, use Create method, otherwise use CreateNew
        if (_id.HasValue)
        {
            return MuscleGroup.Handler.Create(
                id: _id.Value,
                name: _name,
                bodyPartId: _bodyPartId,
                isActive: _isActive
            );
        }
        else
        {
            var muscleGroup = MuscleGroup.Handler.CreateNew(
                name: _name,
                bodyPartId: _bodyPartId
            );

            // If inactive is requested, we need to create a new instance with the inactive state
            if (!_isActive)
            {
                return MuscleGroup.Handler.Create(
                    id: muscleGroup.Id,
                    name: muscleGroup.Name,
                    bodyPartId: muscleGroup.BodyPartId,
                    isActive: false,
                    createdAt: muscleGroup.CreatedAt
                );
            }

            return muscleGroup;
        }
    }

    /// <summary>
    /// Builds and returns just the MuscleGroupId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to MuscleGroup for convenience
    /// </summary>
    public static implicit operator MuscleGroup(MuscleGroupTestBuilder builder)
    {
        return builder.Build();
    }
}