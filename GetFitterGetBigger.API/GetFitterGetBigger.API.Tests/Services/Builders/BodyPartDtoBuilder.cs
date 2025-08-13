using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Services.Builders;

/// <summary>
/// Builder for creating BodyPartDto instances for testing
/// Provides good defaults and allows selective overrides
/// </summary>
public class BodyPartDtoBuilder
{
    private string _id = BodyPartId.New().ToString();
    private string _value = "Test Body Part";
    private string _description = "Test Description";

    public BodyPartDtoBuilder WithId(BodyPartId id)
    {
        _id = id.ToString();
        return this;
    }

    public BodyPartDtoBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public BodyPartDtoBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public BodyPartDtoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }


    public static BodyPartDtoBuilder ForChest()
    {
        return new BodyPartDtoBuilder()
            .WithValue("Chest")
            .WithDescription("Chest muscles");
    }

    public static BodyPartDtoBuilder ForBack()
    {
        return new BodyPartDtoBuilder()
            .WithValue("Back")
            .WithDescription("Back muscles");
    }

    public static BodyPartDtoBuilder ForLegs()
    {
        return new BodyPartDtoBuilder()
            .WithValue("Legs")
            .WithDescription("Leg muscles");
    }

    public BodyPartDto Build()
    {
        return new BodyPartDto
        {
            Id = _id,
            Value = _value,
            Description = _description
        };
    }
}