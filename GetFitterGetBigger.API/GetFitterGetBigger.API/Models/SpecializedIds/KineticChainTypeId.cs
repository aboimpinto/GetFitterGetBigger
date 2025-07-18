using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct KineticChainTypeId : ISpecializedId<KineticChainTypeId>
{
    private readonly Guid _value;
    private const string Prefix = "kineticchaintype";
    
    private KineticChainTypeId(Guid value)
    {
        this._value = value;
    }
    
    public static KineticChainTypeId New() => new(Guid.NewGuid());
    
    public static KineticChainTypeId From(Guid guid) => new(guid);
    
    public static KineticChainTypeId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    // ISpecializedId<KineticChainTypeId> implementation
    public Guid ToGuid() => _value;
    
    private static bool TryParse(string? input, out KineticChainTypeId result)
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
    
    public static KineticChainTypeId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{this._value}";
    
    public string GetPrefix() => Prefix;
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(KineticChainTypeId id) => id._value;
}
