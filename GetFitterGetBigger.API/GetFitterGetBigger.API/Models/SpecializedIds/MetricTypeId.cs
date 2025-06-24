using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MetricTypeId(Guid Value)
{
    public static MetricTypeId New() => new(Guid.NewGuid());
    public static MetricTypeId From(Guid guid) => new(guid);
    public static implicit operator Guid(MetricTypeId id) => id.Value;
    public override string ToString() => Value.ToString();
}
