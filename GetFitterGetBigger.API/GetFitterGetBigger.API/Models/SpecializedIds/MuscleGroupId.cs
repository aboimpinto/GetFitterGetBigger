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
    
    public static MuscleGroupId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
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
    
    public static MuscleGroupId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"musclegroup-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(MuscleGroupId id) => id._value;
}
