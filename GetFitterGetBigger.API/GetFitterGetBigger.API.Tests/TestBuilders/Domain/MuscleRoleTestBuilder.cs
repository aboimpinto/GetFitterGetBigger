using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid MuscleRole entities with proper validation
/// </summary>
public class MuscleRoleTestBuilder
{
    private string _value = "Primary";
    private string? _description = "Primary muscle targeted by the exercise";
    private int _displayOrder = 1;
    private bool _isActive = true;
    private MuscleRoleId? _id = null;

    private MuscleRoleTestBuilder() { }

    /// <summary>
    /// Creates a builder with default values
    /// </summary>
    public static MuscleRoleTestBuilder Default() => new MuscleRoleTestBuilder();

    /// <summary>
    /// Creates a builder for Primary muscle role
    /// </summary>
    public static MuscleRoleTestBuilder Primary() => new MuscleRoleTestBuilder()
        .WithValue("Primary")
        .WithDescription("Primary muscle targeted by the exercise")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for Secondary muscle role
    /// </summary>
    public static MuscleRoleTestBuilder Secondary() => new MuscleRoleTestBuilder()
        .WithValue("Secondary")
        .WithDescription("Secondary muscle engaged during the exercise")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for Stabilizer muscle role
    /// </summary>
    public static MuscleRoleTestBuilder Stabilizer() => new MuscleRoleTestBuilder()
        .WithValue("Stabilizer")
        .WithDescription("Muscle that stabilizes during the exercise")
        .WithDisplayOrder(3);

    public MuscleRoleTestBuilder WithId(MuscleRoleId id)
    {
        _id = id;
        return this;
    }

    public MuscleRoleTestBuilder WithId(string idString)
    {
        if (!idString.StartsWith("musclerole-"))
        {
            throw new ArgumentException($"Invalid MuscleRoleId format: '{idString}'. Expected format: 'musclerole-{{guid}}'");
        }
        
        var guidPart = idString["musclerole-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in MuscleRoleId: '{guidPart}'");
        }
        
        _id = MuscleRoleId.From(guid);
        return this;
    }

    public MuscleRoleTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Muscle role value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public MuscleRoleTestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public MuscleRoleTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public MuscleRoleTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    public MuscleRoleTestBuilder IsInactive() => IsActive(false);

    /// <summary>
    /// Builds a MuscleRole entity with validation
    /// </summary>
    public MuscleRole Build()
    {
        // If ID is provided, use Create method, otherwise use CreateNew
        if (_id.HasValue)
        {
            return MuscleRole.Handler.Create(
                id: _id.Value,
                value: _value,
                description: _description,
                displayOrder: _displayOrder,
                isActive: _isActive
            );
        }
        else
        {
            var muscleRole = MuscleRole.Handler.CreateNew(
                value: _value,
                description: _description,
                displayOrder: _displayOrder,
                isActive: _isActive
            );

            return muscleRole;
        }
    }

    /// <summary>
    /// Builds and returns just the MuscleRoleId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to MuscleRole for convenience
    /// </summary>
    public static implicit operator MuscleRole(MuscleRoleTestBuilder builder)
    {
        return builder.Build();
    }
}