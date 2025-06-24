using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct MuscleGroupId
{
    private readonly Guid _value;
    
    private MuscleGroupId(Guid value)
    {
        this._value = value;
    }
    
    public static MuscleGroupId New() => new(Guid.NewGuid());
    
    public static MuscleGroupId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out MuscleGroupId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("musclegroup-"))
            return false;
            
        string guidPart = input["musclegroup-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"musclegroup-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MuscleGroupId id) => id._value;
}
