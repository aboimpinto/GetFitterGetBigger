using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Tests.Services.Builders;

public class BodyPartBuilder
{
    private BodyPartId _bodyPartId = BodyPartId.Empty;
    private string _value = "Test value";
    private string _description = "Test description";
    private bool _active = true;
    private int _order = 1;

    public BodyPartBuilder WithBodyPartId(BodyPartId bodyPartId)
    {
        this._bodyPartId = bodyPartId;
        return this;
    }

    public BodyPartBuilder WithValue(string value)
    {
        this._value = value;
        return this;
    }

    public BodyPartBuilder WithDescription(string description)
    {
        this._description = description;
        return this;
    }

    public BodyPartBuilder WithOrder(int order)
    {
        this._order = order;
        return this;
    }

    public BodyPartBuilder WithInactiveFlag()
    {
        this._active = false;
        return this;
    }

    public BodyPart Build()
    {
        return BodyPart.Handler.Create(
            this._bodyPartId,
            this._value,
            this._description,
            this._order,
            this._active).Value;
    }
}
