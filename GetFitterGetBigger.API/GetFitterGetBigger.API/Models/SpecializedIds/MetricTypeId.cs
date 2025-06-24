using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MetricTypeId
{
    private readonly Guid _value;
    
    private MetricTypeId(Guid value)
    {
        this._value = value;
    }
    
    public static MetricTypeId New() => new(Guid.NewGuid());
    
    public static MetricTypeId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out MetricTypeId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("metrictype-"))
            return false;
            
        string guidPart = input["metrictype-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"metrictype-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MetricTypeId id) => id._value;
}
