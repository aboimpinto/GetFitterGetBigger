using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.TestBuilders.Domain;

/// <summary>
/// Test builder for creating valid DifficultyLevel entities with proper validation
/// </summary>
public class DifficultyLevelTestBuilder
{
    private string _value = "Intermediate";
    private string _description = "For those with moderate experience";
    private int _displayOrder = 2;
    private bool _isActive = true;
    private DifficultyLevelId? _id = null;

    private DifficultyLevelTestBuilder() { }

    /// <summary>
    /// Creates a builder for BEGINNER difficulty level
    /// </summary>
    public static DifficultyLevelTestBuilder Beginner() => new DifficultyLevelTestBuilder()
        .WithId(DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")))
        .WithValue("Beginner")
        .WithDescription("For those new to exercise")
        .WithDisplayOrder(1);

    /// <summary>
    /// Creates a builder for INTERMEDIATE difficulty level
    /// </summary>
    public static DifficultyLevelTestBuilder Intermediate() => new DifficultyLevelTestBuilder()
        .WithId(DifficultyLevelId.From(Guid.Parse("9b9bec2e-35e3-5a8a-b6b7-1e871f7eb35c")))
        .WithValue("Intermediate")
        .WithDescription("For those with moderate experience")
        .WithDisplayOrder(2);

    /// <summary>
    /// Creates a builder for ADVANCED difficulty level
    /// </summary>
    public static DifficultyLevelTestBuilder Advanced() => new DifficultyLevelTestBuilder()
        .WithId(DifficultyLevelId.From(Guid.Parse("acabfd3f-46f4-6b9b-c7c8-2f982a8fc46d")))
        .WithValue("Advanced")
        .WithDescription("For experienced individuals")
        .WithDisplayOrder(3);

    /// <summary>
    /// Creates a builder for EXPERT difficulty level
    /// </summary>
    public static DifficultyLevelTestBuilder Expert() => new DifficultyLevelTestBuilder()
        .WithValue("Expert")
        .WithDescription("For highly trained individuals")
        .WithDisplayOrder(4);

    public DifficultyLevelTestBuilder WithId(DifficultyLevelId id)
    {
        _id = id;
        return this;
    }

    public DifficultyLevelTestBuilder WithId(string idString)
    {
        if (!idString.StartsWith("difficultylevel-"))
        {
            throw new ArgumentException($"Invalid DifficultyLevelId format: '{idString}'. Expected format: 'difficultylevel-{{guid}}'");
        }
        
        var guidPart = idString["difficultylevel-".Length..];
        if (!Guid.TryParse(guidPart, out var guid))
        {
            throw new ArgumentException($"Invalid GUID in DifficultyLevelId: '{guidPart}'");
        }
        
        _id = DifficultyLevelId.From(guid);
        return this;
    }

    public DifficultyLevelTestBuilder WithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Difficulty level value cannot be empty", nameof(value));
        }
        _value = value;
        return this;
    }

    public DifficultyLevelTestBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Difficulty level description cannot be empty", nameof(description));
        }
        _description = description;
        return this;
    }

    public DifficultyLevelTestBuilder WithDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        }
        _displayOrder = displayOrder;
        return this;
    }

    public DifficultyLevelTestBuilder IsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    /// <summary>
    /// Builds a DifficultyLevel entity with validation
    /// </summary>
    public DifficultyLevel Build()
    {
        // If ID is provided, use it, otherwise generate new
        var id = _id ?? DifficultyLevelId.New();
        
        return DifficultyLevel.Handler.Create(
            id: id,
            value: _value,
            description: _description,
            displayOrder: _displayOrder,
            isActive: _isActive
        );
    }

    /// <summary>
    /// Builds and returns just the DifficultyLevelId string for use in requests
    /// </summary>
    public string BuildId()
    {
        return Build().Id.ToString();
    }

    /// <summary>
    /// Implicit conversion to DifficultyLevel for convenience
    /// </summary>
    public static implicit operator DifficultyLevel(DifficultyLevelTestBuilder builder)
    {
        return builder.Build();
    }
}