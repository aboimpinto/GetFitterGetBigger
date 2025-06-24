using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MovementPatternId
{
    private readonly Guid _value;
    
    private MovementPatternId(Guid value)
    {
        this._value = value;
    }
    
    public static MovementPatternId New() => new(Guid.NewGuid());
    
    public static MovementPatternId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out MovementPatternId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("movementpattern-"))
            return false;
            
        string guidPart = input["movementpattern-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"movementpattern-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MovementPatternId id) => id._value;
}
