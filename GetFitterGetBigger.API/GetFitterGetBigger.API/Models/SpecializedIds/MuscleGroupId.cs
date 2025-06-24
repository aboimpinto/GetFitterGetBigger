using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MuscleGroupId(Guid Value)
{
    public static MuscleGroupId New() => new(Guid.NewGuid());
    public static MuscleGroupId From(Guid guid) => new(guid);
    public static implicit operator Guid(MuscleGroupId id) => id.Value;
    public override string ToString() => Value.ToString();
}
