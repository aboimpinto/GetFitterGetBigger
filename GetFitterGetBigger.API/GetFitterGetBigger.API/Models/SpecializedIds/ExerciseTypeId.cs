using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct ExerciseTypeId
{
    private readonly Guid _value;
    
    private ExerciseTypeId(Guid value)
    {
        this._value = value;
    }
    
    public static ExerciseTypeId New() => new(Guid.NewGuid());
    
    public static ExerciseTypeId From(Guid guid) => new(guid);
    
    public static ExerciseTypeId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    private static bool TryParse(string? input, out ExerciseTypeId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("exercisetype-"))
            return false;
            
        string guidPart = input["exercisetype-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static ExerciseTypeId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        return TryParse(input, out var result) ? result : Empty;
    }
    
    public override string ToString() => $"exercisetype-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(ExerciseTypeId id) => id._value;
}