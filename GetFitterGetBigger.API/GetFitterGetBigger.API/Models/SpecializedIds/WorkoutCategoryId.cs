using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutCategoryId
{
    private readonly Guid _value;
    
    private WorkoutCategoryId(Guid value)
    {
        this._value = value;
    }
    
    public static WorkoutCategoryId New() => new(Guid.NewGuid());
    
    public static WorkoutCategoryId From(Guid guid) => new(guid);
    
    public static WorkoutCategoryId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out WorkoutCategoryId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("workoutcategory-"))
            return false;
            
        string guidPart = input["workoutcategory-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static WorkoutCategoryId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"workoutcategory-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(WorkoutCategoryId id) => id._value;
}