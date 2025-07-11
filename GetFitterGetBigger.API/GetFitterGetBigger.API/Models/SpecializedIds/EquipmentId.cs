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
    
    public static EquipmentId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
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
    
    public static EquipmentId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"equipment-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(EquipmentId id) => id._value;
}
