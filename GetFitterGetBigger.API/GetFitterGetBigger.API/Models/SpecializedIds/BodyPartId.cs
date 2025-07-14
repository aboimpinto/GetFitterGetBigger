using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct BodyPartId
{
    private readonly Guid _value;
    
    private BodyPartId(Guid value)
    {
        this._value = value;
    }
    
    public static BodyPartId New() => new(Guid.NewGuid());
    
    public static BodyPartId From(Guid guid) => new(guid);
    
    public static BodyPartId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out BodyPartId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("bodypart-"))
            return false;
            
        string guidPart = input["bodypart-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static BodyPartId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => $"bodypart-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(BodyPartId id) => id._value;
}
