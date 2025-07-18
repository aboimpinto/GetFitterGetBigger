using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutLogId(Guid Value)
{
    public static WorkoutLogId New() => new(Guid.NewGuid());
    public static WorkoutLogId From(Guid guid) => new(guid);
    public static implicit operator Guid(WorkoutLogId id) => id.Value;
    public override string ToString() => Value.ToString();
}
