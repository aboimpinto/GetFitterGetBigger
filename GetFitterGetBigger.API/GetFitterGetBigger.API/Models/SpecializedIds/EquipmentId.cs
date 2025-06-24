using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct EquipmentId
{
    private readonly Guid _value;
    
    private EquipmentId(Guid value)
    {
        this._value = value;
    }
    
    public static EquipmentId New() => new(Guid.NewGuid());
    
    public static EquipmentId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out EquipmentId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("equipment-"))
            return false;
            
        string guidPart = input["equipment-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"equipment-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(EquipmentId id) => id._value;
}
