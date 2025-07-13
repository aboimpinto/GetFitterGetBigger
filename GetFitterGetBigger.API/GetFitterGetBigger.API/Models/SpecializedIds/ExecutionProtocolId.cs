using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ExecutionProtocolId
{
    private readonly Guid _value;
    
    private ExecutionProtocolId(Guid value)
    {
        this._value = value;
    }
    
    public static ExecutionProtocolId New() => new(Guid.NewGuid());
    
    public static ExecutionProtocolId From(Guid guid) => new(guid);
    
    public static ExecutionProtocolId From(string input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        // If it's a properly formatted ID string (executionprotocol-{guid})
        if (input.StartsWith("executionprotocol-"))
        {
            var guidPart = input["executionprotocol-".Length..];
            if (Guid.TryParse(guidPart, out var guid))
                return new(guid);
        }
        // If it's just a GUID string
        else if (Guid.TryParse(input, out var guid))
        {
            return new(guid);
        }
        
        return Empty;
    }
    
    public static ExecutionProtocolId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out ExecutionProtocolId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("executionprotocol-"))
            return false;
            
        string guidPart = input["executionprotocol-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static ExecutionProtocolId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"executionprotocol-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ExecutionProtocolId id) => id._value;
}