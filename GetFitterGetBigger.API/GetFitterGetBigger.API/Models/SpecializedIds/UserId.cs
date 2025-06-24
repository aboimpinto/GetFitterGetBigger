using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid guid) => new(guid);
    public static implicit operator Guid(UserId id) => id.Value;
    public override string ToString() => Value.ToString();
}
