using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct KineticChainTypeId
{
    private readonly Guid _value;
    
    private KineticChainTypeId(Guid value)
    {
        this._value = value;
    }
    
    public static KineticChainTypeId New() => new(Guid.NewGuid());
    
    public static KineticChainTypeId From(Guid guid) => new(guid);
    
    public static KineticChainTypeId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    private static bool TryParse(string? input, out KineticChainTypeId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("kineticchaintype-"))
            return false;
            
        string guidPart = input["kineticchaintype-".Length..];
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
    
    public override string ToString() => $"kineticchaintype-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(KineticChainTypeId id) => id._value;
}
