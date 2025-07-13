using System;

namespace GetFitterGetBigger.API.Models.SpecializedIds;

public readonly record struct WorkoutMusclesId
{
    private readonly Guid _value;
    
    private WorkoutMusclesId(Guid value)
    {
        this._value = value;
    }
    
    public static WorkoutMusclesId New() => new(Guid.NewGuid());
    
    public static WorkoutMusclesId From(Guid guid) => new(guid);
    
    public static WorkoutMusclesId Empty => new(Guid.Empty);
    
    public bool IsEmpty => _value == Guid.Empty;
    
    public static bool TryParse(string? input, out WorkoutMusclesId result)
    {
        result = default;
        if (string.IsNullOrEmpty(input) || !input.StartsWith("workoutmuscles-"))
            return false;
            
        string guidPart = input["workoutmuscles-".Length..];
        if (Guid.TryParse(guidPart, out Guid guid))
        {
            result = From(guid);
            return true;
        }
        
        return false;
    }
    
    public static WorkoutMusclesId ParseOrEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return Empty;
            
        if (TryParse(input, out var result))
            return result;
            
        return Empty;
    }
    
    public override string ToString() => IsEmpty ? string.Empty : $"workoutmuscles-{this._value}";
    
    // Conversion to/from Guid for EF Core
    public static implicit operator Guid(WorkoutMusclesId id) => id._value;
}