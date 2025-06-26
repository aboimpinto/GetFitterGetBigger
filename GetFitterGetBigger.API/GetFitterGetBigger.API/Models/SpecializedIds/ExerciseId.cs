using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ExerciseId
{
    private readonly Guid _value;
    
    private ExerciseId(Guid value)
    {
        this._value = value;
    }
    
    public static ExerciseId New() => new(Guid.NewGuid());
    
    public static ExerciseId From(Guid guid) => new(guid);
    
    public static bool TryParse(string? input, out ExerciseId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("exercise-"))
            return false;
            
        string guidPart = input["exercise-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public override string ToString() => $"exercise-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ExerciseId id) => id._value;
}