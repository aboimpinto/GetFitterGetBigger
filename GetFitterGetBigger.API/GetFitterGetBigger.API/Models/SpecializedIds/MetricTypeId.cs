using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MetricTypeId
{
    private const string Prefix = "metrictype-";
    private readonly Guid _value;
    
    private MetricTypeId(Guid value)
    {
        this._value = value;
    }
    
    public static MetricTypeId New() => new(Guid.NewGuid());
    
    public static MetricTypeId From(Guid guid) => new(guid);
    
    public static MetricTypeId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    private static bool TryParse(string? input, out MetricTypeId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith(Prefix))
            return false;
            
        string guidPart = input[Prefix.Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static MetricTypeId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => $"{Prefix}{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MetricTypeId id) => id._value;
}
