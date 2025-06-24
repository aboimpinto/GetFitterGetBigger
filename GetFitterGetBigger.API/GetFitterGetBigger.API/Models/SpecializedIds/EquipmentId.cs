using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct EquipmentId(Guid Value)
{
    public static EquipmentId New() => new(Guid.NewGuid());
    public static EquipmentId From(Guid guid) => new(guid);
    public static implicit operator Guid(EquipmentId id) => id.Value;
    public override string ToString() => Value.ToString();
}
