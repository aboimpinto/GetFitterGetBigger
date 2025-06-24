using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ReferenceDataId
{
    private readonly Guid _value;
    
    private ReferenceDataId(Guid value)
    {
        this._value = value;
    }
    
    public static ReferenceDataId New() => new(Guid.NewGuid());
    
    public static ReferenceDataId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out ReferenceDataId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("refdata-"))
            return false;
            
        string guidPart = input["refdata-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"refdata-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ReferenceDataId id) => id._value;
}
