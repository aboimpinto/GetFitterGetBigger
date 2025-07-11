using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid KineticChainType entities with proper validation
/// </summary>
public class KineticChainTypeTestBuilder
{
    private string _value = "COMPOUND";
    private string _description = "Multi-joint movement engaging multiple muscle groups";
    private int _displayOrder = 1;
    private bool _isActive = true;
    private KineticChainTypeId? _id = null;

    private KineticChainTypeTestBuilder() { }

    /// <summary>
    /// Creates a builder for COMPOUND kinetic chain type
    /// </summary>
    public static KineticChainTypeTestBuilder Compound() => new KineticChainTypeTestBuilder()
        .WithId(KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")))
        .WithValue("COMPOUND")
        .WithDescription("Multi-joint movement engaging multiple muscle groups")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for ISOLATION kinetic chain type
    /// </summary>
    public static KineticChainTypeTestBuilder Isolation() => new KineticChainTypeTestBuilder()
        .WithId(KineticChainTypeId.From(Guid.Parse("a7c8b9da-0e1f-2a3b-4c5d-6e7f8a9b0c1d")))
        .WithValue("ISOLATION")
        .WithDescription("Single-joint movement targeting specific muscle")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for FUNCTIONAL kinetic chain type
    /// </summary>
    public static KineticChainTypeTestBuilder Functional() => new KineticChainTypeTestBuilder()
        .WithId(KineticChainTypeId.From(Guid.Parse("b8d9c0ea-1f2a-3b4c-5d6e-7f8a9b0c1d2e")))
        .WithValue("FUNCTIONAL")
        .WithDescription("Movement patterns that mimic daily activities")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for POWER kinetic chain type
    /// </summary>
    public static KineticChainTypeTestBuilder Power() => new KineticChainTypeTestBuilder()
        .WithId(KineticChainTypeId.From(Guid.Parse("c9e0d1fb-2a3b-4c5d-6e7f-8a9b0c1d2e3f")))
        .WithValue("POWER")
        .WithDescription("Explosive movements focusing on speed and force")
        .WithDisplayOrder(4);

    public KineticChainTypeTestBuilder WithId(KineticChainTypeId id)
    {
        _id = id;
        return this;
    }

    public KineticChainTypeTestBuilder WithId(string idString)
    {
        if (!idString.StartsWith("kineticchaintype-"))
        {
            throw new ArgumentException($"Invalid KineticChainTypeId format: '{idString}'. Expected format: 'kineticchaintype-{{guid}}'");
        }
        
        var guidPart = idString["kineticchaintype-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in KineticChainTypeId: '{guidPart}'");
        }
        
        _id = KineticChainTypeId.From(guid);
        return this;
    }

    public KineticChainTypeTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Kinetic chain type value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public KineticChainTypeTestBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Kinetic chain type description cannot be empty", nameof(description));
        }
        _description = description;
        return this;
    }

    public KineticChainTypeTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public KineticChainTypeTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Builds a KineticChainType entity with validation
    /// </summary>
    public KineticChainType Build()
    {
        // If ID is provided, use it, otherwise generate new
        var id = _id ?? KineticChainTypeId.New();
        
        return KineticChainType.Handler.Create(
            id: id,
            value: _value,
            description: _description,
            displayOrder: _displayOrder,
            isActive: _isActive
        );
    }

    /// <summary>
    /// Builds and returns just the KineticChainTypeId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to KineticChainType for convenience
    /// </summary>
    public static implicit operator KineticChainType(KineticChainTypeTestBuilder builder)
    {
        return builder.Build();
    }
}