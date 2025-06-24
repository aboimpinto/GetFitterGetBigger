using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MovementPatternId(Guid Value)
{
    public static MovementPatternId New() => new(Guid.NewGuid());
    public static MovementPatternId From(Guid guid) => new(guid);
    public static implicit operator Guid(MovementPatternId id) => id.Value;
    public override string ToString() => Value.ToString();
}
