using System;
using GetFitterGetBigger.API.Models.Interfaces;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ReferenceDataId : ISpecializedId<ReferenceDataId>
{
    private readonly Guid _value;
    private const string Prefix = "refdata";
    
    private ReferenceDataId(Guid value)
    {
        this._value = value;
    }
    
    public static ReferenceDataId New() => new(Guid.NewGuid());
    
    public static ReferenceDataId From(Guid guid) => new(guid);
        
        public static ReferenceDataId Empty => new(Guid.Empty);
        
        public bool IsEmpty => _value == Guid.Empty;
        
        // ISpecializedId<ReferenceDataId> implementation
        public Guid ToGuid() => _value;
        
        public static ReferenceDataId ParseOrEmpty(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return Empty;
                
            if (TryParse(input, out var result))
                return result;
                
            return Empty;
        }
    
    private static bool TryParse(string? input, out ReferenceDataId result)
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
    
    public override string ToString() => IsEmpty ? string.Empty : $"{Prefix}-{this._value}";
    
    public string GetPrefix() => Prefix;
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ReferenceDataId id) => id._value;
}
