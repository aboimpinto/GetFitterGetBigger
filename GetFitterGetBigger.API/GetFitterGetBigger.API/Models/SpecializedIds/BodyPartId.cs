using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct BodyPartId : ISpecializedId<BodyPartId>
{
    private readonly Guid _value;
    private const string Prefix = "bodypart";
    
    private BodyPartId(Guid value)
    {
        this._value = value;
    }
    
    public static BodyPartId New() => new(Guid.NewGuid());
    
    public static BodyPartId From(Guid guid) => new(guid);
    
    public static BodyPartId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    // ISpecializedId<BodyPartId> implementation
    public Guid ToGuid() => _value;
    
    private static bool TryParse(string? input, out BodyPartId result)
    {
        result = default;
        var prefix = $"{Prefix}-";
        if (string.IsNullOrEmpty(input) || !input.StartsWith(prefix))
            return false;
            
        string guidPart = input[prefix.Length..];
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
    
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{this._value}";
    
    public string GetPrefix() => Prefix;
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(BodyPartId id) => id._value;
}
