using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ExerciseId(Guid Value)
{
    public static ExerciseId New() => new(Guid.NewGuid());
    public static ExerciseId From(Guid guid) => new(guid);
    public static implicit operator Guid(ExerciseId id) => id.Value;
    public override string ToString() => Value.ToString();
}
