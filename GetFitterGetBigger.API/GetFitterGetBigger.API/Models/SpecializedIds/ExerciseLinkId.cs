using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ExerciseLinkId
{
    private readonly Guid _value;
    
    private ExerciseLinkId(Guid value)
    {
        this._value = value;
    }
    
    public static ExerciseLinkId New() => new(Guid.NewGuid());
    
    public static ExerciseLinkId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out ExerciseLinkId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("exerciselink-"))
            return false;
            
        string guidPart = input["exerciselink-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"exerciselink-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ExerciseLinkId id) => id._value;
}