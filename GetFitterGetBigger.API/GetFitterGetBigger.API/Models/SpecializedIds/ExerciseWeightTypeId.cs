using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ExerciseWeightTypeId
{
    private readonly Guid _value;
    
    private ExerciseWeightTypeId(Guid value)
    {
        this._value = value;
    }
    
    public static ExerciseWeightTypeId New() => new(Guid.NewGuid());
    
    public static ExerciseWeightTypeId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out ExerciseWeightTypeId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("exerciseweighttype-"))
            return false;
            
        string guidPart = input["exerciseweighttype-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"exerciseweighttype-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ExerciseWeightTypeId id) => id._value;
}