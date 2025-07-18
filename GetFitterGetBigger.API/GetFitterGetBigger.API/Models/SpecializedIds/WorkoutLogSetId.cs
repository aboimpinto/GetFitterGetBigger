using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutLogSetId(Guid Value)
{
    public static WorkoutLogSetId New() => new(Guid.NewGuid());
    public static WorkoutLogSetId From(Guid guid) => new(guid);
    public static implicit operator Guid(WorkoutLogSetId id) => id.Value;
    public override string ToString() => Value.ToString();
}
